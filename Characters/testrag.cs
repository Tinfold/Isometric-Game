using Godot;
using System;

public partial class testrag : Skeleton3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		PhysicalBonesStartSimulation();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
