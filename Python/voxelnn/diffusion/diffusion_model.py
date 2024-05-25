"""This module contains a trainer class for diffusion."""

import random
from typing import Tuple
import keras
from keras import layers
import tensorflow as tf

@keras.saving.register_keras_serializable()
class DiffusionModel(keras.Model):
    def __init__(self, network: keras.Model, min_signal_rate: float = 0.02, max_signal_rate: float = 0.95, ema: float = 0.999,
                 normalizer: keras.layers.Normalization = None, name: str = "diffusion_model", **kwargs):
        super().__init__(name=name, **kwargs)
        self.normalizer = normalizer or layers.Normalization(axis=-1)
        self.network = network
        self.ema_network = keras.models.clone_model(network)
        self.noise_loss_tracker = keras.metrics.Mean(name="noise_loss")
        self.recovery_loss_tracker = keras.metrics.Mean(name="rcvr_loss")
        self.min_signal_rate = min_signal_rate
        self.max_signal_rate = max_signal_rate
        self.ema = ema
        self.unet_data_input_shape = network.input[0].shape

    def get_config(self):
        config = super().get_config()
        config['min_signal_rate'] = self.min_signal_rate
        config['max_signal_rate'] = self.max_signal_rate
        config['ema'] = self.ema
        config['network'] = keras.saving.serialize_keras_object(self.network)
        config['normalizer'] = keras.saving.serialize_keras_object(self.normalizer)
        return config

    @classmethod
    def from_config(cls, config):
        config['network'] = keras.layers.deserialize(config['network'])
        config['normalizer'] = keras.layers.deserialize(config['normalizer'])

        return cls(**config)

    @property
    def metrics(self):
        return [self.noise_loss_tracker, self.recovery_loss_tracker]

    @tf.function
    def _denoise(self, noisy_data: tf.Tensor, noise_rates: tf.Tensor, signal_rates: tf.Tensor, training: bool):
        """Takes noisy data, noise and signal rates, and attempts to predict the noise, \
            then subtracts the noise from the data."""
        if training:
            network = self.network
        else:
            network = self.ema_network

        signal_rates_reshaped = self._reshape_rates_for_data(signal_rates, noisy_data)
        noise_rates_reshaped = self._reshape_rates_for_data(noise_rates, noisy_data)
        pred_noises = network([noisy_data, noise_rates**2], training=training)
        pred_data = (noisy_data - noise_rates_reshaped * pred_noises) / signal_rates_reshaped

        return pred_noises, pred_data

#region utilities

    def _denormalize(self, data):
        return self.normalizer.mean + data * self.normalizer.variance**0.5

    @tf.function
    def _reshape_rates_for_data(self, rates, data):
        """Reshape a 1D rates tensor to multiply data tensor with."""
        input_shape = tf.shape(data)
        input_rank = tf.rank(data)
        dims_to_append = input_rank - 1
        dims_to_append = tf.reshape(dims_to_append, [1])
        ones = tf.ones(dims_to_append, tf.dtypes.int32)
        target_shape = tf.concat([input_shape[:1], ones], axis=-1)
        rates_reshaped = tf.reshape(rates, target_shape)
        return rates_reshaped

    @tf.function
    def _diffusion_schedule(self, diffusion_times):
        """Calculates two 1D tensors of noise and signal rate for each value for given diffusion timestamps."""
        max_signal_rate = tf.constant([float(self.max_signal_rate)])
        min_signal_rate = tf.constant([float(self.min_signal_rate)])
        start_angle = tf.acos(max_signal_rate)
        end_angle = tf.acos(min_signal_rate)
        diffusion_angles = start_angle + diffusion_times * (end_angle - start_angle)
        signal_rates = tf.cos(diffusion_angles)
        noise_rates = tf.sin(diffusion_angles)
        return noise_rates, signal_rates

#endregion

    def generate_one_sample_with_history(self, diffusion_steps: int, seed: int = None):
        seed = seed or random.randint(-100000000, 100000000)
        pred_data, pred_data_history, pred_noises_history, noisy_data_history = self._generate_one_sample_with_history(diffusion_steps, seed)
        pred_data = self._denormalize(pred_data)
        pred_data_history = self._denormalize(pred_data_history)
        pred_noises_history = self._denormalize(pred_noises_history)
        noisy_data_history = self._denormalize(noisy_data_history)
        return pred_data.numpy(), pred_data_history.numpy(), pred_noises_history.numpy(), noisy_data_history.numpy()

    def generate_many_samples(self, element_count, diffusion_steps, seed: int = None):
        seed = seed or random.randint(-100000000, 100000000)
        data = self._generate_many_samples(element_count, diffusion_steps, seed)
        data = self._denormalize(data) # maybe put this in _generate_many
        return data.numpy()

    @tf.function
    def _generate_many_samples(self, num_samples, diffusion_steps, seed):
        batch_size = tf.constant([int(num_samples)], tf.dtypes.int32)
        input_shape = tf.constant(list(self.unet_data_input_shape)[1:])
        target_shape = tf.concat([batch_size, input_shape], axis=-1)
        initial_noise = tf.random.normal(shape=target_shape, seed=seed)
        generated_samples = self._reverse_diffusion_for_many(initial_noise, diffusion_steps)
        return generated_samples
    
    @tf.function
    def _generate_one_sample_with_history(self, diffusion_steps, seed):
        input_shape = tf.constant(list(self.unet_data_input_shape)[1:])
        target_shape = tf.concat([[1], input_shape], axis=-1)
        initial_noise = tf.random.normal(shape=target_shape, seed=seed)
        pred_data, pred_data_history, pred_noises_history, noisy_data_history = \
            self._reverse_diffusion_for_one_sample_with_history(initial_noise, diffusion_steps)
        return pred_data, pred_data_history, pred_noises_history, noisy_data_history

    @tf.function
    def _reverse_diffusion_for_many(self, initial_noise, diffusion_steps):
        batch_size = initial_noise.shape[0]
        step_size = 1.0 / float(diffusion_steps)

        next_noisy_data = initial_noise
        pred_noises = tf.zeros(shape=initial_noise.shape)
        pred_data = tf.zeros(shape=initial_noise.shape)
        for step in tf.range(diffusion_steps):
            tf.print('step ', (step+1), " out of ", diffusion_steps)
            noisy_data = next_noisy_data

            # separate the current noisy data to its components
            diffusion_times = tf.ones((batch_size,)) - step * float(step_size) # 1D
            noise_rates, signal_rates = self._diffusion_schedule(diffusion_times) #1D
            pred_noises, pred_data  = self._denoise(
                noisy_data, noise_rates, signal_rates, training=False
            )

            next_diffusion_times = diffusion_times - float(step_size)
            next_noise_rates, next_signal_rates = self._diffusion_schedule(
                next_diffusion_times
            )
            next_noisy_data = (
                next_signal_rates * pred_data + next_noise_rates * pred_noises
            )

        return pred_data
    
    @tf.function
    def _reverse_diffusion_for_one_sample_with_history(self, initial_noise, diffusion_steps):
        step_size = 1.0 / float(diffusion_steps)

        next_noisy_data = initial_noise

        history_shape = tf.concat([[int(diffusion_steps)], initial_noise.shape[1:]], axis=-1)
        pred_noises_history = tf.zeros(shape=history_shape)
        pred_data_history = tf.zeros(shape=history_shape)
        noisy_data_history = tf.zeros(shape=history_shape)

        pred_noises = tf.zeros(shape=initial_noise.shape)
        pred_data = tf.zeros(shape=initial_noise.shape)
        for step in tf.range(diffusion_steps):
            tf.print('step ', (step+1), " out of ", diffusion_steps)
            noisy_data = next_noisy_data

            diffusion_times = tf.ones([1]) - float(step) * step_size # 1D
            noise_rates, signal_rates = self._diffusion_schedule(diffusion_times) #1D
            # separate the current noisy data to its components
            pred_noises, pred_data  = self._denoise(
                noisy_data, noise_rates, signal_rates, training=False
            )

            next_diffusion_times = diffusion_times - step_size
            next_noise_rates, next_signal_rates = self._diffusion_schedule(
                next_diffusion_times
            )
            next_noisy_data = (
                next_signal_rates * pred_data + next_noise_rates * pred_noises
            )

            # update history here
            indices = tf.reshape(step, shape=(1, 1))
            pred_noises_history = tf.tensor_scatter_nd_update(pred_noises_history, indices, pred_noises)
            pred_data_history = tf.tensor_scatter_nd_update(pred_data_history, indices, pred_data)
            noisy_data_history = tf.tensor_scatter_nd_update(noisy_data_history, indices, next_noisy_data)

        return pred_data, pred_data_history, pred_noises_history, noisy_data_history

#region training

    @tf.function
    def train_step(self, data):
        with tf.GradientTape() as tape:
            pred_noises, pred_data, noises = self(data, training=True)

            noise_loss = self.loss(noises, pred_noises)
            recovery_loss = self.loss(data, pred_data)

        gradients = tape.gradient(noise_loss, self.network.trainable_weights)
        self.optimizer.apply_gradients(zip(gradients, self.network.trainable_weights))

        self.noise_loss_tracker.update_state(noise_loss)
        self.recovery_loss_tracker.update_state(recovery_loss)

        for weight, ema_weight in zip(self.network.weights, self.ema_network.weights):
            ema_weight.assign(self.ema * ema_weight + (1 - self.ema) * weight)

        return {m.name: m.result() for m in self.metrics}

    @tf.function
    def test_step(self, data):
        pred_noises, pred_data, noises = self(data, training=False)

        noise_loss = self.loss(noises, pred_noises)
        recovery_loss = self.loss(data, pred_data)

        self.recovery_loss_tracker.update_state(recovery_loss)
        self.noise_loss_tracker.update_state(noise_loss)

        return {m.name: m.result() for m in self.metrics}

    @tf.function
    def call(self, inputs, training=False) -> Tuple[tf.Tensor, tf.Tensor, tf.Tensor]:
        """Samples random timestamps, then attempts to predict noise at those timestamps for noisy data.\
            Returns tf.Tensor of predicted noise, predicted data, and the noise that was added."""
        data = self.normalizer(inputs, training=training)
        input_shape = tf.shape(data)
        noises = tf.random.normal(shape=input_shape)
        diffusion_times = tf.random.uniform(shape=input_shape[:1])
        noise_rates, signal_rates = self._diffusion_schedule(diffusion_times)
        signal_rates_reshaped = self._reshape_rates_for_data(signal_rates, data)
        noise_rates_reshaped = self._reshape_rates_for_data(noise_rates, noises)
        noisy_data = signal_rates_reshaped * data + noise_rates_reshaped * noises

        if training:
            network = self.network
        else:
            network = self.ema_network

        pred_noises = network([noisy_data, noise_rates**2], training=training)
        pred_data = ((noisy_data - noise_rates_reshaped) * pred_noises) / signal_rates_reshaped

        return pred_noises, pred_data, noises
# endregion
