﻿using HarmonyLib;
using Il2CppCom.LuisPedroFonseca.ProCamera2D;
using Il2CppVampireSurvivors.Data;
using Il2CppVampireSurvivors.Framework;
using Il2CppVampireSurvivors.Objects;
using Il2CppVampireSurvivors.Objects.Weapons;
using Il2CppVampireSurvivors.Tools;
using MelonLoader;
using UnityEngine;
using VSMenuModHelper;
using static Il2CppSystem.Linq.Expressions.Interpreter.InitializeLocalInstruction;

namespace DebugMode.src
{
    public static class ModInfo
    {
        public const string Name = "Debug Mode";
        public const string Description = "Unleash the power of debug mode.";
        public const string Author = "LeCloutPanda";
        public const string Company = "Pandas Hell Hole";
        public const string Version = "1.0.7.2";
        public const string DownloadLink = "https://github.com/LeCloutPanda/DebugMode";
    }

    public class DebugMode : MelonMod
    {
        private static MelonPreferences_Category preferences; 
        private static MelonPreferences_Entry<bool> enabled;
        private static MelonPreferences_Entry<bool> useShiftForZoom;
        private static MelonPreferences_Entry<bool> useShiftForBinds;

        private static GameManager gameManager;
        private static Cheats cheatsManager;

        public override void OnInitializeMelon()
        {
            preferences = MelonPreferences.CreateCategory("debugmode_preferences");
            enabled = preferences.CreateEntry("enabled", true);
            useShiftForZoom = preferences.CreateEntry("useShiftForZoom", true);
            useShiftForBinds = preferences.CreateEntry("useShiftForBinds", true);


            //DeclareMenuTabs();
        }

        private void DeclareMenuTabs()
        {
            try
            {
                Uri iconUrl = new Uri("https://github.com/LeCloutPanda/DebugMode/blob/main/debugmode.png?raw=true");
                VSMenuHelper.Instance.DeclareOptionsTab("DebugeMode", iconUrl);

                VSMenuHelper.Instance.AddTabSpriteModifier("DebugeMode", (sprite) => {
                    sprite.texture.filterMode = UnityEngine.FilterMode.Point;
                    return sprite;
                });

                VSMenuHelper.Instance.AddElementToTab("DebugeMode", new Title("Debug Mode"));
                VSMenuHelper.Instance.AddElementToTab("DebugeMode", new TickBox("enabled", () => enabled.Value, (value) => enabled.Value = value));
                VSMenuHelper.Instance.AddElementToTab("Debug_Mode", new TickBox("useShiftForZoom", () => useShiftForZoom.Value, (value) => useShiftForZoom.Value = value));
                VSMenuHelper.Instance.AddElementToTab("Debug_Mode", new TickBox("useShiftForBinds", () => useShiftForBinds.Value, (value) => useShiftForBinds.Value = value));
            }
            catch (Exception e)
            {
                Debug.Log("Error when creating config panel: " + e);
            }

        }

        public Il2CppSystem.Collections.Generic.List<R> Select<T, R>(Il2CppSystem.Collections.Generic.List<T> list, Func<T, R> expr)
        {
            Il2CppSystem.Collections.Generic.List<R> ret = new Il2CppSystem.Collections.Generic.List<R>();
            foreach (T item in list)
            {
                ret.Add(expr(item));
            }
            return ret;
        }
        public Il2CppSystem.Collections.Generic.List<T> Where<T>(Il2CppSystem.Collections.Generic.List<T> list, Func<T, bool> expr)
        {
            Il2CppSystem.Collections.Generic.List<T> ret = new Il2CppSystem.Collections.Generic.List<T>();
            foreach (T item in list)
            {
                if (expr(item)) ret.Add(item);
            }
            return ret;
        }

        public Il2CppSystem.Collections.Generic.List<WeaponType> GetEquippedAccessoryTypes() { return Select(gameManager.Player.AccessoriesManager.ActiveEquipment, (e) => e._equipmentType); }

        public Il2CppSystem.Collections.Generic.List<WeaponType> GetAllAccessoryTypes()
        {
            Il2CppSystem.Collections.Generic.List<WeaponType> Accessories = new Il2CppSystem.Collections.Generic.List<WeaponType>();
            foreach (Il2CppSystem.Collections.Generic.KeyValuePair<WeaponType, Il2CppNewtonsoft.Json.Linq.JArray> entry in gameManager.DataManager.AllWeaponData)
            {
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

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();

            if (enabled.Value)
            {
                if (gameManager != null && cheatsManager != null)
                {
                    if (useShiftForBinds.Value) if (!Input.GetKey(KeyCode.LeftShift)) return;

                    if (Input.GetKeyDown(KeyCode.X)) cheatsManager.ForceLevelUp();
                    else if (Input.GetKeyDown(KeyCode.L)) gameManager.Player.PlayerOptions.AddCoins(1000);
                    else if (Input.GetKeyDown(KeyCode.H)) gameManager.Player.SetHealthToMax();
                    else if (Input.GetKeyDown(KeyCode.Z)) gameManager.DebugGiveAllWeapons();
                    else if (Input.GetKeyDown(KeyCode.C)) GiveAllAccessories();
                    else if (Input.GetKeyDown(KeyCode.Semicolon)) GiveAllWeapons();
                    else if (Input.GetKeyDown(KeyCode.I)) gameManager.Player.Debug_ToggleInvulnerability();
                    else if (Input.GetKeyDown(KeyCode.T)) gameManager.Stage.DebugNextMinute();
                    else if (Input.GetKeyDown(KeyCode.O)) gameManager.Player.Kill();
                    else if (Input.GetKeyDown(KeyCode.P)) gameManager.StopTimeForMilliseconds(10000);
                    else if (Input.GetKeyDown(KeyCode.E)) gameManager.Stage.DebugSpawnMaxEnemies();
                    else if (Input.GetKeyDown(KeyCode.F)) foreach (Equipment item in gameManager.Player.WeaponsManager.ActiveEquipment) { item.TryCast<Weapon>().Fire(); }
                    else if (Input.GetKeyDown(KeyCode.K)) gameManager.RosaryDamage();
                    else if (Input.GetKeyDown(KeyCode.G)) cheatsManager.ForceTreasure(3);
                    else if (Input.GetKeyDown(KeyCode.V)) gameManager.TurnOnVacuum();
                    else if (Input.GetKeyDown(KeyCode.J) && setCharacterPreview(gameManager.Stage.ActiveStageData.stageName) != CharacterType.VOID) gameManager.AddCharacterTypeToQueue(setCharacterPreview(gameManager.Stage.ActiveStageData.stageName), gameManager.Player);
                    else if (Input.GetKeyDown(KeyCode.B)) gameManager.Stage.DebugSpawnDestructibles();
                    else if (Input.GetKeyDown(KeyCode.N)) gameManager.OpenMainArcana();
                    //else if (Input.GetKeyDown(KeyCode.M)) MelonLogger.Msg("Not implemented yet: Toggle Move Speed");
                    //else if (Input.GetKeyDown(KeyCode.R)) gameManager.MakeStagePickup(gameManager.Player.CurrentPos, ItemType.RELIC_GOLDENEGG);
                    else if (Input.GetKeyDown(KeyCode.Y)) gameManager.AddWeapon(WeaponType.CANDYBOX, gameManager.Player);
                    else if (Input.GetKeyDown(KeyCode.U)) gameManager.AddWeapon(WeaponType.CANDYBOX2, gameManager.Player);
                    else if (Input.GetKeyDown(KeyCode.Q)) gameManager.TriggerGoldFever(10000f);

                    if (useShiftForZoom.Value && Input.GetKey(KeyCode.LeftShift)) gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, EaseType.Linear);
                    else if (!useShiftForZoom.Value) gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, EaseType.Linear);
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

        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        [HarmonyPatch(typeof(Cheats), nameof(Cheats.Construct))]
        static class PatchCheats { static void Postfix(Cheats __instance) => cheatsManager = __instance; }
    }
}