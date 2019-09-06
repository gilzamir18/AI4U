import random

import numpy as np
import tensorflow as tf


def set_random_seeds(seed):
    tf.set_random_seed(seed)
    np.random.seed(seed)
    random.seed(seed)


def make_copy_ops(from_scope, to_scope):
    """
    Create operations to mirror the values from all trainable variables in from_scope to to_scope.
    """
    from_tvs = tf.get_collection(tf.GraphKeys.TRAINABLE_VARIABLES, scope=from_scope)
    to_tvs = tf.get_collection(tf.GraphKeys.TRAINABLE_VARIABLES, scope=to_scope)

    from_dict = {var.name: var for var in from_tvs}
    to_dict = {var.name: var for var in to_tvs}
    copy_ops = []
    for to_name, to_var in to_dict.items():
        from_name = to_name.replace(to_scope, from_scope)
        from_var = from_dict[from_name]
        op = to_var.assign(from_var.value())
        copy_ops.append(op)

    return copy_ops


def make_grad_histograms(variables, grads):
    summaries = []
    for v, g in zip(variables, grads):
        if g is None:
            continue
        # strip "worker_0/"
        v_name = '/'.join(v.name.split('/')[1:])
        summary_name = "grads/{}".format(v_name)
        histogram = tf.summary.histogram(summary_name, g)
        summaries.append(histogram)
    return summaries


def make_histograms(tensors, name):
    summaries = []
    for tensor in tensors:
        # strip "worker_0/"; extract a nice name
        tensor_name = tensor.name.split('/')
        if name == 'activations':
            tensor_name = [tensor_name[1]]
        elif name == 'rms':
            tensor_name = tensor_name[1:3]
        else:
            tensor_name = tensor_name[1:]
        tensor_name = '/'.join(tensor_name)

        summary_name = "{}/{}".format(name, tensor_name)
        histogram = tf.summary.histogram(summary_name, tensor)
        summaries.append(histogram)
    return summaries


def make_rmsprop_histograms(rmsprop_optimizer):
    rms_vars = [rmsprop_optimizer.get_slot(var, 'rms')
                for var in tf.trainable_variables()]
    rms_vars = [v for v in rms_vars if v is not None]
    summaries = make_histograms(rms_vars, 'rms')
    return summaries


def make_lr(lr_args, step_counter):
    initial_lr = tf.constant(lr_args['initial'])
    schedule = lr_args['schedule']
    if schedule == 'constant':
        lr = initial_lr
    elif schedule == 'linear':
        assert type(step_counter) == tf.Variable
        steps = tf.cast(step_counter, tf.float32)
        zero_by_steps = tf.cast(lr_args['zero_by_steps'], tf.float32)
        lr = initial_lr * (1 - steps / zero_by_steps)
        lr = tf.clip_by_value(lr, clip_value_min=0.0, clip_value_max=float('inf'))
    else:
        raise ValueError("Invalid learning rate schedule '{}'".format(schedule))
    return lr


def make_optimizer(learning_rate):
    # From the paper, Section 4, Asynchronous RL Framework, subsection Optimization:
    #   "We investigated three different optimization algorithms in our asynchronous framework –
    #    SGD with momentum, RMSProp without shared statistics, and RMSProp with shared statistics.
    #    We used the standard non-centered RMSProp update..."
    #   "A comparison on a subset of Atari 2600 games showed that a variant of RMSProp where
    #    statistics g are shared across threads is considerably more robust than the other two
    #    methods."
    #
    # TensorFlow's RMSPropOptimizer defaults to centered=False, so we're good there.
    #
    # For shared statistics, we supply the same optimizer instance to all workers. Couldn't they
    # still end up using different statistics somehow?
    # a) From the source, RMSPropOptimizer's gradient statistics variables are associated with the
    #    variables supplied to apply_gradients(), which happen to be the global set of variables
    #    shared between all threads variables (see multi_scope_train_op.py).
    # b) Empirically, no. See shared_statistics_test.py.
    #
    # In terms of hyperparameters:
    #
    # Learning rate: the paper actually runs a bunch of different learning rates and presents
    # results averaged over the three best learning rates for each game. Empirically, 1e-4 seems to
    # work OK (set in params.py).
    #
    # RMSprop hyperparameters: Section 8, Experimental Setup, says:
    #   "All experiments used...RMSProp decay factor of α = 0.99."
    # There's no mention of the epsilon used. I see that OpenAI Baselines' implementation of A2C
    # uses 1e-5 (https://git.io/vpCQt), instead of TensorFlow's default of 1e-10.
    # Remember, RMSprop divides gradients by a factor based on recent gradient history.
    # Epsilon is added to that factor to prevent a division by zero.
    # If epsilon is too small, we'll get a very large update when the gradient history is close to
    # zero. So my speculation about why Baselines uses a much larger epsilon is: sometimes in RL
    # the gradients can end up being very small, and we want to limit the size of the update...?

    optimizer = tf.train.RMSPropOptimizer(learning_rate=learning_rate, decay=0.99, epsilon=1e-5)
    return optimizer


def logit_entropy(logits):
    """
    Numerically-stable entropy directly from logits.

    We want to calculate p = exp(logits) / sum(exp(logits)), then do -sum(p * log(p)).

    There are two things we need to be careful of:
    - If one of the logits is large, exp(logits) will overflow.
    - If one of the probabilities is zero, we'll accidentally do log(0).
      (Entropy /is/ still well-defined if one of the probabilities is zero.
       We just miss out that probability from the sum.)

    The first problem is just a matter of using a numerically-stable softmax.

    For the second problem, if we have access to the logits, there's a trick we can use.
    Note that if we're calculating probabilities from logits, none of the probabilities should ever
    be zero. If we do up with a zero probability, it's only because of rounding. To get around this,
    when computing log(p), we don't compute probabilities explicitly, but instead compute the result
    directly in terms of the logits. For example:
      logits = [0, 1000]
      log(probs) = log(exp(logits)/sum(exp(logits)))
                 = log(exp(logits)) - log(sum(exp(logits)))
                 = logits - log_sum(exp(logits))
                 = [0, 1000] - log(exp(0) + exp(1000))
                 = [0, 1000] - log(1 + exp(1000))
                 = [0, 1000] - log(exp(1000))  (approximately)
                 = [0, 1000] - 1000
                 = [-1000, 0]
    """
    # We support either:
    # - 1D list of logits
    # - A 2D list, batch size x logits
    assert len(logits.shape) <= 2
    # keepdims=True is necessary so that we get a result
    # which is (batch size, 1) instead of just (batch size,)
    logp = logits - tf.reduce_logsumexp(logits, axis=-1, keepdims=True)
    nlogp = -logp
    probs = tf.nn.softmax(logits, axis=-1)
    nplogp = probs * nlogp
    # This reduce_sum is just the final part of the entropy calculation.
    # Don't worry - we do return the entropy for each item in the batch.
    return tf.reduce_sum(nplogp, axis=-1, keepdims=True)
