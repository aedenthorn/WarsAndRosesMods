using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace Translation
{
    [BepInPlugin("aedenthorn.Translation", "Translation", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> enableMod;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<string> dumpDialogKey;
        public static ConfigEntry<string> dumpDialogOverwriteKey;
        public static ConfigEntry<string> reloadDialogKey;
        public static ConfigEntry<string> fallbackLang;

        private static Dictionary<string, Dictionary<string, string>> stringsDict = new Dictionary<string, Dictionary<string, string>>();

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
            reloadDialogKey = Config.Bind<string>("Options", "ReloadDialogKey", "home", "Key to load dialog from file");
            fallbackLang = Config.Bind<string>("Options", "FallbackLang", "English", "Language fallback");
            dumpDialogKey = Config.Bind<string>("Options", "DumpDialogKey", "end", "Key to dump dialog to file");
            dumpDialogOverwriteKey = Config.Bind<string>("Options", "DumpDialogKey", "delete", "Key to dump dialog in current scene, overwriting existing dumped strings");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");
        }

        private void Update()
        {
            if (!enableMod.Value || !Global.code)
                return;

            if (CheckKeyDown(reloadDialogKey.Value))
            {
                ReloadDialogue();
            }
            if (CheckKeyDown(dumpDialogKey.Value) || CheckKeyDown(dumpDialogOverwriteKey.Value))
            {
                Dbgl("Dumping strings");
                var lang = Global.code.uiSettings.language;
                Dictionary<string, string> strings = new Dictionary<string, string>();

                // Localization Texts

                var lts = FindObjectsOfType<LocalizationText>(true);
                foreach(var lt in lts)
                {
                    string fullName = GetFullName(lt.transform);
                    strings[fullName] = GetArrayEntry(lt.texts, lang);
                }

                // Characters

                foreach (Transform girl in Global.code.transform.Find("Girls"))
                {
                    if (girl)
                    {
                        strings["girlName|" + girl.GetComponent<Girl>().characterName[1]] = GetArrayEntry(girl.GetComponent<Girl>().characterName, lang);
                        strings["girlJob|" + girl.GetComponent<Girl>().characterName[1]] = GetArrayEntry(girl.GetComponent<Girl>().job, lang);
                    }
                }

                // Plots

                var plots = Resources.LoadAll("PlotScenes", typeof(Transform));
                foreach (Transform plotScene in plots)
                {
                    var plot = plotScene.GetComponentInChildren<Plot>();
                    if(plot)
                        strings["plotName|" + plotScene.name] = GetArrayEntry(plot.plotName, lang);
                    
                    var steps = plotScene.GetComponentsInChildren<PlotStep>();
                    foreach (var step in steps)
                    {
                        if (step.dialog0 != null)
                            strings[plotScene.name + "|" + step.transform.GetSiblingIndex() + "|dialog0"] = GetArrayEntry(step.dialog0, lang);
                        if (step.dialog1 != null)
                            strings[plotScene.name + "|" + step.transform.GetSiblingIndex() + "|dialog1"] = GetArrayEntry(step.dialog1, lang);
                        if (step.choice0 != null)
                            strings[plotScene.name + "|" + step.transform.GetSiblingIndex() + "|choice0"] = GetArrayEntry(step.choice0, lang);
                        if (step.choice1 != null)
                            strings[plotScene.name + "|" + step.transform.GetSiblingIndex() + "|choice1"] = GetArrayEntry(step.choice1, lang);
                    }
                }
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Translation");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path = Path.Combine(path, $"{Global.code.uiSettings.selectorLanguage.elements[Global.code.uiSettings.language]}.json");
                List<string> allStrings = new List<string>();
                if (File.Exists(path))
                {
                    var stringsList = JsonUtility.FromJson<StringList>(File.ReadAllText(path));
                    if(stringsList != null)
                    {
                        allStrings = stringsList.list;
                        foreach (var kvp in strings)
                        {
                            string exist = allStrings.Find(s => s.StartsWith(kvp.Key + "|"));
                            if (exist != null)
                            {
                                if (CheckKeyDown(dumpDialogKey.Value))
                                    continue;
                                allStrings.Remove(exist);
                            }
                            allStrings.Add(kvp.Key + "|" + kvp.Value);
                        }
                    }
                    else
                    {
                        foreach (var kvp in strings)
                        {
                            allStrings.Add(kvp.Key + "|" + kvp.Value);
                        }
                    }
                }
                else
                {
                    foreach (var kvp in strings)
                    {
                        allStrings.Add(kvp.Key + "|" + kvp.Value);
                    }
                }
                var outList = new StringList(allStrings);
                outList.list.Sort(delegate(string a, string b) 
                {
                    var partsA = a.Split('|');
                    var partsB = b.Split('|');
                    for(int i = 0; i < (partsA.Length >= partsB.Length ? partsA.Length : partsB.Length); i++)
                    {
                        if (partsA.Length <= i && partsB.Length <= i)
                            return 0;
                        if (partsA.Length <= i)
                            return -1;
                        if (partsB.Length <= i)
                            return 1;
                        string strA = partsA[i];
                        string strB = partsB[i];
                        int c;
                        if (int.TryParse(strA, out int intA) && int.TryParse(strB, out int intB))
                        {
                            c = intA.CompareTo(intB);
                            if (c != 0)
                                return c;
                        }
                        c = strA.CompareTo(strB);
                        if (c != 0)
                            return c;
                    }
                    return 0;
                });
                Dbgl($"Writing {outList.list.Count} strings to {path}");
                File.WriteAllText(path, JsonUtility.ToJson(outList, true));
                return;
            }
        }



        private string GetArrayEntry(string[] array, int lang)
        {
            if (array.Length > lang)
                return array[lang];

            int fallbackIndex = Math.Max(Global.code.uiSettings.selectorLanguage.elements.IndexOf(fallbackLang.Value), 0);
            if (array.Length > fallbackIndex)
                return array[fallbackIndex];

            return "";
        }
        private static string[] AddLanguage(string[] dialogArray, string textKey, bool fallback = false)
        {
            //Dbgl($"Checking {textKey}");

            int langIndex = Global.code.uiSettings.language;
            string langKey = Global.code.uiSettings.selectorLanguage.elements[langIndex];
            int fallbackIndex = Math.Max(Global.code.uiSettings.selectorLanguage.elements.IndexOf(fallbackLang.Value), 0);
            if (dialogArray.Length <= langIndex)
            {
                List<string> dialog = new List<string>(dialogArray);

                while (dialog.Count <= langIndex)
                {
                    if (dialog.Count == langIndex)
                    {
                        string text;
                        if (stringsDict.ContainsKey(langKey) && stringsDict[langKey].ContainsKey(textKey))
                            text = stringsDict[langKey][textKey];
                        else
                        {
                           text = dialog.Count > fallbackIndex ? dialog[fallbackIndex] : "";
                           //Dbgl($"No string for language {langKey}, key {textKey}. Using {text}");
                        }
                        //Dbgl($"key {textKey} set to {text}");
                        dialog.Add(text);
                    }
                    else
                    {
                        //Dbgl($"key {textKey} set to null");
                        dialog.Add(dialog.Count > fallbackIndex ? dialog[fallbackIndex] : "");
                    }
                }
                return dialog.ToArray();
            }
            else
            {
                string text;
                if (stringsDict.ContainsKey(langKey) && stringsDict[langKey].ContainsKey(textKey))
                    text = stringsDict[langKey][textKey];
                else
                {
                    text = dialogArray.Length > fallbackIndex ? dialogArray[fallbackIndex] : "";
                    //Dbgl($"No string for language {langKey}, key {textKey}. Using {text}");
                }
                //Dbgl($"key {textKey} set to {text}");
                dialogArray[langIndex] = text;
                return dialogArray;
            }

        }
        private static void ReloadDialogue()
        {
            stringsDict = new Dictionary<string, Dictionary<string, string>>();
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Translation");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            foreach (var file in Directory.GetFiles(path))
            {
                try
                {
                    stringsDict[Path.GetFileNameWithoutExtension(file)] = new Dictionary<string, string>();
                    var list = JsonUtility.FromJson<StringList>(File.ReadAllText(file)).list;
                    foreach (var entry in list)
                    {
                        var parts = entry.Split('|');

                        stringsDict[Path.GetFileNameWithoutExtension(file)][string.Join("|", parts.ToList().Take(parts.Length - 1))] = parts[parts.Length - 1];
                    }
                    Dbgl($"Loaded {stringsDict[Path.GetFileNameWithoutExtension(file)].Count} strings for {Path.GetFileNameWithoutExtension(file)}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error reading {file}:\n\n" + ex);
                }
            }
            foreach (Transform girl in Global.code.transform.Find("Girls"))
            {
                if (girl)
                {
                    girl.GetComponent<Girl>().characterName = AddLanguage(girl.GetComponent<Girl>().characterName, "girlName|" + girl.GetComponent<Girl>().characterName[1], true);
                    girl.GetComponent<Girl>().job = AddLanguage(girl.GetComponent<Girl>().job, "girlJob|" + girl.GetComponent<Girl>().characterName[1], true);
                }
            }
        }


        private static string GetFullName(Transform transform)
        {
            var t = transform;
            string fullName = t.name;
            while (t.parent)
            {
                t = t.parent;
                fullName = t.name + "/" + fullName;
            }
            return fullName;
        }

        private static bool CheckKeyDown(string value)
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
        private static bool CheckKeyHeld(string value, bool req = true)
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



        [HarmonyPatch(typeof(UISettings), "Start")]
        static class UISettings_Start_Patch
        {
            static void Prefix(UISettings __instance)
            {
                string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Translation");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                foreach (var file in Directory.GetFiles(path))
                {
                    string lang = Path.GetFileNameWithoutExtension(file);
                    if (!__instance.selectorLanguage.elements.Contains(lang))
                    {
                        Dbgl($"Adding language {lang} to settings");
                        __instance.selectorLanguage.elements.Add(lang);
                    }

                }

            }
        }
        [HarmonyPatch(typeof(LocalizationText), "Localize")]
        static class LocalizationText_Localize_Patch
        {
            static bool Prefix(LocalizationText __instance)
            {
                if (!enableMod.Value)
                    return true;
                if (Global.code)
                {
                    __instance.texts = AddLanguage(__instance.texts, GetFullName(__instance.transform), true);
                    __instance.GetComponent<Text>().text = __instance.texts[Global.code.uiSettings.language];
                    if (__instance.fonts.Length != 0)
                    {
                        __instance.GetComponent<Text>().font = __instance.fonts[Global.code.uiSettings.language == 0 ? 0 : 1];
                    }
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(LocalizationPic), "Localize")]
        static class LocalizationPic_Localize_Patch
        {
            static bool Prefix(LocalizationPic __instance)
            {
                if (!enableMod.Value)
                    return true;
                if (Global.code)
                {
                    if (__instance.pics.Length <= Global.code.uiSettings.language)
                        __instance.GetComponent<RawImage>().texture = __instance.pics[1];
                    else
                        __instance.GetComponent<RawImage>().texture = __instance.pics[Global.code.uiSettings.language];
                }
                return false;
            }
        }
        [HarmonyPatch(typeof(UISettings), "LoadSettings")]
        static class UISettings_LoadSettings_Patch
        {
            static void Postfix()
            {
                ReloadDialogue();
            }
        }
        [HarmonyPatch(typeof(SelectionIcon), "Initiate")]
        static class SelectionIcon_Initiate_Patch
        {
            static void Prefix(Girl _girl)
            {
                _girl.characterName = AddLanguage(_girl.characterName, "girlName|" + _girl.characterName[1], true);
                _girl.job = AddLanguage(_girl.job, "girlJob|" + _girl.characterName[1], true);
            }
        }
        [HarmonyPatch(typeof(PlotIcon), "Initiate")]
        static class PlotIcon_Initiate_Patch
        {
            static void Prefix(Plot _plot)
            {
                _plot.plotName = AddLanguage(_plot.plotName, "plotName|" + _plot.plotName, true);
            }
        }
        [HarmonyPatch(typeof(Girl), "Start")]
        static class Girl_Start_Patch
        {
            static void Postfix(Girl __instance)
            {
                Dbgl($"Girl {__instance.characterName[1]}, names {__instance.characterName.Length}, jobs {__instance.job.Length}");

                __instance.characterName = AddLanguage(__instance.characterName, "girlName|" + __instance.characterName[1], true);
                __instance.job = AddLanguage(__instance.job, "girlJob|" + __instance.characterName[1], true);
            }
        }
        [HarmonyPatch(typeof(UIDialog), "DoneWithDialogStep")]
        static class UIDialog_DoneWithDialogStep_Patch
        {
            static void Prefix(UIDialog __instance)
            {
                if (!enableMod.Value)
                    return;

                int langIndex = Global.code.uiSettings.language;
                string langKey = Global.code.uiSettings.selectorLanguage.elements[langIndex];

                if (!stringsDict.ContainsKey(langKey))
                {
                    if (Global.code.uiSettings.language > 1)
                        Dbgl($"Language {langKey} not found!");
                    return;
                }
                if (__instance.curPlotStep.girl.characterName.Length > 0)
                {
                    __instance.curPlotStep.girl.characterName = AddLanguage(__instance.curPlotStep.girl.characterName, "girlName|" + __instance.curPlotStep.girl.name);
                }
                if (__instance.curPlotStep.dialog0.Length > 0)
                {
                    __instance.curPlotStep.dialog0 = AddLanguage(__instance.curPlotStep.dialog0, Global.code.uiDialog.curplot.name + "|" + Global.code.uiDialog.curPlotStep.transform.GetSiblingIndex() + "|dialog0");

                }
                if (__instance.curPlotStep.dialog1.Length > 0)
                {
                    __instance.curPlotStep.dialog1 = AddLanguage(__instance.curPlotStep.dialog1, Global.code.uiDialog.curplot.name + "|" + Global.code.uiDialog.curPlotStep.transform.GetSiblingIndex() + "|dialog1");

                }
                if (__instance.curPlotStep.choice0.Length > 0)
                {
                    __instance.curPlotStep.choice0 = AddLanguage(__instance.curPlotStep.choice0, Global.code.uiDialog.curplot.name + "|" + Global.code.uiDialog.curPlotStep.transform.GetSiblingIndex() + "|choice0");

                }
                if (__instance.curPlotStep.choice1.Length > 0)
                {
                    __instance.curPlotStep.choice1 = AddLanguage(__instance.curPlotStep.choice1, Global.code.uiDialog.curplot.name + "|" + Global.code.uiDialog.curPlotStep.transform.GetSiblingIndex() + "|choice1");

                }


            }
            static void Postfix(UIDialog __instance)
            {
                if (!enableMod.Value)
                    return;
                Dbgl($"saying {__instance.npcText.text}");

            }

        }
    }
}
