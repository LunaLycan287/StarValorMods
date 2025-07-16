using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LL_SV_Dumper;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace LL_SV_PerkUnlockHelper {
    
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class PerkUnlockHelper : BaseUnityPlugin {
        private const string PluginGuid = "lunalycan287.starvalormods.perkunlockhelper";
        private const string PluginName = "Perk Unlock Helper";
        private const string PluginVersion = "1.0.0";

        private static readonly ManualLogSource LOGSource = new ManualLogSource("(LL) " + PluginName.Replace(" ",""));

        private static GameObject _unlockPerksPanel;
        private static bool _test;

        private static ConfigEntry<string> _keybind;
        private static ConfigEntry<bool> _showImages;
        private static ConfigEntry<bool> _showHiddenUnlockConditions;
        
        public void Awake() {
            Harmony.CreateAndPatchAll(typeof(PerkUnlockHelper));
            BepInEx.Logging.Logger.Sources.Add(LOGSource);
            LoadConfig();
            LOGSource.LogInfo("Loaded " + PluginName + " v" + PluginVersion);
        }
        
        private void LoadConfig()
        {
            _keybind = Config.Bind<string>("General Settings", "KeyBind", "f8", "The Key Code to use. (Default: f8) See https://docs.unity3d.com/6000.1/Documentation/ScriptReference/KeyCode.html for possible values. NOTE: names must be all lowercase!");
            _showImages = Config.Bind<bool>("General Settings", "ShowImages", true, "If the image of the Perk should be shown instead of the placeholder.");
            _showHiddenUnlockConditions = Config.Bind<bool>("General Settings", "ShowHiddenUnlockConditions", false, "If the hidden unlock conditions should be shown.");
        }

        private static void FillPerksHelperPanel(PerksPanel __instance) {
            LOGSource.LogDebug("FillPerksHelperPanel");
            Transform panel = __instance.transform.Find("Panel");
            Text title = __instance.transform.Find("Title").GetComponent<Text>();

            int totalPerks = PerkDB.totalPerks;
            int i = 0;
            for (int j = 0; j < totalPerks; j++) {
                Perk byIndex = PerkDB.GetByIndex(j);
                if (!byIndex.locked || (byIndex.statMode == 2 && GameData.data.difficulty == 2) || (GameData.data.difficulty == -1 && !byIndex.unlockOnRelaxedMode) || byIndex.id == 323) {
                    continue;
                }

                if (_showHiddenUnlockConditions.Value && byIndex.showLevel < 2) {
                    byIndex.showLevel = 2;
                }
                
                if (i >= panel.childCount)
                {
                    Instantiate<GameObject>( __instance.perkGO, panel);
                }

                PerkControl perkControl = panel.GetChild(i).GetComponent<PerkControl>();
                perkControl.Setup(byIndex, __instance, null, false, null);
                

                Color? bgColor = GetPerkColor(byIndex);
                if (bgColor != null) {
                    perkControl.bgColor = (Color)bgColor;
                    perkControl.transform.Find("BG").GetComponent<Image>().color = perkControl.bgColor;
                }

                if (_showImages.Value) {
                    perkControl.transform.Find("Image").GetComponent<Image>().sprite = byIndex.image;
                }

                panel.GetChild(i).gameObject.SetActive(true);
                i++;
            }
            //__instance.AdjustPanelSize(i);
            while (i < panel.childCount)
            {
                panel.GetChild(i).gameObject.SetActive(false);
                i++;
            }

            title.text = "Locked Perks";
        }

        private static bool IsQuestCompleted(Quests quest, PlayerControl pc) {
            return QuestDB.IsQuestCompleted(QuestDB.GetQuestRef((int)quest), pc.transform);
        }

        private static Color? GetPerkColor(Perk perk) {
            Color? bgColor = null;
            PlayerControl pc = PlayerControl.inst;
            BaseCharacter character = GameData.data.character;
            switch ((Perks)perk.id) {
                case Perks.Miner:
                    if (!HasPerk(Perks.Outis)) {
                        bgColor = Color.red;
                    }
                    break;
                case Perks.Trader:
                    if (!HasPerk(Perks.Miner)) {
                        bgColor = Color.red;
                    }
                    break;
                case Perks.Scoundrel:
                case Perks.Lone_Wolf:
                    if (IsQuestCompleted(Quests.Old_friends_new_enemies_160, pc)) {
                        bgColor = Color.red;
                    }
                    break;
                case Perks.Pirate:
                case Perks.Indoctrinated:
                    if (!HasPerk(Perks.Outis) && !HasPerk(Perks.Miner) && !HasPerk(Perks.Trader)) {
                        bgColor = Color.red;
                    }
                    break;
                case Perks.Battle_Rush:
                    if (PChar.Char.level >= 10) {
                        bgColor = Color.red;
                    } else if (PChar.Char.level >= 8) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Combat_Genius:
                    if (pc.mercenaries.Count > 0) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Guardian_Angel:
                    if (PChar.HasPerk(5)) {
                        bgColor = Color.red;
                    }
                    break;
                case Perks.Dogfighter:
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Corvette) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Battleship_Raid:
                case Perks.Hoarder:
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Yacht) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Techie:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Tech, character);
                    break;
                case Perks.Ace:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Fighter, character);
                    break;
                case Perks.Just_me_and_the_boys:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.FleetCommander, character);
                    break;
                case Perks.Hard_Worker:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Geology, character);
                    break;
                case Perks.Traveler:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Explorer, character);
                    break;
                case Perks.Early_Supporter:
                    bgColor = Color.red;  // Can never get since time based   
                    break;
                case Perks.Self_Reliant:
                    if (PChar.Char.level >= 10 || GameData.HasDeed("BoughtEquipmentOrShip") || PChar.HasAnyRepOverValue(999)) {
                        bgColor = Color.red;
                    } else if (PChar.Char.level >= 8 || PChar.HasAnyRepOverValue(888)) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Contractor:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Construction, character);
                    break;
            }

            return bgColor;
        }
        
        private static bool HasPerk(Perks perk) {
            return PChar.HasPerk((int)perk);   
        }

        private static Color? GetPerkColorKnowledge(Knowledge.Type knowledge, BaseCharacter character) {
            Color? bgColor = null;
            if (Knowledge.GetValue(knowledge, character) == 25 && Knowledge.HasAnyKnowledgeIn(25, knowledge, character)) {
                bgColor = Color.red;
            }
            else if (Knowledge.HasAnyKnowledgeIn(23, knowledge, character)) {
                bgColor = Color.yellow;
            }
            return bgColor;
        }
        


        [HarmonyPatch(typeof(PlayerControl), "Update")]
        [HarmonyPostfix]
        private static void PCUpdate_Post(PlayerControl __instance) {
            if (Input.GetKeyDown(_keybind.Value)) {
                LOGSource.LogDebug("PCUpdate");
                CharacterScreen charScreen = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<PlayerUIControl>().transform.parent.Find("Character").GetComponent<CharacterScreen>();
                if (charScreen != null) {
                    charScreen.OpenClose(271828);
                }
            }
            if (_test == true) {
                LOGSource.LogDebug("PCUpdate test");
                _test = false;
            }
        }
        
        
        [HarmonyPatch(typeof(CharacterScreen), "CloseSubPanels")]
        [HarmonyPostfix]
        private static void CSCloseSubPanels_Post(CharacterScreen __instance) {
            if (_unlockPerksPanel != null) {
                _unlockPerksPanel.SetActive(false);
            }
        }
        
        
        [HarmonyPatch(typeof(CharacterScreen), nameof(CharacterScreen.Open))]
        [HarmonyPostfix]
        private static void CSOpen_Post(CharacterScreen __instance, int mode) {
            LOGSource.LogDebug("CSOpen_Post: " + mode);
            if (mode == 271828) {
                InstantiatePerksHelperPanel(__instance);
                _unlockPerksPanel.SetActive(true);
                FillPerksHelperPanel(_unlockPerksPanel.GetComponent<PerksPanel>());
                //_unlockPerksPanel.GetComponent<PerksPanel>().ShowCharPerks();
            }
        }
        
        private static void InstantiatePerksHelperPanel(CharacterScreen __instance)
        {
            LOGSource.LogDebug("InstantiatePerksHelperPanel");
            if (_unlockPerksPanel == null) {
                _unlockPerksPanel = Instantiate<GameObject>(__instance.perksPanelRef, __instance.transform);
                _unlockPerksPanel.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0f, -58.61f, 0f);
                _unlockPerksPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1100f, 522f);
                _unlockPerksPanel.transform.Find("BtnClose").gameObject.SetActive(false);
                _unlockPerksPanel.SetActive(false);
            }
        }

        
        [HarmonyPatch(typeof(Perk), "GetHowToUnlock")]
        [HarmonyPostfix]
        private static void PGetHowToUnlock_Post(Perk __instance, ref string __result) {
            if (__instance.locked && __instance.showLevel >= 2) {
                string unlockProgress = GetUnlockProgress(__instance);
                if (!unlockProgress.IsNullOrWhiteSpace()) {
                    __result = __result +"\n\n"+ ColorSys.infoText3 + GetUnlockProgress(__instance)  +  "</color>";
                }
            }
        }

        private static string GetUnlockProgress(Perk perk) {
            string result = "";
            PlayerControl pc = PlayerControl.inst;
            BaseCharacter character = GameData.data.character;
            switch ((Perks)perk.id) {
                case Perks.Techie:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.Tech, character);
                    break;
                case Perks.Ace:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.Fighter, character);
                    break;
                case Perks.Just_me_and_the_boys:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.FleetCommander, character);
                    break;
                case Perks.Hard_Worker:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.Geology, character);
                    break;
                case Perks.Traveler:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.Explorer, character);
                    break;
                case Perks.Contractor:
                    result = GetKnowledgeUnlockProgress(Knowledge.Type.Construction, character);
                    break;
                case Perks.Acquired_Wisdom:
                    result = character.level + "/50";
                    break;
                case Perks.O_C_D_:
                    result = GameData.data.GetDeedCount("FullyExploredSector") + "/10";
                    break;
                case Perks.The_Perfect_Predator:
                    result = GameData.data.GetDeedCount("AmbushAttack") + "/5";
                    break;
                case Perks.Battleship_Raid:
                    result = GameData.data.GetDeedCount("DefeatedBossWithYachtOrShuttle") + "/5";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Yacht) {
                        result += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        result += "\nEither Shuttle or Yacht required.";
                    }
                    break;
                case Perks.Sloppy:
                    result = GameData.data.GetDeedCount("Sloppy") + "/6";
                    break;
                case Perks.Aggressive:
                    result = GameData.data.GetDeedCount("DestroyedIndSyndPMCShip") + "/3";
                    break;
                case Perks.Marauder:
                    result = GameData.data.GetDeedCount("DestroyedIndSyndPMCShip") + "/20";
                    break;
                case Perks.Temperate:
                    result = GameData.data.GetDeedCount("AvoidedIndSyndPMCShip") + "/10";
                    break;
                case Perks.Guardian_Angel:
                    result = GameData.data.GetDeedCount("SavedIndSyndPMCShip") + "/10";
                    if (HasPerk(Perks.Pirate)) {
                        result += "\n" + ColorSys.infoNeg + "Get rid of Pirate Perk!</color>";
                    }
                    break;
                case Perks.Anaximander:
                    result = GameData.data.GetDeedCount("ExploredNewSector") + "/100";
                    break;
                case Perks.Dogfighter:
                    result = GameData.data.GetDeedCount("DogfightWin") + "/100";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Corvette) {
                        result += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        result += "Either Shuttle, Yacht or Corvette required.";
                    }
                    break;
                case Perks.Space_Janitor:
                    result = GameData.data.GetDeedCount("ClearedAsteroidField") + "/10";
                    break;
                case Perks.The_Real_Space_Janitor:
                    result = GameData.data.GetDeedCount("DestroyedJunk") + "/200";
                    break;
                case Perks.Hoarder:
                    int num = pc.GetCargoSystem.cargo.Count(t => t.stockStationID == -1);
                    result = num + "/40";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Yacht) {
                        result += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        result += "Either Shuttle or Yacht required.";
                    }
                    break;
            }
            return result;
        }

        private static string GetKnowledgeUnlockProgress(Knowledge.Type type, BaseCharacter character) {
            int val = Knowledge.GetValue(type, character);
            int max = Knowledge.GetMax(Knowledge.Type.Construction, character);
            string result = val + "/25";
            if (max > val) {
                result += "\n" + ColorSys.infoNeg + "Current Highest: " + max + "</color>";
            }
            return result;
        }

        [HarmonyPatch(typeof(PerkControl), "ActivateTooltipPerk")]
        [HarmonyPostfix]
        public static void PCActivateTooltipPerk_Post(PerkControl __instance) {
            if (!_showImages.Value) {
                return;
            }
            LOGSource.LogDebug("Activating tooltip perk");
            
            Tooltip tooltip;
            if (GameObject.FindGameObjectWithTag("MainMenu"))
            {
                tooltip = GameObject.FindGameObjectWithTag("MainMenu").transform.Find("Tooltip").GetComponent<Tooltip>();
                if (tooltip != null) {
                    LOGSource.LogDebug("Activating tooltip perk: Inner not null");
                }
                return;
            }
            tooltip = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Tooltip").GetComponent<Tooltip>();
            tooltip.sprite = __instance.perk.image;
            
            tooltip.ShowItem("                  " + __instance.perk.GetString(false, null), false, false);
            tooltip.ShowExtras(__instance.perk.GetLockState(), __instance.perk.GetPerkTypeString());
            LOGSource.LogDebug("Activating tooltip perk set image:" + tooltip.sprite.name);
        }
    }
}