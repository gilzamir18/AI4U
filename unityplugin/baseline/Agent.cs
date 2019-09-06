using UnityEngine;

namespace unityremote
{

    public abstract class Brain : MonoBehaviour
    {
        public static byte FLOAT = 0;
        public static byte INT = 1;
        public static byte BOOL = 2;
        public static byte STR = 3;
        public static byte OTHER = 4;
        protected string receivedcmd;
        protected string[] receivedargs;
        public abstract void SendMessage(string[] desc, byte[] tipo, string[] valor);
        
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
        public bool userControl = false;
        private string[] desc;
        private byte[] types;
        private string[] values;

        public virtual void ApplyAction()
        {
        }

        public void StartData()
        {
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

        public void SetStateAsInt(int i, string desc, int value)
        {
            this.desc[i] = desc;
            this.types[i] = Brain.INT;
            this.values[i] = value.ToString();
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

        public virtual bool UpdateState()
        {
            return false;
        }

        public virtual object[] LocalDecision()
        {
            return new object[] { };
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

