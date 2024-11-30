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


		var change = 0;
		if (Input.IsActionJustReleased("right"))
		{
			change = 1;
		}
		else if (Input.IsActionJustPressed("left"))
		{
			change = -1;
		}

		if (change != 0)
		{
			var originalArrays = projectionQuad.Mesh.SurfaceGetArrays(0);
			var originalVertices = originalArrays[(int)Mesh.ArrayType.Vertex];
			var oldVertices = originalVertices.AsVector3Array().ToList();

			var oldSize = Mathf.Abs(oldVertices.Find(vertex => vertex.X != 0).X);
			var newSize = oldSize + 0.1f * change;
			Vector3[] newVertices = oldVertices.Select(vertex =>
			{
				var newVertex = Vector3.Zero;
				foreach (int index in Enumerable.Range(0, 3))
				{
					var coordinate = vertex[index];
					if (coordinate != 0f)
					{
						newVertex[index] = Mathf.Sign(coordinate) * Mathf.Abs(newSize);
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

			var projectionArrays1 = ((PlaneMesh)projectionQuad.Mesh).GetMeshArrays();
			var projectionArrays2 = projectionQuad.Mesh.SurfaceGetArrays(0);

			GD.Print($"KEK1 {VerticesString(projectionArrays1[(int)Mesh.ArrayType.Vertex].AsVector3Array())} KEK2 {VerticesString(projectionArrays2[(int)Mesh.ArrayType.Vertex].AsVector3Array())}");

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
