using Godot;
using System;
using Enemies.Enemy1;

namespace Enemies.Enemy1.States
{
    public partial class Enemy1Dead : State
    {
        private Enemy1 enemy;
        private Timer deathTimer;
        private float deathDuration = 2.0f; // How long the death animation/state lasts

        public override void Enter()
        {
            enemy = GetNode<Enemy1>("../..");

            if (enemy == null)
            {
                GD.PrintErr("Enemy1Dead: Enemy1 node not found at path '../..'");
                return;
            }

            // Play death animation safely
            enemy.PlayAnimationSafely("Death");

            // Stop all movement
            if (enemy != null)
            {
                enemy.Velocity = Vector2.Zero;
            }

            // Create and start death timer
            deathTimer = new Timer();
            AddChild(deathTimer);
            deathTimer.WaitTime = deathDuration;
            deathTimer.OneShot = true;
            deathTimer.Timeout += OnDeathTimerTimeout;
            deathTimer.Start();

            GD.Print("Enemy1 has died!");
        }

        public override void Update(double delta)
        {
            if (enemy == null) return;
            
            // Keep the enemy stationary while dead
            enemy.Velocity = Vector2.Zero;
        }

        public override void Exit()
        {
            // Clean up timer
            if (deathTimer != null)
            {
                deathTimer.QueueFree();
                deathTimer = null;
            }
        }

        private void OnDeathTimerTimeout()
        {
            // Remove the enemy from the scene after death animation
            if (enemy != null)
            {
                enemy.QueueFree();
            }
        }
    }
}
