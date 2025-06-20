using Enemies.Enemy1;
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
            health = 90;
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

    public void ShowBossHealthBar(Boss boss)
    {
        GD.Print("Showing boss health bar...");
        gui.IsBossActive = true;
        gui.BossHealth = boss.Health;
    }

    public void HideBossHealthBar()
    {
        GD.Print("Hiding boss health bar...");
        gui.BossHealth = 0;
        gui.IsBossActive = false;
    }

    public void BossTookDamage(int damage)
    {
        gui.BossHealth -= damage;
    }

    public void TransitionScreenFade(bool shouldScreenBeDark)
    {
        gui.TransitionScreenFade(shouldScreenBeDark);
    }
}
