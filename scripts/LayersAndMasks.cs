using Godot;
using System;

public partial class LayersAndMasks : Node
{
    public uint GetCollisionLayerByName(string layerName)
    {
        for(uint i = 1; i <= 32; ++i)
        {
            var layer = ProjectSettings.GetSetting("layer_names/2d_physics/layer_" + i).ToString();
            if(layer.Equals(layerName))
            {
                return i;
            }
        }
        GD.PrintErr("Could not find the " + layerName + " collision layer.");
        GD.PrintErr("Make sure to set the name: " + layerName + " under 'project settings -> layer names' for the collision layer");
        return 0;
    }
}
