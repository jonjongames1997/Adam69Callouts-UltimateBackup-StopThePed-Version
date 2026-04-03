using Newtonsoft.Json;

namespace Adam69Callouts.Stuff
{
    public static class Localization
    {
        private static Dictionary<string, Dictionary<string, string>> _translations;
        public static string CurrentLanguage { get; set; }

        public static void Load(string path)
        {
            var json = File.ReadAllText(path);
            _translations = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(json);
        }

        public static string Translate(string text)
        {
            if (_translations != null &&
                _translations.TryGetValue(CurrentLanguage, out var langDict) &&
                langDict.TryGetValue(text, out var value))
            {
                return value;
            }
            return text; // fallback to key if not found
        }
    }
}
