"""Contains methods for creating kernels, to use with structurization."""

from typing import Callable
import numpy as np

def inverse_omit_0(x: float):
    """Returns inverse of value, or 0 if 0."""
    return 1/x if x != 0 else 0

def inverse_1_on_0(x: float):
    """Returns inverse of value, or 1 if 1."""
    return 1/x if x != 0 else 1

def create_kernel(size: int, real_dimensions: int, latent_dimensions: int,
                  weight_function: Callable[[float],float] = inverse_omit_0) -> np.ndarray:
    """Returns an n-dimensional kernel with values equal to the value of weight_function
    of the distance between the cell and the center cell of the kernel.\n
    Each latent dimension is separated - they do not influence each other."""

    rdim_tuple = (size,)*real_dimensions+(latent_dimensions,)*2
    filter_arr = np.zeros(rdim_tuple)

    def per_real_point(partial_function: Callable[[list],None],
                       dim_left: int = real_dimensions, stack: list = None):
        if stack is None:
            stack = []

        if dim_left > 0:
            for i in range(size):
                stack.append(i)
                per_real_point(partial_function, dim_left-1, stack)
                stack.pop()
        else:
            partial_function(stack)

    def calculate_distances():
        center = np.array([(size-1)/2.0]*real_dimensions)
        def func(stack: list):
            for i in range(latent_dimensions):
                # set element value to distance in 'real' (data) space from the center of the filter
                filter_arr[tuple(stack)][i,i] = weight_function(np.linalg.norm(center - stack))
        per_real_point(func)

    def normalize():
        for i in range(latent_dimensions):
            # grab sum of values on each layer of the filter
            # make it so the sum would be roughly 1
            weight_sum = np.sum(filter_arr[...,i,i])
            filter_arr[...,i,i] = filter_arr[...,i,i]/weight_sum

    calculate_distances()
    normalize()

    return filter_arr
