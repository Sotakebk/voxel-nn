"""Code for building the diffusion models."""

import tensorflow as tf
import keras
from keras import layers
from voxelnn.diffusion.custom_layers import TimeEmbedding
from voxelnn.diffusion.utils import kernel_init

def build_model(
    input_shape: tuple[int, ...],
    latent_dims: int,
    layer_units: list[int],
    layer_has_attention: list[bool],
    layer_res_blocks: int = 2,
    norm_groups: int = 8,
    first_conv_channels: int = 32,
    time_embedding_dims: int = 32,
    min_time_emb_frequency: float = 1.0,
    max_time_emb_frequency: float = 10000.0):
    
    is3d = len(input_shape) == 3

    def _ResidualBlock(units: int, has_attention: bool, attention_num_heads = 4):
        def apply(inputs):
            x, time_embedding = inputs
            input_width = x.shape[4] if is3d else x.shape[3]
            residual = x if input_width == units else _conv(filters=units, kernel_size=1)(x)

            time_embedding = layers.Dense(units)(time_embedding)

            x = layers.GroupNormalization(groups=norm_groups)(x)
            x = keras.activations.swish(x)
            x = _conv(filters=units, kernel_size=3, padding="same")(x)

            x = layers.Add()([x, time_embedding])

            x = layers.GroupNormalization(groups=norm_groups)(x)
            x = keras.activations.swish(x)
            x = _conv(filters=units, kernel_size=3, padding="same")(x)

            x = layers.Add()([residual, x])

            if has_attention:
                residual = x
                x = layers.GroupNormalization(groups=norm_groups, center=False, scale=False)(x)
                if is3d:
                    x = layers.MultiHeadAttention(num_heads=attention_num_heads, key_dim=units, attention_axes=(1, 2))(x, x)
                else:
                    x = layers.MultiHeadAttention(num_heads=attention_num_heads, key_dim=units, attention_axes=(1, 2, 3))(x, x)
                x = layers.Add()([residual, x])

            return x

        return apply


    def _conv(filters: int, kernel_size: int = 3, stride: int = 1, padding: str = "same", kernel_initializer = None):
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
            
            return layers.Conv2D(filters=filters,
                                strides=(stride, stride),
                                kernel_size=kernel_size,
                                padding=padding,
                                kernel_initializer=_kernel_initializer)(x)

        return apply

    def _DownBlock(block_depth, units, has_attention):
        def apply(x):
            print("UNET: Adding down block")
            x, n, skips = x
            for i in range(block_depth):
                print(f"UNET: Adding residual block {i+1} in down block")
                x = _ResidualBlock(units, has_attention)([x, n])
                skips.append(x)
            if is3d:
                x = layers.AveragePooling3D(pool_size=2)(x)
            else:
                x = layers.AveragePooling2D(pool_size=2)(x)
            return x

        return apply

    def _UpBlock(block_depth, units, has_attention):
        def apply(x):
            print("UNET: Adding up block")
            x, n, skips = x
            if is3d:
                x = layers.UpSampling3D(size=2)(x)
            else:
                x = layers.UpSampling2D(size=2)(x)
            
            for i in range(block_depth):
                print(f"UNET: Adding residual block {i+1} in up block")
                x = layers.Concatenate()([x, skips.pop()])
                x = _ResidualBlock(units, has_attention)([x, n])
            return x

        return apply


    data_input = layers.Input(shape=(*input_shape, latent_dims), name='input')
    time_input = layers.Input(shape=(), dtype=tf.int64, name='time_input')

    print(f"UNET: Constructing a {('3d' if is3d else '2d')} diffusion model.")

    x = _conv(
        filters=first_conv_channels,
        kernel_size=3,
        padding='same',
        kernel_initializer=kernel_init(1.0),
    )(data_input)

    time_embedding = TimeEmbedding(dim=time_embedding_dims,
                                   min_frequency=min_time_emb_frequency,
                                   max_frequency=max_time_emb_frequency)(time_input)
    time_embedding = layers.Dense(time_embedding_dims, activation=keras.activations.swish, kernel_initializer=kernel_init(1.0))(time_embedding)
    time_embedding = layers.Dense(time_embedding_dims, activation=keras.activations.swish, kernel_initializer=kernel_init(1.0))(time_embedding)

    skips = [x]
    for res_blocks, units, has_attention in zip(layer_res_blocks[:-1],
                                                layer_units[:-1],
                                                layer_has_attention[:-1]):
        x = _DownBlock(block_depth=res_blocks, units=units, has_attention=has_attention)([x, time_embedding, skips])

    print('UNET: Adding middle block')
    units = layer_units[-1]
    res_blocks = layer_res_blocks[-1]
    has_attention = layer_has_attention[-1]
    for i in range(res_blocks):
        print(f"UNET: Adding residual block {(i+1)} in the middle")
        x = _ResidualBlock(units=units, has_attention=has_attention)([x, time_embedding])

    for res_blocks, units, has_attention in zip(layer_res_blocks[-2::-1],
                                                layer_units[-2::-1],
                                                layer_has_attention[-2::-1]):
        x = _UpBlock(block_depth=res_blocks, units=units, has_attention=has_attention)([x, time_embedding, skips])

    # End layer
    print('UNET: Adding end layer')
    x = layers.GroupNormalization(groups=norm_groups)(x)
    x = keras.activations.swish(x)
    x = _ResidualBlock(units=layer_units[0], has_attention=layer_has_attention[0])([x, time_embedding])
    x = _conv(filters=latent_dims, kernel_size=3, padding="same", kernel_initializer=kernel_init(0.0))(x)

    assert (x.shape == data_input.shape), "output shape not equal to input shape"
    return keras.Model([data_input, time_input], x, name="unet")
