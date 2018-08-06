using System.Threading;
using Umbraco.Core.Configuration;
using Wr.UmbEpubReader.Helpers;

namespace Wr.UmbEpubReader.Extensions
{
    public static class UmbracoConfigExtensions
    {
        private static AppSettingsConfig _config;

        /// <summary>
        /// Gets configuration for Epub Reader.
        /// </summary>
        /// <param name="umbracoConfig">The umbraco configuration.</param>
        /// <returns>Epub Reader configuration.</returns>
        /// <remarks> 
        /// Getting the configuration freezes its state, and 
        /// any attempt at modifying the configuration will be ignored. 
        /// </remarks>
        public static AppSettingsConfig UmbEpubReader(this UmbracoConfig umbracoConfig)
        {
            LazyInitializer.EnsureInitialized(ref _config, () => AppSettingsConfig.Value);
            return _config;
        }

        // Internal for tests
        internal static void ResetConfig()
        {
            _config = null;
        }
    }
}