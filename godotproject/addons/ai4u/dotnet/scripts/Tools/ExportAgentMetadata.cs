using Godot;
using System;
using ai4u;
using System.IO;

/// <summary>
/// Export the agent metadata to json format and save it in path named <code>filePath</code>.
/// </summary>
public partial class ExportAgentMetadata : Node
{

	/// <summary>
	/// Selected agent.
	/// </summary>
	[Export]
	private BasicAgent agent;


	/// <summary>
	/// File path to save the metadata model.
	/// </summary>
	[Export]
	private string filePath="";

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (agent != null)
		{
			agent.resetEvent += OnReset;
		}
		else
		{
			GD.PrintErr("Export error: agent was not specified! Please set one in the 'agent' field.");
		}
	}


	public void OnReset(Agent agent)
	{
		string content =  ((BasicAgent) agent).GetMetadataAsJson();
		File.WriteAllText(filePath, content);
	}
}
