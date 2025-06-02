using Godot;

public partial class BossDeath : State
{
    private Boss boss;
    private bool deathStarted = false;
    private float deathTimer = 0.0f;
    private float deathDuration = 3.0f;

    public override void Enter()
    {
        boss = GetNode<Boss>("../..");
        var animationPlayer = GetNode<AnimationPlayer>("../../BossAnimator/AnimationPlayer");
        animationPlayer.Play("spr_Death_strip");

        deathStarted = true;
        deathTimer = 0.0f;

        // Stop all movement
        boss.Velocity = Vector2.Zero;

        // Disable collision or other cleanup
        if (boss.AttackArea != null)
        {
            boss.AttackArea.SetDeferred("monitoring", false);
        }
        if (boss.DetectionArea != null)
        {
            boss.DetectionArea.SetDeferred("monitoring", false);
        }

        GD.Print($"Boss Death State: Playing 'spr_Death_strip'");
    }

    public override void Update(double delta)
    {
        deathTimer += (float)delta;

        // Keep boss motionless
        boss.Velocity = Vector2.Zero;

        // Optional: Add death effects, screen shake, etc.

        // After death animation completes, you could:
        // - Remove the boss from scene
        // - Trigger end game sequence
        // - Drop loot, etc.

        if (deathTimer >= deathDuration)
        {
            // Death sequence complete
            GD.Print("Boss death sequence complete");

            // Optional: Queue free the boss or trigger end game
            // boss.QueueFree();
        }
    }

    public override void Exit()
    {
        // This state should not be exited once entered
        // Boss is dead and stays dead
    }
}