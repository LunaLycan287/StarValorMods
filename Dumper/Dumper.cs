using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace LL_SV_Dumper {
    
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Dumper : BaseUnityPlugin {
        private const string PluginGuid = "lunalycan287.starvalormods.dumper";
        private const string PluginName = "Dumper";
        private const string PluginVersion = "1.0.0";

        private const string DumpFolder = "dump";

        private static readonly ManualLogSource LOGSource = new ManualLogSource("(LL) " + PluginName.Replace(" ",""));
        
        private static ConfigEntry<string> _keybind;
        
        public void Awake() {
            Harmony.CreateAndPatchAll(typeof(Dumper));
            BepInEx.Logging.Logger.Sources.Add(LOGSource);
            LoadConfig();
            LOGSource.LogInfo("Loaded " + PluginName + " v" + PluginVersion);
        }
        
        private void LoadConfig()
        {
            _keybind = Config.Bind<string>("General Settings", "KeyBind", "f10", "The Key Code to use. (Default: f8) See https://docs.unity3d.com/6000.1/Documentation/ScriptReference/KeyCode.html for possible values. NOTE: names must be all lowercase!");
        }

        public void Update() {
            if (!Input.GetKeyDown(_keybind.Value)) return;
            
            Directory.CreateDirectory(DumpFolder);
            foreach (FileInfo file in new DirectoryInfo(DumpFolder).GetFiles()) {
                file.Delete(); 
            }
            DumpCrew();
            DumpEquipment();
            DumpFactions();
            DumpItems();
            DumpPerks();
            DumpShips();
            DumpSkills();
            DumpBaseBuilding();
            DumpWeapons();
        }

        private static string SanitizeName(string name) {
            string cleanName = name.Replace(" ", "_").Replace(".", "_").Replace("'", "");
            cleanName = cleanName.Replace("(Clone)", "").Replace("-", "_").Replace(":", "").Replace("+","_plus");
            // Crew specific
            cleanName = cleanName.Replace(",", "");

            if (cleanName.Length == 0) {
                cleanName = "Unknown";
            }
            
            if (char.IsDigit(cleanName[0])) {
                cleanName = "_"+cleanName;
            }

            return cleanName;
        }

        private static string SanitizeCrewName(string name) {
            return name.Replace("’", "'").Replace("\"", "'");
        }

        private static StringBuilder DumpStart(string enumName) {
            StringBuilder sb =  new StringBuilder("using System.ComponentModel;");
            sb.AppendLine();
            sb.AppendLine("namespace LL_SV_Dumper {");
            sb.Append("    public enum ").Append(enumName).AppendLine(" {");
            return sb;
        }

        private static void DumpType(StringBuilder sb, string type, int typeId) {
            sb.Append("        // --- ").Append(type).Append("(").Append(typeId).AppendLine(")");
        }

        private static void DumpDefaultEntry(StringBuilder sb, string name, int id) {
            DumpSanitizedEntry(sb, name, SanitizeName(name), id);
        }

        private static void DumpSanitizedEntry(StringBuilder sb, string name, string sanitizedName, int id, [CanBeNull] string type = null, bool containsDuplicates = false) {
            sb.Append("        [Description(\"").Append(name);
            if (type != null) {
                sb.Append(" (").Append(type).Append(")");
            }
            sb.AppendLine("\")]");

            sb.Append("        ").Append(sanitizedName);
            if (containsDuplicates) {
                sb.Append("_").Append(id);
            }
            sb.Append(" = ").Append(id).AppendLine(",");
        }
        
        private static void DumpEnd(string enumName, StringBuilder sb) {
            sb.AppendLine("    }");
            sb.AppendLine("}");
            
            string path = Path.Combine(DumpFolder, enumName + ".dump.cs");
            File.WriteAllText(path, sb.ToString());
            LOGSource.LogInfo(enumName +" Dumped to: " + Path.GetFullPath(path));
        }

        private static void DumpCrew() {
            const string enumName = "CrewMembers";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<CrewMember> crewMembersList = new List<CrewMember>(GameManager.predefinitions.crewMembers);

            StringBuilder sb = DumpStart(enumName);
            foreach (CrewMember member in crewMembersList) {
                DumpDefaultEntry(sb, SanitizeCrewName(member.aiChar.name), member.id);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpEquipment() {            
            const string enumName = "Equipments";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<Equipment> equipmentList = AccessTools.StaticFieldRefAccess<List<Equipment>>(typeof(EquipmentDB), "equipments");
            Dictionary<EquipmentType, List<Equipment>> equipmentSorted = new Dictionary<EquipmentType, List<Equipment>>();
            List<EquipmentType> equipmentTypes = new List<EquipmentType>();
            foreach (Equipment equipment in equipmentList) {
                if (equipmentSorted.ContainsKey(equipment.type)) {
                    equipmentSorted[equipment.type].Add(equipment);
                }
                else {
                    equipmentTypes.Add(equipment.type);
                    equipmentSorted.Add(equipment.type, new List<Equipment>() { equipment });
                }
            }


            StringBuilder sb = DumpStart(enumName);
            foreach (EquipmentType type in equipmentTypes) {
                DumpType(sb, type.ToString(), (int) type);
                equipmentSorted[type].Sort((item, item1) => item.id < item1.id ? -1 : 1);
                foreach (Equipment equipment in equipmentSorted[type]) {
                    sb.Append("        //").AppendLine(equipment.name);
                    
                    string sanitizedName = SanitizeName(equipment.equipName);
                    // Special cases where we have duplicate name entries.
                    switch (equipment.id) {
                        case 188: // Attack Drone MK2 vs Attack Drone MK2 Built In
                        case 189: // Repair Drone MK2 vs Repair Drone MK2 Built In
                            sanitizedName += "_builtin";
                            break;
                        case 182: //Cloaking vs CloakingCoT aka Terran Mantle
                            sanitizedName += "_CoT";
                            break;
                    }
                    DumpSanitizedEntry(sb, equipment.name, sanitizedName, equipment.id, type.ToString());
                }
                sb.AppendLine();
            }
            DumpEnd(enumName, sb);
        }
        
        private static void DumpFactions() {
            const string enumName = "Factions";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<Faction> factionList = AccessTools.StaticFieldRefAccess<List<Faction>>(typeof(FactionDB), "factions");
            
            StringBuilder sb = DumpStart(enumName);
            foreach (Faction faction in factionList) {
                DumpDefaultEntry(sb, faction.factionName, faction.id);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpItems() {
            const string enumName = "Items";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<Item> itemList = AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items");
            itemList.Sort((item, item1) => item.id < item1.id ? -1 : 1);

            StringBuilder sb = DumpStart(enumName);
            foreach (Item item in itemList) {
                DumpDefaultEntry(sb, item.itemName, item.id);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpPerks() {
            const string enumName = "Perks";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            Dictionary<PerkType, List<Perk>> perkSorted = new Dictionary<PerkType, List<Perk>>();
            List<Perk> perkList = PerkDB.GetAllPerks();
            List<PerkType> perkTypes = new List<PerkType>();
            foreach (Perk perk in perkList) {
                if (perkSorted.ContainsKey(perk.type)) {
                    perkSorted[perk.type].Add(perk);
                }
                else {
                    perkTypes.Add(perk.type);
                    perkSorted.Add(perk.type, new List<Perk>() { perk });
                }
            }

            StringBuilder sb = DumpStart(enumName);

            foreach (PerkType type in perkTypes) {
                DumpType(sb, type.ToString(), (int) type);
                foreach (Perk perk in perkSorted[type]) {
                    DumpSanitizedEntry(sb, perk.Name(), SanitizeName(perk.Name()), perk.id, type.ToString());
                }
                sb.AppendLine();
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpShips() {
            const string enumName = "ShipModels";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<ShipModelData> shipsList = AccessTools.StaticFieldRefAccess<List<ShipModelData>>(typeof(ShipDB), "shipModels");
            
            StringBuilder sb = DumpStart(enumName);
            foreach (ShipModelData ship in shipsList) {
                DumpDefaultEntry(sb, ship.shipModelName, ship.id);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpSkills() {
            const string enumName = "Skills";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<Skill> skillList = SkillDB.Skills;
            
            StringBuilder sb = DumpStart(enumName);
            foreach (Skill skill in skillList) {
                DumpDefaultEntry(sb, skill.skillName, skill.id);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpBaseBuilding() {
            const string enumName = "BuildingPlans";
            if (!GameManager.instance.inGame) {
                LOGSource.LogWarning("Skipping " + enumName + " Dump. We currently have no game loaded.");
                return;
            }
             
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            List<BuildingPlan> planList = AccessTools.StaticFieldRefAccess<List<BuildingPlan>>(typeof(BaseBuildingDB), "plans");
            //List<BuildingPlan> planList = TODO: Fix plans to load when game not loaded
            planList.Sort((item, item1) => item.id < item1.id ? -1 : 1);
            
            StringBuilder sb  = DumpStart(enumName);
            foreach (BuildingPlan plan in planList) {
                if (!string.IsNullOrEmpty(plan.Description)) {
                    sb.Append("       //").AppendLine(plan.Description);
                }
                
                DumpSanitizedEntry(sb, plan.Name, SanitizeName(plan.Name), plan.id, null, true);
            }
            DumpEnd(enumName, sb);
        }

        private static void DumpWeapons() {
            const string enumName = "Weapons";
            LOGSource.LogInfo("Started " + enumName + " Dump");
            
            Dictionary<int, TWeapon> weaponReduced = new Dictionary<int, TWeapon>();
            List<TWeapon> weaponList = new List<TWeapon>(GameManager.predefinitions.weapons);
            foreach (TWeapon weapon in weaponList.Where(weapon => !weaponReduced.ContainsKey(weapon.index))) {
                weaponReduced[weapon.index] = weapon;
            }
            
            StringBuilder sb = DumpStart(enumName);
            sb.AppendLine("        // All weapons are available in different Weapon Types but the ID stays the same");
            sb.AppendLine();
            foreach (TWeapon weapon in weaponReduced.Select(weaponPair => weaponPair.Value)) {
                DumpDefaultEntry(sb, weapon.name, weapon.index);
            }
            DumpEnd(enumName, sb);
        }
    }
}