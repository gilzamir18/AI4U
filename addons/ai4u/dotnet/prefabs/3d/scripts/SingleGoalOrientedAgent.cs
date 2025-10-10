using Godot;
using System;

namespace ai4u.tridimensional
{
    public partial class SingleGoalOrientedAgent : CharacterBody3D
    {
        [Export]
        private Node3D goal;

        [Export]
        private Node3D respawnOptionsPath;

        [Export]
        private string obstacleGroup = "wall";

        [Export]
        private int maxSteps = 500;

        public override void _Ready()
        {
            if (respawnOptionsPath != null)
            {
                GetNode<CBRespawnActuator>("Agent/Respawn").RespawnOptionsPath = respawnOptionsPath.GetPath();
            }
            GetNode<RLAgent>("Agent").MaxStepsPerEpisode = maxSteps;
            GetNode("Agent").GetNode<MinDistReward>("TargetReward").Target = goal;
            GetNode("Agent").GetNode<TouchRewardFunc>("TouchReward").Target = goal;
            GetNode("Agent").GetNode<TouchRewardFunc>("Pain").group = obstacleGroup;
            GetNode("Agent").GetNode("Sensor").GetNode<OrientationSensor>("OrientationSensor").Target = goal;
        }
    }
}
