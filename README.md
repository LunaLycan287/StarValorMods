# StarValorMods

All my Star Valor mods require [BepInEx 5.4+](https://github.com/BepInEx/BepInEx/releases).

## Mod Any Item
Mod attributes of any Item.
Check out it's own [Readme](ModAnyItem/Readme.md) for item information.

### Install
Note: it is suggested to back up your save file first if you want to try things.
- Copy `LL_SV_ModAnyItem.dll` into `Star Valor\BepInEx\plugins`
- Start the Game
- Wait for it to load
- Close the Game
- Edit any items you want to change in `Star Valor\BepInEx\plugins\ItemMods`
- Save your edits
- Start the Game

### Uninstall
I have no idea about the safety of uninstalling after using changed items in a Save.  
I imagine stashed items need to be retrieved and you might need to store everything in the station so you do not go over capacity.  
<span style="color:red">Save file might be corrupted.</span>

## Change Background Perk
Change your current background Perk to another one.

### Install
- Copy `LL_SV_ChangeBackgroundPerk.dll` into `Star Valor\BepInEx\Plugins`
- Start the Game
- Set your perk in `Star Valor\BepInEx\config\lunalycan287.starvalormods.changebgperk.cfg`
- Load your save
- Open the Perk screen with `P` and the Perk will be added

## Dumper
Dump the Database files into working CSharp files for use in any mod projects.

### Install
- Copy `LL_SV_Dumper` into `Star Valor\BepInEx\Plugins`
- Start the Game
- Press the configured key (Default: F10)
- Copy the required dump files from `Star Valor\dump` into your project.

### Configuration
- Edit `Star Valor\BepInEx\config\lunalycan287.starvalormods.dumper.cfg`
- Set the `KeyBind` to your preferred key. Only lower case allowed! Check [possible KeyCodes](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/KeyCode.html).
- Set `Compact` to true, if you do not want an extra line between the enum values.

## More Skill Points
Adjust how many skill points you get during a normal game.

### Install
- Copy `LL_SV_MoreSkillPoints.dll` into `Star Valor\BepInEx\Plugins`
- Start the Game
- Set the ammount of Skill Points in `Star Valor\BepInEx\config\lunalycan287.starvalormods.moreskillpoints.cfg` (Default: 125 = enough for all)
- Load your save
- Open the Character screen with `C` and the Skill Points will be adjusted.

### Uninstall
- Set the value of `uninstall = true` in  `Star Valor\BepInEx\config\lunalycan287.starvalormods.moreskillpoints.cfg` 
- Start the Game
- Load your save
- Open the Character screen with `C` 
- Click on `Reset Skills`
- Save & quit your game
- Remove the mod

## Perk Unlock Helper
Displays helpful unlock information on Perks that are still locked.  
Perks that can not be unlocked in the current run are Red.  
Perks that can be unlocked but either require an Equipment change or are about to be locked are Yellow.  

### Install
- Copy `LL_SV_PerkUnlockHelper.dll` into `Star Valor\BepInEx\Plugins`
- Start the Game
- Load your save
- Press the configured key (Default: F8)

### Configuration
- Edit `Star Valor\BepInEx\config\lunalycan287.starvalormods.perkunlockhelper.cfg`
- Set the `KeyBind` to your preferred key. Only lower case allowed! Check [possible KeyCodes](https://docs.unity3d.com/6000.1/Documentation/ScriptReference/KeyCode.html).
- Set `ShowImages` to `true` if you want to see the skill icons even before unlocking.
- Set `ShowHiddenUnlockConditions` to `true` if you want to see ALL unlock conditions, even of the perks which hide them by default.
- Set `ShowPreviouslyUnlocked` to `true` if you want to see perks that have been perviously unlockde but not acquired in the current save.