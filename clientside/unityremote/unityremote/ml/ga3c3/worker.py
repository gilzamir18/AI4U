import easy_tf_log
import numpy as np

from . import utils
from .multi_scope_train_op import *
from .params import DISCOUNT_FACTOR


class Worker:

    def __init__(self, sess, env, network, log_dir, goal_checker=None):
        if goal_checker is None:
            raise Exception("goal_checker must be a class with __call__(goal, pos, min, max, default_value) method implemented!!!")

        self.sess = sess
        self.env = env
        self.network = network
        self.goal_checker = goal_checker
        self.current_goal = 0

        if network.summaries_op is not None:
            self.summary_writer = tf.summary.FileWriter(log_dir, flush_secs=1)
            self.logger = easy_tf_log.Logger()
            self.logger.set_writer(self.summary_writer.event_writer)
        else:
            self.summary_writer = None
            self.logger = None

        self.updates = 0
        self.last_state = self.env.reset()
        self.goal_checker.reset()

        self.last_goal_inputs = self.last_state[1]

        if len(self.last_goal_inputs) % 3 != 0:
            raise Exception('Proprioceptions must be multiples of three. Current size of Proprioceptions: {0}'.format(len(self.last_goal_inputs)))

        self.goals = self.make_goals(self.last_goal_inputs)
        self.current_goal = np.random.choice(len(self.goals))

        self.last_state = self.last_state[0]

        self.episode_values = []


    def make_goals(self, prop):
        goals = [ [0, 0, 0] ]
        j = 0
        for _ in range(len(prop)//3):
            goals.append([j, j+1, j+2])
            j += 3
        return goals

    def run_update(self, n_steps):
        self.sess.run(self.network.sync_with_global_ops)
        n, done = self.run_goal(n_steps, self.goals[self.current_goal], True)
        for idx, goal in enumerate(self.goals):
            if idx != self.current_goal:
                ns, dn = self.run_goal(n_steps, goal, False)
                n += ns
                self.updates += 1

        if done:
            self.last_state = self.env.reset()
            self.goal_checker.reset()
            self.last_goal_inputs = self.last_state[1]

            if len(self.last_goal_inputs) % 3 != 0:
                raise Exception('Proprioceptions must be multiples of three. Current size of Proprioceptions: '.format(len(self.last_goal_inputs)))

            self.goals = self.make_goals(self.last_goal_inputs)
            self.current_goal = np.random.choice(len(self.goals))
            self.last_state = self.last_state[0]

            if self.logger:
                episode_value_mean = sum(self.episode_values) / len(self.episode_values)
                self.logger.logkv('rl/episode_value_mean', episode_value_mean)
            self.episode_values = []
        return n

    def run_goal(self, n_steps, goal, main_goal=True):
        actions, done, rewards, states, goal_inputs = self.run_steps(n_steps, goal, main_goal)
        returns = self.calculate_returns(done, rewards, goal)

        feed_dict = {self.network.states: states,
                     self.network.goal_inputs: goal_inputs,
                     self.network.goal: [goal]*max(1, len(actions)),
                     self.network.actions: actions,
                     self.network.returns: returns}

        self.sess.run(self.network.train_op, feed_dict)

        if self.summary_writer and self.updates != 0 and self.updates % 100 == 0:
            summaries = self.sess.run(self.network.summaries_op, feed_dict)
            self.summary_writer.add_summary(summaries, self.updates)

        return len(states), done

    def run_steps(self, n_steps, goal_state, main_goal=True):
        # States, action taken in each state, and reward from that action
        states = []
        actions = []
        rewards = []
        goal_inputs = []
        env_rewards = []
        for i in range(n_steps):
            states.append(self.last_state)
            feed_dict = None
            goal_inputs.append(self.last_goal_inputs)
            if (main_goal):
                feed_dict = {self.network.states: [self.last_state], self.network.goal_inputs: [self.last_goal_inputs], self.network.goal: [goal_state]}
            
                [action_probs], [value_estimate] = \
                    self.sess.run([self.network.action_probs, self.network.value],
                                  feed_dict=feed_dict)

                self.episode_values.append(value_estimate)

                action = np.random.choice(self.env.action_space.n, p=action_probs)
                actions.append(action)
                self.last_state, reward, done, _ = self.env.step(action)
                env_rewards.append(reward)

                reward = self.goal_checker(goal_state, self.last_goal_inputs[goal_state[0]], self.last_goal_inputs[goal_state[1]], self.last_goal_inputs[goal_state[2]], reward)

                self.last_goal_inputs = self.last_state[1]
                self.last_state = self.last_state[0]
                rewards.append(reward)
                if done:
                    break
            else:
                feed_dict = {self.network.states: [self.bk_states[i]], self.network.goal_inputs: [self.bk_goal_inputs[i]], self.network.goal: [goal_state]}

                [action_probs], [value_estimate] = \
                    self.sess.run([self.network.action_probs, self.network.value],
                                  feed_dict=feed_dict)

                self.episode_values.append(value_estimate)
                
                goal = goal_state             
                goal_input = self.bk_goal_inputs[i]

                r = self.goal_checker(goal, goal_input[goal[0]], goal_input[goal[1]], goal_input[goal[2]], self.bk_env_rewards[i])
                rewards.append(r)

                if len(rewards) >= len(self.bk_rewards):
                    break

        if main_goal:
            self.bk_actions = actions[:]
            self.bk_rewards = rewards[:]
            self.bk_states = states[:]
            self.bk_goal_inputs = goal_inputs[:]
            self.bk_done = done
            self.bk_env_rewards = env_rewards[:]
        else:
            actions = self.bk_actions[:]
            states = self.bk_states[:]
            goal_inputs = self.bk_goal_inputs[:]
            done = self.bk_done

        return actions, done, rewards, states, goal_inputs

    def calculate_returns(self, done, rewards, goal_state):
        if done:
            returns = utils.rewards_to_discounted_returns(rewards, DISCOUNT_FACTOR)
        else:
            # If we're ending in a non-terminal state, in order to calculate returns,
            # we need to know the return of the final state.
            # We estimate this using the value network.
            feed_dict = None
            feed_dict = {self.network.states: [self.last_state], self.network.goal_inputs: [self.last_goal_inputs], self.network.goal: [goal_state]}

            last_value = self.sess.run(self.network.value, feed_dict=feed_dict)[0]
            rewards += [last_value]
            returns = utils.rewards_to_discounted_returns(rewards, DISCOUNT_FACTOR)
            returns = returns[:-1]  # Chop off last_value
        return returns
