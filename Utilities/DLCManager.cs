using Rage;
using Rage.Native;
using System.Collections.Generic;

namespace Adam69Callouts.Utilities
{
    public static class DLCManager
    {
        // List DLCs the callout pack requires (by internal name)
        private static readonly List<string> RequiredDLCs = new List<string>
        {
            "mp2025_01", // Money Fronts
            "patch2025_01", // The Michael DeSanta DLC
            "patch2025_02" // The Michael DeSanta DLC
        };

        public static bool AreRequiredDLCsInstalled()
        {
            foreach(var dlc in RequiredDLCs)
            {
                uint dlcHash = Game.GetHashKey(dlc);
                bool isPresent = NativeFunction.CallByName<bool>("IS_DLC_PRESENT", dlcHash);

                if (!isPresent)
                {
                    Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~r~DLC Missing", "~w~Adam69 Callouts", $"Required DLC '{dlc}' is not installed. Some callouts may not function properly.");
                    Game.LogTrivial($"[Adam69Callouts] Required DLC '{dlc}' is not installed.");
                    LoggingManager.Log("[Adam69Callouts] Required DLC '" + dlc + "' is not installed.");
                    return false;
                }
            }

            return true;
        }

        public static string GetMissingDLC()
        {
            foreach (var dlc in RequiredDLCs)
            {
                uint dlcHash = Game.GetHashKey(dlc);
                bool isPresent = NativeFunction.CallByName<bool>("IS_DLC_PRESENT", dlcHash);
                if (!isPresent) return dlc;
            }

            return null;
        }

        public static bool IsDLCInstalled(string dlcName)
        {
            uint dlcHash = Game.GetHashKey(dlcName);
            return NativeFunction.CallByName<bool>("IS_DLC_PRESENT", dlcHash);
        }
    }
}
