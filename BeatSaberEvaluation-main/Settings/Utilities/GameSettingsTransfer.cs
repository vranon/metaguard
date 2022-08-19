using IPA.Utilities;
using SaberTailor.Settings.Classes;
using SaberTailor.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace SaberTailor.Settings.Utilities
{
    // Status: This option will need more work, because the settings in game are applied differently than what SaberTailor does
    //         So, unfortunatly, values won't fit 1:1 in this scenario
    internal class GameSettingsTransfer
    {
        internal static Vector3 addPosOculus = new Vector3(0f, 0f, 0.055f);
        internal static Vector3 addRotOculus = new Vector3(-40f, 0f, 0f);

        internal static Vector3 addPosOpenVR = new Vector3(0f, -0.008f, 0f);
        internal static Vector3 addRotOpenVR = new Vector3(-4.3f, 0f, 0f);

        internal static Vector3 addPosViveIndex = new Vector3(0f, 0.022f, -0.01f);
        internal static Vector3 addRotViveIndex = new Vector3(-16.3f, 0f, 0f);

        internal static bool ImportFromGame()
        {
            // Get reference to MainSettingsModelSO
            MainSettingsModelSO mainSettings = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().FirstOrDefault();
            if (mainSettings == null)
            {
                Logger.log.Error("ImportFromGame: Unable to get a handle on MainSettingsModelSO. Exiting...");
                return false;
            }

            // Check for position/rotation settings
            if (mainSettings.controllerPosition == null)
            {
                Logger.log.Error("ImportFromGame: Settings for controller position not found. Exiting...");
                return false;
            }

            if (mainSettings.controllerRotation == null)
            {
                Logger.log.Error("ImportFromGame: Settings for controller rotation not found. Exiting...");
                return false;
            }

            Vector3SO gameCtrlPosSO = mainSettings.controllerPosition;
            Vector3SO gameCtrlRotSO = mainSettings.controllerRotation;

            Vector3 ctrlPos = new Vector3(gameCtrlPosSO.value.x, gameCtrlPosSO.value.y, gameCtrlPosSO.value.z);
            Vector3 ctrlRot = new Vector3(gameCtrlRotSO.value.x, gameCtrlRotSO.value.y, gameCtrlRotSO.value.z);

            Configuration.GripCfg.PosLeft = new Int3()
            {
                x = -(int)Math.Round(ctrlPos.x * 1000, MidpointRounding.AwayFromZero),
                y = (int)Math.Round(ctrlPos.y * 1000, MidpointRounding.AwayFromZero),
                z = (int)Math.Round(ctrlPos.z * 1000, MidpointRounding.AwayFromZero)
            };

            Configuration.GripCfg.RotLeft = new Int3()
            {
                x = (int)Math.Round(ctrlRot.x, MidpointRounding.AwayFromZero),
                y = -(int)Math.Round(ctrlRot.y, MidpointRounding.AwayFromZero),
                z = (int)Math.Round(ctrlRot.z, MidpointRounding.AwayFromZero)
            };

            Configuration.GripCfg.PosRight = new Int3()
            {
                x = (int)Math.Round(ctrlPos.x * 1000, MidpointRounding.AwayFromZero),
                y = (int)Math.Round(ctrlPos.y * 1000, MidpointRounding.AwayFromZero),
                z = (int)Math.Round(ctrlPos.z * 1000, MidpointRounding.AwayFromZero)
            };

            Configuration.GripCfg.RotRight = new Int3()
            {
                x = (int)Math.Round(ctrlRot.x, MidpointRounding.AwayFromZero),
                y = (int)Math.Round(ctrlRot.y, MidpointRounding.AwayFromZero),
                z = (int)Math.Round(ctrlRot.z, MidpointRounding.AwayFromZero)
            };

            Configuration.GripCfg.OffsetLeft = new Int3()
            {
                x = 0,
                y = 0,
                z = 0
            };

            Configuration.GripCfg.OffsetRight = new Int3()
            {
                x = 0,
                y = 0,
                z = 0
            };

            Configuration.Grip.UseBaseGameAdjustmentMode = true;

            return true;
        }

        internal static bool ExportToGame(out string statusMsg)
        {
            bool exportable = true;
            statusMsg = "";

            Vector3 stPos = new Vector3(
                Configuration.GripCfg.PosRight.x / 1000f,
                Configuration.GripCfg.PosRight.y / 1000f,
                Configuration.GripCfg.PosRight.z / 1000f);

            Vector3 stRot = new Vector3(
                Configuration.GripCfg.RotRight.x,
                Configuration.GripCfg.RotRight.y,
                Configuration.GripCfg.RotRight.z);

            Vector3 stOffset = new Vector3(
                Configuration.GripCfg.OffsetRight.x / 1000f,
                Configuration.GripCfg.OffsetRight.y / 1000f,
                Configuration.GripCfg.OffsetRight.z / 1000f);

            ConvertSTtoGameVector(stPos, stRot, stOffset, out Vector3 exportPos, out Vector3 exportRot);

            if (Configuration.Grip.UseBaseGameAdjustmentMode)
            {
                exportPos = stPos;
                exportRot = stRot;
            }

            exportable = CheckGripCompatibility(exportPos, exportRot, out statusMsg);

            if (exportable)
            {
                try
                {
                    // Get reference to MainSettingsModelSO
                    MainSettingsModelSO mainSettings = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>().FirstOrDefault();
                    if (mainSettings == null)
                    {
                        Logger.log.Error("ImportFromGame: Unable to get a handle on MainSettingsModelSO. Exiting...");
                        return false;
                    }

                    // Set new position vector
                    mainSettings.controllerPosition.value = exportPos;

                    // Set new rotation vector. Base game settings are clamped to -180,+180 on load, hence the extra work
                    mainSettings.controllerRotation.value = new Vector3(
                        NormalizeAngle(exportRot.x),
                        NormalizeAngle(exportRot.y),
                        NormalizeAngle(exportRot.z));

                    mainSettings.Save();
                    mainSettings.Load(true);

                    statusMsg = "Export successful.";
                }
                catch (Exception ex)
                {
                    Logger.log.Error("Error trying to export SaberTailor grip config to base game settings.");
                    Logger.log.Error(ex.ToString());
                    statusMsg = "<color=#fb484e>Unable to export to base game: Unknown error.</color>";
                    exportable = false;
                }
            }
            return exportable;
        }

        internal static bool CheckGripCompatibility(Vector3 posRight, Vector3 rotRight, out string statusMsg)
        {
            bool exportable = true;
            statusMsg = "";

            float baseGamePosLimit = 0.1f;            // in m

            // Check to see if position adjustments are within base game limits (Check right only because left values needs mirrored to right anyway)
            if (posRight.x < -baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position X is smaller than -{0:0} cm", baseGamePosLimit * 100), ref exportable);
            }
            if (posRight.x > baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position X is larger than {0:0} cm", baseGamePosLimit * 100), ref exportable);
            }
            if (posRight.y < -baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position Y is smaller than -{0:0} cm", baseGamePosLimit * 100), ref exportable);
            }
            if (posRight.y > baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position Y is larger than {0:0} cm", baseGamePosLimit * 100), ref exportable);
            }
            if (posRight.z < -baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position Z is smaller than -{0:0} cm", baseGamePosLimit * 100), ref exportable);
            }
            if (posRight.z > baseGamePosLimit)
            {
                statusMsg = AddErrorStatus(statusMsg, String.Format("Resulting right position Z is larger than {0:0} cm", baseGamePosLimit * 100), ref exportable);
            }

            // Check to see if position adjustments are mirrored
            if (Configuration.GripCfg.PosRight.x != -Configuration.GripCfg.PosLeft.x)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right position X is not equal to inverted left position X", ref exportable);
            }
            if (Configuration.GripCfg.PosRight.y != Configuration.GripCfg.PosLeft.y)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right position Y is not equal to left position Y", ref exportable);
            }
            if (Configuration.GripCfg.PosRight.z != Configuration.GripCfg.PosLeft.z)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right position Z is not equal to left position Z", ref exportable);
            }

            // Check to see if rotation adjustments are mirrored
            if (Configuration.GripCfg.RotRight.x != Configuration.GripCfg.RotLeft.x)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right rotation X is not equal to left rotation X", ref exportable);
            }
            if (Configuration.GripCfg.RotRight.y != -Configuration.GripCfg.RotLeft.y)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right rotation Y is not equal to inverted left rotation Y", ref exportable);
            }
            if (Configuration.GripCfg.RotRight.z != Configuration.GripCfg.RotLeft.z)
            {
                statusMsg = AddErrorStatus(statusMsg, "Right rotation Z is not equal to left rotation Z", ref exportable);
            }

            if (exportable)
            {
                statusMsg = "Export to game settings is possible.";
            }

            return exportable;
        }

        internal static void ConvertGametoSTVector(Vector3 inPos, Vector3 inRot, out Vector3 outPos, out Vector3 outRot)
        {
            GetVRPlatformHelperVector(out Vector3 vrPlatformPos, out Vector3 vrPlatformRot);

            // Calculate target position using game translation/rotation
            GameObject gameAdjustObject = new GameObject();
            Transform gameAdjustTransform = gameAdjustObject.transform;
            gameAdjustTransform.Rotate(inRot + vrPlatformRot);
            gameAdjustTransform.Translate(inPos + vrPlatformPos);

            // From that target point, calculate data for saber tailor adjustments
            gameAdjustTransform.Translate(-vrPlatformPos);
            gameAdjustTransform.Rotate(-vrPlatformRot);

            outPos = gameAdjustTransform.position.Clone();
            outRot = gameAdjustTransform.rotation.eulerAngles.Clone();

            UnityEngine.Object.Destroy(gameAdjustObject);
        }

        internal static void ConvertSTtoGameVector(Vector3 inPos, Vector3 inRot, Vector3 inOffset, out Vector3 outPos, out Vector3 outRot)
        {
            GetVRPlatformHelperVector(out Vector3 vrPlatformPos, out Vector3 vrPlatformRot);

            // Calculate target position using saber tailor translation/rotation
            GameObject stAdjustObject = new GameObject();
            Transform stAdjustTransform = stAdjustObject.transform;
            stAdjustTransform.Translate(inPos);
            stAdjustTransform.Rotate(inRot);
            stAdjustTransform.Translate(inOffset, Space.World);
            stAdjustTransform.Rotate(vrPlatformRot);
            stAdjustTransform.Translate(vrPlatformPos);

            // From that target point, calculate data for base game adjustments
            outPos = stAdjustTransform.InverseTransformVector(stAdjustTransform.position) - vrPlatformPos;
            outRot = stAdjustTransform.rotation.eulerAngles - vrPlatformRot;

            UnityEngine.Object.Destroy(stAdjustObject);
        }

        // Internal test method for debugging
        internal static void CompareAdjustmentSettings()
        {
            GetVRPlatformHelperVector(out Vector3 vrPlatformPos, out Vector3 vrPlatformRot);

            Logger.log.Debug("==============================================================================");
            Logger.log.Debug("VR Platform modifiers:");
            Logger.log.Debug("Position: x=" + vrPlatformPos.x + " y=" + vrPlatformPos.y + " z=" + vrPlatformPos.z);
            Logger.log.Debug("Rotation: x=" + vrPlatformRot.x + " y=" + vrPlatformRot.y + " z=" + vrPlatformRot.z);
            Logger.log.Debug("==============================================================================");

            Vector3 posTest = new Vector3(0.3f, 0.3f, 0.3f);
            Vector3 rotTest = Quaternion.Euler(new Vector3(50f, 50f, 50f)).eulerAngles;

            Vector3 rotTest2 = new Vector3(50f, 50f, 50f);


            GameObject stAdjustObject = new GameObject();
            Transform stAdjustTransform = stAdjustObject.transform;
            stAdjustTransform.position = Vector3.zero;
            stAdjustTransform.rotation = Quaternion.Euler(Vector3.zero);

            stAdjustTransform.Translate(posTest);
            stAdjustTransform.Rotate(rotTest);
            stAdjustTransform.Rotate(vrPlatformRot);
            stAdjustTransform.Translate(vrPlatformPos);

            Logger.log.Debug("==============================================================================");
            Logger.log.Debug("SaberTailor Adjustments Result:");
            Logger.log.Debug("Position: x=" + stAdjustTransform.position.x + " y=" + stAdjustTransform.position.y + " z=" + stAdjustTransform.position.z);
            Logger.log.Debug("Rotation: x=" + stAdjustTransform.rotation.eulerAngles.x + " y=" + stAdjustTransform.rotation.eulerAngles.y + " z=" + stAdjustTransform.rotation.eulerAngles.z);

            stAdjustTransform.Translate(-vrPlatformPos);
            stAdjustTransform.Rotate(-vrPlatformRot);

            GameObject stToGameObject = new GameObject();
            Transform stToGameTransform = stToGameObject.transform;
            stToGameTransform.position = Vector3.zero;
            stToGameTransform.rotation = Quaternion.Euler(Vector3.zero);

            ConvertSTtoGameVector(posTest, rotTest, Vector3.zero, out Vector3 toGamePos, out Vector3 toGameRot);
            Vector3 stToGameAdjustPos = vrPlatformPos.Clone();
            Vector3 stToGameAdjustRot = vrPlatformRot.Clone();

            stToGameAdjustPos += toGamePos;
            stToGameAdjustRot += toGameRot;

            stToGameTransform.Rotate(stToGameAdjustRot);
            stToGameTransform.Translate(stToGameAdjustPos);

            Logger.log.Debug("==============================================================================");
            Logger.log.Debug("SaberTailor to Game Adjustments Result:");
            Logger.log.Debug("Position: x=" + stToGameTransform.position.x + " y=" + stToGameTransform.position.y + " z=" + stToGameTransform.position.z);
            Logger.log.Debug("Rotation: x=" + stToGameTransform.rotation.eulerAngles.x + " y=" + stToGameTransform.rotation.eulerAngles.y + " z=" + stToGameTransform.rotation.eulerAngles.z);
            Logger.log.Debug("==============================================================================");


            GameObject gameAdjustObject = new GameObject();
            Transform gameAdjustTransform = gameAdjustObject.transform;
            gameAdjustTransform.position = Vector3.zero;
            gameAdjustTransform.rotation = Quaternion.Euler(Vector3.zero);

            Vector3 gameAdjustPos = vrPlatformPos.Clone();
            Vector3 gameAdjustRot = vrPlatformRot.Clone();

            gameAdjustPos += posTest;
            gameAdjustRot += rotTest2;

            gameAdjustTransform.Rotate(gameAdjustRot);
            gameAdjustTransform.Translate(gameAdjustPos);
            
            Logger.log.Debug("==============================================================================");
            Logger.log.Debug("Game Adjustments Result:");
            Logger.log.Debug("Position: x=" + gameAdjustTransform.position.x + " y=" + gameAdjustTransform.position.y + " z=" + gameAdjustTransform.position.z);
            Logger.log.Debug("Rotation: x=" + gameAdjustTransform.rotation.eulerAngles.x + " y=" + gameAdjustTransform.rotation.eulerAngles.y + " z=" + gameAdjustTransform.rotation.eulerAngles.z);


            GameObject gameToSTObject = new GameObject();
            Transform gameToSTTransform = gameToSTObject.transform;
            gameToSTTransform.position = Vector3.zero;
            gameToSTTransform.rotation = Quaternion.Euler(Vector3.zero);

            ConvertGametoSTVector(posTest, rotTest, out Vector3 toSTPos, out Vector3 toSTRot);
            gameToSTTransform.Translate(toSTPos);
            gameToSTTransform.Rotate(toSTRot);
            gameToSTTransform.Rotate(vrPlatformRot);
            gameToSTTransform.Translate(vrPlatformPos);

            Logger.log.Debug("==============================================================================");
            Logger.log.Debug("Game Adjustments to SaberTailor Result:");
            Logger.log.Debug("Position: x=" + gameToSTTransform.position.x + " y=" + gameToSTTransform.position.y + " z=" + gameToSTTransform.position.z);
            Logger.log.Debug("Rotation: x=" + gameToSTTransform.rotation.eulerAngles.x + " y=" + gameToSTTransform.rotation.eulerAngles.y + " z=" + gameToSTTransform.rotation.eulerAngles.z);
            Logger.log.Debug("==============================================================================");

            UnityEngine.Object.Destroy(stAdjustObject);
            UnityEngine.Object.Destroy(stToGameObject);
            UnityEngine.Object.Destroy(gameAdjustObject);
            UnityEngine.Object.Destroy(gameToSTObject);
        }

        private static string AddErrorStatus(string status, string addStatus, ref bool firstMsg)
        {
            if (firstMsg)
            {
                firstMsg = false;
                return "<color=#fb484e>Unable to export to base game: " + addStatus;
            }
            else
            {
                return status + " | " + addStatus;
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
            {
                angle = angle - 360f;
            }

            while (angle < -180f)
            {
                angle = angle + 360f;
            }

            return angle;
        }

        private static bool GetVRPlatformHelperVector(out Vector3 position, out Vector3 rotation)
        {
            position = Vector3.zero;
            rotation = Vector3.zero;

            if (XRSettings.loadedDeviceName == "Oculus")
            {
                position = addPosOculus.Clone();
                rotation = addRotOculus.Clone();
                return true;
            }

            if (XRSettings.loadedDeviceName == "OpenVR")
            {
                OpenVRHelper[] vrHelpers = Resources.FindObjectsOfTypeAll<OpenVRHelper>();
                foreach (OpenVRHelper vrHelper in vrHelpers)
                {
                    if (vrHelper.gameObject.activeInHierarchy)
                    {
                        if (vrHelper.GetField<OpenVRHelper.VRControllerManufacturerName, OpenVRHelper>("_vrControllerManufacturerName") == OpenVRHelper.VRControllerManufacturerName.Valve)
                        {
                            position = addPosViveIndex.Clone();
                            rotation = addRotViveIndex.Clone();
                        }
                        else
                        {
                            position = addPosOpenVR.Clone();
                            rotation = addRotOpenVR.Clone();
                        }
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
