using Godot;
using System;
using System.Linq;

public partial class Enemy1 : CharacterBody2D
{
    // Movement speed of the boss
    [Export] public float Speed { get; set; } = 120f;

    // Gravity (pixels/sec²) – mimic PlayerMovement default gravity
    [Export] public float Gravity { get; set; } = 980f;

    // Patrol points in local or global coordinates (set in the editor)
    [Export] public Vector2 PointA { get; set; }
    [Export] public Vector2 PointB { get; set; }

    // How far the boss will detect and chase the player
    [Export] public float DetectionRange { get; set; } = 250f;

    // How close to the player before attacking
    [Export] public float AttackRange { get; set; } = 40f;

    // Path to the boss’s state machine node
    [Export] public NodePath StateMachinePath { get; set; }
    private StateMachine _stateMachine;

    // Node2D that contains the boss visuals (for flipping)
    [Export] public Node2D VisualsNode { get; set; }

    private Vector2 _currentTarget;
    private Node2D _player;
    private Vector2 _velocity = Vector2.Zero;

    public override void _Ready()
    {
        // Initialize patrol target
        _currentTarget = PointB;

        // Find the first player in the scene
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
            _player = players.First() as Node2D;

        // Cache the state machine
        if (StateMachinePath != null && StateMachinePath != string.Empty)
            _stateMachine = GetNode<StateMachine>(StateMachinePath);
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;
        bool onFloor = IsOnFloor();
        Vector2 pos = Position;

        // 1) Vertical – always apply gravity when not on floor
        if (!onFloor)
        {
            _velocity.Y += Gravity * dt;
            _stateMachine?.TransitionTo("Jump");
        }
        else
        {
            // zero out downward velocity on landing
            if (_velocity.Y > 0)
                _velocity.Y = 0;
        }

        // 2) Attack check
        if (_player != null && pos.DistanceTo(_player.Position) <= AttackRange)
        {
            _velocity.X = 0;
            _stateMachine?.TransitionTo("Attack");
        }
        else
        {
            // 3) Decide horizontal direction (chase vs patrol)
            bool chasing = _player != null && pos.DistanceTo(_player.Position) <= DetectionRange;
            Vector2 dir = Vector2.Zero;
            if (chasing)
            {
                dir = (_player.Position - pos).Normalized();
                _stateMachine?.TransitionTo("Walk");
            }
            else
            {
                if (pos.DistanceTo(_currentTarget) < 8f)
                    _currentTarget = _currentTarget == PointA ? PointB : PointA;
                dir = (_currentTarget - pos).Normalized();
                _stateMachine?.TransitionTo(Mathf.Abs(dir.X) > 0.1f ? "Walk" : "Idle");
            }

            // 4) Apply horizontal speed
            _velocity.X = dir.X * Speed;

            // 5) Flip visuals
            if (!Mathf.IsEqualApprox(dir.X, 0f))
                FlipVisuals(dir.X);
        }

        // 6) Move and slide with up-direction = -Y
        Velocity = _velocity;
        MoveAndSlide();
    }

    private void FlipVisuals(float dirX)
    {
        if (VisualsNode == null)
            return;

        var s = VisualsNode.Scale;
        s.X = dirX < 0 ? -Mathf.Abs(s.X) : Mathf.Abs(s.X);
        VisualsNode.Scale = s;
    }
}