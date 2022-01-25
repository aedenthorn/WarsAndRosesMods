using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using RealisticEyeMovements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UIMod
{
    [BepInPlugin("aedenthorn.UIMod", "UIMod", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<bool> isDebug;
        private static ConfigEntry<string> toggleKey;


        private static bool showUI = true;

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
            toggleKey = Config.Bind<string>("General", "ToggleKey", "u", "Toggle key. Use https://docs.unity3d.com/Manual/class-InputManager.html");


            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");
        }
        private void Update()
        {
            if (!enableMod.Value)
                return;

            if (CheckKeyDown(toggleKey.Value) && Global.code)
            {
                showUI = !showUI;
                
                Dbgl($"Showing UI: {showUI }");

                Global.code.transform.Find("Canvas").GetComponent<Canvas>().enabled = showUI;

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
    }
}
