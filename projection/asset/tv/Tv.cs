using Godot;
using System;
using System.Linq;

public partial class Tv : Node3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}

partial class TvLogic
{
	private MeshInstance3D projectionQuad;
	private SubViewport subViewport;
	
	private CanvasLayer canvasLayer;
	private PanelContainer panelContainer;
	private Label label;
	private AnimatedSprite2D shrek;
	private Tv tv;
	private Remote remote;


	public void init(Node root)
	{
		remote = (Remote)root.FindChild("Remote");

		initTv(root);

	}

	public void Update(float delta)
	{
		var center = panelContainer.GlobalPosition + panelContainer.Size / 2f;

		if (center != shrek.GlobalPosition)
		{
			shrek.GlobalPosition = center;
		}
		
		// ResizeShrek(change, (float)delta);
	}

	public void Button(bool isEnabled)
	{
		if (isEnabled && !shrek.IsPlaying())
		{
			shrek.Play();
		}
		else if (!isEnabled)
		{
			shrek.Stop();
		}
	}

	public void ToggleStretching()
	{
		isStretching = !isStretching;
	}

	private void initTv(Node root)
	{
		tv = (Tv)root.FindChild("TV");
		initShrek(root);
	}
	
	private void initShrek(Node root)
	{
		projectionQuad = (MeshInstance3D)root.FindChild("ProjectionQuad");
		subViewport = (SubViewport)root.FindChild("SubViewport");
		canvasLayer = (CanvasLayer)root.FindChild("CanvasLayer");
		panelContainer = (PanelContainer)root.FindChild("PanelContainer");
		shrek = (AnimatedSprite2D)root.FindChild("AnimatedSprite2D");
		label = (Label)root.FindChild("Label");
		
		initShrekResizeHacks();
		shrek.Play(shrek.Animation);
	}

	void initShrekResizeHacks()
	{
		RenderingServer.ViewportSetClearMode(subViewport.GetViewportRid(), RenderingServer.ViewportClearMode.Never);
		AssignSubviewportToQuad(subViewport, projectionQuad);
		AssignSubviewportToQuad(subViewport, remote.screenPlane);
		
		originalSize = new Vector2(
			projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.X > 0f).X * 2f,
			projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.Z > 0f).Z * 2f
		);

		originalSubViewportSize = subViewport.Size;
	}

	private void AssignSubviewportToQuad(SubViewport subViewport, MeshInstance3D quad)
	{
		var activeMaterial = quad.MaterialOverride;
		var kek = activeMaterial.Duplicate();
		var overrideMaterial = kek as StandardMaterial3D;

		overrideMaterial.AlbedoTexture = subViewport.GetTexture();
		quad.MaterialOverride = overrideMaterial;
	}
	
	private void ResizeShrek(Vector2 change, float delta)
	{
		var speed = 1f;
		var sizeChange = change * speed * delta;

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

			if (isStretching)
			{
				subViewport.Size = originalSubViewportSize;
			}
			else
			{
				var currentSize = new Vector2(
					projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.X > 0f).X * 2f,
					projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.Z > 0f).Z * 2f
				);

				var multiplier = new Vector2(
					 currentSize.X / originalSize.X,
					 currentSize.Y / originalSize.Y
				);
				subViewport.Size = new Vector2I(
					(int)(originalSubViewportSize.X * multiplier.X),
					(int)(originalSubViewportSize.Y * multiplier.Y)
				);
			}
		}

		zoomMultiplier = 2;
	}
	
	private Vector2 originalSize = Vector2.Zero;
	private Vector2I originalSubViewportSize = Vector2I.Zero;
	private float zoomMultiplier = 1f;

	private bool isStretching = true;

}
