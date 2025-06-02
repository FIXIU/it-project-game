using Godot;
using System;

public partial class Fall : State
{
	public override void Enter()
	{
		// Adjust the path if your scene tree structure is different.
		var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
		animationPlayer.Play("Fall"); // Assuming you have a "Fall" animation
	}
}

