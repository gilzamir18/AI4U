# Convenções

[Sumário](summary.md)

* Atuadores e sensores para agentes 2D terminam com o sufixo 2D. Por exemplo, TouchRewardFunc2D é uma função de recompensa para um agente que tem um corpo RigidBody2D.
* Atuadores e sensores para agentes que possuem um RigidBody3D não possuem um sufixo especial indicando a dimensionalidade do corpo do agente.
* Interpretamos que o corpo de  agentes em Godot são objetos físicos, que o jogador não tem controle sobre, portanto, são como corpos rígidos (que há uma física implementada sobre eles) e que o próprio agente controla as forças aplicadas a este corpo rígido. Essa interpretação á baseada na [documentação oficial da Godot](https://docs.godotengine.org/en/stable/tutorials/physics/physics_introduction.html).