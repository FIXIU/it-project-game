using Godot;
using System;
using System.Collections.Generic;

public partial class Attack : State
{
	List<string> _attackAnimations = new List<string> { "KatanaBottom", "KatanaTop" };
	private int _currentAttackIndex = 0;
	private double attackTimer = 0.0;
	private bool hasAttacked = false;
	public override void Enter()
	{
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");

		if (_currentAttackIndex >= _attackAnimations.Count - 1)
		{
			_currentAttackIndex = 0;
		}
		else
		{
			_currentAttackIndex++;
		}

		hasAttacked = true;

		animationPlayer.Play(_attackAnimations[_currentAttackIndex]);
		GD.Print($"Attack State: Playing 'Attack'. Current animation: {animationPlayer.CurrentAnimation}");

		GetNode<Player>("../..").Velocity = new Vector2(0, GetNode<Player>("../..").Velocity.Y);
	}

	public override void _Process(double delta)
	{
		GetNode<Player>("../..").Velocity = new Vector2(0, GetNode<Player>("../..").Velocity.Y);

		if (hasAttacked)
		{
			attackTimer += delta;
		}
		
		if (attackTimer >= 0.6 && hasAttacked)
			{
				GD.Print("Attack finished, transitioning to Idle state.");
				fsm?.TransitionTo("Idle");
				attackTimer = 0.0;
				hasAttacked = false;
			}
	}

}
