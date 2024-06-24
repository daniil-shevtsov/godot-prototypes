using Godot;
using System;

public partial class Door : StaticBody3D
{

	public Area3D area;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		area = GetNode<Area3D>("Area3D");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
