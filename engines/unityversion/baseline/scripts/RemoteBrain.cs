﻿using UnityEngine;
using System.Text;

namespace ai4u
{
    ///
    /// <summary>This class defines a remote controller for an agent of type Agent of the AI4U.
    /// Estes agentes recebem comandos de um script por meio de uma interface de comunicação em rede.
    /// So, Brain is a generic controller, awhile RemoteBrain implements an agent's network controller.
    /// </summary>
    public class RemoteBrain : Brain
    {   
        ///<summary>If true, the remote brain will be 
        ///managed manually. Thus, in this case, command 
        ///line arguments do not alter the properties of 
        ///the remote brain.</summary>
        public bool managed = false;

        private string cmdname; //It's more recently received command/action name.
        private string[] args; //It's more recently command/action arguments.
        private float timeScale = 1.0f; //unity controll of the physical time.
        private bool runFirstTime = false;

        private ControlRequestor controlRequestor;

        void Awake(){
            controlRequestor = GetComponent<ControlRequestor>(); 
            if (controlRequestor == null)
            {

                Debug.LogWarning("No ControlRequestor component added in RemoteBrain component.");
            }

            //one time configuration
            if (!managed && runFirstTime){
                runFirstTime =false;
                string[] args = System.Environment.GetCommandLineArgs ();
                int i = 0;
                while (i < args.Length){
                    switch (args[i]) {
                        case "--ai4u_port":
                            if (controlRequestor != null) controlRequestor.port = int.Parse(args[i+1]);
                            i += 2;
                            break;
                        case "--ai4u_timescale":
                            this.timeScale = float.Parse(args[i+1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                            Time.timeScale = this.timeScale;
                            i += 2;
                            break;
                        case "--ai4u_host":
                            if (controlRequestor != null) controlRequestor.host = args[i+1];
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
            if (agent == null) {
                Debug.LogWarning("You have not defined the agent that the remote brain must control. Game Object: " + gameObject.name);
            }
            agent.SetBrain(this);
            agent.Setup();
        }

        public ControlRequestor ControlRequestor
        {
            get
            {
                return controlRequestor;
            }
        }

    }
}