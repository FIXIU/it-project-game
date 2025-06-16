using Godot;

public partial class BossAttack : State
{
    private Boss boss;
    private bool attackHitRegistered = false;
    private float attackDuration = 4f;
    private float attackTimer = 0.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Attack");
        attackHitRegistered = false;
        attackTimer = 0.0f;
        boss.IsAttacking = true;
        boss.Velocity = new Vector2(0, boss.Velocity.Y);
        GD.Print($"Boss Attack State: Playing 'Attack'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        attackTimer += (float)delta;
        if (!attackHitRegistered && attackTimer >= attackDuration * 0.5f)
        {
            attackHitRegistered = true;
        }
        if (attackTimer >= attackDuration)
        {
            boss.StartAttackCooldown();
            if (boss.CanSeePlayer() && boss.GetDistanceToPlayer() > boss.AttackRange)
            {
                fsm?.TransitionTo("Walk");
            }
            else
            {
                fsm?.TransitionTo("Idle");
            }
        }
    }
}

