"""This module contains loss functions shared by VAE and SAE."""

import tensorflow as tf
import keras

@tf.function
def mean_sparse_categorical_crossentropy(data, reconstruction):
    scc = keras.losses.sparse_categorical_crossentropy(data, reconstruction, axis=-1)
    mean = tf.reduce_mean(scc)

    return mean

@tf.function
def mean_kld(z_mean, z_log_var):
    div = -0.5 * (1 + z_log_var - tf.square(z_mean) - tf.exp(z_log_var))
    mean = tf.reduce_mean(div)

    return mean

def mean_structurization_loss(z, conv_filter):
    averages = None

    if conv_filter.ndim == 4: # 2d conv
        averages = tf.nn.conv2d(input=z, filters=tf.constant(conv_filter, dtype=z.dtype),
                        strides=[1]*conv_filter.ndim, padding='SAME')
    elif conv_filter.ndim == 5: # 3d conv
        averages = tf.nn.conv3d(input=z, filters=tf.constant(conv_filter, dtype=z.dtype),
                        strides=[1]*conv_filter.ndim, padding='SAME')
    else:
        raise Exception("Only 2D and 3D conv is supported")

    norms = tf.math.reduce_euclidean_norm(z - averages, axis=-1)
    str_loss = tf.reduce_mean(norms)
    
    return str_loss
