# GMPROC - Python Based Multiprocessing.


GIL (Global Interpreter Lock) limits the power of the multiprocessing based threads. But thread abstraction is easy to understand compared to multiprocessing abstraction. Thus, We have got a software abstraction to facilitate multiprocessing implementation. We named  GMPROC  (Gil Multi-Processing) this abstraction implementation.

Therefore, GPROC is an abstraction to make ease python based multiprocessing. GMPROC combines with UnityRemote on the client-side. But you can use it in other applications.

## USER GUIDE

GMPROC provides two main abstractions to use Python based multiprocessing: workers and client-server.

### Workers

Workers run as separate processes that return a value when the process end. Create workers at one line:

```
workers = Workers()
workers.add(ID, method, params)
 ...
workers.run(ID_LIST)
```

where *ID* is an integer that identifies the worker instance; *method* is a function that runs when workers run, and *params* are parameters passed forward to worker instance; workers run when the *run* method is called; *ID_LIST* is a list of workers identifiers that were added with add *method*.

See a short example:

```
import numpy as np
import time
from gmproc import Workers

def example(p):
	if p is not None:
		return p + 1
	else:
		return 0

if __name__=="__main__":
	ws = Workers()
	i = 0
	ws.add(1, example, params=i)
	for _ in range(10):
		i = ws.run([1])[1]
		ws.set_params(1, i)
		print(i)
```

### Client-Server

The Client-Server abstraction provides two types of objects (client and server) that interact with each other. An instance of Client-Server application has only one server and many clients.

The server life-cycle (pseudo-code) is shown as follow:

``` 
server.start(params)

while not server.done():
    request = wait_from_client()
    response = server.process(request)
   server.sendTo(request.client, response)
   server.wait()
server.finish()
    
```

With *server.start(params)*, server receives initial parameters *params*. Then, server wait a client request. When a request from the client is received, the server processes it and sends a response in sequence. Then, the server can wait for a while for the next life cycle.

The clients life-cycle is shown as follow:

```
client.start(params)
while not client.done():
    request = client.process()
    response = client.send(request)
    client.update(response)
    client.wait()
client.finish()
```
Client and Server are abstract. Their specific implementations define the behavior of the mehtods *start(params)*, *done()*, *process*, *update*, *wait*, and *finish*. 

The application must have a ClientServer object. ClientServer object receives a class the server and provides methods to create a specific type of client. ClientServer object instantiates a server object. The *new_worker* method of ClientServer object creates new instances of clients.

```
if __name__=="__main__":
	cs = ClientServer(MyServer)
	cs.new_worker('par', Client, params=(2, add2))
	cs.new_worker('impar', Client, params=(1, add2))
	cs.run()
```

The complete example is shown follow.

```
import numpy as np
import time
from gmproc import ClientServer, ClientWorker, ServerWorker


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
```