using System;
using System.Runtime.InteropServices;
using System.IO;

using MelonLoader;
using HarmonyLib;

using VampireSurvivors;
using VampireSurvivors.Objects.Characters;
using VampireSurvivors.Input;
using VampireSurvivors.Framework;
using VampireSurvivors.UI;
using VampireSurvivors.Objects;
using VampireSurvivors.Objects.Weapons;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppSystem;
using MelonLoader.TinyJSON;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static Il2CppSystem.Globalization.CultureInfo;

namespace DebugMode
{
    public class ConfigData
    {
        public bool DebugEnabled { get; set; }
    }

    public class DebugModeMod: MelonMod
    {
        static CharacterController characterController;
        static GameDebugInputManager debugInputManager;
        static GameManager gameManager;
        static OptionsController optionsController;

        static PausePage pausePage;

        static GameObject pauseMenu;

        static bool debugEnabled;

        static void UpdateDebug(bool mode) => SaveConfig(mode);
        static bool debugSettingAdded = false;
        static System.Action<bool> debugSettingChanged = UpdateDebug;

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("Called from TestMod");
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.debugmode");
            harmony.PatchAll();

            ValidateConfig();
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (debugEnabled)
            {
                if (characterController != null && debugInputManager != null && gameManager != null)
                {
                    if (Input.GetKeyDown(KeyCode.X)) debugInputManager.LevelUp();
                    else if (Input.GetKeyDown(KeyCode.L)) characterController.PlayerOptions.AddCoins(1000);
                    else if (Input.GetKeyDown(KeyCode.H)) characterController.SetHealthToMax();
                    else if (Input.GetKeyDown(KeyCode.Z)) gameManager.DebugGiveAllWeapons();
                    else if (Input.GetKeyDown(KeyCode.I)) debugInputManager.ToggleInvulnerable();
                    else if (Input.GetKeyDown(KeyCode.T)) debugInputManager.NextMinute();
                    else if (Input.GetKeyDown(KeyCode.O)) characterController.Kill();
                    else if (Input.GetKeyDown(KeyCode.P)) debugInputManager.Pause();
                    else if (Input.GetKeyDown(KeyCode.E)) debugInputManager.SpawnMaxEnemies();
                    else if (Input.GetKeyDown(KeyCode.F)) foreach (Equipment item in characterController.WeaponsManager.ActiveEquipment) { item.TryCast<Weapon>().Fire(); }
                    else if (Input.GetKeyDown(KeyCode.K)) debugInputManager.RosaryDamage();
                    else if (Input.GetKeyDown(KeyCode.G)) debugInputManager.MakeTreasure3();
                    else if (Input.GetKeyDown(KeyCode.V)) debugInputManager.Vacuum();
                    else if (Input.GetKeyDown(KeyCode.J)) MelonLogger.Msg("Need to implement, Unlock coffin character screen preview");
                    else if (Input.GetKeyDown(KeyCode.B)) debugInputManager.SpawnDestructables();
                    else if (Input.GetKeyDown(KeyCode.N)) gameManager.OpenMainArcana();
                    else if (Input.GetKeyDown(KeyCode.M)) debugInputManager.ToggleMoveSpeed();

                    gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, Com.LuisPedroFonseca.ProCamera2D.EaseType.Linear);
                }
            }
        }

        // Get all required options
        [HarmonyPatch(typeof(CharacterController), nameof(CharacterController.Awake))]
        static class PatchcharacterControllerAwake { static void Postfix(CharacterController __instance) => characterController = __instance; }

        [HarmonyPatch(typeof(GameDebugInputManager), nameof(GameDebugInputManager.Awake))]
        static class PatchDebugInputManager { static void Postfix(GameDebugInputManager __instance) => debugInputManager = __instance; }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.BuildGameplayPage))]
        static class PatchBuildGameplayPage
        {
            [HarmonyPostfix]
            static void Postfix(OptionsController __instance)
            {
                optionsController = __instance;
                MelonLogger.Msg("Adding Debug Mode button");
                if (!debugSettingAdded) optionsController.AddTickBox("Debug Mode", debugEnabled, debugSettingChanged, false);
                debugSettingAdded = true;
            }
        }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.AddVisibleJoysticks))]
        static class PatchAddVisibleJoysticks
        {
            [HarmonyPostfix]
            static void Postfix() => debugSettingAdded = false;
        }


        static string fileName = "config.json";
        static string configDirectory = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, fileName);

        private static void ValidateConfig()
        {
            try
            {

                MelonLogger.Msg("Validating Config file");
                if (!File.Exists(configDirectory))
                {
                    MelonLogger.Msg("Creating Config file");
                    CreateConfig(configDirectory);
                }
                else
                {
                    MelonLogger.Msg("Checking Config file for invalid items");
                    var data = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configDirectory));
                    if (data.DebugEnabled == null)
                    {
                        MelonLogger.Msg("Values are incorrect creating Config file");
                        CreateConfig(configDirectory);
                    }
                    MelonLogger.Msg("Loading Config file");
                    LoadConfig();
                    MelonLogger.Msg("Loaded Config file");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Msg($"Error: {ex}");
            }
        }

        private static void LoadConfig() => debugEnabled = JObject.Parse(File.ReadAllText(configDirectory) ?? "{}").Value<bool>("DebugEnabled");

        private static void SaveConfig(bool newDebugValue)
        {
            try
            {
                MelonLogger.Msg("Saving Config file");
                var data = JsonConvert.DeserializeObject<ConfigData>(File.ReadAllText(configDirectory));
                debugEnabled = data.DebugEnabled = newDebugValue;
                File.WriteAllText(configDirectory, JsonConvert.SerializeObject(data, Formatting.Indented));
                MelonLogger.Msg("Saved Config file");
            }
            catch(System.Exception ex)
            {
                MelonLogger.Msg($"Error: {ex}");
            }

        }

        private static void CreateConfig(string path)
        {
            try {
                File.WriteAllText(path, JsonConvert.SerializeObject(new ConfigData { DebugEnabled = false }, Formatting.Indented));
            }
            catch (System.Exception ex)
            {
                MelonLogger.Msg($"Error: {ex}");
            }
        }
    }
}