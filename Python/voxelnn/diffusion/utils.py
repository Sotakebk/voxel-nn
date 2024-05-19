"""Various other things."""

import keras

def kernel_init(scale):
    """Custom kernel init."""
    scale = max(scale, 1e-10)
    return keras.initializers.VarianceScaling(scale, mode="fan_avg", distribution="uniform")
