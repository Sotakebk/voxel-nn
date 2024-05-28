"""SAE trainer for an encoder-decoder pair."""

import keras
import tensorflow as tf
import numpy as np
from . import ae_losses

class SAETrainer(keras.Model):
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
        r = self.decoder(z)

        reconstruction_loss = ae_losses.mean_sparse_categorical_crossentropy(data, r)

        kld_loss = ae_losses.mean_kld(z_mean, z_log_var) * self.kld_loss_weight

        str_loss = ae_losses.mean_structurization_loss(z, self.filter) * self.str_loss_weight

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
            "rcstr_loss": self.rcstr_loss_tracker.result(),
            "kl_loss": self.kld_loss_tracker.result(),
            "str_loss": self.str_loss_tracker.result(),
        }
