using ai4u;
using Godot;
using System;
using System.Text;
using System.Threading;

namespace ai4u;

[Tool]
public partial class RemoteStarter : Node
{

    [Export]
    internal RemoteConfiguration config = null;

    [Export]
    internal string starterAgentId = "0";

    private bool playingRequested = false;

    private float turnoffCoolDown = 0;
    private float turnoffTime = 3;

    private RemoteBrain brain;

    private bool sceneRunning = false;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        playingRequested = false;
        if (Engine.IsEditorHint() && brain == null)
        { 
            if (config == null)
            {
                config = new RemoteConfiguration();
                CallDeferred("add_child", config);
            }
            if (brain == null)
            {
                brain = new RemoteBrain();
                brain.Host = config.host;
                brain.Port = config.port;
                brain.ReceiveBufferSize = config.receiveBufferSize;
                brain.SendBufferSize = config.sendBufferSize;
                brain.ReceiveTimeout = config.receiveTimeout;
            }
        }
    }

    private void PlayScene()
    {
        if (Engine.IsEditorHint())
        {
            EditorInterface.Singleton.PlayCurrentScene();
        }
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (Engine.IsEditorHint() && EditorInterface.Singleton.IsPlayingScene())
        {
            playingRequested = false;
            sceneRunning = true;
        }

        if (Engine.IsEditorHint() && !EditorInterface.Singleton.IsPlayingScene())
        {
            if (sceneRunning == true)
            {
                if (turnoffCoolDown >= turnoffTime)
                {
                    sceneRunning = false;
                    turnoffCoolDown = 0;
                }
                else
                {
                    GD.Print($"RometeStarter will be initilized in {turnoffTime - turnoffCoolDown:0.f} seconds...");
                    turnoffCoolDown += (float)delta;
                }
            }

            if (!playingRequested && !sceneRunning)
            {
                RequestCommand request = new RequestCommand(3);
                request.SetMessage(0, "__target__", ai4u.Brain.STR, "envcontrol");
                request.SetMessage(1, "__remote_starter__", ai4u.Brain.STR, "remote_starter");
                request.SetMessage(2, "id", ai4u.Brain.STR, starterAgentId);
                if (RequestEnvControl(request) == null)
                {
                    //GD.Print("Listening remote agent...");
                    Thread.Sleep(10);
                }
                else
                {
                    playingRequested = true;
                    PlayScene();
                }
            }
        }
	}
    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            GD.Print("Game closed!!!");
            playingRequested = false;
        }
    }

    public Command[] RequestEnvControl(RequestCommand request)
    {

        var cmdstr = ControlRequestor.SendMessageFrom(brain, request.Command, request.Type, request.Value);

        if (cmdstr != null)
        {
            Command[] cmds = ControlRequestor.UpdateActionData(cmdstr);
            return cmds;
        }

        return null;
    }
}
