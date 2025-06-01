using Godot;
using System;

public partial class Hurtbox : Area2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var layersAndMasks = (LayersAndMasks) GetNode("/root/LayersAndMasks");  // Get the layers and masks singleton
        CollisionLayer = 0;                                                     // Set collision layer to 0 = no layer
        CollisionMask = layersAndMasks.GetCollisionLayerByName("Hitbox");       // Get the hitbox collision layer by name
        Connect("area_entered", new Callable(this, nameof(OnAreaEntered)));   // Create a signal for area_entered
    }

    private void OnAreaEntered(Hitbox hitbox)
    {
        // If the hitbox is null
        if(hitbox == null)
        {
            return; // Return out of the method
        }
        ITakeDamage ownerTakeDamage = (ITakeDamage)Owner;   // Typecast the owner to ITakeDamage
        // Call the TakeDamage method and pass in the damage and AttackFromVector values from the hitbox
        ownerTakeDamage.TakeDamage(hitbox.Damage, hitbox.AttackFromVector);
    }
}