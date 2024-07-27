using Godot;
using System;

public partial class CameraMarker : Node3D
{

	[Export]
	public Vector3 cameraPosition = Vector3.Zero;
	[Export]
	public Vector3 lookAtPosition = Vector3.Zero;
	private Marker3D cameraMarker;
	private Marker3D lookAtMarker;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cameraMarker = GetNode<Marker3D>("CameraMarker");
		lookAtMarker = GetNode<Marker3D>("LookAtMarker");

		if (cameraPosition == null)
		{
			cameraPosition = GlobalPosition;
		}

		cameraMarker.Position = cameraPosition;
		lookAtMarker.Position = lookAtPosition;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (cameraMarker.GlobalPosition != cameraPosition)
		{
			cameraMarker.GlobalPosition = cameraPosition;
		}

		if (lookAtMarker.GlobalPosition != lookAtPosition)
		{
			lookAtMarker.GlobalPosition = lookAtPosition;
		}
	}
}
