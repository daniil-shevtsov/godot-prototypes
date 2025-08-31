using Godot;
using System;
using System.Linq;

public partial class ProjectionPrototype : Node3D
{
	private TvLogic tvLogic= new();
	
	private Remote remote;
	
	private ProjectionPlayer player;
	private RigidBody3D testPlayer;

	private PushableButton groundButton;

	public Joint3D playerHolderJoint;

	private Vector2 totalSizeChange = Vector2.Zero;
	
	private bool isHolding = false;

	[Export] private float speed = 5.0f;
	[Export] private float mouseSensitivity = 0.01f;
	[Export] private float jumpForce = 10.0f;
	[Export] private float gravity = 3.8f;

	[Export] private float pushForce = 500f; //TODO: Calculate from player mass

	private Vector3 velocity = Vector3.Zero;
	private Vector2 rotation = Vector2.Zero;
	private float targetPitch = 0f;
	private float targetYaw = 0f;

	private float torqueY = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tvLogic.init(this);
		initPlayer();
		initProps();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		tvLogic.Update((float)delta);
		var change = Vector2.Zero;
		if (Input.IsActionPressed("right"))
		{
			change.X = 1;
		}
		else if (Input.IsActionPressed("left"))
		{
			change.X = -1;
		}
		else if (Input.IsActionPressed("forward"))
		{
			change.Y = 1;
		}
		else if (Input.IsActionPressed("backwards"))
		{
			change.Y = -1;
		}
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
			direction -= player.Transform.Basis.Z;
		if (Input.IsActionPressed("backwards"))
			direction += player.Transform.Basis.Z;
		if (Input.IsActionPressed("left"))
			direction -= player.Transform.Basis.X;
		if (Input.IsActionPressed("right"))
			direction += player.Transform.Basis.X;

		direction = direction.Normalized();
		velocity.X = direction.X * speed;
		velocity.Z = direction.Z * speed;

		if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			velocity.Y = jumpForce;
		}
		else
		{
			velocity.Y += -gravity * (float)delta;
		}
		player.Velocity = velocity;

		player.MoveAndSlide();

		var collision = player.GetLastSlideCollision();
		if (collision != null)
		{
			var collider = collision.GetCollider();
			var collisionPosition = collision.GetPosition();

			if (collider is RigidBody3D)
			{
				var body = collider as RigidBody3D;
				var pushDirection = -collision.GetNormal();
				var pushPosition = collisionPosition - body.GlobalPosition;
				body.ApplyImpulse(pushDirection * pushForce * (float)delta, pushPosition);
			}
		}

		tvLogic.Button(groundButton.IsEnabled);

		if (isHolding)
		{
			remote.GlobalPosition = player.handMarker.GlobalPosition;
			GD.Print($"TELEPORT REMOTE");
			PhysicsServer3D.BodySetState(
				remote.GetRid(),
				PhysicsServer3D.BodyState.Transform,
				Transform3D.Identity.Translated(player.handMarker.GlobalPosition)
			);
		}

// 		var currentYaw = testPlayer.Rotation.Y;
// 		var yawError   = Mathf.AngleDifference(currentYaw, targetYaw);
//
// 		float stiffness = 30f;
// 		float damping   = 5f;
//
// 		float torqueY = (yawError * stiffness) - (testPlayer.AngularVelocity.Y * damping);
// 		testPlayer.ApplyTorque(new Vector3(0, torqueY, 0));
//
// // Apply pitch directly to camera (not physics, just rotation)
// 		testPlayer.GetNode<Camera3D>("Camera3D").Rotation = new Vector3(targetPitch, 0, 0);
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			rotation.Y -= mouseMotion.Relative.X * mouseSensitivity;
			rotation.X -= mouseMotion.Relative.Y * mouseSensitivity;
			rotation.X = Mathf.Clamp(rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);

			// player.RotationDegrees = new Vector3(0, Mathf.RadToDeg(rotation.Y), 0);
			var newCameraRotation = new Vector3(Mathf.RadToDeg(rotation.X), 0, 0);
			// player.tpsCamera.RotationDegrees = newCameraRotation;
			// player.fpsCamera.RotationDegrees = newCameraRotation;
			// var tpsCamera = testPlayer.GetNode<Camera3D>("Camera3D");
			// if (GetViewport().GetCamera3D() != tpsCamera)
			// {
			// 	player.tpsCamera.Current = false;
			// 	player.fpsCamera.Current = false;
			// 	tpsCamera.Current = true;
			// }
			
			targetYaw   -= mouseMotion.Relative.X * mouseSensitivity;
			targetPitch -= mouseMotion.Relative.Y * mouseSensitivity;
			targetPitch = Mathf.Clamp(targetPitch, -Mathf.Pi/2, Mathf.Pi/2);
		}

		if (Input.IsActionJustReleased("use"))
		{
			tvLogic.ToggleStretching();

			GD.Print("USE PRESSED");
			isHolding = !isHolding;
			// if (isHolding)
			// {
			// 	DropRemote();
			// }
			// else
			// {
			// 	GrabRemote();
			// }

		}

		if (Input.IsActionJustReleased("toggle_camera"))
		{
			if (GetViewport().GetCamera3D() == player.tpsCamera)
			{
				player.tpsCamera.Current = false;
				player.fpsCamera.Current = true;
			}
			else
			{
				player.tpsCamera.Current = true;
				player.fpsCamera.Current = false;
			}
		}
	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
	}

	public void DropRemote()
	{
		isHolding = false;
	}

	public void GrabRemote()
	{
		// remote.GlobalPosition = player.handMarker.GlobalPosition;
		isHolding = true;
	}

	private void initProps()
	{
		groundButton = (PushableButton)FindChild("GroundButton");
		remote = (Remote)FindChild("Remote");
	}

	private void initPlayer()
	{
		player = (ProjectionPlayer)FindChild("Player");
		player.fpsCamera.Current = false;
		player.tpsCamera.Current = true;
		
		//var kek = player.playerHolderJoint.GlobalPosition;
		//kek.Y = kek.Y + remote.collisionShape.Size.Y / 2f;
		//remote.GlobalPosition = kek;
		//player.playerHolderJoint.NodeB = remote.GetPath();
	}

	private void initTestPlayer()
	{
		testPlayer = GetNode<RigidBody3D>("TestPlayer");
	}
	
	


}
