using Godot;
using System;

public partial class DeathScreen : Control
{
    private Button _loadLastSaveButton;
    private Button _menuButton;
    private SaveManager _saveManager;

    [Export] AnimationPlayer _animationPlayer;
    
    private const string SaveFilePath = "user://savegame.json";
    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("DeathScreen: SaveManager not found. Save and Load functionality will be disabled.");
        }

        Hide();
    }
    public void HandleDeathScreen()
    {
        Show();
        GD.Print("DeathScreen: Player has died. Showing death screen.");
        _animationPlayer.Play("DeathSequence");
        GD.Print("DeathScreen: Death sequence animation started.");
    }
    
    private void UpdateLoadLastSaveButtonState()
    {
        if (_loadLastSaveButton == null) return;

        if (_saveManager == null || !FileAccess.FileExists(SaveFilePath))
        {
            _loadLastSaveButton.Disabled = true;
            _loadLastSaveButton.Text = "Load Last Save (Unavailable)";
        }
        else
        {
            _loadLastSaveButton.Disabled = false;
            _loadLastSaveButton.Text = "Load Last Save";
        }
    }
    
    private void OnLoadLastSaveButtonPressed()
    {
        if (_saveManager == null)
        {
            GD.PrintErr("DeathScreen: SaveManager not available. Cannot load game.");
            UpdateLoadLastSaveButtonState();
            return;
        }

        if (!FileAccess.FileExists(SaveFilePath))
        {
            GD.Print("DeathScreen: No save file found to load.");
            UpdateLoadLastSaveButtonState();
            return;
        }

        GD.Print("DeathScreen: Load Last Save button pressed.");
        GetTree().Paused = false;

        SaveData loadedData = _saveManager.LoadGame();

        if (loadedData != null)
        {
            _saveManager.ApplyLoadedData(loadedData);
        }
        else
        {
            GD.PrintErr("DeathScreen: Failed to load game data.");
            UpdateLoadLastSaveButtonState();
        }
    }

    private void OnRestartLevelButtonPressed()
    {
        GD.Print("DeathScreen: Restart Level button pressed.");
        GetTree().Paused = false;
        var error = GetTree().ReloadCurrentScene();
        if (error != Error.Ok)
        {
            GD.PrintErr($"DeathScreen: Error restarting level: {error}");
        }
    }

    private void OnQuitToMainMenuButtonPressed()
    {
        GD.Print("DeathScreen: Quit to Main Menu button pressed.");
        GetTree().Paused = false;
        var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        if (error != Error.Ok)
        {
            GD.PrintErr($"DeathScreen: Error changing scene to MainMenu: {error}");
        }
    }

    private void OnQuitGameButtonPressed()
    {
        GD.Print("DeathScreen: Quit Game button pressed.");
        GetTree().Paused = false;
        GetTree().Quit();
    }

}
