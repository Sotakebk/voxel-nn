"""SAE trainer for an encoder-decoder pair."""

import keras
import tensorflow as tf
import numpy as np

class SAE(keras.Model):
    """Do an explaination here."""
    def __init__(self, encoder: keras.Model, decoder: keras.Model, filter_array : np.ndarray,
                 kld_loss_weight: float, str_loss_weight: float, **kwargs):
        super().__init__(**kwargs)

        self.encoder = encoder
        self.decoder = decoder
        self.filter = filter_array
    
        self.kld_loss_weight = kld_loss_weight
        self.str_loss_weight = str_loss_weight
        
        self.total_loss_tracker = keras.metrics.Mean(name="total_loss")
        self.rcstr_loss_tracker = keras.metrics.Mean(name="rcstr_loss")
        self.kld_loss_tracker = keras.metrics.Mean(name="kld_loss")
        self.str_loss_tracker = keras.metrics.Mean(name="str_loss")

    @property
    def metrics(self):
        return [
            self.total_loss_tracker,
            self.rcstr_loss_tracker,
            self.kld_loss_tracker,
            self.str_loss_tracker,
        ]
    
    def __eval_sae__(self, data):
        z_mean, z_log_var, z = self.encoder(data)
        reconstruction = self.decoder(z)

        reconstruction_loss = tf.reduce_mean(
            tf.reduce_mean(
                keras.losses.sparse_categorical_crossentropy(data, reconstruction, axis=-1,),
                axis=tf.range(1, tf.rank(data)-1),
            ))
        
        kld_loss = tf.reduce_mean(
            tf.reduce_mean(
                -0.5 * (1 + z_log_var - tf.square(z_mean) - tf.exp(z_log_var)),
                axis=tf.range(1, tf.rank(data)-1),
            )) * self.kld_loss_weight

        averages = tf.conv(input=z, filter=tf.constant(self.filter, dtype=z.dtype),
                           strides=[1]*tf.rank(data), padding='SAME')

        str_loss = tf.reduce_mean(
            tf.reduce_mean(
                tf.math.reduce_euclidean_norm(z - averages, axis=-1,),
                axis=tf.range(1, tf.rank(data)-1),
            )) * self.str_loss_weight

        total_loss = reconstruction_loss + kld_loss + str_loss 
        return (total_loss, reconstruction_loss, kld_loss, str_loss)
        
    def train_step(self, data):
        with tf.GradientTape() as tape:
            total_loss, reconstruction_loss, kld_loss, str_loss = self.__eval_sae__(data)
            grads = tape.gradient(total_loss, self.trainable_weights)
            self.optimizer.apply_gradients(zip(grads, self.trainable_weights))
            self.total_loss_tracker.update_state(total_loss)
            self.rcstr_loss_tracker.update_state(reconstruction_loss)
            self.kld_loss_tracker.update_state(kld_loss)
            self.str_loss_tracker.update_state(str_loss)

            return {
                "loss": self.total_loss_tracker.result(),
                "rcstr_loss": self.rcstr_loss_tracker.result(),
                "kld_loss": self.kld_loss_tracker.result(),
                "str_loss": self.str_loss_tracker.result(),
            }

    def test_step(self, data):
        total_loss, reconstruction_loss, kld_loss, str_loss = self.__eval_sae__(data)

        self.total_loss_tracker.update_state(total_loss)
        self.rcstr_loss_tracker.update_state(reconstruction_loss)
        self.kld_loss_tracker.update_state(kld_loss)
        self.str_loss_tracker.update_state(str_loss)

        return {
            "loss": self.total_loss_tracker.result(),
            "reconstruction_loss": self.rcstr_loss_tracker.result(),
            "kl_loss": self.kld_loss_tracker.result(),
            "structurization_loss": self.str_loss_tracker.result(),
        }
