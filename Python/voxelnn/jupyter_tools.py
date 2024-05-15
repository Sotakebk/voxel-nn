"""Helper methods for working with notebooks."""

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
