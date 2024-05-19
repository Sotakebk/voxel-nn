"""Code for building the diffusion models."""

from typing import List, Tuple, Any
import tensorflow as tf
import keras
from keras import layers
from voxelnn.diffusion.custom_layers import TimeEmbedding, AttentionBlock
from voxelnn.diffusion.utils import kernel_init

def ResidualBlock(is3d: bool, filters: int, groups=8):
    def apply(inputs):
        x, t = inputs

        if is3d:
            input_filters = x.shape[4]
        else:
            input_filters = x.shape[3]

        if input_filters == filters:
            residual = x
        else:
            residual = _conv(is3d=is3d, filters=filters, kernel_size=1, kernel_initializer=kernel_init(1.0))(x)

        time_embedding = keras.activations.swish(t)
        time_embedding = layers.Dense(filters, kernel_initializer=kernel_init(1.0))(time_embedding)

        if is3d:
            time_embedding = time_embedding[:, None, None, None, :]
        else:
            time_embedding = time_embedding[:, None, None, :]

        x = layers.GroupNormalization(groups=groups)(x)
        x = keras.activations.swish(x)
        x = _conv(is3d=is3d, filters=filters, kernel_size=3, padding="same", kernel_initializer=kernel_init(1.0))(x)

        x = layers.Add()([x, time_embedding])
        x = layers.GroupNormalization(groups=groups)(x)
        x = keras.activations.swish(x)

        x = _conv(is3d=is3d, filters=filters, kernel_size=3, padding="same", kernel_initializer=kernel_init(0.0))(x)
        x = layers.Add()([x, residual])

        return x

    return apply


def DownSample(is3d: bool, filters: int):
    def apply(x):
        # should this really use convolution?
        x = _conv(is3d, filters = filters, kernel_size=3, stride=2, padding="same",
                 kernel_initializer=kernel_init(1.0))(x)
        return x

    return apply


def UpSample(is3d: bool, filters: int):
    def apply(x):
        if is3d:
            x = layers.UpSampling3D(size=(2,2,2))(x)
        else:
            x = layers.UpSampling2D(size=(2,2))(x)
        x = _conv(is3d,
            filters = filters,
            kernel_size=3,
            stride=1,
            padding="same",
            kernel_initializer=kernel_init(1.0))(x)
        return x

    return apply


def TimeMLP(units, activation_fn=keras.activations.swish):
    def apply(inputs):
        time_embedding = layers.Dense(units, activation=activation_fn, kernel_initializer=kernel_init(1.0))(inputs)
        time_embedding = layers.Dense(units, kernel_initializer=kernel_init(1.0))(time_embedding)
        return time_embedding

    return apply


def _conv(is3d: bool, filters: int, kernel_size: int = 3, stride: int = 1, padding: str = "same", kernel_initializer=None):
    """Returns a Conv3D or Conv2D layer, passing arguments accordingly."""
    def apply(x):
        _kernel_initializer  = kernel_initializer
        if _kernel_initializer is None:
            _kernel_initializer = kernel_init(1.0)

        if is3d:
            return layers.Conv3D(filters=filters,
                                strides=(stride, stride, stride),
                                kernel_size=kernel_size,
                                padding=padding,
                                kernel_initializer=_kernel_initializer)(x)
        else:
            return layers.Conv2D(filters=filters,
                                strides=(stride, stride),
                                kernel_size=kernel_size,
                                padding=padding,
                                kernel_initializer=_kernel_initializer)(x)
    
    return apply


def build_model(
    input_shape: Tuple,
    latent_dims: int,
    downsample_layer: List[bool],
    layer_filters: List[int],
    has_attention: List[bool],
    num_res_blocks: int = 2,
    norm_groups: int = 8,
    first_conv_channels: int = 64):

    data_input = layers.Input(shape=(*input_shape, latent_dims), name='input')
    time_input = layers.Input(shape=(), dtype=tf.int64, name='time_input')

    is3d = len(input_shape) == 3
    print(f"Constructing a {('3d' if is3d else '2d')} diffusion model.")

    x = _conv(
        is3d=is3d,
        filters=first_conv_channels,
        kernel_size=3,
        padding='same',
        kernel_initializer=kernel_init(1.0),
    )(data_input)

    time_embedding = TimeEmbedding(dim=latent_dims * 4)(time_input)
    time_mlp = TimeMLP(units=first_conv_channels * 4, activation_fn='silu')(time_embedding)

    skips = [x]

    # down blocks
    for i, filters in enumerate(layer_filters):
        print(f'Constructing down layer {i}')
        for r in range(num_res_blocks):
            print(f'Constructing resblock {r}')
            x = ResidualBlock(is3d, filters, groups=norm_groups)([x, time_mlp])
            if has_attention[i]:
                print(f'Constructing attention for resblock {r}')
                x = AttentionBlock(is3d, units=filters, groups=norm_groups)(x)

        skips.append(x) # per layer, keep the result once
        print(f'appending {x.shape}')

        if downsample_layer[i]:
            x = DownSample(is3d, layer_filters[i])(x)
            print(f'downsampling to {x.shape}')

    # middle block
    print('Constructing middle layer')
    x = ResidualBlock(is3d, filters=layer_filters[-1], groups=norm_groups)([x, time_mlp])
    x = AttentionBlock(is3d, units=layer_filters[-1], groups=norm_groups)(x)
    x = ResidualBlock(is3d, filters=layer_filters[-1], groups=norm_groups)([x, time_mlp])

    # up blocks

    if downsample_layer[-1]:
        x = UpSample(is3d, layer_filters[-1])(x)
        print('Upsampling before up-block')

    for i, filters in reversed(list(enumerate(layer_filters))):
        print(f'Constructing up layer {i}')
        y = skips.pop() # recover same level layer result, should be of same size!
        x = layers.Concatenate(axis=-1)([x, y])
        for r in range(num_res_blocks):
            print(f'Constructing resblock {r}')
            x = ResidualBlock(is3d, filters=layer_filters[i], groups=norm_groups)([x, time_mlp])
            if has_attention[i]:
                print(f'Constructing attention for resblock {r}')
                x = AttentionBlock(is3d, units=layer_filters[i], groups=norm_groups)(x)

        if i > 0 and downsample_layer[i-1]:
            x = UpSample(is3d, layer_filters[i])(x)
            print(f'up-block upsampling to {x.shape}')

    # End layer
    print('Constructing end layer')
    x = layers.GroupNormalization(groups=norm_groups)(x)
    x = keras.activations.swish(x)
    x = ResidualBlock(is3d, filters=layer_filters[0], groups=norm_groups)([x, time_mlp])
    x = _conv(is3d=is3d, filters=latent_dims, kernel_size=3, padding="same", kernel_initializer=kernel_init(0.0))(x)
    return keras.Model([data_input, time_input], x, name="unet")
