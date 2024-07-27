using Godot;
using System;

public partial class CabinGame : Node3D
{

	private Camera3D camera3D;
	private CameraMarker initialMarker;
	private CameraMarker knifeMarker;

	private CameraMarker currentCameraMarker = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		camera3D = GetNode<Camera3D>("Camera3D");
		initialMarker = GetNode<CameraMarker>("InitialMarker");
		knifeMarker = GetNode<CameraMarker>("KnifeMarker");

		currentCameraMarker = initialMarker;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (currentCameraMarker != null)
		{
			camera3D.GlobalPosition = currentCameraMarker.cameraPosition;
			camera3D.LookAt(currentCameraMarker.lookAtPosition);
		}
	}
}
