"""Helper methods for working with notebooks."""

import os
import json
from typing import Tuple
import keras
import ipywidgets as widgets
import matplotlib as mpl
import matplotlib.pyplot as plt
import numpy as np
import seaborn as sns

def result_json_widget(text: str, title = 'JSON to copy:') -> widgets.Widget:
    """Use to display a text field to conveniently copy from."""
    return widgets.Text(
        value=text,
        placeholder='a JSON string should be here...',
        description=title,
        disabled=False)

def plot_history(history):
    """Plot training history."""
    for k in history.history.keys():
        plt.plot(history.history[k])
        plt.title('history')
        plt.xlabel('epoch')
    plt.legend(history.history.keys(), loc='upper left')
    plt.show()

path_to_models = None

def prepare_io(group: str):
    global path_to_models
    if path_to_models is None:
        try:
            from google.colab import drive
            drive.mount('/content/drive/')
            path_to_models = '/content/drive/MyDrive/voxel-nn/models/'
        except Exception:
            # probably not using google colab then
            path_to_models = '../models/'
    p = os.path.join(path_to_models, group)
    if not os.path.exists(p):
        os.makedirs(p)

def save_model_data(group: str, blocks: list, tags: list):
    prepare_io(group)
    d = {
        "Blocks": blocks,
        "Tags": tags
    }
    with open(os.path.join(path_to_models, group, 'data.json'), 'w') as f:
        json.dump(d, f)

def load_model_data(group: str) -> Tuple[list, list]:
    prepare_io(group)
    with open(os.path.join(path_to_models, group, 'data.json'), 'r') as f:
        data = json.load(f)
        return list(data["Blocks"]), list(data["Tags"])

def load_model(group: str, name: str, **kwargs):
    prepare_io(group)
    return keras.models.load_model(os.path.join(path_to_models, group, name), **kwargs)

def save_model(group: str, name: str, model: keras.Model, **kwargs):
    prepare_io(group)
    model.save(os.path.join(path_to_models, group, name), **kwargs)

def render_data_history_and_other_stuff(pred_data_history, decoder, index = 0):
    iterations = pred_data_history.shape[0]
    unit_per_box = 4
    f, axarr = plt.subplots(iterations, 2, sharex=True, sharey=True, figsize=(unit_per_box*2, unit_per_box*iterations))
    decoded_history = decoder.predict(pred_data_history[:,index,...])
    decoded_history = np.argmax(decoded_history, axis=-1).astype(int)
    colors_local = np.array(mpl.colormaps['Set1'].colors)
    colored_history = colors_local[decoded_history]
    plt.tight_layout()

    def render_data(data, row, column):
        min_value = np.min(data)
        max_value = np.max(data)
        normalized_data = (data - min_value) / (max_value - min_value)
        normalized_data = np.clip(normalized_data, 0, 1)
        with_zeros = np.zeros((*data.shape[:2], 3))
        with_zeros[:,:,:2] = normalized_data
        axes = axarr[row, column]
        swapped = np.swapaxes(with_zeros, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')
        axes.set_title(f'mean: {data.mean():.5f}, var: {data.var():.5f}')

    def render_result(data, row, column):
        axes = axarr[row, column]
        swapped = np.swapaxes(data, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')
        #axes.set_title(title)

    for iteration in range(iterations):
        render_data(pred_data_history[iteration, index, ...], iteration, 0)
        render_result(colored_history[iteration, ...], iteration, 1)

def peek_encoding_and_decoding(blocks, encoder, decoder, samples=10):
    rows = samples
    columns = 5
    z_mean, _, z = encoder.predict(blocks[:samples,...])
    z_mean_decoded = decoder.predict(z_mean)
    z_decoded = decoder.predict(z)
    z_mean_decoded = np.argmax(z_mean_decoded, axis=-1).astype(int)
    z_decoded = np.argmax(z_decoded, axis=-1).astype(int)
    colors_local = np.array(mpl.colormaps['Set1'].colors)
    colored_z_mean_decoded = colors_local[z_mean_decoded]
    colored_z_decoded = colors_local[z_decoded]
    colored_blocks = colors_local[blocks]

    size_per_unit = 3
    f, axarr = plt.subplots(rows, columns, sharex=True, sharey=True, figsize=(columns * size_per_unit, rows * size_per_unit))

    def render_data(data, row, column):
        min_value = np.min(data)
        max_value = np.max(data)
        normalized_data = (data - min_value) / (max_value - min_value)
        normalized_data = np.clip(normalized_data, 0, 1)
        with_zeros = np.zeros((*data.shape[:2], 3))
        with_zeros[:,:,:2] = normalized_data
        axes = axarr[row, column]
        swapped = np.swapaxes(with_zeros, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.set_title(f'mean: {data.mean():.5f}, var: {data.var():.5f}')
        axes.axis('off')

    def render_result(data, row, column, title):
        axes = axarr[row, column]
        swapped = np.swapaxes(data, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')
        axes.set_title(title)

    for i in range(samples):
        render_result(colored_blocks[i, ...], i, 0, title='original')
        render_data(z_mean[i,...], i, 1)
        render_result(colored_z_mean_decoded[i, ...], i, 2, title='mean decoded')
        render_data(z[i,...], i, 3)
        render_result(colored_z_decoded[i, ...], i, 4, title='z decoded')


def preview_distribution_over_time(pred_data_history, pred_noise_history):
    iterations = pred_data_history.shape[0]
    size_per_unit = 4
    fig = plt.figure()
    fig.suptitle("Value distributions for predicted data and predicted noise over time.", fontsize=16)
    fig, axes = plt.subplots(iterations, 2, figsize=(2 * size_per_unit, iterations * size_per_unit))
    for i in range(iterations):
        sns.kdeplot(ax = axes[i, 0], x=pred_data_history[i,:,:,:,0].flatten(), y=pred_data_history[i,:,:,:,1].flatten(), fill=True, levels=100, thresh=0, cmap='Spectral_r')
        axes[i, 0].set_title(f'Predicted data for t={i+1}/{iterations}')
        sns.kdeplot(ax = axes[i, 1], x=pred_noise_history[i,:,:,:,0].flatten(), y=pred_noise_history[i,:,:,:,1].flatten(), fill=True, levels=100, thresh=0, cmap='Spectral_r')
        axes[i, 1].set_title(f'Predicted noise for t={i+1}/{iterations}')

def render_all_predictions(pred_data, decoder):
    entries = pred_data.shape[0]
    unit_per_box = 4
    f, axarr = plt.subplots(entries//2, 4, sharex=True, sharey=True, figsize=(unit_per_box*4, unit_per_box*(entries//2)))
    decoded_data = decoder.predict(pred_data)
    decoded_data = np.argmax(decoded_data, axis=-1).astype(int)
    colors_local = np.array(mpl.colormaps['Set1'].colors)
    colored_data = colors_local[decoded_data]
    plt.tight_layout()

    def render_data(data, row, column):
        min_value = np.min(data)
        max_value = np.max(data)
        normalized_data = (data - min_value) / (max_value - min_value)
        normalized_data = np.clip(normalized_data, 0, 1)
        with_zeros = np.zeros((*data.shape[:2], 3))
        with_zeros[:,:,:2] = normalized_data
        axes = axarr[row, column]
        swapped = np.swapaxes(with_zeros, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')

    def render_result(data, row, column):
        axes = axarr[row, column]
        swapped = np.swapaxes(data, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')
        #axes.set_title(title)

    for i in range(entries):
        r = i // 2
        c = i % 2
        c1 = c * 2
        c2 = c * 2 + 1
        render_data(pred_data[i, ...], r, c1)
        render_result(colored_data[i, ...], r, c2)

def render_some_data(blocks):
    entries = blocks.shape[0]
    unit_per_box = 4
    f, axarr = plt.subplots(entries//4, 4, sharex=True, sharey=True, figsize=(unit_per_box*4, unit_per_box*(entries//4)))
    colors_local = np.array(mpl.colormaps['Set1'].colors)
    colored_data = colors_local[blocks]
    plt.tight_layout()

    def render_result(data, row, column):
        axes = axarr[row, column]
        swapped = np.swapaxes(data, 0,1)
        axes.imshow(swapped, origin='lower')
        axes.axis('off')
        #axes.set_title(title)

    for i in range(entries):
        r = i // 4
        c = i % 4
        render_result(colored_data[i, ...], r, c)
