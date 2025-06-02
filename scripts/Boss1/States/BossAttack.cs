using Godot;

public partial class BossAttack : State
{
    private Boss boss;
    private bool attackHitRegistered = false;
    private float attackDuration = 1.0f;
    private float attackTimer = 0.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Attack");

        attackHitRegistered = false;
        attackTimer = 0.0f;
        boss.IsAttacking = true;

        // Stop movement during attack
        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        GD.Print($"Boss Attack State: Playing 'spr_Attack_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        attackTimer += (float)delta;

        // Check for hit during attack animation (middle of animation)
        if (!attackHitRegistered && attackTimer >= attackDuration * 0.5f)
        {
            PerformAttack();
            attackHitRegistered = true;
        }

        // End attack
        if (attackTimer >= attackDuration)
        {
            boss.StartAttackCooldown();

            // Decide next state
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

    private void PerformAttack()
    {
        // Deal damage to player if in range
        if (boss.PlayerInRange && boss.Player != null)
        {
            // Try to call TakeDamage on player
            if (boss.Player.HasMethod("TakeDamage"))
            {
                boss.Player.Call("TakeDamage", 25.0f);
                GD.Print("Boss dealt 25 damage to player!");
            }
        }
    }

    public override void Exit()
    {
        boss.IsAttacking = false;
    }
}