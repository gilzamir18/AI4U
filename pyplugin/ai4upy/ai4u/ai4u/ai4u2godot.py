import sys
import socketserver
from .workers import BMWorker
from .utils import get_int_from, get_bool_from, get_float_from, get_from
import socket
import platform
import os
import signal
import traceback


class UDPServer:
    def __init__(self, host, port, timeout, force_exit, handler):
        self.host = host
        self.port = port
        self.timeout = timeout
        self.force_exit = force_exit
        self.max_packet_size = 8192
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.socket.settimeout(timeout)
        self.handler = handler

    def listen(self):
        try:
            self.socket.bind((self.host, self.port))
            print("Waiting for your Godot app on {}:{}".format(self.host, self.port))       
            print(f"After {self.timeout} seconds, this terminal will automatically shut down if it does not receive a request!")
            while True:
                mydata, addr = self.socket.recvfrom(self.max_packet_size)
                self.handler.handle(self.socket, mydata, addr)
                #print("Received message from {}: {}".format(addr, data.decode()))
        except socket.timeout:
            # Aqui você pode chamar a função desejada após o tempo limite
            self.handle_exit("Timeout occurred, no message received within {} seconds".format(self.timeout))
        except KeyboardInterrupt:
            self.handle_exit("Exiting...")
        except Exception as e:
            self.handle_exit(f"Unexpected error: \n {print(traceback.format_exc())}")
            
    def handle_exit(self, msg):
        print(msg)
        if self.force_exit:
            if "WINDOWS" == platform.system().upper():
                os._exit(0)
            else:         
                os.kill(os.getpid(), signal.SIGINT)
        else:
            sys.exit()

class BMUDPHandler:
    worker = BMWorker()
    def handle(self, socket, data, client_address):
        # Receive a message from a client
        #print("=============> ", type(data))
        content = BMUDPHandler.worker.proccess(data)
        # Send a message from a client
        if  content is not None:
            #self.wfile.write(content.encode(encoding="utf-8"))
            socket.sendto(content.encode(encoding="utf-8"), client_address)
        else:
            print("WARNING: returning empty message!")
            #self.wfile.write("".encode(encoding="utf-8"))
            socket.sendto(content.encode(encoding="utf-8"), client_address)

def create_server(agents, ids, config=None):
    if config is None:
        config = {}
    for i in range(len(agents)):
        if not BMWorker.register_agent(agents[i], ids[i], config):
            sys.exit(-1)

    serverUDP = UDPServer(get_from(config, 'server_IP', '127.0.0.1'), get_int_from(config, 'server_port', 8080), get_float_from(config, 'timeout', 30), get_bool_from(config, "force_exit", True), BMUDPHandler())
    serverUDP.max_packet_size = get_int_from(config, 'buffer_size', 8192)
    serverUDP.listen()

if __name__ == "__main__":
    print('''
    ai4u2godot it is not an application.
    Use ai4u.appserver.startasdaemon function
    to initialize an controller to control
    an Godot application.
    ''')
