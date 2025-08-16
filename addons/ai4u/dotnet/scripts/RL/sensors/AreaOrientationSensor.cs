using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

namespace ai4u
{
    class CompareNodes3D : IComparer<Node3D>
    {
		private Node3D origim;

		public CompareNodes3D(Node3D origim)
		{
			this.origim = origim;
		}

        public int Compare(Node3D node1, Node3D node2)
        {
			var d1 = node1.Position.DistanceSquaredTo(origim.Position);
			var d2 = node2.Position.DistanceSquaredTo(origim.Position);

			if (d1 > d2)
			{
				return 1;
			}
			else if (d2 > d1)
			{
				return -1;
			}

            return 0;
        }
    }

    public partial class AreaOrientationSensor : Sensor
	{

        [Export]
        private Area3D area;

        [Export]
		private Node3D reference;

		[Export]
		private int maxDetectionReported = 20;

		[Export]
		private float minDistance = 1;

		[ExportCategory("Items Identification")]

        [Export]
        private string targetGroup = "EMITTER";

		[Export]
        private Godot.Collections.Dictionary<string, int> groupCode;

		[Export]
		private int maxCodeValue = 100;

        [Export]
        private bool enableVisibilityTest = true;

        [Export(PropertyHint.Layers3DPhysics)]
        private uint visibilityTestMask = uint.MaxValue;

		private int infoByDetection = 3;

        private HistoryStack<float> history;

		public SortedSet<Node3D> lastResults = null;

		private CompareNodes3D distanceComparer;

		public override void OnSetup(Agent agent)
		{

            this.agent = (RLAgent) agent;


            distanceComparer = new CompareNodes3D((Node3D)this.agent.GetAvatarBody());

            perceptionKey = "areasensor";
			type = SensorType.sfloatarray;
			shape = new int[]{stackedObservations * infoByDetection * maxDetectionReported};
			history = new HistoryStack<float>(shape[0]);

			this.agent.OnResetStart += (RLAgent a) =>
			{
                history = new HistoryStack<float>(shape[0]);
				lastResults = null;
            };
		}

        public override float[] GetFloatArrayValue()
		{
            history = new HistoryStack<float>(shape[0]);
            var bodies = area.GetOverlappingBodies();

            SortedSet<Node3D> objects = new SortedSet<Node3D>(distanceComparer);
            PhysicsBody3D body = (PhysicsBody3D) this.agent.GetAvatarBody();

            for (int i = 0; i < bodies.Count; i++)
			{
                if (bodies[i] != null && bodies[i].IsInGroup(targetGroup))
				{
                    if (this.enableVisibilityTest)
                    {
                        Node3D node3D = bodies[i];
                        var query = PhysicsRayQueryParameters3D.Create(body.GlobalPosition, node3D.GlobalPosition, visibilityTestMask);
                        query.Exclude.Add(body.GetRid());

                        var result = body.GetWorld3D().DirectSpaceState.IntersectRay(query);

                        if (result.Count > 0)
                        {
                            if (((Node)result["collider"]) == node3D)
                            {
                                objects.Add(bodies[i]);
                            }
                        }
                    }
                    else
                    {
                        objects.Add(bodies[i]);
                    }
                }
            }

			foreach(var target in objects)
			{
                Vector3 f = reference.GlobalTransform.Basis.Z.Normalized();

                Vector3 d = target.GlobalTransform.Origin - reference.GlobalTransform.Origin;
                var dist = d.Length();

                float prox = 0;
                if (dist < minDistance)
                {
                    prox = 1;
                }
                else
                {
                    prox = minDistance / dist;
                }

                var angResult = f.Dot(d.Normalized());


				var code = 0;
                if (groupCode != null)
                {
                    foreach (string g in target.GetGroups())
                    {
                        if (groupCode.ContainsKey(g))
                        {
							code = groupCode[g];
                            break;
                        }
                    }
                }
                //GD.Print(code + " :: " + angResult + " :: " + prox);
                history.Push(code / maxCodeValue);
                history.Push(angResult);
                history.Push(prox);
            }

            this.lastResults = objects;
			return history.Values;
		}
	}
}
