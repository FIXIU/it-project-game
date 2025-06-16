using Godot;
using System;
using Enemies.Enemy1;

namespace Enemies.Enemy1.States
{
    public partial class Enemy1Hurt : State
    {
        private Enemy1 enemy;
        private Timer hurtTimer;
        private float hurtDuration = 0.5f; // Duration of hurt state in seconds

        public override void Enter()
        {
            enemy = GetNode<Enemy1>("../..");

            if (enemy == null)
            {
                GD.PrintErr("Enemy1Hurt: Enemy1 node not found at path '../..'");
                return;
            }

            // Play hurt animation safely
            enemy.PlayAnimationSafely("Hurt");

            // Stop movement during hurt
            if (enemy != null)
            {
                enemy.Velocity = Vector2.Zero;
            }

            // Create and start hurt timer
            hurtTimer = new Timer();
            AddChild(hurtTimer);
            hurtTimer.WaitTime = hurtDuration;
            hurtTimer.OneShot = true;
            hurtTimer.Timeout += OnHurtTimerTimeout;
            hurtTimer.Start();
        }

        public override void Update(float delta)
        {
            if (enemy == null || enemy.IsDead) return;
            
            // Keep the enemy stationary during hurt state
            enemy.Velocity = Vector2.Zero;
        }

        public override void Exit()
        {
            // Clean up timer
            if (hurtTimer != null)
            {
                hurtTimer.QueueFree();
                hurtTimer = null;
            }
        }

        private void OnHurtTimerTimeout()
        {
            // Transition back to idle after hurt duration
            fsm?.TransitionTo("Idle");
        }
    }
}
