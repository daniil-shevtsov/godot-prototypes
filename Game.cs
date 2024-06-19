using Godot;
using System;

public partial class Game : Node2D
{
    private RigidBody2D Player;
    private RigidBody2D UpperArm;
    private RigidBody2D LowerArm;
    private RigidBody2D Gun;

    private PackedScene BulletScene;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player = (RigidBody2D)FindChild("Player");
        UpperArm = (RigidBody2D)FindChild("UpperArm");
        LowerArm = (RigidBody2D)FindChild("LowerArm");
        Gun = (RigidBody2D)FindChild("Gun");

        BulletScene = GD.Load<PackedScene>("res://bullet.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _PhysicsProcess(double delta)
    {
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

        if (Input.IsMouseButtonPressed(MouseButton.Left))
        {
            SpawnBullet();
        }

        var gunDegreesIncrement = 0f;
        var kek = 1f;
        var rotationSensitivity = 10f;
        if (Input.IsActionJustPressed("rotate_clockwise"))
        {
            gunDegreesIncrement = kek;
        }
        else if (Input.IsActionJustPressed("rotate_counterclockwise"))
        {
            gunDegreesIncrement = -kek;
        }
        var degrees = Input.GetAxis("rotate_clockwise", "rotate_counterclockwise");
        if (gunDegreesIncrement != 0)
        {
            GD.Print($"gunDegreesIncrement: {gunDegreesIncrement}");
        }
        Gun.RotationDegrees = Gun.RotationDegrees + gunDegreesIncrement * rotationSensitivity;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            MoveGun(eventMouseMotion);
        }
    }

    private void MoveGun(InputEventMouseMotion eventMouseMotion)
    {
        var armLength = 450f - 30f;
        var newDistance = Player.GlobalPosition.DistanceTo(
            Gun.GlobalPosition + eventMouseMotion.Relative
        );
        if (newDistance <= armLength)
        {
            var collision = Gun.MoveAndCollide(eventMouseMotion.Relative);
            if (collision != null)
            {
                HandleCollision(collision);
            }
            else
            {
                Gun.GlobalPosition += eventMouseMotion.Relative;
            }
        }
    }

    private void HandleCollision(KinematicCollision2D collision)
    {
        var remainder = collision.GetRemainder();
        var collider = collision.GetCollider() as RigidBody2D;
        var mass = collider.Mass;
        Vector2 normal = collision.GetNormal();
        float forceFactor = 50.0f;

        Vector2 impulse = remainder.Length() * -normal * forceFactor * mass;

        GD.Print(
            $"Parsed collider {collider} remainder {remainder} normal {normal} impulse {impulse}"
        );

        collider.ApplyCentralImpulse(impulse);

        Gun.GlobalPosition += remainder;
    }

    private void SpawnBullet()
    {
        var bulletSpeed = 10f;
        var bullet = (RigidBody2D)BulletScene.Instantiate();
        bullet.GlobalPosition = Gun.GlobalPosition;
        bullet.RotationDegrees = 90f;
        bullet.LinearVelocity = new Vector2(bulletSpeed, 10f);
        AddChild(bullet);

        GD.Print($"Spawn bullet at {bullet.GlobalPosition}");
    }
}
