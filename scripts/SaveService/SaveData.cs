using Godot;

public partial class SaveData : GodotObject
{
    public Vector2 PlayerPosition { get; set; }
    public string CurrentLevelScenePath { get; set; }

    public SaveData()
    {
        PlayerPosition = Vector2.Zero;
        CurrentLevelScenePath = string.Empty;
    }

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

        var saveData = new SaveData();

        if (data.ContainsKey("PlayerPositionX") && data.ContainsKey("PlayerPositionY"))
        {
            float x = (float)data["PlayerPositionX"];
            float y = (float)data["PlayerPositionY"];
            saveData.PlayerPosition = new Vector2(x, y);
        }

        if (data.ContainsKey("CurrentLevelScenePath"))
        {
            saveData.CurrentLevelScenePath = (string)data["CurrentLevelScenePath"];
        }

        return saveData;
    }
}

