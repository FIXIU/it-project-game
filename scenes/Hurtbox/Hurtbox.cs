using Godot;
using System;

public partial class Hurtbox : Area2D
{
    public override void _Ready()
    {
        var layersAndMasks = (LayersAndMasks) GetNode("/root/LayersAndMasks");
        CollisionLayer = 0;
        CollisionMask = layersAndMasks.GetCollisionLayerByName("Hitbox");
        Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));
    }

    private void OnAreaEntered(Hitbox hitbox)
    {
        if(hitbox == null)
        {
            return;
        }
        ITakeDamage ownerTakeDamage = (ITakeDamage)Owner;
        ownerTakeDamage.TakeDamage(hitbox.Damage, hitbox.AttackFromVector);
    }
}

