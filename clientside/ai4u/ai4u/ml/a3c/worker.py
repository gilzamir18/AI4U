import easy_tf_log
import numpy as np

from . import utils
from .multi_scope_train_op import *
from .params import DISCOUNT_FACTOR


class Worker:

    def __init__(self, sess, env, network, log_dir):
        self.sess = sess
        self.env = env
        self.network = network

        if network.summaries_op is not None:
            self.summary_writer = tf.summary.FileWriter(log_dir, flush_secs=1)
            self.logger = easy_tf_log.Logger()
            self.logger.set_writer(self.summary_writer.event_writer)
        else:
            self.summary_writer = None
            self.logger = None

        self.updates = 0
        self.last_state = self.env.reset()
        if type(self.last_state) is tuple:
            self.last_extra_inputs = self.last_state[1]
            self.last_state = self.last_state[0]
        else:
            self.last_extra_inputs = None

        self.episode_values = []

    def run_update(self, n_steps):
        self.sess.run(self.network.sync_with_global_ops)

        actions, done, rewards, states, extra_inputs = self.run_steps(n_steps)

        returns = self.calculate_returns(done, rewards)

        if done:
            self.last_state = self.env.reset()
            if type(self.last_state) is tuple:
                self.last_extra_inputs = self.last_state[1]
                self.last_state = self.last_state[0]
            else:
                self.last_extra_inputs = None

            if self.logger:
                episode_value_mean = sum(self.episode_values) / len(self.episode_values)
                self.logger.logkv('rl/episode_value_mean', episode_value_mean)
            self.episode_values = []

        
        feed_dict = None

        if self.last_extra_inputs is not None:
            feed_dict = {self.network.states: states,
                         self.network.extra_inputs: extra_inputs,
                         self.network.actions: actions,
                         self.network.returns: returns}
        else:
            feed_dict = {self.network.states: states,
                         self.network.actions: actions,
                         self.network.returns: returns}

        self.sess.run(self.network.train_op, feed_dict)

        if self.summary_writer and self.updates != 0 and self.updates % 100 == 0:
            summaries = self.sess.run(self.network.summaries_op, feed_dict)
            self.summary_writer.add_summary(summaries, self.updates)

        self.updates += 1

        return len(states)

    def run_steps(self, n_steps):
        # States, action taken in each state, and reward from that action
        states = []
        actions = []
        rewards = []
        extra_inputs = []

        for _ in range(n_steps):
            states.append(self.last_state)
            feed_dict = None
            if self.last_extra_inputs is not None:
                extra_inputs.append(self.last_extra_inputs)
                feed_dict = {self.network.states: [self.last_state], self.network.extra_inputs: [self.last_extra_inputs]}
            else:
                feed_dict = {self.network.states: [self.last_state]}

            [action_probs], [value_estimate] = \
                self.sess.run([self.network.action_probs, self.network.value],
                              feed_dict=feed_dict)

            self.episode_values.append(value_estimate)

            action = np.random.choice(self.env.action_space.n, p=action_probs)
            
            self.last_state, reward, done, envinfo = self.env.step(action, (action_probs, value_estimate) )
            if 'action' in envinfo:
                action = envinfo['action']
            actions.append(action)
            if type(self.last_state) is tuple:
                self.last_extra_inputs = self.last_state[1]
                self.last_state = self.last_state[0]
            else:
                self.last_extra_inputs = None

            rewards.append(reward)

            if done:
                break

        return actions, done, rewards, states, extra_inputs

    def calculate_returns(self, done, rewards):
        if done:
            returns = utils.rewards_to_discounted_returns(rewards, DISCOUNT_FACTOR)
        else:
            # If we're ending in a non-terminal state, in order to calculate returns,
            # we need to know the return of the final state.
            # We estimate this using the value network.
            feed_dict = None
            if self.last_extra_inputs is not None:
                feed_dict = {self.network.states: [self.last_state], self.network.extra_inputs: [self.last_extra_inputs]}
            else:       
                feed_dict = {self.network.states: [self.last_state]}

            last_value = self.sess.run(self.network.value, feed_dict=feed_dict)[0]
            rewards += [last_value]
            returns = utils.rewards_to_discounted_returns(rewards, DISCOUNT_FACTOR)
            returns = returns[:-1]  # Chop off last_value
        return returns
