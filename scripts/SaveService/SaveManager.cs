using Godot;
using System;

public partial class SaveManager : Node
{
    private const string SaveFilePath = "user://savegame.json";
    private bool _isApplyingLoadedData = false;
    private Vector2 _pendingPlayerPosition;
    
    private const string DefaultStartingLevel = "res://scenes/level_1.tscn";
    private readonly Vector2 DefaultStartingPosition = new Vector2(-64, 2);

    public void StartNewGame()
    {
        if (_isApplyingLoadedData)
        {
            GD.Print("Game loading already in progress, ignoring new game request.");
            return;
        }

        GD.Print($"Starting new game at: {DefaultStartingLevel}");
        
        _isApplyingLoadedData = true;
        _pendingPlayerPosition = DefaultStartingPosition;
        
        Error err = GetTree().ChangeSceneToFile(DefaultStartingLevel);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to load starting level '{DefaultStartingLevel}': {err}");
            _isApplyingLoadedData = false;
            return;
        }

        StartPlayerPositionSetupRoutine();
    }

    public void ClearSaveFile()
    {
        if (FileAccess.FileExists(SaveFilePath))
        {
            try
            {
                DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SaveFilePath));
                GD.Print("Save file cleared for new game.");
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to clear save file: {e.Message}");
            }
        }
    }

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

        StartPlayerPositionSetupRoutine();
    }
    
    private async void StartPlayerPositionSetupRoutine()
    {
        for (int attempts = 0; attempts < 60; attempts++)
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
            
            if (GetTree().CurrentScene != null)
            {
                bool success = TrySetPlayerPosition(_pendingPlayerPosition);
                if (success)
                {
                    GD.Print("Successfully set player position after scene load.");
                    break;
                }
                else if (attempts > 10)
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
        
        Node player = null;
        string[] playerPaths = { 
            "Player",
            "*/Player",
            "**/Player"
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
        if (parent.IsInGroup("Player"))
        {
            return parent;
        }
        
        if (parent is CharacterBody2D body && (
            parent.Name.ToString().ToLower().Contains("player") ||
            parent.HasMethod("_physics_process")
        ))
        {
            return parent;
        }
        
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
}

