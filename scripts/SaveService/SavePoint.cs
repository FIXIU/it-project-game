using Godot;

public partial class SavePoint : Area2D
{
    private SaveManager _saveManager;
    private bool _playerInRange = false;
    private Node2D _playerNode = null;

    public override void _Ready()
    {
        _saveManager = GetNode<SaveManager>("/root/SaveManager");
        if (_saveManager == null)
        {
            GD.PrintErr("SavePoint: SaveManager not found. Make sure it's autoloaded.");
        }

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body.IsInGroup("Player") || body is Player)
        {
            GD.Print("Player entered save point area.");
            _playerInRange = true;
            _playerNode = body;
        }
    }

    private void OnBodyExited(Node2D body)
    {
        if (body.IsInGroup("Player") || body is Player)
        {
            GD.Print("Player exited save point area.");
            _playerInRange = false;
            _playerNode = null;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_playerInRange && @event.IsActionPressed("ui_accept"))
        {
            AttemptSave();
        }
    }

    private void AttemptSave()
    {
        if (_saveManager == null || _playerNode == null)
        {
            GD.PrintErr("Cannot save: SaveManager or player reference is null.");
            return;
        }

        string currentScenePath = GetTree().CurrentScene.SceneFilePath;
        if (string.IsNullOrEmpty(currentScenePath))
        {
            GD.PrintErr("Cannot save: Current scene path is empty.");
            return;
        }

        _saveManager.SaveGame(_playerNode.GlobalPosition, currentScenePath);
        
        GD.Print("Game saved successfully!");
        
        DisplaySavedMessage();
    }

    private void DisplaySavedMessage()
    {
        var label = GetNode<Label>("SavedLabel");
        if (label != null)
        {
            label.Visible = true;
            var timer = GetTree().CreateTimer(2.0f);
            timer.Timeout += () => label.Visible = false;
        }
    }
}

