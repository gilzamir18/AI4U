# Problemas Conhecidos

## Problema: ValueError: unsupported pickle protocol: 5

A solução deste problema pode ser obtida de duas formas:

1) fazendo upgrade da versão do Python para uma versão que suporta o protocolog pickle 5:

```
pip install --upgrade python
```
.

2) Se não for possível atualizar a versão do interpretador python, instalar o protocolo especificado:

```
pip install pickle5
```
