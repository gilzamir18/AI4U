using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class BrainManager : MonoBehaviour
    {
        public RemoteBrain[] brainList;
        public bool autoConfiguration = true;
        public int startInputPort = 7070;
        public int stepInputPort = 1;
        public int startOuputPort = 8080;
        public int stepOutputPort = 1;

        public string remoteIP = "127.0.0.1";

        public bool acceptRemoteConfiguration = true;

        private bool runFirstTime = true;

        // Start is called before the first frame update
        void Awake()
        {
            if (runFirstTime && acceptRemoteConfiguration) {
                runFirstTime = false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--ai4u_inputport":
                            startInputPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_outputport":
                            startOuputPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_stepinputport":
                            stepInputPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_stepoutputport":
                            stepOutputPort = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_timescale":
                            Time.timeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            i += 2;
                            break;
                        case "--ai4u_remoteip":
                            remoteIP = args[i+1];
                            i += 2;
                            break;
                        case "--ai4u_targetframerate":
                            Application.targetFrameRate = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_vsynccount":
                            QualitySettings.vSyncCount = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        default:
                            i+=1;
                            break;
                    }
                }
            }

            if (autoConfiguration) {
                Configure();
            }
        }

        public void Configure() {
            int inputPort = startInputPort;
            int outputPort = startOuputPort;
            int stepIn = stepInputPort;
            int stepOut = stepOutputPort;


            foreach (RemoteBrain rb in brainList)
            {
                rb.remoteIP = remoteIP;
                rb.port = inputPort;
                rb.remotePort = outputPort;
                inputPort += stepIn;
                outputPort += stepOut;
            }
        }
    }
}
