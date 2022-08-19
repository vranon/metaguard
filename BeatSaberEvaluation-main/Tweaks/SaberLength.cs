using BS_Utils.Gameplay;
using IPA.Utilities;
using SaberTailor.Settings;
using SaberTailor.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaberTailor.Tweaks
{
    public class SaberLength : MonoBehaviour
    {
        public static string Name => "SaberLength";
        private bool EnableHitboxScaling = Configuration.Scale.ScaleHitBox;
        private float LengthMultiplier = Configuration.Scale.Length;
        private bool IsMultiplayerEnv = false;

#pragma warning disable IDE0051 // Used by MonoBehaviour
        private void Start() => Load();
#pragma warning restore IDE0051 // Used by MonoBehaviour

        private void Load()
        {
            // Check for modes that would force changes to the config for the current map
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                // Disable hitbox scaling and cosmetic length scaling in multiplayer
                if (SceneManager.GetSceneAt(i).name == "MultiplayerGameplay")
                {
                    LengthMultiplier = 1;
                    EnableHitboxScaling = false;
                    IsMultiplayerEnv = true;
                    Logger.log.Info("Multiplayer environment detected, disabling hitbox scaling and cosmetic length scaling.");
                }
                
                // Disable hitbox scaling in mission gameplay
                if (SceneManager.GetSceneAt(i).name == "MissionGameplay")
                {
                    EnableHitboxScaling = false;
                    Logger.log.Info("Campaign environment detected, disabling hitbox scaling.");
                }
            }

            // If BS Utils is not available, disable hitbox scaling in any case
            if (!Plugin.IsBSUtilsAvailable)
            {
                EnableHitboxScaling = false;
            }

            // Allow the user to run in any other mode, but don't allow ScoreSubmission if hitbox scaling is enabled
            if (EnableHitboxScaling)
            {
                ScoreSubmission.DisableSubmission(Plugin.PluginName);
                Logger.log.Info("ScoreSubmission has been disabled.");
            }

            StartCoroutine(ApplyGameCoreModifications());
        }

        private IEnumerator ApplyGameCoreModifications()
        {
            yield return new WaitForSeconds(0.1f);

            Saber[] sabers = Resources.FindObjectsOfTypeAll<Saber>();
            foreach (Saber saber in sabers)
            {
                // Only scale solo sabers in solo mode
                if (IsMultiplayerEnv || IsSoloGameplaySaber(saber.gameObject))
                {
                    // Scaling sabers will affect its hitbox, so save the default hitbox positions first before scaling
                    HitboxRevertWorkaround hitboxVariables = null;
                    if (!EnableHitboxScaling)
                    {
                        hitboxVariables = new HitboxRevertWorkaround(saber);
                    }

                    // Rescale visible saber (FIXME: Need to account for custom sabers once that mod is available again)
                    RescaleSaber(saber.gameObject, LengthMultiplier, Configuration.Scale.Girth);

                    // Revert hitbox changes to sabers, if hitbox scaling is disabled
                    if (hitboxVariables != null)
                    {
                        hitboxVariables.RestoreHitbox();
                    }
                }
            }

            #region TrailScaling
            // The new SaberTrail in 1.12.1 is kinda hardcoded to current blade top/bottom positions. So disabling trail scaling for now.
            /*
            IEnumerable<SaberModelController> saberModelControllers = Resources.FindObjectsOfTypeAll<SaberModelController>();
            foreach (SaberModelController saberModelController in saberModelControllers)
            {
                SaberTrail saberTrail = saberModelController.GetField<SaberTrail, SaberModelController>("_saberTrail");
                Logger.log.Debug("SaberTrailName is '" + saberTrail.name + "'.");

                if (!usingCustomModels || saberTrail.name != "BasicSaberModel")
                {
                    //RescaleWeaponTrail(saberTrail, Configuration.Scale.Length, usingCustomModels);
                }
            }
            */
            #endregion

            #region CustomSabers
            // Deal with CustomSabers once it got updated.
            /*
            if (Utilities.Utils.IsPluginEnabled("Custom Sabers"))
            {
                // Wait a moment for CustomSaber to catch up
                yield return new WaitForSeconds(0.1f);
                GameObject customSaberClone = GameObject.Find("_CustomSaber(Clone)");

                // If customSaberClone is null, CustomSaber is most likely not replacing the default sabers.
                if (customSaberClone != null)
                {
                    LeftSaber = GameObject.Find("LeftSaber");
                    RightSaber = GameObject.Find("RightSaber");
                    usingCustomModels = true;
                }
                else
                {
                    Logger.log.Debug("Either the Default Sabers are selected or CustomSaber were too slow!");
                }
            }
            */
            #endregion

            yield return null;
        }

        private void OnDestroy()
        {
            // GameCore ended - for whatever reason - check if we were in an multiplayer environment
            if (!IsMultiplayerEnv)
            {
                // No cleanup needed
                return;
            }

            // We were in a multiplayer environment - reset saber to original size
            Saber[] sabers = Resources.FindObjectsOfTypeAll<Saber>();
            foreach (Saber saber in sabers)
            {
                RescaleSaber(saber.gameObject, 1 / LengthMultiplier, 1 / Configuration.Scale.Girth);
            }
        }

        private bool IsSoloGameplaySaber(GameObject saber)
        {
            bool saberIsSolo = true;
            while (saber.transform.parent != null)
            {
                saber = saber.transform.parent.gameObject;
                if (saber.name.Contains("Multiplayer"))
                {
                    saberIsSolo = false;
                }
            }
            return saberIsSolo;
        }

        private void RescaleSaber(GameObject saber, float lengthMultiplier, float widthMultiplier)
        {
            if (saber != null)
            {
                saber.transform.localScale = Vector3Extensions.Rescale(saber.transform.localScale, widthMultiplier, widthMultiplier, lengthMultiplier);
            }
        }

        private void RescaleSaberHitBox(Saber saber, float lengthMultiplier)
        {
            if (saber != null)
            {
                Transform topPos = saber.GetField<Transform, Saber>("_saberBladeTopTransform");
                Transform bottomPos = saber.GetField<Transform, Saber>("_saberBladeBottomTransform");

                topPos.localPosition = Vector3Extensions.Rescale(topPos.localPosition, 1.0f, 1.0f, lengthMultiplier);
                bottomPos.localPosition = Vector3Extensions.Rescale(bottomPos.localPosition, 1.0f, 1.0f, lengthMultiplier);
            }
        }

        private void RescaleWeaponTrail(SaberTrail trail, float lengthMultiplier, bool usingCustomModels)
        {
            SaberTrailRenderer trailRenderer = trail.GetField<SaberTrailRenderer, SaberTrail>("_trailRenderer");

            float trailWidth = trailRenderer.GetField<float, SaberTrailRenderer>("_trailWidth");
            trailRenderer.SetField("_trailWidth", trailWidth * lengthMultiplier);

            // Fix the local z position for the default trail on custom sabers
            if (usingCustomModels)
            {
                Transform pointEnd = trail.GetField<Transform, SaberTrail>("_pointEnd");
                pointEnd.localPosition = Vector3Extensions.Rescale(pointEnd.localPosition, 1.0f, 1.0f, pointEnd.localPosition.z * lengthMultiplier);
            }
        }

        /// <summary>
        /// Work-Around for reverting Saber Hit-box scaling
        /// </summary>
        private class HitboxRevertWorkaround
        {
            private readonly Transform saberTop;
            private readonly Transform saberBot;

            private Vector3 defaultHitboxTopPos;
            private Vector3 defaultHitboxBotPos;

            public HitboxRevertWorkaround(Saber saber)
            {
                // Scaling sabers will affect their hitboxes, so save the default hitbox positions first before scaling
                GetHitboxDefaultTransforms(saber, out saberTop, out saberBot);
                defaultHitboxTopPos = saberTop.position.Clone();
                defaultHitboxBotPos = saberBot.position.Clone();

                //Logger.log.Info("Distance is: " + Vector3.Distance(defaultHitboxTopPos, defaultHitboxBotPos).ToString());
            }

            /// <summary>
            /// Restores the sabers original Hit-box scale
            /// </summary>
            public void RestoreHitbox()
            {
                saberTop.position = defaultHitboxTopPos;
                saberBot.position = defaultHitboxBotPos;
            }

            private void GetHitboxDefaultTransforms(Saber saber, out Transform saberTop, out Transform saberBot)
            {
                saberTop = saber.GetField<Transform, Saber>("_saberBladeTopTransform");
                saberBot = saber.GetField<Transform, Saber>("_saberBladeBottomTransform");
            }
        }
    }
}
