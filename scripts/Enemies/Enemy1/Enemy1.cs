namespace Enemies.Enemy1;

using Godot;

public partial class Enemy1 : CharacterBody2D, ITakeDamage
{
    [Export] private float speed = 100f;
    public float Speed => speed;

    [Export] private float attackRange = 50f;
    public float AttackRange => attackRange;

    [Export] private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;

    private RayCast2D lineOfSightRay;

    public Node2D Player { get; private set; }
    public bool IsDead { get; private set; }

    public override void _Ready()
    {
        Player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        lineOfSightRay = GetNode<RayCast2D>("LineOfSight");
        stateMachine.TransitionTo("Idle");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsDead)
        {
            MoveAndSlide();
        }
    }

    public Vector2 GetDirectionToPlayer()
    {
        return (Player.GlobalPosition - GlobalPosition).Normalized();
    }

    public float GetDistanceToPlayer()
    {
        return GlobalPosition.DistanceTo(Player.GlobalPosition);
    }

    public bool CanAttackPlayer()
    {
        return Player != null && GetDistanceToPlayer() <= AttackRange;
    }

    public bool CanSeePlayer()
    {
        if (Player == null || lineOfSightRay == null) return false;
        
        // Point the raycast towards the player
        Vector2 directionToPlayer = GetDirectionToPlayer();
        lineOfSightRay.TargetPosition = directionToPlayer * GetDistanceToPlayer();
        
        // Force the raycast to update
        lineOfSightRay.ForceRaycastUpdate();
        
        // Check if raycast hit something
        if (lineOfSightRay.IsColliding())
        {
            // Check if what we hit is the player
            var collider = lineOfSightRay.GetCollider();
            return collider == Player || (collider as Node)?.IsInGroup("player") == true;
        }
        
        // If no collision, we can see the player (assuming they're within range)
        return GetDistanceToPlayer() <= attackRange;
    }

    public void TakeDamage(int damage)
    {
        // TODO: apply damage to health
        stateMachine.TransitionTo("Hurt");
    }
}
