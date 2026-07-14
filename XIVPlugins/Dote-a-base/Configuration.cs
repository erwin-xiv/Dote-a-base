using Dalamud.Configuration;

namespace DoteTracker;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int LogSize { get; set; } = 100;

    /// <summary>Whether the main window was open when the plugin was last unloaded.</summary>
    public bool ShowUI { get; set; } = false;

    /// <summary>Maximum distance (in yalms) to include a player in the nearby list.</summary>
    public float ScanDistance { get; set; } = 50.0f;

    /// <summary>Whether to show players with no dotes in the list.</summary>
    public bool ShowNone { get; set; } = false;

    /// <summary>Whether to show mutual dotes in the list.</summary>
    public bool ShowMutual { get; set; } = true;

    /// <summary>Whether to show dotes from others in the list.</summary>
    public bool ShowDotedThem { get; set; } = true;

    /// <summary>Whether to show dotes to others in the list.</summary>
    public bool ShowDotedMe { get; set; } = true;

    public void Save() => Plugin.PluginInterface.SavePluginConfig(this);
}
