using Rage.Attributes;
using Rage.ConsoleCommands.AutoCompleters;

namespace Adam69Callouts.Stuff
{
    internal static class ConsoleCommands
    {
        // Credit to Rich for the original code for this command. Source: https://www.lcpdfr.com/forums/topic/148138-lspdfr-plugin-visual-studio-project-template/#comment-797893

        [ConsoleCommand("EndCurrentAdam69Callout", Description = "End the current callout from Adam69 Callouts")]

        internal static void Command_EndCurrentAdam69CalloutsCallout([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean))] bool endCallout = true)
        {
            if (!endCallout)
            {
                return;
            }

            // Check if a callout is currently running or not
            if (!Functions.IsCalloutRunning())
            {
                Game.DisplayNotification("No Adam69 callout is currently running at the moment.");
                return;
            }

            Functions.StopCurrentCallout(); // Stops the current callout
        }

        // This command reloads the configuration file for the plugin
        [ConsoleCommand("ReloadAdam69CalloutsConfig", Description = "Reloads the Adam69 Callouts configuration file")]

        internal static void Command_ReloadAdam69CalloutsConfig([ConsoleCommandParameter(AutoCompleterType = typeof(ConsoleCommandAutoCompleterBoolean))] bool reloadConfig = true)
        {
            if (!reloadConfig)
            {
                return;
            }
            // Reload the configuration file
            Settings.LoadSettings();
            Game.DisplayNotification("~g~Adam69Callouts~s~: Configuration reloaded successfully.");
        }
    }
}
