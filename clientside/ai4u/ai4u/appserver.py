from .agents import BasicAgent
from .utils import stepfv
from threading import Thread
from .ai4u2unity import create_server
from .workers import AI4UWorker
import time

def startasdaemon(ids, controllers_classes=None, waitfor=0.1):
    agents = [BasicAgent] * len(ids) 
    t = Thread(target=lambda:create_server(agents, ids))
    t.daemon = True
    t.start()
    time.sleep(waitfor)
    print("starting ai4u2unity...", end='\r')
    while (AI4UWorker.count_agents()==0):
        time.sleep(waitfor)
    agents = AI4UWorker.get_agents()
    for i in range(len(ids)):
        agents[i].controller = controllers_classes[i]()
        agents[i].controller.agent = agents[i]
    print("ai4u2unity started...")
    return [agent.controller for agent in agents]

def reset(id):
    return AI4UWorker.get_agent(id).controller.reset()
