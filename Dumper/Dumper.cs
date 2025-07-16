using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
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
            if (Input.GetKeyDown(_keybind.Value)) {
                Directory.CreateDirectory(DumpFolder);
                foreach (FileInfo file in new DirectoryInfo(DumpFolder).GetFiles())
                {
                    file.Delete(); 
                }
                /*DumpCrew();*/
                DumpEquipment();
                DumpFactions();
                DumpItems();
                DumpPerks();
                DumpShips();
                DumpSkills();
                DumpBaseBuilding();
            }
        }

        private static void SaveFile(string filename, string content) {
            string path = Path.Combine(DumpFolder, filename + ".dump.cs");
            File.WriteAllText(path, content);
            LOGSource.LogInfo(filename +" Dumped to: " + Path.GetFullPath(path));
        }

        private static string SanitizeName(string name) {
            string cleanName = name.Replace(" ", "_").Replace(".", "_").Replace("'", "");
            cleanName = cleanName.Replace("(Clone)", "").Replace("-", "_").Replace(":", "").Replace("+","_plus");
            //Crew Only
            //cleanName = cleanName.Replace("’", "").Replace(",", "");
            
            if (Char.IsDigit(cleanName[0])) {
                cleanName = "_"+cleanName;
            }

            return cleanName;
        }

        /*private static void DumpCrew() {
            LOGSource.LogInfo("Started CrewMembers Dump");
            List<CrewMember> crewMembersList = AccessTools.StaticFieldRefAccess<List<CrewMember>>(typeof(CrewDB), "crewList");
            StringBuilder csb =  new StringBuilder("using System.ComponentModel;");
            csb.AppendLine();
            csb.AppendLine("namespace LL_SV_Dumper {");
            csb.AppendLine("    public enum CrewMembers {");
            foreach (CrewMember member in crewMembersList) {
                csb.Append("        [Description(\"").Append(member.aiChar.name).AppendLine("\")]");
                csb.Append("        ").Append(sanitizeName(member.aiChar.name)).Append(" = ").Append(member.id).AppendLine(",");
            }
            csb.AppendLine("    }");
            csb.AppendLine("}");
            
            saveFile("CrewMembers", csb.ToString());
        }*/

        private static void DumpEquipment() {
            LOGSource.LogInfo("Started Equipments Dump");
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
            
            
            StringBuilder esb =  new StringBuilder("using System.ComponentModel;");
            esb.AppendLine();
            esb.AppendLine("namespace LL_SV_Dumper {");
            esb.AppendLine("    public enum Equipments {");
            foreach (EquipmentType type in equipmentTypes) {
                esb.Append("        // --- ").Append(type).Append("(").Append((int)type).AppendLine(")");
                equipmentSorted[type].Sort((item, item1) => item.id < item1.id ? -1 : 1);
                foreach (Equipment equipment in equipmentSorted[type]) {
                    esb.Append("        //").AppendLine(equipment.name);
                    esb.Append("        [Description(\"").Append(equipment.equipName).Append(" (").Append(equipment.type).AppendLine(")\")]");
                    string name = SanitizeName(equipment.equipName);
                    if (equipment.id == 188 || equipment.id == 189) {
                        name += "_builtin";
                    } else if (equipment.id == 182) {
                        name += "_CoT";
                    }
                    esb.Append("        ").Append(name).Append(" = ").Append(equipment.id).AppendLine(",");
                }
                esb.AppendLine();
            }
            
            esb.AppendLine("    }");
            esb.AppendLine("}");
            
            SaveFile("Equipments", esb.ToString());
        }
        
        private static void DumpFactions() {
            LOGSource.LogInfo("Started Faction Dump");
            List<Faction> factionList = AccessTools.StaticFieldRefAccess<List<Faction>>(typeof(FactionDB), "factions");
            StringBuilder fsb =  new StringBuilder("using System.ComponentModel;");
            fsb.AppendLine();
            fsb.AppendLine("namespace LL_SV_Dumper {");
            fsb.AppendLine("    public enum Factions {");
            foreach (Faction faction in factionList) {
                fsb.Append("        [Description(\"").Append(faction.factionName).AppendLine("\")]");
                fsb.Append("        ").Append(SanitizeName(faction.factionName)).Append(" = ").Append(faction.id).AppendLine(",");
            }
            fsb.AppendLine("    }");
            fsb.AppendLine("}");
            
            SaveFile("Factions", fsb.ToString());
        }

        private static void DumpItems() {
            LOGSource.LogInfo("Started Items Dump");
            List<Item> itemList = AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items");
            itemList.Sort((item, item1) => item.id < item1.id ? -1 : 1);
            StringBuilder isb =  new StringBuilder("using System.ComponentModel;");
            isb.AppendLine();
            isb.AppendLine("namespace LL_SV_Dumper {");
            isb.AppendLine("    public enum Items {");
            foreach (Item item in itemList) {
                isb.Append("        [Description(\"").Append(item.itemName).AppendLine("\")]");
                isb.Append("        ").Append(SanitizeName(item.itemName)).Append(" = ").Append(item.id).AppendLine(",");
            }
            isb.AppendLine("    }");
            isb.AppendLine("}");
            
            SaveFile("Items", isb.ToString());
        }

        private static void DumpPerks() {
            LOGSource.LogInfo("Started Perks Dump");
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

            StringBuilder psb = new StringBuilder("using System.ComponentModel;");
            psb.AppendLine();
            psb.AppendLine("namespace LL_SV_Dumper {");
            psb.AppendLine("    public enum Perks {");

            foreach (PerkType type in perkTypes) {
                psb.Append("        // --- ").Append(type).Append("(").Append((int)type).AppendLine(")");
                foreach (Perk perk in perkSorted[type]) {
                    psb.Append("        [Description(\"").Append(perk.Name()).Append(" (").Append(perk.Type()).AppendLine(")\")]");
                    psb.Append("        ").Append(SanitizeName(perk.Name())).Append(" = ").Append(perk.id).AppendLine(",");
                }
                psb.AppendLine();
            }
            
            psb.AppendLine("    }");
            psb.AppendLine("}");
            
            SaveFile("Perks", psb.ToString());
        }

        private static void DumpShips() {
            LOGSource.LogInfo("Started ShipModels Dump");
            List<ShipModelData> shipsList = AccessTools.StaticFieldRefAccess<List<ShipModelData>>(typeof(ShipDB), "shipModels");
            StringBuilder smsb =  new StringBuilder("using System.ComponentModel;");
            smsb.AppendLine();
            smsb.AppendLine("namespace LL_SV_Dumper {");
            smsb.AppendLine("    public enum ShipModels {");
            foreach (ShipModelData ship in shipsList) {
                smsb.Append("        [Description(\"").Append(ship.shipModelName).AppendLine("\")]");
                smsb.Append("        ").Append(SanitizeName(ship.shipModelName)).Append(" = ").Append(ship.id).AppendLine(",");
            }
            smsb.AppendLine("    }");
            smsb.AppendLine("}");
            
            SaveFile("ShipModels", smsb.ToString());

        }

        private static void DumpSkills() {
            LOGSource.LogInfo("Started Skills Dump");
            List<Skill> skillList = SkillDB.Skills;
            StringBuilder ssb =  new StringBuilder("using System.ComponentModel;");
            ssb.AppendLine();
            ssb.AppendLine("namespace LL_SV_Dumper {");
            ssb.AppendLine("    public enum Skills {");
            foreach (Skill skill in skillList) {
                ssb.Append("        [Description(\"").Append(skill.skillName).AppendLine("\")]");
                ssb.Append("        ").Append(SanitizeName(skill.skillName)).Append(" = ").Append(skill.id).AppendLine(",");
            }
            ssb.AppendLine("    }");
            ssb.AppendLine("}");
            
            SaveFile("Skills", ssb.ToString());
        }

        private static void DumpBaseBuilding() {
            LOGSource.LogInfo("Started BuildingPlan Dump");
            List<BuildingPlan> planList = AccessTools.StaticFieldRefAccess<List<BuildingPlan>>(typeof(BaseBuildingDB), "plans");
            planList.Sort((item, item1) => item.id < item1.id ? -1 : 1);
            StringBuilder bbsb =  new StringBuilder("using System.ComponentModel;");
            bbsb.AppendLine();
            bbsb.AppendLine("namespace LL_SV_Dumper {");
            bbsb.AppendLine("    public enum BuildingPlans {");
            foreach (BuildingPlan plan in planList) {
                if (!String.IsNullOrEmpty(plan.Description)) {
                    bbsb.Append("       //").AppendLine(plan.Description);
                }

                bbsb.Append("       [Description(\"").Append(plan.Name).AppendLine("\")]");
                bbsb.Append("       ").Append(SanitizeName(plan.Name)).Append("_").Append(plan.id).Append(" = ").Append(plan.id).AppendLine(",");
            }
            bbsb.AppendLine("    }");
            bbsb.AppendLine("}");
            
            
            SaveFile("BuildingPlans", bbsb.ToString());
        }
    }
}