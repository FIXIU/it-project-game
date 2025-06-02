using Godot;

public partial class Boss : CharacterBody2D
{
    [Export] public float Speed = 100.0f;
    [Export] public float JumpVelocity = -400.0f;
    [Export] public float AttackRange = 50.0f;
    [Export] public float AggroRange = 200.0f;
    [Export] public float Health = 100.0f;
    [Export] public float MaxHealth = 100.0f;
    [Export] public float DashSpeed = 300.0f;
    [Export] public float LeapSpeed = 400.0f;
    
    // Raycast detection parameters
    [Export] public float VisionRange = 300.0f;
    [Export] public float VisionAngle = 90.0f; // Total angle in degrees
    [Export] public float AttackDetectionRange = 60.0f; // Range for attack raycast detection
    [Export] public uint VisionCollisionMask = 1; // Physics layer for walls/obstacles

    [Signal] public delegate void HealthChangedEventHandler(float health);
    [Signal] public delegate void BossDefeatedEventHandler();

    // References
    public StateMachine StateMachine;
    public AnimationPlayer AnimationPlayer;
    public Node2D Player;
    public Timer AttackCooldownTimer;
    public Timer TauntTimer;
    public RayCast2D VisionRaycast;
    public RayCast2D AttackRaycast;

    // Combat variables
    public float AttackCooldown = 2.0f;
    public bool CanAttack = true;
    public bool IsAttacking = false;
    public bool IsDead = false;
    public bool PlayerInRange = false;
    public bool PlayerDetected = false;
    public bool PlayerInAttackRange = false;

    // Movement variables
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    public Vector2 TargetPosition;
    public bool FacingRight = true;

    public override void _Ready()
    {
        // Get references
        StateMachine = GetNode<StateMachine>("FSM");
        AnimationPlayer = GetNode<AnimationPlayer>("BossAnimator/AnimationPlayer");
        AttackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
        TauntTimer = GetNode<Timer>("TauntTimer");

        // Create and setup raycast for vision
        VisionRaycast = new RayCast2D();
        AddChild(VisionRaycast);
        VisionRaycast.Enabled = true;
        VisionRaycast.CollisionMask = VisionCollisionMask;
        VisionRaycast.CollideWithAreas = false;
        VisionRaycast.CollideWithBodies = true;

        // Create and setup raycast for attack detection
        AttackRaycast = new RayCast2D();
        AddChild(AttackRaycast);
        AttackRaycast.Enabled = true;
        AttackRaycast.CollisionMask = VisionCollisionMask;
        AttackRaycast.CollideWithAreas = false;
        AttackRaycast.CollideWithBodies = true;

        // Find player in scene
        Player = GetNode<Node2D>("/root/Game/Player") ?? GetTree().GetFirstNodeInGroup("player") as Node2D;

        // Setup signals
        AttackCooldownTimer.Timeout += OnAttackCooldownTimeout;
        TauntTimer.Timeout += OnTauntTimeout;

        // Configure timers
        AttackCooldownTimer.WaitTime = AttackCooldown;
        TauntTimer.WaitTime = 3.0f; // Taunt every 3 seconds when idle

        // Start taunt timer
        TauntTimer.Start();
        StateMachine.TransitionTo("Idle");

        GD.Print("Boss initialized successfully with raycast vision and attack detection");
    }
    
    public override void _Process(double delta)
    {
        StateMachine?.CurrentState.Update(delta);
        
        // Update player detection using raycast
        UpdatePlayerDetection();
        
        // Update attack range detection using raycast
        UpdateAttackRangeDetection();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Apply gravity
        if (!IsOnFloor())
        {
            Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
        }

        // Update facing direction based on player position
        if (Player != null && !IsDead)
        {
            UpdateFacingDirection();
        }

        MoveAndSlide();
    }

    private void UpdatePlayerDetection()
    {
        if (Player == null || IsDead)
        {
            PlayerDetected = false;
            return;
        }

        Vector2 directionToPlayer = GetDirectionToPlayer();
        float distanceToPlayer = GetDistanceToPlayer();

        // Check if player is within vision range
        if (distanceToPlayer > VisionRange)
        {
            PlayerDetected = false;
            return;
        }

        // Check if player is within vision angle
        if (!IsPlayerInVisionCone(directionToPlayer))
        {
            PlayerDetected = false;
            return;
        }

        // Perform raycast to check for obstacles
        PlayerDetected = CanSeePlayerDirectly(VisionRaycast, VisionRange);
    }

    private void UpdateAttackRangeDetection()
    {
        if (Player == null || IsDead)
        {
            PlayerInRange = false;
            PlayerInAttackRange = false;
            return;
        }

        float distanceToPlayer = GetDistanceToPlayer();

        // Check if player is within attack detection range
        if (distanceToPlayer > AttackDetectionRange)
        {
            PlayerInRange = false;
            PlayerInAttackRange = false;
            return;
        }

        // Check if player is within actual attack range using raycast
        if (distanceToPlayer <= AttackRange)
        {
            // Use raycast to verify clear line of sight for attack
            PlayerInAttackRange = CanSeePlayerDirectly(AttackRaycast, AttackRange);
            PlayerInRange = PlayerInAttackRange; // Keep compatibility with existing code
        }
        else
        {
            PlayerInAttackRange = false;
            PlayerInRange = false;
        }
    }

    private bool IsPlayerInVisionCone(Vector2 directionToPlayer)
    {
        Vector2 forwardDirection = FacingRight ? Vector2.Right : Vector2.Left;
        float angleToPlayer = forwardDirection.AngleTo(directionToPlayer);
        float halfVisionAngle = Mathf.DegToRad(VisionAngle / 2.0f);
        
        return Mathf.Abs(angleToPlayer) <= halfVisionAngle;
    }

    private bool CanSeePlayerDirectly(RayCast2D raycast, float maxDistance)
    {
        if (Player == null) return false;

        Vector2 startPosition = GlobalPosition;
        Vector2 targetPosition = Player.GlobalPosition;
        Vector2 direction = (targetPosition - startPosition).Normalized();
        float distance = Mathf.Min(startPosition.DistanceTo(targetPosition), maxDistance);

        // Set up the raycast
        raycast.GlobalPosition = startPosition;
        raycast.TargetPosition = direction * distance;
        raycast.ForceRaycastUpdate();

        // If raycast hits something, check if it's the player or an obstacle
        if (raycast.IsColliding())
        {
            var collider = raycast.GetCollider();
            
            // Check if the collider is the player
            if (collider is Node node && node.IsInGroup("player"))
            {
                return true;
            }
            
            // If it hit something else (wall, obstacle), player is blocked
            return false;
        }

        // No collision means clear line of sight within range
        return true;
    }

    private void UpdateFacingDirection()
    {
        if (Player.GlobalPosition.X > GlobalPosition.X && !FacingRight)
        {
            FlipSprite();
        }
        else if (Player.GlobalPosition.X < GlobalPosition.X && FacingRight)
        {
            FlipSprite();
        }
    }

    private void FlipSprite()
    {
        FacingRight = !FacingRight;
        Scale = new Vector2(-Scale.X, Scale.Y);
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;

        Health -= damage;
        Health = Mathf.Max(0, Health);

        EmitSignal(SignalName.HealthChanged, Health);

        if (Health <= 0)
        {
            Die();
        }
        else
        {
            // Trigger hurt state or animation
            StateMachine?.TransitionTo("Taunt"); // Brief hurt reaction
        }
    }

    private void Die()
    {
        IsDead = true;
        StateMachine?.TransitionTo("Death");
        EmitSignal(SignalName.BossDefeated);
    }

    public float GetDistanceToPlayer()
    {
        if (Player == null) return float.MaxValue;
        return GlobalPosition.DistanceTo(Player.GlobalPosition);
    }

    public Vector2 GetDirectionToPlayer()
    {
        if (Player == null) return Vector2.Zero;
        return (Player.GlobalPosition - GlobalPosition).Normalized();
    }

    public bool CanSeePlayer()
    {
        return PlayerDetected && Player != null;
    }

    // New method to check if player is in attack range with raycast verification
    public bool CanAttackPlayer()
    {
        return CanAttack && PlayerInAttackRange && Player != null && !IsDead;
    }

    private void OnAttackCooldownTimeout()
    {
        CanAttack = true;
    }

    private void OnTauntTimeout()
    {
        // Randomly taunt when idle
        if (StateMachine?.CurrentState?.Name == "Idle" && !PlayerDetected)
        {
            if (GD.Randf() > 0.7f) // 30% chance to taunt
            {
                StateMachine?.TransitionTo("Taunt");
            }
        }
    }

    public void StartAttackCooldown()
    {
        CanAttack = false;
        AttackCooldownTimer.Start();
    }
}