using Godot;
using System;
using Prototypes.projection.asset.button;

public partial class PushableButton : StaticBody3D, MyButton
{
	private float _resistance = 0f;
	private float _enabledThreshold = 0.20f;

	public SliderJoint3D SliderJoint;
	public RigidBody3D ButtonBody;
	public MeshInstance3D ButtonMesh;

	public bool IsEnabled = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SliderJoint = (SliderJoint3D)FindChild("ButtonSlider");
		ButtonBody = (RigidBody3D)FindChild("ButtonBody");
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
		ButtonBody.ApplyCentralImpulse(Transform.Basis.Y * _resistance * (float)delta);

		var range = SliderJoint.GetParam(SliderJoint3D.Param.LinearLimitUpper) * 2;
		var differenceFromCenter = SliderJoint.GlobalPosition - ButtonBody.GlobalPosition;
		var oldIsEnabled = IsEnabled;
		IsEnabled = differenceFromCenter.Y > _enabledThreshold;

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
