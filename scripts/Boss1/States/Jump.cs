using Godot;
using System;

public partial class Boss1Jump : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Jump");
    }
}