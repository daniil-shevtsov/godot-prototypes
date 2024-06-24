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


	private Dictionary<String, List<PersistedData>> levelData = new();

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
		if (loadedLevel != null)
		{
			RemoveChild(loadedLevel);
		}
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

		LoadPersistentData();
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
		newCan.AddToGroup("my_persist", persistent: true);
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
			SavePersistentData();
			LoadLevel(levelScene2);
		}
	}

	private void SavePersistentData()
	{
		var sceneTree = loadedLevel.GetTree();
		var nodesToPersist = sceneTree.GetNodesInGroup("my_persist");
		var dataToPersist = new List<PersistedData>();
		foreach (var node in nodesToPersist)
		{
			if (node is RigidBody3D node3D)
			{
				var data = new PersistedData
				{
					position = node3D.GlobalPosition,
					rotation = node3D.Rotation,
					linearVelocity = node3D.LinearVelocity,
					angularVelocity = node3D.AngularVelocity,
					path = node3D.GetPath()
				};
				dataToPersist.Add(data);
			}
		}
		levelData[loadedLevel.GetPath()] = dataToPersist;
	}

	private void LoadPersistentData()
	{
		var sceneTree = loadedLevel.GetTree();
		var path = loadedLevel.GetPath();
		var persistedLevel = levelData[path];
		if (persistedLevel != null)
		{
			foreach (var data in persistedLevel)
			{
				//TODO: Example does not handle that cans don't exist initially
				var destination = loadedLevel.GetNode(data.path);
				if (destination != null && destination is RigidBody3D destinationBody)
				{
					destinationBody.GlobalPosition = data.position;
					destinationBody.Rotation = data.rotation;
					destinationBody.LinearVelocity = data.linearVelocity;
					destinationBody.AngularVelocity = data.angularVelocity;
				}
			}
		}
	}

	private struct PersistedData
	{
		public Vector3 position;
		public Vector3 rotation;
		public Vector3 linearVelocity;
		public Vector3 angularVelocity;
		public NodePath path;
	}
}
