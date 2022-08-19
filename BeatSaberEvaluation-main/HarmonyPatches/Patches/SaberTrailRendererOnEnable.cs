using HarmonyLib;
using SaberTailor.Settings;

namespace SaberTailor.HarmonyPatches.Patches
{
    [HarmonyPatch(typeof(SaberTrailRenderer))]
    [HarmonyPatch("OnEnable")]
    internal class SaberTrailRendererOnEnable
    {
        private static void Postfix(SaberTrailRenderer __instance)
        {
            if (Configuration.Trail.TweakEnabled && !Configuration.Trail.TrailEnabled)
            {
                __instance.enabled = false;
                Logger.log.Info("Successfully disabled trails!");
            }
        }
    }
}
