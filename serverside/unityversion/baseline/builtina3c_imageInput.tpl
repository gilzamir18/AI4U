def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None):
    import tensorflow as tf
    from ai4u.ml.a3c.multi_scope_train_op import make_train_op 
    from ai4u.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, logit_entropy, make_copy_ops
    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))
    normalized_obs = tf.keras.layers.Lambda(lambda x : x/#NUMOBJ)(observations)
    conv1 = tf.keras.layers.Conv2D(128, (2,2), (1,1), activation='relu', name='conv1')(normalized_obs)
    if debug:
        conv1 = tf.Print(conv1, [observations], message='\ndebug observations:', summarize=2147483647)
    conv2 = tf.keras.layers.Conv2D(128, (2,2), (2,2), activation='relu', name='conv2')(conv1)
    flattened = tf.keras.layers.Flatten()(conv2)
    hidden1 = tf.keras.layers.Dense(512, activation='relu', name='hidden1')(flattened)
    hidden2 = tf.keras.layers.Dense(64, activation='relu', name='hidden2')(hidden1)
    action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden2)
    action_probs = tf.nn.softmax(action_logits)
    values = tf.layers.Dense(1, activation=None, name='value')(hidden2)
    values = values[:, 0]
    layers = [conv1, conv2, hidden1, hidden2]
    return observations, action_logits, action_probs, values, layers

