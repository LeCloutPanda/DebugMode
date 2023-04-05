using MelonLoader;
using UnityEngine;
using VampireSurvivors.Objects.Characters;
using VampireSurvivors.Input;
using VampireSurvivors.Framework;
using VampireSurvivors.UI;
using System;
using HarmonyLib;
using VampireSurvivors.Objects;
using System.CodeDom;
using VampireSurvivors.Objects.Weapons;
using Il2CppSystem.Collections.Generic;

namespace DebugMode
{
    public class DebugModeMod: MelonMod
    {
        static CharacterController characterController;
        static GameDebugInputManager debugInputManager;
        static GameManager gameManager;
        static OptionsController optionsController;

        static bool debugEnabled;

        private static Action<bool> setDebug;
        static void UpdateDebug(bool mode) => debugEnabled = mode;
        static bool debugSettingAdded = false;

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("Called from TestMod");
            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("dev.panda.debugmode");
            harmony.PatchAll();

            setDebug = UpdateDebug;
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
                    else if (Input.GetKeyDown(KeyCode.F)) 
                    {
                        foreach (Equipment item in characterController.WeaponsManager.ActiveEquipment)
                        {
                            item.TryCast<Weapon>().Fire();
                        }
                    }// CharacterControllerOnna.FireWeapons();
                    else if (Input.GetKeyDown(KeyCode.K)) debugInputManager.RosaryDamage();
                    else if (Input.GetKeyDown(KeyCode.G)) debugInputManager.MakeTreasure3();
                    else if (Input.GetKeyDown(KeyCode.V)) debugInputManager.Vacuum();
                    else if (Input.GetKeyDown(KeyCode.J)) MelonLogger.Msg("Need to implement, Unlock coffin character screen preview");
                    else if (Input.GetKeyDown(KeyCode.B)) debugInputManager.SpawnDestructables();
                    else if (Input.GetKeyDown(KeyCode.N)) gameManager.OpenMainArcana(); // Make it open the menu instead
                    else if (Input.GetKeyDown(KeyCode.M)) debugInputManager.ToggleMoveSpeed();

                    

                    gameManager.ZoomCamera(-Input.mouseScrollDelta.y, 0, Com.LuisPedroFonseca.ProCamera2D.EaseType.Linear);
                }
            }
        }

        // Grab the characterController controller
        [HarmonyPatch(typeof(CharacterController), nameof(CharacterController.Awake))]
        static class PatchcharacterControllerAwake { static void Postfix(CharacterController __instance) => characterController = __instance; }

        // Grab the debug manager
        [HarmonyPatch(typeof(GameDebugInputManager), nameof(GameDebugInputManager.Awake))]
        static class PatchDebugInputManager { static void Postfix(GameDebugInputManager __instance) => debugInputManager = __instance; }

        // Grab the debug manager
        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
        static class PatchGameManager { static void Postfix(GameManager __instance) => gameManager = __instance; }

        // Grab the debug manager
        [HarmonyPatch(typeof(OptionsController), nameof(OptionsController.BuildGameplayPage))]
        static class PatchOptionsController
        {
            static void Postfix(OptionsController __instance)
            {
                optionsController = __instance;
                if (!debugSettingAdded) optionsController.AddTickBox("Debug Mode", debugEnabled, setDebug, false);
            }
        }
    }
}
