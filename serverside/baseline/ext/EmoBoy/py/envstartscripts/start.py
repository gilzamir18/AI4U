'''
This script is an example of the efficiently starting an environment app that implements AI4U
infrascructure supporting algorithms as A3C, PPO2 and SAC.
'''
import os
import time

input_port = 7070 #match this value with the property start input port of the object BrainManager
output_port = 8080 #match this value with the property start output port of the object BrainManager
host = "127.0.0.1" #point to localhost if standalone, otherwise use correspondent ip address of the remote controller.
sep = os.sep #OS path separator
N = 1 #number of environments, use more that one for A3C, PPO2 and similars algorithms 
for i in range(N):
    #replace <appname> by binary name in your platform.
    path = "./<appname> --ai4u_inputport %d --ai4u_outputport %d --ai4u_remoteip %s --ai4u_timescale 1 --ai4u_targetframerate 1000 --ai4u_vsynccount 0"%(input_port + i, output_port + i, host)
    os.system(path) #run current app
    time.sleep(0.3) #it's import one wait for a time while previous started app loads your resources.