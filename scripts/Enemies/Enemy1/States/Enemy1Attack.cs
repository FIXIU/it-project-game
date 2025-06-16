using Godot;
using System;
using Enemies.Enemy1;

namespace Enemies.Enemy1.States
{
    public partial class Enemy1Attack : State
    {
        private Enemy1 enemy;
        private Timer attackTimer;
        private float attackDuration = 1.0f; // Duration of attack animation in seconds
        private int attackDamage = 10;
        private bool hasAttacked = false;

        public override void Enter()
        {
            enemy = GetNode<Enemy1>("../..");

            if (enemy == null)
            {
                GD.PrintErr("Enemy1Attack: Enemy1 node not found at path '../..'");
                return;
            }

            // Play attack animation safely
            enemy.PlayAnimationSafely("Attack");

            // Stop movement during attack
            if (enemy != null)
            {
                enemy.Velocity = Vector2.Zero;
            }

            // Reset attack flag
            hasAttacked = false;

            // Create and start attack timer
            attackTimer = new Timer();
            AddChild(attackTimer);
            attackTimer.WaitTime = attackDuration;
            attackTimer.OneShot = true;
            attackTimer.Timeout += OnAttackTimerTimeout;
            attackTimer.Start();

            // Schedule the actual attack to happen mid-animation
            var attackDelayTimer = new Timer();
            AddChild(attackDelayTimer);
            attackDelayTimer.WaitTime = attackDuration * 0.5f; // Attack happens halfway through animation
            attackDelayTimer.OneShot = true;
            attackDelayTimer.Timeout += OnAttackDelayTimeout;
            attackDelayTimer.Start();
        }

        public override void Update(double delta)
        {
            if (enemy == null || enemy.IsDead) return;
            
            // Keep the enemy stationary during attack
            enemy.Velocity = Vector2.Zero;

            // Check if player is still in range and visible
            if (!hasAttacked && (enemy.Player == null || !enemy.CanSeePlayer() || enemy.GetDistanceToPlayer() > enemy.AttackRange))
            {
                // Player moved out of range, cancel attack
                fsm?.TransitionTo("Idle");
            }
        }

        public override void Exit()
        {
            // Clean up timers
            if (attackTimer != null)
            {
                attackTimer.QueueFree();
                attackTimer = null;
            }
        }

        private void OnAttackDelayTimeout()
        {
            // Perform the actual attack
            if (enemy != null && enemy.Player != null && !hasAttacked)
            {
                float distanceToPlayer = enemy.GetDistanceToPlayer();
                
                // Check if player is still in attack range
                if (distanceToPlayer <= enemy.AttackRange && enemy.CanSeePlayer())
                {
                    // Deal damage to player if they implement ITakeDamage
                    if (enemy.Player is ITakeDamage damageable)
                    {
                        damageable.TakeDamage(attackDamage);
                        GD.Print($"Enemy1 attacked player for {attackDamage} damage!");
                    }
                    
                    hasAttacked = true;
                }
            }
        }

        private void OnAttackTimerTimeout()
        {
            // Attack animation finished, decide next state
            if (enemy != null && enemy.Player != null)
            {
                if (enemy.CanSeePlayer() && enemy.GetDistanceToPlayer() <= enemy.AttackRange)
                {
                    // Player still in range, attack again
                    fsm?.TransitionTo("Attack");
                }
                else if (enemy.CanSeePlayer() && enemy.GetDistanceToPlayer() > enemy.AttackRange)
                {
                    // Player visible but out of range, chase them
                    fsm?.TransitionTo("Run");
                }
                else
                {
                    // Can't see player, return to idle
                    fsm?.TransitionTo("Idle");
                }
            }
            else
            {
                // No player, return to idle
                fsm?.TransitionTo("Idle");
            }
        }
    }
}
