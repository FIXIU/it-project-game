using Godot;
using System;

public partial class Idle : State
{
	public override void Enter()
	{
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
		animationPlayer.Play("Idle");
	}
}
