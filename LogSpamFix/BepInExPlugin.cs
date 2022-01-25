using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace LogSpamFix
{
    [BepInPlugin("aedenthorn.LogSpamFix", "Log Spam Fix", "0.1.0")]
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
            enableMod = Config.Bind<bool>("General", "Enabled", false, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");
        

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }

        [HarmonyPatch(typeof(AffectionDisplay), "RefreshAffectionInfo")]
        static class RefreshAffectionInfo_Patch
        {
            static bool Prefix(AffectionDisplay __instance)
            {
                return !(__instance.affectionFill == null || __instance.girl == null || __instance.txtAffection == null || __instance.maxAffectionEffect == null || __instance.txtAffectionString == null);
            }
        }


        [HarmonyPatch(typeof(Character), "LateUpdate")]
        static class Character_LateUpdate_Patch1
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Dbgl("Transpiling Character LateUpdate");

                var codes = new List<CodeInstruction>(instructions);

                var index = codes.FindIndex(c => c.opcode == OpCodes.Call && (MethodInfo)c.operand == AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.print)));
                if (index > 2)
                {
                    Dbgl("Removing print");

                    codes[index - 3] = new CodeInstruction(OpCodes.Nop);
                    codes[index - 2] = new CodeInstruction(OpCodes.Nop);
                    codes[index - 1] = new CodeInstruction(OpCodes.Nop);
                    codes[index - 0] = new CodeInstruction(OpCodes.Nop);
                }

                return codes.AsEnumerable();
            }
        }
        [HarmonyPatch(typeof(LODSwitcher), "SetLODLevel")]
        static class LODSwitcher_SetLODLevel_Patch1
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                Dbgl("Transpiling LODSwitcher SetLODLevel");

                var codes = new List<CodeInstruction>(instructions);

                var index = codes.FindIndex(c => c.opcode == OpCodes.Call && (MethodInfo)c.operand == AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.print)));
                if (index > 1)
                {
                    Dbgl("Removing print");

                    codes[index - 2] = new CodeInstruction(OpCodes.Nop);
                    codes[index - 1] = new CodeInstruction(OpCodes.Nop);
                    codes[index - 0] = new CodeInstruction(OpCodes.Nop);
                }

                return codes.AsEnumerable();
            }
        }

        public static object lastLogMessage;


        [HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(LogType), typeof(object)})]
        static class Logger_Log_Patch1
        {
            static bool Prefix(object message)
            {
                //Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { Environment.StackTrace });
                if (message == lastLogMessage)
                {
                    
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(object)})]
        static class Logger_Log_Patch2
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 2 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(LogType), typeof(object), typeof(Object)})]
        static class Logger_Log_Patch3
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 3 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(LogType), typeof(string), typeof(object) })]
        static class Logger_Log_Patch4
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 4 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(LogType), typeof(string), typeof(object), typeof(Object) })]
        static class Logger_Log_Patch5
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 5 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(string), typeof(object), typeof(Object) })]
        static class Logger_Log_Patch6
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 6 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "Log", new Type[] { typeof(string), typeof(object) })]
        static class Logger_Log_Patch7
        {
            static bool Prefix(object message)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 7 });
                if (message == lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = message;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "LogFormat", new Type[] { typeof(LogType), typeof(string), typeof(object[]) })]
        static class Logger_LogFormat_Patch1
        {
            static bool Prefix(string format)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 8 });
                if (lastLogMessage is string && format == (string)lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = format;
                return true;
            }
        }
        //[HarmonyPatch(typeof(Logger), "LogFormat", new Type[] { typeof(LogType),  typeof(Object), typeof(string), typeof(object[]) })]
        static class Logger_LogFormat_Patch2
        {
            static bool Prefix(string format)
            {
                Debug.unityLogger.logHandler.LogFormat(LogType.Log, null, "{0}", new object[] { 9 });
                if (lastLogMessage is string && format == (string)lastLogMessage)
                {
                    Debug.unityLogger.Log(LogType.Log, Environment.StackTrace);
                    return false;
                }
                lastLogMessage = format;
                return true;
            }
        }
    }
}
