# Rodando um controlador baseado em ONNX diretamente na Godot
O ONNX (Open Neural Network Exchange) é um formato aberto para representar modelos de aprendizado de máquina. Ele permite que os modelos sejam treinados em um framework e consumidos em outro, proporcionando interoperabilidade entre diferentes frameworks.

No tutorial de [Introdução](introduction.md), mostramos como criar uma cena simples e treinar um agente para esta cena usando pytorch e python. No final, mostramos como rodar o modelo, mas usando python e comunicação remota entre o script python e a cena desenvolvida na Godot. Agora, vamos mostrar que o modelo gerado pelo nosso script python baseado em pytorch pode ser executado diretamente na Godot sem precisar da intermediação do pytorch. Para isso, usamos nossos scripts da ferramenta [bemaker](https/github.com/gilzamir18/bemaker) de converção do modelo pytorch no formato ONNX.

Para rodar a rede neural treinada diretamente na Godot usando AI4U, há duas etapas. Primeiramente, precisamos converter a rede neural treinada em ONNX. Depois, devemos usar um controlador baseado em rede neural disponível na AI4U. Este controlador depende de dois componentes externos: Microsoft.ML e Microsoft.ML.OnnxRuntime. Você pode instalar estes componentes diretamente via terminal usando o utilitário de linha de comando *dotnet* com o comando:

> dotnet add package <nomedopacote>

Vamos usar como exemplo o projeto desenvolvido para o tutorial de [Introdução](introduction.md). Este projeto pode ser baixado [aqui](https://1drv.ms/u/s!AkkX5pv0cl3aieYYTQz_d9S1kVhJAQ?e=rlCgnh).

