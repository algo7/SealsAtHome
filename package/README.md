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
- **Breeding** like vanilla: two happy, fed seals make a pup that's born tame (they have to stand really close, see
  How Things Behave).
- **Pups grow up** like vanilla cubs. Prefer the cute baby seal? Give it a name and it stays a baby forever. Clear
  the name and it grows up again.
- **Wild pups can be tamed** too.

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
*A pup is born.*

![A tamed pup named "Ultra Cute", with the Pet and Rename prompts](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/named-pup.jpg)
*Give a pup a name and it stays a baby forever.*

![An ice-block seal pen with an iron gate under a "Seals @ Home" sign](https://raw.githubusercontent.com/algo7/SealsAtHome/main/images/seal-pen.jpg)
*Welcome to Seals @ Home.*

## How Things Behave

- **Tamed seals never fight:** when something hurts them they run off for a while, then come back.
- **Wild seals behave like before:** lazy, they scoot away when you get very close and run when hit.
- **A scared seal won't eat.** If you spooked it, back off and give it half a minute.
- **Wild seals are afraid of fire:** near a campfire or hearth they get frightened and won't tame, so tame them away
  from fires. Tamed seals don't mind.
- Taming only progresses while the seal is fed and not frightened (see its hover text).
- **Breeding seals need to cuddle up. Really close.** The two have to be within 3 m of each other, counted from their
  middles. That's the game's own breeding rule, the same as for boars, not something this mod made up. But a seal is
  about 4 m long, and 3 m is only three 1x1 floor tiles. So one seal right behind the other never works: even touching,
  their middles are about 4 m apart, the length of two 2x2 floor tiles, and that's already too far. Put them
  **side by side**, touching. Starred seals only look bigger: their real body is the same size, so put them just as
  close, even if they seem to overlap.
- **Too many seals stop breeding:** with 5 or more seals and pups within about 10 m (wild ones count too), none of
  them breed. That's the game's rule too. Split a big herd up.
- **Your weapons don't hurt tamed seals.** Normally, with PvP off, only the butcher knife hurts a tamed animal. But the
  game has a special category of *provokable* creatures: peaceful towards players, but hurting one provokes it, like the
  Dvergr. Any weapon can hit them, whether they're peaceful or not. The game puts adult seals in that category, tamed
  ones too; seals just don't attack back, they only take the damage. Pups aren't in it, so tamed pups are safe with or
  without the mod. SealsAtHome takes adult seals out of that category: tamed seals are as safe as any tamed animal, and
  wild seals can still be hunted. In the games of friends without the mod, adult seals are still provokable, and thus
  vulnerable (see Multiplayer).
- **Starred pups:** Valheim never spawns wild seal pups with stars (the game's vanilla code, which this mod doesn't
  touch), but you can get one by breeding: a pup gets the stars of the seal that gives birth to it.

## Multiplayer

SealsAtHome makes changes to the game's own seals instead of adding new mod-only ones, so friends without the mod still
see your pets, and uninstalling the mod keeps them. A friend's game without the mod shows a tamed seal as tamed (green
health bar, its name), but can't feed it, command it or make it breed.

SealsAtHome only runs in players' games: dedicated servers don't run it. Valheim lets one player's game control each
creature at a time, usually whoever loaded the zone first. If that's a friend without the mod, your game takes the seals
near you under its control, so everything works: taming, feeding, follow / stay, names, breeding, pups growing up, and
anyone with the mod can command them. The first time, your game checks for up to half a minute that the friend really
doesn't have the mod. After that, your game takes control of seals controlled by that friend's game right away. Friends
who have the mod keep control of their seals. Tested in a few sessions with friends so far: please report how it goes.

Only when nobody with the mod is near does a seal behave like the unmodded game's seal for the moment. It stays tamed,
with its green health bar and name, but it doesn't eat, tame, breed or grow, and it wanders instead of following.
Nothing is lost: it all picks up again when someone with the mod comes by.

Players without the mod still see your tamed seals with green health bars, pet names, hearts and sounds. But adult
seals are still provokable in their game (see How Things Behave), so a stray swing or arrow of theirs hurts your tamed
adults; pups are safe. Best: everyone in your group has the mod, same version.

## Compatibility

- Built and tested for Valheim 1.0 (Deep North). Tested on Linux; there's no OS-specific code.
- **If another mod already changed the seals when a world loads** (gave them taming or a different AI, like "tame
  everything" mods), SealsAtHome leaves them alone and says why in `BepInEx/LogOutput.log`.
- **Other mods that change creatures as they spawn** (taming, AI or creature-overhaul mods) can clash with SealsAtHome
  without it noticing. If seals act strangely next to another mod, please open an issue.
- Mods that don't touch seals aren't affected: no game code is patched, nothing else changes, and there's no config file.
- Every language the game has: all texts are the game's own.

## Uninstalling

Nothing is stored in your characters. Tamed seals stay tamed in your world (green health bar, pet names, they don't run
from players), but they stop following and wander where they are, so tell them to **stay** at home first. Without the
mod they don't breed or grow up, and adult seals are provokable again, so any weapon hurts tamed adults (see How Things
Behave). Reinstalling picks up where you left off.

## Thanks

SealsAtHome started from [TameableSeals](https://thunderstore.io/c/valheim/p/pseudopulse/TameableSeals/) by pseudopulse:
thanks for the idea of tameable seals! TameableSeals has no public source to contribute to, so SealsAtHome was built
separately. It's a fresh take on the idea, built on the game's own taming, AI and breeding, so seals act like any
vanilla tameable, with their own sounds, breeding, and pups that grow up (or stay babies), and friends without the mod
still see your pets.

## Links

- Source and bug reports: https://github.com/algo7/SealsAtHome (issues welcome)
- Changes: the Changelog tab
- Made with AI assistance.
- Built with [BepInEx](https://github.com/BepInEx/BepInEx). MIT license.
