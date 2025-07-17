using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace LL_SV_ModAnyItem {
    [Serializable]
    public class ItemMod {
        
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        
        [XmlComment("Possible Rarities: 0 = Poor | 1 = Common | 2 = Uncommon | 3 = Rare | 4 = Epic | 5 = Legendary")]
        public int Rarity { get; set; }
        [XmlComment("Possible Rarities: 1 = Common | 2 = Uncommon | 3 = Rare | 4 = Epic | 5 = Legendary")]
        public ItemRarity CanUpgradeToRarity { get; set; }
        
        public int LevelPlus { get; set; } // TODO: figure out what this exactly is for
        public float Weight { get; set; }
        
        public float BasePrice { get; set; }
        public float PriceVariation { get; set; }
        public TradeChance TradeChance { get; set; } // TODO: Needed?
        public int TradeQuantity { get; set; }
        
        [XmlComment("Possible Types: Data | Crystal | Metal | Food | Electronic | Ammunition | Chemical | Utility | Other | Package | Container | Organic | Component | CraftingComponent")]
        public ItemType Type { get; set; }
        //TODO: Figure out how to set sprites for custom items
        [XmlComment("If the item can be requested in generated quests, like supply quests.")]
        public bool AskedInQuests { get; set; }
        [XmlComment("If the item can be added to the global stash.")]
        public bool CanBeStashed { get; set; }
        [XmlComment("Can be traded by player or NPC stations")]
        public bool CanBeTraded { get; set; }
        [XmlComment("Can be found randomly (blueprint, derelict, etc)?")]
        public bool CanBeRandomDrop { get; set; }
        
        [XmlComment("Geology level required for mining: -1 = can't be mined")]
        public int GeologyRequired { get; set; }
        
        public bool IsCraftable { get; set; }
        public bool CraftingLevelAffectsYield { get; set; }
        public int CraftingYield { get; set; }
        public List<CraftMaterial> CraftMaterials { get; set; } //TODO: does CraftMaterial serialize correctly?
        [XmlComment("Still figuring out what it does.")]
        public int DefaultFabricatorId { get; set; } //TODO: is that needed? Do we need to set it for custom items?
        [XmlComment("Item ids of blueprints that will be unlocked when picking up this item.")]
        public int[] TechItemBlueprints { get; set; } //TODO: is this needed?
        
        //[XmlIgnore]
        [NonSerialized]
        public string FileName;

        public static string SanitizeFilename(string name) {
            return name.Replace("+", "_plus");
        }

        public bool ApplyPatch() {
            Item item = ItemDB.GetItem(Id);
            if (item == null) {
                return false;
            }
            
            ModAnyItem.LOGSource.LogDebug("Applying patch for: " + ItemName);
            
            item.itemName = ItemName;
            item.description = Description;

            item.rarity = Rarity;
            item.canUpgradeToTier = CanUpgradeToRarity;

            item.levelPlus = LevelPlus;
            item.weight = Weight;
            
            item.basePrice = BasePrice;
            item.priceVariation = PriceVariation;
            item.tradeChance = TradeChance.ToArray();
            item.tradeQuantity = TradeQuantity;
            
            item.type = Type;
            item.askedInQuests = AskedInQuests;
            item.canBeStashed = CanBeStashed;
            item.canBeTraded = CanBeTraded;
            item.randomDrop = CanBeRandomDrop;
             
            item.geologyRequired = GeologyRequired;
            
            item.craftable = IsCraftable;
            item.craftingLevelAffectsYield = CraftingLevelAffectsYield;
            item.craftingMaterials = CraftMaterials;
            item.defaultFabricatorID = DefaultFabricatorId;
            item.teachItemBlueprints = TechItemBlueprints;

            return true;
        }
    }
    
    // All this bullshit below is just to be able to serialize it with comments.
    // There is a crash any time [XMLAnyAttribute] is used and I can not be fucking arsed to implement a full ReadXml.
    
    [XmlRoot("ItemMod")]
    public class ItemModSerializable : ItemMod, IXmlSerializable {

        public static ItemModSerializable FromItem(Item item) {
            ItemModSerializable mod = new ItemModSerializable {
                Id = item.id,
                ItemName = item.itemName,
                Description = item.description,
                
                Rarity = item.rarity,
                CanUpgradeToRarity = item.canUpgradeToTier,
                
                LevelPlus = item.levelPlus,
                Weight = item.weight,
                
                BasePrice = item.basePrice,
                PriceVariation = item.priceVariation,
                TradeChance = TradeChance.FromArray(item.tradeChance),
                TradeQuantity = item.tradeQuantity,
                
                Type = item.type,
                AskedInQuests = item.askedInQuests,
                CanBeStashed = item.canBeStashed,
                CanBeTraded = item.canBeTraded,
                CanBeRandomDrop = item.randomDrop,
                GeologyRequired = item.geologyRequired,
                
                IsCraftable = item.craftable,
                CraftingLevelAffectsYield = item.craftingLevelAffectsYield,
                CraftMaterials = item.craftingMaterials,
                CraftingYield = item.craftingYield,
                DefaultFabricatorId = item.defaultFabricatorID,
                TechItemBlueprints = item.teachItemBlueprints,
                FileName = SanitizeFilename(item.itemName)
            };
            return mod;
        }
        
        public void WriteXml(XmlWriter writer)
        {
            PropertyInfo[] properties = GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                {
                    writer.WriteComment(
                        propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false)
                            .Cast<XmlCommentAttribute>().Single().Value);
                }
                
                object value = propertyInfo.GetValue(this, null);
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("", "");
                XmlSerializer serializer = new XmlSerializer(value.GetType(), new XmlRootAttribute(propertyInfo.Name));
                serializer.Serialize(writer, value, ns);
            }
        }
        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public XmlCommentAttribute(string value)
        {
            Value = value;
        }
        public string Value { get; set; }
    }
}