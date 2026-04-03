using System.Net.Http;

namespace Adam69Callouts.VersionChecker
{
    public class PluginCheck
    {
        private static readonly HttpClient httpClient = new();

        public static async Task<bool> IsUpdateAvailableAsync()
        {
            string curVersion = Settings.PluginVersion;
            Uri latestVersionUri = new("https://www.lcpdfr.com/applications/downloadsng/interface/api.php?do=checkForUpdates&fileId=49465&textOnly=1");
            string receiveData;

            try
            {
                receiveData = await httpClient.GetStringAsync(latestVersionUri);
            }
            catch (HttpRequestException ex)
            {
                HandleUpdateCheckFailure(ex);
                return false;
            }

            if (receiveData != curVersion)
            {
                NotifyUpdateAvailable(curVersion, receiveData);
                return true;
            }
            else
            {
                NotifyLatestVersion();
                return false;
            }
        }

        private static void HandleUpdateCheckFailure(HttpRequestException ex)
        {
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~Adam69 Callouts Warning", "~r~Failed to check for an update", "Please make sure you're ~y~connected~w~ to your WiFi Network or try to reload the plugin");
            Game.Console.Print();
            Game.Console.Print("===================================================== Adam69 Callouts ===========================================");
            Game.Console.Print();
            Game.Console.Print("[WARNING!]: Failed to check for an update!");
            Game.Console.Print("[LOG]: Please make sure you are connected to the internet or try to reload the plugin.");
            Game.Console.Print("[ERROR]: " + ex.Message);
            Game.Console.Print();
            Game.Console.Print("==================================================== Adam69 Callouts ============================================");
            Game.Console.Print();
        }

        private static void NotifyUpdateAvailable(string curVersion, string newVersion)
        {
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~Adam69 Callouts Warning", "~y~A new update is available!", $"Current Version: ~r~{curVersion}~w~<br>New Version: ~y~{newVersion}<br>~w~Please Update to the latest build for new ~p~callouts~w~ and ~g~improvements~w~!:-)");
            Game.DisplayNotification("commonmenu", "mp_alerttriangle", "~w~Adam69 Callouts Deprecation Warning", "~y~Adam69Callouts Deprecation Info:", "v0.3.3.3 or lower is ~r~NO LONGER supported~w~ and ~r~deprecated~w~. Update to latest build for guaranteed support.");
            Game.Console.Print();
            Game.Console.Print("===================================================== Adam69 Callouts ===========================================");
            Game.Console.Print();
            Game.Console.Print("[WARNING!]: A new version of Adam69 Callouts is NOW AVAILABLE to download! Update to latest build!");
            Game.Console.Print("[WARNING!]: v0.4.5.0 or lower is NO LONGER Supported! Update to latest build for guaranteed suppport");
            Game.Console.Print($"[LOG]: Current Version: {curVersion}");
            Game.Console.Print($"[LOG]: New Version: {newVersion}");
            Game.Console.Print();
            Game.Console.Print("===================================================== Adam69 Callouts ===========================================");
            Game.Console.Print();
        }

        private static void NotifyLatestVersion()
        {
            Game.DisplayNotification("web_adam69callouts", "web_adam69callouts", "~w~Adam69 Callouts", "", "Detected the ~g~latest~w~ build of ~o~Adam69 Callouts~w~! Thank you for downloading! :-)");
        }
    }
}
