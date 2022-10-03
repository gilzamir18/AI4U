using Godot;
using System.Text;

namespace ai4u
{
	public abstract class Controller: Node
	{
		private string[] desc;
		private byte[] type;
		private string[] value;


		public Controller(): base()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
		}


		public virtual string GetAction()
		{
			return "";
		}


		public virtual void NewStateEvent()
		{
		}
		
		//----------------------
		public string GetStateAsString(int i = 0)
		{
			return value[i];
		}

		public int GetStateSize()
		{
			return this.desc.Length;
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
