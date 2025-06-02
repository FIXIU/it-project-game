using Godot;
using System;

public partial class Hitbox : Area2D
{
    [Export]
    public float Damage = 1.0f;
    public Vector2? AttackFromVector = null;
    public override void _Ready()
    {
        var layersAndMasks = (LayersAndMasks) GetNode("/root/LayersAndMasks");
        CollisionLayer = layersAndMasks.GetCollisionLayerByName("Hitbox");
        CollisionMask = 0;
    }
    public void SetAttackFromVector(Vector2 attackVector)
    {
        this.AttackFromVector = attackVector;
    }
}
