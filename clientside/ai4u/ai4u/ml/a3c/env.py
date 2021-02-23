from multiprocessing import Pipe, Process
from os import path as osp

import gym

from .debug_wrappers import MonitorEnv, NumberFrames

class thunk:
    def __init__(self, env_n, env_id, preprocess_wrapper, max_n_noops, n_envs, seed, debug, log_dir, env_defs):
        self.env_n = env_n
        self.env_id = env_id
        self.preprocess_wrapper = preprocess_wrapper
        self.max_n_noops = max_n_noops
        self.n_envs = n_envs
        self.seed = seed
        self.debug = debug
        self.log_dir = log_dir
        self.env_defs = env_defs
    
    def __call__(self):
        env = gym.make(self.env_id)
        env.configure(self.env_defs, self.env_n)
        # We calculate the env seed like this so that changing the global seed completely
        # changes the whole set of env seeds.
        env_seed = self.seed * self.n_envs + self.env_n
        env.seed(env_seed)

        if self.env_n == 0:
            env_log_dir = osp.join(self.log_dir, "env_0")
        else:
            env_log_dir = None
        env = MonitorEnv(env, "Env {}".format(self.env_n), log_dir=env_log_dir)

        if self.debug:
            env = NumberFrames(env)
        env = self.preprocess_wrapper(env, self.max_n_noops)
        return env

def make_make_env_fn(env_n, env_id, preprocess_wrapper, max_n_noops, n_envs, seed, debug, log_dir, env_defs):
    return thunk(env_n, env_id, preprocess_wrapper, max_n_noops, n_envs, seed, debug, log_dir, env_defs)

def make_envs(env_id, preprocess_wrapper, max_n_noops, n_envs, seed, debug, log_dir, env_defs):
    make_env_fns = [make_make_env_fn(env_n, env_id, preprocess_wrapper, max_n_noops, n_envs, seed, debug, log_dir, env_defs) for env_n in range(n_envs)]
    envs = [SubProcessEnv(make_env_fns[env_n]) for env_n in range(n_envs)]
    return envs


class SubProcessEnv:
    """
    Run a gym environment in a subprocess so that we can avoid GIL and run multiple environments
    asynchronously from a single thread.
    """

    @staticmethod
    def env_process(pipe, make_env_fn):
        env = make_env_fn()
        pipe.send((env.observation_space, env.action_space))
        while True:
            cmd, data, agent_info = pipe.recv()
            if cmd == 'step':
                action = data
                if agent_info is None:
                    obs, reward, done, info = env.step(action)
                else:
                    obs, reward, done, info = env.step(action, agent_info)
                pipe.send((obs, reward, done, info))
            elif cmd == 'reset':
                obs = env.reset()
                pipe.send(obs)

    def __init__(self, make_env_fn):
        p1, p2 = Pipe()
        self.pipe = p1
        self.proc = Process(target=self.env_process, args=[p2, make_env_fn])
        self.proc.start()
        self.observation_space, self.action_space = self.pipe.recv()

    def reset(self):
        self.pipe.send(('reset', None, None))
        return self.pipe.recv()

    def step(self, action, info=None):
        self.pipe.send(('step', action, info))
        return self.pipe.recv()

    def close(self):
        self.proc.terminate()
