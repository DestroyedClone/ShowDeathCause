# ShowDeathCause
This mod lets you know how your and your friends miserable lives ended. This is based on the original [ShowDeathCause](https://github.com/MOV-MB/ShowDeathCause) with some improvements.

## Features
The death notice lets everyone know what killed you and how. The end game report screen is improved now too! This fork provides the following improvements over the old version:
- Works with the latest game version as of the anniversary update
- Handles a few more cases
- Prints the name of the dead player along with damage taken
- Updates the end game report screen to include the enhanced information

| Chat Message (Before) | Chat Message (After) |
| ----- | ----- |
| ![Before](https://raw.githubusercontent.com/NotTsunami/ShowDeathCause/master/ExampleChatBefore.jpg) | ![After](https://raw.githubusercontent.com/NotTsunami/ShowDeathCause/master/ExampleChatAfter.jpg) 

| Game End Report (Before) | Game End Report (After) |
| ------ | ------ |
| ![Before](https://raw.githubusercontent.com/NotTsunami/ShowDeathCause/master/ExampleBefore.jpg) | ![After](https://raw.githubusercontent.com/NotTsunami/ShowDeathCause/master/ExampleAfter.jpg) |

_* The player is killed by the same type of monster, a Glacial Beetle, in both cases_

_** Screenshots are taken on ShowDeathCause 2.0.0 with the anniversary update_

## Installation
It is highly recommended to use [r2modman](https://thunderstore.io/package/ebkr/r2modman/) to install ShowDeathCause because it will set up everything for you! If you are installing manually, you will need to make a folder in `Risk of Rain 2\BepInEx\plugins` called `ShowDeathCause` and drop the contents of the zip into it. The Language folder must be included, otherwise the mod will fail to function correctly.

## Changelog
### Version 3.0.0
- R2API dependency has been removed, making ShowDeathCause vanilla-compatible!
- Language support has been added
    - If you would like to contribute a translation, please look at the section below!
- Subscribe to `onCharacterDeathGlobal` instead of hooking at runtime

### Version 2.0.2
- Fixed NullReferenceException when the killer's body no longer exists in between the death message and end game screen
    - This happened most commonly when being killed by a Jellyfish

### Version 2.0.1
- Reverted change to remove original death message as this may break other mods that depend on `OnPlayerCharacterDeath`

### Version 2.0.0 - Happy almost 100k downloads!
- The biggest change in this update is that the end game report screen now includes the same information as the death notice
- Elite/Umbra prefixes are now shown (Thanks WolfgangIsBestWolf for the suggestion!)
- Fall damage is now labeled as such (Can occur when max HP <= 1 or with artifact)
- Friendly fire kills are now attributed to the display name of the player who killed them
- The original red death message is no longer shown, only the messsage from ShowDeathCause is printed in chat now

### Version 1.0.5
- Switched to local stripped libs instead of relying on game's installation
- Updated for anniversary update

### Version 1.0.4
- Updated for Risk of Rain 2 version 1.0

### Contributing a Translation
Thank you for your interest in contributing a translation! You can contribute a translation by following the steps below:
1. Make a folder in the `Language` folder with the ISO 639-1 code as the name. For example, if I were contributing a French translation, the correct path would be `Language/fr`.
2. Copy the `sdc_tokens.json` file from `Language/en` to your newly created folder.
3. Add your translations!

Anything in brackets ({}) or wrapped in <> does not need to be translated. For example, if you were translating the `SDC_PLAYER_DEATH` token, which reads `<color=#00FF80>{0}</color> killed by <color=#FF8000>{1}</color> ({2} damage taken).` as of version 3.0.0, you would only translate "killed by" and "damage taken".

## Credits to the Original Authors
[MOV-MB](https://github.com/MOV-MB)
[MagnusMagnuson](https://thunderstore.io/package/MagnusMagnuson/)

[Skull icon](https://icons8.com/icons/set/skull) by [Icons8](https://icons8.com).
