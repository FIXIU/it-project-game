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

        if (boss.CanSeePlayer())
        {
            float distanceToPlayer = boss.GetDistanceToPlayer();
            GD.Print(distanceToPlayer + " : " + boss.AttackRange);
            if (distanceToPlayer <= boss.AttackRange)
            {
                fsm?.ListAllStates();
                if (GD.Randf() > 0.6f)
                {
                    GD.Print("Spin Attack");
                    fsm?.TransitionTo("SpinAttack");
                }
                else
                {
                    GD.Print("Normal Attack");
                    fsm?.TransitionTo("Attack");
                }
            }
            else if (distanceToPlayer > boss.AttackRange * 2.0f && GD.Randf() > 0.7f)
            {
                GD.Print("Leap Attack");
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

