using Godot;
using System;

public partial class Enemy : CharacterBody3D
{

    AnimationNodeStateMachinePlayback StateMachine;
    Entity entity;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        entity = GetNode<Entity>("Entity");
        GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").Active = true;
        StateMachine = (AnimationNodeStateMachinePlayback)GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").Get("parameters/playback");
        StateMachine.Travel("Idle");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (entity.Health <= 0)
        {
			StateMachine.Travel("Flying Back Death");
        }
    }
}
