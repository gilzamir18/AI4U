using System.Text;
using System;
using System.Collections.Generic;
using Godot;

namespace ai4u
{


	public struct Ray
	{
		private Vector3 origin;
		private Vector3 direction;
		private Vector3 endPoint;
		
		public Ray(Vector3 o, Vector3 d)
		{
			this.origin = o;
			this.direction = d.Normalized();
			this.endPoint = origin + direction;
		}
		
		public Vector3 Origin
		{
			get
			{
				return origin;
			}
		}
		
		public Vector3 Direction
		{
			get
			{
				return direction;
			}
		}
		
		public Vector3 EndPoint
		{
			get
			{
				return endPoint;
			}
		}
		
		public float GetDist(Vector3 q)
		{
			return (q - origin).Length();
		}
	}

	public partial class RequestCommand 
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

	public sealed class Utils
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
			return System.Array.ConvertAll(arg.Split(';'), ParseFloat);
		}

		public static string ParseAction(string actionName, float[] args)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(actionName);
			sb.Append(";");
			sb.Append("" + args.Length);
			sb.Append(";");
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
				{
					sb.Append(";");
				}
				sb.Append(args[i]);
			}
			return sb.ToString();
		}

		public static string ParseAction(string actionName, int[] args)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(actionName);
			sb.Append(";");
			sb.Append("" + args.Length);
			sb.Append(";");
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
				{
					sb.Append(";");
				}
				sb.Append(args[i]);
			}
			return sb.ToString();
		}

		public static string ParseAction(string actionName)
		{
			return actionName + ";0";
		}
		
		public static string ParseAction(string actionName, int value)
		{
			return actionName + ";1;" + value;
		}

		public static string ToPythonTuple(int[] a)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("(");
			for (int i = 0; i < a.Length; i++)
			{
				sb.Append(a[i]);
				sb.Append(",");
			}
			sb.Append(")");
			return sb.ToString();
		}
    }

	public class MaxSizeQueue<T>
    {
        private Queue<T> queue;
        private int maxSize;

        public MaxSizeQueue(int maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentException("Max size must be greater than zero.", nameof(maxSize));

            this.queue = new Queue<T>();
            this.maxSize = maxSize;
        }

        public void Enqueue(T item)
        {
            if (queue.Count >= maxSize)
                queue.Dequeue(); // Remove the oldest item if the queue is full

            queue.Enqueue(item);
        }

        public T Dequeue()
        {
			return queue.Dequeue();
        }

		public T[] Values
		{
			get
			{
				T[] values = new T[maxSize];
				var i = 0;
				foreach (var v in this.queue)
				{
					values[i] = v; i++;
				}
				return values;
			}
		}


        public int Count => queue.Count;
    }

    public partial class HistoryStack<T>
	{
		private T[] values;
		private int pos;
		private int capacity;

		public HistoryStack(int capacity)
		{
			this.capacity = capacity;
			values = new T[capacity];
			pos = 0;
		}

		public int Capacity
		{
			get 
			{
				return capacity;
			}
		}

		public void Push(T item)
		{
			values[pos] = item;
			pos ++;
			if (pos >= Capacity)
			{
				pos = 0;
			}
		}

		public T[] Values 
		{
			set
			{
				for (int i = 0; i < value.Length; i++)
				{
					Push(value[i]);
				}
			}

			get
			{
				T[] copy = new T[capacity];
				int k = 0;
				int p = pos;
				while (k < capacity)
				{
					copy[k] = values[p];
					p++;
					if (p >= capacity)
					{
						p = 0;
					}
					k++;
				}
				return copy;
			}
		}
	}
}
