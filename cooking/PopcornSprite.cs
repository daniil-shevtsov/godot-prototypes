using Godot;
using System;

public partial class PopcornSprite : Node2D
{
    private CollisionShape2D collisionShape2D;
    private CapsuleShape2D capsuleShape;

    public Color color = new Color(1f, 1f, 1f);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    public void SetShape(CollisionShape2D shape)
    {
        collisionShape2D = shape;
        capsuleShape = (CapsuleShape2D)shape.Shape;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _Draw()
    {
        var center = collisionShape2D.Position;
        float radius = capsuleShape.Radius;
        GD.Print($"Color {color}");
        DrawCircle(center, radius, color);
        var rect = new Rect2(center, new Vector2(capsuleShape.Radius, capsuleShape.Height));
        DrawRect(rect, color);
        DrawCircle(center, radius, color);
    }
}
