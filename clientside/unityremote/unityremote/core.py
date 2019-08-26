# -*- coding: utf-8 -*-
import socket
from threading import Thread
import time
import sys
import cv2
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
            e = sys.exc_info()[1]
            netcon.open(waiting)
            if waiting > 0:
                time.sleep(waiting)
            fields = None
    return fields
    
def get_image(frame, width=84, height=84):
    imgdata = base64.b64decode(frame)
    inputdata = np.asarray(bytearray(imgdata), dtype=np.uint8)
    img = cv2.imdecode(inputdata, cv2.IMREAD_COLOR)
    img = cv2.resize(img, (width, height), interpolation = cv2.INTER_AREA)
    return cv2.cvtColor(img, cv2.COLOR_BGR2GRAY)

class RemoteEnv:
    def __init__(self, host="127.0.0.1", OUT_PORT=8081, IN_PORT=8080, bs=100000):
        self.OUTPUT_PORT = OUT_PORT
        self.INPUT_PORT = IN_PORT
        self.HOST = host
        self.BUFFER_SIZE = bs
        self.UDP = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        

    def close(self):
        self.UDP.close()

    def open(self, timeout=None):
        self.timeout = timeout
        return self.open_receive()


    def open_receive(self):
        try:
            server_address = (self.HOST, self.INPUT_PORT)
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
                else:
                    fields[desc] = str(value, 'utf-8').strip()
                count += 1
                pos += valuesize
            return fields
        except:
            e = sys.exc_info()[1]
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
        return __get_state__(self, False)

    def step(self, action, value=None):
        if value is not None:
            return self.send(action, [value])
        else:
            return self.send(action)

