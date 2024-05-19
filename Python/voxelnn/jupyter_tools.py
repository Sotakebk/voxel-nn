"""Helper methods for working with notebooks."""

import os
import json
from typing import Tuple
import keras
import ipywidgets as widgets
import matplotlib.pyplot as plt

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
