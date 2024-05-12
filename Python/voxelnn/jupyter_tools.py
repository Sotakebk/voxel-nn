"""Helper methods for working with notebooks."""

import ipywidgets as widgets

def result_json_widget(text: str, title = 'JSON to copy:') -> widgets.Widget:
    """Use to display a text field to conveniently copy from."""
    return widgets.Text(
        value=text,
        placeholder='a JSON string should be here...',
        description=title,
        disabled=False)
