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

        // Stop movement during taunt
        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        GD.Print($"Boss Taunt State: Playing 'spr_Taunt_strip'");

        // Play taunt sound or effect here if available
    }

    public override void Update(double delta)
    {
        if (boss.IsDead) return;

        tauntTimer += (float)delta;

        // Keep boss stationary during taunt
        boss.Velocity = new Vector2(0, boss.Velocity.Y);

        // End taunt
        if (tauntTimer >= tauntDuration)
        {
            // Check what to do after taunt
            if (boss.CanSeePlayer())
            {
                float distanceToPlayer = boss.GetDistanceToPlayer();

                if (distanceToPlayer <= boss.AttackRange && boss.PlayerInRange && boss.CanAttack)
                {
                    // Player got close during taunt - punish them!
                    fsm?.TransitionTo("SpinAttack");
                }
                else if (distanceToPlayer > boss.AttackRange)
                {
                    // Player is far - chase them
                    if (GD.Randf() > 0.6f) // 40% chance to leap instead of walk
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
