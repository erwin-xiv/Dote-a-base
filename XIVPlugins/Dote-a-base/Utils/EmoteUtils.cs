using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.RaptureHotbarModule;

namespace DoteTracker.Utils
{
    public static class EmoteUtils
    {
        private static unsafe RaptureHotbarModule* RaptureHotbarModule => Framework.Instance()
            ->UIModule
            ->GetRaptureHotbarModule();

        public static unsafe void SendDote(ushort emoteId)
        {
            // 146 is the usual Dote emote, empirically confirmed.
            RaptureHotbarModule->ScratchSlot.Set(HotbarSlotType.Emote, emoteId);
            RaptureHotbarModule->ExecuteSlot(&RaptureHotbarModule->ScratchSlot);
            RaptureHotbarModule->ScratchSlot.Set(HotbarSlotType.Empty, 0);
        }

    }
}
