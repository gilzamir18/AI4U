import sys
import socketserver
from .workers import BMWorker
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
        self.max_packet_size = 819200
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
        self.socket.settimeout(timeout)
        self.handler = handler
        self.handler.server = self

    def listen(self):
        try:
            self.socket.bind((self.host, self.port))
            print("Waiting for your Godot app on {}:{}".format(self.host, self.port))       
            print(f"After {self.timeout} seconds, this terminal will automatically shut down if it does not receive a request!")
            while True:
                mydata, addr = self.socket.recvfrom(self.max_packet_size)
                if not self.handler.handle(self.socket, mydata, addr):
                    self.handle_exit("Halted")
                    break
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
    def __init__(self) -> None:
        self.server = None

    def handle(self, socket, data, client_address):
        # Receive a message from a client
        #print("=============> ", type(data))
        content = BMUDPHandler.worker.proccess(data)
        # Send a message from a client
        if  content == "halt":
            socket.sendto(content.encode(encoding="utf-8"), client_address)
            if self.server is not None:
                self.server.socket.close()
            return False
        elif content is not None:
            #self.wfile.write(content.encode(encoding="utf-8"))
            socket.sendto(content.encode(encoding="utf-8"), client_address)
            return True
        else:
            print("WARNING: returning empty message!")
            #self.wfile.write("".encode(encoding="utf-8"))
            socket.sendto(content.encode(encoding="utf-8"), client_address)
            return True

def create_server(host='127.0.0.1', port=8080, buffer_size=819200, timeout=30, force_exit=True):
    serverUDP = UDPServer(host, port, timeout, force_exit, BMUDPHandler())
    serverUDP.max_packet_size = buffer_size
    serverUDP.listen()

if __name__ == "__main__":
    print('''
    ai4u2godot it is not an application.
    Use ai4u.appserver.startasdaemon function
    to initialize an controller to control
    an Godot application.
    ''')
