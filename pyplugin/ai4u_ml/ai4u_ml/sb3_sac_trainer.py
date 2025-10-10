import argparse
import torch
import gymnasium as gym
from stable_baselines3 import SAC
# Import both policies to allow for selection
from stable_baselines3.sac import MlpPolicy, MultiInputPolicy, CnnPolicy
from stable_baselines3.common.callbacks import CheckpointCallback

# Your project's imports
import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv

def parse_args():
    """
    Defines and parses command-line arguments.
    """
    parser = argparse.ArgumentParser(description="Train a SAC agent in the AI4U environment.")
    
    # Environment connection arguments
    parser.add_argument('--rid', type=str, default='0', help='Robot/agent ID in the environment.')
    parser.add_argument('--ip', type=str, default='127.0.0.1', help='IP address of the environment server.')
    parser.add_argument('--port', type=int, default=8080, help='Port of the environment server.')
    parser.add_argument('--buffer-size', type=int, default=819200, help='Communication buffer size.')

    # Training arguments
    parser.add_argument('--timesteps', type=int, default=2500000, help='Total number of timesteps for training.')
    parser.add_argument('--learning-starts', type=int, default=1000, help='Number of steps before learning starts.')
    parser.add_argument('--log-interval', type=int, default=5, help='Log interval during training.')
    parser.add_argument('--tensorboard-log', type=str, default='tflog', help='Directory for TensorBoard logs.')

    # Checkpoint and Saving arguments
    parser.add_argument('--save-freq', type=int, default=10000, help='Frequency (in steps) to save checkpoints.')
    parser.add_argument('--save-path', type=str, default='./logs/', help='Directory to save checkpoints.')
    parser.add_argument('--name-prefix', type=str, default='rl_model', help='Name prefix for checkpoint files.')
    parser.add_argument('--model-name', type=str, default='sac1m', help='Filename for saving the final model.')

    # Policy and Network Architecture arguments
    parser.add_argument(
        '--policy', 
        type=str, 
        default='MlpPolicy',  # MlpPolicy is the default
        choices=['MlpPolicy', 'MultiInputPolicy', 'CnnPolicy'],
        help='The policy to use for the SAC model (MlpPolicy or MultiInputPolicy).'
    )
    parser.add_argument('--net-arch', type=str, default='64,64', help='Neural network architecture, comma-separated (e.g., "1024,512").')
    parser.add_argument('--no-expln', action='store_false', dest='use_expln', help='Disables the use of the Exploration Network (expln).')

    return parser.parse_args()

if __name__ == '__main__':
    # Parse the arguments provided via command line
    args = parse_args()

    # --- Map policy string to the corresponding class ---
    policy_map = {
        "MlpPolicy": MlpPolicy,
        "MultiInputPolicy": MultiInputPolicy,
        "CnnPolicy": CnnPolicy  # Placeholder if CNN policy is to be added later
    }
    selected_policy = policy_map[args.policy]

    # --- Setup Checkpoint Callback ---
    checkpoint_callback = CheckpointCallback(
        save_freq=args.save_freq, 
        save_path=args.save_path, 
        name_prefix=args.name_prefix
    )

    # --- Setup Gym Environment ---
    env_config = {
        "server_IP": args.ip,
        "server_port": args.port,
        "buffer_size": args.buffer_size
    }
    env = gym.make("AI4UEnv-v0", rid=args.rid, config=env_config)

    # --- Setup Policy (Neural Network) Keyword Arguments ---
    # Convert the 'net_arch' string argument to a list of integers
    net_arch_list = [int(x.strip()) for x in args.net_arch.split(',')]
    
    policy_kwargs = {
        "net_arch": net_arch_list,
        "use_expln": args.use_expln,
        "optimizer_class": torch.optim.AdamW
    }

    # --- Configure and Train the SAC Model ---
    model = SAC(
        selected_policy,  # The policy is now dynamically selected
        env, 
        learning_starts=args.learning_starts, 
        policy_kwargs=policy_kwargs, 
        tensorboard_log=args.tensorboard_log, 
        verbose=1
    )
    
    model.set_env(env)

    print("Starting training with the following parameters:")
    print(f" - Timesteps: {args.timesteps}")
    print(f" - IP/Port: {args.ip}:{args.port}")
    print(f" - Policy: {args.policy}")
    print(f" - Final model: {args.model_name}.zip")
    print(f" - Network architecture: {net_arch_list}")
    print("-" * 30)

    model.learn(
        total_timesteps=args.timesteps, 
        callback=checkpoint_callback, 
        log_interval=args.log_interval
    )
    
    # Save the final model with the provided name
    model.save(args.model_name)
    
    print("Training finished.")
    del model # remove to demonstrate saving and loading
    print("Script finished!!!")