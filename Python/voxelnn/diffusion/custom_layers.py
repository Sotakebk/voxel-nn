"""Custom layers for the diffusion model."""

import math
import tensorflow as tf
import keras
from keras import layers
from voxelnn.diffusion.utils import kernel_init

@keras.saving.register_keras_serializable('voxel-nn')
class AttentionBlock(layers.Layer):
    """Applies self-attention.

    Args:
        units: Number of units in the dense layers
        groups: Number of groups to be used for GroupNormalization layer
    """

    def __init__(self, is3d: bool, units: int, groups: int = 8,
                 norm: layers.GroupNormalization=None,
                 query: layers.Dense=None,
                 key: layers.Dense=None,
                 value: layers.Dense=None,
                 proj: layers.Dense=None,
                 **kwargs):
        self.units = units
        self.groups = groups
        self.is3d = is3d
        super().__init__(**kwargs)

        self.norm = norm or layers.GroupNormalization(groups=groups)
        self.query = query or layers.Dense(units, kernel_initializer=kernel_init(1.0))
        self.key = key or layers.Dense(units, kernel_initializer=kernel_init(1.0))
        self.value = value or layers.Dense(units, kernel_initializer=kernel_init(1.0))
        self.proj = proj or layers.Dense(units, kernel_initializer=kernel_init(0.0))

    def call(self, inputs):
        batch_size = tf.shape(inputs)[0]
        height = tf.shape(inputs)[1]
        width = tf.shape(inputs)[2]
        scale = tf.cast(self.units, tf.float32) ** (-0.5)

        inputs = self.norm(inputs)
        q = self.query(inputs)
        k = self.key(inputs)
        v = self.value(inputs)

        if self.is3d:
            depth = tf.shape(inputs)[3]
            attn_score = tf.einsum("bhwdc, bHWDc->bhwdHWD", q, k) * scale
            attn_score = tf.reshape(attn_score, [batch_size, height, width, depth, height * width * depth])

            attn_score = tf.nn.softmax(attn_score, -1)
            attn_score = tf.reshape(attn_score, [batch_size, height, width, depth, height, width, depth])

            proj = tf.einsum("bhwdHWD,bHWDc->bhwdc", attn_score, v)
            proj = self.proj(proj)
        else:
            attn_score = tf.einsum("bhwc, bHWc->bhwHW", q, k) * scale
            attn_score = tf.reshape(attn_score, [batch_size, height, width, height * width])

            attn_score = tf.nn.softmax(attn_score, -1)
            attn_score = tf.reshape(attn_score, [batch_size, height, width, height, width])

            proj = tf.einsum("bhwHW,bHWc->bhwc", attn_score, v)
            proj = self.proj(proj)

        return inputs + proj

    def get_config(self):
        config = super().get_config()
        config['units'] = self.units
        config['groups'] = self.groups
        config['is3d'] = self.is3d
        config['norm'] = self.norm
        config['query'] = self.query
        config['key'] = self.key
        config['value'] = self.value
        config['proj'] = self.proj

        return config

    @classmethod
    def from_config(cls, config):
        config['norm'] = keras.layers.deserialize(config['norm'])
        config['query'] = keras.layers.deserialize(config['query'])
        config['key'] = keras.layers.deserialize(config['key'])
        config['value'] = keras.layers.deserialize(config['value'])
        config['proj'] = keras.layers.deserialize(config['proj'])

        return cls(**config)

    def build(self, input_shape = None):
        pass


@keras.saving.register_keras_serializable('voxel-nn')
class TimeEmbedding(layers.Layer):
    def __init__(self, dim, **kwargs):
        super().__init__(**kwargs)
        self.dim = dim
        self.half_dim = dim // 2
        self.emb = math.log(10000) / (self.half_dim - 1)
        self.emb = tf.exp(tf.range(self.half_dim, dtype=tf.float32) * -self.emb)

    def call(self, inputs):
        inputs = tf.cast(inputs, dtype=tf.float32)
        emb = inputs[:, None] * self.emb[None, :]
        emb = tf.concat([tf.sin(emb), tf.cos(emb)], axis=-1)
        return emb

    def get_config(self):
        config = super().get_config()
        config['dim'] = self.dim

        return config

    def build(self, input_shape = None):
        pass
