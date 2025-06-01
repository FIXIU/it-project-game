using Godot;
using System;

public partial class Run : State
{
	public override void Enter()
	{
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
		animationPlayer.Play("Run");
		GD.Print($"Run State: Playing 'Run'. Current animation: {animationPlayer.CurrentAnimation}");
	}
}