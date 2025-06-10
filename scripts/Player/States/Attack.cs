using Godot;
using System;
using System.Collections.Generic;

public partial class Attack : State
{
	List<string> _attackAnimations = new List<string> { "KatanaBottom", "KatanaTop" };
	private int _currentAttackIndex = 0;
	public override void Enter()
	{
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
		
		if (_currentAttackIndex >= _attackAnimations.Count)
		{
			_currentAttackIndex = 0;
		}
		
		animationPlayer.Play(_attackAnimations[_currentAttackIndex]);
		GD.Print($"Attack State: Playing 'Attack'. Current animation: {animationPlayer.CurrentAnimation}");
	}
}
