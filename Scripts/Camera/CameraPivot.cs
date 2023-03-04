using Godot;
using System;

public partial class CameraPivot : Node3D
{
    [Export]
    public CharacterBody3D Player { get; set; }

    private Camera3D camera;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        camera = GetNode<Camera3D>("Camera");
        RotateY(0.61f); // 35 degrees in radians
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _PhysicsProcess(double delta)
    {
        Position = Position.Lerp(Player.Position, 0.1f);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (eventMouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                camera.Size -= 1;
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                camera.Size += 1;
            }
            camera.Size = Math.Clamp(camera.Size, 10, 40);
        }
        else if (@event is InputEventKey eventKey && eventKey.Pressed)
        {
            if (eventKey.Keycode == Key.E)
            {
                // Rotate camera
                RotateY(MathF.PI / 2); // need to add a lerping mechanism to this at some point
            }
        }
    }
}
