import numpy as np
from inspect import signature
import sys
from queue import Queue
from .utils import get_float_from, get_int_from
from .types import *

class BasicWorker:
    def proccess(self, data):
        return "received!"

class TestWorker:
    def __init__(self):
        self.id = 0

    def proccess(self, id):
        self.id += 1
        return f"id: {self.id}"

class BMWorker:
    _agents = {}
    _n_agents = 0 

    def register_agent(Agent_Class, id = 0, config=None):
        if config is None:
            config = {}
        dirs = dir(Agent_Class)
        action_buffersize = get_int_from(config, 'action_buffersize', 0)
        agent = Agent_Class(Queue(action_buffersize), Queue(action_buffersize), get_float_from(config, 'waittime', 0), get_float_from(config, 'agent_timeout', 20))
        agent.id = id
        if  "act" in dirs  and  "handleEnvCtrl" in dirs:
            sig = signature(Agent_Class.act)
            sig2 = signature(Agent_Class.handleEnvCtrl)
            if len(sig.parameters) != 2 or len(sig2.parameters) != 2:
                print('''Error: invalid agent class signature. 
                        Expected methods: act(self, action) and handleEnvCtrl(self, action).''')
                return False
        else:
            print('''Error: invalid agent class signature. 
                    Expected methods: act(self, action) and handleEnvCtrl(self, action).''')
            return False
        BMWorker._agents[agent.id] = agent
        BMWorker._n_agents = BMWorker._n_agents + 1
        return True

    def get_agent(id):
        return BMWorker._agents[id]

    def has(id):
        return id in BMWorker._agents

    def get_agents():
        return [a[1] for a in BMWorker._agents.items()]
    
    def count_agents():
        return BMWorker._n_agents

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

            if datatype == SENSOR_SFLOAT:
                fields[desc] = float(str(value, 'utf-8').strip())
            elif datatype == SENSOR_SSTRING:
                fields[desc] = int(str(value, 'utf-8').strip())
            elif datatype == SENSOR_SBOOL:
                fields[desc] = True if int(str(value, 'utf-8').strip()) == 1 else False
            elif datatype == SENSOR_SINT:
                fields[desc] = str(value, 'utf-8').strip()
            elif datatype == SENSOR_SBYTEARRAY:
                fields[desc] = str(value, 'utf-8').strip()
            elif datatype == SENSOR_SFLOATARRAY:
                v = str(value, 'utf-8').strip()
                vs = v.split(' ')
                a = np.zeros(len(vs))
                for idx, e in enumerate(vs):
                    a[idx] = float(e.replace(",", "."))
                fields[desc] = a
            elif datatype == SENSOR_SINTARRAY:
                v = str(value, 'utf-8').strip()
                vs = v.split(' ')
                a = np.zeros(len(vs), dtype=np.int32)
                for idx, e in enumerate(vs):
                    a[idx] = int(e)
                fields[desc] = a
            elif datatype == SENSOR_SSTRINGS:
                v = str(value, 'utf-8').strip()
                vs = v.split(' ')
                a = [] 
                for e in vs:
                    a.append(e)
                fields[desc] = a
            count += 1
            pos += valuesize
        if not 'truncated' in fields:
            fields['truncated'] = False
        if fields['truncated']:
            fields['TimeLimit.truncated'] = True
        else:
            fields['TimeLimit.truncated'] = False
        return fields

    def proccess(self, msg):
        info = self.decode(msg)
        assert 'id' in info, 'Error:  id not found in message received from client. %s'%(info)
        id = info['id']
        if BMWorker.has(id):
            agent = BMWorker.get_agent(id)
            target = None
            if '__target__' in info:
                target = info['__target__']
            if not agent:
                print("Error: agent not informed. Set the --agent argument when call ai4u2unity!")
                sys.exit(-1)
            if target is None or target == 'act':
                return agent.act(info)
            elif target == 'envcontrol':
                return agent.handleEnvCtrl(info)
            else:
                print("ERROR: invalid target! Set a valid __target__ field when you send a message from Unity Editor.")
        else:
            print("ERROR: no registered agent with id ", id)
