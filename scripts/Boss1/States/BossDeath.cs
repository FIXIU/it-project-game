using Godot;

public partial class BossDeath : State
{
    private Boss boss;
    private bool deathStarted = false;
    private float deathTimer = 0.0f;
    private float deathDuration = 6.0f;
    private bool hasDied = false;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("Death");

        deathStarted = true;
        deathTimer = 0.0f;

        boss.Velocity = Vector2.Zero;

        GD.Print($"Boss Death State: Playing 'spr_Death_strip'");

        hasDied = true;
    }

    public override void Update(double delta)
    {
        if (hasDied)
        {
            deathTimer += (float)delta;
        }
        
        boss.Velocity = Vector2.Zero;

        if (deathTimer >= deathDuration)
        {
            boss.QueueFree();
            GD.Print("Boss defeated and removed from scene!");
        }
    }
}

