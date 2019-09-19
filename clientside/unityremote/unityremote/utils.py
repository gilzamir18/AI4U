import cv2
import numpy as np

def image_decode(frame):
    imgdata = base64.b64decode(frame)
    return np.asarray(bytearray(imgdata), dtype=np.uint8)

def get_image(frame, width=84, height=84):
    imgdata = base64.b64decode(frame)
    inputdata = np.asarray(bytearray(imgdata), dtype=np.uint8)
    img = cv2.imdecode(inputdata, cv2.IMREAD_COLOR)
    img = cv2.resize(img, (width, height), interpolation = cv2.INTER_AREA)
    return cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

def make_inference_network(obs_shape, n_actions, debug=False):
    import tensorflow as tf
    from unityremote.ml.a3c.multi_scope_train_op import make_train_op 
    from unityremote.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
        logit_entropy, make_copy_ops

    observations = tf.placeholder(tf.float32, [None] + list(obs_shape))

    hidden1 = tf.layers.dense(observations, 100, activation=tf.nn.relu, name='hidden1')

    hidden2 = tf.layers.dense(hidden1, 100, activation=tf.nn.relu, name='hidden2')

    action_logits = tf.layers.dense(hidden2, n_actions, activation=None, name='action_logits')
    action_probs = tf.nn.softmax(action_logits)

    values = tf.layers.dense(hidden2, 1, activation=None, name='value')
    # Shape is currently (?, 1)
    # Convert to just (?)
    values = values[:, 0]
    layers = [hidden1, hidden2]
    return observations, action_logits, action_probs, values, layers


environment_definitions = {'host': '127.0.0.1', 'input_port': 8080, 'output_port': 7070, 'n_envs': 1,
                                "action_shape": (1, ), "state_shape": (1, ), 'min_value': -100.0, 
                                    'max_value': 100.0, 'state_type': np.float32,  'actions': [], 
                                        'action_meaning':[], 'state_wrapper': lambda s: s,
                                        'make_inference_network': make_inference_network}
