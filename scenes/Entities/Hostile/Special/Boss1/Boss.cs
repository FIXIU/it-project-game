using System.Net.Http;
using Godot;

public partial class Boss : CharacterBody2D, ITakeDamage
{
    [Export] public float Speed = 100.0f;
    [Export] public float JumpVelocity = -400.0f;
    [Export] public float AttackRange = 50.0f;
    [Export] public float AggroRange = 200.0f;
    [Export] public int Health = 500;
    [Export] public float MaxHealth = 100.0f;
    [Export] public float DashSpeed = 300.0f;
    [Export] public float LeapSpeed = 400.0f;

    [Export] public float VisionRange = 300.0f;
    [Export] public float VisionAngle = 90.0f;
    [Export] public float AttackDetectionRange = 60.0f;
    [Export] public uint VisionCollisionMask = 1;

    [Signal] public delegate void HealthChangedEventHandler(float health);
    [Signal] public delegate void BossDefeatedEventHandler();

    public AnimationPlayer AnimationPlayer;
    public Node2D Player;
    public Timer AttackCooldownTimer;
    public Timer TauntTimer;
    public RayCast2D VisionRaycast;
    public RayCast2D AttackRaycast;

    public float AttackCooldown = 2.0f;
    public bool CanAttack = true;
    public bool IsAttacking = false;
    public bool IsDead = false;
    public bool PlayerInRange = false;
    public bool PlayerDetected = false;
    public bool PlayerInAttackRange = false;

    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
    public Vector2 TargetPosition;
    public bool FacingRight = true;

    [Export] public StateMachine StateMachine;

    [Export]
    public Vector2 Position;

    [Export] private Label HealthIndicator;

    public override void _Ready()
    {
        AnimationPlayer = GetNode<AnimationPlayer>("BossAnimator/AnimationPlayer");
        AttackCooldownTimer = GetNode<Timer>("AttackCooldownTimer");
        TauntTimer = GetNode<Timer>("TauntTimer");

        VisionRaycast = new RayCast2D();
        AddChild(VisionRaycast);
        VisionRaycast.Enabled = true;
        VisionRaycast.CollisionMask = VisionCollisionMask;
        VisionRaycast.CollideWithAreas = false;
        VisionRaycast.CollideWithBodies = true;

        AttackRaycast = new RayCast2D();
        AddChild(AttackRaycast);
        AttackRaycast.Enabled = true;
        AttackRaycast.CollisionMask = VisionCollisionMask;
        AttackRaycast.CollideWithAreas = false;
        AttackRaycast.CollideWithBodies = true;

        Player = GetTree().GetFirstNodeInGroup("player") as Node2D;

        AttackCooldownTimer.Timeout += OnAttackCooldownTimeout;
        TauntTimer.Timeout += OnTauntTimeout;

        AttackCooldownTimer.WaitTime = AttackCooldown;
        TauntTimer.WaitTime = 3.0f;

        TauntTimer.Start();
        StateMachine.TransitionTo("Idle");

        GlobalPosition = Position;
    }

    public override void _Process(double delta)
    {
        StateMachine?.CurrentState?.Update(delta);

        UpdatePlayerDetection();

        UpdateAttackRangeDetection();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsOnFloor())
        {
            Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
        }

        if (Player != null && !IsDead && (StateMachine?.CurrentState?.Name == "Idle" || StateMachine?.CurrentState?.Name == "Walk"))
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

        if (distanceToPlayer > VisionRange)
        {
            PlayerDetected = false;
            return;
        }

        if (!IsPlayerInVisionCone(directionToPlayer))
        {
            PlayerDetected = false;
            return;
        }

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

        if (distanceToPlayer > AttackDetectionRange)
        {
            PlayerInRange = false;
            PlayerInAttackRange = false;
            return;
        }

        if (distanceToPlayer <= AttackRange)
        {
            PlayerInAttackRange = CanSeePlayerDirectly(AttackRaycast, AttackRange);
            PlayerInRange = PlayerInAttackRange;
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

        raycast.GlobalPosition = startPosition;
        raycast.TargetPosition = direction * distance;
        raycast.ForceRaycastUpdate();

        if (raycast.IsColliding())
        {
            var collider = raycast.GetCollider();

            if (collider is Node node && node.IsInGroup("player"))
            {
                return true;
            }

            return false;
        }

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

    // public void TakeDamage(float damage)
    // {
    //     if (IsDead) return;
    //
    //     Health -= damage;
    //     Health = Mathf.Max(0, Health);
    //
    //     EmitSignal(SignalName.HealthChanged, Health);
    //
    //     if (Health <= 0)
    //     {
    //         Die();
    //     }
    //     else
    //     {
    //         StateMachine?.TransitionTo("Taunt");
    //     }
    // }

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
        if (StateMachine?.CurrentState?.Name == "Idle" && !PlayerDetected)
        {
            if (GD.Randf() > 0.7f)
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

    public void TakeDamage(int damage)
    {

        Health -= damage;
        GD.Print($"BOSS DAMAGED: {Health}");
        if (Health <= 0)
        {
            Die();
        }
    }
}

