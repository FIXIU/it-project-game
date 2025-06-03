using System.Threading;
using Godot;

public partial class BossLeap : State
{
    private Boss boss;
    private bool hasLeaped = false;
    private float leapTimer = 0.0f;
    private float maxLeapTime = 3.0f;
    private Vector2 leapTarget;
    private bool targetSet = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Leap");
        hasLeaped = false;
        leapTimer = 0.0f;
        targetSet = false;
        // Always set a target, regardless of seeing the player
        targetSet = true;
        if (boss.CanSeePlayer())
        {
            leapTarget = boss.Player.GlobalPosition;
        }
        GD.Print($"Boss Leap State: Playing 'spr_Leap_strip'");
    }

    public override void Update(double delta)
    {
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        if (boss.IsDead) return;
        leapTimer += (float)delta;
        if (!hasLeaped && leapTimer >= 0.3f)
        {
            animationPlayer.Play("Leap");
            hasLeaped = true;
        
            // Execute teleport regardless of other conditions
            GD.Print($"Boss leaping towards target");
            GD.Print("Before: " + boss.GlobalPosition);
            var dir = boss.FacingRight ? Vector2.Right : Vector2.Left;
            boss.GlobalPosition += dir * 108;
            GD.Print("After: " + boss.GlobalPosition);
        }
    
        if (hasLeaped && leapTimer > 3.0f)
        {
            fsm?.TransitionTo("Idle");
            return;
        }
    }
}
