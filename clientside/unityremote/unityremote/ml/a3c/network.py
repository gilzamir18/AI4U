import tensorflow as tf

from .multi_scope_train_op import make_train_op
from .utils_tensorflow import make_grad_histograms, make_histograms, make_rmsprop_histograms, \
    logit_entropy, make_copy_ops


def make_loss_ops(action_logits, values, entropy_bonus, value_loss_coef, debug):
    actions = tf.placeholder(tf.int64, [None])
    returns = tf.placeholder(tf.float32, [None])

    # For the policy loss, we want to calculate log π(action_t | state_t).
    # That means we want log(action_prob_0 | state_t) if action_t = 0,
    #                    log(action_prob_1 | state_t) if action_t = 1, etc.
    # It turns out that's exactly what a cross-entropy loss gives us!
    # The cross-entropy of a distribution p wrt a distribution q is:
    #   - sum over x: p(x) * log2(q(x))
    # Note that for a categorical distribution, considering the cross-entropy of the ground truth
    # distribution wrt the distribution of predicted class probabilities, p(x) is 1 if the ground
    # truth label is x and 0 otherwise. We therefore have:
    #   - log2(q(0)) if ground truth label = 0,
    #   - log2(q(1)) if ground truth label = 1, etc.
    # So here, by taking the cross-entropy of the distribution of action 'labels' wrt the produced
    # action probabilities, we can get exactly what we want :)
    _neglogprob = tf.nn.sparse_softmax_cross_entropy_with_logits(logits=action_logits,
                                                                 labels=actions)
    with tf.control_dependencies([tf.assert_rank(_neglogprob, 1)]):
        neglogprob = _neglogprob

    _advantage = returns - values
    with tf.control_dependencies([tf.assert_rank(_advantage, 1)]):
        advantage = _advantage

    if debug:
        neglogprob = tf.Print(neglogprob, [actions], message='\ndebug actions:',
                              summarize=2147483647)
        advantage = tf.Print(advantage, [returns], message='\ndebug returns:',
                             summarize=2147483647)

    policy_entropy = tf.reduce_mean(logit_entropy(action_logits))

    # Note that the advantage is treated as a constant for the policy network update step.
    # We're calculating advantages on-the-fly using the value approximator. This might make us
    # worry: what if we're using the loss for training, and the advantages are calculated /after/
    # training has changed the network? But for A3C, we don't need to worry, because we compute the
    # gradients separately from applying them.
    #
    # Note also that we want to maximise entropy, which is the same as minimising negative entropy.
    policy_loss = neglogprob * tf.stop_gradient(advantage)
    policy_loss = tf.reduce_mean(policy_loss) - entropy_bonus * policy_entropy
    value_loss = value_loss_coef * tf.reduce_mean(0.5 * advantage ** 2)
    loss = policy_loss + value_loss

    return actions, returns, advantage, policy_entropy, policy_loss, value_loss, loss


class Network:

    def __init__(self, scope, n_actions, entropy_bonus, value_loss_coef, max_grad_norm, optimizer,
                 add_summaries, state_shape, make_inference_network, detailed_logs=False, debug=False, extra_inputs_shape=None):

        with tf.variable_scope(scope):
            #extra_inputs is a list of input channels that it is not a main input channel
            observations, action_logits, action_probs, value, layers = \
                make_inference_network(obs_shape=state_shape, n_actions=n_actions, debug=debug, extra_inputs_shape=extra_inputs_shape)

            actions, returns, advantage, policy_entropy, policy_loss, value_loss, loss = \
                make_loss_ops(action_logits, value, entropy_bonus, value_loss_coef, debug)

        sync_with_global_op = make_copy_ops(from_scope='global', to_scope=scope)

        train_op, grads_norm = make_train_op(loss, optimizer,
                                             compute_scope=scope, apply_scope='global',
                                             max_grad_norm=max_grad_norm)

        self.states = None
        self.extra_inputs = None

        if observations is tuple:
            self.states = observations[0]
            self.extra_inputs = observations[1]
        else:
            self.states = observations
    
        self.action_probs = action_probs
        self.value = value
        self.actions = actions
        self.returns = returns
        self.advantage = advantage
        self.policy_entropy = policy_entropy
        self.policy_loss = policy_loss
        self.value_loss = value_loss
        self.loss = loss
        self.layers = layers

        self.sync_with_global_ops = sync_with_global_op
        self.optimizer = optimizer
        self.train_op = train_op
        self.grads_norm = grads_norm

        if add_summaries:
            self.summaries_op = self.make_summary_ops(scope, detailed_logs)
        else:
            self.summaries_op = None

    def make_summary_ops(self, scope, detailed_logs):
        variables = tf.trainable_variables(scope)
        grads_policy = tf.gradients(self.policy_loss, variables)
        grads_value = tf.gradients(self.value_loss, variables)
        grads_combined = tf.gradients(self.loss, variables)
        grads_norm_policy = tf.global_norm(grads_policy)
        grads_norm_value = tf.global_norm(grads_value)
        grads_norm_combined = tf.global_norm(grads_combined)

        scalar_summaries = [
            ('rl/policy_entropy', self.policy_entropy),
            ('rl/advantage_mean', tf.reduce_mean(self.advantage)),
            ('loss/loss_policy', self.policy_loss),
            ('loss/loss_value', self.value_loss),
            ('loss/loss_combined', self.loss),
            ('loss/grads_norm_policy', grads_norm_policy),
            ('loss/grads_norm_value', grads_norm_value),
            ('loss/grads_norm_combined', grads_norm_combined),
            ('loss/grads_norm_combined_clipped', self.grads_norm),
        ]
        summaries = []
        for name, val in scalar_summaries:
            summary = tf.summary.scalar(name, val)
            summaries.append(summary)

        if detailed_logs:
            summaries.extend(make_grad_histograms(variables, grads_combined))
            summaries.extend(make_rmsprop_histograms(self.optimizer))
            summaries.extend(make_histograms(self.layers, 'activations'))
            summaries.extend(make_histograms(variables, 'weights'))

        return tf.summary.merge(summaries)
