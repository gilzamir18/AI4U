import sys
from multiprocessing import Process
import socketserver
from .workers import TestWorker, AI4UWorker
import argparse
import importlib

parser = argparse.ArgumentParser()
parser.add_argument('--host',
                    default='127.0.0.1',
                    dest='host',
                    help='Host IP address.'
                    )
parser.add_argument('--port',
                    default='8080',
                    help='port address'
                    )

parser.add_argument('--worker',
                    default='',
                    help='user worker.'
                    )

parser.add_argument('--agent',
                    default='ai4u.agents.BasicAgent',
                    help='user worker.'
                    )
                
parser.add_argument('--agent_id',
                    default='0',
                    help='user worker.'
                    )
                

class AI4UUDPHandler(socketserver.DatagramRequestHandler):
    worker = AI4UWorker()
    def handle(self):
        # Receive a message from a client
        msgRecvd = self.rfile.read(None)
        content = AI4UUDPHandler.worker.proccess(msgRecvd)
        # Send a message from a client
        if  content is not None:
            self.wfile.write(content.encode(encoding="utf-8"))
        else:
            print("WARNING: returning empty message!")
            self.wfile.write("".encode(encoding="utf-8"))

def create_server(agents, ids, server_IP="127.0.0.1", server_port=8080):
    for i in range(len(agents)):
        if not AI4UWorker.register_agent(agents[i], ids[i]):
            sys.exit(-1)
    serverAddress   = (server_IP, server_port)
    serverUDP = socketserver.UDPServer(serverAddress, AI4UUDPHandler)
    serverUDP.serve_forever()

if __name__ == "__main__":
    print('''
    AI4U2Unity it is not an application.
    Use ai4u.appserver.startasdaemon function
    to initialize an controller to control
    an unity application.
    ''')
