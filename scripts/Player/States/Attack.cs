using Godot;
using System;
using System.Collections.Generic;

public partial class Attack : State
{
    List<string> _attackAnimations = new List<string> { "KatanaBottom", "KatanaTop" };
    private int _currentAttackIndex = 0;
    public override void Enter()
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../PlayerAnimator/AnimationPlayer");
        
        // Cycle through attack animations
        if (_currentAttackIndex >= _attackAnimations.Count)
        {
            _currentAttackIndex = 0; // Reset to the first animation
        }
        
        animationPlayer.Play(_attackAnimations[_currentAttackIndex]);
        GD.Print($"Attack State: Playing 'Attack'. Current animation: {animationPlayer.CurrentAnimation}");
    }
}