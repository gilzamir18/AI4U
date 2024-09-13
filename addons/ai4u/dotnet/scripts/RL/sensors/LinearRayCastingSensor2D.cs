using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;

namespace ai4u
{	
	[Tool]
	public partial class LinearRayCastingSensor2D : Sensor
	{
		
		[ExportCategory("Object Detection")]
		[Export]
		public int[] groupCode;
		[Export]
		public string[] groupName;
		
		[Export]
		public int noObjectCode = 1;

		[Export]
		public int objectNotFoundCode = 0;
	
		[Export]
		private bool selfExclude = true;

		[Export(PropertyHint.Layers2DPhysics)]
		private uint collisionMask = uint.MaxValue;

		[ExportCategory("Resolution")]
		[Export]
		public int numberOfRays = 10;
		

		[ExportCategory("Projection")]	
		[Export]
		public Node2D eye;
		[Export]
		private float fieldOfView = 90;
		[Export]
		public float shift = 0;
		[Export]
		public float visionMaxDistance = 30f;


		[ExportCategory("Data Shape")]
		[Export]
		private bool _normalized = true;
		[Export]		
		private float[] modelDataRange = {0, 1};
		[Export]
		private float[] worldDataRange = {0, 255};

		[ExportCategory("Distance")]
		[Export]
		public bool returnDistance = false;
		[Export]
		public bool normalizeDistance = false;
		[Export]
		public float maxDistance = 30;

		[ExportCategory("Debug")]
		[Export]
		public bool debugEnabled = false;
		[Export]
		public Color debugColor = new Color(1, 0, 0);
		[Export]
		public Color noHitColor = new Color(0, 0, 0);
		[Export]
		public float lineTickeness = 1;
		[Export]
		public float startSize = 5;


		private Dictionary<string, int> mapping;
		private HistoryStack<float> history;
		private PhysicsDirectSpaceState2D spaceState;

		private LineDrawer2D lineDrawer = null;

		public override void _Ready()
		{
			if (Engine.IsEditorHint())
			{
				lineDrawer = new LineDrawer2D();
				AddChild(lineDrawer);
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
						var spid = PhysicsServer2D.SpaceCreate();
						spaceState = PhysicsServer2D.SpaceGetDirectState(spid);
					}
					StartRays(eye.GlobalPosition, eye.GlobalTransform.X, eye.GlobalTransform.Y, fieldOfView, true);
				}
			}
		}


		public override void OnSetup(Agent agent) 
		{

			int k = 1;
			if (returnDistance)
			{
				k = 2;
			}


			normalized = _normalized;
			rangeMin = modelDataRange[0];
			rangeMax = modelDataRange[1];

			type = SensorType.sfloatarray;
			shape = new int[1]{stackedObservations * numberOfRays * k};
			history = new HistoryStack<float>(shape[0]);
			
			
			agent.AddResetListener(this);
			
			mapping = new Dictionary<string, int>();

			this.agent = (RLAgent) agent;
			this.spaceState = (this.agent.GetAvatarBody() as PhysicsBody2D).GetWorld2D().DirectSpaceState;

			if (this.eye == null) {
				this.eye = this.agent.GetAvatarBody() as Node2D;
			}

			if (debugEnabled)
			{
				lineDrawer = new LineDrawer2D();
				AddChild(lineDrawer);
			}
		}

		public override float[] GetFloatArrayValue()
		{
			StartRays(eye.GlobalPosition, eye.GlobalTransform.X, eye.GlobalTransform.Y, fieldOfView);
			return history.Values;
		}

		private void StartRays(Vector2 position, Vector2 forward, Vector2 up, float fieldOfView = 90, bool inEditor = false)
		{				
			var interval = fieldOfView / (numberOfRays + 1);
			int lineNumber = 0;
			for (int i = 0; i < numberOfRays; i++)
			{
				var angle = 0.0f;
				if (numberOfRays > 1)
				{
					angle = (i+1) * interval;
					angle = angle - (fieldOfView/2);
				}

				var fwd = forward.Rotated(Mathf.DegToRad(angle-shift)).Normalized();
				var ray =  new Ray2D(position, fwd);
				if (!inEditor)
				{
					ThrowRay(lineNumber, ray);
				}
				else
				{
					ThrowRayInEditor(lineNumber, ray);
				}
				lineNumber++;
			}
		}
		
		public void ThrowRay(int id, Ray2D ray)
		{
			var query = PhysicsRayQueryParameters2D.Create(ray.Origin, ray.Origin + ray.Direction*visionMaxDistance, collisionMask);
			if (selfExclude)
			{
				query.Exclude.Add( ((PhysicsBody2D) agent.GetAvatarBody()).GetRid() );
			}
			var result = this.spaceState.IntersectRay( query );
			bool isTagged = false;
			float t = objectNotFoundCode;
			if (result.Count > 0)
			{
				t = ray.GetDist((Vector2)result["position"]);					
				
				Node2D gobj = (Node2D) result["collider"];

				var groups = gobj.GetGroups();

				if (t <= visionMaxDistance)
				{
					foreach(string g in groups)
					{
						if (mapping.ContainsKey(g))
						{
							int code = mapping[g];
							AddCodeToHistory(code);
							AddDistanceToHistory(t);
							isTagged = true;
							break;
						}
					}
				}
				if (!isTagged)
				{
					AddCodeToHistory(noObjectCode);
					AddDistanceToHistory(t);
				}				
			}
			else
			{
				AddCodeToHistory(noObjectCode);
				AddDistanceToHistory(0);
			}
			if (debugEnabled)
			{
				if (isTagged) {
					lineDrawer.DrawLine(id, ray.Origin, ray.Origin + ray.Direction * visionMaxDistance, debugColor, debugColor, lineTickeness, startSize);
				} else 
				{
					lineDrawer.DrawLine(id, ray.Origin, ray.Origin + ray.Direction * visionMaxDistance, noHitColor, noHitColor, lineTickeness, startSize);
				}
				lineDrawer.QueueRedraw();
			}
		}

		public void ThrowRayInEditor(int id, Ray2D ray)
		{
			var query = PhysicsRayQueryParameters2D.Create(ray.Origin, ray.Origin + ray.Direction*visionMaxDistance, collisionMask);
			
			var result = this.spaceState.IntersectRay( query );
			bool isTagged = false;
			float t =objectNotFoundCode;
			if (result.Count > 0)
			{
				t = ray.GetDist((Vector2)result["position"]);					
				
				Node2D gobj = (Node2D) result["collider"];

				var groups = gobj.GetGroups();

				if (t <= visionMaxDistance)
				{
					foreach(string g in groups)
					{
						if (mapping.ContainsKey(g))
						{
							int code = mapping[g];
							AddCodeToHistory(code);
							AddDistanceToHistory(t);
							isTagged = true;
							break;
						}
					}
				}
				if (!isTagged)
				{
					AddCodeToHistory(noObjectCode);
					AddDistanceToHistory(t);
				}				
			}
			else
			{
				AddCodeToHistory(noObjectCode);
				AddDistanceToHistory(0);
			}
			if (debugEnabled)
			{
				if (isTagged) {
					lineDrawer.DrawLine(id, ray.Origin, ray.Origin + ray.Direction * visionMaxDistance, debugColor, debugColor, lineTickeness, startSize);
				} else 
				{
					lineDrawer.DrawLine(id, ray.Origin, ray.Origin + ray.Direction * visionMaxDistance, noHitColor, noHitColor, lineTickeness, startSize);
				}
				lineDrawer.QueueRedraw();
			}
		}

		private void AddCodeToHistory(float v)
		{
			if (history != null)
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
		}

		private void AddDistanceToHistory(float v)
		{
			if (returnDistance && history != null)
			{
				if (normalizeDistance)
				{
					history.Push( MapRange(v, 0, float.MaxValue, 0, 1 ) );
				}
				else
				{
					history.Push(v);
				}
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
			if (groupName != null && groupCode != null)
			{
				for (int o = 0; o < groupName.Length; o++ )
				{
					var code = groupCode[o];
					var name = groupName[o];
					mapping[name] = code;
				}
			}
			else
			{
				GD.PrintRaw("The LinearRayCastingSensor does not have a group name or a group code, so the agent will not perceive anything!");
			}
		} 
	}
}
