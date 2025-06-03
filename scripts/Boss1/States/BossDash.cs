using Godot;

public partial class BossDash : State
{
    private Boss boss;
    private float dashTimer = 0.0f;
    private float dashDuration = 0.7f;
    private bool hasDashed = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Dash");
        dashTimer = 0.0f;
        hasDashed = false;
        GD.Print("Boss Dash State: Playing 'Dash'");
    }
    public override void Update(double delta)
    {
        if (boss.IsDead) return;
        dashTimer += (float)delta;
        if (!hasDashed && dashTimer >= 0.25f)
        {
            PerformDash();
            hasDashed = true;
        }
        if (dashTimer >= dashDuration)
        {
            fsm?.TransitionTo("Idle");
        }
    }
    private void PerformDash()
    {
        Vector2 dashDirection = boss.FacingRight ? Vector2.Right : Vector2.Left;
        boss.GlobalPosition += new Vector2(dashDirection.X * 58, 0);
    }
}
