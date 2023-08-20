using UnityEngine;
namespace ai4u
{
    public class LocalBrain : Brain
    {
        public Controller controller;
        
        public void Awake()
        {
            if (!isEnabled)
            {
                return;
            }
            if (agent == null)
            {
                agent = GetComponent<Agent>();
            }
            if (controller == null) {
                Debug.LogWarning("You must specify a controller for the game object: " + gameObject.name);
            }

            if (agent == null) {
                Debug.LogWarning("You must specify an agent for the game object: " + gameObject.name);
            }

            agent.SetBrain(this);
            agent.Setup();
            controller.Setup(agent);
        }

        public string SendMessage(string[] desc, byte[] tipo, string[] valor)
        {
            controller.ReceiveState(desc, tipo, valor);
            return controller.GetAction();
        }
    }
}
