# Controladores

[Sumário](summary.md)

## Introdução
Um controlador é um objeto que faz o mapeamento dos dados do mundo do jogo (aplicativo ou cena rodando na Godot) para o mundo de ambientes que seguem o padrão [Gymnasium](https://gymnasium.farama.org/index.html). Portanto, quando um ambiente AI4UEnv-v0 for criado, você tem que definir um controlador para ele ou usar o controlador padrão **ai4u.controllers.BasicGymController**.

## Usando o Controlador Padrão

Para usar o controlador padrão, basta você criar um ambiente sem especificar qualquer controlador, dessa forma:

```python
import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym

'''
Criamos um agente cujo ID na Godot foi definido como 0,
comunicando-se por meio do IP 127.0.0.1, na porta 8080.
Não foi definido um custom controller, portanto,
o controlador padrão BasicGymController é usado.
'''
env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080)) 
```
import numpy as np

Observe que não há um argumento especificando qualquer controlador que seja, ou seja, esse código usa um controlador padrão (BasicGymController). 

O controlador padrão usa uma convenção de dados e, por meio desta convenção, consegue automaticamente mapear dados entre a Godot e o script de treinamento em Python. Um algoritmo de treinamento tem que seguir as convenções de ambiente de Gymnasium para ser compatível com o controllador padrão da AI4U. Lembando que esta é uma condição necessária de compatibilidade, mas não suficiente. O ambiente criado na Godot somente será compatível com o controlador padrão se seguir as [convenções de dados da AI4U](dataconventions.md). Se você utilizou apenas os componentes pré-definidos (sensores e atuadores) da AI4U na Godot, então essa condição também estará satisfeita. Se você quiser criar seu próprio atuador ou sensor e mantê-lo compatível com o controlador padrão, então leia as [convenções de dados da AI4U](dataconventions.md) e garanta que o seu atuador ou o seu sensor siga estas regras.

Portanto, supondo que seu ambiente na Godot é compatível com o controlador padrão, para treinar o agente usando, por exemplo, o framework stable-baselines3, basta você executar:

```python
import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym
from stable_baselines3 import SAC
from stable_baselines3.sac import MlpPolicy

env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080)) #o ambiente AI4UEnv é criado.


model = SAC(MlpPolicy, env, learning_starts=10000 tensorboard_log='tflog', verbose=1) #Cria-se um modelo com uma política MlpPolicy da stable-baseline3.

model.set_env(env) #Define o ambiente do modelo.

print("Training....")
model.learn(total_timesteps=5000000, callback=checkpoint_callback, log_interval=5) #Inicializa o algoritmo de aprendizagem. Isso pode demorar muito.

model.save("sac1m") #salva o modelo treinado em disco. 
print("Trained...")

del model # remove to demonstrate saving and loading
print("Train finished!!!")
```

Observe que usamos o framework stable-baselines3, que é o nosso framework preferencial para ser usado com AI4U, embora o core da AI4U seja independente de framework.

### Convenções para o Controlador Padrão
O controlador padrão supõe algumas convenções no uso de sensores e de atuadores. Atuadores e sensores são componentes criados na Godot (alguns já pré-fabricados) que transmitem dados e ações para o ambiente e recebem informações do ambiente, respectivamente. O controlador BasicGymController supõe que haverá dois formatos de dados: linear (nomeado como array) e bi ou tridimensional (denominado como vision). Ou seja, o agente deve pode ter um sensor com a chave (PerceptionKey) igual a "array" e um outro sensor com a chave igual a "vision". Isso não significa que um agente só possa ter dois sensores. Na verdade, o controlador padrão espera um ou dois sensores, mas como há sensores que agrupam dados de um ou mais sensores sob uma mesma chave, então, o agente pode ter qualquer número de sensores.


Se o agente a ser treinado tem apenas um sensor, esse sensor tem que ter uma chave ou "array" ou "vision". Ao ler dados de um sensor com que tenha a chave "array", o controlador padrão espera que os dados possam ser tratados como um array. Se, por outro lado, se o valor da chave for "vision", o controlador espera uma imagem WxH (W e H são inteiros maiores do que zero), um objeto CxWxH (C é um inteiro maior do que zero), ou um objeto KxCxWxH (K é um inteiro maior do zero) onde W é a largura da imagem, H é a altura, C é a quantidade de canais na imagem e K é o tamanho da pilha de imagens. Se a imagem tiver apenas luminância, um tensor da forma CxWxH pode ser suficiente para a entrada conter uma sequência de imagens.


### O Controlador Padrão e a Stable-Baselines3

O framework Stable-baselines3 (SB3) é suportado por padrão pela AI4U. SB3 provê vários algoritmos de aprendizado por reforço. Um algoritmo deste tipo gera uma política, que determina para cada estado qual a ação a ser executada. 

Quando se usa um algoritmo da SB3, portanto, é necessário definir o tipo de política a ser gerada. Se o atuador atual da AI4U for discreto, por exemplo, um algoritmo como DQN é mais apropriado para este atuador. Já se o atuador for contínuo, algoritmos como PPO e SAC dão conta do recado.

Já se agente contém apenas um sensor com a chave "array", uma política do tipo MlpPolicy é suficiente para este agente. Já se o sensor for aquele com a chave "vision", uma político do tipo CnnPolicy é mais apropriada. Mas se há tanto um sensor "array" quanto um sensor "vision", uma política do tip MultiInputPolicy é mais apropriada.

### Enviando dados extras para o ambiente do agente
É possível adicionar dados extras a serem enviados ao agente quando uma ação é decidida. Podemos, por exemplo, enviar a função de Qvalue aprendida pelo algoritmo SAC:
```python

import ai4u
from ai4u.controllers import BasicGymController
import AI4UEnv
import gymnasium as gym
import numpy as np
from stable_baselines3 import SAC
from stable_baselines3.sac import MultiInputPolicy, MlpPolicy
from stable_baselines3.common.callbacks import CheckpointCallback
import torch

model = None

'''
Uma função de callback executada sempre que uma ação
é escolhida pelo agente.
'''
def step_callback(last_obs, action):
    ndaction = np.array([action])
    if model is not None:
        '''
        Executa processamento para frente no modelo para calcular a avaliação do estado atual (last_obs)
        de acordo com a função Q-Value.
        '''
        qvalue, _ = model.critic.forward(torch.from_numpy(last_obs).cuda(), torch.from_numpy(ndaction).cuda())
        qvalue = qvalue.cpu().detach().item() #extrai o elemento do device.
        BasicGymController.add_field('qvalue', qvalue) #adiciona um campo extra.

BasicGymController.step_callback = step_callback #executa depois que uma ação é decidida

env = gym.make("AI4UEnv-v0", rid='0', config=dict(server_IP='127.0.0.1', server_port=8080))

model = SAC(MlpPolicy, env, learning_starts=10000 tensorboard_log='tflog', verbose=1)

model.set_env(env)

print("Training....")
model.learn(total_timesteps=5000000, callback=checkpoint_callback, log_interval=5) #treinando durante cinco milhões de passos

model.save("sac1m") #salvando o modelo
print("Trained...")
del model # removendo o modelo da memória
print("Train finished!!!")
```

## Criando seus Próprios Controladores
Finalmente, é possível você criar seus próprios controladores. Para isso vai ter que criar uma classe que herda ou implementa a interface de *ai4u.agents.BasicController*. Leia a interface desta classe para se orientar como criar seu próprio controlador ou leia o tutorial sobre [como criar um controlador customizável](customcontroller.md).


## Considerações Finais
AI4U está sendo projetada para casos de uso que interessam aos seus criadores, contudo, está sendo desenvolvida de modo que possa ser facilmente adaptada para outros casos de uso. O controlador padrão é útil para casos de uso clássicos de aprendizagem por reforço. Se você quiser algo muito específico ou componentes (sensores e atuadores) que fogem à regra, experimente implementar um controlador que se adeque ao seu caso de uso.

















