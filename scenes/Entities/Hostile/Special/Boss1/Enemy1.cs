using Godot;
using System;
using System.Linq;

public partial class Enemy1 : CharacterBody2D
{
    // Movement speed of the boss
    [Export] public float Speed { get; set; } = 120f;

    // Patrol points in local or global coordinates (set in the editor)
    [Export] public Vector2 PointA { get; set; }
    [Export] public Vector2 PointB { get; set; }

    // How far the boss will detect and chase the player
    [Export] public float DetectionRange { get; set; } = 250f;

    // Node2D that contains the boss visuals (for flipping)
    [Export] public Node2D VisualsNode { get; set; }

    private Vector2 _currentTarget;
    private Node2D _player;

    public override void _Ready()
    {
        // Initialize patrol target
        _currentTarget = PointB;

        // Find the first node in the "Player" group
        var players = GetTree().GetNodesInGroup("Player");
        if (players.Count > 0)
            _player = players.First() as Node2D;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Vector2.Zero;
        Vector2 position = Position;

        // If player is within detection range, chase
        if (_player != null && position.DistanceTo(_player.Position) <= DetectionRange)
        {
            direction = (_player.Position - position).Normalized();
        }
        else
        {
            // Patrol between PointA and PointB
            if (position.DistanceTo(_currentTarget) < 8f)
                _currentTarget = _currentTarget == PointA ? PointB : PointA;

            direction = (_currentTarget - position).Normalized();
        }

        // Apply movement
        Velocity = direction * Speed;
        MoveAndSlide();

        // Flip visuals based on movement direction
        if (!Mathf.IsEqualApprox(direction.X, 0f))
            FlipVisuals(direction.X);
    }

    private void FlipVisuals(float dirX)
    {
        if (VisualsNode == null)
            return;

        // Ensure scale.x is non-zero
        var scale = VisualsNode.Scale;
        scale.X = dirX < 0 ? -Mathf.Abs(scale.X) : Mathf.Abs(scale.X);
        VisualsNode.Scale = scale;
    }
}