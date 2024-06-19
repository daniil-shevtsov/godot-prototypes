using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Game : Node2D
{
    private RigidBody2D Player;
    private RigidBody2D UpperArm;
    private RigidBody2D LowerArm;
    private RigidBody2D Gun;

    private Area2D FryingArea;

    private PackedScene BulletScene;
    private List<RigidBody2D> Bullets = new();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player = (RigidBody2D)FindChild("Player");
        UpperArm = (RigidBody2D)FindChild("UpperArm");
        LowerArm = (RigidBody2D)FindChild("LowerArm");
        Gun = (RigidBody2D)FindChild("Gun");
        FryingArea = (Area2D)FindChild("FryingArea");

        BulletScene = GD.Load<PackedScene>("res://bullet.tscn");

        // FryingArea.BodyEntered += OnFryingAreaEntered;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

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
        var fryingSpeed = 10f;
        FryingArea
            .GetOverlappingBodies()
            .ToList()
            .ForEach(
                (body) =>
                {
                    var fryingBullet = Bullets.Find((bullet) => bullet == body);
                    GD.Print($"Frying bullet {fryingBullet}");
                    if (fryingBullet != null)
                    {
                        var scaleChange = new Vector2(0.1f, 0.1f) * fryingSpeed * (float)delta;
                        var shape = fryingBullet.GetNode<CollisionShape2D>("CollisionShape2D");
                        shape.Scale += scaleChange;
                        fryingBullet.Mass += scaleChange.X;
                        GD.Print($"New scale: {fryingBullet.Scale}");
                    }
                }
            );
    }

    // private void OnFryingAreaEntered(Node2D body)
    // {
    //     var fryingBullet = Bullets.Find((bullet) => bullet == body);
    //     GD.Print($"Frying bullet {fryingBullet}");
    //     if (fryingBullet != null)
    //     {
    //         var scaleChange = new Vector2(1.1f, 1.1f);
    //         var shape = fryingBullet.GetNode<CollisionShape2D>("CollisionShape2D");
    //         shape.Scale += scaleChange;
    //         fryingBullet.Mass += scaleChange.X;
    //         GD.Print($"New scale: {fryingBullet.Scale}");
    //     }
    // }

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
        var sensitivity = 0.75f;
        var distanceChange = eventMouseMotion.Relative * 0.5f;
        var newDistance = Player.GlobalPosition.DistanceTo(Gun.GlobalPosition + distanceChange);
        if (newDistance <= armLength)
        {
            var collision = Gun.MoveAndCollide(distanceChange);
            if (collision != null)
            {
                HandleCollision(collision);
            }
            else
            {
                Gun.GlobalPosition += distanceChange;
            }
        }
    }

    private void HandleCollision(KinematicCollision2D collision)
    {
        var remainder = collision.GetRemainder();
        var collider = collision.GetCollider() as RigidBody2D;
        var mass = collider.Mass;
        Vector2 normal = collision.GetNormal();
        float forceFactor = 10.0f;

        Vector2 impulse = remainder.Length() * -normal * forceFactor;

        GD.Print(
            $"Parsed collider {collider} remainder {remainder} normal {normal} impulse {impulse} mass {mass}"
        );

        collider.ApplyCentralImpulse(impulse / mass);

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
        Bullets.Add(bullet);

        GD.Print($"Spawn bullet at {bullet.GlobalPosition}");
    }
}
