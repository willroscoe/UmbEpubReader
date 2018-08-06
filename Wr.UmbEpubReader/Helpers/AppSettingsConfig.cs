using System.Configuration;

namespace Wr.UmbEpubReader.Helpers
{
    /// <summary>
    /// Configuration for app settings from the web.config
    /// </summary>
    public class AppSettingsConfig
    {
        private static AppSettingsConfig _value;

        public string BooksPathSegment { get; }
        public string ReadPathSegment { get; }

        /// <summary>
        /// Setup AppSettings for the app
        /// with details from configuration.
        /// </summary>
        private AppSettingsConfig()
        {
            BooksPathSegment = GetConfigStringValue("UmbEpubReader.BooksPathSegment", "books");
            ReadPathSegment = GetConfigStringValue("UmbEpubReader.ReadPathSegment", "read");
        }

        /// <summary>
        /// Initializes a new instance with details passed as parameters.
        /// </summary>
        public AppSettingsConfig(string booksPathSegment = "books", string readPathSegment = "read")
        {
            BooksPathSegment = booksPathSegment;
            ReadPathSegment = readPathSegment;
        }

        private static bool GetConfigBoolValue(string key, bool defaultValue)
        {
            return bool.TryParse(GetConfigStringValue(key), out bool value) ? value : defaultValue;
        }

        private static int GetConfigIntValue(string key, int defaultValue)
        {
            return int.TryParse(GetConfigStringValue(key), out int value) ? value : defaultValue;
        }

        private static string GetConfigStringValue(string key, string defaultValue = "")
        {
            var value = ConfigurationManager.AppSettings[key];
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        internal static AppSettingsConfig Value => _value ?? new AppSettingsConfig();

        public static void Setup(AppSettingsConfig config)
        {
            _value = config;
        }
    }
}