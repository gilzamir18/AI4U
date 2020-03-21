import gym


env = gym.make("UnityRemote-v0")
env.reset()
print(env.step())
print('ola')
env.close()