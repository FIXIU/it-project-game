using Godot;
using System;

public partial class SaveManager : Node
{
    private const string SaveFilePath = "user://savegame.json";
    private bool _isApplyingLoadedData = false;
    private Vector2 _pendingPlayerPosition;

    public void SaveGame(Vector2 playerPosition, string currentLevelScenePath)
    {
        var saveData = new SaveData(playerPosition, currentLevelScenePath);
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
        if (!FileAccess.FileExists(SaveFilePath))
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
            
            SaveData loadedData = SaveData.FromJson(jsonString);
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
            return; 
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

        if (_isApplyingLoadedData)
        {
            GD.Print("Load already in progress, ignoring duplicate request.");
            return;
        }

        if (string.IsNullOrEmpty(data.CurrentLevelScenePath))
        {
            GD.PrintErr("Loaded data contains an empty scene path. Cannot change scene.");
            return;
        }
        
        _isApplyingLoadedData = true;
        _pendingPlayerPosition = data.PlayerPosition;
        
        GD.Print($"Attempting to load level: {data.CurrentLevelScenePath}");
        
        Error err = GetTree().ChangeSceneToFile(data.CurrentLevelScenePath);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load level '{data.CurrentLevelScenePath}': {err}");
            _isApplyingLoadedData = false;
            return;
        }

        // Use a more robust method to wait for scene loading
        StartPlayerPositionSetupRoutine();
    }
    
    private async void StartPlayerPositionSetupRoutine()
    {
        // Wait for multiple frames and keep checking if scene is ready
        for (int attempts = 0; attempts < 60; attempts++) // Max 1 second at 60 FPS
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            if (GetTree().CurrentScene != null)
            {
                // Scene is loaded, now try to set player position
                bool success = TrySetPlayerPosition(_pendingPlayerPosition);
                if (success)
                {
                    GD.Print("Successfully set player position after scene load.");
                    break;
                }
                else if (attempts > 10) // Give it a few more frames for player to initialize
                {
                    GD.PrintErr("Failed to find player node after multiple attempts.");
                    PrintSceneStructure(GetTree().CurrentScene, 0, 3);
                    break;
                }
            }
        }
        
        _isApplyingLoadedData = false;
    }
    
    private bool TrySetPlayerPosition(Vector2 position)
    {
        if (GetTree().CurrentScene == null)
        {
            return false;
        }
        
        // Try multiple possible player node paths
        Node player = null;
        string[] playerPaths = { 
            "Player",           // Direct child named Player
            "*/Player",         // Player one level deep
            "**/Player"         // Player anywhere in the tree
        };
        
        foreach (string path in playerPaths)
        {
            player = GetTree().CurrentScene.GetNodeOrNull(path);
            if (player != null)
            {
                GD.Print($"Found player at path: {path}");
                break;
            }
        }
        
        // If still not found, try searching by type
        if (player == null)
        {
            player = FindPlayerByType(GetTree().CurrentScene);
        }
        
        if (player != null && IsInstanceValid(player))
        {
            if (player is CharacterBody2D characterBody)
            {
                characterBody.GlobalPosition = position;
                GD.Print($"Player (CharacterBody2D) position set to: {position} in scene {GetTree().CurrentScene.SceneFilePath}");
                return true;
            }
            else if (player is Node2D node2D)
            {
                node2D.GlobalPosition = position;
                GD.Print($"Player (Node2D) position set to: {position} in scene {GetTree().CurrentScene.SceneFilePath}");
                return true;
            }
            else
            {
                GD.PrintErr($"Found player node but it's not a Node2D or CharacterBody2D: {player.GetType()}");
            }
        }
        
        return false;
    }
    
    private Node FindPlayerByType(Node parent)
    {
        // Check if this node is a player (in the Player group or has specific script)
        if (parent.IsInGroup("Player"))
        {
            return parent;
        }
        
        // Check if it's a CharacterBody2D with player-like characteristics
        if (parent is CharacterBody2D body && (
            parent.Name.ToString().ToLower().Contains("player") ||
            parent.HasMethod("_physics_process") // Assuming player has physics processing
        ))
        {
            return parent;
        }
        
        // Recursively check children
        foreach (Node child in parent.GetChildren())
        {
            Node result = FindPlayerByType(child);
            if (result != null) return result;
        }
        
        return null;
    }
    
    private void PrintSceneStructure(Node node, int depth, int maxDepth)
    {
        if (depth > maxDepth) return;
        
        string indent = new string(' ', depth * 2);
        string groups = "";
        if (node.GetGroups().Count > 0)
        {
            groups = $" [Groups: {string.Join(", ", node.GetGroups())}]";
        }
        GD.Print($"{indent}{node.Name} ({node.GetType().Name}){groups}");
        
        foreach (Node child in node.GetChildren())
        {
            PrintSceneStructure(child, depth + 1, maxDepth);
        }
    }
}

public partial class PlayerCharacter : CharacterBody2D
{
    // This is just a placeholder. Your actual player script would be more complex.
}