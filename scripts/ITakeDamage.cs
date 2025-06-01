using Godot;
using System;

public interface ITakeDamage
{
    void TakeDamage(float damage, Vector2? attackFromVector);
}
