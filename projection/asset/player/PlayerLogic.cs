using Godot;

namespace Prototypes.projection.asset.player;

public enum PlayerAction
{
    Use,
    ToggleCamera
}
public class PlayerLogic
{
    private Node _root;
    private ProjectionPlayer player;
    private Remote remote;


    private Vector3 velocity = Vector3.Zero;
    private Vector2 rotation = Vector2.Zero;

    public void Init(Node root)
    {
        _root = root;
        player = root.FindChild("Player") as ProjectionPlayer;
        initPlayer();
        remote = root.FindChild("Remote") as Remote;
    }
    
    private void initPlayer()
    {
       SwitchCamera();
		
        //var kek = player.playerHolderJoint.GlobalPosition;
        //kek.Y = kek.Y + remote.collisionShape.Size.Y / 2f;
        //remote.GlobalPosition = kek;
        //player.playerHolderJoint.NodeB = remote.GetPath();
    }

    private void SwitchCamera()
    {
        if (_root.GetViewport().GetCamera3D() == player.tpsCamera)
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
    
    public void PhysicsProcess(float delta)
    {
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
        
        player.Velocity = velocity;

        player.MoveAndSlide();

        var collision = player.GetLastSlideCollision();
        if (collision != null)
        {
            var collider = collision.GetCollider();
            var collisionPosition = collision.GetPosition();

            if (collider is RigidBody3D body)
            {
                var pushDirection = -collision.GetNormal();
                var pushPosition = collisionPosition - body.GlobalPosition;

                body.ApplyImpulse(pushDirection * pushForce, pushPosition); // remove * delta
            }
        }
    }

    public void HandleMouseInput(InputEventMouseMotion mouseMotion, float mouseSensitivity)
    {
        rotation.Y -= mouseMotion.Relative.X * mouseSensitivity;
        rotation.X -= mouseMotion.Relative.Y * mouseSensitivity;
        rotation.X = Mathf.Clamp(rotation.X, -Mathf.Pi / 2, Mathf.Pi / 2);

        // player.RotationDegrees = new Vector3(0, Mathf.RadToDeg(rotation.Y), 0);
        var newCameraRotation = new Vector3(Mathf.RadToDeg(rotation.X), 0, 0);
        // player.tpsCamera.RotationDegrees = newCameraRotation;
        // player.fpsCamera.RotationDegrees = newCameraRotation;
        // var tpsCamera = testPlayer.GetNode<Camera3D>("Camera3D");
        // if (GetViewport().GetCamera3D() != tpsCamera)
        // {
        // 	player.tpsCamera.Current = false;
        // 	player.fpsCamera.Current = false;
        // 	tpsCamera.Current = true;
        // }
    }

    public void HandleAction(PlayerAction action)
    {
        if (action == PlayerAction.Use)
        {
            isHolding = !isHolding;
            // if (isHolding)
            // {
            // 	DropRemote();
            // }
            // else
            // {
            // 	GrabRemote();
            // }
        } else if (action == PlayerAction.ToggleCamera)
        {
            SwitchCamera();
        }
    }
    
    public void HandleInput(Vector2 input, float delta)
    {
        Vector3 direction = Vector3.Zero;

        if (input.Y > 0)
            direction -= player.Transform.Basis.Z;
        if (input.Y < 0)
            direction += player.Transform.Basis.Z;
        if (input.X < 0)
            direction -= player.Transform.Basis.X;
        if (input.X > 0)
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
    }
    
    private void DropRemote()
    {
        isHolding = false;
    }

    private void GrabRemote()
    {
        // remote.GlobalPosition = player.handMarker.GlobalPosition;
        isHolding = true;
    }
    
    [Export] private float speed = 5.0f;
    [Export] private float jumpForce = 3.0f;
    [Export] private float gravity = 3.8f;
    [Export] private float pushForce = 1f; //TODO: Calculate from player mass

    
    private bool isHolding = false;


}