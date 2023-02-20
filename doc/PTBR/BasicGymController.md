# Controlador BasicGymController

## Introdução
A AI4UPE foi projetada para suportar naturalmente o protocolo de ambientes OpenAI Gym. Para isso, basta executar a criação de um ambiente usando Unity e Godot e então criar um *script* que controla o agente, como neste caso:

~~~python
env = gym.make("AI4UEnv-v0")
~~~

Este código irá gerar um controlador padrão da classe *ai4u.controllers.BasicGymController* capaz de reconhecer as características do ambiente modelado na Unity ou na Godot. Com base nessas características, pode-se executar ações no ambiente/agente por meio de comandos como *step(action)*, onde *action* é o valor da ação. Se a ação é discreta, *action* é um inteiro indicando o código da ação; caso a ação seja contínua, *action* é um arranjo (vetor) de números reais indicando a intensidade de cada uma das dimensões de ação.


Aqui temos um exemplo completo de implementação para treinar o agente do jogo [Pong](https://github.com/gilzamir18/PhongDemo):

~~~python
import ai4u
import AI4UEnv
import gym
import numpy as np
from stable_baselines3 import DQN
from stable_baselines3.common.callbacks import CheckpointCallback
checkpoint_callback = CheckpointCallback(save_freq=100000, save_path='./logs/',
                                            name_prefix='rl_model')
env = gym.make("AI4UEnv-v0", buffer_size=81920)
model = DQN("CnnPolicy", env, verbose=1, tensorboard_log="tblogs", buffer_size=500000)
model.learn(total_timesteps=3000000, log_interval=4, callback=checkpoint_callback)
model.save("model")

del model # remove to demonstrate saving and loading
~~~

Para testar este *script*, primeiro é preciso rodar o jogo Pong no Editor da Unity. Suponho que você esteja usando a versão 2021.3.* da Unity. Abra o projeto no Editor e garanta que a cena *SampleScene* esteja aberta. Agora clique em *play* e execute o *script* acima. Executei este experimento em um computador com a seguintes características:
* Processador: Intel(R) Core(TM) i5-7400 CPU @ 3.00GHz   3.00 GHz.
* 8,00 GB de RAM.
* Sistema Operacional Windows 11 de 64 bits.
* GPU Nvidia GForce 1050ti 4GB de RAM

O sistema demorou 12 horas para obter um milhão de passos e o resultado foi favorável. O agente, de fato aprendeu a jogar Pong, como mostra o seguinte gráfico:

![Gráfico](https://by3301files.storage.live.com/y4mBFID5H01I_Z5o5VdQ_dAYnAP-eh_MsDKZpWCywqhqx-BMvzHbtD23roz99QqsdmE5BncH0c59wy6OEkVyE7TsblGg-In_CY29MQ81MRzXmrIOwO2Q2XhSy9kcHFSLGneVhOlDB7KYvCsKF0nXYTkWbmihxz_1IeKyBR7qlk_lAFA6dFtbISekGqKNlmFkC110-E6CXpkIqsMYRLzoJKbRjjnpPcziXRnpTU6WkJu7c0?encodeFailures=1&width=384&height=311)

Se você não tiver paciência para esperar o treinamento, pode rodar o agente já treinado com o seguinte script:

~~~python
import ai4u
import AI4UEnv
import gym
from stable_baselines3 import DQN

env = gym.make("AI4UEnv-v0", buffer_size=81920)

model = DQN.load("logs/rl_model_1000000_steps")
obs = env.reset()
while True:
    action, _states = model.predict(obs, deterministic=True)
    obs, reward, done, info = env.step(action)
    env.render()
    if done:
      obs = env.reset()
~~~

O resultado deve ser como o mostrado abaixo:

![](https://public.by.files.1drv.com/y4mE4z_1xivtrP8mdLnopcJSoad1Vs70jnclJtfQrK5GTBCXjnVVfavVBvTgizC0ytDV4acsbPokboN_tnW8iIppCDHZs1OP1ZJ0_NRh5f2T5DTDSrXSIauYIhPOalXStNutHBQ3StqPYfHcseiwq6kqFQasiuaDN_ozHHnkRkIPDOo3Wn2JTat0XamQo0JxU7jlxYSiUzP4TECDSZDGXWh2KHbKBYOtlXXLhjKQNE5ziw)

A jogador da esquerda é pré-programado enquanto que o jogador da esquerda é controlado pela rede neural carregada no código de demonstração. Todo o código mostrado nessa página pode ser obtido no site do projeto [Pong](https://github.com/gilzamir18/PhongDemo), inclusive a rede neural (modelo) usada neste exemplo está no diretório python_scripts/logs (a partir da raiz do repositório).