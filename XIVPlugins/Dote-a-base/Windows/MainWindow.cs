using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using DoteTracker.Utils;
using System.Linq;
using System.Numerics;

namespace DoteTracker.Windows;

public sealed class MainWindow : Window
{
    private readonly Plugin plugin;

    // Colour palette for DoteState indicators
    private static readonly Vector4 ColMutual      = new(0.30f, 0.50f, 0.90f, 1.0f); // Blue
    private static readonly Vector4 ColDotedThem   = new(0.20f, 0.90f, 0.20f, 1.0f); // Green
    private static readonly Vector4 ColTheyDotedMe = new(1.00f, 0.35f, 0.35f, 1.0f); // Red
    private static readonly Vector4 ColNone        = new(1.00f, 1.00f, 1.00f, 1.0f); // White

    public MainWindow(Plugin plugin)
        : base("Dote-a-base ##DoteTrackerMain",
               ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.plugin = plugin;
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(425, 260),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public void Dispose() { }

    public override void Draw()
    {
        var localPlayer = Plugin.ObjectTable.LocalPlayer;
        if (localPlayer == null)
        {
            ImGui.TextUnformatted("Waiting for game world…");
            return;
        }

        DrawTopBar();
        DrawLegend();
        ImGui.Separator();
        DrawNearbyTable(localPlayer);
        //DrawRosterSection();
    }

    // -------------------------------------------------------------------------
    // Sub-sections
    // -------------------------------------------------------------------------

    private void DrawTopBar()
    {
        if (ImGui.Button("Reset List"))
            plugin.DoteState.Clear();

        ImGui.SameLine();
        ImGui.SetNextItemWidth(260);
        var dist = plugin.Configuration.ScanDistance;
        if (ImGui.SliderFloat("Scan Range", ref dist, 5.0f, 100.0f, "%.0f yalms"))
        {
            plugin.Configuration.ScanDistance = dist;
            plugin.Configuration.Save();
        }
    }

    private void DrawLegend()
    {
        var showMutual = plugin.Configuration.ShowMutual;
        var showDotedThem = plugin.Configuration.ShowDotedThem;
        var showDotedMe = plugin.Configuration.ShowDotedMe;
        var showNone = plugin.Configuration.ShowNone;

        ImGui.Spacing();
        ImGui.PushStyleColor(ImGuiCol.Text, ColMutual);
        //if (ImGui.Checkbox($"■ It's mutual", ref showMutual))
        if (ImGui.Checkbox($"It's mutual", ref showMutual))
        {
            plugin.Configuration.ShowMutual = showMutual;
            plugin.Configuration.Save();
        }
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ColDotedThem);
        //if (ImGui.Checkbox($"■ I Doted them", ref showDotedThem))
        if (ImGui.Checkbox($"I Doted them", ref showDotedThem))
        {
            plugin.Configuration.ShowDotedThem = showDotedThem;
            plugin.Configuration.Save();
        }
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ColTheyDotedMe);
        //if (ImGui.Checkbox($"■ They Doted me", ref showDotedMe))
        if (ImGui.Checkbox($"They Doted me", ref showDotedMe))
        {
            plugin.Configuration.ShowDotedMe = showDotedMe;
            plugin.Configuration.Save();
        }
        ImGui.PopStyleColor();
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Text, ColNone);
        //if (ImGui.Checkbox($"■ None", ref showNone))
        if (ImGui.Checkbox($"None", ref showNone))
        {
            plugin.Configuration.ShowNone = showNone;
            plugin.Configuration.Save();
        }
        ImGui.PopStyleColor();
        ImGui.Spacing();
    }

    private void DrawNearbyTable(IPlayerCharacter localPlayer)
    {
        //ImGui.TextUnformatted("Nearby Players");

        var availY     = ImGui.GetContentRegionAvail().Y;
        var tableHeight = availY;

        if (!ImGui.BeginTable("##NearbyPlayers", 2,
                ImGuiTableFlags.Borders             |
                ImGuiTableFlags.RowBg               |
                ImGuiTableFlags.ScrollY             |
                ImGuiTableFlags.SizingStretchProp,
                new Vector2(0, tableHeight)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn("Player Name", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoHide);
        //ImGui.TableSetupColumn("Status",      ImGuiTableColumnFlags.WidthFixed, 100);
        ImGui.TableSetupColumn(" ",           ImGuiTableColumnFlags.WidthFixed,  100);
        //ImGui.TableSetupColumn(" ",           ImGuiTableColumnFlags.WidthFixed, 50);
        ImGui.TableHeadersRow();

        foreach (var obj in Plugin.ObjectTable.OrderBy(o => o.Name.TextValue))
        {
            if (obj is not IPlayerCharacter player) continue;
            if (player.GameObjectId == localPlayer.GameObjectId) continue;
            if (obj.Name.TextValue.Length < 1) continue;

            var distance = Vector3.Distance(localPlayer.Position, player.Position);
            if (distance >= plugin.Configuration.ScanDistance) continue;

            var normalizedName = DoteTrackerState.NormalizeName(player.Name.TextValue);
            plugin.DoteState.DoteRoster.TryGetValue(normalizedName, out var state);

            if ((state == DoteState.None && !plugin.Configuration.ShowNone) ||
                (state == DoteState.Mutual && !plugin.Configuration.ShowMutual) ||
                (state == DoteState.DotedThem && !plugin.Configuration.ShowDotedThem) ||
                (state == DoteState.TheyDotedMe && !plugin.Configuration.ShowDotedMe))
            {
                continue;
            }

            ImGui.TableNextRow();

            // Name (coloured by state)
            ImGui.TableSetColumnIndex(0);
            ImGui.PushStyleColor(ImGuiCol.Text, StateColor(state));
            ImGui.TextUnformatted(player.Name.TextValue);
            ImGui.PopStyleColor();

            // Status label
            //ImGui.TableSetColumnIndex(1);
            //ImGui.TextUnformatted(StateLabel(state));

            // Target button — unique ID via GameObjectId
            ImGui.TableSetColumnIndex(1);
            if (ImGui.Button($" Target ##{player.GameObjectId}"))
                Plugin.TargetManager.Target = player;

            ImGui.SameLine();
            //ImGui.TableSetColumnIndex(2);
            //if (ImGui.Button($"Dote##{player.GameObjectId}"))
            if (ImGui.Button($" ♥ ##{player.GameObjectId}"))
            {
                if (Plugin.TargetManager.Target != player)
                    Plugin.TargetManager.Target = player;
                EmoteUtils.SendDote(146);
            }
        }

        ImGui.EndTable();
    }
    
    private static Vector4 StateColor(DoteState state) => state switch
    {
        DoteState.Mutual      => ColMutual,
        DoteState.DotedThem   => ColDotedThem,
        DoteState.TheyDotedMe => ColTheyDotedMe,
        _                     => ColNone
    };

    private static string StateLabel(DoteState state) => state switch
    {
        DoteState.Mutual      => "Mutual",
        DoteState.DotedThem   => "I Doted",
        DoteState.TheyDotedMe => "They Doted",
        _                     => " "
    };
}
