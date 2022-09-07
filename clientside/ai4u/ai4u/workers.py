import numpy as np

import sys

class BasicWorker:
    def proccess(self, data):
        return "received!"

class TestWorker:
    def __init__(self):
        self.id = 0

    def proccess(self, id):
        self.id += 1
        return f"id: {self.id}"

class AI4UWorker:
    agent = None

    def __init__(self):
        self.id = 0

    def decode(self, data):
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

    def proccess(self, msg):
        info = self.decode(msg)
        target = None
        if '__target__' in info:
            target = info['__target__']
        if not AI4UWorker.agent:
            print("Error: agent not informed. Set the --agent argument when call ai4u2unity!")
            sys.exit(-1)
        if target is None or target == 'act':
            return AI4UWorker.agent.act(info)
        elif target == 'envcontrol':
            return AI4UWorker.agent.handleEnvCtrl(info)
        else:
            print("ERROR: invalid target! Set a valid __target__ field when you send a message from Unity Editor.")
