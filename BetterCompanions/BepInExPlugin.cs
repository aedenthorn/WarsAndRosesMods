using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace BetterCompanions
{
    [BepInPlugin("aedenthorn.BetterCompanions", "Better Companions", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> enableMod;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<float> damageTakenMult;
        public static ConfigEntry<int> minTimeBetweenRadio;
        public static ConfigEntry<int> radioChancePerSecond;
        public static ConfigEntry<bool> noRadioWhileFollowing;
        public static ConfigEntry<bool> extraEquipmentSlots;
        public static ConfigEntry<int> healthRegenPerSecond;
        

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
            damageTakenMult = Config.Bind<float>("General", "DamageTakenMult", 0.5f, "Multiply all damage taken by companions by this amount");
            minTimeBetweenRadio = Config.Bind<int>("General", "MinTimeBetweenRadio", 20, "Min number of seconds between idle radio chatter. Vanilla is 15");
            radioChancePerSecond = Config.Bind<int>("General", "RadioChancePerSecond", 20, "Percent chance per second for radio chatter. Vanilla is 30");
            noRadioWhileFollowing = Config.Bind<bool>("General", "NoRadioWhileFollowing", true, "No radio chatter while following player.");
            extraEquipmentSlots = Config.Bind<bool>("General", "ExtraEquipmentSlots", true, "Add 3 extra equipment slots beyond the vanilla 3");
            healthRegenPerSecond = Config.Bind<int>("General", "HealthRegenPerSecond", 1, "Regen this many health per second until full");
        
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }

        [HarmonyPatch(typeof(UIPreparation), "Refresh")]
        static class UIPreparation_Refresh_Patch
        {
            static void Postfix(UIPreparation __instance)
            {
                if (!enableMod.Value)
                    return;

                var buttons = __instance.transform.Find("Content/Box 3/Categories (1)");
                if (buttons)
                {
                    buttons.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -62);
                }

                foreach (var asi in __instance.GetComponentsInChildren<AgentSelectionIcon>())
                {
                    if (asi.name == "Agent Player")
                        continue;
                    int target = extraEquipmentSlots.Value ? 10 : 7;
                    var ess = asi.GetComponentsInChildren<EquipmentSlot>();
                    Dbgl($"{asi.name} has {ess.Length} slots");
                    while (ess.Length < target)
                    {
                        var newSlot = Instantiate(ess[ess.Length - 1].gameObject, ess[ess.Length - 1].transform.parent);
                        ess = asi.GetComponentsInChildren<EquipmentSlot>();
                        newSlot.name = "equipment" + (ess.Length - 4);
                        newSlot.GetComponent<RectTransform>().anchoredPosition = new Vector2(48, -714.6f) + new Vector2(104 * ((ess.Length - 5) % 3), -104.6f * ((ess.Length - 5) / 3));
                    }
                    while(ess.Length > target)
                    {
                        DestroyImmediate(ess[ess.Length - 1].gameObject);
                        ess = asi.GetComponentsInChildren<EquipmentSlot>();
                    }
                    foreach(var es in ess)
                    {
                        es.Initiate(Global.code.combatagents.items[asi.index]?.GetComponent<Girl>());
                    }
                    //Dbgl($"{asi.name} now has {ess.Length} slots");
                }
            }
        }

        [HarmonyPatch(typeof(ID), "AddHealth")]
        static class ID_AddHealth_Patch
        {
            static void Prefix(ID __instance, ref float point)
            {
                if (!enableMod.Value || !__instance.isFriendly || point > 0)
                    return;
                point *= damageTakenMult.Value;
            }
        }

        [HarmonyPatch(typeof(Character), "FriendlyTactics")]
        static class Character_FriendlyTactics_Patch
        {
            static void Prefix(Character __instance)
            {
                if (!enableMod.Value || !__instance._ID.isFriendly || !noRadioWhileFollowing.Value)
                    return;
                __instance.idleTimer = 0;
            }
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Dbgl("Transpiling Character FriendlyTactics");

                var codes = new List<CodeInstruction>(instructions);

                var index = codes.FindIndex(c => c.opcode == OpCodes.Ldc_I4_S && (sbyte)c.operand == 15);
                if (index > 0 && codes[index + 5].opcode == OpCodes.Ldc_I4_S && (sbyte)codes[index + 5].operand == 30)
                {
                    Dbgl("Changing radio logic");

                    codes[index] = new CodeInstruction(OpCodes.Ldc_I4, minTimeBetweenRadio.Value);
                    codes[index +5].operand = radioChancePerSecond.Value;
                }

                return codes.AsEnumerable();
            }
            static void Postfix(Character __instance)
            {
                if (!enableMod.Value)
                    return;
                __instance._ID.AddHealth(healthRegenPerSecond.Value);
            }
        }
    }
}
