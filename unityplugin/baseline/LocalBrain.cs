namespace unityremote
{
    public class LocalBrain : Brain
    {
        public Agent agent;
        public bool fixedUpdate = true;

        public void Start()
        {
            agent.StartData();
        }

        private void LocalDecision()
        {
            if (!agent.userControl)
            {
                object[] msg = agent.LocalDecision();
                receivedcmd = (string)msg[0];
                receivedargs = (string[])msg[1];
                agent.ApplyAction();
                agent.UpdatePhysics();
                agent.UpdateState();
                agent.GetState();
            } else
            {
                agent.UpdatePhysics();
            }
        }

        public void FixedUpdate()
        {
            if (fixedUpdate)
            {
                LocalDecision();
            }
        }

        public void Update()
        {
            if (!fixedUpdate)
            {
                LocalDecision();
            }
        }

        public override void SendMessage(string[] desc, byte[] tipo, string[] valor)
        {

        }
    }
}
