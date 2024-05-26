using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;
using System.Text;
using System.Runtime.InteropServices;

namespace ai4u
{	
	[Obsolete("RayCastingSensor will be discontinued soon. Please use LinearRayCastingSensor instead!")]
	public partial class RayCastingSensor : Sensor
	{
		
		[Export]
		public int[] groupCode;
		[Export]
		public string[] groupName;
		
		[Export]
		public int noObjectCode;
	
		[Export]
		private bool selfExclude = true;

		[Export(PropertyHint.Layers3DPhysics)]
		private uint collisionMask = uint.MaxValue;

		[Export]
		public int hSize = 10;
		
		[Export]
		public int vSize = 10;
		
		[Export]
		public float horizontalShift = 0;
		[Export]
		public float verticalShift = 30;
	
		[Export]
		public Node3D eye;

		[Export]
		public float visionMaxDistance = 30f;
		
		[Export]
		public float fieldOfView = 45.0f;

		[Export]
		public bool debugEnabled = false;

		[Export]
		private Color detectionFailColor = new Color(1, 1, 1, 1);

		[Export]
		private Color detectionSuccessColor = new Color(1, 0, 0, 1);

		[Export]
		public bool flattened = false;

		[Export]
		private bool _normalized = true;

		[Export]
		private float[] modelDataRange = {-1, 1};
		[Export]
		private float[] worldDataRange = {0, 255};


		private Dictionary<string, int> mapping;
		private Ray[,] raysMatrix = null;
		private HistoryStack<float> history;
		private PhysicsDirectSpaceState3D spaceState;
		

		public override void OnSetup(Agent agent) 
		{
			normalized = _normalized;
			rangeMin = modelDataRange[0];
			rangeMax = modelDataRange[1];

			type = SensorType.sfloatarray;
			if (flattened)
			{
				shape = new int[1]{stackedObservations * hSize * vSize};
				history = new HistoryStack<float>(shape[0]);
			}
			else
			{
				shape = new int[3]{stackedObservations, hSize,  vSize};
				history = new HistoryStack<float>(shape[0] * shape[1] * shape[2]);	
			}
			
			agent.AddResetListener(this);
			
			mapping = new Dictionary<string, int>();

			this.agent = (BasicAgent) agent;
			this.spaceState = (this.agent.GetAvatarBody() as PhysicsBody3D).GetWorld3D().DirectSpaceState;

			if (this.eye == null) {
				this.eye = this.agent.GetAvatarBody() as Node3D;
			}
			raysMatrix = new Ray[hSize, vSize];
		}

		public override float[] GetFloatArrayValue()
		{
			var aim = eye.GlobalTransform.Basis;			
			Vector3 forward = aim.Z.Normalized();
			Vector3 up = aim.Y.Normalized();
			Vector3 right = aim.X.Normalized();
			UpdateRaysMatrix(eye.GlobalTransform.Origin, forward, up, right, fieldOfView);
			return history.Values;
		}

		private void UpdateRaysMatrix(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 45.0f)
		{
			float vangle = 2 * fieldOfView / hSize;
			float hangle = 2 * fieldOfView / vSize;

			
			float iangle = -fieldOfView;
			
			var debugline = 0;
			for (int i = 0; i < hSize; i++)
			{
				var k1 = 1;
				if (hSize <= 1)
				{
					k1 = 0;
				}
				var fwd = forward.Rotated(up, Mathf.DegToRad( (iangle * k1 + horizontalShift)  + vangle * i) ).Normalized();
				for (int j = 0; j < vSize; j++)
				{
					var k2 = 1;
					if (vSize <= 1)
					{
						k2 = 0;
					}
					var direction = fwd.Rotated(right, Mathf.DegToRad( (iangle * k2 + verticalShift) + hangle * j)).Normalized();
					raysMatrix[i, j] =  new Ray(position, direction);
					UpdateView(i, j, debugline);
					debugline ++;
				}
			}
		}
		
		public void UpdateView(int i, int j, int debug_line = 0)
		{
			var myray = raysMatrix[i,j];
			var query = PhysicsRayQueryParameters3D.Create(myray.Origin, myray.Origin + myray.Direction*visionMaxDistance, collisionMask);
			
			if (selfExclude)
			{
				query.Exclude.Add( ((PhysicsBody3D) agent.GetAvatarBody()).GetRid() );
			}
			var result = this.spaceState.IntersectRay( query);//new Godot.Collections.Array { agent.GetBody() }
			bool isTagged = false;
			float t = -1;
			if (result.Count > 0)
			{
				t = myray.GetDist((Vector3)result["position"]);					
				
				Node3D gobj = (Node3D) result["collider"];

				var groups = gobj.GetGroups();

				if (t <= visionMaxDistance)
				{
					foreach(string g in groups)
					{
						if (mapping.ContainsKey(g))
						{
							int code = mapping[g];
							AddValueToHistory(code);
							isTagged = true;
							break;
						}
					}
				}
				if (!isTagged)
				{
					AddValueToHistory(noObjectCode);
				}				
			}
			else
			{
				AddValueToHistory(noObjectCode);
			}
			if (debugEnabled)
			{
				if (isTagged) {
					/*GetNode<LineDrawer>("/root/LineDrawer").Draw_Line3D(debug_line, myray.Origin, 
																		myray.Origin + myray.Direction * visionMaxDistance, 
																		detectionSuccessColor, new Color(0, 1, 0, 1), 1, 10);*/
				} else 
				{
					/*GetNode<LineDrawer>("/root/LineDrawer").Draw_Line3D(debug_line, myray.Origin, 
																			myray.Origin + myray.Direction * visionMaxDistance, 
																			detectionFailColor, 
																			new Color(0, 0, 0, 1), 1, 10); */					
				}
				/*GetNode<LineDrawer>("/root/LineDrawer").Redraw();*/
			}
		}

		private void AddValueToHistory(float v)
		{
			if (normalized)
			{
				history.Push( MapRange(v, worldDataRange[0], worldDataRange[1], modelDataRange[0], modelDataRange[1] ) );
			}
			else
			{
				history.Push(v);
			}
		}

		public static float MapRange(float value, float fromSource, float toSource, float fromTarget, float toTarget)
		{
			// Primeiro, normalizamos o valor de entrada para o intervalo [0, 1]
			float normalizedValue = (value - fromSource) / (toSource - fromSource);

			// Em seguida, escalamos o valor normalizado para o intervalo de destino [a, b]
			float mappedValue = fromTarget + (normalizedValue * (toTarget - fromTarget));

			return mappedValue;
		}

		public override void OnReset(Agent agent) {
			if (flattened)
			{
				shape = new int[1]{stackedObservations * hSize * vSize};
				history = new HistoryStack<float>(shape[0]);
			}
			else
			{
				shape = new int[3]{stackedObservations, hSize,  vSize};
				history = new HistoryStack<float>(shape[0] * shape[1] * shape[2]);	
			}
			mapping = new Dictionary<string, int>();
			
			for (int o = 0; o < groupName.Length; o++ )
			{
				var code = groupCode[o];
				var name = groupName[o];
				mapping[name] = code;
			}
			raysMatrix = new Ray[hSize, vSize];
		} 
	}
}
