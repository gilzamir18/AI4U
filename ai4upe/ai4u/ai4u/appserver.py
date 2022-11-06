from .agents import BasicAgent
from .utils import stepfv
from threading import Thread
from .ai4u2unity import create_server
from .workers import AI4UWorker
import time

def startasdaemon(ids, controllers_classes=None, server_IP="127.0.0.1", server_port=8080, waitfor=0.1):
    agents = [BasicAgent] * len(ids) 
    t = Thread(target=lambda:create_server(agents, ids, server_IP, server_port))
    t.daemon = True
    t.start()
    time.sleep(waitfor)
    print("starting ai4u2unity...", end='\r')
    while (AI4UWorker.count_agents() < len(ids)):
        time.sleep(waitfor)
    agents = AI4UWorker.get_agents()
    for i in range(len(ids)):
        agents[i].controller = controllers_classes[i]()
        agents[i].controller.agent = agents[i]
    print("ai4u2unity started...")
    return [agent.controller for agent in agents]

