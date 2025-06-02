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

    [Signal] public delegate void HealthChangedEventHandler(float health);
    [Signal] public delegate void BossDefeatedEventHandler();

    // References
    public StateMachine StateMachine;
    public AnimationPlayer AnimationPlayer;
    public Area2D AttackArea;
    public Area2D DetectionArea;
    public Node2D Player;
    public Timer AttackCooldownTimer;
    public Timer TauntTimer;

    // Combat variables
    public float AttackCooldown = 2.0f;
    public bool CanAttack = true;
    public bool IsAttacking = false;
    public bool IsDead = false;
    public bool PlayerInRange = false;
    public bool PlayerDetected = true;

    // Movement variables
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    public Vector2 TargetPosition;
    public bool FacingRight = true;

    public override void _Ready()
    {
        // Get references
        StateMachine = GetNode<StateMachine>("FSM");
        AnimationPlayer = GetNode<AnimationPlayer>("BossAnimator/AnimationPlayer");
        AttackArea = GetNode<Area2D>("AttackArea");
        DetectionArea = GetNode<Area2D>("DetectionArea");
        AttackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
        TauntTimer = GetNode<Timer>("TauntTimer");

        // Find player in scene
        Player = GetNode<Node2D>("/root/Game/Player") ?? GetTree().GetFirstNodeInGroup("player") as Node2D;

        // Setup signals
        AttackArea.BodyEntered += OnAttackAreaBodyEntered;
        AttackArea.BodyExited += OnAttackAreaBodyExited;
        DetectionArea.BodyEntered += OnDetectionAreaBodyEntered;
        DetectionArea.BodyExited += OnDetectionAreaBodyExited;
        AttackCooldownTimer.Timeout += OnAttackCooldownTimeout;
        TauntTimer.Timeout += OnTauntTimeout;

        // Configure timers
        AttackCooldownTimer.WaitTime = AttackCooldown;
        TauntTimer.WaitTime = 3.0f; // Taunt every 3 seconds when idle

        // Start taunt timer
        TauntTimer.Start();
        StateMachine.TransitionTo("Idle");

        GD.Print("Boss initialized successfully");
    }
    
    public override void _Process(double delta)
    {
        StateMachine?.CurrentState.Update(delta);
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

    // Signal handlers
    private void OnAttackAreaBodyEntered(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            PlayerInRange = true;
        }
    }

    private void OnAttackAreaBodyExited(Node2D body)
    {
        if (body.IsInGroup("player"))
        {
            PlayerInRange = false;
        }
    }

    private void OnDetectionAreaBodyEntered(Node2D body)
    {
        GD.Print("Player detected!");
        if (body.IsInGroup("player"))
        {
            PlayerDetected = true;
        }
    }

    private void OnDetectionAreaBodyExited(Node2D body)
    {
        GD.Print("Player undetected!");
        if (body.IsInGroup("player"))
        {
            PlayerDetected = false;
        }
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
