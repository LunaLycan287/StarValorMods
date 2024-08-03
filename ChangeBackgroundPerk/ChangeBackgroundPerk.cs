using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace LL_SV_ChangeBackgroundPerk
{
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class ChangeBackgroundPerk : BaseUnityPlugin
    {
        public const string pluginGuid = "lunalycan287.starvalormods.changebgperk";
        public const string pluginName = "Change Background Perk";
        public const string pluginVersion = "1.0.1";

        private static ConfigEntry<int> perkId;

        public void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(ChangeBackgroundPerk));
            LoadConfig();
        }

        private void LoadConfig()
        {
            perkId = Config.Bind<int>("General Settings", "PerkId", 1, "Perk that should be set.\n" +
                "Valid Values: \n" +
                " 0 = Outis; 1 = Miner; 2 = Trader; 3 = White Collar; 4 = Lone Wolf; 5 = Pirate; 6 = Rebel; 7 = Indoctrinated; 8 = Scoundrel");
        }


        [HarmonyPatch(typeof(PerksPanel), "ShowCharPerks")]
        [HarmonyPrefix]
        private static void PerksPanel_ShowCharPerks_Pre()
        {
            if (PChar.Char.perks == null || PChar.HasPerk(perkId.Value))
            {
                return;
            }

            PerkDB.AcquirePerk(perkId.Value);
        }
    }
}
