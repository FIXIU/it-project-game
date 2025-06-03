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
        if (boss.Player != null)
        {
            walkDirection = boss.GetDirectionToPlayer();
        }
        else
        {
            walkDirection = new Vector2(GD.Randf() > 0.5f ? 1 : -1, 0);
        }
        GD.Print($"Boss Walk State: Moving towards target");
    }
    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        walkTime += (float)delta;
        boss.Velocity = new Vector2(walkDirection.X * boss.Speed, boss.Velocity.Y);
        if (boss.CanSeePlayer())
        {
            float distanceToPlayer = boss.GetDistanceToPlayer();
            if (distanceToPlayer <= boss.AttackRange)
            {
                boss.Velocity = new Vector2(0, boss.Velocity.Y);
                
                if (GD.Randf() > 0.5f)
                {
                    fsm?.TransitionTo("SpinAttack");
                }
                else
                {
                    fsm?.TransitionTo("Attack");
                }
            }
            else if (GD.Randf() > 0.7f && distanceToPlayer > boss.AttackRange * 1.5f)
            {
                GD.Print("Tried to Dash");
                fsm?.TransitionTo("Dash");
            }
            
        }
        if (walkTime >= maxWalkTime || (!boss.CanSeePlayer() && walkTime > 1.0f))
        {
            fsm?.TransitionTo("Idle");
        }
    }

    // public override void Exit()
    // {
    //     boss.Velocity = new Vector2(0, boss.Velocity.Y);
    // }
}

