using Godot;
using System;

public partial class DeathZone : Node2D
{
    private Area2D _area2D;

    public override void _Ready()
    {
        _area2D = GetNode<Area2D>("Area2D");
        if (_area2D != null)
        {
            _area2D.BodyEntered += OnBodyEntered;
        }
        else
        {
            GD.PrintErr("DeathZone requires an Area2D child node named 'Area2D'.");
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        GD.Print("Body entered DeathZone: " + body.Name);
        if (body.IsInGroup("player"))
        {
            GD.Print($"Player entered DeathZone: {body.Name}");
            if (body.HasMethod("TakeDamage"))
            {
                body.Call("TakeDamage", 100);
                GD.Print("Player entered DeathZone and took lethal damage.");
            }
            else
            {
                GD.PrintErr("Player node does not have a 'TakeDamage' or 'Die' method.");
            }
        }
    }
}

