using Godot;
using System;

public partial class PauseMenu : Control
{
    private Button _resumeButton;
    private Button _saveGameButton;
    private Button _loadLastSaveButton;
    private Button _quitButton;
    private SaveManager _saveManager;

    [Export] AnimationPlayer _animationPlayer;

    private const string SaveFilePath = "user://savegame.json";

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("PauseMenu: SaveManager not found. Save and Load functionality will be disabled.");
        }

        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            TogglePauseMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void TogglePauseMenu()
    {
        Visible = !Visible;
        GetTree().Paused = Visible;
        if (Visible)
        {
            _animationPlayer.Play("blur");
        }
        else
        {
            _animationPlayer.PlayBackwards("blur");
        }
        if (Visible)
        {
            UpdateLoadLastSaveButtonState();
        }
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

    private void OnResumeButtonPressed()
    {
        TogglePauseMenu();
    }

    private void OnLoadLastSaveButtonPressed()
    {
        if (_saveManager == null)
        {
            GD.PrintErr("PauseMenu: SaveManager not available. Cannot load game.");
            UpdateLoadLastSaveButtonState();
            return;
        }

        if (!FileAccess.FileExists(SaveFilePath))
        {
            GD.Print("PauseMenu: No save file found to load.");
            UpdateLoadLastSaveButtonState();
            return;
        }

        GD.Print("PauseMenu: Load Last Save button pressed.");
        GetTree().Paused = false;

        SaveData loadedData = _saveManager.LoadGame();

        if (loadedData != null)
        {
            _saveManager.ApplyLoadedData(loadedData);
        }
        else
        {
            GD.PrintErr("PauseMenu: Failed to load game data.");
            UpdateLoadLastSaveButtonState();
        }
    }

    private void OnRestartLevelButtonPressed()
    {
        GD.Print("PauseMenu: Restart Level button pressed.");
        GetTree().Paused = false;
        var error = GetTree().ReloadCurrentScene();
        if (error != Error.Ok)
        {
            GD.PrintErr($"PauseMenu: Error restarting level: {error}");
        }
    }

    private void OnQuitToMainMenuButtonPressed()
    {
        GD.Print("PauseMenu: Quit to Main Menu button pressed.");
        GetTree().Paused = false;
        var error = GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
        if (error != Error.Ok)
        {
            GD.PrintErr($"PauseMenu: Error changing scene to MainMenu: {error}");
        }
    }

    private void OnQuitGameButtonPressed()
    {
        GD.Print("PauseMenu: Quit Game button pressed.");
        GetTree().Paused = false;
        GetTree().Quit();
    }
}

