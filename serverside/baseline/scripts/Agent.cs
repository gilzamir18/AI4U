using UnityEngine;
using System.Collections.Generic;

namespace ai4u
{


    ///<summary>
    ///An agent is an object that supports the cycle of updating the state 
    ///represented by the tuple (s[t], a, s[t + 1]), where s [t] is the current 
    ///state, s [t+1] is the next state and 'a' is the action taken that resulted 
    ///in s[t+1]. An agent receives an action or command from a controlle (instance of the Brain class),
    ///executes this action in the environment and returns to the controller the resulting 
    ///state named s[t+t1]. </summary>

    public abstract class Agent : MonoBehaviour
    {
        protected Brain brain;

        public int numberOfFields = 0;
        protected string[] desc;
        protected byte[] types;
        protected string[] values;
        private List<IAgentResetListener> resetListener = new List<IAgentResetListener>();
 
        public void AddResetListener(IAgentResetListener listener) 
        {
            resetListener.Add(listener);
        }

        public void RemoveResetListenerAt(int pos) {
            resetListener.RemoveAt(pos);
        }

        public bool RemoveResetListenerAt(IAgentResetListener listener) {
            return resetListener.Remove(listener);
        }

        /***
        This method receives client's command to apply to remote environment.
        ***/
        public virtual void ApplyAction()
        {
        }

        public virtual void StartData()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            desc = new string[numberOfFields];
            types = new byte[numberOfFields];
            values = new string[numberOfFields];
        }
        
        public void SetState(int i, string desc, byte type, string value)
        {
            this.desc[i] = desc;
            this.types[i] = type;
            this.values[i] = value;
        }

        public void SetStateAsFloatArray(int i, string desc, float[] value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.FLOAT_ARRAY;
            this.values[i] = string.Join(" ", value);
        }

        public void SetStateAsInt(int i, string desc, int value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.INT;
            this.values[i] = value.ToString();
        }

        public virtual void HandleOnResetEvent() {

        }

        public void NotifyReset() {
            this.HandleOnResetEvent();
            foreach (IAgentResetListener listener in resetListener) {
                listener.OnReset(this);
            }
        }

        public void SetStateAsFloat(int i, string desc, float value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.FLOAT;
            this.values[i] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        public void SetStateAsString(int i, string desc, string value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.STR;
            this.values[i] = value;
        }

        public void SetStateAsBool(int i, string desc, bool value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.BOOL;
            this.values[i] = value ? "1" : "0";
        }

        public void SetStateAsByteArray(int i, string desc, byte[] value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.OTHER;
            this.values[i] = System.Convert.ToBase64String(value);
        }

        private static float ParseFloat(string v) {
            return float.Parse(v, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public int GetStateIndex(string description)
        {
            for (int i = 0; i < this.desc.Length; i++) {
                if (desc[i] == description) 
                {
                    return i;
                }
            }
            return -1;
        }

        public string[] GetStateDescriptions()
        {
            return (string[])this.desc.Clone();
        }

        public byte GetStateType(int idx)
        {
            return this.types[idx];
        }

        public string GetStateValue(int idx)
        {
            return this.values[idx];
        }

        public string GetActionArgAsString(int i=0)
        {
            return this.brain.GetReceivedArgs()[i];
        }

        public float GetActionArgAsFloat(int i=0)
        {
            return float.Parse(this.brain.GetReceivedArgs()[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public bool GetActionArgAsBool(int i = 0)
        {
            return bool.Parse(this.brain.GetReceivedArgs()[i]);
        }

        public float[] GetActionArgAsFloatArray(int i = 0)
        {
            return System.Array.ConvertAll(this.brain.GetReceivedArgs()[i].Split(' '), ParseFloat);
        }

        public int GetActionArgAsInt(int i = 0)
        {
            return int.Parse(this.brain.GetReceivedArgs()[i]);
        }

        public string GetActionName()
        {
            return this.brain.GetReceivedCommand();
        }
        
        public virtual void UpdatePhysics()
        {

        }

        public virtual void UpdateState()
        {
        }

        public void GetState()
        {
            if (this.brain == null)
            {
                Debug.Log("Remote not found!");
                return;
            }
            try
            {
                this.brain.SendMessage(desc, types, values);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
                Debug.Log(e.StackTrace);
            }
        }

        public void SetBrain(Brain brain)
        {
            this.brain = brain;
        }
    }

    ///<summary>The Brain class communicates with the character's controller, that is, with the remote or 
    ///local mechanism it takes, selects the next action given the current state. This class does not fix
    /// any particular decision-making approach, but rather encapsulates a decision-making protocol that 
    ///allows the agent to be controlled remotely (code in programming languages ​​other than those supported by Unity)
    ///or locally (scripts that use languages ​​supported by Unity. Python is the naturally supported remote 
    ///scripting language. But others may be supported in the future. AI4U provides two instances of Brain. 
    ///One is a remote controller called RemoteBrain, which allows a remote controller to send 
    ///commands to an avatar. a local controller, which allows commands to be sent without using 
    /// network protocols. A local controller can be used to adapt the use of a trained model 
    /// using a remote controller. This is a possible scenario given that there are many algorithms
    ///and frameworks that they are easier for prototyping than with a Unity language.</summary>
    public abstract class Brain : MonoBehaviour
    {
        public static byte FLOAT = 0;
        public static byte INT = 1;
        public static byte BOOL = 2;
        public static byte STR = 3;
        public static byte OTHER = 4;
        public static byte FLOAT_ARRAY = 5;
        protected string receivedcmd; 
        protected string[] receivedargs;
        public abstract void SendMessage(string[] desc, byte[] tipo, string[] valor);

        public Agent agent = null;
        public bool fixedUpdate = true;
        public bool updateStateOnUpdate = false;

        public string GetReceivedCommand()
        {
            return receivedcmd;
        }


        public string[] GetReceivedArgs()
        {
            return receivedargs;
        }
    }

    public interface IAgentResetListener
    {
        void OnReset(Agent agent);
    }
}

