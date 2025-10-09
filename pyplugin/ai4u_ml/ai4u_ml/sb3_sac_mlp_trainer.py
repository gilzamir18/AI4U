import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MlpPolicy
from stable_baselines3.common.callbacks import CheckpointCallback
import torch
import argparse # Importa a biblioteca para argumentos de linha de comando

def parse_args():
    """
    Define e analisa os argumentos de linha de comando.
    """
    parser = argparse.ArgumentParser(description="Treina um agente SAC no ambiente AI4U.")
    
    # Argumentos da conexão do ambiente
    parser.add_argument('--rid', type=str, default='0', help='ID do robô/agente no ambiente.')
    parser.add_argument('--ip', type=str, default='127.0.0.1', help='Endereço IP do servidor do ambiente.')
    parser.add_argument('--port', type=int, default=8080, help='Porta do servidor do ambiente.')
    parser.add_argument('--buffer-size', type=int, default=819200, help='Tamanho do buffer de comunicação.')

    # Argumentos do treinamento
    parser.add_argument('--timesteps', type=int, default=2500000, help='Número total de timesteps para o treinamento.')
    parser.add_argument('--learning-starts', type=int, default=1000, help='Número de passos antes do início do aprendizado.')
    parser.add_argument('--log-interval', type=int, default=5, help='Intervalo de log durante o treinamento.')
    parser.add_argument('--tensorboard-log', type=str, default='tflog', help='Diretório para logs do TensorBoard.')

    # Argumentos do Checkpoint e Salvamento
    parser.add_argument('--save-freq', type=int, default=10000, help='Frequência (em passos) para salvar checkpoints.')
    parser.add_argument('--save-path', type=str, default='./logs/', help='Diretório para salvar os checkpoints.')
    parser.add_argument('--name-prefix', type=str, default='rl_model', help='Prefixo do nome dos arquivos de checkpoint.')
    parser.add_argument('--model-name', type=str, default='sac1m', help='Nome do arquivo para salvar o modelo final.')

    # Argumentos da Arquitetura da Rede
    parser.add_argument('--net-arch', type=str, default='64,64', help='Arquitetura da rede neural, separada por vírgulas (ex: "1024,512").')
    parser.add_argument('--no-expln', action='store_false', dest='use_expln', help='Desativa o uso de Exploration Network (expln).')

    return parser.parse_args()

if __name__ == '__main__':
    # Analisa os argumentos fornecidos na linha de comando
    args = parse_args()

    # --- Configuração do Callback de Checkpoint ---
    checkpoint_callback = CheckpointCallback(
        save_freq=args.save_freq, 
        save_path=args.save_path, 
        name_prefix=args.name_prefix
    )

    # --- Configuração do Ambiente Gym ---
    env_config = {
        "server_IP": args.ip,
        "server_port": args.port,
        "buffer_size": args.buffer_size
    }
    env = gym.make("AI4UEnv-v0", rid=args.rid, config=env_config)

    # --- Configuração da Política (Rede Neural) ---
    # Converte o argumento string 'net_arch' para uma lista de inteiros
    net_arch_list = [int(x.strip()) for x in args.net_arch.split(',')]
    
    policy_kwargs = {
        "net_arch": net_arch_list,
        "use_expln": args.use_expln,
        "optimizer_class": torch.optim.AdamW
    }

    # --- Configuração e Treinamento do Modelo SAC ---
    model = SAC(
        MlpPolicy, 
        env, 
        learning_starts=args.learning_starts, 
        policy_kwargs=policy_kwargs, 
        tensorboard_log=args.tensorboard_log, 
        verbose=1
    )
    
    model.set_env(env)

    print("Iniciando o treinamento com os seguintes parâmetros:")
    print(f" - Timesteps: {args.timesteps}")
    print(f" - IP/Porta: {args.ip}:{args.port}")
    print(f" - Modelo final: {args.model_name}.zip")
    print(f" - Arquitetura da rede: {net_arch_list}")
    print("-" * 30)

    model.learn(
        total_timesteps=args.timesteps, 
        callback=checkpoint_callback, 
        log_interval=args.log_interval
    )
    
    # Salva o modelo final com o nome fornecido
    model.save(args.model_name)
    
    print("Treinamento concluído.")
    del model # remove to demonstrate saving and loading
    print("Script finalizado!!!")
