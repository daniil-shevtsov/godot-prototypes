using Godot;
using System;
using System.Linq;

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
		if (Input.IsActionJustReleased("right"))
		{
			change.X = 1;
		}
		else if (Input.IsActionJustPressed("left"))
		{
			change.X = -1;
		}
		else if (Input.IsActionJustPressed("forward"))
		{
			change.Y = 1;
		}
		else if (Input.IsActionJustPressed("backwards"))
		{
			change.Y = -1;
		}

		if (change != Vector2.Zero)
		{
			var originalArrays = projectionQuad.Mesh.SurfaceGetArrays(0);
			var originalVertices = originalArrays[(int)Mesh.ArrayType.Vertex];
			var oldVertices = originalVertices.AsVector3Array().ToList();

			var oldSize = Mathf.Abs(oldVertices.Find(vertex => vertex.X != 0).X);
			var sizeChange = change * 0.1f;
			var newSize = oldSize + 0.1f * change.X;
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

			// var oldPlaneMesh = (PlaneMesh)projectionQuad.Mesh;

			// var planeMesh = new PlaneMesh
			// {
			// 	Size = oldPlaneMesh.Size,
			// 	SubdivideDepth = oldPlaneMesh.SubdivideDepth,
			// 	SubdivideWidth = oldPlaneMesh.SubdivideWidth
			// };

			// var surfaceTool = new SurfaceTool();
			// surfaceTool.CreateFrom(planeMesh, 0);
			// var arrayPlane = surfaceTool.Commit();
			// var dataTool = new MeshDataTool();
			// dataTool.CreateFromSurface(arrayPlane, 0);

			// for (int i = 0; i < arrayPlane.GetSurfaceCount(); i++)
			// {
			// 	// There is no SurfaceRemove in Godot 4.0+ so we have to remove it manually
			// 	arrayPlane.ClearSurfaces();
			// }

			// // Commit and generate normals
			// dataTool.CommitToSurface(arrayPlane);
			// // Generate with SurfaceTool
			// surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
			// surfaceTool.CreateFrom(arrayPlane, 0);
			// surfaceTool.GenerateNormals();

			// projectionQuad.Mesh = surfaceTool.Commit();

			var meshTool = new MeshDataTool();
			// var surfaceTool = new SurfaceTool();
			// surfaceTool.CreateFrom(projectionQuad.Mesh, 0);
			// var mesh = surfaceTool.Commit();
			var mesh = new ArrayMesh();
			var arrays = projectionQuad.Mesh.SurfaceGetArrays(0);
			arrays[0] = newVertices;
			// mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);

			var projectionArrays2 = projectionQuad.Mesh.SurfaceGetArrays(0);
			mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, projectionArrays2);

			meshTool.CreateFromSurface(mesh, 0);

			GD.Print($"projectionQuad {originalVertices.AsVector3Array().Length} {meshTool.GetVertexCount()}");

			for (var i = 0; i < meshTool.GetVertexCount(); i++)
			{
				Vector3 vertex = meshTool.GetVertex(i);
				// // In this example we extend the mesh by one unit, which results in separated faces as it is flat shaded.
				// vertex += meshTool.GetVertexNormal(i);
				// Save your change.
				meshTool.SetVertex(i, newVertices[i]);
			}
			mesh.ClearSurfaces();
			meshTool.CommitToSurface(mesh);

			projectionQuad.Mesh = mesh;

			var oldVerticesString = VerticesString(oldVertices.ToArray());
			var newVerticesString = VerticesString(newVertices);

			GD.Print($"old {oldVerticesString} new {newVerticesString}");
		}

	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
	}


}
