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
    private ProgressBar _healthBar;
    
    public override void _Ready()
    {
        UpdateGui();
    }

    public void UpdateGui()
    {
        _healthBar.Value = Health;
        GD.Print("Updated GUI");
    }
}
