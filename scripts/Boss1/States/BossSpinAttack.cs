using Godot;

public partial class BossSpinAttack : State
{
    private Boss boss;
    private float spinDuration = 1.5f;
    private float spinTimer = 0.0f;
    private bool isSpinning = false;
    private float spinSpeed = 150.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("SpinAttack");

        spinTimer = 0.0f;
        isSpinning = true;
        boss.IsAttacking = true;

        GD.Print($"Boss SpinAttack State: Playing 'spr_SpinAttack_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        spinTimer += (float)delta;

        // Move in a spinning pattern
        if (isSpinning && boss.CanSeePlayer())
        {
            Vector2 directionToPlayer = boss.GetDirectionToPlayer();
            boss.Velocity = new Vector2(directionToPlayer.X * spinSpeed, boss.Velocity.Y);

            // Deal continuous damage during spin
            if (boss.PlayerInRange)
            {
                PerformSpinDamage();
            }
        }

        // End spin attack
        if (spinTimer >= spinDuration)
        {
            boss.Velocity = new Vector2(0, boss.Velocity.Y);
            boss.StartAttackCooldown();

            // Brief recovery period
            fsm?.TransitionTo("Idle");
        }
    }

    private void PerformSpinDamage()
    {
        // Deal damage to player if in range (reduced damage due to continuous nature)
        if (boss.Player != null && boss.Player.HasMethod("TakeDamage"))
        {
            boss.Player.Call("TakeDamage", 15.0f);
            GD.Print("Boss spin attack dealt 15 damage to player!");
        }
    }

    public override void Exit()
    {
        boss.IsAttacking = false;
        isSpinning = false;
        boss.Velocity = new Vector2(0, boss.Velocity.Y);
    }
}