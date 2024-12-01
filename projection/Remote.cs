using Godot;
using System;

public partial class Remote : RigidBody3D
{
	public MeshInstance3D screenPlane;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		screenPlane = GetNode<MeshInstance3D>("RemoteScreenPlane");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
