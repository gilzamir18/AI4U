from .agents import BasicAgent
from .utils import stepfv
from threading import Thread
from .ai4u2godot import create_server
from .workers import BMWorker
import time
from .utils import get_int_from, get_bool_from, get_float_from, get_from
import sys

def startasdaemon(ids, controllers_classes=None, config=None):
    if config is None:
        config = {}
    agents = [BasicAgent] * len(ids)
    for i in range(len(agents)):
        if not BMWorker.register_agent(agents[i], ids[i], config):
            sys.exit(-1)
        force_exit = get_bool_from(config, "force_exit", True)
        port = get_int_from(config, "server_port", 8080)
        host = get_from(config, "server_IP", "127.0.0.1")
        timeout = get_int_from(config, "timeout", 30)
        if timeout <= 0:
            timeout = None
        bf = get_int_from(config, "buffer_size", 189200)
        if 'port_generator' in config:
            port = next(config['port_generator'])
        t = Thread(target=lambda:create_server(host, port, bf, timeout, force_exit))
        t.daemon = True
        t.start()
    
    waittime = get_float_from(config, 'waittime_on_startup', 0.0)
    time.sleep( waittime )
    print("starting ai4u2godot...", end='\r')
    while (BMWorker.count_agents() < len(ids)):
        time.sleep( waittime )
    agents = BMWorker.get_agents()
    for i in range(len(ids)):
        agents[i].controller = controllers_classes[i]()
        agents[i].controller.agent = agents[i]
    print("ai4u2godot started...")
    return [agent.controller for agent in agents]

