using Godot;

public partial class MainMenu : Control 
{
    private SaveManager _saveManager;
    private Button _loadGameButton;
    private Button _newGameButton; // Add this

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("MainMenu: SaveManager not found.");
            return;
        }

        // Setup Load Game button
        _loadGameButton = GetNode<Button>("LoadGameButton"); 
        if (_loadGameButton == null)
        {
            GD.PrintErr("MainMenu: LoadGameButton not found.");
            return;
        }
        _loadGameButton.Pressed += OnLoadGameButtonPressed;

        // Setup New Game button
        _newGameButton = GetNode<Button>("NewGameButton");
        if (_newGameButton == null)
        {
            GD.PrintErr("MainMenu: NewGameButton not found.");
            return;
        }
        _newGameButton.Pressed += OnNewGameButtonPressed;

        // Update load button state based on save file existence
        UpdateLoadButtonState();
    }

    private void UpdateLoadButtonState()
    {
        if (!FileAccess.FileExists("user://savegame.json"))
        {
            _loadGameButton.Disabled = true;
            _loadGameButton.Text = "Load Game (No Save)";
        }
        else
        {
            _loadGameButton.Disabled = false;
            _loadGameButton.Text = "Load Game";
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

        // Disable both buttons to prevent rapid clicks
        _newGameButton.Disabled = true;
        _loadGameButton.Disabled = true;

        // Optionally clear existing save file for a fresh start
        // Uncomment the next line if you want to clear saves when starting new game
        // _saveManager.ClearSaveFile();

        // Start the new game
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

        // Disable both buttons immediately to prevent rapid re-clicks
        _loadGameButton.Disabled = true;
        _newGameButton.Disabled = true;

        SaveData loadedData = _saveManager.LoadGame();

        if (loadedData != null)
        {
            GD.Print("MainMenu: Data loaded successfully. Applying data...");
            _saveManager.ApplyLoadedData(loadedData);
        }
        else
        {
            GD.PrintErr("MainMenu: Failed to load game data or no save file exists.");
            // Re-enable buttons if loading failed
            _newGameButton.Disabled = false;
            UpdateLoadButtonState();
        }
    }
}