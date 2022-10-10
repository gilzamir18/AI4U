using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;
using System.Text;

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

	public class RayCastingSensor : Sensor
	{
		
		[Export]
		public int[] groupCode;
		[Export]
		public string[] groupName;
		
		[Export]
		public int noObjectCode;
	
		[Export]
		public int hSize = 10;
		
		[Export]
		public int vSize = 10;
	
		[Export]
		public NodePath eyePath;
		public Spatial eye;

		[Export]
		public float visionMaxDistance = 500f;
		
		[Export]
		public float fieldOfView = 90.0f;



		private Dictionary<string, int> mapping;
		private Ray[,] raysMatrix = null;
		private HistoryStack<float> history;
		private PhysicsDirectSpaceState spaceState;
		private int[,] viewMatrix = null;
		private float[,] depthMatrix = null;
		
		[Export]
		public bool debugEnabled = false;

		public override void OnSetup(Agent agent) 
		{
			mapping = new Dictionary<string, int>();
			agent.AddResetListener(this);
			this.agent = (BasicAgent) agent;
			this.spaceState = (this.agent.GetAvatarBody() as PhysicsBody).GetWorld().DirectSpaceState;
			type = SensorType.sfloatarray;
			shape = new int[2]{hSize,  vSize};
			history = new HistoryStack<float>(stackedObservations * shape[0] * shape[1]);
			if (this.eyePath != null && this.eyePath != "") {
				this.eye = GetNode(this.eyePath) as Spatial;
			} else {
				this.eye = this.agent.GetAvatarBody() as Spatial;
			}
			raysMatrix = new Ray[shape[0], shape[1]];
			viewMatrix = new int[shape[0], shape[1]];
			depthMatrix = new float[shape[0], shape[1]];
		}

		public override float[] GetFloatArrayValue()
		{
			var aim = eye.GlobalTransform.basis;			
			Vector3 forward = aim.z.Normalized();
			Vector3 up = aim.y.Normalized();
			Vector3 right = aim.x.Normalized();
			UpdateRaysMatrix(eye.GlobalTransform.origin, forward, up, right, fieldOfView);
			UpdateViewMatrix(visionMaxDistance);
			return history.Values;
		}

		public int[,] GetViewMatrix() {
			return this.viewMatrix;
		}

		private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 45.0f)
		{
			float vangle = 2 * fieldOfView / shape[0];
			float hangle = 2 * fieldOfView / shape[1];

			float iangle = -fieldOfView;

			for (int i = 0; i < shape[0]; i++)
			{
				var fwd = ( (new Quat(right, Mathf.Deg2Rad(iangle + vangle * i) )).Normalized().Xform(forward) );
				for (int j = 0; j < shape[1]; j++)
				{
					var direction = ( (new Quat(up, Mathf.Deg2Rad(iangle + hangle * j)  )).Normalized().Xform(fwd) );
					raysMatrix[i, j] =  new Ray(position, direction);
				}
			}
		}
		
		public void UpdateViewMatrix(float maxDistance = 500.0f)
		{
			var debug_line = 0;
			for (int i = 0; i < shape[0]; i++)
			{
				for (int j = 0; j < shape[1]; j++)
				{				
					var myray = raysMatrix[i,j];
					var result = this.spaceState.IntersectRay(myray.Origin, myray.Origin + myray.Direction*maxDistance, null, 2147483647, true, true);//new Godot.Collections.Array { agent.GetBody() }
					if (debugEnabled)
					{
						GetNode<LineDrawer>("/root/LineDrawer").Draw_Line3D(debug_line, myray.Origin, myray.Origin + myray.Direction * maxDistance, new Color(1, 0, 0, 1), new Color(0, 1, 0, 1), 1, 10);
						debug_line += 1;
					}
					float t = -1;
					if (result.Count > 0)
					{
						t = myray.GetDist((Vector3)result["position"]);					
						
						Spatial gobj = result["collider"] as Spatial;

						var groups = gobj.GetGroups();
						bool isTagged = false;
						if (t <= maxDistance)
						{
							foreach(string g in groups)
							{
								if (mapping.ContainsKey(g))
								{
									int code = mapping[g];
									viewMatrix[i, j] = code;
									history.Push(code);
									depthMatrix[i, j] = t;
									isTagged = true;
									break;
								}
							}
						}
						if (!isTagged)
						{
							viewMatrix[i, j] = noObjectCode;
							history.Push(noObjectCode);
							depthMatrix[i, j] = -1.0f;
						}				
					}
					else
					{
						viewMatrix[i, j] = noObjectCode;
						history.Push(noObjectCode);
						depthMatrix[i, j] = -1.0f;
					}					
				}
			}
		}
		
		public override void OnReset(Agent agent) {
			mapping = new Dictionary<string, int>();
			
			for (int o = 0; o < groupName.Length; o++ )
			{
				var code = groupCode[o];
				var name = groupName[o];
				mapping[name] = code;
			}
			raysMatrix = new Ray[shape[0], shape[1]];
			viewMatrix = new int[shape[0], shape[1]];
			depthMatrix = new float[shape[0], shape[1]];
		} 
	}
}
