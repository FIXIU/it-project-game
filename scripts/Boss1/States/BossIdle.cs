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
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        idleTime += (float)delta;

        if (boss.CanSeePlayer())
        {
            GD.Print("Can see player");
            float distanceToPlayer = boss.GetDistanceToPlayer();
            if (distanceToPlayer <= boss.AttackRange)
            {
                if (GD.Randf() > 0.6f)
                {
                    fsm?.TransitionTo("SpinAttack");
                }
                else
                {
                    fsm?.TransitionTo("Attack");
                }
            }
            else if (distanceToPlayer > boss.AttackRange * 2.0f && GD.Randf() > 0.7f)
            {
                fsm?.TransitionTo("Leap");
            }
            else
            {
                fsm?.TransitionTo("Walk");
            }
        }
        else if (idleTime >= maxIdleTime)
        {
            if (GD.Randf() > 0.8f)
            {
                fsm?.TransitionTo("Taunt");
            }
            else
            {
                fsm?.TransitionTo("Walk");
            }
        }
    }
}

