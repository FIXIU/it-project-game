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
        GD.Print($"Boss Idle State: Playing 'spr_Idle_strip'. Current animation: {animationPlayer.CurrentAnimation}");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        idleTime += (float)delta;

        // Check if player is detected
        if (boss.CanSeePlayer())
        {
            float distanceToPlayer = boss.GetDistanceToPlayer();

            if (distanceToPlayer <= boss.AttackRange && boss.PlayerInRange && boss.CanAttack)
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
            var randomActions = new string[] { "Taunt", "Walk" };
            string randomAction = randomActions[GD.RandRange(0, randomActions.Length - 1)];
            fsm?.TransitionTo(randomAction);
        }
    }
}