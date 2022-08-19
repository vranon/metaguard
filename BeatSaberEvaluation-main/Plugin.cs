using IPA;
using IPA.Config;
using IPA.Loader;
using SaberTailor.HarmonyPatches;
using SaberTailor.Settings;
using SaberTailor.Settings.UI;
using SaberTailor.Tweaks;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace SaberTailor
{
    public static class BuildInfo
    {
        public const string Name = "SaberTailor";
        public const string Version = "3.5.0";
    }

    [Plugin(RuntimeOptions.DynamicInit)]
    public class Plugin
    {
        public static string PluginName => BuildInfo.Name;
        public static Hive.Versioning.Version PluginVersion { get; private set; } = new Hive.Versioning.Version("0.0.0"); // Default

        public static bool IsBSMLAvailable => CheckPluginAvailable("BeatSaberMarkupLanguage", new Hive.Versioning.Version("1.5.0"));
        public static bool IsBSUtilsAvailable => CheckPluginAvailable("BS Utils", new Hive.Versioning.Version("1.10.0"));


        [Init]
        public void Init(IPALogger logger, PluginMetadata metadata)
        {
            Logger.log = logger;
            Configuration.Init();

            if (metadata?.HVersion != null)
            {
                PluginVersion = metadata.HVersion;
            }
        }

        [OnEnable]
        public void OnEnable() => Load();
        [OnDisable]
        public void OnDisable() => Unload();

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
            if (nextScene.name == "GameCore")
            {
                if (Configuration.Scale.TweakEnabled)
                {
                    new GameObject(PluginName).AddComponent<SaberLength>();
                }
            }
        }

        private void Load()
        {
            Configuration.Load();
            Settings.Utilities.ProfileManager.LoadProfiles();

            AddEvents();
            if (IsBSMLAvailable)
            {
                AddMenu();
            }
            else
            {
                Logger.log.Debug("BSML is missing. Skipping setting up BSML settings menu...");
            }
            Logger.log.Info($"{PluginName} v.{PluginVersion} has started.");
        }

        private void Unload()
        {
            SaberTailorPatches.RemoveHarmonyPatches();
            Configuration.Save();
            RemoveEvents();

            if (IsBSMLAvailable)
            {
                RemoveMenu();
            }
        }

        private void AddEvents()
        {
            RemoveEvents();
            SceneManager.activeSceneChanged += OnActiveSceneChanged;
        }

        private void RemoveEvents()
        {
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void AddMenu()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu("SaberTailor", "SaberTailor.Settings.UI.Views.mainsettings.bsml", MainSettings.instance);
        }

        private void RemoveMenu()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.RemoveSettingsMenu(MainSettings.instance);
        }

        private static bool CheckPluginAvailable(string pluginName, Hive.Versioning.Version minVersion)
        {
            PluginMetadata pluginMetadata = IPA.Loader.PluginManager.GetPluginFromId(pluginName);
            if (pluginMetadata == null)
            {
                return false;
            }
            return pluginMetadata.HVersion >= minVersion;
        }

    }
}
