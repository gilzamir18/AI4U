using Godot;
namespace ai4u;

public partial class Trainer : Node
{

    protected TrainController controller;
    protected Agent agent;
    
    public void Initialize(TrainController controller, Agent agent)
    {
        this.agent = agent;
        this.controller = controller;
        OnSetup();
    }

    /// <summary>
    /// Here you allocate extra resources for your specific training loop.
    /// </summary>
    public virtual void OnSetup()
    {

    }

    /// <summary>
    /// This callback method run after agent percept a new state.
    /// </summary>
    public virtual void StateUpdated()
    {

    }
}