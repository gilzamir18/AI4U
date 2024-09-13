import numpy as np
import base64

import numpy as np
import base64
import sys
from typing import Callable
import os

INVALID_TYPE_ERROR = 1
NAN_DETECTED_ERROR = 2
VALUE_PROPERTY_ERROR = 3

def start_envs_windows(paths, rids, host, port_generator):
    for idx, path in enumerate(paths):
        p = "".join(["start ",  path, " --ai4u_rid ", rids[idx], " --ai4u_port ", str(next(port_generator)), " --ai4u_host ", host])
        print(p)
        os.system(p)

def  rid_generator(_from: int = 0, _n: int = 1, _step:int = 1):
    for i in range(_from, _n, _step):
        yield str(i)

def address_port_generator(server_port, n):
  for i in range(n):
    yield server_port+i

def linear_schedule(initial_value) -> Callable[[float], float]:
    """
    Linear learning rate schedule.
    :param initial_value: (float or str)
    :return: (function)
    """
    if isinstance(initial_value, str):
        initial_value = float(initial_value)

    def func(progress_remaining: float) -> float:
        """
        Progress will decrease from 1 (beginning) to 0
        :param progress_remaining: (float)
        :return: (float)
        """
        return progress_remaining * initial_value
    return func

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

def imageseq_from_str(imgstr, c, w, h, dtype=np.uint8):
    imgs = imgstr.strip().split(':')
    seq = []
    i = 0
    for channel in imgs:
        seq.append(image_from_str(channel, w, h, dtype=np.uint8))
        i += 1
    return seq

def str_as_dictlist(src, sep=";", type=str):
    objlist = src.split(sep)
    result = []
    for obj in objlist:
        if len(obj) > 0:
            result.append(eval(obj))
    return result

def print_error(msg, code=1):
    print(msg)
    sys.exit(code)

def get_from(config, parameter, default_value):
    if parameter in config:
        return config[parameter]
    else:
        return default_value

def get_int_from(config, parameter, default_value=0):
    if parameter in config:
        if type(config[parameter]) is int:
            return config[parameter]
        else:
            sv = config[parameter].strip()
            try:
                return int(float(sv))
            except:
                print_error('Parameter %s with invalid value %s, expected numeric!'%(sv, parameter), INVALID_TYPE_ERROR)
    else:
        return default_value

def get_float_from(config, parameter, default_value=0.0):
    if parameter in config:
        t = type(config[parameter])
        if  t is int or t is float:
            return config[parameter]
        elif t is str:
            return float(config[parameter].strip())
        else:
            print("ERROR: invalid property %s with value %s"%(parameter, config[parameter]))
    else:
        return default_value

def get_float_or_str_from(config, parameter, default_value=0.0):
    if parameter in config:
        if type(config[parameter]) is float:
            return config[parameter]
        else:
            try:
                return float(config[parameter].strip())
            except:
                return config[parameter].strip()
    else:
        return default_value

def get_float_or_func_from(config, parameter, default_value=0.0):
    pvalue = config[parameter].strip()
    if parameter in config:
        if type(config[parameter]) is float:
            return config[parameter]
        else:
            try:
                return float(pvalue)
            except:
                arg = pvalue
                sep = arg.index("_")
                assert sep >= 0, "Error: parameter %s value %s is not supported!"%(parameter, pvalue)
                name_fn = arg[0:sep]
                if name_fn == "lin":
                    arg_fn = float(arg[sep+1:])
                    return linear_schedule(arg_fn)
                else:
                    print("Error: parameter %s value %s is not supported!"%(parameter, pvalue))
    else:
        return default_value

def get_bool_from(config, parameter, default_value=False):
    if parameter in config:
        if type(config[parameter]) is bool:
            return config[parameter]
        else:
            pvalue = config[parameter].strip() 
            assert  pvalue in ['True', 'False'], 'Value error: %s is not a boolean value!'%(pvalue)
            return pvalue=="True"
    else:
        return default_value

def eval_from(config, parameter, default_value):
    if parameter in config:
        return eval(config[parameter])
    else:
        return default_value

def bemakercmd_parser(cmdname, args=None):
    if args is None:
        args = []
    nsize = len(args)
    cmd = []
    for a in args:
        cmd.append(str(a))
    command = "%s;%d;%s"%(cmdname, nsize, ";".join(cmd))
    return command

def step(action, value=None):
    if value is not None:
        if isinstance(value, list) or isinstance(value, np.ndarray):
            return bemakercmd_parser(action, value)
        else:
            return bemakercmd_parser(action, [value])
    else:
        return bemakercmd_parser(action)

def steps(action, value, actions=None):
    cmds = [ step(action, value) ]
    assert actions is None or type(actions) is list or type(actions) is np.ndarray, "Error: invalid action list: {a}".format(a=actions)
    if actions is not None:
        for i in range(len(actions)):
            if (type(actions[i]) is tuple and len(actions[i]) == 2):
                cmds.append( step(actions[i][0], actions[i][1]) )
            else:
                print("Error: the expected field of action is tuple, but {x} was the received value.".format(x=action[i]) )
    #print("@".join(cmds))
    return "@".join(cmds)

def stepfv(action, values):
    return bemakercmd_parser(action, values)

def import_getch():
    try:
        from msvcrt import getch
        return getch
    except ImportError:
        def getch_unix():
            import sys, tty, termios
            old_settings = termios.tcgetattr(0)
            new_settings = old_settings[:]
            new_settings[3] &= ~termios.ICANON
            try:
                termios.tcsetattr(0, termios.TCSANOW, new_settings)
                ch = sys.stdin.read(1)
            finally:
                termios.tcsetattr(0, termios.TCSANOW, old_settings)
            return ch
        return getch_unix
