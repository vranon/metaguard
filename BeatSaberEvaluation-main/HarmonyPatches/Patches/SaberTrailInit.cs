using HarmonyLib;
using SaberTailor.Settings;

namespace SaberTailor.HarmonyPatches
{
    [HarmonyPatch(typeof(SaberTrail))]
    [HarmonyPatch("Init")]
    internal class SaberTrailInit
    {
        private static void Prefix(SaberTrail __instance, ref float ____trailDuration, ref int ____granularity, ref float ____whiteSectionMaxDuration)
        {
            if (Configuration.Trail.TweakEnabled && Configuration.Trail.TrailEnabled)
            {
                ____trailDuration = Configuration.Trail.Duration / 1000f;
                ____granularity = Configuration.Trail.Granularity;
                ____whiteSectionMaxDuration = Configuration.Trail.WhiteSectionDuration / 1000f;
                Logger.log.Info("Successfully modified trails!");
            }
        }
    }
}
