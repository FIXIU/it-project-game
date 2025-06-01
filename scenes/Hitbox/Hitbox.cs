using Godot;
using System;

public partial class Hitbox : Area2D
{
    [Export]
    public float Damage = 1.0f;                 // The damage the hitbox will deliver
    public Vector2? AttackFromVector = null;    // The vector the attack came from
    public override void _Ready()
    {
        var layersAndMasks = (LayersAndMasks) GetNode("/root/LayersAndMasks");  // Get the LayersAndMasks singleton
        CollisionLayer = layersAndMasks.GetCollisionLayerByName("Hitbox");      // Get the collision layer for the hitbox by name
        CollisionMask = 0;                                                      // Set collision mask to 0 = no mask
    }
    public void SetAttackFromVector(Vector2 attackVector)
    {
        this.AttackFromVector = attackVector;
    }
}
