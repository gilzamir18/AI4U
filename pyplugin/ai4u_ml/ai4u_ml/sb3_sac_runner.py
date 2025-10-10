import argparse
import gymnasium as gym
import numpy as np
import torch
from stable_baselines3 import SAC

# Your project's imports
import ai4u
import AI4UEnv
from ai4u.controllers import BasicGymController

# --- Global Variables for Callbacks ---
# Used for communication between callbacks and the main loop.
total_steps = 0
# The 'model' variable will be set in the main function and declared as global
# so that callbacks can access it.
model = None


def parse_arguments():
    """Sets up and parses command-line arguments."""
    parser = argparse.ArgumentParser(description="Run a Stable Baselines 3 trained agent in the AI4U Environment.")
    
    parser.add_argument(
        '--model-path',
        type=str,
        default="model",
        help="Path to the trained model's .zip file."
    )
    parser.add_argument(
        '--server-ip',
        type=str,
        default='127.0.0.1',
        help="IP address of the AI4U environment server."
    )
    parser.add_argument(
        '--server-port',
        type=int,
        default=8080,
        help="Port of the AI4U environment server."
    )
    parser.add_argument(
        '--buffer-size',
        type=int,
        default=81900,
        help="Buffer size for communication with the environment."
    )
    parser.add_argument(
        '--rid',
        type=str,
        default='0',
        help="Remote agent ID (RID)."
    )
    parser.add_argument(
        '--nondeterministic',
        action='store_true',
        help="If specified, uses the stochastic (non-deterministic) policy for prediction."
    )
    
    return parser.parse_args()

def main():
    """Main function to load the model and run the inference loop."""
    global model # Allow callbacks to access the model loaded here

    args = parse_arguments()
    is_deterministic = not args.nondeterministic

    print("--- Configuration ---")
    print(f"Model: {args.model_path}")
    print(f"Server: {args.server_ip}:{args.server_port}")
    print(f"Prediction Mode: {'Deterministic' if is_deterministic else 'Stochastic'}")
    print("---------------------")

    # Create the environment with command-line parameters
    env = gym.make(
        "AI4UEnv-v0",
        rid=args.rid,
        config=dict(
            server_IP=args.server_ip,
            server_port=args.server_port,
            buffer_size=args.buffer_size
        )
    )

    # Load the trained model
    try:
        model = SAC.load(
            args.model_path,
            custom_objects={'action_space': env.action_space, 'observation_space': env.observation_space}
        )
        print("Model loaded successfully!")
    except Exception as e:
        print(f"Error loading model from '{args.model_path}': {e}")
        return

    # Agent execution loop
    obs, info = env.reset()
    reward_sum = 0
    while True:
        try:
            action, _states = model.predict(obs, deterministic=is_deterministic)
            obs, reward, done, truncate, info = env.step(action)
            
            reward_sum += reward
            print(f"Step: {total_steps} | Reward: {reward:.4f} | Cumulative Reward: {reward_sum:.4f}")

            if done or truncate:
                print(f"End of episode! Final reward: {reward_sum}")
                reward_sum = 0
                obs, info = env.reset()

        except KeyboardInterrupt:
            print("\nExecution interrupted by user.")
            break
        except Exception as e:
            print(f"\nAn error occurred during execution: {e}")
            break
            
    env.close()
    print("Environment closed.")

if __name__ == '__main__':
    main()
