using IPA.Utilities;
using Newtonsoft.Json;
using System;
using System.IO;

namespace SaberTailor.Settings.Utilities
{
    class FileHandler
    {
        internal static readonly string filePath = UnityGame.UserDataPath;

        // Load current config file, return true with loaded config if succesful, return false with default config if failed
        internal static bool LoadConfig(out PluginConfig config, string fileName = "SaberTailor.json")
        {
            string configPath = Path.Combine(filePath, fileName);
            if (File.Exists(configPath))
            {   
                try
                {
                    config = JsonConvert.DeserializeObject<PluginConfig>(File.ReadAllText(configPath));
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.log.Warn(ex);
                    Logger.log.Warn("Unable to load config from file. Creating new default configuration.");
                    config = new PluginConfig();
                    return false;
                }
            }
            else
            {
                Logger.log.Debug("Configuration file not found. Creating new default configuration.");
                config = new PluginConfig();
                return false;
            }
        }

        // Save config to specific file (current config file as default)
        internal static bool SaveConfig(PluginConfig config, string fileName = "SaberTailor.json")
        {
            bool saveSuccessful = true;
            string configPath = Path.Combine(filePath, fileName);
            try
            {
                File.WriteAllText(configPath, JsonConvert.SerializeObject(config, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Logger.log.Error(ex);
                Logger.log.Error("Unable to save configuration ");
                saveSuccessful = false;
            }

            return saveSuccessful;
        }
    }
}
