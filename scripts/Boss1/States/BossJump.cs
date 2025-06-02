using Godot;

public partial class BossJump : State
{
    private Boss boss;
    private bool hasJumped = false;
    private float jumpTimer = 0.0f;
    private float maxJumpTime = 2.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("spr_Jump_strip");

        hasJumped = false;
        jumpTimer = 0.0f;

        // Perform jump
        if (boss.IsOnFloor())
        {
            boss.Velocity = new Vector2(boss.Velocity.X, boss.JumpVelocity);
            hasJumped = true;

            // Add horizontal movement towards player if detected
            if (boss.CanSeePlayer())
            {
                Vector2 direction = boss.GetDirectionToPlayer();
                boss.Velocity = new Vector2(direction.X * boss.Speed * 0.7f, boss.Velocity.Y);
            }
        }

        GD.Print($"Boss Jump State: Playing 'spr_Jump_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        jumpTimer += (float)delta;

        // Continue horizontal movement during jump
        if (hasJumped && boss.CanSeePlayer())
        {
            Vector2 direction = boss.GetDirectionToPlayer();
            boss.Velocity = new Vector2(direction.X * boss.Speed * 0.5f, boss.Velocity.Y);
        }

        // Check if landed
        if (hasJumped && boss.IsOnFloor() && jumpTimer > 0.5f)
        {
            // Landing - decide next action
            if (boss.CanSeePlayer())
            {
                float distanceToPlayer = boss.GetDistanceToPlayer();

                if (distanceToPlayer <= boss.AttackRange && boss.CanAttack)
                {
                    fsm?.TransitionTo("Attack");
                }
                else
                {
                    fsm?.TransitionTo("Walk");
                }
            }
            else
            {
                fsm?.TransitionTo("Idle");
            }
            return;
        }

        // Timeout if jump takes too long
        if (jumpTimer >= maxJumpTime)
        {
            fsm?.TransitionTo("Idle");
        }
    }
}
