using Godot;
using System;

public partial class Player : PlayerMovement
{
    [Export]
    Gui gui;
    private int health = 100;
    public void TakeDamage(int damage)
    {
        GD.Print($"Player took {damage} damage.");
        health -= damage;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        GD.Print("Updating health bar...");
        gui.Health = health;
        GD.Print($"Health updated to {gui.Health}.");
    }
}
