import easy_tf_log
import numpy as np

from . import utils
from .multi_scope_train_op import *
from .params import DISCOUNT_FACTOR
import copy
from collections import OrderedDict

class Worker:
    def __init__(self, sess, env, network, log_dir):
        self.sess = sess
        self.env = env
        self.network = network 
        self.bank_ops = []
        self.banks_template = OrderedDict()
        for key in network.memory_bank:
            self.banks_template[key] = []
            b = network.memory_bank[key]
            self.bank_ops += b.update

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
        if not self.network.syncAtEndOfEpisode:
            self.sess.run(self.network.sync_with_global_ops)

        actions, done, rewards, states, extra_inputs, banks = self.run_steps(n_steps)

        returns = self.calculate_returns(done, rewards)

        if done:
            if self.network.syncAtEndOfEpisode:
                self.sess.run(self.network.sync_with_global_ops)
            if len(self.network.memory_bank) > 0:
                for key in self.network.memory_bank:
                    self.network.memory_bank[key].reset()

            self.last_state = self.env.reset()

            if type(self.last_state) is tuple:
                self.last_extra_inputs = self.last_state[1]
                self.last_state = self.last_state[0]
            else:
                self.last_extra_inputs = None

            for r in self.network.resetables:
                r.reset()

            if self.logger:
                episode_value_mean = sum(self.episode_values) / len(self.episode_values)
                self.logger.logkv('rl/episode_value_mean', episode_value_mean)
            self.episode_values = []

        if self.last_extra_inputs is not None:
            feed_dict = {self.network.states: states,
                        self.network.extra_inputs: extra_inputs,
                        self.network.actions: actions,
                        self.network.returns: returns}
        else:
            feed_dict = {self.network.states: states,
                        self.network.actions: actions,
                        self.network.returns: returns}


        for key in self.network.memory_bank:
            feed_dict[self.network.memory_bank[key].input] = banks[key]

        self.sess.run(self.network.train_op, feed_dict)

        if self.summary_writer and self.updates != 0 and self.updates % 100 == 0:
            summaries = self.sess.run(self.network.summaries_op, feed_dict)
            self.summary_writer.add_summary(summaries, self.updates)

        self.updates += 1

        return len(states)

    def fillwithbank(feed_dict, banks):
        for key in banks:
            bank = banks[key]
            feed_dict[bank.input] = [bank.value] 

    def setnewmemorytobanks(banks, start_idx, outputs):
        for key in banks:
            bank = banks[key]
            if bank.isSingle:
                bank.value = outputs[start_idx][0]
                start_idx += 1
            else:
                nu = len(bank.update)
                if nu > 0:
                    result = []
                    for _ in range(nu):
                        result.append(outputs[start_idx][0])
                        start_idx += 1
                    bank.value = np.array(result, dtype=np.float32)

    def run_steps(self, n_steps):
        # States, action taken in each state, and reward from that action
        states = []
        actions = []
        rewards = []
        extra_inputs = []
        banks = copy.deepcopy(self.banks_template)
        steps = 0
        while steps <  n_steps or self.network.endepsodeglobalsync:
            for key in banks:
                banks[key].append(self.network.memory_bank[key].value.copy())

            states.append(self.last_state)
            feed_dict = None

            if self.last_extra_inputs is not None:
                extra_inputs.append(self.last_extra_inputs)
                feed_dict = {self.network.states: [self.last_state], self.network.extra_inputs: [self.last_extra_inputs]}
            else:
                feed_dict = {self.network.states: [self.last_state]}

            Worker.fillwithbank(feed_dict, self.network.memory_bank)

            outputs = \
                self.sess.run([self.network.action_probs, self.network.value] + self.bank_ops,
                            feed_dict=feed_dict)
            action_probs = outputs[0][0]
            value_estimate = outputs[1][0]
            Worker.setnewmemorytobanks(self.network.memory_bank, 2, outputs)

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
            steps += 1
            if done:
                break


        return actions, done, rewards, states, extra_inputs, banks

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
            
            Worker.fillwithbank(feed_dict, self.network.memory_bank)

            last_value = self.sess.run(self.network.value, feed_dict=feed_dict)[0]
            
            rewards += [last_value]
            returns = utils.rewards_to_discounted_returns(rewards, DISCOUNT_FACTOR)
            returns = returns[:-1]  # Chop off last_value
        return returns
