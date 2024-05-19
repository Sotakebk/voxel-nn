"""Custom layers for data coding/decoding."""

import tensorflow as tf
import keras
from keras import layers

@keras.saving.register_keras_serializable('voxel-nn')
class Sampling(layers.Layer):
    """Makes a sampling layer for an encoder of a VAE pair."""
    def __init__(self, stddev: float = 0.1, **kwargs):
        super().__init__(**kwargs)
        self.stddev = stddev

    def get_config(self):
        config = super().get_config()
        config['stddev'] = self.stddev
        return config
    
    def build(self, input_shape = None):
        pass

    def call(self, inputs):
        z_mean, z_log_var = inputs
        g = tf.random.get_global_generator()
        epsilon = g.normal(shape=tf.shape(z_mean), mean=0., stddev=self.stddev)
        value = z_mean + tf.exp(0.5 * z_log_var) * epsilon
        return value

@keras.saving.register_keras_serializable('voxel-nn')
class OneHot(layers.Layer):
    """Turns a tensor of indices into a tensor of one-hot vectors."""
    def __init__(self, max_value: int, **kwargs):
        super().__init__(**kwargs)
        self.max_value=max_value

    def get_config(self):
        config = super().get_config()
        config['max_value'] = self.max_value
        return config

    def build(self, input_shape = None):
        pass

    def call(self, inputs):
        return tf.one_hot(inputs, self.max_value, on_value=1.0, off_value=0.0, axis=-1)
