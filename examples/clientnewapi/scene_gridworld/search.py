import ai4u
from controller import SimpleController
from ai4u.appserver import startasdaemon
import numpy as np
import time
from bisect import insort

FLOOR = 1
WALL = 50
ENEMY = 100
NPC = 150
DIRTY = 200
OTHER = 255
map = {'W': 0, 'S': 1, 'L': 2, 'A': 3, 'D': 4, 'T': 5, 'P': 6, 'R': 7, 'U': 8}
class Node:
    def __init__(self, state, goal, action = None, parent=None, stepcost=0):
        self.cost = stepcost

        if parent is not None:
            self.cost += parent.cost
        self.state = state
        self.parent = parent
        self.action = action
        self.goal = goal 

    def fcost(self):
        return (self.state[0] - self.goal[0])**2 + (self.state[1] - self.goal[1]) ** 2

    def __gt__(self, other):
        return self.fcost() > other.fcost()
    
    def __ge__(self, other):
        return self.fcost() >= other.fcost()

    def __lt__(self, other):
        return self.fcost() < other.fcost()

    def __le__(self, other):
        return self.fcost() <= other.fcost()

# Identificador do agente na Unity.
# Todo agente está associado a um
# processo de comunicação. Pode-se
# especificar vários identificadores,
# um para cada agente. Neste exemplo,
# temos apenas um agente com identificador 0 (zero).
ids = ["0"] 

# Classes que irão estabelecer protocolo de comunicação com a Unity.
# Deve ser especificada uma classe para cada agente. Como só há
# um agente neste  exemplo, apenas uma classe é necessária.
# Para visualizar a clase SimpleController, veja o arquivo
# controller.py.
controllers_classes =  [SimpleController] 

# Inicializa o processo de comunicação
# entre os agentes especificados e a Unity.
controller = startasdaemon(ids, controllers_classes)[0] 

# Sempre que quiser reiniciar o ambiente,
# execute o método request_reset do controlador.
# este método retorna o estado inicial do ambiente.
# Você pode especificar um parâmetro goalDist que indica
# a menor distância válida para se colocar a sujeira.
# Quando se usa o parâmetro goalDist, apenas um local
# de sujeira é gerado. Observe que os parâmetros são
# enviados para a Unity e o formato deve ser este do
# exemplo, ou seja, uma lista de tuplas (acao, argumento).
state = controller.request_reset([("goalDist", 3)]) 

# Neste gridworld, o estado inicial deve ser
# interpretado como uma matriz 20x20, cada
# posição da matriz tem o código do objeto 
# que ocupa esta posição.
grid = np.reshape(state['vision'], (20, 20) )

# A posição do agente (NPC) tem
# código NPC. O chamada a np.where
# como colocada abaixo,
# retorna as posições com este código.
# Como o agente somente pode ocupar uma posição,
# uma única posição é retornada.
pos = np.where(grid == NPC)

# np.where retorna uma sequência para cada dimensão.
# As dimensões então em uma tupla (D1, D2, ..., DN),
# onde Di é uma lista de coordenadas da i-ésima dimensão.
# Como temos uma matriz de duas dimensões e apenas
# um resultado possível, podemos recuperar manualmente
# cada dimensão e "espremer" a lista de forma a remover
# qualquer aninhamento, garantindo que teremos um escalar
# por dimensão.
start_pos = ( int(pos[0].squeeze()), int(pos[1].squeeze()) )
objetivos_testados = {}

def next_goal(state, grid):
    """O objetivo do agente é encontrar a
    posição do jogo que tenha sujeira na
    vizinhança e que seja mais próxima 
    de sua posição atual.
    Uma vez encontrada a posição alvo, o
    agente tem que executar as ações para lá chegar
    e finalmente executar a ação limpar.
    Parâmetros:
    state: posição a partir do qual se quer achar o caminho.
    grid: o mapa atual do mundo.
    """
    s = grid.shape
    minDist = 801
    minC = -1
    minL = -1

    for i in range(s[0]):
        for j in range(s[1]):
            if (i, j) not in objetivos_testados:
                achou_sujeira = False
                vizinhos = get_neighborhood(i, j, grid)
                for c, l in vizinhos:
                    if grid[c, l] == DIRTY:
                        achou_sujeira = True
                        break
                if achou_sujeira:
                    d = (i - state[0])**2 + (j - state[1])**2
                    if d < minDist:
                        minDist = d
                        minC = i
                        minL = j
    return (minC, minL)

def get_neighborhood(x, y, grid):
    """
    Retorna todos os vizinhos da posição (x, y),
    onde x é a coluna, y é a linha e grid
    é a matriz que representa o mapa do ambiente.
    """
    vizinhos = []
    # vizinho da esquerda
    v1x = x-1
    v1y = y
    if v1x >= 0: #se existir
        vizinhos.append((int(v1x), int(v1y)))

    # vizinho da direita
    v2x = x+1
    v2y = y
    if v2x < 20: #se existir
        vizinhos.append((int(v2x), int(v2y)))
    
    #vizinho de cima
    v3x = x
    v3y = y - 1 
    if v3y >= 0: #se existir
        vizinhos.append((int(v3x), int(v3y)))

    #vizinho de baixo
    v4x = x
    v4y = y + 1
    if v4y < 20: #se existir
        vizinhos.append((int(v4x), int(v4y)))
    return vizinhos

# Observe que a função de transição não altera
# efetivamente o mundo, mas sim uma representação
# do estado do mundo. Isso significa que o projeto
# de um agente de busca deve saber exatamente como o mundo
# funciona. Este é o principal problema desta abordage uma
# vez que descrever a maneira como funciona é extremamente
# complexo em problemas do mundo real.
def next_state(action, state, grid):
    """
    Calcula o próximo estaod se a ação 'action' for executada.
    state = estado atual, tal que (state[0], state[1]) é a 
    posição do agente.
    grid = o mapa do ambiente. 
    """
    neighborhood = get_neighborhood(state[0], state[1], grid)
    #print(neighborhood)
    if action == 'A':
        x = state[0] - 1
        y = state[1]
        if (x, y) in neighborhood and int(grid[x, y]) == FLOOR:
            return (x, y)
        else:
            return state
    elif action == 'D':
        x = state[0] + 1
        y = state[1]
        if (x, y) in neighborhood and int(grid[x, y]) == FLOOR:
            return (x, y)
        else:
            return state
    elif action == 'W':
        x = state[0]
        y = state[1] + 1
        if (x, y) in neighborhood and int(grid[x, y]) == FLOOR:
            return (x, y)
        else:
            return state
    elif action == 'S':
        x = state[0]
        y = state[1] - 1
        if (x, y) in neighborhood and int(grid[x, y]) == FLOOR:
            return (x, y)
        else:
            return state

state = start_pos

actions = ['W', 'A', 'S', 'D']
falha = True
tentativas = 0
while falha: #Vai tentar encontrar um caminho até o objetivo durante 100 tentativas.
    goal = next_goal(state, grid) 
    if goal is None: #se não encontrou o objetivo, continua tentando
        falha = True
        continue
    tentativas += 1
    if tentativas >= 100:
        break
    vizinhos = get_neighborhood(goal[0], goal[1], grid)
    visitados = {}

    print("vizinhos ", vizinhos)
    print("Goal: ", goal)
    print("Initial State: ", state)
    if goal is None:
        print("Goal not found!")
    else:
        print("Initial State: ", state)
        print("Goal ", goal)
        visitados[state] = True
        frontier = [Node(state, goal)]
        current = None
        success = False
        while len(frontier) > 0:
            current = frontier.pop(0)
            print(current.state)
            if current.state == goal:
                success = True
                print("Goal? ", current.state)
                break
            for action in actions:
                nextstate = next_state(action, current.state, grid)
                if nextstate is not None and (nextstate not in visitados):
                    insort(frontier, Node(nextstate, goal, action, current, 1))
                    visitados[nextstate] = True

        if not success:
            print("Search Fail.")
            objetivos_testados[goal] = True
            falha = True
        else:
            falha = False
            c = current
            actions = []
            while c.parent is not None:
                actions.append(c.action)
                c = c.parent
            actions = reversed(actions)

            for action in actions:
                time.sleep(1)
                controller.request_step(map[action])
            controller.request_step(map['L'])
            break
            print(actions)

time.sleep(120)
