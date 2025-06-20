namespace Enemies.Enemy1;

using Godot;

public partial class Enemy1 : CharacterBody2D, ITakeDamage
{
    [Export] private float speed = 100f;
    public float Speed => speed;

    [Export] private float attackRange = 50f;
    public float AttackRange => attackRange;

    [Export] private float sightRange = 500f;
    public float SightRange => sightRange;

    [Export] private int maxHealth = 100;
    private int currentHealth;
    public int Health => currentHealth;

    [Export] private int attackDamage = 10;
    public int AttackDamage => attackDamage;

    [Export] private StateMachine stateMachine;
    public StateMachine StateMachine => stateMachine;

    private RayCast2D lineOfSightRay;

    public RayCast2D standRay;

    public Node2D Player { get; private set; }
    public bool IsDead { get; private set; }

    [Export] public AnimationPlayer AnimationPlayer;
    
    private bool animationsEnabled = true;
    private bool facingRight = true; // Track which direction the enemy is facing

    [Export] public Timer AttackCooldownTimer;

    // Helper method to flip the enemy sprite based on direction
    public void FlipSprite(Vector2 direction)
    {
        bool shouldFaceRight = direction.X > 0;
        
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            // Flip the sprite by negating the X scale
            Scale = new Vector2(-Scale.X, Scale.Y);
        }
    }

    // Helper method to safely play animations
    public void PlayAnimationSafely(string animationName)
    {
        // Skip all animation operations if animations are disabled
        if (!animationsEnabled)
        {
            return;
        }

        // Don't attempt any animation operations if AnimationPlayer is null
        if (AnimationPlayer == null)
        {
            GD.PrintErr($"Warning: AnimationPlayer not found for Enemy1 when trying to play '{animationName}'. Disabling animations.");
            animationsEnabled = false;
            return;
        }

        try
        {
            // Check if AnimationPlayer has any animations at all
            var animationList = AnimationPlayer.GetAnimationList();
            if (animationList.Length == 0)
            {
                GD.PrintErr($"Warning: No animations found in AnimationPlayer for Enemy1. Disabling animations.");
                animationsEnabled = false;
                return;
            }

            // Check if the specific animation exists
            if (AnimationPlayer.HasAnimation(animationName))
            {
                AnimationPlayer.Play(animationName);
                GD.Print($"Playing animation: {animationName}");
            }
            else
            {
                GD.PrintErr($"Warning: '{animationName}' animation not found in AnimationPlayer for Enemy1");
                GD.Print($"Available animations: {string.Join(", ", animationList)}");
                
                // Don't try fallback animations to avoid more errors
            }
        }
        catch (System.Exception ex)
        {
            GD.PrintErr($"Error playing animation '{animationName}': {ex.Message}. Disabling animations.");
            animationsEnabled = false;
        }
    }

    public override void _Ready()
    {
        currentHealth = maxHealth;
        Player = GetTree().GetFirstNodeInGroup("player") as Node2D;
        
        // Try to get the LineOfSight RayCast2D, but don't fail if it doesn't exist
        lineOfSightRay = GetNodeOrNull<RayCast2D>("LineOfSight");
        standRay = GetNodeOrNull<RayCast2D>("StandRay");
        if (lineOfSightRay == null)
        {
            GD.PrintErr("Warning: LineOfSight RayCast2D node not found for Enemy1. Line of sight checking will be disabled.");
        }
        
        // Validate AnimationPlayer setup
        if (AnimationPlayer == null)
        {
            GD.PrintErr("Warning: AnimationPlayer not assigned for Enemy1. Animations will be disabled.");
            animationsEnabled = false;
        }
        else
        {
            try
            {
                var animList = AnimationPlayer.GetAnimationList();
                if (animList.Length == 0)
                {
                    GD.PrintErr("Warning: No animations found in AnimationPlayer for Enemy1. Animations will be disabled.");
                    animationsEnabled = false;
                }
                else
                {
                    GD.Print($"Enemy1 AnimationPlayer initialized with {animList.Length} animations: {string.Join(", ", animList)}");
                }
            }
            catch (System.Exception ex)
            {
                GD.PrintErr($"Error initializing AnimationPlayer for Enemy1: {ex.Message}. Animations will be disabled.");
                animationsEnabled = false;
            }
        }
        
        stateMachine.TransitionTo("Idle");
        
        // Debug: Print enemy status every 2 seconds
        var debugTimer = new Timer();
        AddChild(debugTimer);
        debugTimer.WaitTime = 2.0f;
        debugTimer.Timeout += PrintEnemyStatus;
        debugTimer.Start();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsDead)
        {
            // Add gravity
            if (!IsOnFloor())
            {
                Velocity += GetGravity() * (float)delta;
            }
            
            MoveAndSlide();
        }
    }

    public Vector2 GetDirectionToPlayer()
    {
        if (Player == null) return Vector2.Zero;
    
        Vector2 direction = (Player.GlobalPosition - GlobalPosition).Normalized();
        
        return direction;
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
        if (Player == null) return false;
        
        float distanceToPlayer = GetDistanceToPlayer();
        
        // Check if player is within sight range
        if (distanceToPlayer > sightRange) return false;
        
        // If we have a RayCast2D, use it for line-of-sight checking
        if (lineOfSightRay != null)
        {
            // Point the raycast towards the player
            Vector2 directionToPlayer = GetDirectionToPlayer();
            lineOfSightRay.TargetPosition = directionToPlayer * distanceToPlayer;
            
            // Force the raycast to update
            lineOfSightRay.ForceRaycastUpdate();
            
            // Check if raycast hit something
            if (lineOfSightRay.IsColliding())
            {
                // Check if what we hit is the player
                var collider = lineOfSightRay.GetCollider();
                return collider == Player || (collider as Node)?.IsInGroup("player") == true;
            }
            
            // If no collision, we can see the player (they're within sight range)
            return true;
        }
        else
        {
            // Fallback: if no RayCast2D, just use distance-based detection
            GD.Print($"Enemy1: Using distance-based detection. Distance to player: {distanceToPlayer}");
            return true; // Player is within sight range, assume we can see them
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage; // Reduce current health by damage amount
        stateMachine.TransitionTo("Hurt");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        IsDead = true;
        stateMachine.TransitionTo("Dead");
        // Add any additional death logic here (e.g., playing a death animation, removing the enemy from the scene, etc.)
    }

    // Debug method to check enemy status
    public void PrintEnemyStatus()
    {
        GD.Print($"=== Enemy1 Status ===");
        GD.Print($"Position: {GlobalPosition}");
        GD.Print($"Player: {(Player != null ? Player.GlobalPosition.ToString() : "null")}");
        GD.Print($"Distance to Player: {(Player != null ? GetDistanceToPlayer().ToString("F1") : "N/A")}");
        GD.Print($"Can See Player: {CanSeePlayer()}");
        GD.Print($"Sight Range: {SightRange}");
        GD.Print($"Attack Range: {AttackRange}");
        GD.Print($"Current State: {(stateMachine?.CurrentState?.Name ?? "null")}");
        GD.Print($"Is Dead: {IsDead}");
        GD.Print($"Health: {currentHealth}/{maxHealth}");
        GD.Print($"LineOfSight Ray: {(lineOfSightRay != null ? "exists" : "null")}");
        GD.Print($"===================");
    }
}
