using Godot;
using System;

public partial class Boss1Idle : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Idle");
    }
}