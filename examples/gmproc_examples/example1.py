import numpy as np
import time
from unityremote.gmproc import Workers

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