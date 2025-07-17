# Mod Any Item
This mod lets you mod any item in the game.  

## Editing Existing items
Files for all existing items will be created in `Star Valor\BepInEx\plugins\ItemMods` and you can then adjust the values to your needs.

## Creating Custom Items
Not Yet Implemented!

## Elements Explained:
- `Id` = The item id. <span style="color:red">Never edit this.</span>
- `ItemName` = The name displayed for the item.
- `Description` = Description for the item. If an item has it, it is shown in green directly below the title.
- `Rarity` = The Default Rarity of the item. 
  - 0 = Poor 
  - 1 = Common 
  - 2 = Uncommon 
  - 3 = Rare 
  - 4 = Epic 
  - 5 = Legendary
- `CanUpgradeToRarity` = To which Rarity the item can upgrade other items.
  - 0 = Poor = Can not be Upgraded
  - 1 = Common
  - 2 = Uncommon
  - 3 = Rare
  - 4 = Epic
  - 5 = Legendary
- `LevelPlus` = ??? have not figured it out yet
- `Weight` = The weight / cargo space an item has. It can take float values so `0.5` would be a valid value.
- `BasePrice` = The base price for an item.
- `PriceVariation` = Is used to adjust the BasePrice and set some variation per station.
- `TradeChance` = The chance to be able to trade the item in one of the factions stations.
  - Is split up into sub nodes for each faction.
  - Values should be `0` to `100`
- `TradeQuantity` = ??? have not figured it out yet
- `Type` = What Type the item is of.
  - Data
  - Crystal
  - Metal
  - Food
  - Electronic
  - Ammunition
  - Chemical
  - Utility
  - Other
  - Package
  - Container
  - Organic
  - Component
  - CraftingComponent
- `AskedInQuests` = If the item can be requested in generated quests (like supply quests).
- `CanBeStashed` = If the item can be added to the Global Stash.
- `CanBeTraded` = If the item can be traded by the player or NPC stations
- `CanBeRandomDrop` = If the item can be randomly dropped (e.g.: blueprint, derelict, etc)
- `GeologyRequired` = Geology level required to mine it.
  - -1 = can not be mined
- `IsCraftable` = If the item can be crafted
- `CraftingLevelAffectsYield` = If your crafting level affects how high your yield is when crafting.
- `CraftingYield` = How many items you get from crafting.
- `CraftingMaterials` = A list of all the materials requried to craft.
  - Example for Microchip:
  ```xml
  <CraftMaterials>
    <CraftMaterial>
      <itemID>42</itemID> <!-- Same as `Id` in the .itemmod files. 42 = Scrap Metal -->
      <quantity>1</quantity>
    </CraftMaterial>
    <CraftMaterial>
      <itemID>29</itemID> <!-- Same as `Id` in the .itemmod files. 29 = Silicon -->
      <quantity>2</quantity>
    </CraftMaterial>
  </CraftMaterials>
  ```
- `DefaultFabricatorId` = ??? have not figured it out yet
- `TechItemBlueprints` = Item ids of blueprints that will be unlocked when picking up this item.
  - Example for Microchip
  ```xml
  <TechItemBlueprints>
    <int>23</int> <!-- Same as `Id` in the .itemmod files. 23 = Drone Parts -->
    <int>47</int> <!-- Same as `Id` in the .itemmod files. 47 = Upgrade Kit -->
  </TechItemBlueprints>
  ```
## Credits
This mod is inspired by [Mod Any Ship](https://github.com/MPC88/MC_SVWeModAnyShipDotCom).