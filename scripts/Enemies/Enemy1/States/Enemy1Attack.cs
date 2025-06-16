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

        private float tempAttackTimer = 0.0f;

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

            // Face the player when attacking
            if (enemy.Player != null)
            {
                Vector2 directionToPlayer = enemy.GetDirectionToPlayer();
                enemy.FlipSprite(directionToPlayer);
            }

            // Stop movement during attack
            if (enemy != null)
            {
                enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
            }

            // Reset attack flag
            hasAttacked = false;

            enemy.AttackCooldownTimer.Start(); // Start cooldown timer
            tempAttackTimer = 0.0f; // Reset temporary attack timer


        }

        public override void Update(float delta)
        {
            if (enemy == null || enemy.IsDead) return;
            GD.Print(tempAttackTimer);
            if (tempAttackTimer >= 0.6f)
            {
                fsm?.TransitionTo("Idle");
            }
            
            tempAttackTimer += delta;

            // Keep the enemy stationary during attack
            enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
        }
    }
}
