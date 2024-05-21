using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;

namespace ai4u
{	
	[Tool]
	public partial class LinearRayCastingSensor : Sensor
	{
		
		[ExportCategory("Object Detection")]
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

		[ExportCategory("Resolution")]
		[Export]
		public int numberOfRays = 10;
		

		[ExportCategory("Projection")]	
		[Export]
		public Node3D eye;
		[Export]
		private float fieldOfView = 90;
		[Export]
		public float horizontalShift = 0;
		[Export]
		public float verticalShift = 30;
		[Export]
		public float visionMaxDistance = 30f;


		[ExportCategory("Data Shape")]
		[Export]
		private bool _normalized = true;
		[Export]		
		private float[] modelDataRange = {-1, 1};
		[Export]
		private float[] worldDataRange = {-1, 1};


		[ExportCategory("Debug")]
		[Export]
		public bool debugEnabled = false;
		[Export]
		public Color debugColor = new Color(1, 0, 0);



		private Dictionary<string, int> mapping;
		private HistoryStack<float> history;
		private PhysicsDirectSpaceState3D spaceState;

		private LineDrawer inEditorLineDrawer = null;

        public override void _Ready()
        {
			if (Engine.IsEditorHint())
			{
				inEditorLineDrawer = new LineDrawer();
				inEditorLineDrawer.SetColor(debugColor);
				AddChild(inEditorLineDrawer);
				inEditorLineDrawer.StartInEditor();
			}	
        }

        public override void _PhysicsProcess(double delta)
        {
			if ( Engine.IsEditorHint() )
			{
				if (debugEnabled && eye != null)
				{
					if (spaceState == null)
					{
						var spid = PhysicsServer3D.SpaceCreate();
						spaceState = PhysicsServer3D.SpaceGetDirectState(spid);
					}
					var aim = eye.GlobalTransform.Basis;
					Vector3 forward = aim.Z.Normalized();
					Vector3 up = aim.Y.Normalized();
					Vector3 right = aim.X.Normalized();
					StartRays(eye.GlobalTransform.Origin, forward, up, right, fieldOfView, true);
				}
			}
        }


        public override void OnSetup(Agent agent) 
		{
			normalized = _normalized;
			rangeMin = modelDataRange[0];
			rangeMax = modelDataRange[1];

			type = SensorType.sfloatarray;
			shape = new int[1]{stackedObservations * numberOfRays};
			history = new HistoryStack<float>(shape[0]);
			
			
			agent.AddResetListener(this);
			
			mapping = new Dictionary<string, int>();

			this.agent = (BasicAgent) agent;
			this.spaceState = (this.agent.GetAvatarBody() as PhysicsBody3D).GetWorld3D().DirectSpaceState;

			if (this.eye == null) {
				this.eye = this.agent.GetAvatarBody() as Node3D;
			}

			if (debugEnabled)
			{
				GetNode<LineDrawer>("/root/LineDrawer").SetColor(debugColor);				
			}
		}

		public override float[] GetFloatArrayValue()
		{
			var aim = eye.GlobalTransform.Basis;			
			Vector3 forward = aim.Z.Normalized();
			Vector3 up = aim.Y.Normalized();
			Vector3 right = aim.X.Normalized();
			StartRays(eye.GlobalTransform.Origin, forward, up, right, fieldOfView);
			return history.Values;
		}

		private void StartRays(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 90, bool inEditor = false)
		{
			if (debugEnabled && !inEditor)
			{
				GetNode<LineDrawer>("/root/LineDrawer").Clear();
			}
			else
			{
				inEditorLineDrawer.Clear();
			}


			var interval = fieldOfView / (numberOfRays + 1);

			for (int i = 0; i < numberOfRays; i++)
			{
				var angle = 0.0f;
				if (numberOfRays > 1)
				{
					angle = (i+1) * interval;
					angle = angle - (fieldOfView/2);
				}

				var fwd = forward.Rotated(up, Mathf.DegToRad(angle-horizontalShift) ).Normalized();
				fwd = fwd.Rotated(right, Mathf.DegToRad(-verticalShift)).Normalized();
				var ray =  new Ray(position, fwd);
				if (!inEditor)
				{
					ThrowRay(ray);
				}
				else
				{
					ThrowRayInEditor(ray);
				}
			}
		}
		
		public void ThrowRay(Ray ray)
		{
			var query = PhysicsRayQueryParameters3D.Create(ray.Origin, ray.Origin + ray.Direction*visionMaxDistance, collisionMask);
			if (selfExclude)
			{
				query.Exclude.Add( ((PhysicsBody3D) agent.GetAvatarBody()).GetRid() );
			}
			var result = this.spaceState.IntersectRay( query);
			bool isTagged = false;
			float t = -1;
			if (result.Count > 0)
			{
				t = ray.GetDist((Vector3)result["position"]);					
				
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
					GetNode<LineDrawer>("/root/LineDrawer").AddLine(ray.Origin, ray.Origin + ray.Direction * visionMaxDistance);
				} else 
				{
					GetNode<LineDrawer>("/root/LineDrawer").AddLine(ray.Origin, ray.Origin + ray.Direction * visionMaxDistance);
				}
				GetNode<LineDrawer>("/root/LineDrawer").DrawLines();
			}
		}

		public void ThrowRayInEditor(Ray ray)
		{
			var query = PhysicsRayQueryParameters3D.Create(ray.Origin, ray.Origin + ray.Direction*visionMaxDistance, collisionMask);
			

			var result = this.spaceState.IntersectRay( query);
			
			
			bool isTagged = false;
			float t = -1;
			if (result.Count > 0)
			{
				t = ray.GetDist((Vector3)result["position"]);					
				
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
			}

			if (debugEnabled)
			{
				if (isTagged) {
					inEditorLineDrawer.AddLine(ray.Origin, ray.Origin + ray.Direction * visionMaxDistance);
				} else 
				{
					inEditorLineDrawer.AddLine(ray.Origin, ray.Origin + ray.Direction * visionMaxDistance);
				}
				inEditorLineDrawer.DrawLines();
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
			shape = new int[1]{stackedObservations * numberOfRays};
			history = new HistoryStack<float>(shape[0]);

			mapping = new Dictionary<string, int>();
			
			for (int o = 0; o < groupName.Length; o++ )
			{
				var code = groupCode[o];
				var name = groupName[o];
				mapping[name] = code;
			}
		} 
	}
}
