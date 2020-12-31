def to_image(img):
    imgdata = image_decode(img, #IW, #IH)
    return imgdata

'''
This method extract environemnt state from a remote environment response.
'''
def get_state_from_fields(fields):
    #TPL_RETURN_STATE

'''
It's necessary overloading the BasicAgent because server response (remote environment) don't have default field 'frame' as state.
'''
class Agent(BasicAgent):
    def __init__(self):
        BasicAgent.__init__(self)
        #RAYCASTING1self.history = deque(maxlen=#HISTSIZE)
        #RAYCASTING2for _ in range(#HISTSIZE):
            #RAYCASTING1self.history.append( np.zeros( (#SHAPE1, #SHAPE2) ) )

    def __get_state__(self, env_info):
        p = get_state_from_fields(env_info)
        state = None
        if p[1] is not None:
            #RAYCASTING1self.history.append(p[1])
            frameseq = np.array(self.history, dtype=np.float32)
            frameseq = np.moveaxis(frameseq, 0, -1)
            if p[0] is not None:
                state = (frameseq, np.array(p[0], dtype=np.float32))
            else:
                state = frameseq
        elif p[0] is not None:
            state = np.array(p[0], dtype=np.float32)
        return state

    def reset(self, env):
        env_info = env.remoteenv.step("restart")
        return self.__get_state__(env_info)

    def act(self, env, action, info=None):
        reward = 0
        envinfo = {}
        for _ in range(8):
            envinfo = env.one_stepfv(action)
            reward += envinfo['reward']
            if envinfo['done']:
                break
        return self.__get_state__(envinfo), reward, envinfo['done'], envinfo

def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--run",
                        choices=['train', 'test'],
                        default='train')
    parser.add_argument('--path', default='.')
    parser.add_argument('--preprocessing', choices=['generic', 'user_defined'])
    return parser.parse_args()

def make_env_def():
        #DISABLE51environment_definitions['state_shape'] = #TPL_INPUT_SHAPE
        #DISABLE52environment_definitions['extra_inputs_shape'] = (#ARRAY_SIZE,)
        #NETWORKenvironment_definitions['make_inference_network'] = make_inference_network
        environment_definitions['action_shape'] = #TPL_OUTPUT_SHAPE
        environment_definitions['actions'] = #TPL_ACTIONS
        environment_definitions['agent'] = Agent

def train():
        args = ['--n_workers=#WORKERS', 'AI4U-v0']
        make_env_def()
        run_train(environment_definitions, args)

def test(path):
        args = ['AI4U-v0', path,]
        make_env_def()
        run_test(environment_definitions, args)


if __name__ == '__main__':
   args = parse_args()
   if args.run == "train":
        train()
   elif args.run == "test":
        test(args.path)
