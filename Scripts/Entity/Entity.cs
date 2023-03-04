using Godot;
using System;

public partial class Entity : Node
{

    [Export]
    public int Health;
    [Export]
    public int MaxHealth;
    [Export]
    public int Armor;

	[Export]
    public bool CanBleed;

    private void CheckDeath()
    {
        if (Health <= 0)
        {
            // We died!
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        Health=Math.Clamp(Health, 0, MaxHealth); // Make sure Health is always in the correct range
        CheckDeath();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
