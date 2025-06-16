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
            GD.Print("Enemy1Idle: Entering Idle state");
            enemy = GetNode<Enemy1>("../..");

            if (enemy == null)
            {
                GD.PrintErr("Enemy1Idle: Enemy1 node not found at path '../..'");
                return;
            }

            GD.Print($"Enemy1Idle: Enemy found. FSM reference: {(fsm != null ? "exists" : "null")}");
            
            // Play idle animation safely
            enemy.PlayAnimationSafely("Idle");

            // Stop horizontal movement (only if enemy is initialized)
            if (enemy != null)
            {
                enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
            }
        }

        public override void Update(float delta)
        {
            if (enemy.IsDead) return;
            
            // Debug information
            if (enemy.Player != null)
            {
                float distanceToPlayer = enemy.GetDistanceToPlayer();
                bool canSeePlayer = enemy.CanSeePlayer();
                
                // Check if player is visible
                if (canSeePlayer)
                {
                    // If player is in attack range, attack
                    if (distanceToPlayer <= enemy.AttackRange && enemy.AttackCooldownTimer.TimeLeft <= 0)
                    {
                        fsm?.TransitionTo("Attack");
                    }
                    // If player is visible but out of attack range, run towards them
                    else if (enemy.standRay.IsColliding())
                    {
                        fsm?.TransitionTo("Run");
                    }
                    else
                    {
                        enemy.FlipSprite(enemy.GetDirectionToPlayer());
                    }
                }
            }
        }
    }
}
