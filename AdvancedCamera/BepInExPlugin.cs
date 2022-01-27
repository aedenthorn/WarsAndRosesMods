using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RealisticEyeMovements;
using System;
using System.Reflection;
using UnityEngine;

namespace AdvancedCamera
{
    [BepInPlugin("aedenthorn.AdvancedCamera", "AdvancedCamera", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> enableMod;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<bool> enableRandom;
        private static ConfigEntry<string> randomToggleKey;
        private static ConfigEntry<string> xRotateKey;
        private static ConfigEntry<string> yRotateKey;
        private static ConfigEntry<string> zRotateKey;
        private static ConfigEntry<string> xNegRotateKey;
        private static ConfigEntry<string> yNegRotateKey;
        private static ConfigEntry<string> zNegRotateKey;
        private static ConfigEntry<string> xMoveKey;
        private static ConfigEntry<string> yMoveKey;
        private static ConfigEntry<string> zMoveKey;
        private static ConfigEntry<string> xNegMoveKey;
        private static ConfigEntry<string> yNegMoveKey;
        private static ConfigEntry<string> zNegMoveKey;
        private static ConfigEntry<string> resetKey;
        private static ConfigEntry<string> lookAtCameraKey;


        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                Debug.Log((pref ? typeof(BepInExPlugin).Namespace + " " : "") + str);
        }
        private void Awake()
        {

            context = this;
            enableMod = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");
            enableRandom = Config.Bind<bool>("Options", "EnableRandom", true, "Enable random camera switching");
            randomToggleKey = Config.Bind<string>("Options", "RandomToggleKey", "-", "Toggle random camera key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            lookAtCameraKey = Config.Bind<string>("Options", "LookAtCameraKey", "[5]", "Look at camera key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            xRotateKey = Config.Bind<string>("Options", "XRotateKey", "[2]", "X rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            yRotateKey = Config.Bind<string>("Options", "YRotateKey", "[6]", "Y rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            zRotateKey = Config.Bind<string>("Options", "ZRotateKey", "[9]", "Z rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            xNegRotateKey = Config.Bind<string>("Options", "XNegRotateKey", "[8]", "X negative rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            yNegRotateKey = Config.Bind<string>("Options", "YNegRotateKey", "[4]", "Y negative rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            zNegRotateKey = Config.Bind<string>("Options", "ZNegRotateKey", "[7]", "Z negative rotate key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            xMoveKey = Config.Bind<string>("Options", "XMoveKey", "right", "X Move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            yMoveKey = Config.Bind<string>("Options", "YMoveKey", "page up", "Y Move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            zMoveKey = Config.Bind<string>("Options", "ZMoveKey", "up", "Z Move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            xNegMoveKey = Config.Bind<string>("Options", "XNegMoveKey", "left", "X Negative move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            yNegMoveKey = Config.Bind<string>("Options", "YNegMoveKey", "page down", "Y negative move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            zNegMoveKey = Config.Bind<string>("Options", "ZNegMoveKey", "down", "Z negative move key. Use https://docs.unity3d.com/Manual/class-InputManager.html");
            resetKey = Config.Bind<string>("Options", "ResetKey", "r", "Reset camera key. Use https://docs.unity3d.com/Manual/class-InputManager.html");


            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }

        private void Update()
        {
            if (!enableMod.Value)
                return;

            if (CheckKeyDown(randomToggleKey.Value))
            {
                enableRandom.Value = !enableRandom.Value;

                Dbgl($"Random camera switching: {enableRandom.Value }");
                if (Global.code.curInteraction)
                {
                    var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                    var anim = camera.GetComponentInChildren<Animation>();
                    if (anim)
                    {
                        Dbgl($"Preventing random camera movement: {enableRandom.Value}");

                        anim.enabled = enableRandom.Value;
                    }
                }
                return;
            }

            if (enableRandom.Value || !Global.code.curInteraction)
                return;

            if (CheckKeyHeld(xRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += camera.transform.GetChild(0).localRotation * new Vector3(1, 0, 0);
                return;
            }
            if (CheckKeyHeld(yRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += camera.transform.GetChild(0).localRotation * new Vector3(0, 1, 0);
                return;
            }
            if (CheckKeyHeld(zRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += new Vector3(0, 0, 1);
                return;
            }
            
            if (CheckKeyHeld(xNegRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += camera.transform.GetChild(0).localRotation * new Vector3(-1, 0, 0);
                return;
            }
            if (CheckKeyHeld(yNegRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += camera.transform.GetChild(0).localRotation * new Vector3(0, -1, 0);
                return;
            }
            if (CheckKeyHeld(zNegRotateKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localEulerAngles += new Vector3(0, 0, -1);
                return;
            }

            if (CheckKeyHeld(xMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(0.05f, 0, 0);
                return;
            }
            if (CheckKeyHeld(yMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(0, 0.05f, 0);
                return;
            }
            if (CheckKeyHeld(zMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(0, 0, 0.05f);
                return;
            }

            if (CheckKeyHeld(xNegMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(-0.05f, 0, 0);
                return;
            }
            if (CheckKeyHeld(yNegMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(0, -0.05f, 0);
                return;
            }
            if (CheckKeyHeld(zNegMoveKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition += camera.transform.GetChild(0).localRotation * new Vector3(0, 0, -0.05f);
                return;
            }
            if (CheckKeyDown(resetKey.Value))
            {
                var camera = Global.code.curInteraction.cameras[Math.Max(Math.Min(Global.code.curInteraction.curCamIndex, Global.code.curInteraction.cameras.Length - 1), 0)];
                camera.transform.GetChild(0).localPosition = Vector3.zero;
                camera.transform.GetChild(0).localEulerAngles = Vector3.zero;
                return;
            }
            if (CheckKeyDown(lookAtCameraKey.Value))
            {
                LookTargetController[] ltcs = FindObjectsOfType<LookTargetController>();
                if (ltcs.Length != 0)
                {
                    foreach (LookTargetController ltc in ltcs)
                    {
                        if((int)AccessTools.Field(typeof(LookTargetController), "state").GetValue(ltc) != 0)
                        {
                            if (Global.code.curInteraction.cameras[Global.code.curInteraction.curCamIndex].transform.childCount > 0)
                            {
                                ltc.SetTarget(Global.code.curInteraction.cameras[Global.code.curInteraction.curCamIndex].transform.GetChild(0));
                            }
                            else
                            {
                                ltc.SetTarget(Global.code.curInteraction.cameras[Global.code.curInteraction.curCamIndex].transform);
                            }
                        }
                        else
                        {
                            ltc.LookAroundIdly();
                        }
                    }
                }
                return;
            }

        }
        public static bool CheckKeyDown(string value)
        {
            try
            {
                return Input.GetKeyDown(value.ToLower());
            }
            catch
            {
                return false;
            }
        }
        public static bool CheckKeyHeld(string value, bool req = true)
        {
            try
            {
                return Input.GetKey(value.ToLower());
            }
            catch
            {
                return !req;
            }
        }
        [HarmonyPatch(typeof(Interaction), "SelectRandomCamera")]
        static class Interaction_SelectRandomCamera_Patch
        {
            static bool Prefix(Interaction __instance)
            {
                if (!enableMod.Value)
                    return true;
                //Dbgl($"Preventing random camera switch: {!enableRandom.Value}");

                return enableRandom.Value;
            }
        }
        [HarmonyPatch(typeof(Interaction), nameof(Interaction.SelectCamera))]
        static class Interaction_SelectCamera_Patch
        {
            static void Postfix(Interaction __instance)
            {
                if (!enableMod.Value)
                    return;

                var camera = __instance.cameras[Math.Max(Math.Min(__instance.curCamIndex, __instance.cameras.Length - 1), 0)];
                var anim = camera.GetComponentInChildren<Animation>();
                if (anim)
                {
                    Dbgl($"Preventing random camera movement: {!enableRandom.Value}");

                    anim.enabled = enableRandom.Value;
                }
            }
        }
    }
}
