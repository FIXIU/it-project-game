using Godot;
using System;

public partial class Player : PlayerMovement, ITakeDamage
{
    [Export]
    DeathScreen _deathScreen;
    [Export]
    Gui gui;
    private int health = 100;
    public void TakeDamage(int damage)
    {
        GD.Print($"Player took {damage} damage.");
        int tempHealth = health;
        health -= damage;
        UpdateHealthBar();
        if (tempHealth > 0 && health <= 0)
        {
            GD.Print("Player health is zero or below. Handling death...");
            HandleDeath();
        }
        if (health >= 100)
        {
            health = 100;
        }
    }

    public void HandleDeath()
    {
        GD.Print("Player has died. Triggering death sequence...");
        ProcessMode = ProcessModeEnum.Disabled;
        _deathScreen.HandleDeathScreen();
    }

    public void UpdateHealthBar()
    {
        GD.Print("Updating health bar...");
        gui.Health = health;
        GD.Print($"Health updated to {gui.Health}.");
    }
}
