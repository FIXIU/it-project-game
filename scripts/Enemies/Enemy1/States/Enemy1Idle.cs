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
            var anim = GetNode<AnimationPlayer>("../EnemyAnimator/AnimationPlayer");
            anim.Play("Idle");
            // Stop horizontal movement
            enemy.Velocity = new Vector2(0, enemy.Velocity.Y);
        }

        public override void Update(double delta)
        {
            if (enemy.IsDead) return;
            // Idle state update logic
        }
    }
}
