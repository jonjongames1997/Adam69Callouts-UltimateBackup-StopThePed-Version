namespace Adam69Callouts.Utilities
{
    internal static class LoggingManager
    {
        private const string LoggingPrefix = "Adam69 Callouts";

        private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins/LSPDFR/Adam69Callouts/Log/Adam69Callouts.log");

        internal enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        internal static void Log(string message, LogLevel level = LogLevel.Info)
        {
            string logMessage = $"{LoggingPrefix} [{level}]: {message}";

            // Log to file
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logMessage}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"{LoggingPrefix} [Error]: Failed to write to log file: {ex.Message}");
            }

            // Log based on level
            switch (level)
            {
                case LogLevel.Debug:
                    if (GlobalsManager.TheApplication.DebugLogging)
                    {
                        Game.LogTrivial(logMessage);
                    }
                    break;

                case LogLevel.Info:
                case LogLevel.Warning:
                case LogLevel.Error:
                    Game.LogTrivial(logMessage);
                    break;
            }
        }

        internal static void Normal(string msg)
        {
            string logMessage = $"[NORMAL] Adam69 Callouts: {msg}";
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {logMessage}{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"{LoggingPrefix} [Error]: Failed to write to log file: {ex.Message}");
            }
            Game.LogTrivial(logMessage);
        }
    }
}
