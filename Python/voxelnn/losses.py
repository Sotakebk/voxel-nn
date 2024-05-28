"""This module contains loss functions used by models."""

import tensorflow as tf
import keras

class EuclideanDistanceLatentVectorLoss(keras.Loss):
    def __init__(self, name='euclidean_distance_latent_vector_loss'):
        super(EuclideanDistanceLatentVectorLoss, self).__init__(name=name)

    @tf.function
    def call(self, y_true, y_pred):
        # squared difference of each latent dimensions
        squared_diff = tf.square(y_true - y_pred)

        # per latent vector, squared distance
        loss = tf.reduce_sum(squared_diff, axis=-1)
        # square root
        loss = tf.sqrt(loss)
        loss = tf.reduce_mean(loss)

        return loss
