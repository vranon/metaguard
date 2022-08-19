using IPA.Utilities;
using SaberTailor.Settings.Classes;
using SaberTailor.Settings.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SaberTailor.Settings.Utilities
{
    class ProfileManager
    {
        internal static bool profilesLoaded = false;
        internal static bool profilesPresent = false;
        internal static List<object> profileNames;

        internal static void LoadProfiles()
        {
            profileNames = new List<object>();

            string[] fileNames = Directory.GetFiles(UnityGame.UserDataPath, @"SaberTailor.*.json");
            foreach (string fileName in fileNames)
            {
                Regex r = new Regex(@"^SaberTailor\.([a-zA-Z0-9_-]+)\.json$");
                Match m = r.Match(Path.GetFileName(fileName));
                if (m.Success)
                {
                    profileNames.Add(m.Groups[1].Value);
                }
            }

            if (profileNames.Count > 0)
            {
                profilesPresent = true;
            }
            else
            {
                profilesPresent = false;
                profileNames.Add("NONE AVAILABLE");
            }

            profilesLoaded = true;
        }

        internal static bool DeleteProfile(string profileName, out string statusMsg)
        {
            string fileName = @"SaberTailor." + profileName + @".json";
            string filePath = Path.Combine(UnityGame.UserDataPath, fileName);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    statusMsg = "Profile '" + profileName + "' deleted successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    Logger.log.Warn(ex);
                    Logger.log.Warn("Unable to delete profile '" + profileName + "'.");
                    statusMsg = "Unable to delete '" + profileName + "'. Please check the logs.";
                    return false;
                }
            }
            else
            {
                Logger.log.Debug("File not found. Unable to delete profile '" + profileName + "'.");
                statusMsg = "Profile '" + profileName + "' not found. Delete failed.";
                return false;
            }
        }

        internal static bool LoadProfile(string profileName, out string statusMsg)
        {
            string fileName = @"SaberTailor." + profileName + @".json";
            bool loadSuccessful = FileHandler.LoadConfig(out PluginConfig config, fileName);
            if (loadSuccessful)
            {
                PluginConfig.Instance = config;
                Configuration.Load();
                statusMsg = "Profile '" + profileName + "' loaded successfully.";
                return true;
            }
            else
            {
                statusMsg = "Profile loading failed. Please check logs files.";
                return false;
            }
        }

        internal static bool SaveProfile(string profileName, out string statusMsg)
        {
            string fileName = @"SaberTailor." + profileName + @".json";

            //Save the active configuration to a new object
            PluginConfig config = new PluginConfig();
            Configuration.SaveConfig(ref config);

            bool saveSuccessful = FileHandler.SaveConfig(config, fileName);

            if (saveSuccessful)
            {
                statusMsg = "Profile '" + profileName + "' saved successfully.";
                return true;
            }
            else
            {
                statusMsg = "Error saving profile. Please check the log files.";
                return false;
            }
        }

        internal static void PrintProfileNames()
        {
            Logger.log.Debug("Printing list of Profile names:");
            if (!profilesPresent)
            {
                Logger.log.Debug("No profiles present.");
                return;
            }
            Logger.log.Debug(String.Join("; ", profileNames));
        }
    }
}
