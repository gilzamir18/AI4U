import sys
import socketserver
from .workers import BMWorker
from .utils import get_int_from, get_bool_from, get_float_from, get_from

class BMUDPHandler(socketserver.DatagramRequestHandler):
    worker = BMWorker()
    def handle(self):
        # Receive a message from a client
        msgRecvd = self.rfile.read(None)
        content = BMUDPHandler.worker.proccess(msgRecvd)
        # Send a message from a client
        if  content is not None:
            self.wfile.write(content.encode(encoding="utf-8"))
        else:
            print("WARNING: returning empty message!")
            self.wfile.write("".encode(encoding="utf-8"))

def create_server(agents, ids, config=None):
    if config is None:
        config = {}
    for i in range(len(agents)):
        if not BMWorker.register_agent(agents[i], ids[i], config):
            sys.exit(-1)

    serverAddress   = (get_from(config, 'server_IP', '127.0.0.1'), get_int_from(config, 'server_port', 8080))
    serverUDP = socketserver.UDPServer(serverAddress, BMUDPHandler)
    serverUDP.max_packet_size = get_int_from(config, 'buffer_size', 8192)
    serverUDP.timeout = get_float_from(config, 'timeout', 20)
    serverUDP.serve_forever()

if __name__ == "__main__":
    print('''
    ai4u2godot it is not an application.
    Use ai4u.appserver.startasdaemon function
    to initialize an controller to control
    an unity application.
    ''')
