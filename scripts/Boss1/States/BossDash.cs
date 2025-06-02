using Godot;

public partial class BossDash : State
{
    private Boss boss;
    private float dashDuration = 0.8f;
    private float dashTimer = 0.0f;
    private Vector2 dashDirection;
    private bool dashHitRegistered = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("spr_Dash_strip");

        dashTimer = 0.0f;
        dashHitRegistered = false;
        boss.IsAttacking = true;

        // Set dash direction towards player
        if (boss.CanSeePlayer())
        {
            dashDirection = boss.GetDirectionToPlayer();
        }
        else
        {
            dashDirection = boss.FacingRight ? Vector2.Right : Vector2.Left;
        }

        GD.Print($"Boss Dash State: Playing 'spr_Dash_strip'. Direction: {dashDirection}");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        dashTimer += (float)delta;

        // Dash movement
        if (dashTimer < dashDuration * 0.8f) // Dash for 80% of duration
        {
            boss.Velocity = new Vector2(dashDirection.X * boss.DashSpeed, boss.Velocity.Y);

            // Check for collision with player
            if (!dashHitRegistered && boss.PlayerInRange)
            {
                PerformDashAttack();
                dashHitRegistered = true;
            }
        }
        else
        {
            // Slow down near end of dash
            boss.Velocity = new Vector2(boss.Velocity.X * 0.5f, boss.Velocity.Y);
        }

        // End dash
        if (dashTimer >= dashDuration)
        {
            boss.Velocity = new Vector2(0, boss.Velocity.Y);
            boss.StartAttackCooldown();
            fsm?.TransitionTo("Idle");
        }
    }

    private void PerformDashAttack()
    {
        // Deal heavy damage on dash hit
        if (boss.Player != null && boss.Player.HasMethod("TakeDamage"))
        {
            boss.Player.Call("TakeDamage", 35.0f);
            GD.Print("Boss dash attack dealt 35 damage to player!");
        }
    }

    public override void Exit()
    {
        boss.IsAttacking = false;
        boss.Velocity = new Vector2(0, boss.Velocity.Y);
    }
}
