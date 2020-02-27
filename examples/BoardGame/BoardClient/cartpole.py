from unityremote.gmproc.ga3c import A3CMaster, A3CWorker
from unityremote.gmproc import ClientServer
import gym
import os
import shutil

def env_maker(name):
	return gym.make(name)

if __name__=="__main__":
	if os.path.exists('./logs'):
		shutil.rmtree('./logs')
	os.mkdir('logs')

	params = {}
	params['env_name'] = 'CartPole-v0'
	env = gym.make(params['env_name'])
	params['state_size'] = env.observation_space.shape[0]
	params['action_size'] = env.action_space.n
	params['update_freq'] = 30
	params['entropy_bonus'] = 0.01
	params['value_loss_coef'] = 0.5
	params['learning_rate'] = 0.001
	params['max_grad_norm'] = 5
	params['gamma'] = 0.90
	params['env_maker'] = env_maker
	params['debug_freq'] = 10
	params['log_bsize'] = 1000
	params['log_verbose'] = True
	cs = ClientServer(A3CMaster)
	cs.new_workers(4, A3CWorker, params=params)
	cs.run(params=params)
