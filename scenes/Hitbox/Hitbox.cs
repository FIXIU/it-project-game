using Godot;
using System;

public partial class Hitbox : Area2D
{
	[Export]
	public int Damage = 10;
	public Vector2? AttackFromVector = null;

	// reference to the entity that spawned this hitbox
	public Node2D Attacker { get; private set; }
	
	public override void _Ready()
	{
		CollisionLayer = 8;
		CollisionMask = 0;
		// set attacker to parent node (e.g. boss or player)
		Attacker = GetParent<Node2D>();
	}
	public void SetAttackFromVector(Vector2 attackVector)
	{
		this.AttackFromVector = attackVector;
	}
}
