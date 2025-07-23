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
        private const string PluginVersion = "1.3.1";

        private static readonly ManualLogSource LOGSource = new ManualLogSource("(LL) " + PluginName.Replace(" ",""));

        private const int PanelMode = 271828; // Because everything is better with E :3
        private static GameObject _unlockPerksPanel;

        private static ConfigEntry<string> _keybind;
        private static ConfigEntry<bool> _showImages;
        private static ConfigEntry<bool> _showHiddenUnlockConditions;
        private static ConfigEntry<bool> _showPreviouslyUnlocked;
        private static ConfigEntry<bool> _sortPerkList;
        
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
            _showPreviouslyUnlocked = Config.Bind<bool>("General Settings", "ShowPreviouslyUnlocked", true, "Show perks that have been perviously unlocked, but not yet acquired this run.");
            _sortPerkList = Config.Bind<bool>("General Settings", "SortPerkList", true, "If perk list should be sorted. Yellow / issue or almost not acquirable first, then normal and at the end all the red / not acquirable ones.");
        }

        private struct PerkInfo
        {
           public Perk Perk { get; set; }
           public Color Color { get; set; }
           public string UnlockProgress { get; set; }
        }
        private static void FillPerksHelperPanel(PerksPanel __instance) {
            LOGSource.LogDebug("FillPerksHelperPanel");
            Transform panel = __instance.transform.Find("Panel");
            Text title = __instance.transform.Find("Title").GetComponent<Text>();
            Traverse.Create(__instance).Field("showOnly").SetValue(true);

            
            List<PerkInfo> validPerks = new List<PerkInfo>();
            
            int totalPerks = PerkDB.totalPerks;
            for (int j = 0; j < totalPerks; j++) {
                Perk byIndex = PerkDB.GetByIndex(j);
               
                // Already acquired in this save.
                // Do not display Experience type perks, since they are not acquire-able
                // Do not display special early support perk which can not be acquired anymore
                // Do not display unlocked perks if PerviouslyUnlocked setting is false.
                // Do not display perks which are not unlockable in relaxed mode if they have not previously been unlocked
                if (PChar.HasPerk(byIndex.id) || byIndex.type == PerkType.Experience || byIndex.id == (int)Perks.Early_Supporter || 
                    (!_showPreviouslyUnlocked.Value && !byIndex.locked) ||
                    (GameData.data.difficulty == -1 && !byIndex.unlockOnRelaxedMode && byIndex.locked) ) {
                    continue;
                }
                
                validPerks.Add(GetPerkInfo(byIndex));
            }

            if (_sortPerkList.Value) {
                validPerks.Sort((x, y) => {
                    int ret = x.Perk.id < y.Perk.id ? -1 : 1;
                    if (x.Color == y.Color) return ret;

                    if (x.Color == Color.red) {
                        ret = 1;
                    }
                    else if (y.Color == Color.red) {
                        ret = -1;
                    }
                    else if (x.Color == Color.yellow) {
                        ret = -1;
                    }
                    else if (y.Color == Color.yellow) {
                        ret = 1;
                    }
                    return ret;
                });
            }

            int i = 0;
            foreach (PerkInfo perkInfo in validPerks) {
                
                if (_showHiddenUnlockConditions.Value && perkInfo.Perk.showLevel < 2) {
                    perkInfo.Perk.showLevel = 2;
                }
                
                if (i >= panel.childCount) {
                    Instantiate(__instance.perkGO, panel);
                }

                PerkControl perkControl = panel.GetChild(i).GetComponent<PerkControl>();
                perkControl.Setup(perkInfo.Perk, __instance, null, false, null);
                
                perkControl.bgColor = perkInfo.Color;
                perkControl.transform.Find("BG").GetComponent<Image>().color = perkControl.bgColor;
                

                if (_showImages.Value && perkInfo.Perk.locked) {
                    Image img = perkControl.transform.Find("Image").GetComponent<Image>();
                    img.sprite = perkInfo.Perk.image;
                    //img.color = new Color32(47,79,79, 255);
                    img.color = new Color32(255, 255, 255, 50);
                }

                panel.GetChild(i).gameObject.SetActive(true);
                i++;
            }
            
            //__instance.AdjustPanelSize(i);
            while (i < panel.childCount) {
                panel.GetChild(i).gameObject.SetActive(false);
                i++;
            }

            title.text = "Locked Perks";
        }

        private static bool IsQuestCompleted(Quests quest, PlayerControl pc) {
            return pc && QuestDB.IsQuestCompleted(QuestDB.GetQuestRef((int)quest), pc.transform);
        }

        private static PerkInfo GetPerkInfo(Perk perk) {
            string unlockProgress = "";
            Color bgColor = Color.gray;
            
            PlayerControl pc = PlayerControl.inst;
            BaseCharacter character = GameData.data.character;
            switch ((Perks)perk.id) {
                case Perks.Outis: 
                    if (!HasPerk(Perks.Outis)) { // You can not re-acquire the perk (by normal means).
                        bgColor = Color.red;
                        unlockProgress = "Can not be re-aquired.";
                    }
                    break;
                case Perks.Miner:
                    if (!HasPerk(Perks.Outis)) {
                        bgColor = Color.red;
                        unlockProgress = "Requires being Outis.";
                    }
                    break;
                case Perks.Trader:
                    if (!HasPerk(Perks.Miner)) {
                        bgColor = Color.red;
                        unlockProgress = "Requires being Miner.";
                    }
                    break;
                case Perks.White_Collar:
                    if (!HasPerk(Perks.Trader) && !HasPerk(Perks.Miner)) {
                        bgColor = Color.red;
                        unlockProgress = "Requires being Miner or Trader";
                    }
                    break;
                case Perks.Lone_Wolf:
                case Perks.Scoundrel:
                    if (IsQuestCompleted(Quests.Old_friends_new_enemies_160, pc)) {
                        bgColor = Color.red;
                        unlockProgress = "Quest already done, but other choice was used";
                    }
                    break;
                case Perks.Pirate:
                case Perks.Rebel:
                case Perks.Indoctrinated:
                    if (!HasPerk(Perks.Outis) && !HasPerk(Perks.Miner) && !HasPerk(Perks.Trader)) {
                        bgColor = Color.red;
                        unlockProgress = "Requires being Outis, Miner or Trader.";
                    }
                    break;
                case Perks.Battle_Rush:
                    if (PChar.Char.level >= 10) {
                        bgColor = Color.red;
                        unlockProgress = "Player level too high.";
                    } else if (PChar.Char.level >= 8) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Combat_Genius:
                    if (pc.mercenaries.Count > 0) {
                        bgColor = Color.yellow;
                        unlockProgress = "Not allowed to have assistance. Dismiss them first.";
                    }
                    break;
                case Perks.Acquired_Wisdom:
                    unlockProgress = character.level + "/50";
                    break;
                case Perks.O_C_D_:
                    unlockProgress = GameData.data.GetDeedCount("FullyExploredSector") + "/10";
                    break;
                case Perks.The_Perfect_Predator:
                    unlockProgress = GameData.data.GetDeedCount("AmbushAttack") + "/5";
                    break;
                case Perks.Battleship_Raid:
                    unlockProgress = GameData.data.GetDeedCount("DefeatedBossWithYachtOrShuttle") + "/5";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Yacht) {
                        bgColor = Color.yellow;
                        unlockProgress += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        unlockProgress += "Either Shuttle or Yacht required.";
                    }
                    break;
                case Perks.Sloppy:
                    unlockProgress = GameData.data.GetDeedCount("Sloppy") + "/6";
                    break;
                case Perks.Aggressive:
                    unlockProgress = GameData.data.GetDeedCount("DestroyedIndSyndPMCShip") + "/3";
                    break;
                case Perks.Marauder:
                    unlockProgress = GameData.data.GetDeedCount("DestroyedIndSyndPMCShip") + "/20";
                    break;
                case Perks.Temperate:
                    unlockProgress = GameData.data.GetDeedCount("AvoidedIndSyndPMCShip") + "/10";
                    break;
                case Perks.Guardian_Angel:
                    unlockProgress = GameData.data.GetDeedCount("SavedIndSyndPMCShip") + "/10";
                    if (HasPerk(Perks.Pirate)) {
                        bgColor = Color.red;
                        unlockProgress += "\n" + ColorSys.infoNeg + "Get rid of Pirate Perk!</color>";
                    }
                    break;
                case Perks.Anaximander:
                    unlockProgress = GameData.data.GetDeedCount("ExploredNewSector") + "/100";
                    break;
                case Perks.Dogfighter:
                    unlockProgress = GameData.data.GetDeedCount("DogfightWin") + "/100";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Corvette) {
                        bgColor = Color.yellow;
                        unlockProgress += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        unlockProgress += "Either Shuttle, Yacht or Corvette required.";
                    }
                    break;
                case Perks.Space_Janitor:
                    unlockProgress = GameData.data.GetDeedCount("ClearedAsteroidField") + "/10";
                    break;
                case Perks.The_Real_Space_Janitor:
                    unlockProgress = GameData.data.GetDeedCount("DestroyedJunk") + "/200";
                    break;
                case Perks.Hoarder:                     
                    int num = pc.GetCargoSystem.cargo.Count(t => t.stockStationID == -1);
                    unlockProgress = num + "/40";
                    if (pc.GetSpaceShip.shipClass > (int)ShipClassLevel.Yacht) {
                        bgColor = Color.yellow;
                        unlockProgress += "\n" + ColorSys.infoNeg + "CHANGE SHIP!</color> ";
                        unlockProgress += "Either Shuttle or Yacht required.";
                    }
                    break;
                case Perks.Techie:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Tech, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.Tech, character);
                    break;
                case Perks.Ace:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Fighter, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.Fighter, character);
                    break;
                case Perks.Just_me_and_the_boys:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.FleetCommander, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.FleetCommander, character);
                    break;
                case Perks.Hard_Worker:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Geology, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.Geology, character);
                    break;
                case Perks.Traveler:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Explorer, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.Explorer, character);
                    break;
                case Perks.Early_Supporter:
                    bgColor = Color.red;  // Can never get since time based
                    unlockProgress = ColorSys.infoNeg + "Can never be acquired</color> ";
                    break;
                case Perks.Self_Reliant:
                    if (PChar.Char.level >= 10 || GameData.HasDeed("BoughtEquipmentOrShip") || PChar.HasAnyRepOverValue(999)) {
                        bgColor = Color.red;
                        unlockProgress = ColorSys.infoNeg + "Player level too high, bought equipment or has too high reputation.</color>";
                    } else if (PChar.Char.level >= 8 || PChar.HasAnyRepOverValue(888)) {
                        bgColor = Color.yellow;
                        unlockProgress = "Level or reputation getting close to too high.";
                    }
                    break;
                case Perks.Contractor:
                    bgColor = GetPerkColorKnowledge(Knowledge.Type.Construction, character, bgColor);
                    unlockProgress = GetKnowledgeUnlockProgress(Knowledge.Type.Construction, character);
                    break;
            }

            return new PerkInfo() {
                Perk = perk,
                Color = bgColor,
                UnlockProgress = unlockProgress
            };
        }
        
        private static bool HasPerk(Perks perk) {
            return PChar.HasPerk((int)perk);   
        }

        private static Color GetPerkColorKnowledge(Knowledge.Type knowledge, BaseCharacter character, Color defaultColor) {
            Color bgColor = defaultColor;
            if (Knowledge.HasAnyKnowledgeIn(25, knowledge, character)) {
                bgColor = Color.red;
            }
            else if (Knowledge.HasAnyKnowledgeIn(22, knowledge, character)) {
                bgColor = Color.yellow;
            }
            return bgColor;
        }

        [HarmonyPatch(typeof(PlayerControl), "Update")]
        [HarmonyPostfix]
        private static void PCUpdate_Post() {
            if (Input.GetKeyDown(_keybind.Value)) {
                CharacterScreen charScreen = GameObject.FindGameObjectWithTag("PlayerUI").GetComponent<PlayerUIControl>().transform.parent.Find("Character").GetComponent<CharacterScreen>();
                if (charScreen) {
                    charScreen.OpenClose(PanelMode);
                }
            }
        }
        
        [HarmonyPatch(typeof(CharacterScreen), nameof(CharacterScreen.OpenClose))]
        [HarmonyPrefix]
        private static bool CSOpenClose_Pre(CharacterScreen __instance) {
            if (_unlockPerksPanel && _unlockPerksPanel.activeSelf) {
                __instance.Close();
                return false; //skip original
            }
            return true;
        }

        [HarmonyPatch(typeof(CharacterScreen), "CloseSubPanels")]
        [HarmonyPostfix]
        private static void CSCloseSubPanels_Post() {
            if (_unlockPerksPanel && _unlockPerksPanel.activeSelf) {
                _unlockPerksPanel.SetActive(false);
            }
        }
        
        [HarmonyPatch(typeof(CharacterScreen), nameof(CharacterScreen.Open))]
        [HarmonyPostfix]
        private static void CSOpen_Post(CharacterScreen __instance, int mode) {
            LOGSource.LogDebug("CSOpen_Post: " + mode);
            if (mode == PanelMode) {
                InstantiatePerksHelperPanel(__instance);
                _unlockPerksPanel.SetActive(true);
                FillPerksHelperPanel(_unlockPerksPanel.GetComponent<PerksPanel>());
            }
        }
        
        private static void InstantiatePerksHelperPanel(CharacterScreen __instance)
        {
            LOGSource.LogDebug("InstantiatePerksHelperPanel");
            if (!_unlockPerksPanel) {
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
            if (!GameManager.instance.inGame) return;
            
            if (!__instance.locked) {
                string unlockTxt = __instance.UnlockText;
                if (!unlockTxt.IsNullOrWhiteSpace()) {
                    __result += "\n\n" + "<size=12>To Unlock:</size>\n" + ColorSys.infoText2 + unlockTxt + "</color>";
                }
            }

            if (__instance.showLevel >= 2) {
                PerkInfo unlockProgress = GetPerkInfo(__instance);
                if (!unlockProgress.UnlockProgress.IsNullOrWhiteSpace()) {
                    __result += "\n\n" + ColorSys.infoText3 + unlockProgress.UnlockProgress + "</color>";
                }
            }
        }

        private static string GetKnowledgeUnlockProgress(Knowledge.Type type, BaseCharacter character) {
            int val = Knowledge.GetValue(type, character);
            int max = Knowledge.GetMax(type, character);
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

            GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (!mainCanvas) return;
            
            Tooltip tooltip = mainCanvas.transform.Find("Tooltip").GetComponent<Tooltip>();
            if (!tooltip) return;
            
            tooltip.sprite = __instance.perk.image;

            tooltip.ShowItem("                  " + __instance.perk.GetString(false, null), false, false);
            tooltip.ShowExtras(__instance.perk.GetLockState(), __instance.perk.GetPerkTypeString());
        }
    }
}