using System;
using System.Threading;
using Godot;

public partial class BossLeap : State
{
    private Boss boss;
    private bool hasLeaped = false;
    private float leapTimer = 0.0f;
    private float leapDuration = 1.2f; // Duration of the leap in seconds
    private float maxLeapTime = 3.0f;  // Total time in this state
    private Vector2 leapStartPosition;
    private Vector2 leapEndPosition;
    private Vector2 leapTarget;
    private bool targetSet = false;
    private Godot.Timer leapTimerNode;
    private float leapDistance = 0;
    private float distanceToPlayer = 0;
    
     public override void Enter()
    {
        leapTimerNode = GetNode<Godot.Timer>("../../LeapTimer");
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Leap");
        hasLeaped = false;
        leapTimer = 0.0f;
        targetSet = false;
        leapStartPosition = boss.GlobalPosition;
        if (boss.CanSeePlayer())
        {
            leapTarget = boss.Player.GlobalPosition;
        }
        leapDistance = leapTarget.X - leapStartPosition.X;
    }
    
    public override void Update(double delta)
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        if (boss.IsDead) return;
        leapTimer += (float)delta;
        
        if (!hasLeaped)
        {
            animationPlayer.Play("Leap");
            hasLeaped = true;
            leapTimerNode.Start();
        }
        if (leapTimer >= 1.2f && leapTimer <= 1.67)
        {
            leapTarget = boss.Player.GlobalPosition;
            distanceToPlayer = Math.Abs(leapTarget.X - boss.GlobalPosition.X);
            
            Vector2 dashDirection = boss.FacingRight ? Vector2.Right : Vector2.Left;
            
            GD.Print(distanceToPlayer);
            
            float dashSpeed = 3.0f * distanceToPlayer;
            boss.Velocity = new Vector2(dashDirection.X * dashSpeed, boss.Velocity.Y);
        }
        else
        {
            boss.Velocity = new Vector2(0, boss.Velocity.Y);
        }
        if (hasLeaped && leapTimer > 3.0f)
        {
            fsm?.TransitionTo("Idle");
            return;
        }
    }
}