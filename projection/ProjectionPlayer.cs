using Godot;
using System;

public partial class ProjectionPlayer : CharacterBody3D
{

    public Camera3D tpsCamera;
    public Camera3D fpsCamera;

    public override void _Ready()
    {
        tpsCamera = GetNode<Camera3D>("ThirdPersonCamera");
        fpsCamera = GetNode<Camera3D>("FirstPersonCamera");

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

}
