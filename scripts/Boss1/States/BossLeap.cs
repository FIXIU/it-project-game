using Godot;

public partial class BossLeap : State
{
    private Boss boss;
    private bool hasLeaped = false;
    private float leapTimer = 0.0f;
    private float maxLeapTime = 3.0f;
    private Vector2 leapTarget;
    private bool targetSet = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("spr_Leap_strip");
        hasLeaped = false;
        leapTimer = 0.0f;
        targetSet = false;
        if (boss.CanSeePlayer())
        {
            leapTarget = boss.Player.GlobalPosition;
            targetSet = true;
        }
        GD.Print($"Boss Leap State: Playing 'spr_Leap_strip'");
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        leapTimer += (float)delta;
        if (!hasLeaped && leapTimer >= 0.3f && boss.IsOnFloor())
        {
            if (targetSet)
            {
                Vector2 direction = (leapTarget - boss.GlobalPosition).Normalized();
                float distance = boss.GlobalPosition.DistanceTo(leapTarget);
                float leapForceX = direction.X * boss.LeapSpeed;
                float leapForceY = boss.JumpVelocity * 1.2f;
                boss.Velocity = new Vector2(leapForceX, leapForceY);
                hasLeaped = true;
                GD.Print($"Boss leaping towards target at distance: {distance}");
            }
            else
            {
                Vector2 direction = boss.FacingRight ? Vector2.Right : Vector2.Left;
                boss.Velocity = new Vector2(direction.X * boss.LeapSpeed, boss.JumpVelocity);
                hasLeaped = true;
            }
        }
        if (hasLeaped && boss.IsOnFloor() && leapTimer > 1.0f)
        {
            if (boss.PlayerInRange)
            {
                PerformLeapImpact();
            }
            if (boss.CanSeePlayer())
            {
                float distanceToPlayer = boss.GetDistanceToPlayer();
                if (distanceToPlayer <= boss.AttackRange && boss.CanAttack)
                {
                    fsm?.TransitionTo("SpinAttack");
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
            return;
        }
        if (leapTimer >= maxLeapTime)
        {
            fsm?.TransitionTo("Idle");
        }
    }

    private void PerformLeapImpact()
    {
        if (boss.Player != null && boss.Player.HasMethod("TakeDamage"))
        {
            boss.Player.Call("TakeDamage", 30.0f);
            GD.Print("Boss leap impact dealt 30 damage to player!");
        }
    }
}
