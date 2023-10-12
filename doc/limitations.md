# Limitações Conhecidas

[Sumário](summary.md)

* Ainda não implementamos nenhuma sensor ou atuador para agentes 2D.
* Apenas suportamos o algoritmo SAC da stable-baselines3. O uso de qualquer outro algoritmo ou framework pode ser feito mediante alterações do desenvolvedor.
* A versão atual da AI4U apenas suporta ou foi testada com dois tipos de política da stable-baselines3: MlpPolicy e MultiInputPolicy.
* O uso de arquivos ONNX diretamente dentro da Godot é limitado a redes MLP com uma ou muitas entradas. Não testamos o uso de imagens diretamente com redes convolucionais.
* O uso de imagens de alta resolução como parte da observação do agente não é recomendo, pois resulta em grande perda de desempenho.
* Desempenho: o desempenho padrão está muito baixo, obtendo somente 5 fps durante o treinamento do agente (segundo métrica da stable-baselines3). Para melhorar este desempenho, procure as opções em **Project --> Project Settings --> Physics --> Common**  e  altere o valor da propriedade **Physics Ticks per Second** para um valor compatível com o hardware que você tem disponível. Em um notebook gamer com um processador intel da décima primeira geração, placa de vídeo GForce 1650 com 4GB VRAM e 25 GB de RAM, configurei esta propriedade com o valor 320 e obtive 30fps durante a fase de treinamento.