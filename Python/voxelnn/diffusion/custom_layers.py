"""Custom layers for the diffusion model."""

import math
import tensorflow as tf
import keras
from keras import layers

@keras.saving.register_keras_serializable('voxel-nn')
class TimeEmbedding(layers.Layer):
    def __init__(self, dim, min_frequency, max_frequency, **kwargs):
        super().__init__(**kwargs)
        self.dim = dim
        self.min_frequency = min_frequency
        self.max_frequency = max_frequency
        half_dim = dim // 2
        frequencies = tf.exp(
            tf.linspace(
                math.log(min_frequency),
                math.log(max_frequency),
                half_dim
            ))
        self.angular_speeds = 2.0 * math.pi * frequencies

    def call(self, inputs):
        inputs = tf.cast(inputs, dtype=tf.float32)
        points = self.angular_speeds * inputs[:, None]
        s = tf.sin(points)
        c = tf.cos(points)
        embeddings = tf.concat([s, c], axis=1)
        return embeddings

    def get_config(self):
        config = super().get_config()
        config['dim'] = self.dim
        config['min_frequency'] = self.min_frequency
        config['max_frequency'] = self.max_frequency
        return config

    def build(self, input_shape = None):
        pass
