using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;
using System.Linq;
using DoteTracker;

namespace EmoteLog.Data
{
    public class EmoteQueue : IDisposable
    {
        public LinkedList<EmoteEntry> Log { get; set; }
        public LinkedList<CollapsedEmoteEntry> CollapsedLog { get; set; }
        private Plugin Plugin { get; }

        public EmoteQueue(Plugin plugin)
        {
            this.Plugin = plugin;
            this.Log = new LinkedList<EmoteEntry>();
            this.CollapsedLog = new LinkedList<CollapsedEmoteEntry>();
            this.Plugin.EmoteReaderHooks.OnEmote += OnEmote;
        }

        private void OnEmote(IPlayerCharacter playerCharacter, ushort emoteId, bool IsIncoming)
        {
            //Plugin.PluginLog.Information($"Player {playerCharacter.Name} used emote {emoteId} ({(IsIncoming ? "incoming" : "outgoing")})");

            if (emoteId == 146 || emoteId == 147)
            {
                if (this.Log.Count > 0)
                {
                    while (this.Plugin.Configuration.LogSize <= this.Log.Count)
                    {
                        Dequeue();
                    }
                }

                EmoteEntry emoteEntry = new(playerCharacter.Name.ToString(), playerCharacter.HomeWorld.RowId, emoteId);
                this.Log.AddFirst(emoteEntry);
                if (this.CollapsedLog.Count == 0 || !this.CollapsedLog.First().EmoteEntry.Equals(emoteEntry))
                {
                    this.CollapsedLog.AddFirst(new CollapsedEmoteEntry(1, emoteEntry));
                }
                else
                {
                    CollapsedEmoteEntry collapsedEmoteEntry = this.CollapsedLog.First();
                    collapsedEmoteEntry.Count++;
                    collapsedEmoteEntry.EmoteEntry = emoteEntry;
                }

                

                if (IsIncoming)
                {
                    UpdateState(playerCharacter.Name.ToString(), isOutgoing: false);
                    return;
                }
                else
                {
                    UpdateState(playerCharacter.Name.ToString(), isOutgoing: true);
                    return;
                }
            }

        }

        public readonly Dictionary<string, DoteState> DoteRoster =
        new(StringComparer.OrdinalIgnoreCase);
        private void UpdateState(string name, bool isOutgoing)
        {
            var normalizedName = DoteTrackerState.NormalizeName(name);
            
            Plugin.DoteState.DoteRoster.TryGetValue(normalizedName, out var current);

            if (current != DoteState.Mutual)
            {
                if (isOutgoing)
                {
                    if (current == DoteState.TheyDotedMe)
                    {
                        Plugin.DoteState.DoteRoster[normalizedName] = DoteState.Mutual;
                    }
                    else
                    {
                        Plugin.DoteState.DoteRoster[normalizedName] = DoteState.DotedThem;
                    }
                }
                else
                {
                    if (current == DoteState.DotedThem)
                    {
                        Plugin.DoteState.DoteRoster[normalizedName] = DoteState.Mutual;
                    }
                    else
                    {
                        Plugin.DoteState.DoteRoster[normalizedName] = DoteState.TheyDotedMe;
                    }
                }
            }
        }



        private void Dequeue()
        {
            this.Log.RemoveLast();
            CollapsedEmoteEntry collapsedEntry = this.CollapsedLog.Last();
            collapsedEntry.Count--;
            if (collapsedEntry.Count == 0)
            {
                this.CollapsedLog.RemoveLast();
            }
        }

        public void Dispose()
        {
            this.Plugin.EmoteReaderHooks.OnEmote -= OnEmote;
            Clear();
        }

        public void Clear()
        {
            Log.Clear();
            CollapsedLog.Clear();
        }
    }
}
