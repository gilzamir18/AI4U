using UnityEngine;


namespace ai4u
{
    public abstract class Controller: MonoBehaviour
    {
        private string[] desc;
        private byte[] type;
        private string[] value;


        public Controller(): base()
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        }


        public virtual object[] GetAction()
        {
            return new Object[] { };
        }


        public virtual void NewStateEvent()
        {
        }

        public object[] GetFloatArrayAction(string action, float[] value)
        {
            return new object[] { action, new string[] { string.Join(" ", value) } };
        }

        public object[] GetIntAction(string action, int value)
        {
            return new object[] { action, new string[] { value.ToString() } };
        }

        public object[] GetFloatAction(string action, float value)
        {
            return new object[] { action, new string[] { value.ToString(System.Globalization.CultureInfo.InvariantCulture) } };
        }

        public object[] GetStringAction(string action, string value)
        {

            return new object[] {action, new string[] { value }};
        }

        public object[] GetBoolAction(string action, bool value)
        {
                return new object[] { action, new string[] { value ? "1" : "0" } };
        }

        public object[] GetByteArrayAction(string action, byte[] value)
        {
            return new object[] { action, new string[] { System.Convert.ToBase64String(value) }};
        }
        
        //----------------------
        public string GetStateAsString(int i = 0)
        {
            return value[i];
        }

        public float GetStateAsFloat(int i = 0)
        {
            return float.Parse(value[i], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public bool GetStateAsBool(int i = 0)
        {
            return bool.Parse(value[i]);
        }

        public string GetStateName(int i = 0)
        {
            return desc[i];
        }

        public float[] GetStateAsFloatArray(int i = 0)
        {
            return System.Array.ConvertAll(value[i].Trim().Split(' '), float.Parse);
        }

        public int GetStateAsInt(int i = 0)
        {
            return int.Parse(value[i]);
        }

        public void ReceiveState(string[] desc, byte[] type, string[] val)
        {
            this.type = type;
            this.value = val;
            this.desc = desc;
            this.NewStateEvent();
        }
    }
}
