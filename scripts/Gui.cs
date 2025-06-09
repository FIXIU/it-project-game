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
    [Export]
    private TextureProgressBar _healthBar;
    
    public override void _Ready()
    {
        UpdateGui();
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
}
