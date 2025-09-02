using Godot;

namespace Prototypes.projection.asset.button;

public partial class ColliderButton : RigidBody3D
{
	private float _resistance = 0f;
	private float _enabledThreshold = 0.20f;

	public CollisionShape3D ButtonBaseCollider;
	public CollisionShape3D ButtonCollider;

	public Marker3D BaseBottom;
	public Marker3D BaseTop;
	
	public Marker3D ButtonBottom;
	public Marker3D ButtonTop;
	
	public MeshInstance3D ButtonMesh;

	public bool IsEnabled = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ButtonBaseCollider = (CollisionShape3D)FindChild("ButtonBaseCollider");
		ButtonCollider = (CollisionShape3D)FindChild("ButtonCollider");
		
		BaseBottom = (Marker3D)FindChild("BaseBottom");
		BaseTop = (Marker3D)FindChild("BaseTop");
		
		ButtonBottom = (Marker3D)FindChild("ButtonBottom");
		ButtonTop = (Marker3D)FindChild("ButtonTop");
		
		ButtonMesh = (MeshInstance3D)FindChild("ButtonMesh");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void Init(float resistance, float enabledThreshold)
	{
		_resistance = resistance;
		_enabledThreshold = enabledThreshold;
	}

	public bool GetEnabled()
	{
		return IsEnabled;
	}

	public override void _PhysicsProcess(double delta)
	{
		// ButtonBody.ApplyCentralImpulse(Transform.Basis.Y * _resistance * (float)delta);

		var oldIsEnabled = IsEnabled;
		var buttonHeight = ((CylinderShape3D)ButtonCollider.Shape).Height;
		// var differenceFromCenter = ButtonTop.GlobalPosition.Y - ButtonBaseCollider.GlobalPosition.Y;
		var differenceFromCenter = ButtonTop.GlobalPosition.Y - ButtonBaseCollider.GlobalPosition.Y;
		var percentageOut = (ButtonTop.GlobalPosition.Y - BaseTop.GlobalPosition.Y) / buttonHeight;
		var percentageIn = 1f-percentageOut;
		;
		GD.Print($"KEK percentage {percentageIn}");
		IsEnabled = percentageIn > _enabledThreshold;

		if (oldIsEnabled != IsEnabled)
		{
			StandardMaterial3D material = new StandardMaterial3D();
			if (IsEnabled)
			{
				material.AlbedoColor = Color.FromHtml("#00FF00");
			}
			else
			{
				material.AlbedoColor = Color.FromHtml("#FF0000");
			}
			ButtonMesh.MaterialOverride = material;
		}
	}
}
