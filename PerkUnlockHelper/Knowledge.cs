using System;
using System.Linq;

namespace LL_SV_PerkUnlockHelper {
    public class Knowledge {
        public enum Type {
            Tech,
            Fighter,
            FleetCommander,
            Leadership,
            Geology,
            Explorer,
            Construction
        }

        public static int GetValue(Type type, BaseCharacter character) {
            switch (type) {
                case Type.Tech:             return character.techLevel;
                case Type.Fighter:          return character.fighterPilot;
                case Type.FleetCommander:   return character.fleetCommander;
                case Type.Leadership:       return character.leadership;
                case Type.Geology:          return character.geology;
                case Type.Explorer:         return character.explorer;
                case Type.Construction:     return character.construction;
                default:                    return 0;
            }
        }
        
        public static bool HasAnyKnowledgeIn(int value, Type ignoreKnowledge, BaseCharacter character) {
            return (ignoreKnowledge != Type.Tech && character.techLevel >= value) || 
                   (ignoreKnowledge != Type.Fighter && character.fighterPilot >= value) || 
                   (ignoreKnowledge != Type.FleetCommander && character.fleetCommander >= value) || 
                   (ignoreKnowledge != Type.Leadership && character.leadership >= value) || 
                   (ignoreKnowledge != Type.Geology && character.geology >= value) || 
                   (ignoreKnowledge != Type.Explorer && character.explorer >= value) || 
                   (ignoreKnowledge != Type.Construction && character.construction >= value);
        }

        public static int GetMax(Type ignoreKnowledge, BaseCharacter character) {
            return (from Type type in Enum.GetValues(typeof(Type)) where type != ignoreKnowledge select GetValue(type, character)).Prepend(0).Max();
        }
    }
}