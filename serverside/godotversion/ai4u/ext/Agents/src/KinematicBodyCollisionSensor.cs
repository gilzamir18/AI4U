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
			[Export]
			public bool returnGroups = true;
			
			[Export]
			public string separator = ";";
		
			private KinematicBody body;

			private List< Dictionary<string, string> > list;

			public override string GetStringValue() 
			{
				StringBuilder r = new StringBuilder();
				var j = 0;
				foreach(Dictionary<string, string> d in list) 
				{
					if (j > 0)
					{
						r.Append(",");
					}
					r.Append(Utils.DictToPythonDict<string, string>(d));
					j++;
				}
				list.Clear();
				return r.ToString();
			}
			
			public override void _PhysicsProcess(float delta)
			{
				for (int i = 0; i < body.GetSlideCount(); i++)
				{
					var collision = body.GetSlideCollision(i);
					Node node = (Spatial)collision.Collider;
					Dictionary<string, string> result = new Dictionary<string, string>();
					if (returnGroups)
					{
						string gs = "";
						var groups = node.GetGroups();
						foreach(string g in groups)
						{
							gs += g + ";";
						}
						result["groups"] = gs;
					}
					result["name"] = node.Name;
					Vector3 position = collision.Position;
					Vector3 normal = collision.Normal;
					result["position"] = "[" + position.x + ", " + position.y + ", " + position.z + "]";			
					result["normal"] = "[" + normal.x + ", " + normal.y + ", " + normal.z + "]";
					list.Add(result);
				}
			}

			public override void OnBinding(Agent agent) 
			{
				type = SensorType.sstring;
				list = new List< Dictionary<string, string> > ();
				body = agent.GetBody() as KinematicBody;
			}
			
			public override void OnReset(Agent agent) 
			{
				type = SensorType.sstring;
				list = new List< Dictionary<string, string> > ();
			}
	}
}
