using Godot;
using System;

public partial class Player : PlayerMovement, ITakeDamage
{
    public void TakeDamage(float damage, Vector2? attackFromVector)
    {
        GD.Print($"Player took {damage} damage.");
        if (attackFromVector.HasValue)
        {
            GD.Print($"Attack came from {attackFromVector.Value}");
        }
    }
}
