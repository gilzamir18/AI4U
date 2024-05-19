using Godot;
using System;
using ai4u;

public partial class TargetPositions : Node, IAgentResetListener
{

	[Export]
	private BasicAgent basicAgent;

    [Export]
    private StaticBody3D body3D;

    [Export]
    private NodePath respawnOptionsPath;

    private Node nodeRef;


    private Godot.Collections.Array<Node> children;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        nodeRef = GetNode(respawnOptionsPath);
        children = nodeRef.GetChildren();
        basicAgent.AddResetListener(this);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void OnReset(Agent agent)
    {
        Vector3 reference;
        if (children.Count > 0)
        {
            int idx = (int)GD.RandRange(0, children.Count - 1);
            reference = ((Node3D)children[idx]).Position;
        }
        else
        {
            reference = ((Node3D)nodeRef).GlobalPosition;
        }

        body3D.Position = reference;
        body3D.ConstantAngularVelocity = new Vector3(0, 0, 0);
        body3D.ConstantLinearVelocity = new Vector3(0, 0, 0);
    }
}
