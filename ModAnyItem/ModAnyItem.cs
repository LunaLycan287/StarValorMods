using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LL_SV_ModAnyItem {
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class ModAnyItem : BaseUnityPlugin {
        private const string PluginGuid = "lunalycan287.starvalormods.modanyitem";
        private const string PluginName = "Mod Any Item";
        private const string PluginVersion = "1.0.0";
        internal static readonly ManualLogSource LOGSource = new ManualLogSource("(LL) " + PluginName.Replace(" ",""));
        
        private const string ModFilesDir = "ItemMods";
        //private const string CustomFilesDir = "ItemCustom"; TODO: Add functionality for custom item.
        private const string ItemExtension = ".itemmod";
        private const string ErrorFilesDir = "Error";

        private static string _pluginFolder = "";
        
        public void Awake() {
            Harmony.CreateAndPatchAll(typeof(ModAnyItem));
            BepInEx.Logging.Logger.Sources.Add(LOGSource);
            _pluginFolder = Path.GetDirectoryName(GetType().Assembly.Location);
            LOGSource.LogInfo("Loaded " + PluginName + " v" + PluginVersion);
        }
        
        [HarmonyPatch(typeof(ItemDB), nameof(ItemDB.LoadDatabaseForce))]
        [HarmonyPostfix]
        private static void ShipDBLoad_Post()
        {
            LoadModFiles();
        }

        private static void LoadModFiles() {
            LOGSource.LogDebug("Loading mod files");
            string modPath = Path.Combine(_pluginFolder, ModFilesDir);
            if (!Directory.Exists(modPath)) {
                Directory.CreateDirectory(modPath);
            }

            List<Item> itemList = AccessTools.StaticFieldRefAccess<List<Item>>(typeof(ItemDB), "items");
            Dictionary<int, ItemMod> itemMods = LoadFiles(modPath);
            List<Item> misingItems = new List<Item>();
            
            List<ItemMod> validItemMods = new List<ItemMod>();
            foreach (Item item in itemList) {
                if (itemMods.ContainsKey(item.id)) {
                    validItemMods.Add(itemMods[item.id]);
                    itemMods.Remove(item.id);
                }
                else {
                    misingItems.Add(item);
                }
            }

            MoveErrorFiles(modPath, itemMods);
            
            DumpMissingFiles(misingItems);
            
            foreach (ItemMod mod in validItemMods) {
                if (!mod.ApplyPatch()) {
                    LOGSource.LogWarning("Patching failed for " + mod.ItemName);
                }
            }
        }

        private static Dictionary<int, ItemMod> LoadFiles(string dir) {
            Dictionary<int, ItemMod> items = new  Dictionary<int, ItemMod>();
            string path = Path.Combine(_pluginFolder, dir);
            string[] fileList = Directory.GetFiles(path, "*"+ItemExtension);
            LOGSource.LogDebug("Files found: " + fileList.Length + " in " + path);
            
            foreach (string filePath in fileList) {
                string fileName = Path.GetFileName(filePath);
                LOGSource.LogDebug("Loading file: " + fileName);
                
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ItemMod));
                using (XmlReader reader = XmlReader.Create(new StreamReader(filePath))) {
                    ItemMod item = (ItemMod)xmlSerializer.Deserialize(reader);
                    item.FileName = fileName;
                    items.Add(item.Id, item);
                }
            }
            return items;
        }
        
        private static void DumpMissingFiles(List<Item> misingItems) {
            foreach (Item misingItem in misingItems) {
                ItemModSerializable mod = ItemModSerializable.FromItem(misingItem);
                
                string file = Path.Combine(_pluginFolder, ModFilesDir, mod.FileName + ItemExtension);
                LOGSource.LogInfo("Writing missing file: " + mod.Id + " IN: '" + mod.ItemName +"'");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(ItemModSerializable));
                using (TextWriter writer = new StreamWriter(file)) {
                    xmlSerializer.Serialize(writer, mod);
                }
            }
        }
        
        private static void MoveErrorFiles(string originalPath, Dictionary<int, ItemMod> itemMods) {
            if (itemMods.Count == 0) return;
            
            string path = Path.Combine(originalPath, ErrorFilesDir);
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
                
            foreach (ItemMod item in itemMods.Values) {
                LOGSource.LogWarning("Found invalid item mod: " + item.FileName);
                string fullPathOld = Path.Combine(originalPath, item.FileName + ItemExtension);
                string fullPathNew = Path.Combine(path, item.FileName + ItemExtension);
                File.Move(fullPathOld, fullPathNew);
            }
        }
    }
}