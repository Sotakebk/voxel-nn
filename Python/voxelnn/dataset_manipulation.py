"""Methods to manipulate the dataset."""

import json
import numpy as np

def load_dataset(file_paths: list) -> tuple[list[str], list[str], list[str], np.ndarray, np.ndarray]:
    """Load entries from a list of files, then preprocess the entries into a unified dataset."""
    loaded_list = []
    for file_path in file_paths:
        with open(file_path, encoding='utf-8') as content:
            obj = json.load(content)
            loaded_list.extend(obj)

    entry_names = [obj['FriendlyName'] for obj in loaded_list]

    tag_names = sorted(list({tag for obj in loaded_list for tag in obj['Tags']}))

    block_names = sorted(list({block for obj in loaded_list for block in obj['BlockNames']}))

    dimensions_set = {tuple(obj['Dimensions']) for obj in loaded_list}
    dimensions = (0,)
    try:
        (dimensions,) = dimensions_set
    except Exception as e:
        print(f'Variable dimensions detected: {dimensions_set}')
        raise e

    tags_multilabel = np.array([[1 if tag_name in obj['Tags'] else 0 for tag_name in tag_names] for obj in loaded_list]).reshape((-1, len(tag_names)))

    for obj in loaded_list:
        local_to_global_map = [block_names.index(b) for b in obj['BlockNames']]
        obj['Blocks'] = [local_to_global_map[b] for b in obj['Blocks']]

    blocks = np.array([obj['Blocks'] for obj in loaded_list]).reshape((-1, *dimensions))

    return (tag_names, block_names, entry_names, tags_multilabel, blocks)


def cooccurence_matrix(tags_multilabel: np.ndarray, tag_names) -> tuple[np.ndarray, list[str]]:
    """Calculate """
    negative_labels = ['NOT ' + element for element in tag_names]
    result_names = tag_names + negative_labels
    negatives = 1 - tags_multilabel
    tags_multilabel = np.append(tags_multilabel, negatives, axis=1)
    _, tag_count = tags_multilabel.shape
    matrix = np.zeros((tag_count, tag_count))
    print(tags_multilabel.shape)
    for x in range(0, tag_count):
        for y in range(0, tag_count):
            if y > x:
                continue
            value = np.sum(np.multiply(tags_multilabel[:,x], tags_multilabel[:,y]))
            matrix[x, y] = value
            matrix[y, x] = value

    return matrix, result_names


def construct_entry_dto(friendly_name: str,
                        tag_names: list,
                        tags_multilabel: np.ndarray,
                        block_names: list,
                        blocks: np.ndarray) -> dict:
    """Construct a dictionary in expected data format from entry data.\n
    Make sure to pass the following arguments:
    * tags_multilabel: a one-dimensional ndarray of 1/0 markers for tag_names
    * blocks: a 2d/3d (slice of) an ndarray
    """

    def tag_names_from_tags(names, indices):
        l = []
        for i, v in enumerate(indices):
            if v == 1:
                l.append(names[i])
        return l

    o = {
        'FriendlyName': friendly_name,
        'Tags': tag_names_from_tags(tag_names, tags_multilabel),
        'Dimensions': blocks.shape,
        'BlockNames': block_names.copy(),
        'Blocks': np.concatenate(blocks).ravel().tolist()
    }
    return o
