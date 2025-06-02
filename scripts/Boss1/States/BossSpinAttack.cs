using Godot;

public partial class BossSpinAttack : State
{
    private Boss boss;
    private float spinTimer = 0.0f;
    private float spinDuration = 1.2f;
    private bool hasHit = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("SpinAttack");
        spinTimer = 0.0f;
        hasHit = false;
        GD.Print("Boss SpinAttack State: Playing 'SpinAttack'");
    }
    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        spinTimer += (float)delta;
        if (!hasHit && spinTimer >= spinDuration * 0.5f)
        {
            PerformSpinAttack();
            hasHit = true;
        }
        if (spinTimer >= spinDuration)
        {
            fsm?.TransitionTo("Idle");
        }
    }
    private void PerformSpinAttack()
    {
        if (boss.PlayerInRange && boss.Player != null && boss.Player.HasMethod("TakeDamage"))
        {
            boss.Player.Call("TakeDamage", 40.0f);
            GD.Print("Boss SpinAttack dealt 40 damage to player!");
        }
    }
}

