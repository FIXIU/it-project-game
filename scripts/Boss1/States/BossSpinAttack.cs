using Godot;

public partial class BossSpinAttack : State
{
    private Boss boss;
    private float spinTimer = 0.0f;
    private float spinDuration = 3.2f;
    private bool hasHit = false;
    private Vector2 walkDirection;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("SpinAttack");
        spinTimer = 0.0f;
        hasHit = false;
        if (boss.Player != null)
        {
            walkDirection = boss.GetDirectionToPlayer();
        }
        else
        {
            walkDirection = new Vector2(GD.Randf() > 0.5f ? 1 : -1, 0);
        }
    }
    public override void Update(double delta)
    {
        if (boss.IsDead) return;
            spinTimer += (float)delta;
        if (spinTimer >= 1.0f)
        {
            boss.Velocity = new Vector2(walkDirection.X * 50, boss.Velocity.Y);
        }
        if (spinTimer >= spinDuration)
        {
            fsm?.TransitionTo("Idle");
        }
    }
}

