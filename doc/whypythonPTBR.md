# Por que Python?

O fluxo padrão de projeto de um experimento usando AI4U é: 

1. modelar o agente e o ambiente na Godot usando o *addon* *dotnet* da AI4U.
2. Escrever o script python para treinar o agente.
3. Escrever um script python para testar o agente.
4. Se o desempenho em teste for aceitável, converter o modelo do agente do pytorch em onnx e rodar o modelo diretamente na Godot. Caso contrário, refazer as etapas 1, 2 e 3.

Este fluxo seria mais simples se tudo rodasse dentro da Godot, o que pode levar muitos a se perguntarem: por que a AI4U não roda o treinamento diretamente em C# ou GDScript na Godot? A verdade é que as implementações de DRL (Deep Reinforcement Learning) em Python já estão por aí um bom tempo e, naturalmente, são bastante robusta. Fora o ecosistema completo de bibliotecas auxiliares para as mais diversas tarefas que podem ser úteis durante o treinamento. Além disso, Python pode ser usado durante o treinamento e, depois, descartado.

Ainda não há uma implementação robusta de DRL em GDScript. Em C#, apesar de termos o TorchSharp, ainda não há uma implementação tão completa de DRL quanto, digamos, a *stable-baselines3*. Estou implementando um protótipo com, por enquanto, apenas o algoritmo PPO, mas ainda muito rudimentar e não suporta metadade das features encontrada em implementações como a stable-baselines3. Portanto, Python ainda possui vantagens competitivas e o esforço de ligar python com C# é muito amplo. Uma motivação extra é que substituir o módulo C# da Godot por GDScript se comunicando com Python seria muito mais fácil do que implementar todos os módulos diretamente em GDScript. Quando eu tiver tempo, implementarei uma versão completa da AI4U toda em GDScript se comunicando com Python. Usando extensão da Godot, é possível usar alguma biblioteca dinâmica para ler e executar um modelo ONNX. Se alguém quiser contribuir com isso, fale comigo em: [aqui](gilzamir_gomes@uvanet.br).