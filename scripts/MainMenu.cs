using Godot;

public partial class MainMenu : Control 
{
    private SaveManager _saveManager;
    private Button _loadGameButton;

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: SaveManager not found.");
            return;
        }

        _loadGameButton = GetNode<Button>("LoadGameButton"); 
        if (_loadGameButton == null)
        {
            GD.PrintErr("MainMenu: LoadGameButton not found.");
            return;
        }

        _loadGameButton.Pressed += OnLoadGameButtonPressed;

        if (!FileAccess.FileExists("user://savegame.json"))
        {
            _loadGameButton.Disabled = true;
            _loadGameButton.Text = "Load Game (No Save)";
        }
    }

    private void OnLoadGameButtonPressed()
    {
        GD.Print("Load Game button pressed.");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: Cannot load game, SaveManager instance is missing.");
            return;
        }

        // Disable the button immediately to prevent rapid re-clicks
        _loadGameButton.Disabled = true; 

        SaveData loadedData = _saveManager.LoadGame();

        if (loadedData != null)
        {
            GD.Print("MainMenu: Data loaded successfully. Applying data...");
            _saveManager.ApplyLoadedData(loadedData);
            // The scene will change. If loading fails and we stay on the menu,
            // we might need a callback from SaveManager to re-enable the button,
            // but the _isApplyingLoadedData flag in SaveManager helps prevent issues.
        }
        else
        {
            GD.PrintErr("MainMenu: Failed to load game data or no save file exists.");
            _loadGameButton.Disabled = false; // Re-enable if loading didn't even start (e.g. file not found by LoadGame)
        }
    }
}