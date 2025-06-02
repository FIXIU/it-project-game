using Godot;

public partial class MainMenu : Control 
{
    private SaveManager _saveManager;
    private Button _loadGameButton;
    private Button _newGameButton;

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: SaveManager not found.");
            return;
        }

        _loadGameButton = GetNode<Button>("PanelContainer/VBoxContainer/LoadGameButton");
        _newGameButton  = GetNode<Button>("PanelContainer/VBoxContainer/NewGameButton");

        UpdateLoadButtonState();
    }

    private void UpdateLoadButtonState()
    {
        if (!FileAccess.FileExists("user://savegame.json"))
        {
            _loadGameButton.Disabled = true;
            _loadGameButton.Text     = "Load Game (No Save)";
        }
        else
        {
            _loadGameButton.Disabled = false;
            _loadGameButton.Text     = "Load Game";
        }
    }

    private void OnNewGameButtonPressed()
    {
        GD.Print("New Game button pressed.");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: Cannot start new game, SaveManager instance is missing.");
            return;
        }

        _newGameButton.Disabled  = true;
        _loadGameButton.Disabled = true;
        _saveManager.StartNewGame();
    }

    private void OnLoadGameButtonPressed()
    {
        GD.Print("Load Game button pressed.");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: Cannot load game, SaveManager instance is missing.");
            return;
        }

        _loadGameButton.Disabled = true;
        _newGameButton.Disabled  = true;

        SaveData loadedData = _saveManager.LoadGame();
        if (loadedData != null)
        {
            GD.Print("MainMenu: Data loaded successfully. Applying data...");
            _saveManager.ApplyLoadedData(loadedData);
        }
        else
        {
            GD.PrintErr("MainMenu: Failed to load game data or no save file exists.");
            _newGameButton.Disabled = false;
            UpdateLoadButtonState();
        }
    }
}

