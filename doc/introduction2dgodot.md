# Princípios Gerais de Modelagem de Jogos 2D para AI4U na Godot

[Sumário](summary.md)

## Introdução
Criar um jogo ou um experimento baseado em tecnologia de jogo pode ser desafiador diante da grande quantidade de opções que um motor de jogos proporciona. Podemos identificar diversas opções de modelagem disponíveis e escolher aquelas mais adequadas para o propósito que se quer. Especificamente para experimentação de Inteligência Artificial (IA) ou mesmo para criação de personagens autônoms em jogos, usar o máximo o motor de física traz certas vantagens. Nossa diretriz para modelar um personagem que vai ser treinado com aprendizado por reforço usando AI4U é que este personagem tenha um corpo rígido (RigidBody2D). Isso parece contrasensual com a maioria dos tutorais e com a própria documentação da Godot, que usam um CharacterBody2D para criar o avatar do jogador. Mas, observe, não vamos criar um objeto controlado por um humano, mas um sim um objeto controlado por uma IA. Usar um RigidBody2D significa deixar a física por conta do motor de jogos.

Neste tutorial, vou focar na modelagem de um cenário simples para experimentação e nos componentes deste cenário. Vamos modelar uma cena como se fosse de um nível inicial de um jogo sem nos preocuparmos agora em inserir AI4U no processo. No próximo tutorial, modificaremos esta cena para incluirmos AI4U.

# Criando o Cenário 2D
Nosso cenário é bidimensional, vamos chamá-lo de CrocoBoy. O CrocoBoy é um garoto preso em um bosque  e que precisa se manter vivo até a chegama de um helicoptero, o que demora alguns minutos. No bosque, surge uma ameaça espinhenta, o Spiker. O CrocoBody pode comer frutas vermelhas que aumentam sua energia. Há dimantes vermelhos que aumentam o poder do CrocoBody. Quando ele tem poderes, pode atacar o Spiker. Ao atacar o Spiker e destruí-lo, o CrocoBody também ganha energia. Se o CrocoBoy atacar o Spiker sem ter poder para isso, ele morre. Atacar aqui significa apenas se aproximar e tocar. Se o Spiker morrer, ele renasce algum tempo depois.

O objetivo do CrocoBoy é se manter vivo até a chegada do helicóptero. Quando o helicoptero chega, o CrocoBoy vence. Se ele morre antes disso, ele perde o jogo. Uma medida de desempenho que pode ser usada neste caso é o tempo que o CrocoBoy se mantém vivo.

# Identificando os Objetos e Itens do Jogo
Um passo importante para a modelagem bem-sucedida de uma cena de jogo é identificar adequadamente todos os itens e objetos. 

O *CrocoBoy* é o personagem principal e vai se movimentar muito na cena. O movimento do CrocoBoy será por meio de IA, portanto, pode-se usar um RigidBody2D para representar o corpo do personagem. Além disso, este personagem tem alguns movimentos e ações básicos predefinidos. Implementamos estas ações na classe [BotController](../demo2d/BotController.cs). Observe que esta classe herda de RidigBody2D. Além disso, a classe *BotController* não possui nenhum acoplamento aparente com a AI4U, mas a presença dos métodos *Reset* e *SetAction(int)* é uma convenção para o atuator *DiscretActuator*, que irá manipular este controlador por meio destes métodos.

O Spiker é um personagem menos inteligentes, movendo-se da esquerda para a direita e da direita para a esquerda, sem parar. Implementamos a classe [SpikeController](../demo2d/SpikeController.cs) que representa um controlador muito simples para o **Spiker**. Poderíamos ter utilizados qualquer tipo de corpo (StaticBody2D, CharacterBody2D, etc), mas usar RigidBody2D para este personagem facilitou a implementação do movimento do personagem, pois com isso apenas precisamos definir a força necessária para o personagem se movimentar da esquerda para a direita e vice-versa.

Os outros itens do jogo são a fruta e o diamante, que são implementados como objetos do tipo Node2D, pois são objetos estáticos durante o jogo e precisamos apenas calcular a colisão destes itens com o personagem principal.

Usou-se sprites para dar vida aos personagens. O gerenciamento de sprites foi todo feito por meio do objeto *AnimatedSprite2D*.

Agora, siga o tutorial [*Introdução a AI4U 2D*](introduction2dgodotwithAI4U.md) para saber como adicionar IA ao CrocoBoy.