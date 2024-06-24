using Godot;
using System;
using System.Collections.Generic;

public partial class Persistence : Node3D
{
	private Playerr player;
	private Door door;
	private Marker3D canSpawn;
	private PinJoint3D hand;
	private RayCast3D cameraRay;
	private PackedScene canScene;
	private PackedScene playerScene;
	private PackedScene levelScene1;
	private PackedScene levelScene2;

	private Node3D loadedLevel;

	private List<RigidBody3D> cans = new();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("Game ready");
		canScene = GD.Load<PackedScene>("res://persistence/can.tscn");
		playerScene = GD.Load<PackedScene>("res://persistence/player.tscn");
		levelScene1 = GD.Load<PackedScene>("res://persistence/level1.tscn");
		levelScene2 = GD.Load<PackedScene>("res://persistence/level2.tscn");


		LoadLevel(levelScene1);
	}

	private void LoadLevel(PackedScene sceneToLoad)
	{
		loadedLevel = (Node3D)sceneToLoad.Instantiate();
		AddChild(loadedLevel);
		player = (Playerr)playerScene.Instantiate();
		player.GlobalPosition = loadedLevel.GlobalPosition + new Vector3(0, 2f, 0f);
		loadedLevel.AddChild(player);

		canSpawn = (Marker3D)player.FindChild("CanSpawn");
		hand = (PinJoint3D)player.FindChild("Hand");
		cameraRay = (RayCast3D)player.FindChild("CameraRay");
		door = (Door)loadedLevel.FindChild("Door");
		door.GetNode<Area3D>("Area3D").BodyEntered += HandleDoorEntered;

		GD.Print($"Loaded level: {loadedLevel} player {player} and hand {hand}");
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
		loadedLevel.AddChild(newCan);
		newCan.GlobalPosition = canSpawn.GlobalPosition;

		cans.Add(newCan);
	}

	private void HandleUse()
	{

		var collided = cameraRay.GetCollider();
		GD.Print($"Use {collided} by {cameraRay}");
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
			if (!hand.NodeB.IsEmpty)
			{
				GD.Print($"path {hand.NodeB.IsEmpty}");
				var rigidBody = hand.GetNode<RigidBody3D>(hand.NodeB);
				GD.Print($"Drop {rigidBody} by {hand.NodeB}");
				rigidBody.LockRotation = false;
				hand.NodeB = null;
			}
		}
	}

	private void HandleDoorEntered(Node3D body)
	{
		if (body == player)
		{
			GD.Print("LOAD LEVEL 2");
			LoadLevel(levelScene2);
		}
	}
}
