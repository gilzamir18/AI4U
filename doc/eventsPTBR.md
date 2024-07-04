# Eventos

Um objeto do tipo ai4u.RLAgent provê um conjunto de eventos que permite você interceptar eventos relevantes que ocorrem durante o ciclo de vida do agente. Ao interceptar um evento, você pode, por exemplo, alterar um indicador de tempo na interface gráfica com o usuário ou mesmo realizar mudanças convenientes no ambiente. 

Se a opção *PhysicsMode* do nó *ControlRequestor*  estiver ativada, todo evento ocorre no laço de atualização de física do agente (corresponde a executar código no método *_PhysicsProcess*), caso contrário, a atualização ocorre no laço de renderização (equivalente a executar código no método *_Process*). Qualquer código ou comportamento que você queira inserir no agente ou no ambiente e que interfira na simulação do agente e do ambiente, deve ser inserido por meio de um controlador de evento. Por exemplo, digamos que você queira checar se uma colisão ocorreu com o agente. E você criou o método *HandleCollision(RLAgent)*, então você vai executar o seguinte código para dizer que este método deve ser executado no final de um passo de tempo:

```CSharp
agent.OnStepEnd += HandleCollision; 
```

onde *agent* é o objeto do tipo *RLAgent*. Neste caso, evite executar a verificação de colisão diretamente no método *_PhysicsProcess*.

Os eventos suportados por um objeto *RLAgent* são:

* OnResetStart: executado no ínicio de um episódio, antes de qualquer método OnReset ser executado.
* OnEpisodeEnd: executado quando no final de um episódio.
* OnEpisodeStart: executado no início de um episódio.
* OnStepEnd: executado no final de um passo-de-tempo.
* OnStepStart: executado no início de um passo-de-tempo.
* OnStateUpdateStart: executado no início de um atualização de estado.
* OnStateUpdateEnd: executado no fim de uma atualização de estado.
* OnActionStart: executado antes de todas as ações do passo-de-tempo atual serem executadas.
* OnActionEnd: executado depois que todas as ações do passo-de-tempo atual forem executadas.
* OnAgentStart: executado durante a inicialização do agente.


Todos os controladores destes eventos são do tipo AgentEpisodeHandler:

```CSharp
       /// <summary>
        /// Delegate for handling agent episode events.
        /// </summary>
        /// <param name="agent">The agent that triggered the event.</param>
        public delegate void AgentEpisodeHandler(RLAgent agent);
```
.

