using Godot;
using System; // Required for Exception

// Ensure the SaveData class is NOT defined in this file.
// It should be in its own SaveData.cs file.

public partial class SaveManager : Node
{
    private const string SaveFilePath = "user://savegame.json";

    public void SaveGame(Vector2 playerPosition, string currentLevelScenePath)
    {
        var saveData = new SaveData(playerPosition, currentLevelScenePath); // Uses SaveData from SaveData.cs
        string jsonString = saveData.ToJson();

        try
        {
            using var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                GD.PrintErr($"Failed to open file for writing: {FileAccess.GetOpenError()}");
                return;
            }
            file.StoreString(jsonString);
            GD.Print("Game saved successfully to: ", ProjectSettings.GlobalizePath(SaveFilePath));
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving game: {e.Message}");
        }
    }

    public SaveData LoadGame()
    {
        string globalPath = ProjectSettings.GlobalizePath(SaveFilePath);
        if (!FileAccess.FileExists(SaveFilePath)) // Use original path for FileExists
        {
            GD.Print("No save file found at: ", globalPath);
            return null;
        }

        try
        {
            using var file = FileAccess.Open(SaveFilePath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                 GD.PrintErr($"Failed to open file for reading {globalPath}: {FileAccess.GetOpenError()}");
                return null;
            }
            string jsonString = file.GetAsText();
            
            SaveData loadedData = SaveData.FromJson(jsonString); // Uses SaveData from SaveData.cs
            if (loadedData != null)
            {
                GD.Print("Game loaded successfully from: ", globalPath);
            }
            else
            {
                GD.PrintErr("Failed to parse save data from: ", globalPath);
            }
            return loadedData;
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error loading game from {globalPath}: {e.Message}");
            return null;
        }
    }

    public void OnPlayerNeedsSave(PlayerCharacter player) 
    {
        if (player == null || !IsInstanceValid(player)) 
        {
            GD.PrintErr("Player instance is null or invalid for saving.");
            return;
        }
        if (GetTree() == null || GetTree().CurrentScene == null)
        {
            GD.PrintErr("Scene tree or current scene is not available for saving.");
            return;
        }
        string currentScenePath = GetTree().CurrentScene.SceneFilePath;
        if (string.IsNullOrEmpty(currentScenePath))
        {
            GD.PrintErr("Current scene path is empty, cannot save level context.");
            // Optionally, save with a default/empty scene path or prevent saving
            // return; 
        }
        SaveGame(player.GlobalPosition, currentScenePath);
    }

    public void ApplyLoadedData(SaveData data)
    {
        if (data == null)
        {
            GD.PrintErr("Cannot apply loaded data: data is null.");
            return;
        }

        if (string.IsNullOrEmpty(data.CurrentLevelScenePath))
        {
            GD.PrintErr("Loaded data contains an empty scene path. Cannot change scene.");
            return;
        }
        
        GD.Print($"Attempting to load level: {data.CurrentLevelScenePath}");
        Error err = GetTree().ChangeSceneToFile(data.CurrentLevelScenePath);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load level '{data.CurrentLevelScenePath}': {err}");
            return;
        }

        // Deferred call to set player position after scene is loaded
        Callable.From(() => OnSceneLoadedSetPlayerPosition(data.PlayerPosition)).CallDeferred();
    }
    
    private void OnSceneLoadedSetPlayerPosition(Vector2 position)
    {
        if (GetTree().CurrentScene == null)
        {
            GD.PrintErr("Current scene is null after scene change. Cannot set player position.");
            return;
        }
        // Attempt to find player node. This might need adjustment based on your scene structure.
        // Common names: "Player", "Character". Adjust type if not Node2D (e.g. CharacterBody2D).
        var player = GetTree().CurrentScene.GetNode<Node2D>("Player"); 
        if (player != null && IsInstanceValid(player))
        {
            player.GlobalPosition = position;
            GD.Print($"Player position set to: {position} in scene {GetTree().CurrentScene.SceneFilePath}");
        }
        else
        {
            GD.PrintErr("Could not find player node named 'Player' (type Node2D) in the loaded scene to set position.");
            GD.Print("Ensure your player node is named 'Player' and is a direct child of the scene root, or adjust the GetNode path.");
        }
    }
}

// Dummy PlayerCharacter class for the example. 
// You should replace this with your actual player class if it's different.
// If your player class is already defined elsewhere, you can remove this.
public partial class PlayerCharacter : CharacterBody2D
{
    // This is just a placeholder. Your actual player script would be more complex.
}