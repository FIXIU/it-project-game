using Godot;
using System;

public partial class Hurtbox : Area2D
{
    public override void _Ready()
    {
        CollisionLayer = 0;
        CollisionMask = 8;
        
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if(area is not Hitbox hitbox)
        {
            return;
        }
        
        // ignore self-inflicted hits
        if (hitbox.Attacker == Owner)
            return;
        if(Owner is ITakeDamage ownerTakeDamage)
        {
            ownerTakeDamage.TakeDamage(hitbox.Damage);
        }
    }
}