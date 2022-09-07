using System.Text;

namespace ai4u
{

    public class RequestCommand 
    {
        private string[] commands;
        private byte[] types;
        private string[] values;

        public string[] Command
        {
            get
            {
                return commands;
            }
        }

        public byte[] Type 
        {
            get
            {
                return types;
            }
        }

        public string[] Value 
        {
            get
            {
                return values;
            }
        }

        public RequestCommand(int size)
        {
            commands = new string[size];
            types = new byte[size];
            values = new string[size];
        }
        
        public void SetMessage(int i, string cmd, byte type, string value)
        {
            commands[i] = cmd;
            types[i] = type;
            values[i] = value;
        }
        
        public void SetMessage(int i, string cmd, byte type, int value)
        {
            SetMessage(i, cmd, type, "" + value);
        }

        public void SetMessage(int i, string cmd, byte type, float value)
        {
            SetMessage(i, cmd, type, "" + value);
        }

        public void SetMessage(int i, string cmd, byte type, bool value)
        {
            SetMessage( i, cmd, type, "" + (value ? "1": "0") );
        }
    }

    public class Utils
    {
        private Utils()
        {

        }

        private static float ParseFloat(string v) {
            return float.Parse(v, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public static int GetActionArgAsInt(string arg)
        {
            return int.Parse(arg);
        }

        public float GetActionArgAsFloat(string arg)
        {
            return float.Parse(arg, System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
        }

        public bool GetActionArgAsBool(string arg)
        {
            return bool.Parse(arg);
        }

        public float[] GetActionArgAsFloatArray(string arg)
        {
            return System.Array.ConvertAll(arg.Split(' '), ParseFloat);
        }

        public static string ParseAction(string actionName, float[] args)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(actionName);
            sb.Append(";1;");
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" ");
                }
                sb.Append(args[i]);
            }
            return sb.ToString();
        }

        public static string ParseAction(string actionName)
        {
            return ";" + actionName + ";0;";
        }
    }
}
