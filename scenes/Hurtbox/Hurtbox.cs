using Godot;
using System;

public partial class Hurtbox : Area2D
{
    public override void _Ready()
    {
        CollisionLayer = 0;
        
        // Add safety check to prevent null reference exception
        var layersAndMasks = GetNode<LayersAndMasks>("/root/LayersAndMasks");
        if (layersAndMasks != null)
        {
            CollisionMask = layersAndMasks.GetCollisionLayerByName("Hitbox");
        }
        else
        {
            GD.PrintErr("LayersAndMasks singleton not found! Make sure it's added as an Autoload in Project Settings.");
            // Fallback to a default mask if needed
            // CollisionMask = 2; // Assuming 2 is the hitbox layer
        }
        
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if(area is not Hitbox hitbox)
        {
            return;
        }
        
        if(Owner is ITakeDamage ownerTakeDamage)
        {
            ownerTakeDamage.TakeDamage(hitbox.Damage, hitbox.AttackFromVector);
        }
    }
}