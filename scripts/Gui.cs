using Godot;
using System;

public partial class Gui : Control
{
    private int _health = 100;
    public int Health
    {
        get { return _health; }
        set
        {
            _health = value;
            UpdateGui();
        }
    }

    public bool IsBossActive { get; set; } = false;

    [Export]
    private AnimationPlayer _bossBarAnimationPlayer;
    [Export]
    private TextureProgressBar _bossHealthBar;

    private int _bossHealth = 500;
    public int BossMaxHealth { get; set; } = 500;
    private bool wasBossActive = false;

    public int BossHealth
    {
        get { return _bossHealth; }
        set
        {
            _bossHealth = value;
            UpdateBossGui();
        }
    }
    [Export]
    private TextureProgressBar _healthBar;

    public override void _Ready()
    {
        UpdateGui();
        _bossHealthBar.Value = BossHealth;
    }

    public void UpdateGui()
    {
        _healthBar.Value = Health;
        GD.Print("Updated GUI");
        if (Health < 0)
        {
            Health = 0;
        }
        _healthBar.GetChild<Label>(0).Text = $"{Health} HP";
    }

    public void UpdateBossGui()
    {
        _bossHealthBar.MaxValue = BossMaxHealth;
        if (_bossHealth > 0 && IsBossActive)
        {
            _bossHealthBar.GetChild<Label>(0).Text = $"{_bossHealth} HP";
            _bossHealthBar.Value = _bossHealth;
            if (!wasBossActive)
            {
                _bossBarAnimationPlayer.Play("FadeIn");
                wasBossActive = true;
            }
        }
        else
        {
            _bossBarAnimationPlayer.PlayBackwards("FadeIn");
            wasBossActive = false;
        }
    }

    public void TransitionScreenFade(bool shouldScreenBeDark)
    {
        var screenFadeAnimator = GetNode<AnimationPlayer>("ScreenFade/ScreenFadeAnimationPlayer");
        if (shouldScreenBeDark)
        {
            screenFadeAnimator.PlayBackwards("Fade");
        }
        else
        {
            screenFadeAnimator.Play("Fade");
        }
    }
}
