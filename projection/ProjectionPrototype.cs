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
	private Tv tv;

	private PushableButton groundButton;
	private Remote remote;

	private Vector2 originalSize = Vector2.Zero;
	private Vector2 totalSizeChange = Vector2.Zero;

	private Vector2I originalSubViewportSize = Vector2I.Zero;
	private float zoomMultiplier = 1f;

	private bool isStretching = true;
	private bool isHolding = false;

	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export] private float speed = 5.0f;
	[Export] private float mouseSensitivity = 0.1f;
	[Export] private float jumpForce = 10.0f;
	[Export] private float gravity = 3.8f;

	[Export] private float pushForce = 500f; //TODO: Calculate from player mass

	private Vector3 velocity = Vector3.Zero;
	private Vector2 rotation = Vector2.Zero;

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
		tv = (Tv)FindChild("TV");
		groundButton = (PushableButton)FindChild("GroundButton");
		remote = (Remote)FindChild("Remote");

		player.fpsCamera.Current = false;
		player.tpsCamera.Current = true;

		RenderingServer.ViewportSetClearMode(subViewport.GetViewportRid(), RenderingServer.ViewportClearMode.Never);
		AssignSubviewportToQuad(subViewport, projectionQuad);
		AssignSubviewportToQuad(subViewport, remote.screenPlane);

		shrek.Play(shrek.Animation);

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

		// ResizeShrek(change, (float)delta);
	}

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

		Vector3 direction = Vector3.Zero;

		if (Input.IsActionPressed("forward"))
			direction -= player.Transform.Basis.Z;
		if (Input.IsActionPressed("backwards"))
			direction += player.Transform.Basis.Z;
		if (Input.IsActionPressed("left"))
			direction -= player.Transform.Basis.X;
		if (Input.IsActionPressed("right"))
			direction += player.Transform.Basis.X;

		direction = direction.Normalized();
		velocity.X = direction.X * speed;
		velocity.Z = direction.Z * speed;

		if (player.IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			velocity.Y = jumpForce;
		}
		else
		{
			velocity.Y += -gravity * (float)delta;
		}
		player.Velocity = velocity;

		player.MoveAndSlide();

		var collision = player.GetLastSlideCollision();
		if (collision != null)
		{
			var collider = collision.GetCollider();
			var collisionPosition = collision.GetPosition();

			if (collider is RigidBody3D)
			{
				var body = collider as RigidBody3D;
				var pushDirection = -collision.GetNormal();
				var pushPosition = collisionPosition - body.GlobalPosition;
				body.ApplyImpulse(pushDirection * pushForce * (float)delta, pushPosition);
			}
		}
		if (groundButton.IsEnabled && !shrek.IsPlaying())
		{
			shrek.Play();
		}
		else if (!groundButton.IsEnabled)
		{
			shrek.Stop();
		}

		if (isHolding)
		{
			remote.GlobalPosition = player.handMarker.GlobalPosition;
			GD.Print($"TELEPORT REMOTE");
			PhysicsServer3D.BodySetState(
				remote.GetRid(),
				PhysicsServer3D.BodyState.Transform,
				Transform3D.Identity.Translated(player.handMarker.GlobalPosition)
			);
		}
	}


	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			rotation.Y -= mouseMotion.Relative.X * mouseSensitivity;
			rotation.X -= mouseMotion.Relative.Y * mouseSensitivity;
			rotation.X = Mathf.Clamp(rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);

			player.RotationDegrees = new Vector3(0, Mathf.RadToDeg(rotation.Y), 0);
			// var currentCamera = GetViewport().GetCamera3D();
			var newCameraRotation = new Vector3(Mathf.RadToDeg(rotation.X), 0, 0);
			player.tpsCamera.RotationDegrees = newCameraRotation;
			player.fpsCamera.RotationDegrees = newCameraRotation;

		}

		if (Input.IsActionJustReleased("use"))
		{
			isStretching = !isStretching;

			GD.Print("USE PRESSED");
			isHolding = !isHolding;
			// if (isHolding)
			// {
			// 	DropRemote();
			// }
			// else
			// {
			// 	GrabRemote();
			// }

		}

		if (Input.IsActionJustReleased("toggle_camera"))
		{
			if (GetViewport().GetCamera3D() == player.tpsCamera)
			{
				player.tpsCamera.Current = false;
				player.fpsCamera.Current = true;
			}
			else
			{
				player.tpsCamera.Current = true;
				player.fpsCamera.Current = false;
			}
		}
	}

	private String VerticesString(Vector3[] vertices)
	{
		return $"{vertices.Length} {String.Join(", ", vertices.Select(vertex => $"{vertex}"))}";
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

	public void DropRemote()
	{
		isHolding = false;
	}

	public void GrabRemote()
	{
		// remote.GlobalPosition = player.handMarker.GlobalPosition;
		isHolding = true;
	}



}
