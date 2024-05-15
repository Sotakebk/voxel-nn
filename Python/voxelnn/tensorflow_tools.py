"""Helper functions for dealing with Tensorflow."""

import tensorflow as tf

def maybe_use_tpu() -> bool:
    """Use TPU if available, otherwise don't do anything."""
    try:
        tpu = tf.distribute.cluster_resolver.TPUClusterResolver()  # TPU detection
        tf.config.experimental_connect_to_cluster(tpu)
        tf.tpu.experimental.initialize_tpu_system(tpu)
        tpu_strategy = tf.distribute.TPUStrategy(tpu)
        print('Running on TPU ', tpu.cluster_spec().as_dict()['worker'])
        print(tpu_strategy)
        return True
    except Exception as e: #pylint: disable=broad-except
        print(f"Something went wrong when trying to connect to a TPU: '{e}'.")
        return False
