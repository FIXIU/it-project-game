using Godot;
using System;

public partial class WallSlide : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
        animationPlayer.Play("WallSlide");
        GD.Print($"Fall State: Playing 'WallSlide'. Current animation: {animationPlayer.CurrentAnimation}");
    }
}