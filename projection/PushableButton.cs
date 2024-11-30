using Godot;
using System;

public partial class PushableButton : StaticBody3D
{
	[Export]
	public float resistance = 0f;

	public SliderJoint3D SliderJoint;
	public RigidBody3D ButtonBody;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SliderJoint = (SliderJoint3D)FindChild("ButtonSlider");
		ButtonBody = (RigidBody3D)FindChild("ButtonBody");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		ButtonBody.ApplyCentralImpulse(Transform.Basis.Y * resistance * (float)delta);
	}
}
