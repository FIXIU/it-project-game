using Godot;
using System;

public partial class Fireplace : Node2D
{
    private Area2D _healArea;
    private Node2D _player;
    private float timer = 0.0f;

    public override void _Ready()
    {
        _healArea = GetNode<Area2D>("HealArea");
        if (_healArea != null)
        {
            _healArea.BodyEntered += OnBodyEntered;
        }
        else
        {
            GD.PrintErr("HealZone requires an Area2D child node named 'Area2D'.");
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            GD.Print("Player entered Fireplace heal area: " + body.Name);
            _player = body;
        }
    }
    
    private void OnBodyExited(Node2D body)
    {
        if (body == _player)
        {
            GD.Print("Player exited Fireplace heal area: " + body.Name);
            _player = null;
        }
    }
    
    public override void _Process(double delta)
    {
        timer += (float)delta;
        if (_player != null && timer >= 1.0f)
        {
            GD.Print("Healing player in Fireplace heal area: " + _player.Name);
            _player.Call("TakeDamage", -10);
            timer = 0.0f;
        }
    }
}
