# Princípios Gerais de Modelagem de Jogos 2D para AI4U na Godot

[Sumário](summary.md)

## Introdução

Criar um jogo ou um experimento baseado em tecnologia de jogo pode ser desafiador diante da grande quantidade de opções que um motor de jogos proporciona. Podemos identificar de modelagem disponíveis e escolher aquelas mais adequadas para o propósito que se quer. Especificamente para experimentação de Inteligência Artificial (IA) ou mesmo para criação de personagens autônoms em jogos, usar o máximo o motor de física traz certas vantagens. Nossa diretriz para modelar um personagem que vai ser treinado com aprendizado por reforço usando AI4U é que este personagem tenha um corpo rígido (RigidBody2D). Parece contrasensual com a maioria dos tutorais e com a própria documentação da Godot que usam um CharacterBody2D para criar o avatar do jogador. Mas, observe, não vamos criar um objeto controlado por um humano, mas um sim um objeto controlado por uma IA. Usar um RigidBody2D significa deixar a física por conta do motor de jogos.

Neste tutorial, vou focar na modelagem de um cenário simples para experimentação e nos componentes deste cenário. Vamos modelar uma cena como se fosse de um nível inicial de um jogo sem nos preocuparmos agora em inserir AI4U no processo. No próximo tutorial, modificaremos esta cena para incluirmos AI4U.

# Criando o Cenário 2D

Nosso cenário 2D é muito simples, vamos chamálo de CrocoBoy. O CrocoBoy é um garoto preso em um bosque  e que precisa se manter vivo até a chegama de um helicoptero, o que demora alguns minutos. No bosque, surge uma ameaça espinhenta, o Spike. O CrocoBody pode comer frutas vermelhas que aumentam sua energia. Há dimantes vermelhos que aumentam o poder do CrocoBody. Quando ele tem poderes, pode atacar o Spike. Ao atacar o Spike e destruí-lo, o CrocoBody também ganha energia. Se o CrocoBoy atacar o Spike sem ter poder para isso, ele morre. Atacar aqui significa apenas se aproximar e tocar. Se o Spike morrer, ele renasce algum tempo depois.

O objetivo do CrocoBoy é se manter vivo até a chegada do helicóptero. Quando o helicoptero chega, o CrocoBoy vence. Se ele morre antes disso, ele perde o jogo. Uma medida de desempenho que pode ser usada neste caso é o tempo que o CrocoBoy se mantém vivo.



