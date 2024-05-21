"""This module contains a trainer class for diffusion."""

import keras
from keras import layers
from keras import ops
import tensorflow as tf

@keras.saving.register_keras_serializable()
class DiffusionModel(keras.Model):
    def __init__(self, unet: keras.Model, expected_batch_size: int, min_signal_rate: float = 0.02, max_signal_rate: float = 0.95, ema: float = 0.999):
        super().__init__()

        self.normalizer = layers.Normalization()
        self.network = unet
        self.ema_network = keras.models.clone_model(unet)
        self.noise_loss_tracker = keras.metrics.Mean(name="n_loss")
        self.image_loss_tracker = keras.metrics.Mean(name="i_loss")
        self.min_signal_rate = min_signal_rate
        self.max_signal_rate = max_signal_rate
        self.expected_batch_size = expected_batch_size
        self.ema = ema

    @property
    def metrics(self):
        return [self.noise_loss_tracker, self.image_loss_tracker]

    def denormalize(self, data):
        return self.normalizer.mean + data * self.normalizer.variance**0.5

    def diffusion_schedule(self, diffusion_times):
        start_angle = ops.cast(ops.arccos(self.max_signal_rate), "float32")
        end_angle = ops.cast(ops.arccos(self.min_signal_rate), "float32")
        diffusion_angles = start_angle + diffusion_times * (end_angle - start_angle)
        signal_rates = ops.cos(diffusion_angles)
        noise_rates = ops.sin(diffusion_angles)
        return noise_rates, signal_rates

    def denoise(self, noisy_images, noise_rates, signal_rates, training):
        if training:
            network = self.network
        else:
            network = self.ema_network

        pred_noises = network([noisy_images, noise_rates**2], training=training)
        pred_images = (noisy_images - ops.reshape(noise_rates, self.get_times_shape()) * pred_noises) / ops.reshape(signal_rates, self.get_times_shape())

        return pred_noises, pred_images

    def reverse_diffusion(self, initial_noise, diffusion_steps):
        num_images = initial_noise.shape[0]
        step_size = 1.0 / diffusion_steps

        next_noisy_images = initial_noise
        for step in range(diffusion_steps):
            noisy_images = next_noisy_images

            # separate the current noisy image to its components
            diffusion_times = ops.ones((num_images,)) - step * step_size # 1D
            noise_rates, signal_rates = self.diffusion_schedule(diffusion_times) #1D
            pred_noises, pred_images = self.denoise(
                noisy_images, noise_rates, signal_rates, training=False
            )

            next_diffusion_times = diffusion_times - step_size
            next_noise_rates, next_signal_rates = self.diffusion_schedule(
                next_diffusion_times
            )
            next_noisy_images = (
                next_signal_rates * pred_images + next_noise_rates * pred_noises
            )

        return pred_images

    def get_data_input_shape(self, batch_size = None):
        batch_size = batch_size or self.expected_batch_size
        return (batch_size, *(self.network.input_shape[0])[1:])

    def get_data_input_real_dimensions(self):
        return len(self.network.input_shape[0]) - 2 # real dimensions minus one batch and one for latents
    
    def get_times_shape(self):
        return (self.expected_batch_size,) + (1,)*self.get_data_input_real_dimensions()

    def generate(self, num_images, diffusion_steps):
        initial_noise = keras.random.normal(shape=self.get_data_input_shape(num_images))
        generated_images = self.reverse_diffusion(initial_noise, diffusion_steps)
        generated_images = self.denormalize(generated_images)
        return generated_images

    def train_step(self, data):
        data = self.normalizer(data, training=True)
        noises = keras.random.normal(shape=self.get_data_input_shape())

        diffusion_times = keras.random.uniform(shape=(self.expected_batch_size,))
        noise_rates, signal_rates = self.diffusion_schedule(diffusion_times)
        noisy_images = ops.reshape(signal_rates, self.get_times_shape()) * data + ops.reshape(noise_rates, self.get_times_shape()) * noises

        with tf.GradientTape() as tape:
            pred_noises, pred_images = self.denoise(noisy_images, noise_rates, signal_rates, training=True)

            noise_loss = self.loss(noises, pred_noises)
            image_loss = self.loss(data, pred_images)

        gradients = tape.gradient(noise_loss, self.network.trainable_weights)
        self.optimizer.apply_gradients(zip(gradients, self.network.trainable_weights))

        self.noise_loss_tracker.update_state(noise_loss)
        self.image_loss_tracker.update_state(image_loss)

        for weight, ema_weight in zip(self.network.weights, self.ema_network.weights):
            ema_weight.assign(self.ema * ema_weight + (1 - self.ema) * weight)

        return {m.name: m.result() for m in self.metrics}

    def test_step(self, data):
        data = self.normalizer(data, training=False)
        noises = keras.random.normal(shape=self.get_data_input_shape())

        diffusion_times = keras.random.uniform(shape=self.get_times_shape())
        noise_rates, signal_rates = self.diffusion_schedule(diffusion_times)
        noisy_images = signal_rates * data + noise_rates * noises

        pred_noises, pred_images = self.denoise(noisy_images, noise_rates, signal_rates, training=False)

        noise_loss = self.loss(noises, pred_noises)
        image_loss = self.loss(data, pred_images)

        self.image_loss_tracker.update_state(image_loss)
        self.noise_loss_tracker.update_state(noise_loss)

        return {m.name: m.result() for m in self.metrics}
