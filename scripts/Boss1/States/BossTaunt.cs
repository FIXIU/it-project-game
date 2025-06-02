using Godot;

public partial class BossTaunt : State
{
    private Boss boss;
    private float tauntDuration = 1.5f;
    private float tauntTimer = 0.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("spr_Taunt_strip");

        tauntTimer = 0.0f;

        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        GD.Print($"Boss Taunt State: Playing 'spr_Taunt_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        tauntTimer += (float)delta;

        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        if (tauntTimer >= tauntDuration)
        {
            if (boss.CanSeePlayer())
            {
                float distanceToPlayer = boss.GetDistanceToPlayer();

                if (distanceToPlayer <= boss.AttackRange && boss.PlayerInRange && boss.CanAttack)
                {
                    fsm?.TransitionTo("SpinAttack");
                }
                else if (distanceToPlayer > boss.AttackRange)
                {
                    if (GD.Randf() > 0.6f)
                    {
                        fsm?.TransitionTo("Leap");
                    }
                    else
                    {
                        fsm?.TransitionTo("Walk");
                    }
                }
                else
                {
                    fsm?.TransitionTo("Idle");
                }
            }
            else
            {
                fsm?.TransitionTo("Idle");
            }
        }
    }
}
