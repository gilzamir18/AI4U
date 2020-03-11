import re

import tensorflow as tf

"""
Create a training operator which calculates gradients in one scope (the per-worker copy of
parameters) and applies them in another (the shared set of parameters).
"""


def strip_var_name(name):
    """
    e.g. scope/weights:0 -> weights
    """
    return re.match('\w*/([^:]*):\w*', name).group(1)


def make_train_op(compute_scope_loss, optimizer, compute_scope, apply_scope, max_grad_norm=None):
    """
    compute_scope: the scope in which to calculate gradients
    apply_scope: the scope in which to apply the gradients
    """

    # Clip gradients
    compute_tvs = tf.trainable_variables(compute_scope)
    compute_grads = tf.gradients(compute_scope_loss, compute_tvs)
    if max_grad_norm is not None:
        compute_grads, _ = tf.clip_by_global_norm(compute_grads, max_grad_norm)

    # Create a dictionary mapping from variable name to gradients calculated in compute_scope
    compute_scope_grads_dict = {}
    for grad, var in zip(compute_grads, compute_tvs):
        if grad is None:
            continue
        var_name = strip_var_name(var.name)
        compute_scope_grads_dict[var_name] = grad

    grads_norm = tf.global_norm(list(compute_scope_grads_dict.values()))

    # Create a dictionary mapping from variable names to variables in apply_scope
    apply_tvs = tf.trainable_variables(apply_scope)
    apply_tvs_dict = {}
    for var in apply_tvs:
        var_name = strip_var_name(var.name)
        apply_tvs_dict[var_name] = var

    # Create an operator which applies gradients to variables in apply_scope
    grads_and_compute_scope_vars = []
    for var_name, grad in compute_scope_grads_dict.items():
        grads_and_compute_scope_vars.append((grad, apply_tvs_dict[var_name]))
    train_op = optimizer.apply_gradients(grads_and_compute_scope_vars)

    return train_op, grads_norm
