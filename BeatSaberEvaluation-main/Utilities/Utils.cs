using IPA.Loader;
using IPA.Old;

namespace SaberTailor.Utilities
{
    public class Utils
    {
        /// <summary>
        /// Check if a BSIPA plugin is enabled
        /// </summary>
        public static bool IsPluginEnabled(string PluginName)
        {
            if (IsPluginPresent(PluginName))
            {
                PluginMetadata metadata = PluginManager.GetPlugin(PluginName);
                if (metadata == null)
                {
                    metadata = PluginManager.GetPluginFromId(PluginName);
                }

                if (metadata != null)
                {
                    return PluginManager.IsEnabled(metadata);
                }
            }

            return false;
        }

        /// <summary>
        /// Check if a plugin exists
        /// </summary>
        public static bool IsPluginPresent(string PluginName)
        {
            // Check in BSIPA
            if (PluginManager.GetPlugin(PluginName) != null ||
                PluginManager.GetPluginFromId(PluginName) != null)
            {
                return true;
            }

#pragma warning disable CS0618 // IPA is obsolete
            // Check in old IPA
            foreach (IPlugin plugin in PluginManager.Plugins)
            {
                if (plugin.Name == PluginName)
                {
                    return true;
                }
            }
#pragma warning restore CS0618 // IPA is obsolete

            return false;
        }
    }
}
