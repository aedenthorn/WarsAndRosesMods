using BepInEx;
using BepInEx.Configuration;
using RealisticEyeMovements;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace ClothesMod
{
    [BepInPlugin("aedenthorn.ClothesMod", "Clothes Mod", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<bool> isDebug;
        private static ConfigEntry<string> toggleKey
            ;
        private static ConfigEntry<Vector2> windowPosition;
        private static ConfigEntry<int> characterColumnWidth;
        private static ConfigEntry<Color> windowBackgroundColor;
        private static ConfigEntry<Color> selectedColor;
        private static ConfigEntry<float> windowHeight;
        private static ConfigEntry<int> smrColumnWidth;
        private static ConfigEntry<int> buttonHeight;
        private static ConfigEntry<string> windowTitleText;

        private static Vector2 characterScrollPosition;
        private static Vector2 smrScrollPosition;

        public static float rowWidth;
        private static Rect windowRect;

        private static int characterIndex;
        private static Dictionary<string, Texture2D> textureDict = new Dictionary<string, Texture2D>();

        private static List<string> hiddenSMRs = new List<string>();

        private static bool showWindow;

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
            toggleKey = Config.Bind<string>("General", "ToggleKey", "k", "Toggle key. Use https://docs.unity3d.com/Manual/class-InputManager.html");

            windowPosition = Config.Bind<Vector2>("UI", "WindowPosition", new Vector2(40, 40), "Position of the window on the screen");
            characterColumnWidth = Config.Bind<int>("UI", "CharacterColumnWidth", 300, "Width of the update text (will wrap if it is too long)");
            smrColumnWidth = Config.Bind<int>("UI", "ClothesColumnWidth", 300, "Width of the clothes column");
            buttonHeight = Config.Bind<int>("UI", "ButtonHeight", 30, "Height of the update button");
            windowHeight = Config.Bind<float>("UI", "WindowHeight", Screen.height / 3, "Height of the window");
            windowBackgroundColor = Config.Bind<Color>("UI", "WindowBackgroundColor", new Color(1, 1, 1, 0.3f), "Color of the window background");
            selectedColor = Config.Bind<Color>("UI", "SelectedColor", new Color(0, 1, 0, 0.5f), "Color of the window background");

            windowTitleText = Config.Bind<string>("Text", "WindowTitleText", "<b>Clothes Toggle</b>", "Window title");

            ApplyConfig();

            /*
            //Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Skins");
            foreach(var file in Directory.GetFiles(path))
            {
                byte[] data = File.ReadAllBytes(file);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(data);
                textureDict.Add(Path.GetFileNameWithoutExtension(file), texture);
                Dbgl($"Added skin texture {Path.GetFileNameWithoutExtension(file)}");
            }
            */
            Dbgl("Plugin awake");
        }
        private static void ApplyConfig()
        {
            rowWidth = characterColumnWidth.Value + smrColumnWidth.Value;

            windowRect = new Rect(windowPosition.Value.x, windowPosition.Value.y, rowWidth + 50, windowHeight.Value);


        }
        private void OnGUI()
        {
            if (enableMod.Value && showWindow)
            {
                GUI.backgroundColor = windowBackgroundColor.Value;

                windowRect = GUI.Window(424242, windowRect, new GUI.WindowFunction(WindowBuilder), windowTitleText.Value);
                if (!Input.GetKey(KeyCode.Mouse0) && (windowRect.x != windowPosition.Value.x || windowRect.y != windowPosition.Value.y))
                {
                    windowPosition.Value = new Vector2(windowRect.x, windowRect.y);
                    Config.Save();
                }
            }
        }

        private void WindowBuilder(int id)
        {
            var models = FindObjectsOfType<EyeAndHeadAnimator>();
            GUILayout.BeginVertical();
            GUI.DragWindow(new Rect(0, 0, rowWidth + 50, 20));
            if (!models.Any())
                return;

            var buttonStyle = GUI.skin.button;
            buttonStyle.fontStyle = FontStyle.Bold;

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            characterScrollPosition = GUILayout.BeginScrollView(characterScrollPosition, new GUILayoutOption[] { GUILayout.Width(characterColumnWidth.Value + 20), GUILayout.Height(windowHeight.Value - 30) });

            for(int i = 0; i < models.Length; i++)
            {
                var color = GUI.backgroundColor;
                if (i == characterIndex)
                {
                    GUI.backgroundColor = selectedColor.Value;
                }
                if (GUILayout.Button(models[i].name, (i == characterIndex ? buttonStyle : GUI.skin.button), new GUILayoutOption[]{
                            GUILayout.Width(characterColumnWidth.Value),
                            GUILayout.Height(buttonHeight.Value)
                        }))
                {
                    characterIndex = i;
                }
                GUI.backgroundColor = color;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            smrScrollPosition = GUILayout.BeginScrollView(smrScrollPosition, new GUILayoutOption[] { GUILayout.Width(smrColumnWidth.Value + 20), GUILayout.Height(windowHeight.Value - 30) });
            var smrs = new List<SkinnedMeshRenderer>();
            if (characterIndex < 0)
                characterIndex = models.Length - 1;
            if (characterIndex >= models.Length)
                characterIndex = 0;
            GetSMRs(models[characterIndex], smrs);
            for (int i = 0; i < smrs.Count; i++)
            {
                var hidden = hiddenSMRs.Contains(smrs[i].name) || !smrs[i].gameObject.activeSelf;
                if (hidden && smrs[i].gameObject.activeSelf)
                {
                    smrs[i].gameObject.SetActive(false);
                }
                var color = GUI.backgroundColor;
                if (!hidden)
                {
                    GUI.backgroundColor = selectedColor.Value;
                }
                if (GUILayout.Button(smrs[i].name, (!hidden ? buttonStyle : GUI.skin.button), new GUILayoutOption[]{
                            GUILayout.Width(smrColumnWidth.Value),
                            GUILayout.Height(buttonHeight.Value)
                        }))
                {
                    if (hidden)
                    {
                        hiddenSMRs.Remove(smrs[i].name);
                    }
                    else if(!hiddenSMRs.Contains(smrs[i].name))
                    {
                        hiddenSMRs.Add(smrs[i].name);
                    }
                    smrs[i].gameObject.SetActive(!smrs[i].gameObject.activeSelf);
                }
                GUI.backgroundColor = color;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void Update()
        {
            if (!enableMod.Value)
                return;
            if (!showWindow)
            {
                var models = FindObjectsOfType<EyeAndHeadAnimator>();
                if (models.Any())
                {
                    if (characterIndex >= models.Length)
                        characterIndex = 0;
                    if (characterIndex <= 0)
                        characterIndex = models.Length - 1;
                    if (!models[characterIndex])
                        return;
                    /*
                    var smr = models[characterIndex].transform.Find("Genesis8Female/Genesis8Female.Shape").GetComponent<SkinnedMeshRenderer>();
                    for (int j = 0; j < smr.materials.Length; j++)
                    {
                        foreach (string property in smr.materials[j].GetTexturePropertyNames())
                        {
                            int propHash = Shader.PropertyToID(property);
                            //Dbgl($"prop {property}, texture {smr.materials[j].GetTexture(propHash)?.name}");
                            if (smr.materials[j].HasProperty(propHash) && smr.materials[j].GetTexture(propHash) && textureDict.ContainsKey(smr.materials[j].GetTexture(propHash).name))
                            {
                                smr.materials[j].SetTexture(propHash, textureDict[smr.materials[j].GetTexture(propHash).name]);
                                Dbgl($"replaced skin texture for {models[characterIndex].name}");
                            }

                        }

                    } 
                    */
                    var smrs = new List<SkinnedMeshRenderer>();
                    GetSMRs(models[characterIndex], smrs);
                    for (int i = 0; i < smrs.Count; i++)
                    {
                        if (hiddenSMRs.Contains(smrs[i].name) && smrs[i].gameObject.activeSelf)
                        {
                            smrs[i].gameObject.SetActive(false);
                        }
                    }
                }
            }

            if (CheckKeyDown(toggleKey.Value))
            {
                showWindow = !showWindow;
                
                Dbgl($"Showing window: {showWindow }");

                return;
            }
        }

        private void GetSMRs(EyeAndHeadAnimator model, List<SkinnedMeshRenderer> smrs)
        {
            if (!model.transform.Find("Genesis8Female"))
                return;
            foreach (Transform child in model.transform.Find("Genesis8Female"))
            {
                if (child.name != "hip" && !child.name.StartsWith("Genesis8Female"))
                {
                    smrs.AddRange(child.GetComponentsInChildren<SkinnedMeshRenderer>(true));
                }
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
