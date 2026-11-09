# DSR Gadget Local Loader by Nordgaren and Meikk99

 Fork of TKGP's DSR Gadget that loads locally accessible .txt files to populate various lists (bonfire, items, etc) Resource .txt files are found in DS-Gadget\Resources.
Many features implimented by [Meikk99](https://github.com/Meikk99/DSR-Gadget), including multiplayer features and most of the functionality in this fork

## Requirements
* [.NET 4.7.2](https://www.microsoft.com/net/download/thank-you/net472)

## Installing

* Extract contents of zip archive to it's own folder. You may have to run as admin if DS Gadget crashes

## Updating

* If you have a previous version of DSR Gadget Local Loader with a Resource folder, there is no need to copy the resource folder over, unless noted in the patch notes. This prevents you from overwriting the changes you made and the saved positions you stored.

## Troubleshooting
If you are having problems getting it to launch and you tried installing the package above, right click the exe, go to properties, hit the compatability tab and hit "Run compatability troubleshooter". If it works, apply the settings windows found.

You can also try adding DS Gadget as an exception to your antivirus

#### Based on [DSR-Gadget](https://github.com/JKAnderson/DSR-Gadget) by JKAnderson.
#### Fork of [DSR-Gadget](https://github.com/Meikk99/DSR-Gadget) by Meikk99.

## Thank You
**[TKGP](https://github.com/JKAnderson/)** Author of DS Gadget and answering my dumb questions  
**[Meikk99](https://github.com/Meikk99/)** For his improvements on DSR Gadget  
**[King Borehaha](https://github.com/kingborehaha/)** For helping with DS Gadget Local Loader and helping test various features  

# Change Log

## **Best bet is to use this offline for now.**
### Release 1.8.2 (catofwonders fork)

* Params tab is much faster: params load in the background, the field list fills in one go, and scrolling no longer redraws line by line

### Release 1.8.1 (catofwonders fork)

* Fixed a crash on startup caused by the Params tab layout

### Release 1.8.0 (catofwonders fork)

* Params tab speaks English: plain field names ("Damage: Physical", "Weight", "Requirement: STR") instead of internal names
* A help box explains what the selected field does
* Fields with set choices (weapon category, element, etc.) and on/off flags are dropdowns
* Fields that point at another param show what they point to (e.g. which special effect)
* Friendly param names in the dropdown ("Weapons", "Spells", "Enemies and NPCs"...) and a field finder
* English names, descriptions and value lists from Smithbox by Vawser and contributors (MIT license), in Resources/Params/Smithbox

### Release 1.7.0 (catofwonders fork)

* New Params tab: a live param editor. Pick any of the game's 38 params, find a row by ID or name, and edit its fields while the game runs
* Changed values are highlighted (hover to see the original); Reset row / Reset all changes put them back
* Changes are in memory only: game files are never touched, and restarting the game undoes everything
* Param definitions and row names come from Paramdex (https://github.com/soulsmods/Paramdex), in Resources/Params

### Release 1.6.0 (catofwonders fork)

Ported from Phokz's Dark Souls Remastered Cheat Engine table:

* Cheats tab: Access everything in shops, Upgrade without materials, No fall damage, Force quick or ninja rolls
* Graphics tab: Hide HUD, Custom field of view, Set draw distance, Extra lock-on range
* Everything is put back to normal when you untick it or close DSR Gadget

### Release 1.5.2 (catofwonders fork)

* Fixed a crash when reading or writing an unsupported event flag ID
* Fixed a crash when the game reports health or stamina outside the displayed range
* Fixed a crash when position values are outside the displayed range
* Includes the "anim cancel does standup" change from the test0503 branch
* Builds are now made automatically on GitHub (see Releases)

### Release 1.5.1

* Hair Color script now persists through loading screens  

### Release 1.5

* Hair color script that makes your hair do the RGB thing. 

* Arrow refill cheat that gives all arrows and constantly refills them.  

### Release 1.4

* Heal Timer  

* Crash fix for loading gadget while game is open and when restoring postion  

* "Quick Select Bonfire" on the player tab and "Max", "Restrict", and "Search All" all have saved states in gadget settings, now (They will remember if they are checked or not when re-opening Gadget)  

### Release 1.3

* (Player tab) Freeze position checkbox. When position frozen you can move your character with the current position numeric boxes  

* (Player tab) Saved positions. Give a position a name and it will be stored in an XML file in the Resources folder, and loaded when you reload Gadget.  

* (Misc tab) Fashion items in the misc tab. Apply items to your hair bolt or arrow slot for fashion purposes.  

* (Misc tab) Hair and eye color pickers (replaces the hair and eye color in the fashion tab in Stats)  

### Release 1.2

* Fixed Humanity and Souls boxes in Stats tab  

### Release 1.1

* Fixed the "Unlock Features" checkbox  

### Release 1

* Game info is now loaded through local text files in Resource folder  
* Search bar for Bonfire and Items no available to use  
* Reworked how selecting bonfire would change your currently rested bonfire, and gave a checkbox for the old functionality  
* Max checkbox for items (works for upgrade and quantity)  
* Checkbox to enable untested or features that are known to ban.  

Changes from Meik99s gadget:  
* Add options for changing and freezing Character, Team and Invasion Type.  
* Add options for changing and freezing Area and Multiplayer Area ID.  
* Add options for teleporting to bloodstain and initial positions.  
* Add Infinite durability cheats.  
* Add reset magic quantity button.  
* Add saving of magic quantity to save state feature.  
* Add various hotkey options.  
* Add stat info on recent players  
* View recent and current players  
* Kick player  
* Leave session (button and shortcut)  
* Change name  
* Change covenant  
* Change covenant level  
* Change weapon level  
* View indictments  
* Change hair slot  
* Change hair colour  
* Change Eye colour  
* Add steam family share check (set api key in settings)  
* Add equipment info to current/recent players and summon sign info  
* Ability to level up stats directly  
* Added NG + Cycle  



--------------------------------------------------------------------------------------------------------------------------------------
Original readme:

# DSR Gadget 1.6 - By Pav & TKGP
A multi-purpose testing tool for Dark Souls: Remastered.  
Compatible with DSR App ver. 1.01, 1.01.1, 1.01.2, 1.03, and possibly future versions.  
Requires [.NET 4.7.2](https://www.microsoft.com/net/download/thank-you/net472) - Windows 10 users should already have this.

# Special Thanks
*Villhellm*, for generalizing the hotkey UI and kicking off controls not being frozen when they shouldn't be.

# Credits
[Costura.Fody](https://github.com/Fody/Costura) by Simon Cropp, Cameron MacFarland

[LowLevelHooking](https://github.com/jnm2/LowLevelHooking) by Joseph N. Musser II

[Octokit](https://github.com/octokit/octokit.net) by GitHub

[Semver](https://github.com/maxhauser/semver) by Max Hauser

# Changelog
### 1.6
* Fixed cheats and other things staying on if you unchecked them while unloaded
* Fixed no gravity, no collision, and filters not reapplying properly after reloads

### 1.5
* Supports 1.03
* Controls that don't need the game to be loaded can now be used without the game being loaded
* Added hotkey to create whatever's selected in the Items tab
	
### 1.4
* Add basic event flag support (Misc tab)
* Fix stable position not working on 1.01.2
* Fix stable angle never working in the first place
* Fix no gravity and no collision not reapplying after load screens
* Fix camera storing/restoring not working sometimes
* Hooking the game is much faster now

### 1.3
* Supports 1.01.2
* Camera is restored with positions
* The download link actually works now

### 1.2
* Supports 1.01.1 and hopefully all future versions automatically
* Option to restore state with position (but not camera, yet)
* Add cut covenant items to spawner

### 1.1
* Fixed crash with out of range player angle
