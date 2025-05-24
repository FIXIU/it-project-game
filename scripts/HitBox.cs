using Godot;
using System;

public partial class HitBox : Area2D
{
    [Signal]
    public delegate void HitEventHandler(Node2D body, float damage);

    [Export]
    public float Damage { get; set; } = 10f;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node2D body)
    {
        EmitSignal(SignalName.Hit, body, Damage);
        GetNode<CollisionShape2D>("CollisionShape2D").Disabled = true; 
        // Or queue_free() if the hitbox is a one-time effect.
        // QueueFree();
    }
}