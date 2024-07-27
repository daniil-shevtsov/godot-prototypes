using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CookingGame : Node2D
{
    private RigidBody2D Player;
    private RigidBody2D UpperArm;
    private RigidBody2D LowerArm;
    private RigidBody2D Gun;

    private Area2D FryingArea;
    private Area2D CookingArea;

    private PackedScene BulletScene;
    private List<RigidBody2D> Bullets = new();

    private Label label;

    private float currentScore = 0f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Player = (RigidBody2D)FindChild("Player");
        UpperArm = (RigidBody2D)FindChild("UpperArm");
        LowerArm = (RigidBody2D)FindChild("LowerArm");
        Gun = (RigidBody2D)FindChild("Gun");
        FryingArea = (Area2D)FindChild("FryingArea");
        CookingArea = (Area2D)FindChild("CookingArea");
        label = GetNode<Label>("Label");

        BulletScene = GD.Load<PackedScene>("res://cooking/bullet.tscn");

        CookingArea.BodyEntered += OnCookingAreaEntered;
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
        var explosionStrength = 10000f;
        FryingArea
            .GetOverlappingBodies()
            .ToList()
            .ForEach(
                (body) =>
                {
                    var fryingBullet = (Popcorn)Bullets.Find((bullet) => bullet == body);
                    if (fryingBullet != null)
                    {
                        var scaleChange = 0.1f * fryingSpeed * (float)delta;
                        fryingBullet.UpdateMultiplier(scaleChange);

                        if (fryingBullet.multiplier > 2f)
                        {
                            GD.Print("EXPLODE");
                            var explosionPosition = fryingBullet.GlobalPosition;
                            Bullets.ForEach(
                                (bullet) =>
                                {
                                    if (bullet != fryingBullet)
                                    {
                                        var direction = (
                                            bullet.GlobalPosition - explosionPosition
                                        ).Normalized();
                                        var distance =
                                            (bullet.GlobalPosition - explosionPosition).Length()
                                            + 0.001f;
                                        var force = explosionStrength * direction / distance;
                                        GD.Print(
                                            $"Calculated force {force} direction {direction} distance {distance}"
                                        );
                                        bullet.ApplyCentralImpulse(force);
                                    }
                                }
                            );
                            RemoveBullet(fryingBullet);
                        }
                    }
                }
            );
    }

    private void OnCookingAreaEntered(Node2D body)
    {
        var fryingBullet = (Popcorn)Bullets.Find((bullet) => bullet == body);
        if (fryingBullet != null)
        {
            var score = fryingBullet.multiplier;
            IncreaseScore(score);
            RemoveBullet(fryingBullet);
        }
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
        var bullet = (Popcorn)BulletScene.Instantiate();
        bullet.GlobalPosition = Gun.GlobalPosition;
        bullet.RotationDegrees = 90f;
        bullet.LinearVelocity = new Vector2(bulletSpeed, 10f);
        bullet.Setup(bullet.GetIndex(), 1f);
        AddChild(bullet);
        Bullets.Add(bullet);

        GD.Print($"Spawn bullet at {bullet.GlobalPosition}");
    }

    private void RemoveBullet(Popcorn popcorn)
    {
        Bullets.Remove(popcorn);
        RemoveChild(popcorn);
    }

    private void IncreaseScore(float score)
    {
        if (score >= 1.25f && score <= 2f)
        {
            currentScore += score;
        }
        else
        {
            currentScore -= 10f;
        }

        GD.Print($"Current Score: {currentScore}");
        label.Text = $"Score: {currentScore}";
    }
}
