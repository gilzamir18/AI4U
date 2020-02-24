import numpy as np
import time
from unityremote.gmproc import ClientServer, ClientWorker, ServerWorker


class MyServer(ServerWorker):
	def __init__(self):
		super().__init__()
		self.name = "myserver"

	def start(self, params):
		print('starting server %s'%(self.name))

	def process(self, id, msg):
		print("Msg from client %s is %d"%(id, msg))
		return msg

	def wait(self):
		time.sleep(0.1)

class Client(ClientWorker):
	def __init__(self, name):
		super().__init__()
		self.name = name
		self.counter = 0

	def start(self, params):
		self.state, self.method = params
		print("starting client %s with state %d"%(self.name, self.state))
		
	def process(self):
		self.counter = self.counter + 1
		return self.method(self.state)
		
	def wait(self):
		print("Client %s waiting"%(self.name))

	def update(self, newstate):
		self.state = newstate

	def finish(self):
		print("Client %s is finishing..."%(self.name))

	def done(self):
		return self.counter > 10


def add2(n):
	return n + 2

if __name__=="__main__":
	cs = ClientServer(MyServer)
	cs.new_worker('par', Client, params=(2, add2))
	cs.new_worker('impar', Client, params=(1, add2))
	cs.run()

