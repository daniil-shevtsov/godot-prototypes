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
	private ProjectionPlayer player;

	private Vector2 originalSize = Vector2.Zero;
	private Vector2 totalSizeChange = Vector2.Zero;

	private Vector2I originalSubViewportSize = Vector2I.Zero;
	private float zoomMultiplier = 1f;

	private bool isStretching = true;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	// Get the gravity from the project settings to be synced with RigidBody nodes.
	public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		projectionQuad = (MeshInstance3D)FindChild("ProjectionQuad");
		subViewport = (SubViewport)FindChild("SubViewport");
		canvasLayer = (CanvasLayer)FindChild("CanvasLayer");
		panelContainer = (PanelContainer)FindChild("PanelContainer");
		shrek = (AnimatedSprite2D)FindChild("AnimatedSprite2D");
		label = (Label)FindChild("Label");
		player = (ProjectionPlayer)FindChild("Player");

		RenderingServer.ViewportSetClearMode(subViewport.GetViewportRid(), RenderingServer.ViewportClearMode.Never);
		var activeMaterial = projectionQuad.MaterialOverride;
		var kek = activeMaterial.Duplicate();
		var overrideMaterial = kek as StandardMaterial3D;

		overrideMaterial.AlbedoTexture = subViewport.GetTexture();
		projectionQuad.MaterialOverride = overrideMaterial;

		shrek.Play(shrek.Animation);

		originalSize = new Vector2(
			projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.X > 0f).X * 2f,
			projectionQuad.Mesh.SurfaceGetArrays(0)[(int)Mesh.ArrayType.Vertex].AsVector3Array().ToList().Find(kek => kek.Z > 0f).Z * 2f
		);

		originalSubViewportSize = subViewport.Size;

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
				GD.Print($"multiplier: ${multiplier}");
				subViewport.Size = new Vector2I(
					(int)(originalSubViewportSize.X * multiplier.X),
					(int)(originalSubViewportSize.Y * multiplier.Y)
				);
			}
		}

		zoomMultiplier = 2;

		if (Input.IsActionJustReleased("use"))
		{
			isStretching = !isStretching;
		}

		// if (Input.IsActionJustReleased("zoom_in"))
		// {
		// 	subViewport.Size = subViewport.Size * zoomMultiplier;
		// }
		// else if (Input.IsActionJustReleased("zoom_out"))
		// {
		// 	subViewport.Size = subViewport.Size / zoomMultiplier;
		// }
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = player.Velocity;

		// Add the gravity.
		if (!player.IsOnFloor())
		{
			velocity.Y -= gravity * (float)delta;
		}


		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && player.IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}


		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(player.Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(player.Velocity.Z, 0, Speed);
		}

		player.Velocity = velocity;
		player.MoveAndSlide();
	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
	}


}
