using Enemies.Enemy1;
using Godot;

public partial class Enemy1Run : State
{
    private Enemy1 enemy;
    private Vector2 direction;

    public override void Enter()
    {
        enemy = GetNode<Enemy1>("../..");
        
        if (enemy == null)
        {
            GD.PrintErr("Enemy1Run: Enemy1 node not found at path '../..'");
            return;
        }

        // Play run animation safely
        enemy.PlayAnimationSafely("Run");
        
        // determine run direction toward player if we can see them, otherwise default to right
        direction = enemy.Player != null && enemy.CanSeePlayer() ? enemy.GetDirectionToPlayer() : Vector2.Right;
        
        // flip sprite to face initial movement direction
        enemy.FlipSprite(direction);
    }

    public override void Update(double delta)
    {
        if (enemy.IsDead) return;
        
        // update direction if we can see the player
        if (enemy.Player != null && enemy.CanSeePlayer())
        {
            direction = enemy.GetDirectionToPlayer();
        }
        
        // flip sprite to face movement direction
        enemy.FlipSprite(direction);
        
        // move enemy
        enemy.Velocity = new Vector2(direction.X * enemy.Speed, enemy.Velocity.Y);
        
        // transition to attack if we can see player and in range
        if (enemy.CanSeePlayer() && enemy.GetDistanceToPlayer() <= enemy.AttackRange)
        {
            fsm?.TransitionTo("Attack");
        }
    }

    public override void Exit()
    {
        // stop movement when exiting run (only if enemy is initialized)
        if (enemy != null)
        {
            enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
        }
    }
}
