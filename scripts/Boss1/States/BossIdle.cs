using Godot;

public partial class BossIdle : State
{
    private Boss boss;
    private float idleTime = 0.0f;
    private float maxIdleTime = 2.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Idle");

        idleTime = 0.0f;
        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        GD.Print($"Boss Idle State: Playing 'Idle'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        idleTime += (float)delta;

        // Check if player is detected
        if (boss.CanSeePlayer())
        {
            GD.Print("Player detected, transitioning to attack or walk state.");
            float distanceToPlayer = boss.GetDistanceToPlayer();

            // Use the new raycast-based attack detection
            if (distanceToPlayer <= boss.AttackRange && boss.CanAttackPlayer())
            {
                // Choose random attack
                var attacks = new string[] { "Attack", "SpinAttack" };
                string randomAttack = attacks[GD.RandRange(0, attacks.Length - 1)];
                fsm?.TransitionTo(randomAttack);
            }
            else if (distanceToPlayer > boss.AttackRange)
            {
                // Move towards player
                fsm?.TransitionTo("Walk");
            }
        }

        // Random behavior when idle for too long
        if (idleTime >= maxIdleTime && !boss.PlayerDetected)
        {
            // Random movement or taunt
            if (GD.Randf() > 0.5f)
            {
                fsm?.TransitionTo("Walk");
            }
        }
    }

    public override void Exit()
    {
        // Clean up if needed
    }
}