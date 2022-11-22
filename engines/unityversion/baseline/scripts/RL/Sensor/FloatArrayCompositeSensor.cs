using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u 
{
    public class FloatArrayCompositeSensor : Sensor
    {
        private List<Sensor> sensors;
        private int size = 0;
        private HistoryStack<float> stack;

        public override void OnSetup(Agent agent)
        {
            sensors = new List<Sensor>();
            this.type = SensorType.sfloatarray;
            for (int i = 0; i < transform.childCount; i++) 
            {
                GameObject obj = transform.GetChild(i).gameObject;
                Sensor s = obj.GetComponent<Sensor>();
                if (s != null && s.isActive)
                {
                    sensors.Add(s);
                    if (s.resetable)
                    {
                        agent.AddResetListener(this);
                    }
                    s.OnSetup(agent);
                    if (s.type == SensorType.sfloatarray)
                    {
                        size += s.shape[0];
                    } else if (s.type == SensorType.sfloat || 
                                s.type == SensorType.sint  ||
                                s.type == SensorType.sbool)
                    {
                        size += 1;
                    }
                }
            }
            this.shape = new int[1]{size};
            this.stack = new HistoryStack<float>(this.shape[0] * stackedObservations);
        }

        public override void OnReset(Agent agent)
        {
            this.stack = new HistoryStack<float>(this.shape[0] * stackedObservations);
            int n = sensors.Count;
            for (int i = 0; i < n; i++)
            {
                sensors[i].OnReset(agent);
            }
            GetFloatArrayValue();
        }

        public override float[] GetFloatArrayValue()
        {
            int n = sensors.Count;
            for (int i = 0; i < n; i++)
            {
                Sensor s = sensors[i];
                if (s.type == SensorType.sfloatarray)
                {
                    float[] a = s.GetFloatArrayValue();
                    for (int j = 0; j < a.Length; j++)
                    {
                        stack.Push(a[j]);
                    }
                }
                else if (s.type == SensorType.sfloat)
                {
                    stack.Push(s.GetFloatValue());
                } else if (s.type == SensorType.sint)
                {
                    stack.Push(s.GetIntValue());
                } else if (s.type == SensorType.sbool)
                {
                    stack.Push(s.GetBoolValue() ? 1.0f: -1.0f);
                }
            }
            return stack.Values;
        }
    }
}
