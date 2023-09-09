using System.Collections;
using System.Collections.Generic;
using Godot;

namespace ai4u
{
	public enum SensorType {
		sfloat,
		sstring,
		sbool,
		sint,
		sbytearray,
		sfloatarray,
		sintarray
	}

	public interface ISensor: IAgentResetListener
	{
		public void SetAgent(BasicAgent own);
		public void OnSetup(Agent agent);
		public float GetFloatValue();
		public string GetStringValue();
		public bool GetBoolValue();
		public byte[] GetByteArrayValue();
		public int GetIntValue();
		public int[] GetIntArrayValue();
		public float[] GetFloatArrayValue();
		public SensorType GetSensorType();
		public string GetName();
		public string GetKey();
		public int[] GetShape();
		public bool IsState();
		public bool IsResetable();
		public bool IsActive();
		public bool IsInput();
		public int GetStackedObservations();
		public void SetKey(string newkey);
		public void SetShape(int[] newshape);
		public void SetIsActive(bool v);
		public void SetIsInput(bool v);
		public void SetStackedObservations(int so);
		public void SetSensorType(SensorType t);
		public float GetRangeMin();
		public float GetRangeMax();
		public void SetName(string name);
		public void SetRange(float min, float max);
		public void SetIsResetable(bool v);
	}

	public abstract class AbstractSensor: ISensor
	{
		private SensorType type;
		private int[] shape;
		private BasicAgent agent;
		private string key;
		private string name;
		private int stackedObservations = 1;
		private bool isActive = true;
		private bool isInput = false;
		private bool isState = false;
		private bool resetable = true;
		private float rangeMin = 0;
		private float rangeMax = 1;
		
		public void SetAgent(BasicAgent own)
		{
			this.agent = own;
		}

		public virtual void OnSetup(Agent agent)
		{
			this.agent = (BasicAgent) agent;
		}

		public virtual float GetFloatValue() {
			throw new System.NotSupportedException();
		}

		public virtual string GetStringValue() {
			throw new System.NotSupportedException();
		}

		public virtual bool GetBoolValue() {
			throw new System.NotSupportedException();
		}

		public virtual byte[] GetByteArrayValue() {
			throw new System.NotSupportedException();
		}

		public virtual int GetIntValue() {
			throw new System.NotSupportedException();
		}

		public virtual int[] GetIntArrayValue() {
			throw new System.NotSupportedException();
		}

		public virtual float[] GetFloatArrayValue() {
			throw new System.NotSupportedException();
		}

		public virtual SensorType GetSensorType()
		{
			return type;
		}

		public virtual string GetName()
		{
			return name;
		}

		public virtual string GetKey()
		{
			return key;
		}

		public virtual int[] GetShape()
		{
			return shape;
		}

		public virtual bool IsState()
		{
			return isState;
		}

		public virtual bool IsInput()
		{
			return isInput;
		}

		public virtual bool IsResetable()
		{
			return resetable;
		}

		public virtual bool IsActive()
		{
			return isActive;
		}

		public virtual int GetStackedObservations()
		{
			return stackedObservations;
		}

		public BasicAgent GetAgent()
		{
			return this.agent;
		}

		public virtual void OnReset(Agent agent) 
		{
		}
		
		public virtual void SetKey(string newkey)
		{
			this.key = newkey;
		}
		
		public virtual void SetShape(int[] newshape) 
		{
			this.shape = newshape;
		}

		public virtual void SetIsActive(bool v)
		{
			this.isActive = v;
		}

		public virtual void SetIsInput(bool v)
		{
			this.isInput = v;
		}

		public virtual void SetIsResetable(bool v)
		{
			this.resetable = v;
		}

		public virtual void SetStackedObservations(int so)
		{
			this.stackedObservations = so;
		}

		public virtual void SetSensorType(SensorType t)
		{
			this.type = t;
		}

		public virtual float GetRangeMin()
		{
			return rangeMin;
		}

		public virtual float GetRangeMax()
		{
			return rangeMax;
		}

		public virtual void SetRange(float min, float max)
		{
			this.rangeMin = min;
			this.rangeMax = max;
		}

		public virtual void SetName(string name)
		{
			this.name = name;
		}
	}


	public partial class Sensor : Node, ISensor, IAgentResetListener
	{
		[Export]
		public string perceptionKey;
		[Export]
		public int stackedObservations = 1;
		[Export]
		public bool isActive = true;
		[Export]
		public bool isInput = true;
		[Export]
		public bool  normalized = true;
		[Export]
		public bool resetable = true;
		[Export]
		public float rangeMin = 0.0f;
		[Export]
		public float rangeMax = 1.0f;
		[Export]
		public bool isState;
		
		protected SensorType Type;
		protected int[] Shape;
		protected BasicAgent agent;
		
		public bool Normalized
		{
			get
			{
				return normalized;
			}
		}

		public void SetName(string name)
		{
			this.perceptionKey = name;
		}

		public bool IsInput()
		{
			return isInput;
		}

		public float GetRangeMin()
		{
			return rangeMin;
		}

		public float GetRangeMax()
		{
			return rangeMax;
		}

		public void SetRange(float min, float max)
		{
			this.rangeMin = min;
			this.rangeMax = max;
		}

		public void SetIsResetable(bool v)
		{
			this.resetable = v;
		}

		public SensorType type
		{
			get 
			{
				return Type;
			}

			set
			{
				Type = value;
			}
		}

		public int[] shape 
		{
			get
			{
				return Shape;
			}

			set
			{
				Shape = value;
			}
		}

		public void SetAgent(BasicAgent own)
		{
			agent = own;
		}

		public virtual void OnSetup(Agent agent)
		{
		}

		public virtual float GetFloatValue() {
			return 0;
		}

		public virtual string GetStringValue() {
			return string.Empty;
		}

		public virtual bool GetBoolValue() {
			return false;
		}

		public virtual byte[] GetByteArrayValue() {
			return null;
		}

		public virtual int GetIntValue() {
			return 0;
		}

		public virtual int[] GetIntArrayValue() {
			return null;
		}

		public virtual float[] GetFloatArrayValue() {
			return null;
		}

		public SensorType GetSensorType()
		{
			return type;
		}

		public string GetName()
		{
			return perceptionKey;
		}

		public string GetKey()
		{
			return perceptionKey;
		}

		public int[] GetShape()
		{
			return shape;
		}

		public bool IsState()
		{
			return isState;
		}

		public bool IsResetable()
		{
			return resetable;
		}

		public bool IsActive()
		{
			return isActive;
		}

		public int GetStackedObservations()
		{
			return stackedObservations;
		}

		public void SetKey(string newkey)
		{
			this.perceptionKey = newkey;
		}
		
		public void SetShape(int[] newshape) 
		{
			this.shape = newshape;
		}

		public void SetIsActive(bool v)
		{
			isActive = v;
		}

		public void SetIsInput(bool v)
		{
			isInput = v;
		}

		public void SetStackedObservations(int v)
		{
			this.stackedObservations = v;
		}

		public void SetSensorType(SensorType t)
		{
			this.type = t;
		}

		public BasicAgent GetAgent()
		{
			return this.agent;
		}

		public virtual void OnReset(Agent agent) {

		}
	}
}
