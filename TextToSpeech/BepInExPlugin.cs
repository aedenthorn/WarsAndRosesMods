using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Speech.Synthesis;

namespace TextToSpeech
{
    [BepInPlugin("aedenthorn.TextToSpeech", "TextToSpeech", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> enableMod;
        public static ConfigEntry<bool> isDebug;
        

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
        

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }


        [HarmonyPatch(typeof(UIDialog), "DoneWithDialogStep")]
        static class UIDialog_DoneWithDialogStep_Patch
        {
            static void Postfix(UIDialog __instance)
            {
                if (!enableMod.Value || __instance.curPlotStep.dialog0.Length == 0)
                    return;
                Dbgl($"saying {__instance.npcText.text}");
                var synthesizer = new SpeechSynthesizer();
                synthesizer.SetOutputToDefaultAudioDevice();
                synthesizer.Speak(__instance.npcText.text);
            }
        }
    }
}
