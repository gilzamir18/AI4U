using UnityEngine;
using System.Collections.Generic;

namespace ai4u
{

    public interface IAgentResetListener
    {
        void OnReset(Agent agent);
    }

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

    public abstract class Agent : MonoBehaviour
    {
        protected Brain brain;

        public int numberOfFields = 0;
        private string[] desc;
        private byte[] types;
        private string[] values;
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

        public void StartData()
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

        public void NotifyReset() {
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
            return System.Array.ConvertAll(this.brain.GetReceivedArgs()[i].Split(' '), float.Parse);
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
}

