"""VAE trainer for an encoder-decoder pair."""

import keras
import tensorflow as tf

class VAETrainer(keras.Model):
    """Do an explaination here."""
    def __init__(self, encoder: keras.Model, decoder: keras.Model,
                 kld_loss_weight: float, **kwargs):
        super().__init__(**kwargs)
        self.encoder = encoder
        self.decoder = decoder

        self.kld_loss_weight = kld_loss_weight

        self.total_loss_tracker = keras.metrics.Mean(name="total_loss")
        self.rcstr_loss_tracker = keras.metrics.Mean(name="rcstr_loss")
        self.kld_loss_tracker = keras.metrics.Mean(name="kl_loss")

    @property
    def metrics(self):
        return [
            self.total_loss_tracker,
            self.rcstr_loss_tracker,
            self.kld_loss_tracker,
        ]

    def __eval_vae__(self, data):
        z_mean, z_log_var, z = self.encoder(data)
        r = self.decoder(z)

        rcstr_loss = tf.reduce_mean(
            tf.reduce_mean(
                keras.losses.sparse_categorical_crossentropy(data, r, axis=-1,),
                axis=tf.range(1, tf.rank(data)-1),
            ))

        kld_loss = tf.reduce_mean(
            tf.reduce_mean(
                -0.5 * (1 + z_log_var - tf.square(z_mean) - tf.exp(z_log_var)),
                axis=tf.range(1, tf.rank(data)-1),
            )) * self.kld_loss_weight

        total_loss = rcstr_loss + kld_loss
        return (total_loss, rcstr_loss, kld_loss)

    def train_step(self, data):
        with tf.GradientTape() as tape:
            total_loss, rcstr_loss, kld_loss = self.__eval_vae__(data)
            grads = tape.gradient(total_loss, self.trainable_weights)
            self.optimizer.apply_gradients(zip(grads, self.trainable_weights))
            self.total_loss_tracker.update_state(total_loss)
            self.rcstr_loss_tracker.update_state(rcstr_loss)
            self.kld_loss_tracker.update_state(kld_loss)
            return {
                "loss": self.total_loss_tracker.result(),
                "rcstr_loss": self.rcstr_loss_tracker.result(),
                "kl_loss": self.kld_loss_tracker.result(),
            }

    def test_step(self, data):
        total_loss, rcstr_loss, kld_loss = self.__eval_vae__(data)

        self.total_loss_tracker.update_state(total_loss)
        self.rcstr_loss_tracker.update_state(rcstr_loss)
        self.kld_loss_tracker.update_state(kld_loss)

        return {
            "loss": self.total_loss_tracker.result(),
            "rcstr_loss": self.rcstr_loss_tracker.result(),
            "kl_loss": self.kld_loss_tracker.result(),
        }
