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
        animationPlayer.Play("Jump");

        hasJumped = false;
        jumpTimer = 0.0f;

        if (boss.IsOnFloor())
        {
            animationPlayer.Play("Jump");
            hasJumped = true;

            if (boss.CanSeePlayer())
            {
                Vector2 direction = boss.GetDirectionToPlayer();
                
            }
        }

        GD.Print($"Boss Jump State: Playing 'spr_Jump_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        jumpTimer += (float)delta;

        if (hasJumped && boss.IsOnFloor() && jumpTimer > 0.5f)
        {
            fsm?.TransitionTo("Idle");
            return;
        }

        if (jumpTimer >= maxJumpTime)
        {
            fsm?.TransitionTo("Idle");
        }
    }
}
