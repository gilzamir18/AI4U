import numpy as np
import base64

def image_decode(frame, w, h, dtype=np.uint8):
    imgdata = base64.b64decode(frame).decode('UTF-8')
    return image_from_str(imgdata, w, h, dtype)

def image_decode_asarray(frame, w, h, dtype=np.uint8):
    imgstr = base64.b64decode(frame).decode('UTF-8')
    lines = imgstr.strip().split(';')
    result = np.zeros(w*h, dtype=dtype)
    i = 0
    for line in lines:
        values = line.strip().split(',')
        for value in values:
            result[i] = int(value)
            i += 1
    return result

def image_from_str(imgstr, w, h, dtype=np.uint8):
    lines = imgstr.strip().split(';')
    result = np.zeros(shape=(w, h), dtype=dtype)
    i = 0
    for line in lines:
        values = line.strip().split(',')
        j = 0
        for value in values:
            if len(value) > 0:
                if dtype == np.uint8 or dtype == np.uint32 or dtype==np.int32:
                    result[i,j] = int(value)
                elif dtype == np.float32:
                    result[i, j] = float(value)
                else:
                    assert False, "DTYPE informed to function image_from_str is not supported"
                j += 1
        i += 1
    return result

def str_as_dictlist(src, sep=";", type=str):
    objlist = src.split(sep)
    result = []
    for obj in objlist:
        if len(obj) > 0:
            result.append(eval(obj))
    return result

def make_inference_network(obs_shape, n_actions, debug=False, extra_inputs_shape=None, network=None):
    import tensorflow as tf
    from ai4u.ml.a3c.multi_scope_train_op import make_train_op 
    from ai4u.ml.a3c.utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
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
                                        'action_meaning':[], 'make_inference_network': make_inference_network}

def ai4ucmd_parser(cmdname, args=[]):
    nsize = len(args)
    cmd = ""
    for a in args:
        cmd += str(a) + ";"
    command = "%s;%d;%s"%(cmdname, nsize, cmd)
    return command

def step(action, value=None):
    if value is not None:
        return format_ai4ucmd(action, [value])
    else:
        return ai4ucmd_parser(action)

def stepfv(action, values):
    strvalues = None
    if not isinstance(values, list):
        values = list(values)
    strvalues = str(values)
    strvalues = strvalues.replace(' ', '').replace(',', ' ').replace('[','').replace(']', '')
    return ai4ucmd_parser(action, [strvalues])