using HarmonyLib;
using Il2CppCom.LuisPedroFonseca.ProCamera2D;
using Il2CppSystem.Collections.Generic;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Objects;
using Il2CppVampireSurvivors.Objects.Weapons;
using Il2CppVampireSurvivors.Tools;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using UnityEngine;

namespace DebugMode
{
    public class ConfigData
    {
        public string Name = ModInfo.Name;
        public bool EnableDebugMode = false;
        public bool UseShiftKeyForZoom = false;
        public bool UseShiftForBinds = false;
        public bool ShowBindsHelper = false;
    }

    public static class ModInfo
    {
        public const string Name = "Debug Mode";
        public const string Description = "Unleash the power of debug mode.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.6.0";
        public const string DownloadLink = "https://github.com/LeCloutPanda/DebugMode";
    }

    public class DebugModeMod : MelonMod
    {
        static readonly string configFolder = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Configs");
        static readonly string filePath = Path.Combine(configFolder, "DebugMode.json");

        public static ConfigData config = new ConfigData();

        static GameManager gameManager;
        static Cheats cheatsManager;

        public override void OnInitializeMelon()
        {
            base.OnInitializeMelon();

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.debugmode");
            harmony.PatchAll();

            ValidateConfig();

        }

        public List<R> Select<T, R>(List<T> list, Func<T, R> expr)
        {
            List<R> ret = new List<R>();
            foreach (T item in list)
            {
                ret.Add(expr(item));
            }
            return ret;
        }
        public List<T> Where<T>(List<T> list, Func<T, bool> expr)
        {
            List<T> ret = new List<T>();
            foreach (T item in list)
            {
                if (expr(item)) ret.Add(item);
            }
            return ret;
        }

        public List<WeaponType> GetEquippedAccessoryTypes() { return Select(gameManager.Player.AccessoriesManager.ActiveEquipment, (e) => e._equipmentType); }

        public List<WeaponType> GetAllAccessoryTypes()
        {
            List<WeaponType> Accessories = new List<WeaponType>();
            foreach (KeyValuePair<WeaponType, Il2CppNewtonsoft.Json.Linq.JArray> entry in gameManager.DataManager.AllWeaponData)
            {
                // Currently only Accessories have the "isPowerUp" json member, but just to make sure check it's value.
                if (entry.Value.First.SelectToken("isPowerUp") != null && (bool)entry.Value.First.SelectToken("isPowerUp"))
                {
                    Accessories.Add(entry.Key);
                }
            }
            return Accessories;
        }

        private void GiveAllAccessories()
        {
            Action<WeaponType> addAccesory = (type) => gameManager.AccessoriesFacade.AddAccessory(type, gameManager.Player);
            Where(GetAllAccessoryTypes(), (type) => !GetEquippedAccessoryTypes().Contains(type)).ForEach(addAccesory);
        }

        private void GiveAllWeapons()
        {
            Action<WeaponType> addWeapon = (type) => gameManager.WeaponsFacade.AddWeapon(type, gameManager.Player);

            foreach (WeaponType type in gameManager.WeaponsFacade._weaponFactory._weapons.Keys)
            {
                addWeapon(type);
            }
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
                if (gameManager != null && cheatsManager != null) 
                {
                    if (config.UseShiftForBinds) if (!Input.GetKey(KeyCode.LeftShift)) return;

                    if (Input.GetKeyDown(KeyCode.X)) cheatsManager.ForceLevelUp(); // Works
                    else if (Input.GetKeyDown(KeyCode.L)) gameManager.Player.PlayerOptions.AddCoins(1000); // Works
                    else if (Input.GetKeyDown(KeyCode.H)) gameManager.Player.SetHealthToMax(); // Works
                    else if (Input.GetKeyDown(KeyCode.Z)) gameManager.DebugGiveAllWeapons(); // Works
                    else if (Input.GetKeyDown(KeyCode.C)) GiveAllAccessories();
                    else if (Input.GetKeyDown(KeyCode.Semicolon)) GiveAllWeapons();
                    else if (Input.GetKeyDown(KeyCode.I)) gameManager.Player.Debug_ToggleInvulnerability(); // Works
                    else if (Input.GetKeyDown(KeyCode.T)) gameManager.Stage.DebugNextMinute(); // Works
                    else if (Input.GetKeyDown(KeyCode.O)) gameManager.Player.Kill(); // Works
                    else if (Input.GetKeyDown(KeyCode.P)) MelonLogger.Msg("Not implemented yet: Trigger Screen Freeze");
                    else if (Input.GetKeyDown(KeyCode.E)) gameManager.Stage.DebugSpawnMaxEnemies(); // Works
                    else if (Input.GetKeyDown(KeyCode.F)) foreach (Equipment item in gameManager.Player.WeaponsManager.ActiveEquipment) { item.TryCast<Weapon>().Fire(); } // Works
                    else if (Input.GetKeyDown(KeyCode.K)) gameManager.RosaryDamage(); // Works
                    else if (Input.GetKeyDown(KeyCode.G)) cheatsManager.ForceTreasure(3); // Works
                    else if (Input.GetKeyDown(KeyCode.V)) gameManager.TurnOnVacuum(); // Works
                    else if (Input.GetKeyDown(KeyCode.J) && setCharacterPreview(gameManager.Stage.ActiveStageData.stageName) != CharacterType.VOID) gameManager.AddCharacterTypeToQueue(setCharacterPreview(gameManager.Stage.ActiveStageData.stageName)); // Works
                    else if (Input.GetKeyDown(KeyCode.B)) gameManager.Stage.DebugSpawnDestructibles(); // works
                    else if (Input.GetKeyDown(KeyCode.N)) gameManager.OpenMainArcana(); // works
                    else if (Input.GetKeyDown(KeyCode.M)) MelonLogger.Msg("Not implemented yet: Toggle Move Speed");
                    else if (Input.GetKeyDown(KeyCode.R)) gameManager.MakeStagePickup(gameManager.Player.CurrentPos, ItemType.RELIC_GOLDENEGG); // works
                    else if (Input.GetKeyDown(KeyCode.Y)) gameManager.AddWeapon(WeaponType.CANDYBOX); // works
                    else if (Input.GetKeyDown(KeyCode.U)) gameManager.AddWeapon(WeaponType.CANDYBOX2); // works
                    else if (Input.GetKeyDown(KeyCode.Q)) gameManager.TriggerGoldFever(10000f); // works

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
        public override void OnGUI()
        {
            base.OnGUI();
            var binds =
                "X: Level up\n" +
                "L: Add 1000 coins\n" +
                "H: Heal player\n" +
                "Z: Give all equipment\n" +
                "C: Give all accessories\n" +
                "Semicolon(;): Give all weapons\n" +
                "I: Toggle invulnerability\n" +
                "T: Advance timer forward a minute\n" +
                "O: Kill player\n" +
                "P: Toggle freeze (not working)\n" +
                "E: Spawn max enemies\n" +
                "F: Fire all weapons\n" +
                "K: Trigger rosary damage\n" +
                "G: Spawn max level chest\n" +
                "V: Vacuum\n" +
                "J: Open level coffin\n" +
                "B: Spawn destructables\n" +
                "N: Open aranca menu\n" +
                "M: Toggle Movement speed (not working)\n" +
                "R: Spawn golden egg\n" +
                "Y: Give normal candybox\n" +
                "U: Give evolved candybox\n" +
                "Q: Start gold fever";

            if (config.ShowBindsHelper)
            {
                GUI.Box(new Rect(10, Screen.height / 2 - (400 / 2) - 10, 220, 340), "");
                GUI.Label(new Rect(20, Screen.height / 2 - (400 / 2), 200, 330), binds);
            }
        }

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        [HarmonyPatch(typeof(Cheats), nameof(Cheats.Construct))]
        static class PatchCheats { static void Postfix(Cheats __instance) => cheatsManager = __instance; }

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
            config.UseShiftForBinds = (bool)json.GetValue("UseShiftForBinds");
            config.ShowBindsHelper = (bool)json.GetValue("ShowBindsHelper");
        }
    }
}