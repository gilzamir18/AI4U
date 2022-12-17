import numpy as np
from inspect import signature
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
    _agents = {}
    _n_agents = 0 

    def register_agent(Agent_Class, id = 0):
        dirs = dir(Agent_Class)
        agent = Agent_Class()
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
        AI4UWorker._agents[agent.id] = agent
        AI4UWorker._n_agents = AI4UWorker._n_agents + 1
        return True

    def get_agent(id):
        return AI4UWorker._agents[id]

    def has(id):
        return id in AI4UWorker._agents

    def get_agents():
        return [a[1] for a in AI4UWorker._agents.items()]
    
    def count_agents():
        return AI4UWorker._n_agents

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
        assert 'id' in info, 'Error:  id not found in message received from client. %s'%(info)
        id = info['id']
        if AI4UWorker.has(id):
            agent = AI4UWorker.get_agent(id)
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
            print("ERROR: invalid agent id ", id)
