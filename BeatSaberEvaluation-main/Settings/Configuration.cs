using IPA.Config;
using SaberTailor.HarmonyPatches;
using SaberTailor.Settings.Classes;
using SaberTailor.Settings.Utilities;
using System;
using UnityEngine;

namespace SaberTailor.Settings
{
    internal enum ConfigSection { All, Grip, GripLeft, GripRight, Scale, Trail, Menu };
    internal enum GripConfigSide { Left, Right };

    public class Configuration
    {
        public static int ConfigVersion { get; private set; }                // Config version, to handle changes in config where existing configs shouldn't just get default config applied

        public static GripConfig Grip { get; internal set; } = new GripConfig();
        public static ScaleConfig Scale { get; internal set; } = new ScaleConfig();
        public static TrailConfig Trail { get; internal set; } = new TrailConfig();

        public static MenuConfig Menu { get; internal set; } = new MenuConfig();

        // Config vars for representing player settings before it gets mangled into something that works with the game,
        // but changes representation of these settings in the process - also avoiding floating points
        public static GripRawConfig GripCfg { get; internal set; } = new GripRawConfig();
        public static ScaleRawConfig ScaleCfg { get; internal set; } = new ScaleRawConfig();

        internal static void Init()
        {
            FileHandler.LoadConfig(out PluginConfig config);
            PluginConfig.Instance = config;
        }

        /// <summary>
        /// Save Configuration
        /// </summary>
        internal static void Save()
        {
            SaveConfig(ref PluginConfig.Instance);
            FileHandler.SaveConfig(PluginConfig.Instance);
        }

        /// <summary>
        /// Load Configuration
        /// </summary>
        internal static void Load()
        {
            // Yeet this once ModPrefs support is dropped by BSIPA.
#pragma warning disable CS0618 // ModPrefs is obsolete
            if (ModPrefs.HasKey(Plugin.PluginName, "GripLeftPosition") && !ModPrefs.GetBool(Plugin.PluginName, "IsExportedToNewConfig", false))
#pragma warning restore CS0618 // ModPrefs is obsolete
            {
                // Import SaberTailor's settings from the old configuration (ModPrefs)
                try
                {
                    PluginConfig importedConfig = ConfigurationImporter.ImportSettingsFromModPrefs();
                    PluginConfig.Instance = importedConfig;

                    // Store configuration in the new format immediately
                    PluginConfig.Instance.Changed();
                    Logger.log.Info("Configuration loaded from ModPrefs.");
                }
                catch (Exception ex)
                {
                    Logger.log.Warn("Failed to import ModPrefs configuration. Loading default BSIPA configuration instead.");
                    Logger.log.Warn(ex);
                }
            }

            try
            {
                LoadConfig();
            }
            catch (Exception ex)
            {
                Logger.log.Warn("Failed to load config file. Generating new default configuration...");
                PluginConfig.Instance = new PluginConfig();
                LoadConfig();
            }
            UpdateConfig();
            Logger.log.Debug("Configuration has been set");

            // Update variables used by mod logic
            UpdateModVariables();
        }

        /// <summary>
        /// Reload configuration
        /// </summary>
        internal static void Reload(ConfigSection cfgSection = ConfigSection.All)
        {
            LoadConfig(cfgSection);
            UpdateModVariables();
        }

        /// <summary>
        /// Update Saber Length, Position and Rotation
        /// </summary>
        internal static void UpdateModVariables()
        {
            UpdateSaberLength();
            UpdateSaberPosition();
            UpdateSaberRotation();
            UpdateSaberOffset();
        }

        /// <summary>
        /// Update Saber Length
        /// </summary>
        internal static void UpdateSaberLength()
        {
            Scale.Length = ScaleCfg.Length / 100f;
            Scale.Girth = ScaleCfg.Girth / 100f;
        }

        /// <summary>
        /// Update Saber Position
        /// </summary>
        internal static void UpdateSaberPosition()
        {
            Grip.PosLeft = Int3.ToVector3(GripCfg.PosLeft) / 1000f;
            Grip.PosRight = Int3.ToVector3(GripCfg.PosRight) / 1000f;
        }

        /// <summary>
        /// Update Saber Offset
        /// </summary>
        internal static void UpdateSaberOffset()
        {
            Grip.OffsetLeft = Int3.ToVector3(GripCfg.OffsetLeft) / 1000f;
            Grip.OffsetRight = Int3.ToVector3(GripCfg.OffsetRight) / 1000f;
        }

        /// <summary>
        /// Update Saber Rotation
        /// </summary>
        internal static void UpdateSaberRotation()
        {
            Grip.RotLeft = Quaternion.Euler(Int3.ToVector3(GripCfg.RotLeft)).eulerAngles;
            Grip.RotRight = Quaternion.Euler(Int3.ToVector3(GripCfg.RotRight)).eulerAngles;
        }

        /// <summary>
        /// Mirrors a grip config from one side to another
        /// </summary>
        /// <param name="toTarget"></param>
        internal static void MirrorGripToSide(GripConfigSide targetSide)
        {
            if (targetSide == GripConfigSide.Left)
            {
                GripCfg.PosLeft = new Int3()
                {
                    x = -GripCfg.PosRight.x,
                    y = GripCfg.PosRight.y,
                    z = GripCfg.PosRight.z
                };
                GripCfg.RotLeft = new Int3()
                {
                    x = GripCfg.RotRight.x,
                    y = -GripCfg.RotRight.y,
                    z = -GripCfg.RotRight.z
                };
                GripCfg.OffsetLeft = new Int3()
                {
                    x = -GripCfg.OffsetRight.x,
                    y = GripCfg.OffsetRight.y,
                    z = GripCfg.OffsetRight.z
                };
            }
            else
            {
                GripCfg.PosRight = new Int3()
                {
                    x = -GripCfg.PosLeft.x,
                    y = GripCfg.PosLeft.y,
                    z = GripCfg.PosLeft.z
                };
                GripCfg.RotRight = new Int3()
                {
                    x = GripCfg.RotLeft.x,
                    y = -GripCfg.RotLeft.y,
                    z = -GripCfg.RotLeft.z
                };
                GripCfg.OffsetRight = new Int3()
                {
                    x = -GripCfg.OffsetLeft.x,
                    y = GripCfg.OffsetLeft.y,
                    z = GripCfg.OffsetLeft.z
                };
            }
        }

        private static void LoadConfig(ConfigSection cfgSection = ConfigSection.All)
        {
            #region Internal settings
            ConfigVersion = PluginConfig.Instance.ConfigVersion;
            #endregion

            #region Saber scale
            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Scale)
            {
                Scale.TweakEnabled = PluginConfig.Instance.IsSaberScaleModEnabled;
                Scale.ScaleHitBox = PluginConfig.Instance.SaberScaleHitbox;

                if (PluginConfig.Instance.SaberLength < 5 || PluginConfig.Instance.SaberLength > 500)
                {
                    ScaleCfg.Length = 100;
                }
                else
                {
                    ScaleCfg.Length = PluginConfig.Instance.SaberLength;
                }

                if (PluginConfig.Instance.SaberGirth < 5 || PluginConfig.Instance.SaberGirth > 500)
                {
                    ScaleCfg.Girth = 100;
                }
                else
                {
                    ScaleCfg.Girth = PluginConfig.Instance.SaberGirth;
                }
            }
            #endregion

            #region Saber trail
            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Scale)
            {
                Trail.TweakEnabled = PluginConfig.Instance.IsTrailModEnabled;
                Trail.TrailEnabled = PluginConfig.Instance.IsTrailEnabled;
                Trail.Duration = Mathf.Clamp(PluginConfig.Instance.TrailDuration, 100, 5000);
                Trail.Granularity = Mathf.Clamp(PluginConfig.Instance.TrailGranularity, 10, 200);
                Trail.WhiteSectionDuration = Mathf.Clamp(PluginConfig.Instance.TrailWhiteSectionDuration, 0, 2000);
            }
            #endregion

            #region Saber grip
            // Even though the field says GripLeftRotation/GripRightRotation, it is actually the Cfg values that are loaded!
            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Grip || cfgSection == ConfigSection.GripLeft)
            {
                Int3 gripLeftPosition = PluginConfig.Instance.GripLeftPosition ?? Int3.zero;
                GripCfg.PosLeft = new Int3()
                {
                    x = Mathf.Clamp(gripLeftPosition.x, -500, 500),
                    y = Mathf.Clamp(gripLeftPosition.y, -500, 500),
                    z = Mathf.Clamp(gripLeftPosition.z, -500, 500)
                };

                Int3 gripLeftRotation = PluginConfig.Instance.GripLeftRotation ?? Int3.zero;
                GripCfg.RotLeft = new Int3(gripLeftRotation);

                Int3 gripLeftOffset = PluginConfig.Instance.GripLeftOffset ?? Int3.zero;
                GripCfg.OffsetLeft = new Int3()
                {
                    x = Mathf.Clamp(gripLeftOffset.x, -500, 500),
                    y = Mathf.Clamp(gripLeftOffset.y, -500, 500),
                    z = Mathf.Clamp(gripLeftOffset.z, -500, 500)
                };
            }

            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Grip || cfgSection == ConfigSection.GripRight)
            {
                Int3 gripRightPosition = PluginConfig.Instance.GripRightPosition ?? Int3.zero;
                GripCfg.PosRight = new Int3()
                {
                    x = Mathf.Clamp(gripRightPosition.x, -500, 500),
                    y = Mathf.Clamp(gripRightPosition.y, -500, 500),
                    z = Mathf.Clamp(gripRightPosition.z, -500, 500)
                };

                Int3 gripRightRotation = PluginConfig.Instance.GripRightRotation ?? Int3.zero;
                GripCfg.RotRight = new Int3(gripRightRotation);

                Int3 gripRightOffset = PluginConfig.Instance.GripRightOffset ?? Int3.zero;
                GripCfg.OffsetRight = new Int3()
                {
                    x = Mathf.Clamp(gripRightOffset.x, -500, 500),
                    y = Mathf.Clamp(gripRightOffset.y, -500, 500),
                    z = Mathf.Clamp(gripRightOffset.z, -500, 500)
                };
            }

            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Grip)
            {
                Grip.IsGripModEnabled = PluginConfig.Instance.IsGripModEnabled;
                Grip.ModifyMenuHiltGrip = PluginConfig.Instance.ModifyMenuHiltGrip;
                Grip.UseBaseGameAdjustmentMode = PluginConfig.Instance.UseBaseGameAdjustmentMode;
            }
            #endregion

            #region Menu settings
            if (cfgSection == ConfigSection.All || cfgSection == ConfigSection.Menu)
            {
                Menu.SaberPosIncrement = Mathf.Clamp(PluginConfig.Instance.SaberPosIncrement, 1, 200);
                Menu.SaberPosIncValue = Mathf.Clamp(PluginConfig.Instance.SaberPosIncValue, 1, 20);
                Menu.SaberRotIncrement = Mathf.Clamp(PluginConfig.Instance.SaberRotIncrement, 1, 20);

                Menu.SaberPosDisplayUnit = Enum.TryParse(PluginConfig.Instance.SaberPosDisplayUnit, out PositionDisplayUnit displayUnit)
                    ? displayUnit
                    : PositionDisplayUnit.cm;

                Menu.SaberPosIncUnit = Enum.TryParse(PluginConfig.Instance.SaberPosIncUnit, out PositionUnit positionUnit)
                    ? positionUnit
                    : PositionUnit.cm;
            }
            #endregion

            SaberTailorPatches.CheckHarmonyPatchStatus();
        }

        internal static void SaveConfig(ref PluginConfig config)
        {
            #region Internal settings
            config.ConfigVersion = ConfigVersion;
            #endregion

            #region Saber scale
            config.IsSaberScaleModEnabled = Scale.TweakEnabled;
            config.SaberScaleHitbox = Scale.ScaleHitBox;
            config.SaberLength = ScaleCfg.Length;
            config.SaberGirth = ScaleCfg.Girth;
            #endregion

            #region Saber trail
            config.IsTrailModEnabled = Trail.TweakEnabled;
            config.IsTrailEnabled = Trail.TrailEnabled;
            config.TrailDuration = Trail.Duration;
            config.TrailGranularity = Trail.Granularity;
            config.TrailWhiteSectionDuration = Trail.WhiteSectionDuration;
            #endregion

            #region Saber grip
            // Even though the field says GripLeftPosition/GripRightPosition, it is actually the Cfg values that are stored!
            config.GripLeftPosition = new Int3(GripCfg.PosLeft);
            config.GripRightPosition = new Int3(GripCfg.PosRight);

            // Even though the field says GripLeftRotation/GripRightRotation, it is actually the Cfg values that are stored!
            config.GripLeftRotation = new Int3(GripCfg.RotLeft);
            config.GripRightRotation = new Int3(GripCfg.RotRight);

            config.GripLeftOffset = new Int3(GripCfg.OffsetLeft);
            config.GripRightOffset = new Int3(GripCfg.OffsetRight);

            config.IsGripModEnabled = Grip.IsGripModEnabled;
            config.ModifyMenuHiltGrip = Grip.ModifyMenuHiltGrip;
            config.UseBaseGameAdjustmentMode = Grip.UseBaseGameAdjustmentMode;
            #endregion

            #region Menu settings
            config.SaberPosDisplayUnit = Menu.SaberPosDisplayUnit.ToString();
            config.SaberPosIncrement = Menu.SaberPosIncrement;
            config.SaberPosIncUnit = Menu.SaberPosIncUnit.ToString();
            config.SaberPosIncValue = Menu.SaberPosIncValue;
            config.SaberRotIncrement = Menu.SaberRotIncrement;
            #endregion
        }

        /// <summary>
        /// Handle updates and additions to configuration.
        /// Only needed if new settings shouldn't be set to default values in (some) existing config files
        /// </summary>
        private static void UpdateConfig()
        {
            // Get latest version from default config values
            int latestVersion = new PluginConfig().ConfigVersion;

            // Do nothing if config is already up to date
            if (ConfigVersion == latestVersion)
            {
                return;
            }

            // v1/v2 -> v3: Added enable/disable options for trail and scale modifications
            // Updating v2 as well because of a beta build that is floating around with v2 already being used
            if (ConfigVersion == 1 || ConfigVersion == 2)
            {
                // Due to changes in Beat Saber 1.12.2, just reset trail settings to default for these old config files
                Trail.TweakEnabled = false;

                // Check scale modifications and disable tweak if settings are default
                if (ScaleCfg.Length == 100 && ScaleCfg.Girth == 100)
                {
                    Scale.TweakEnabled = false;
                }
                else
                {
                    // else enable tweak and hitbox scaling to preserve existing settings
                    Scale.TweakEnabled = true;
                    Scale.ScaleHitBox = true;
                }
                ConfigVersion = 3;
            }

            if (ConfigVersion == 3)
            {
                // v3 -> v4: Added enable/disable option for saber grip, disabled by default, will override base game option
                // For existing configurations: Enable, if non-default settings are present
                bool gripAdjPresent = false;
                if (GripCfg.PosLeft != Int3.zero || GripCfg.RotLeft != Int3.zero
                    || GripCfg.PosRight != Int3.zero || GripCfg.RotRight != Int3.zero)
                {
                    gripAdjPresent = true;
                }

                Grip.IsGripModEnabled = gripAdjPresent;
                ConfigVersion = 4;
            }

            if (ConfigVersion == 4)
            {
                // v4 -> v5: Added toggle to change adjustment mode (switch between SaberTailor or base game adjustment mode)
                // For existing configuration: Enable, if non-default settings are present
                bool gripAdjPresent = false;
                if (GripCfg.PosLeft != Int3.zero || GripCfg.RotLeft != Int3.zero
                    || GripCfg.PosRight != Int3.zero || GripCfg.RotRight != Int3.zero)
                {
                    gripAdjPresent = true;
                }

                Grip.UseBaseGameAdjustmentMode = !gripAdjPresent;
                ConfigVersion = 5;
            }

            // Updater done - set to latest version and save
            ConfigVersion = latestVersion;
            Save();
        }
    }
}
