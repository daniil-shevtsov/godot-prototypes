using Godot;
using System;

public partial class Game : Node2D
{

	private RigidBody2D Player;
	private RigidBody2D UpperArm;
	private RigidBody2D LowerArm;
	private RigidBody2D Gun;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Player = (RigidBody2D)FindChild("Player");
		UpperArm = (RigidBody2D)FindChild("UpperArm");
		LowerArm = (RigidBody2D)FindChild("LowerArm");
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
		var force = 5;
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			//Gun.LookAt(GetGlobalMousePosition());
			// Gun.ApplyCentralImpulse(eventMouseMotion.Relative * force);
			// Gun.MoveAndCollide(eventMouseMotion.Relative);
			var armLength = 450f - 30f;
			var newDistance = Player.GlobalPosition.DistanceTo(Gun.GlobalPosition + eventMouseMotion.Relative);
			if (newDistance <= armLength)
			{
				Gun.MoveAndCollide(eventMouseMotion.Relative);
			}
		}
	}
}
