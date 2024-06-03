# Introdução

O objeto base de toda interação de agente com o ambiente é defindo pela classe ai4u.BasicAgent. Apesar desta classe implementar a classe abstrata ai4u.Agent, se você quiser criar uma classe própria de agente, é altamente recomendado que sua nova classe de agente herde de ai4u.BasicAgent. Exceto se você realmente sabe o que está fazendo. Podemos ser mais radicais e sugerir que você nunca tente criar uma classe de agente de sua autoria, toda personalização do framework deveria ser feita nos sensores, atuadores, controladores e funções de recompensa. Mas, ainda assim, se você quiser se arriscar, é possível (embora muito difícil) criar sua própria classe de agente sem quebrar as coisas. 

Temos desenvolvido vários projetos que mostram diferentes formas de personalização de agentes centrados na classe BasicAgent:

* [Projetos Básicos da AI4U](https://github.com/gilzamir18/ai4u_demo_projects)
    * BoxChase;
    * Platform2D;
    * Jumper (aqui temos o sistema de animação da Godot integrado com o agente).


Para suportar mais personalizações na AI4U, a classe BasicAgent provê um conjunto de eventos que permitem saber exatamente o que está acontecendo. Veja mais detalhes na nossa documentação sobre [eventos](eventsPTBR.md).