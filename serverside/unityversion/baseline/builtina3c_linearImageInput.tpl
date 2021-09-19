def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None, network=None):
    import tensorflow as tf
    from ai4u.ml.a3c.multi_scope_train_op import make_train_op 
    from ai4u.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, logit_entropy, make_copy_ops
    observations = tf.placeholder(tf.float32, [None] + list(obs_shape) )
    linearInput = tf.placeholder(tf.float32, (None, extra_inputs_shape[0]) )
    normalized_LinearInput = tf.keras.layers.Lambda(lambda x : x/#LINEARINPUTNORM)(linearInput)
    normalized_obs = tf.keras.layers.Lambda(lambda x : x/#NUMOBJ)(observations)
    conv1 = tf.keras.layers.Conv2D(128, (1,1), (1,1), activation='relu', name='conv1')(normalized_obs)
    if debug:
        conv1 = tf.Print(conv1, [observations], message='\ndebug observations:', summarize=2147483647)
    liDense1 = tf.keras.layers.Dense(30, activation='tanh', name='phidden')(normalized_LinearInput[:, 0:extra_inputs_shape[0]])
    liDense2 = tf.keras.layers.Dense(30, activation='tanh', name='phidden')(liDense1)
    flattened = tf.keras.layers.Flatten()(conv1)
    exp_features = tf.keras.layers.Concatenate()([flattened, liDense2])
    hidden1 = tf.keras.layers.Dense(256, activation='tanh', name='hidden1')(exp_features)
    hidden2 = tf.keras.layers.Dense(64, activation='tanh', name='hidden2')(hidden1)
    action_logits = tf.keras.layers.Dense(n_actions, activation=None, name='action_logits')(hidden2)
    action_probs = tf.nn.softmax(action_logits)
    values = tf.keras.layers.Dense(1, activation=None, name='value')(hidden2)
    values = values[:, 0]
    layers = [conv1, liDense1, liDense2, exp_features, hidden1, hidden2]
    return (observations, linearInput), action_logits, action_probs, values, layers

