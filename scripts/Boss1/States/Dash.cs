using Godot;
using System;

public partial class Boss1Dash : State
{
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Dash");
    }
}