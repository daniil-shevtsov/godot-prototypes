using Godot;
using System;
using System.Linq;
using Prototypes.projection.asset.button;
using Prototypes.projection.asset.player;

public partial class ProjectionPrototype : Node3D
{
	private TvLogic tvLogic= new();
	private PlayerLogic _playerLogic = new();
	
	private Remote remote;
	
	private RigidBody3D testPlayer;

	private MyButton groundButton;
	private MyButton colliderGroundButton;

	public Joint3D playerHolderJoint;
	
	[Export] private float mouseSensitivity = 0.01f;
	
	private float targetPitch = 0f;
	private float targetYaw = 0f;

	private float torqueY = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		tvLogic.init(this);
		_playerLogic.Init(this);
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

		var input = Input.GetVector("left", "right", "backwards", "forward");
		_playerLogic.HandleInput(input, (float)delta);

		tvLogic.Button(groundButton.GetEnabled());

		_playerLogic.PhysicsProcess((float)delta);
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_playerLogic.HandleMouseInput(mouseMotion, mouseSensitivity);
			
			targetYaw   -= mouseMotion.Relative.X * mouseSensitivity;
			targetPitch -= mouseMotion.Relative.Y * mouseSensitivity;
			targetPitch = Mathf.Clamp(targetPitch, -Mathf.Pi/2, Mathf.Pi/2);
		}

		if (Input.IsActionJustReleased("use"))
		{
			tvLogic.ToggleStretching();
			_playerLogic.HandleAction(PlayerAction.Use);
		}

		if (Input.IsActionJustReleased("toggle_camera"))
		{
			_playerLogic.HandleAction(PlayerAction.ToggleCamera);
		}
	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
	}

	

	private void initProps()
	{
		groundButton = (MyButton)FindChild("GroundButton");
		colliderGroundButton = (MyButton)FindChild("ColliderGroundButton");
		remote = (Remote)FindChild("Remote");
	}

	private void initTestPlayer()
	{
		testPlayer = GetNode<RigidBody3D>("TestPlayer");
	}
	
	


}
