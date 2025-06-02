using Godot;

public partial class BossWalk : State
{
    private Boss boss;
    private float walkTime = 0.0f;
    private float maxWalkTime = 3.0f;
    private Vector2 walkDirection;
    private bool hasTarget = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Walk");

        walkTime = 0.0f;

        // Determine walk direction
        if (boss.CanSeePlayer())
        {
            walkDirection = boss.GetDirectionToPlayer();
            hasTarget = true;
        }
        else
        {
            // Random walk direction
            walkDirection = new Vector2(GD.Randf() > 0.5f ? 1.0f : -1.0f, 0);
            hasTarget = false;
        }

        GD.Print($"Boss Walk State: Playing 'spr_Walk_strip'. Direction: {walkDirection}");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        walkTime += (float)delta;
        
        
        // Move towards target
        boss.Velocity = new Vector2(walkDirection.X * boss.Speed, boss.Velocity.Y);

        // Check if player is in attack range
        if (boss.CanSeePlayer())
        {
            float distanceToPlayer = boss.GetDistanceToPlayer();

            if (distanceToPlayer <= boss.AttackRange && boss.PlayerInRange && boss.CanAttack)
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
                return;
            }

            // Update direction to player
            walkDirection = boss.GetDirectionToPlayer();

            // Consider jumping if there's an obstacle
            if (boss.IsOnFloor() && GD.Randf() > 0.9f)
            {
                fsm?.TransitionTo("Jump");
                return;
            }
        }

        // Return to idle after walking for too long
        if (walkTime >= maxWalkTime || (!hasTarget && walkTime >= 1.5f))
        {
            boss.Velocity = new Vector2(0, boss.Velocity.Y);
            fsm?.TransitionTo("Idle");
        }
    }

    public override void Exit()
    {
        boss.Velocity = new Vector2(0, boss.Velocity.Y);
    }
}
