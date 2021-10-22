using Godot;
using System;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using ai4u;


namespace ai4u.ext
{
	public class KinematicBodyCollisionSensor : Sensor
	{	
			private KinematicBody body;
			[Export]
			public bool codeByGroup = true;
			
			[Export]
			public string[] itemName;
			[Export]
			public float[] itemCode;

			private Dictionary<string, float> nameCode = new Dictionary<string, float>();
			
			public override float[] GetFloatArrayValue() 
			{
				List<float> codes = new List<float>();
				for (int i = 0; i < body.GetSlideCount(); i++)
				{
					var collision = body.GetSlideCollision(i);
					Node node = (Spatial)collision.Collider;
					if (codeByGroup)
					{						
						var groups = node.GetGroups();
						foreach(string g in groups)
						{
							if (nameCode.ContainsKey(g))
							{
								codes.Add(nameCode[g]);
							}
							else
							{
								codes.Add(1);
							}
						}
					} 
					else
					{
						if (nameCode.ContainsKey(node.Name))
						{
							codes.Add(nameCode[node.Name]);
						}
						else
						{
							codes.Add(1);
						}
					}
				}				
				float[] r =  new float[shape[0]];
				for (int i = 0; i < codes.Count; i++)
				{
					r[i] = codes[i];
					if (i >= shape[0]) break;
				}
				
				return r;
			}

			public override void OnBinding(Agent agent) 
			{
				type = SensorType.sfloatarray;
				body = agent.GetBody() as KinematicBody;
				if (itemName != null)
				{
					for (int i = 0; i < itemName.Length; i++)
					{
						nameCode[itemName[i]] = itemCode[i];
					}
				}
			}
			
			public override void OnReset(Agent agent) 
			{
			}
	}
}
