using Godot;

// Ensure this class is not defined anywhere else
public partial class SaveData : GodotObject
{
    public Vector2 PlayerPosition { get; set; }
    public string CurrentLevelScenePath { get; set; }

    // Default constructor
    public SaveData()
    {
        PlayerPosition = Vector2.Zero;
        CurrentLevelScenePath = string.Empty;
    }

    // Constructor with parameters
    public SaveData(Vector2 playerPosition, string currentLevelScenePath)
    {
        PlayerPosition = playerPosition;
        CurrentLevelScenePath = currentLevelScenePath;
    }

    public string ToJson()
    {
        return Json.Stringify(new Godot.Collections.Dictionary
        {
            { "PlayerPositionX", PlayerPosition.X },
            { "PlayerPositionY", PlayerPosition.Y },
            { "CurrentLevelScenePath", CurrentLevelScenePath }
        });
    }

    public static SaveData FromJson(string jsonString)
    {
        var json = new Json();
        Error err = json.Parse(jsonString);
        if (err != Error.Ok)
        {
            GD.PrintErr("Error parsing save data JSON: ", err.ToString());
            return null;
        }

        var data = json.Data.AsGodotDictionary();
        if (data == null)
        {
            GD.PrintErr("Error: Parsed JSON data is not a dictionary.");
            return null;
        }
        
        // Ensure keys exist before trying to access them or provide defaults
        float posX = data.ContainsKey("PlayerPositionX") ? data["PlayerPositionX"].AsSingle() : 0.0f;
        float posY = data.ContainsKey("PlayerPositionY") ? data["PlayerPositionY"].AsSingle() : 0.0f;
        string scenePath = data.ContainsKey("CurrentLevelScenePath") ? data["CurrentLevelScenePath"].AsString() : string.Empty;

        return new SaveData
        {
            PlayerPosition = new Vector2(posX, posY),
            CurrentLevelScenePath = scenePath
        };
    }
}