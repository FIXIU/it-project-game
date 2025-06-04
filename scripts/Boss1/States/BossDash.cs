using Godot;

public partial class BossDash : State
{
    private Boss boss;
    private double dashTimer = 0.0f;
    private float dashDuration = 0.7f;
    private bool hasDashed = false;
    private Timer dashTimerNode;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Dash");
        dashTimerNode = animationPlayer.GetNode<Timer>("../../DashTimer");
        hasDashed = false;
        dashTimerNode.Start();
        GD.Print("Boss Dash State: Playing 'Dash'");
    }
    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        dashTimer += (float)delta;
        if (!hasDashed && dashTimer >= 0.25f)
        {
            PerformDash();
            dashTimerNode.Start();
            hasDashed = true;
        }
        if (dashTimer >= dashDuration)
        {
            fsm?.TransitionTo("Idle");
        }
    }
    private void PerformDash()
    {
        
        // Get dash direction based on boss facing direction
        Vector2 dashDirection = boss.FacingRight ? Vector2.Right : Vector2.Left;
    
        // Set a high velocity for the dash
        float dashSpeed = 500.0f;
        boss.Velocity = new Vector2(dashDirection.X * dashSpeed, boss.Velocity.Y);
    }
}
