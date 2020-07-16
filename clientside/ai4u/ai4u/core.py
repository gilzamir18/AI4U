# -*- coding: utf-8 -*-
import socket
import threading
from threading import Thread
import time
import sys
import base64
import numpy as np
import time

def __get_state__(netcon, get_state = True, waiting=0, verbose=True):
    fields = None
    while fields == None:
        try:
            if get_state:
                netcon.send("GetState")
            fields = netcon.receive_data()
        except:
            if verbose:
                print("waiting for connection...\r", end="")
                #e = sys.exc_info()[1]
                #print(e)
            netcon.open(waiting)
            if waiting > 0:
                time.sleep(waiting)
            fields = None
    return fields

class IteractiveEnvironmentManager():
    def __init__(self, host="127.0.0.1", start_input_port=8080, step_input_port=1, start_output_port=7070, step_output_port=1, wrapper=None):
        self.environments=[]
        self.host = host
        self.input_port = start_input_port
        self.input_step = step_input_port
        self.output_port = start_output_port
        self.output_step = step_output_port
        self.wrapper = wrapper
        self.size = 0
        self.lock = threading.Lock()

    def make(self):
        self.lock.acquire()
        if self.wrapper is RemoteEnv:
            env = wrapper()
            env.configure(self.host, self.output_port, self.input_port)
            env.make()
            self.environments.append(env)
        else:
            self.environments.append(RemoteEnv(self.host, self.output_port, self.input_port))
        
        self.output_port += self.output_step
        self.input_port += self.input_step
        self.size += 1
        p = self.size-1
        self.lock.release()
        return (p, self.environments[p])

    def open(self, i, timeout):
        self.environments[i].open(timeout)

    def open_receive(self, i):
        self.environments[i].open_receive()
    
    def send(self, i, cmdname, args=[]):
        return self.environments[i].send(cmdname, args)

    def step(self, i, action, value=None):
        return self.environments[i].step(action, value)
        
    def openAll(self, timeout=0.01):
        for i in range(self.size):
            self.open(i, timeout)

    def claseAll(self):
        for i in range(self.size):
            self[i].close()
 
    def __getitem__(self, key):
        return self.environments[key]
    
    def __len__(self):
        return self.size

class EnvironmentManager():
    def __init__(self, size=1, host="127.0.0.1", start_input_port=8080, step_input_port=1, start_output_port=7070, step_output_port=1, wrapper=None):
        self.environments=[]
        self.size = size
        input_port = start_input_port
        input_step = step_input_port
        output_port = start_output_port
        output_step = step_output_port
        for i in range(size):
            if not (wrapper is None):
                env = wrapper()
                env.configure(host, output_port, input_port)
                env.make()
                self.environments.append(env)
            else:
                self.environments.append(RemoteEnv(host, output_port, input_port))
            output_port += output_step
            input_port += input_step

    def open(self, i, timeout):
        self.environments[i].open(timeout)

    def open_receive(self, i):
        self.environments[i].open_receive()
    
    def send(self, i, cmdname, args=[]):
        return self.environments[i].send(cmdname, args)

    def step(self, i, action, value=None):
        return self.environments[i].step(action, value)
        
    def openAll(self, timeout=0.01):
        for i in range(self.size):
            self.open(i, timeout)

    def claseAll(self):
        for i in range(self.size):
            self[i].close()
 
    def __getitem__(self, key):
        return self.environments[key]
    
    def __len__(self):
        return self.size

class RemoteEnv:
    def __init__(self):
        self.OUTPUT_PORT = 8081
        self.INPUT_PORT = 8080
        self.HOST = "127.0.0.1"
        self.BUFFER_SIZE = 100000
        self.verbose = True

    def configure(self, host="127.0.0.1", OUT_PORT=8081, IN_PORT=8080, bs=100000):
        self.OUTPUT_PORT = OUT_PORT
        self.INPUT_PORT = IN_PORT
        self.HOST = host
        self.BUFFER_SIZE = bs
        self.verbose = True

    def make(self):
        self.UDP = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    def __init__(self, host="127.0.0.1", OUT_PORT=8081, IN_PORT=8080, bs=100000):
        self.OUTPUT_PORT = OUT_PORT
        self.INPUT_PORT = IN_PORT
        self.HOST = host
        self.BUFFER_SIZE = bs
        self.UDP = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.verbose = True

    def close(self):
        self.UDP.close()

    def reset(self):
        pass

    def open(self, timeout=0):
        self.timeout = timeout
        return self.open_receive()

    def open_receive(self):
        try:
            server_address = ('', self.INPUT_PORT)
            #print('starting up on %s port %s' % server_address)
            self.UDP.settimeout(self.timeout)
            self.UDP.bind(server_address)
            return True
        except:
            return False

    def receive_data(self):
        # Bind the socket to the port
        return self.recvall()

    def recvall(self):
        data = bytearray(self.BUFFER_SIZE)
        try:
            data, _ = self.UDP.recvfrom(self.BUFFER_SIZE)
            #self.UDP.setsockopt(socket.SOL_SOCKET, socket.SO_RCVBUF, 0)
            numberoffields = int(str(data[0:4], 'utf-8').strip())
            pos = 4
            fields = {}
            count = 0
            while True:
                if count >= numberoffields:
                    break
                descsize = int(str(data[pos:pos+4], 'utf-8').strip())
                pos += 4
                desc = str(data[pos:pos+descsize], 'utf-8').strip()
                pos += descsize
                datatype = int(str(data[pos:pos+1], 'utf-8').strip())
                pos += 1
                valuesize = int(str(data[pos:pos+8], 'utf-8').strip())
                pos += 8
                value = data[pos:pos+valuesize]

                if datatype == 0:
                    fields[desc] = float(str(value, 'utf-8').strip())
                elif datatype == 1:
                    fields[desc] = int(str(value, 'utf-8').strip())
                elif datatype == 2:
                    fields[desc] = True if int(str(value, 'utf-8').strip()) == 1 else False
                elif datatype == 3:
                    fields[desc] = str(value, 'utf-8').strip()
                elif datatype == 4:
                    fields[desc] = str(value, 'utf-8').strip()
                elif datatype == 5:
                    v = str(value, 'utf-8').strip()
                    vs = v.split(' ')
                    a = np.zeros(len(vs))
                    for idx, e in enumerate(vs):
                        a[idx] = e.replace(",", ".")
                    fields[desc] = a
                count += 1
                pos += valuesize
            return fields
        except:
            raise
            return None

    def send(self, cmdname, args=[]):
        dest = (self.HOST, self.OUTPUT_PORT)
        nsize = len(args)
        cmd = ""
        for a in args:
            cmd += str(a) + ";"
        command = "%s;%d;%s"%(cmdname, nsize, cmd)
        self.UDP.sendto(command.encode(encoding="utf-8"), dest)
        #self.UDP.setsockopt(socket.SOL_SOCKET, socket.SO_RCVBUF, 8192)
        return __get_state__(self, False, verbose=self.verbose)

    def step(self, action, value=None):
        if value is not None:
            return self.send(action, [value])
        else:
            return self.send(action)

    def stepfv(self, action, values):
        strvalues = None
        if not isinstance(values, list):
            values = list(values)
        strvalues = str(values)
        strvalues = strvalues.replace(' ', '').replace(',', ' ').replace('[','').replace(']', '')
        return self.send(action, [strvalues])
