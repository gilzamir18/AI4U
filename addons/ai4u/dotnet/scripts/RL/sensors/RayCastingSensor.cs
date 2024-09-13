using Godot;
using System;
using System.Collections.Generic;
using System.Collections;
using ai4u;
using System.Text;
using System.Runtime.InteropServices;

namespace ai4u
{
    [Tool]
    public partial class RayCastingSensor : Sensor
	{	
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
		private Color debugColor = new Color(1, 1, 1, 1);


		[Export]
		public bool flattened = false;

		[Export]
		private bool _normalized = true;

		[Export]
		private float[] modelDataRange = {0, 1};
		[Export]
		private float[] worldDataRange = {0, 255};

        [Export]
        public bool returnDistance = false;

        [Export]
        public bool normalizeDistance = false;

        private Dictionary<string, int> mapping;
		private Ray[,] raysMatrix = null;
		private HistoryStack<float> history;
		private PhysicsDirectSpaceState3D spaceState;
        private LineDrawer lineDrawer = null;

        public override void _Ready()
        {
               lineDrawer = new LineDrawer();
               lineDrawer.SetColor(debugColor);
               AddChild(lineDrawer);
               lineDrawer.StartMeshes();
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
			if (flattened)
			{
				shape = new int[1]{stackedObservations * hSize * vSize * k};
				history = new HistoryStack<float>(shape[0]);
			}
			else
			{
				if (k == 1)
				{
					shape = new int[3] { stackedObservations, hSize, vSize };
				}
				else
				{
					shape = new int[4] { k, stackedObservations, hSize, vSize };
				}
				history = new HistoryStack<float>(k * shape[0] * shape[1] * shape[2]);	
			}
			
			agent.AddResetListener(this);
			
			mapping = new Dictionary<string, int>();

			this.agent = (RLAgent) agent;
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
			StartRays(eye.GlobalTransform.Origin, forward, up, right, fieldOfView);
			return history.Values;
		}

		private void StartRays(Vector3 position, Vector3 forward, Vector3 up, Vector3 right, float fieldOfView = 45.0f, bool inEditor = false)
		{

            if (lineDrawer != null)
            {
                lineDrawer.Clear();
            }

            float vangle = 2 * fieldOfView / hSize;
			float hangle = 2 * fieldOfView / vSize;

			
			float iangle = -fieldOfView;
			
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
					var ray =  new Ray(position, direction);
					if (inEditor)
					{
						ThrowRayInEditor(ray);
					}
					else
					{
                        ThrowRay(ray);
                    }
				}
			}
		}
		
		public void ThrowRay(Ray myray)
		{
			var query = PhysicsRayQueryParameters3D.Create(myray.Origin, myray.Origin + myray.Direction*visionMaxDistance, collisionMask);
			
			if (selfExclude)
			{
				query.Exclude.Add( ((PhysicsBody3D) agent.GetAvatarBody()).GetRid() );
			}
			var result = this.spaceState.IntersectRay( query);//new Godot.Collections.Array { agent.GetBody() }
			bool isTagged = false;
			float t = objectNotFoundCode;
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
                if (isTagged)
                {
                    lineDrawer.AddLine(myray.Origin, myray.Origin + myray.Direction * visionMaxDistance);
                }
                else
                {
                    lineDrawer.AddLine(myray.Origin, myray.Origin + myray.Direction * visionMaxDistance);
                }
                lineDrawer.DrawLines();
            }
        }

        public void ThrowRayInEditor(Ray myray)
        {
            var query = PhysicsRayQueryParameters3D.Create(myray.Origin, myray.Origin + myray.Direction * visionMaxDistance, collisionMask);

            var result = this.spaceState.IntersectRay(query);//new Godot.Collections.Array { agent.GetBody() }
            bool isTagged = false;
            float t = objectNotFoundCode;
            if (result.Count > 0)
            {
                t = myray.GetDist((Vector3)result["position"]);

                Node3D gobj = (Node3D)result["collider"];

                var groups = gobj.GetGroups();

                if (t <= visionMaxDistance)
                {
                    foreach (string g in groups)
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
                if (isTagged)
                {
                    lineDrawer.AddLine(myray.Origin, myray.Origin + myray.Direction * visionMaxDistance);
                }
                else
                {
                    lineDrawer.AddLine(myray.Origin, myray.Origin + myray.Direction * visionMaxDistance);
                }
                lineDrawer.DrawLines();
            }
        }

        private void AddCodeToHistory(float v)
		{
			if (history != null)
			{
				if (normalized)
				{
					history.Push(MapRange(v, worldDataRange[0], worldDataRange[1], modelDataRange[0], modelDataRange[1]));
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
                    history.Push(MapRange(v, 0, float.MaxValue, 0, 1));
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

        public override void _PhysicsProcess(double delta)
        {
            if (Engine.IsEditorHint())
            {
                if (debugEnabled && eye != null)
                {
                    lineDrawer.SetColor(debugColor);
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
    }
}
