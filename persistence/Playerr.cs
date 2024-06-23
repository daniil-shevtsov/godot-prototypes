using Godot;
using System;

public partial class Playerr : CharacterBody3D
{
	[Export] private float speed = 5.0f;
	[Export] private float mouseSensitivity = 0.1f;
	[Export] private float jumpForce = 10.0f;
	[Export] private float gravity = 3.8f;

	private Camera3D camera;
	private Vector3 velocity = Vector3.Zero;
	private Vector2 rotation = Vector2.Zero;

	public override void _Ready()
	{
		camera = GetNode<Camera3D>("Camera3D");
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("quit"))
		{
			GetTree().Quit();
		}

		if (Input.IsActionJustPressed("toggle_mouse"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Visible)
			{
				Input.MouseMode = Input.MouseModeEnum.Captured;
			}
			else
			{
				Input.MouseMode = Input.MouseModeEnum.Visible;
			}
		}

		Vector3 direction = Vector3.Zero;

		if (Input.IsActionPressed("forward"))
			direction -= Transform.Basis.Z;
		if (Input.IsActionPressed("backwards"))
			direction += Transform.Basis.Z;
		if (Input.IsActionPressed("left"))
			direction -= Transform.Basis.X;
		if (Input.IsActionPressed("right"))
			direction += Transform.Basis.X;

		direction = direction.Normalized();
		velocity.X = direction.X * speed;
		velocity.Z = direction.Z * speed;

		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			velocity.Y = jumpForce;
		}
		else
		{
			velocity.Y += -gravity * (float)delta;
		}
		Velocity = velocity;

		MoveAndSlide();
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			rotation.Y -= mouseMotion.Relative.X * mouseSensitivity;
			rotation.X -= mouseMotion.Relative.Y * mouseSensitivity;
			GD.Print($"Before clamp: {rotation}");
			rotation.X = Mathf.Clamp(rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);

			GD.Print($"After clamp: {rotation}");
			RotationDegrees = new Vector3(0, Mathf.RadToDeg(rotation.Y), 0);
			camera.RotationDegrees = new Vector3(Mathf.RadToDeg(rotation.X), 0, 0);
		}
	}
}
