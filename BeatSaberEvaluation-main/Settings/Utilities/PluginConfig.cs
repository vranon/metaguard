using SaberTailor.Settings.Classes;

namespace SaberTailor.Settings.Utilities
{
    public class PluginConfig
    {
        public static PluginConfig Instance;

        public int ConfigVersion = 5;

        public bool IsSaberScaleModEnabled = false;
        public bool SaberScaleHitbox = false;
        public int SaberLength = 100;
        public int SaberGirth = 100;

        public bool IsTrailModEnabled = false;
        public bool IsTrailEnabled = true;
        public int TrailDuration = 400;                 // Age of trail - in ms?
        public int TrailGranularity = 60;               // Segments count in trail
        public int TrailWhiteSectionDuration = 100;     // Duration of gradient from white

        public bool IsGripModEnabled = false;

        public Int3 GripLeftPosition = new Int3();      // Position in mm
        public Int3 GripRightPosition = new Int3();

        public Int3 GripLeftRotation = new Int3();      // Rotation in °
        public Int3 GripRightRotation = new Int3();

        public Int3 GripLeftOffset = new Int3();        // Offset in mm
        public Int3 GripRightOffset = new Int3();

        public bool ModifyMenuHiltGrip = true;
        public bool UseBaseGameAdjustmentMode = true;

        public int SaberPosIncrement = 10;
        public int SaberPosIncValue = 1;
        public int SaberRotIncrement = 5;
        public string SaberPosIncUnit = "cm";
        public string SaberPosDisplayUnit = "cm";

        /// <summary>
        /// Call this to save to disk
        /// </summary>
        public virtual void Changed() { }
    }
}
