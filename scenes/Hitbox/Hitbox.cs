using Godot;
using System;

public partial class Hitbox : Area2D
{
	[Export]
	public int Damage = 10;
	public Vector2? AttackFromVector = null;
	public override void _Ready()
	{
		CollisionLayer = 8;
		CollisionMask = 0;
	}
	public void SetAttackFromVector(Vector2 attackVector)
	{
		this.AttackFromVector = attackVector;
	}
}
