using Enemies.Enemy1;
using Godot;
using System;

public partial class BossArea : Area2D
{
    private Node2D _player;
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            GD.Print("Player entered Boss area: " + body.Name);
            _player = body;
            _player.Call("ShowBossHealthBar", GetNode<Boss>("../"));
        }
    }
    
    private void OnBodyExited(Node2D body)
    {
        if (body == _player)
        {
            GD.Print("Player exited Boss area: " + body.Name);
            _player.Call("HideBossHealthBar");
            _player = null;
        }
    }
}
