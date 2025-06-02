using Godot;
using System;

public partial class WallSlide : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
        animationPlayer.Play("WallSlide");
    }
}