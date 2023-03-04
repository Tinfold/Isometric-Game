using Godot;
using System;

public partial class Projectile : Area3D
{
    // later on this class will have various properties, like Destroy on Hit, timer, etc.

    [Export]
    Node3D SourceEntity { set; get; }

    [Export]
    public int BulletSpeed;

    [Export]
    public int TimeBeforeDespawn;

    // TODO: Time before remove impact particle
    // Among various other settings...

    private bool HasCollided = false; // Can be replaced with #collisions in the future
    // Called when the node enters the scene tree for the first time.
    private Vector3 LocalCollisionPosition;

    private Vector3 LocalCollisionNormal;

    private Vector3 BulletLastPos;

    PackedScene ImpactParticles;
    PackedScene BloodSplatter;
    public override void _Ready()
    {
        QueueForDespawn();
        BulletLastPos = GlobalPosition;
        ImpactParticles = ResourceLoader.Load<PackedScene>("VFX/TestProjectileImpact.tscn");
        BloodSplatter = ResourceLoader.Load<PackedScene>("VFX/BloodSplatter.tscn");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {

        Translate(new Vector3(0, 0, -1 * BulletSpeed) * (float)delta);
        CheckHit(BulletLastPos);
        BulletLastPos = GlobalPosition;
    }

    async private void CheckHit(Vector3 lastPos)
    {
        var State = GetWorld3D().DirectSpaceState;
        Vector3 OurPos = GlobalPosition;
        var query = PhysicsRayQueryParameters3D.Create(lastPos, OurPos, 1);
        var result = State.IntersectRay(query);
        if (result != null && result.ContainsKey("collider") && HasCollided == false)
        {
            //GD.Print(result["collider"]);
            //QueueFree();
            HasCollided = true;
            //Godot.Object PhysObj = (Godot.Object)result["collider"];
            var PhysObj = result["collider"];
            //Object PhysObj = (Object)result["collider"];
            if (PhysObj.AsGodotObject().IsClass("RigidBody3D"))
            {
                ((RigidBody3D)PhysObj).ApplyImpulse(-result["normal"].AsVector3() * 5);
            }
            if (PhysObj.AsGodotObject().IsClass("Node3D"))
            {
                Node3D Obj = ((Node3D)(PhysObj));
                if (Obj.HasNode("Entity"))
                {
                    Obj.GetNode<Entity>("Entity").TakeDamage(5);
                    GD.Print(Obj.GetNode<Entity>("Entity").Health);
                    if (Obj.GetNode<Entity>("Entity").CanBleed == true)
                    {
                        GenerateImpactParticles(BloodSplatter, result["position"].AsVector3(), result["normal"].AsVector3());
                    }
                }
            }
            GetNode<MeshInstance3D>("Geometry").Visible = false;
            GetNode<CpuParticles3D>("ParticleHolder/Particles").Emitting = false;
            GetNode<Node3D>("MiscFX").Visible = false;
            GenerateImpactParticles(ImpactParticles, result["position"].AsVector3(), result["normal"].AsVector3());
            SceneTreeTimer timer = GetTree().CreateTimer(1.0);
            await ToSignal(timer, "timeout");
            QueueFree();
        }
    }

    private async void QueueForDespawn()
    {
        SceneTreeTimer timer = GetTree().CreateTimer(TimeBeforeDespawn);
        await ToSignal(timer, "timeout");
        QueueFree();
    }

    private async void GenerateImpactParticles(PackedScene scene, Vector3 Pos, Vector3 Norm)
    {
        Node3D Impact = (Node3D)(scene.Instantiate());
        GetParent().AddChild(Impact);
        Impact.Position = Pos;
        Impact.LookAt(Impact.Position - Norm);
        Impact.GetNode<CpuParticles3D>("Particles").Emitting = true;
        SceneTreeTimer timer = GetTree().CreateTimer(1.0);
        await ToSignal(timer, "timeout");
        Impact.QueueFree();
    }

}
