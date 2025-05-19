using Godot;

public partial class SavePoint : Area2D
{
    private SaveManager _saveManager;
    private bool _playerInRange = false;
    private Node2D _playerNode = null; // To store a reference to the player if needed for position

    public override void _Ready()
    {
        // Get the SaveManager autoload instance
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("SavePoint: SaveManager not found. Make sure it's autoloaded.");
        }

        // Connect the signals for body entered and exited
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        // Check if the body that entered is the player
        // You might have a specific group "Player" for your player node,
        // or check its class name, or a specific script attached to it.
        if (body.IsInGroup("Player") || body is Player) // Assuming your player is in group "Player" or is of type PlayerCharacter
        {
            GD.Print("Player entered save point area.");
            _playerInRange = true;
            _playerNode = body; // Store player reference
            // Optional: Show a visual cue, like "Press E to Save"
            // You could also save immediately upon entering:
            // AttemptSave();
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player") || body is Player)
        {
            GD.Print("Player exited save point area.");
            _playerInRange = false;
            _playerNode = null;
            // Optional: Hide visual cue
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Check if the player is in range and presses a specific key (e.g., "E")
        if (_playerInRange && @event.IsActionPressed("save_interaction")) // "ui_accept" is typically Enter/Space. Create a custom action like "save_game" (e.g., mapped to E).
        {
            GD.Print("Save action triggered by player.");
            AttemptSave();
            GetTree().Root.SetInputAsHandled(); // Consume the input event
        }
    }
    
    // Or, if you want to save immediately on entering (without key press):
    // Call this from OnBodyEntered if you don't want a key press.
    public void AttemptSave()
    {
        if (_saveManager == null)
        {
            GD.PrintErr("SavePoint: Cannot save, SaveManager is not available.");
            return;
        }

        if (_playerNode == null)
        {
            GD.PrintErr("SavePoint: Cannot save, player reference is missing.");
            // This might happen if saving immediately on enter and player leaves quickly,
            // or if player detection logic needs refinement.
            return;
        }

        if (GetTree() == null || GetTree().CurrentScene == null)
        {
            GD.PrintErr("SavePoint: Scene tree or current scene is not available for saving.");
            return;
        }

        string currentScenePath = GetTree().CurrentScene.SceneFilePath;
        if (string.IsNullOrEmpty(currentScenePath))
        {
            GD.PrintErr("SavePoint: Current scene path is empty, cannot determine level context for saving.");
            return; 
        }

        Vector2 playerPosition = _playerNode.GlobalPosition; // Get player's current global position

        GD.Print($"SavePoint: Attempting to save game. Player at {playerPosition}, Level: {currentScenePath}");
        _saveManager.SaveGame(playerPosition, currentScenePath);
        
        // Optional: Play a sound, show a "Game Saved!" message, or an animation.
        GD.Print("Game save initiated by SavePoint.");
    }
}