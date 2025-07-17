using System;
using JetBrains.Annotations;
using LL_SV_Dumper;

namespace LL_SV_ModAnyItem {
    
    [Serializable]
    public class TradeChance {
        public int Civilian { get; set; }
        public int ProximaMiningCompany { get; set; }
        public int Syndicate { get; set; }
        public int RedSkullPirates { get; set; }
        public int VenghiAscension { get; set; }
        public int ChildrenOfTerra { get; set; }
        public int Technomancers { get; set; }

        [NonSerialized]
        private const int ExpectedFactions = 7;

        [CanBeNull]
        public static TradeChance FromArray(int[] tradeChance) {
            if (tradeChance == null || tradeChance.Length == 0) {
                return null;
            }
            
            if (tradeChance.Length != ExpectedFactions) {
                string warning = "Trade chance array invalid.";
                warning += tradeChance.Length > ExpectedFactions ? "More " : "Fewer ";
                warning += "entries as expected. Expected " + ExpectedFactions + " but found " + tradeChance.Length;
                ModAnyItem.LOGSource.LogWarning(warning);
            }
            TradeChance result = new TradeChance();
            for (int i = 0; i < tradeChance.Length; ++i) {
                switch ((Factions)i) {
                    case Factions.Civilian:                 result.Civilian = tradeChance[i]; break;
                    case Factions.Proxima_Mining_Company:   result.ProximaMiningCompany = tradeChance[i]; break;
                    case Factions.Syndicate:                result.Syndicate = tradeChance[i]; break;
                    case Factions.Red_Skull_Pirates:        result.RedSkullPirates = tradeChance[i]; break;
                    case Factions.Venghi_Ascension:         result.VenghiAscension = tradeChance[i]; break;
                    case Factions.Children_of_Terra:        result.ChildrenOfTerra = tradeChance[i]; break;
                    case Factions.Technomancers:            result.Technomancers = tradeChance[i]; break;
                    default:
                        ModAnyItem.LOGSource.LogWarning("TradeChance unknown Faction ID = " + i);
                        break;
                }
            }
            return result;
        }

        public int[] ToArray() {
            int[] array = new int[ExpectedFactions];
            array[(int)Factions.Civilian] = Civilian;
            array[(int)Factions.Proxima_Mining_Company] = ProximaMiningCompany;
            array[(int)Factions.Syndicate] = Syndicate;
            array[(int)Factions.Red_Skull_Pirates] = RedSkullPirates;
            array[(int)Factions.Venghi_Ascension] = VenghiAscension;
            array[(int)Factions.Children_of_Terra] = ChildrenOfTerra;
            array[(int)Factions.Technomancers] = Technomancers;
            return array;
        }
    }
}