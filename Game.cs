using Godot;
using System;

public partial class Game : Node2D
{

	private RigidBody2D Gun;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Gun = (RigidBody2D)FindChild("Gun");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _Input(InputEvent @event)
	{
		var sensitivity = 100;
		var speed = 50;
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			Gun.LookAt(GetGlobalMousePosition());
			Gun.MoveAndCollide(eventMouseMotion.Relative);
		}
	}
}
