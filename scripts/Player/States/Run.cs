using Godot;
using System;

public partial class Run : State
{
	public override void Enter()
	{
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
		animationPlayer.Play("Run");
	}
}