using System;
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
                case Perks.Pirate:
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
                case Perks.Hoarder:
                    if (pc.GetSpaceShip.shipClass > 2) {
                        bgColor = Color.yellow;
                    }
                    break;
                case Perks.Techie:
                    bgColor = GetPerkColorKnowledge(0, character);
                    break;
                case Perks.Ace:
                    bgColor = GetPerkColorKnowledge(1, character);
                    break;
                case Perks.Just_me_and_the_boys:
                    bgColor = GetPerkColorKnowledge(2, character);
                    break;
                case Perks.Hard_Worker:
                    bgColor = GetPerkColorKnowledge(4, character);
                    break;
                case Perks.Traveler:
                    bgColor = GetPerkColorKnowledge(5, character);
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
                    bgColor = GetPerkColorKnowledge(6, character);
                    break;
            }

            return bgColor;
        }
        
        private static bool HasPerk(Perks perk) {
            return PChar.HasPerk((int)perk);   
        }

        private static Color? GetPerkColorKnowledge(int tech, BaseCharacter character) {
            Color? bgColor = null;
            if (character.techLevel == 25 && HasAnyKnowledgeIn(25,tech, character)) {
                bgColor = Color.red;
            }
            else if (HasAnyKnowledgeIn(23,tech, character)) {
                bgColor = Color.yellow;
            }
            return bgColor;
        }
        
        private static bool HasAnyKnowledgeIn(int value, int ignoreKnowledge, BaseCharacter character)
        {
            return (ignoreKnowledge != 0 && character.techLevel >= value) || (ignoreKnowledge != 1 && character.fighterPilot >= value) || 
                   (ignoreKnowledge != 2 && character.fleetCommander >= value) || (ignoreKnowledge != 3 && character.leadership >= value) || 
                   (ignoreKnowledge != 4 && character.geology >= value) || (ignoreKnowledge != 5 && character.explorer >= value) || 
                   (ignoreKnowledge != 6 && character.construction >= value);
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

    }
}