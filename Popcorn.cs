using Godot;
using System;
using System.Security.Principal;

public partial class Popcorn : RigidBody2D
{
    public long id;
    public float multiplier;
    public CollisionShape2D collisionShape2D;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        collisionShape2D = GetNode<CollisionShape2D>("CollisionShape2D");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public void Setup(long id, float initialMultiplier)
    {
        this.id = id;
        this.multiplier = initialMultiplier;
    }

    public void UpdateMultiplier(float multiplierChange)
    {
        multiplier += multiplierChange;
        collisionShape2D.Scale += new Vector2(multiplierChange, multiplierChange);
        Mass += multiplierChange;
    }
}
