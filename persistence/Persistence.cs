using Godot;
using System;
using System.Collections.Generic;

public partial class Persistence : Node3D
{
	private Playerr player;
	private Marker3D canSpawn;
	private PinJoint3D hand;
	private RayCast3D cameraRay;
	private PackedScene canScene;
	private PackedScene playerScene;
	private PackedScene levelScene1;

	private Node3D loadedLevel;

	private List<RigidBody3D> cans = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Game ready");
		canScene = GD.Load<PackedScene>("res://persistence/can.tscn");
		playerScene = GD.Load<PackedScene>("res://persistence/player.tscn");
		levelScene1 = GD.Load<PackedScene>("res://persistence/level1.tscn");

		loadedLevel = (Node3D)levelScene1.Instantiate();
		AddChild(loadedLevel);
		player = (Playerr)playerScene.Instantiate();
		loadedLevel.AddChild(player);

		canSpawn = (Marker3D)player.FindChild("CanSpawn");
		hand = (PinJoint3D)player.FindChild("Hand");
		cameraRay = (RayCast3D)player.FindChild("CameraRay");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionJustPressed("spawn_can"))
		{
			SpawnCan();
		}

		if (Input.IsActionJustPressed("use"))
		{
			HandleUse();
		}
	}

	private void SpawnCan()
	{
		var newCan = (RigidBody3D)canScene.Instantiate().Duplicate();

		newCan.GlobalPosition = canSpawn.GlobalPosition;
		loadedLevel.AddChild(newCan);
		cans.Add(newCan);
	}

	private void HandleUse()
	{
		GD.Print("Use");
		var collided = cameraRay.GetCollider();
		if (collided != null && collided is RigidBody3D)
		{
			var rigidBody = (RigidBody3D)collided;
			var pathToGrab = hand.GetPathTo(rigidBody);

			if (hand.NodeB != pathToGrab)
			{
				rigidBody.GlobalPosition = canSpawn.GlobalPosition;
				rigidBody.LockRotation = true;
				rigidBody.RotationDegrees = new Vector3(0f, 0f, 0f);
				hand.NodeB = pathToGrab;
			}
			else
			{
				var heldRigidBody = hand.GetNode<RigidBody3D>(hand.NodeB);
				GD.Print($"Drop {heldRigidBody} by {hand.NodeB}");
				heldRigidBody.LockRotation = false;
				hand.NodeB = null;
			}
		}
		else
		{
			if (hand.NodeB != null)
			{
				var rigidBody = hand.GetNode<RigidBody3D>(hand.NodeB);
				GD.Print($"Drop {rigidBody} by {hand.NodeB}");
				rigidBody.LockRotation = false;
				hand.NodeB = null;
			}
		}
	}
}
