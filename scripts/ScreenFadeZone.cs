using Godot;
using System;

public partial class ScreenFadeZone : Area2D
{
    private Node2D _player;
    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            _player = body;
            _player.Call("TransitionScreenFade", true);
        }
    }
    
    private void OnBodyExited(Node2D body)
    {
        if (body == _player)
        {
            _player.Call("TransitionScreenFade", false);
            _player = null;
        }
    }
}
