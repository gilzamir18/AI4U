import numpy as np
import sys

if  __name__ == "__main__":
    args = sys.argv
    n = 8
    INT_MAX = 2**31 - 1
    if len(args) > 1:
        n = int(args[1])
    with open('seed.txt', 'w') as seedstore:
        for i in range(n):
            seed = int(np.random.uniform() *  INT_MAX)
            seedstore.write("%d\n"%(seed))
    print("Done")