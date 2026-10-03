# SealsAtHome

![Four tamed baby seals at home, one of them named "Ultra Cute"](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/header.jpg)

Tame the Deep North's seals like any vanilla tameable: feed them raw fish, call them to follow or stay with E, breed
them at home, and keep their pups as babies if you like. Client-side, and everything other players see is vanilla: no
new creatures, items or effects.

## Quick Start

1. Install with a mod manager (r2modman, Thunderstore Mod Manager, Gale): BepInEx comes along automatically.
2. Find seals in the Deep North. Drop **raw fish** near one without scaring it off: it waddles over and eats it.
3. Keep it fed and calm. After 30 minutes it's tame. Press **E** to make it follow you or stay.

## Features

- **Taming like vanilla:** raw fish, the usual hearts and messages.
- **Follow, stay and rename** just like vanilla tamed animals, with their own seal sounds.
- **Breeding** like vanilla: two happy, fed seals make a pup that's born tame.
- **Pups grow up** like vanilla cubs. Prefer the cute baby seal? Give it a name and it stays a baby forever.
- **Wild pups can be tamed** too: bring one home with the Abyssal Harpoon.
- **Tamed seals never fight:** when something hurts them they run off for a while, then come back.
- **Wild seals behave like before:** lazy, they scoot away when you get very close and run when hit.

## Screenshots

![A wild seal being tamed: "Seal (Tameness 1%, Acclimatizing)" with a golden taming heart](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/taming-seal.jpg)
*Taming a wild seal: drop raw fish, keep your distance, and its tameness rises.*

![A wild baby seal being tamed: "Baby Seal (Tameness 1%, Acclimatizing)"](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/taming-pup.jpg)
*Wild pups can be tamed too.*

![Two tamed seals at home, with green health bars](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/tamed-seals.jpg)
*Tamed seals at home.*

![Breeding: pink love hearts over two tamed seals and their pup](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/breeding.jpg)
*Two fed, tamed seals close together fall in love.*

![A tame baby seal born next to its parents](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/baby-seal.jpg)
*A pup is born, already tame.*

![A tamed pup named "Ultra Cute", with the Pet and Rename prompts](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/named-pup.jpg)
*Give a pup a name and it stays a baby forever.*

## How Things Behave

- **A scared seal won't eat.** If you spooked it, back off and give it half a minute.
- Taming only progresses while the seal is fed and not frightened (see its hover text).
- With PvP off, only the butcher knife hurts tamed seals, as with any tamed animal. They never despawn.
- Pups keep aging while you're away, as long as the world (or server) is running.

## Multiplayer

SealsAtHome runs only on your client; servers don't need it. The game lets one player's game run each creature
(whoever was there first keeps it). Taming, commands, naming, breeding and growing up only happen while that's a
player **with** the mod. Players without it still see vanilla seals, the green health bar on tamed ones, pet names and
all effects; they just can't command or tame them, and their weapons can still hurt tamed adult seals (vanilla seals
are "provokable" like the Dvergr; pups aren't). Best: everyone in your group has the mod, same version.

## Compatibility

- Built and tested for Valheim 1.0 (Deep North). Tested on Linux; there's no OS-specific code.
- If another mod has already changed the seals when a world loads (TameableSeals, "tame everything" mods), SealsAtHome
  leaves them alone and says why in `BepInEx/LogOutput.log`. Remove one of the two.
- No game code is patched and there's no config file: only the two seal creatures change.
- Every language the game has: all texts are the game's own.

## Uninstalling

Nothing is stored in your characters. Tamed seals stay tamed in your world (green health bar, pet names, they don't
run from players), but they stop following and wander where they are, so tell them to **stay** at home first. Without
the mod they don't breed or grow up, and any weapon can hurt tamed adults again (a vanilla seal quirk). Reinstalling
picks up where you left off.

## Links

- Source and bug reports: https://github.com/algo7/SealsAtHome (issues welcome)
- Changes: the Changelog tab
- Made with AI assistance.
- Built with [BepInEx](https://github.com/BepInEx/BepInEx). MIT license.
