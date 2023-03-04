using Godot;
using System;

//asdf
// IDEA: Use another camera to raycast for the aiming.
// This one will be a top-down camera.
// This might give us more accurate raycast results.
public partial class Player : CharacterBody3D
{
    public const float Speed = 5.0f;
    public const float JumpVelocity = 4.5f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    [Export]
    public Node3D CameraPivot;

    private Camera3D camera;

    PackedScene testBullet;
    PackedScene testShootParticle;

    AnimationNodeStateMachinePlayback StateMachine;


    public override void _Ready()
    {
        camera = CameraPivot.GetNode<Camera3D>("Camera");
        testBullet = ResourceLoader.Load<PackedScene>("Projectiles/test_bullet.tscn");
        testShootParticle = ResourceLoader.Load<PackedScene>("VFX/TestShootParticle.tscn");

        GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").Active = true;
        //GetNode<AnimationPlayer>("Character/AnimationPlayer").play
        //GetNode<AnimationPlayer>("Character/AnimationPlayer").Play("Walk");
        StateMachine = (AnimationNodeStateMachinePlayback)GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").Get("parameters/playback");
        StateMachine.Travel("Idle");
        GetNode<Marker3D>("Character/Armature/Skeleton3D/LeftHandMarker/Target").GlobalPosition = GetNode<Node3D>("ItemPoint").GlobalPosition;
        GetNode<Marker3D>("Character/Armature/Skeleton3D/RightHandMarker/Target").GlobalPosition = GetNode<Node3D>("ItemPoint").GlobalPosition;
        GetNode<SkeletonIK3D>("Character/Armature/Skeleton3D/LeftArmIK").Start();
        GetNode<SkeletonIK3D>("Character/Armature/Skeleton3D/RightArmIK").Start();
        // GetNode<SkeletonIK3D>("Character/Human Armature/Skeleton3D/LowerLeftArmIK").Start();
        //GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").state
        //GetNode<AnimationTree>("Character/AnimationPlayer/AnimationTree").Active=true;
    }

    // There are probably two ways to handle strafing
    // One would be to have the torso just straight up be separate from the legs.]
    // I think that's actually a really good idea potentially.
    // The other is to change which strafing animation depending on the direction we are moving and the direction we are aiming.
    // I will try that one first... However I feel that having the torso and legs separate might end up being the most dynamic solution in the long run.

    private void RunAndStrafe()
    {
        if (Velocity.Length() == 0)
        {
            StateMachine.Travel("Idle");
        }
        else
        {
            // Find angle between direction facing, direction moving.
            //GD.Print(Basis.z.SignedAngleTo(Velocity,Vector3.Up));
            float angle = Basis.Z.SignedAngleTo(Velocity, Vector3.Up);
            // If it's close to zero, we should be running BACKWARDS
            if (Mathf.Abs(0 - Mathf.Abs(angle)) < MathF.PI / 4)
            {
                StateMachine.Travel("Running Backward");
            }
            else if (angle > MathF.PI / 4 && angle < 2.5)
            {
                StateMachine.Travel("Right Strafe");
            }
            else if (angle < -MathF.PI / 4 && angle > -2.5)
            {
                StateMachine.Travel("Left Strafe");
                // GD.Print("LEFT STRAFE");
            }
            else
            {
                StateMachine.Travel("Slow Run");
            }
            //if (angle<=Mathf.Pi/2  && angle>0) {
            //  GD.Print("WOAH"); // LEFT STRAFE
            // StateMachine.Travel("Left Strafe");
            // }
        }
    }

    // The player must look towards the mouse at all times.
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Handle Jump.
        if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
            velocity.Y = JumpVelocity;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector("left", "right", "up", "down");
        //  Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

        // Using the CameraPivot's basis so that we are moving in the right direction regardless of camera rotation.
        Vector3 direction = (CameraPivot.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();

        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
        }

        Velocity = velocity;
        MoveAndSlide();
        RunAndStrafe();

        // Factor in camera rotation and also this magic offset number
        float offset = (-Mathf.Pi * 0.5f) + CameraPivot.Rotation.Y;
        // This code was on some godot forum post and it fuckin' works...
        // This faces towards the LITERAL mouse position
        // The default cursor/crosshair may have to be completely removed
        // Otherwise we will have to come up with a raycast solution
        /*
         * Vector2 screenPos = camera.UnprojectPosition(GlobalTransform.origin);
         * Vector2 mousePos = GetViewport().GetMousePosition();
         * camera.ProjectLocalRayNormal
         * float angle = screenPos.AngleToPoint(mousePos);
         * Rotation = new Vector3(Rotation.x, -angle + offset, Rotation.z);
         */

        HandleOrientation();
    }

    private void HandleOrientation()
    {
        // This would be the raycast solution
        Vector2 mousePos = GetViewport().GetMousePosition();


        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePos) * 3000;
        PhysicsRayQueryParameters3D ray = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd, (uint)Math.Pow(2, 32 - 1));
        //TODO: need to make sure the position key exists
        var op = GetWorld3D().DirectSpaceState.IntersectRay(ray);

        if (op != null && op.ContainsKey("position"))
        {
            Vector3 intersection = op["position"].AsVector3();
            Vector3 dir = GlobalPosition.DirectionTo(intersection);
            LookAt(new Vector3(intersection.X, GlobalPosition.Y, intersection.Z));
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed && eventMouseButton.ButtonIndex == MouseButton.Left)
        {
            //This must get rid of itself automatically. Later on we should make a garbage collection system
            ShootFX();
            CreateTestProjectile();
        }
    }

    private async void ShootFX()
    {
        GetNode<AnimationPlayer>("ItemPoint/Pistol/AnimationPlayer").Play("Fire");
        Node3D point = GetNode<Node3D>("ProjectilePoint");
        Node3D p = (Node3D)testShootParticle.Instantiate();
        p.Position = point.GlobalPosition;
        p.Basis = new Basis(Basis.X, Basis.Y, -Basis.Z);
        GetParent().AddChild(p);
        p.GetNode<CpuParticles3D>("Smoke").GlobalRotation = new Vector3(p.GetNode<CpuParticles3D>("Smoke").GlobalRotation.X, p.GetNode<CpuParticles3D>("Smoke").GlobalRotation.Y, 0);
        p.GetNode<CpuParticles3D>("Smoke").Emitting = true;
        p.GetNode<CpuParticles3D>("Muzzle").Emitting = true;
        p.GetNode<CpuParticles3D>("Muzzle2").Emitting = true;
        p.GetNode<CpuParticles3D>("Muzzle3").Emitting = true;
        SceneTreeTimer timer = GetTree().CreateTimer(1.0);
        await ToSignal(timer, "timeout");
        p.QueueFree();
    }

    private void CreateTestProjectile()
    {
        Node3D point = GetNode<Node3D>("ProjectilePoint");
        Area3D newBullet = (Area3D)testBullet.Instantiate();

        //newBullet.Freeze = false;
        // Only needs to be the forward of the bullet point
        newBullet.Position = point.GlobalPosition;
        newBullet.Set("SourceEntity", this);
        newBullet.Basis = new Basis(newBullet.Basis.X, newBullet.Basis.Y, point.GlobalTransform.Basis.Z.Normalized());
        GetParent().AddChild(newBullet);

        // idk im so fucking bad
        //newBullet.ApplyImpulse(-point.GlobalTransform.basis.z.Normalized() * 150);
        //Callable test=new Callable(this,nameof(onCollide));
        //newBullet.Connect("body_entered",test);
    }
}