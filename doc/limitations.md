# Limitações Conhecidas

* Ainda não implementamos nenhuma sensor ou atuador para agentes 2D.
* Apenas suportamos o algoritmo SAC da stable-baselines3. O uso de qualquer outro algoritmo ou framework pode ser feito mediante alterações do desenvolvedor.
* O uso de arquivos ONNX diretamente dentro da Godot é limitado a redes MLP com uma ou muitas entradas. Não testamos o uso de imagens diretamente com redes convolucionais.
* O uso de imagens de alta resolução como parte da observação do agente não é recomendo, pois resulta em grande perda de desempenho.