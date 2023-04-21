using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MelonLoader;
using HarmonyLib;
using Il2CppVampireSurvivors.UI;
using Il2CppVampireSurvivors.Input;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Objects.Characters;
using UnityEngine;
using Il2CppVampireSurvivors.Objects.Weapons;
using Il2CppVampireSurvivors.Objects;
using Il2CppCom.LuisPedroFonseca.ProCamera2D;
using Il2CppVampireSurvivors.Data;

namespace DebugMode
{
    public class ConfigData
    {
        public bool Enabled { get; set; }
    }

    public static class ModInfo
    {
        public const string Name = "Debug Mode";
        public const string Description = "Unleash the power of debug mode.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.2";
        public const string DownloadLink = "https://github.com/LeCloutPanda/DebugMode";
    }

    public class DebugModeMod : MelonMod
    {
        static readonly string configFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");
        static readonly string filePath = Path.Combine(configFolder, "DebugMode.json");

        static readonly string enabledKey = "Enabled";
        static bool enabled;

        static void UpdateDebug(bool value) => UpdateEnabled(value);
        static bool debugSettingAdded = false;
        static System.Action<bool> debugSettingChanged = UpdateDebug;

        static CharacterController characterController;
        static GameDebugInputManager debugInputManager;
        static GameManager gameManager;
        static OptionsController optionsController;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.debugmode");
            harmony.PatchAll();

            ValidateConfig();
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (enabled)
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

                    gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, EaseType.Linear);
                }
            }
        }

        [HarmonyPatch(typeof(CharacterController), nameof(CharacterController.Awake))]
        static class PatchcharacterControllerAwake { static void Postfix(CharacterController __instance) => characterController = __instance; }

        [HarmonyPatch(typeof(GameDebugInputManager), nameof(GameDebugInputManager.Awake))]
        static class PatchDebugInputManager { static void Postfix(GameDebugInputManager __instance) => debugInputManager = __instance; }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.BuildGameplayPage))]
        static class PatchBuildGameplayPage
        {
            static void Postfix(OptionsController __instance)
            {
                optionsController = __instance;
                if (!debugSettingAdded) optionsController.AddTickBox("Debug Mode", enabled, debugSettingChanged, false);
                debugSettingAdded = true;
            }
        }

        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.AddVisibleJoysticks))]
        static class PatchAddVisibleJoysticks { static void Postfix() => debugSettingAdded = false; }

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
        private static void UpdateEnabled(bool value)
        {
            ModifyConfigValue(enabledKey, value);
            enabled = value;
        }

        private static void ValidateConfig()
        {
            try
            {
                if (!Directory.Exists(configFolder)) Directory.CreateDirectory(configFolder);
                if (!File.Exists(filePath)) File.WriteAllText(filePath, JsonConvert.SerializeObject(new ConfigData { }, Formatting.Indented));

                LoadConfig();
            }
            catch (System.Exception ex) { MelonLogger.Msg($"Error: {ex}"); }
        }

        private static void ModifyConfigValue<T>(string key, T value)
        {
            string file = File.ReadAllText(filePath);
            JObject json = JObject.Parse(file);

            if (!json.ContainsKey(key)) json.Add(key, JToken.FromObject(value));
            else
            {
                System.Type type = typeof(T);
                JToken newValue = JToken.FromObject(value);

                if (type == typeof(string)) json[key] = newValue.ToString();
                else if (type == typeof(int)) json[key] = newValue.ToObject<int>();
                else if (type == typeof(bool)) json[key] = newValue.ToObject<bool>();
                else { MelonLogger.Msg($"Unsupported type '{type.FullName}'"); return; }
            }

            string finalJson = JsonConvert.SerializeObject(json, Formatting.Indented);
            File.WriteAllText(filePath, finalJson);
        }

        private static void LoadConfig() => enabled = JObject.Parse(File.ReadAllText(filePath) ?? "{}").Value<bool>(enabledKey);
    }
}