using Godot;
using System;
using Enemies.Enemy1;

namespace Enemies.Enemy1.States
{
    public partial class Enemy1Idle : State
    {
        private Enemy1 enemy;

        public override void Enter()
        {
            enemy = GetNode<Enemy1>("../..");

            if (enemy == null)
            {
                GD.PrintErr("Enemy1Idle: Enemy1 node not found at path '../..'");
                return;
            }

            // Play idle animation safely
            enemy.PlayAnimationSafely("Idle");

            // Stop horizontal movement (only if enemy is initialized)
            if (enemy != null)
            {
                enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
            }
        }

        public override void Update(double delta)
        {
            if (enemy.IsDead) return;
            
            // Check if player is visible
            if (enemy.Player != null && enemy.CanSeePlayer())
            {
                float distanceToPlayer = enemy.GetDistanceToPlayer();
                
                // If player is in attack range, attack
                if (distanceToPlayer <= enemy.AttackRange)
                {
                    fsm?.TransitionTo("Attack");
                }
                // If player is visible but out of attack range, run towards them
                else
                {
                    fsm?.TransitionTo("Run");
                }
            }
        }
    }
}
