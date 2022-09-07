import sys
from multiprocessing import Process
import socketserver
from .workers import TestWorker, AI4UWorker
import argparse
from inspect import signature
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

class AI4UUDPHandler(socketserver.DatagramRequestHandler):
    worker = AI4UWorker()
    def handle(self):
        # Receive a message from a client
        msgRecvd = self.rfile.read(None)
        content = AI4UUDPHandler.worker.proccess(msgRecvd)
        # Send a message from a client
        self.wfile.write(content.encode(encoding="utf-8"))

if __name__ == "__main__":
    n = len(sys.argv) 
    args = parser.parse_args()
    server_IP = args.host
    server_port = int(args.port)

    if args.worker:
        print("--worker argument is not implemented. Use the argument --agent instead!")
        sys.exit(-1)
    if args.agent:
        try:
            names = args.agent.split('.')
            Agent = importlib.import_module('.'.join(names[0:-1]))
            Agent = getattr(Agent,  names[-1])
            AI4UWorker.agent = Agent()
            dirs = dir(Agent)
            if  "act" in dirs  and  "handleEnvCtrl" in dirs:
                sig = signature(Agent.act)
                sig2 = signature(Agent.handleEnvCtrl)
                if len(sig.parameters) != 2 or len(sig2.parameters) != 2:
                    print('''Error: invalid agent class signature. 
                            Expected methods: act(self, action) and handleEnvCtrl(self, action).''')
                    sys.exit(-1)
            else:
                print('''Error: invalid agent class signature. 
                        Expected methods: act(self, action) and handleEnvCtrl(self, action).''')
                sys.exit(-1)
        except Exception as e:
            print(e)
            print("Invalid agent module. Try <modulename>.<agentclass>!")
            sys.exit(-1)
    serverAddress   = (server_IP, server_port)
    serverUDP = socketserver.UDPServer(serverAddress, AI4UUDPHandler)
    serverUDP.serve_forever()
