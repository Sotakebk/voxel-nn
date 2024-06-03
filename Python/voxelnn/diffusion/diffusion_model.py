"""This module contains a trainer class for diffusion."""

import random
from typing import Tuple
import keras
from keras import layers
import tensorflow as tf
import numpy as np

@keras.saving.register_keras_serializable()
class DiffusionModel(keras.Model):
    def __init__(self, network: keras.Model, min_signal_rate: float = 0.02, max_signal_rate: float = 0.95, ema: float = 0.999,
                 normalizer: keras.layers.Normalization = None, ema_network: keras.Model = None,
                 name: str = "diffusion_model", **kwargs):
        super().__init__(name=name, **kwargs)
        self.normalizer = normalizer or layers.Normalization(axis=-1)
        self.network = network
        self.ema_network = ema_network or keras.models.clone_model(network)
        self.noise_loss_tracker = keras.metrics.Mean(name="noise_loss")
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
        config['ema_network'] = keras.saving.serialize_keras_object(self.ema_network)
        config['normalizer'] = keras.saving.serialize_keras_object(self.normalizer)
        return config

    @classmethod
    def from_config(cls, config):
        config['network'] = keras.layers.deserialize(config['network'])
        config['ema_network'] = keras.layers.deserialize(config['ema_network'])
        config['normalizer'] = keras.layers.deserialize(config['normalizer'])

        return cls(**config)

    @property
    def metrics(self):
        return [self.noise_loss_tracker]

    @tf.function
    def _predict_noise(self, noisy_data: tf.Tensor, noise_powers: tf.Tensor, training: bool) -> tf.Tensor:
        """Takes noisy data and noise power, and attempts to predict the noise."""
        if training:
            network = self.network
        else:
            network = self.ema_network

        pred_noises = network([noisy_data, noise_powers], training=training)

        return pred_noises

#region utilities

    def _denormalize(self, data):
        return self.normalizer.mean + data * (self.normalizer.variance**0.5)

    @tf.function
    def _reshape_1d_for_data(self, rates, data):
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
    def _cosine_diffusion_schedule(self, diffusion_times) -> tf.Tensor:
        """Calculates a tensor of signal powers from diffusion timestamps."""
        max_signal_rate = tf.constant([float(self.max_signal_rate)])
        min_signal_rate = tf.constant([float(self.min_signal_rate)])
        start_angle = tf.acos(max_signal_rate)
        end_angle = tf.acos(min_signal_rate)
        diffusion_angles = start_angle + diffusion_times * (end_angle - start_angle)
        signal_rates = tf.cos(diffusion_angles)
        signal_powers = signal_rates ** 2
        return signal_powers

#endregion

    def generate(self, diffusion_steps: int = 10, element_count: int = 1, seed: int = None, method: str = 'DDIM'
                 ) -> tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
        seed = seed or random.randint(-100000000, 100000000)

        pred_data, pred_data_history, pred_noises_history, noisy_data_history = \
            self._generate_with_history(diffusion_steps=diffusion_steps, element_count=element_count, seed=seed, method=method)

        pred_data = self._denormalize(pred_data)
        pred_data_history = self._denormalize(pred_data_history)
        pred_noises_history = self._denormalize(pred_noises_history)
        noisy_data_history = self._denormalize(noisy_data_history)
        return pred_data.numpy(), pred_data_history.numpy(), pred_noises_history.numpy(), noisy_data_history.numpy()

    @tf.function
    def _generate_with_history(self, diffusion_steps: int, element_count: int, seed: int, method: str):
        input_shape = tf.constant(list(self.unet_data_input_shape)[1:])
        target_shape = tf.concat([[int(element_count)], input_shape], axis=-1)
        initial_noise = tf.random.normal(shape=target_shape, seed=seed)

        pred_data, pred_data_history, pred_noises_history, noisy_data_history = \
            self._reverse_diffusion_with_history(initial_noise, diffusion_steps, method)

        return pred_data, pred_data_history, pred_noises_history, noisy_data_history

    @tf.function
    def _self_norm(self, data):
        reduceDims = tf.range(1, tf.rank(data)-1)
        mean = tf.reduce_mean(data, axis=reduceDims, keepdims=True)
        var = tf.math.reduce_variance(data, axis=reduceDims, keepdims=True)
        result = (data - mean) / (var ** 0.5)
        return result

    @tf.function
    def _reverse_diffusion_with_history(self, initial_noise: tf.Tensor, diffusion_steps: int, method: str):
        tf.print('method:', method)
        step_size = 1.0 / float(diffusion_steps)

        next_noisy_data = initial_noise
        tf.print('initial_noise variance:', tf.math.reduce_variance(initial_noise))

        history_shape = tf.concat([[int(diffusion_steps)], initial_noise.shape], axis=-1)
        pred_noises_history = tf.zeros(shape=history_shape)
        pred_data_history = tf.zeros(shape=history_shape)
        noisy_data_history = tf.zeros(shape=history_shape)

        pred_noises = tf.zeros(shape=initial_noise.shape)
        pred_data = tf.zeros(shape=initial_noise.shape)
        for step in tf.range(diffusion_steps):
            tf.print('step ', (step+1), " out of ", diffusion_steps)
            noisy_data = next_noisy_data

            diffusion_times = tf.ones([1]) - float(step) * step_size
            tf.print('diffusion_times: ', diffusion_times)
            signal_powers = self._cosine_diffusion_schedule(diffusion_times)
            noise_powers = 1 - signal_powers
            noise_power_sqrt = noise_powers ** 0.5
            signal_power_sqrt = signal_powers ** 0.5
            tf.print('noise_powers:', noise_powers, 'signal_powers:', signal_powers)

            pred_noises = self._predict_noise(noisy_data=noisy_data, noise_powers=noise_powers, training=False)
            tf.print('pred_noises variance:', tf.math.reduce_variance(pred_noises))

            next_diffusion_times = tf.ones([1]) - float(step+1) * step_size
            next_signal_powers = self._cosine_diffusion_schedule(next_diffusion_times)
            next_noise_powers = 1 - next_signal_powers
            next_noise_power_sqrt = next_noise_powers ** 0.5
            next_signal_power_sqrt = next_signal_powers ** 0.5
            tf.print('next_noise_powers:', next_noise_powers, 'next_signal_powers:', next_signal_powers)


            if method == 'DDIM':
                pred_data = (noisy_data - (noise_power_sqrt * pred_noises)) / signal_power_sqrt
                tf.print('pred_data variance:', tf.math.reduce_variance(pred_data))
                noisy_data = next_signal_power_sqrt * pred_data + next_noise_power_sqrt * pred_noises
                tf.print('noisy_data variance:', tf.math.reduce_variance(pred_data))
            elif method == 'DDIM-NORM':
                pred_data = (noisy_data - (noise_power_sqrt * pred_noises)) / signal_power_sqrt
                tf.print('pred_data variance:', tf.math.reduce_variance(pred_data))
                noisy_data = next_signal_power_sqrt * pred_data + next_noise_power_sqrt * pred_noises
                noisy_data = next_signal_powers * noisy_data + next_noise_powers * self._self_norm(noisy_data)
                tf.print('noisy_data variance:', tf.math.reduce_variance(pred_data))
            elif method == 'DDPM':
                sigma = ((next_noise_powers/noise_powers) ** 0.5) * ((noise_powers/next_signal_powers) ** 0.5)
                pred_data = (noisy_data - (noise_power_sqrt * pred_noises)) / signal_power_sqrt
                random_noise = tf.random.normal(shape=initial_noise.shape)
                noisy_data = next_signal_power_sqrt * pred_data + ((next_noise_powers * sigma ** 2) ** 0.5) * pred_noises + sigma * random_noise
            elif method == 'EXPERIMENTAL':
                pred_data = (noisy_data - (noise_power_sqrt * pred_noises)) / signal_power_sqrt
                tf.print('pred_data variance:', tf.math.reduce_variance(pred_data))
                noisy_data = next_signal_power_sqrt * pred_data - next_noise_power_sqrt * pred_noises
                tf.print('noisy_data variance:', tf.math.reduce_variance(pred_data))
            elif method == 'EXPERIMENTAL2':
                pred_data = noisy_data - pred_noises * noise_power_sqrt
                tf.print('pred_data variance:', tf.math.reduce_variance(pred_data))
                noisy_data = next_signal_power_sqrt * pred_data - next_noise_power_sqrt * pred_noises
                tf.print('noisy_data variance:', tf.math.reduce_variance(pred_data))
            elif method == 'EXPERIMENTAL3':
                pred_data = (noisy_data - (noise_power_sqrt * pred_noises)) / signal_power_sqrt
                tf.print('pred_data variance:', tf.math.reduce_variance(pred_data))
                noisy_data = next_signal_power_sqrt * pred_data - next_noise_power_sqrt * pred_noises
                tf.print('noisy_data variance:', tf.math.reduce_variance(pred_data))
            else:
                raise Exception(f"Unknown method '{method}'!")

            # update history here
            indices = tf.reshape(step, [1,1])  # The iteration index
            pred_noises_updates = tf.expand_dims(pred_noises, 0)
            pred_noises_history = tf.tensor_scatter_nd_update(pred_noises_history, indices, pred_noises_updates)
            pred_data_updates = tf.expand_dims(pred_data, 0)
            pred_data_history = tf.tensor_scatter_nd_update(pred_data_history, indices, pred_data_updates)
            noisy_data_updates = tf.expand_dims(noisy_data, 0)
            noisy_data_history = tf.tensor_scatter_nd_update(noisy_data_history, indices, noisy_data_updates)
            next_noisy_data = noisy_data

        return pred_data, pred_data_history, pred_noises_history, noisy_data_history

#region training

    def learn_data_distribution(self, data):
        self.normalizer.adapt(data)
        print(f'DiffusionModel: Learned data mean: {self.normalizer.mean}')
        print(f'DiffusionModel: Learned data variance: {self.normalizer.variance}')

    @tf.function
    def train_step(self, data):
        with tf.GradientTape() as tape:
            pred_noises, noises = self(data, training=True)

            noise_loss = self.loss(noises, pred_noises)

        gradients = tape.gradient(noise_loss, self.network.trainable_weights)
        self.optimizer.apply_gradients(zip(gradients, self.network.trainable_weights))

        self.noise_loss_tracker.update_state(noise_loss)

        for weight, ema_weight in zip(self.network.weights, self.ema_network.weights):
            ema_weight.assign(self.ema * ema_weight + (1 - self.ema) * weight)

        return {m.name: m.result() for m in self.metrics}

    @tf.function
    def test_step(self, data):
        pred_noises, noises = self(data, training=False)

        noise_loss = self.loss(noises, pred_noises)

        self.noise_loss_tracker.update_state(noise_loss)

        return {m.name: m.result() for m in self.metrics}

    @tf.function
    def call(self, inputs, training=False) -> Tuple[tf.Tensor, tf.Tensor, tf.Tensor]:
        """Samples random timestamps, then attempts to predict noise at those timestamps for noisy data.\
            Returns tf.Tensor of predicted noise, predicted data, and the noise that was added."""
        normalized_data = self.normalizer(inputs, training=False)
        input_shape = tf.shape(normalized_data)
        noises = tf.random.normal(shape=input_shape)
        noise_powers = tf.random.uniform(
            shape=(input_shape[0],), minval=0.0, maxval=1.0
        )
        signal_powers = 1.0 - noise_powers
        noise_rates = noise_powers**0.5
        signal_rates = signal_powers**0.5
        # mix the images with noises accordingly
        signal_rates_reshaped = self._reshape_1d_for_data(signal_rates, normalized_data)
        noise_rates_reshaped = self._reshape_1d_for_data(noise_rates, noises)
        noisy_data = signal_rates_reshaped * normalized_data + noise_rates_reshaped * noises

        pred_noises = self._predict_noise(noisy_data=noisy_data, noise_powers=noise_powers, training=training)

        return pred_noises, noises

# endregion
