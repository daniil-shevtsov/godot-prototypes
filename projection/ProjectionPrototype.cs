using Godot;
using System;
using System.Linq;

public partial class ProjectionPrototype : Node3D
{

	private MeshInstance3D projectionQuad;
	private SubViewport subViewport;

	private CanvasLayer canvasLayer;
	private PanelContainer panelContainer;
	private Label label;
	private AnimatedSprite2D shrek;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		projectionQuad = (MeshInstance3D)FindChild("ProjectionQuad");
		subViewport = (SubViewport)FindChild("SubViewport");
		canvasLayer = (CanvasLayer)FindChild("CanvasLayer");
		panelContainer = (PanelContainer)FindChild("PanelContainer");
		shrek = (AnimatedSprite2D)FindChild("AnimatedSprite2D");
		label = (Label)FindChild("Label");

		RenderingServer.ViewportSetClearMode(subViewport.GetViewportRid(), RenderingServer.ViewportClearMode.Never);
		var activeMaterial = projectionQuad.MaterialOverride;
		var kek = activeMaterial.Duplicate();
		var overrideMaterial = kek as StandardMaterial3D;

		overrideMaterial.AlbedoTexture = subViewport.GetTexture();
		projectionQuad.MaterialOverride = overrideMaterial;

		shrek.Play(shrek.Animation);
		// GD.Print($"panel {panelContainer.Size} {panelContainer.GlobalPosition}  label {label.Size} {label.GlobalPosition} {label.Position} shrek {shrek.SpriteFrames.GetFrameTexture("default", 0).GetSize() * shrek.Scale} {shrek.GlobalPosition}");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var center = panelContainer.GlobalPosition + panelContainer.Size / 2f;

		if (center != shrek.GlobalPosition)
		{
			shrek.GlobalPosition = center;
		}

		var change = Vector2.Zero;
		if (Input.IsActionPressed("right"))
		{
			change.X = 1;
		}
		else if (Input.IsActionPressed("left"))
		{
			change.X = -1;
		}
		else if (Input.IsActionPressed("forward"))
		{
			change.Y = 1;
		}
		else if (Input.IsActionPressed("backwards"))
		{
			change.Y = -1;
		}
		var speed = 1f;
		var sizeChange = change * speed * (float)delta;

		if (change != Vector2.Zero)
		{
			var originalArrays = projectionQuad.Mesh.SurfaceGetArrays(0);
			var originalVertices = originalArrays[(int)Mesh.ArrayType.Vertex];
			var oldVertices = originalVertices.AsVector3Array().ToList();

			Vector3[] newVertices = oldVertices.Select(vertex =>
			{
				var newVertex = Vector3.Zero;
				foreach (int index in Enumerable.Range(0, 3))
				{
					var coordinate = vertex[index];

					if (index == 0)
					{
						newVertex[index] = coordinate + Mathf.Sign(coordinate) * Mathf.Sign(sizeChange.X) * Mathf.Abs(sizeChange.X);
					}
					else if (index == 2)
					{
						newVertex[index] = coordinate + Mathf.Sign(coordinate) * Mathf.Sign(sizeChange.Y) * Mathf.Abs(sizeChange.Y);
					}
				}
				return newVertex;
			}).ToArray();

			var meshTool = new MeshDataTool();
			var mesh = new ArrayMesh();

			var projectionArrays = projectionQuad.Mesh.SurfaceGetArrays(0);
			mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, projectionArrays);

			meshTool.CreateFromSurface(mesh, 0);
			for (var i = 0; i < meshTool.GetVertexCount(); i++)
			{
				meshTool.SetVertex(i, newVertices[i]);
			}
			mesh.ClearSurfaces();
			meshTool.CommitToSurface(mesh);

			projectionQuad.Mesh = mesh;
		}

		var zoomMultiplier = 2;
		if (Input.IsActionJustReleased("zoom_in"))
		{
			subViewport.Size = subViewport.Size * zoomMultiplier;
		}
		else if (Input.IsActionJustReleased("zoom_out"))
		{
			subViewport.Size = subViewport.Size / zoomMultiplier;
		}
	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
	}


}
