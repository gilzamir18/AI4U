import os
import time

input_port = 7070
output_port = 8080
host = "127.0.0.1"
sep = os.sep
for i in range(8):
    path = "%shome%skaos%sHeadWorldRC%sHeadWorldRC.x86_64 %d %d %s &"%(sep, sep, sep, sep, input_port + i, output_port + i, host)
    os.system(path)
    time.sleep(0.2)
