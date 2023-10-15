# Convenções

[Sumário](summary.md)

* Atuadores e sensores para agentes 2D terminam com o sufixo 2D. Por exemplo, TouchRewardFunc2D é uma função de recompensa para um agente que tem um corpo RigidBody2D.
* Atuadores e sensores para agentes que possuem um RigidBody3D não possuem um sufixo especial indicando a dimensionalidade do corpo do agente.
* Interpretamos que o corpo de  um agente em Godot é um objeto físico. Assim, o jogador não tem controle sobre o agente (que age como um NPC). Portanto, de acordo com essa interpretação embasada na [documentação oficial da Godot](https://docs.godotengine.org/en/stable/tutorials/physics/physics_introduction.html), faz todo sentido implementar o corpo de um agente 3D como um RigidBody3D.