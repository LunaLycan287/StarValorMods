using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Linq;

namespace LL_SV_MoreSkillPoints
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class MoreSkillPoints : BaseUnityPlugin
    {
        public const string pluginGuid = "lunalycan287.starvalormods.moreskillpoints";
        public const string pluginName = "More Skill Points";
        public const string pluginVersion = "1.1.0";

        static ManualLogSource logSource = new ManualLogSource("(LL) MoreSkillPoints");

        private static ConfigEntry<int> maxSkillPoints;
        private static ConfigEntry<bool> ignoreResetPoints;
        private static ConfigEntry<bool> uninstall;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(MoreSkillPoints));
            LoadConfig();
            BepInEx.Logging.Logger.Sources.Add(logSource);
            logSource.LogInfo("Loaded " + pluginName + " v" + pluginVersion);
            logSource.LogDebug("MaxSP: " + maxSkillPoints.Value + " IgnoreReset: " + ignoreResetPoints.Value + " Uninstall: " + uninstall.Value);
        }

        private void LoadConfig()
        {
            maxSkillPoints = Config.Bind<int>("General Settings", "MaxSkillPoints", 125, "The amount of skill points at max level.");
            ignoreResetPoints = Config.Bind<bool>("General Settings", "InfiniteResets", false, "If resseting should not cost any reset points.");
            uninstall = Config.Bind<bool>("Uninstall", "Uninstall", false, "If this is set to true, changes by this mod will be reset. \n" +
                "After opening your skill screen and saving your game you will have normal skill points again.");
        }

        [HarmonyPatch(typeof(PlayerCharacter), "LevelUP")]
        [HarmonyPostfix]
        private static void PlayerCharacter_LevelUP_Post()
        {
            SetSkillPoints();
        }

        [HarmonyPatch(typeof(CharacterScreen), "ResetSkills")]
        [HarmonyPrefix]
        private static void CharacterScreen_ResetSkills_Pre()
        {
            if (ignoreResetPoints.Value || uninstall.Value)
            {
                PChar.Char.resetSkillsPoints++;
                logSource.LogDebug("Increased reset points.");
            }
        }
        
        [HarmonyPatch(typeof(PlayerCharacter), "ResetSkills")]
        [HarmonyPostfix]
        private static void PlayerCharacter_ResetSkills_Post()
        {
            SetSkillPoints();
        }

        [HarmonyPatch(typeof(CharacterScreen), "ShowData")]
        [HarmonyPostfix]
        private static void CharacterScreen_ShowData_Post()
        {
            SetSkillPoints();
        }

        private static void SetSkillPoints()
        {
            if (uninstall.Value)
            {
                return;
            }

            PlayerCharacter player = PChar.Char;

            float maxSP = (float)maxSkillPoints.Value / PChar.maxLevel;
            float curSkillPoints = maxSP * (float)player.level;
            int usedSP = 0;
            int maxSkillTree = Enum.GetValues(typeof(SkillTree)).Cast<int>().Max();
            for (int i = 0; i <= maxSkillTree; i++)
            {
                usedSP += player.GetSkillTreeLevel(i);
            }
            player.skillPoints = (int)curSkillPoints - usedSP;
        }

    }
}
