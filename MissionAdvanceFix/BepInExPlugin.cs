using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace MissionAdvanceFix
{
    [BepInPlugin("aedenthorn.MissionAdvanceFix", "MissionAdvanceFix", "0.1.0")]
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


        [HarmonyPatch(typeof(MissionIcon), "Initiate")]
        static class MissionIcon_Initiate_Patch
        {
            static void Prefix()
            {
                int curMission = 0;
                foreach(var m in Global.code.missions.items)
                {
                    curMission++;
                    if (m.GetComponent<Mission>().stars <= 0)
                        break;
                }
                Global.code.curMissionLevel = curMission;
            }
        }
    }
}
