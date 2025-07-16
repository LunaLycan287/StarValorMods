# StarValorMods

All my Star Valor mods require [BepInEx 5.4+](https://github.com/BepInEx/BepInEx/releases).

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