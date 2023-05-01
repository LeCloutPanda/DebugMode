using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.Input;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Objects.Characters;
using UnityEngine;
using Il2CppVampireSurvivors.Objects.Weapons;
using Il2CppVampireSurvivors.Objects;
using Il2CppCom.LuisPedroFonseca.ProCamera2D;
using System;
using Il2CppVampireSurvivors.Data;

namespace DebugMode
{
    public class ConfigData
    {
        public string Name = ModInfo.Name;
        public bool EnableDebugMode = false;
        public bool UseShiftKeyForZoom = false;
    }
    
    public static class ModInfo
    {
        public const string Name = "Debug Mode";
        public const string Description = "Unleash the power of debug mode.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.4";
        public const string DownloadLink = "https://github.com/LeCloutPanda/DebugMode";
    }

    public class DebugModeMod : MelonMod
    {
        static readonly string configFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");
        static readonly string filePath = Path.Combine(configFolder, "DebugMode.json");

        public static ConfigData config = new ConfigData();

        static CharacterController characterController;
        static GameDebugInputManager debugInputManager;
        static GameManager gameManager;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.debugmode");
            harmony.PatchAll();

            ValidateConfig();
        }

        DateTime lastModified;

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (lastModified != File.GetLastWriteTime(filePath))
            {
                lastModified = File.GetLastWriteTime(filePath);
                LoadConfig();
                MelonLogger.Msg($"[{lastModified.ToString("HH:mm:ss")}] Reloading Config for {ModInfo.Name}");
            }

            if (config.EnableDebugMode)
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
                    else if (Input.GetKeyDown(KeyCode.J) && setCharacterPreview(gameManager.Stage.ActiveStageData.stageName) != CharacterType.VOID) gameManager.AddCharacterTypeToQueue(setCharacterPreview(gameManager.Stage.ActiveStageData.stageName));
                    else if (Input.GetKeyDown(KeyCode.B)) debugInputManager.SpawnDestructables();
                    else if (Input.GetKeyDown(KeyCode.N)) gameManager.OpenMainArcana();
                    else if (Input.GetKeyDown(KeyCode.M)) debugInputManager.ToggleMoveSpeed();

                    if (config.UseShiftKeyForZoom && Input.GetKey(KeyCode.LeftShift)) gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, EaseType.Linear);
                    else if (!config.UseShiftKeyForZoom) gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, EaseType.Linear);
                }
            }
        }

        private static CharacterType setCharacterPreview(string level)
        {
            CharacterType characterType = CharacterType.VOID;
            if (level == "Mad Forest") characterType = CharacterType.PUGNALA;
            else if (level == "Inlaid Library") characterType = CharacterType.GIOVANNA;
            else if (level == "Dairy Plant") characterType = CharacterType.POPPEA;
            else if (level == "Gallo Tower") characterType = CharacterType.CONCETTA;
            else if (level == "Cappella Magna") characterType = CharacterType.ASSUNTA;
            else if (level == "Mt.Moonspell") characterType = CharacterType.MIANG;
            else if (level == "Lake Foscari") characterType = CharacterType.ELEANOR;

            return characterType;
        }

        [HarmonyPatch(typeof(CharacterController), nameof(CharacterController.Awake))]
        static class PatchcharacterControllerAwake { static void Postfix(CharacterController __instance) => characterController = __instance; }

        [HarmonyPatch(typeof(GameDebugInputManager), nameof(GameDebugInputManager.Awake))]
        static class PatchDebugInputManager { static void Postfix(GameDebugInputManager __instance) => debugInputManager = __instance; }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        private static void ValidateConfig()
        {
            try
            {
                if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
                if (!File.Exists(filePath)) File.WriteAllText(filePath, JsonConvert.SerializeObject(new ConfigData { }, Formatting.Indented));

                LoadConfig();
            }
            catch (System.Exception ex) { MelonLogger.Error($"Error validating Config: {ex}"); }
        }

        private static void LoadConfig()
        {
            JObject json = JObject.Parse(File.ReadAllText(filePath) ?? "{}");

            config.Name = (string)json.GetValue("Name");
            config.EnableDebugMode = (bool)json.GetValue("EnableDebugMode");
            config.UseShiftKeyForZoom = (bool)json.GetValue("UseShiftKeyForZoom");
        }
    }
}