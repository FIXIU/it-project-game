using Godot;
using System;

public partial class Jump : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
        animationPlayer.Play("Jump");
    }
}
