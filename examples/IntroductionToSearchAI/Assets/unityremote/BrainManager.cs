using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace unityremote
{
    public class BrainManager : MonoBehaviour
    {
        public RemoteBrain[] brainList;
        public int startInputPort = 7070;
        public int stepInputPort = 1;
        public int startOuputPort = 8080;
        public int stepOutputPort = 1;

        // Start is called before the first frame update
        void Awake()
        {
            int inputPort = startInputPort;
            int outputPort = startOuputPort;
            int stepIn = stepInputPort;
            int stepOut = stepOutputPort;

            foreach (RemoteBrain rb in brainList)
            {
                rb.port = inputPort;
                rb.remotePort = outputPort;
                inputPort += stepIn;
                outputPort += stepOut;
            }
        }
    }
}
