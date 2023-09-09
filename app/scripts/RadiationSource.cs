using System.Collections;
using System.Collections.Generic;
using Godot;
using ai4u;

public partial class RadiationSource : RigidBody3D, IAgentResetListener
{
	[Export]
	public NodePath agentPath;
	private BasicAgent agent;
	
	[Export]
	public float probability = 0.5f;
	[Export]
	public StandardMaterial3D radiationOn;
	[Export]
	public StandardMaterial3D radiationOff;
	[Export]
	public NodePath meshInstancePath;
	private MeshInstance3D meshInstance;
	[Export]
	public bool chooseOnReset = true;
	[Export]
	public int step = 3;

	private float intensity = 0.0f;
	private System.Random rand;
	private PhysicsDirectSpaceState3D spaceState;
	
	public override void _Ready()
	{
			agent = GetNode(agentPath) as BasicAgent;
			this.spaceState = (this.agent.GetAvatarBody() as PhysicsBody3D).GetWorld3D().DirectSpaceState;
		
			meshInstance = GetNode(meshInstancePath) as MeshInstance3D;
			rand = new System.Random();
			agent.beforeTheResetEvent += OnReset;
			agent.endOfStepEvent += HandleEndOfStep;
	}

	private void HandleEndOfStep(BasicAgent agent)
	{
		if (!chooseOnReset)
		{
			if (agent.NSteps >= step && Intensity <= 0)
			{
				if (rand.NextDouble() < probability)
				{
					On();
				}
				else
				{
					Off();
				}
			}
		}
	}

	public float IntensityTo(RigidBody3D obj)
	{
		if (intensity == 0)
		{
			return 0.0f;
		}
		var query = PhysicsRayQueryParameters3D.Create(GlobalTransform.Origin, obj.GlobalTransform.Origin, 2147483647);
		var result = this.spaceState.IntersectRay(query);
		float d = (GlobalTransform.Origin - obj.GlobalTransform.Origin).Length();
		bool flag = false;
		if (result.Count > 0)
		{

				Vector3 tp = (Vector3)result["position"];
				float t = (obj.GlobalTransform.Origin - tp).Length();					
				Node3D gobj = (Node3D)result["collider"];
				
				if (gobj == obj)
				{

					if (d > 1.0f)
					{
						return intensity/(d*d);
					}
					else
					{
						return intensity;
					}					
				}
				else
				{
					flag = true;
				}
		} else {
			flag = true;
		}
		
		if (flag)
		{
				if (d < 1f)
				{
					return intensity;
				}
				else
				{
					return 0.0f;
				}
		}
		
		return 0.0f;
	}

	public float Intensity
	{
		get
		{
			return intensity;
		}
	}

	public void On()
	{
		intensity = 1.0f;
		meshInstance.Mesh.SurfaceSetMaterial(0, radiationOn);
	}

	public void Off()
	{
		intensity = 0;
		meshInstance.Mesh.SurfaceSetMaterial(0, radiationOff);
	}

	public void OnReset(Agent agent)
	{
		if (chooseOnReset)
		{
			if (rand.NextDouble() < probability)
			{
				On();
			}
			else
			{
				Off();
			}
		}
		else
		{
			Off();
		}
	}
}
