using Godot;

public partial class BossWalk : State
{
    private Boss boss;
    private Vector2 walkDirection;
    private float walkTime = 0.0f;
    private float maxWalkTime = 3.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Walk");

        walkTime = 0.0f;

        // Determine walk direction towards player
        if (boss.Player != null)
        {
            walkDirection = boss.GetDirectionToPlayer();
        }
        else
        {
            // Random direction if no player
            walkDirection = new Vector2(GD.Randf() > 0.5f ? 1 : -1, 0);
        }

        GD.Print($"Boss Walk State: Moving towards target");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        walkTime += (float)delta;
        
        // Move towards target
        boss.Velocity = new Vector2(walkDirection.X * boss.Speed, boss.Velocity.Y);

        // Check if player is in attack range using new raycast detection
        if (boss.CanSeePlayer())
        {
            float distanceToPlayer = boss.GetDistanceToPlayer();

            if (distanceToPlayer <= boss.AttackRange && boss.CanAttackPlayer())
            {
                // Stop and attack
                boss.Velocity = new Vector2(0, boss.Velocity.Y);

                // Choose attack based on distance and random chance
                if (GD.Randf() > 0.7f && distanceToPlayer > boss.AttackRange * 0.5f)
                {
                    fsm?.TransitionTo("Dash");
                }
                else if (GD.Randf() > 0.5f)
                {
                    fsm?.TransitionTo("SpinAttack");
                }
                else
                {
                    fsm?.TransitionTo("Attack");
                }
            }
            else if (distanceToPlayer <= boss.AttackRange * 0.8f)
            {
                // Close enough, prepare to attack
                fsm?.TransitionTo("Idle");
            }
        }

        // Stop walking if been walking too long or lost player
        if (walkTime >= maxWalkTime || (!boss.CanSeePlayer() && walkTime > 1.0f))
        {
            fsm?.TransitionTo("Idle");
        }
    }

    public override void Exit()
    {
        boss.Velocity = new Vector2(0, boss.Velocity.Y);
    }
}