using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace PlayerTweaks
{
    [BepInPlugin("aedenthorn.PlayerTweaks", "PlayerTweaks", "0.1.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> enableMod;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<float> healthRegenPerSecond;
        public static ConfigEntry<float> walkSpeed;
        public static ConfigEntry<float> sprintSpeed;
        public static ConfigEntry<float> jumpSpeed;
        public static ConfigEntry<int> multiJump;
        public static ConfigEntry<float> crouchSpeedMult;
        public static ConfigEntry<float> zoomSpeedMult;
        public static ConfigEntry<float> maxStamina;


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
            healthRegenPerSecond = Config.Bind<float>("Options", "HealthRegenPerSecond", 0.1f, "Regen this many health per second until full");
            walkSpeed = Config.Bind<float>("Options", "WalkSpeed", 4.2f, "Speed while walking");
            sprintSpeed = Config.Bind<float>("Options", "SprintSpeed", 6.3f, "Speed while sprinting");
            jumpSpeed = Config.Bind<float>("Options", "JumpSpeed", 2.35f, "Speed of jumping");
            crouchSpeedMult = Config.Bind<float>("Options", "CrouchSpeedMult", 0.6f, "Speed mult for crouching (0 to 1)");
            zoomSpeedMult = Config.Bind<float>("Options", "ZoomSpeedMult", 0.8f, "Speed mult for crouching (0 to 1)");
            maxStamina = Config.Bind<float>("Options", "MaxStamina", 8f, "Max stamina");
            //multiJump = Config.Bind<int>("Options", "MultiJump", 2, "Number of multiple jumps");
            

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
            Dbgl("Plugin awake");

        }


        [HarmonyPatch(typeof(PlayerControl), "CS")]
        static class PlayerControl_CS_Patch
        {
            static void Prefix(PlayerControl __instance)
            {
                if (!enableMod.Value)
                    return;
                __instance._ID.AddHealth(healthRegenPerSecond.Value);
            }
        }
        [HarmonyPatch(typeof(FPSRigidBodyWalker), "Start")]
        static class FPSRigidBodyWalker_Start_Patch
        {
            static void Prefix(FPSRigidBodyWalker __instance)
            {
                if (!enableMod.Value)
                    return;
                __instance.staminaForSprint = maxStamina.Value;
                __instance.walkSpeed = walkSpeed.Value;
                __instance.sprintSpeed = sprintSpeed.Value;
                __instance.crouchSpeedPercentage = crouchSpeedMult.Value;
                __instance.zoomSpeedPercentage = zoomSpeedMult.Value;
                __instance.jumpSpeed = jumpSpeed.Value;

            }
        }
        private static int numJumps;
        //[HarmonyPatch(typeof(FPSRigidBodyWalker), "Update")]
        static class FPSRigidBodyWalker_FixedUpdate_Patch
        {
            static void Prefix(FPSRigidBodyWalker __instance, ref bool __state)
            {
                if (!enableMod.Value)
                    return;
                if (__instance.grounded)
                    numJumps = 0;
                if(__instance.InputComponent.jumpPress && multiJump.Value > numJumps)
                {
                    __instance.grounded = true;
                    __instance.jumping = false;
                    numJumps++;
                    __state = true;
                    Dbgl($"Multijump #{numJumps}");
                }

            }
            static void Postfix(FPSRigidBodyWalker __instance, bool __state)
            {
                if (!enableMod.Value || !__state)
                    return;
                Dbgl($"Post multijump #{numJumps}");
                __instance.grounded = false;

            }
        }
    }
}
