using Godot;
using System;

public partial class ProjectionPrototype : Node3D
{

	private MeshInstance3D projectionQuad;
	private SubViewport subViewport;

	private PanelContainer panelContainer;
	private Label label;
	private AnimatedSprite2D shrek;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		projectionQuad = (MeshInstance3D)FindChild("ProjectionQuad");
		subViewport = (SubViewport)FindChild("SubViewport");
		panelContainer = (PanelContainer)FindChild("PanelContainer");
		shrek = (AnimatedSprite2D)FindChild("AnimatedSprite2D");
		label = (Label)FindChild("Label");

		RenderingServer.ViewportSetClearMode(subViewport.GetViewportRid(), RenderingServer.ViewportClearMode.Never);
		GD.Print($"projectionQuad {projectionQuad} {projectionQuad.MaterialOverride}");
		var activeMaterial = projectionQuad.MaterialOverride;
		var kek = activeMaterial.Duplicate();
		GD.Print($" {activeMaterial} {kek}");
		var overrideMaterial = kek as StandardMaterial3D;
		GD.Print($"{overrideMaterial}");

		overrideMaterial.AlbedoTexture = subViewport.GetTexture();
		projectionQuad.MaterialOverride = overrideMaterial;

		shrek.Play(shrek.Animation);
		// GD.Print($"panel {panelContainer.Size} {panelContainer.GlobalPosition}  label {label.Size} {label.GlobalPosition} {label.Position} shrek {shrek.SpriteFrames.GetFrameTexture("default", 0).GetSize() * shrek.Scale} {shrek.GlobalPosition}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		GD.Print($"is Playing {shrek.IsPlaying()}");
		// if (label.GlobalPosition != shrek.GlobalPosition)
		// {
		// 	shrek.GlobalPosition = label.GlobalPosition;
		// }
	}
}
