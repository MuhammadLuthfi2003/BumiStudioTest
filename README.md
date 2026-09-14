# Depth Charge

## Game Overview
Depth Charge is a 2D Roguelike, where players dives into an ocean riding inside a submarine, then catches fish with it, and afterwards return to the surface.
Sell the fishes, earn money and buy more upgrades to get better submarine stats, like better oxygen, more health, and more cargo.
With better stats, players can dive for much more longer and catch more fishes. But be careful, getting too deep and your chance of going back to the surface becomes smaller since you had to return to the surface to sell your fish, fail to return and your catch for the dive is wiped, forever.

There are three resources that affects the current dive, which are, oxygen, health, and cargo. 

1. Oxygen
This resource dictates how long the submarine is able to dive into the deep sea to catch fish, this resource decreases over time during the dive no matter
what the player does. When it hits zero, instantly returns the player back to the lobby.

2. Health
This resource dictates the current submarine health it was in. When the submarine hits a hazard, this stat decreases. When this stat reaches zero, also instantly
returns the player back to the lobby.

3. Cargo
This resource dictates how much the weight of the fishes you can carry. Some fishes are heavier than others, thus consuming more cargo space. When it hits the max amount or when the player catches a fish that has uses space than the current remaining cargo space, the player will be unable to catch more fish until the some cargo space of the submarine has been freed (by selling for example).

## How to Run
1. Extract ZIP file of the game
2. Run the game's .exe file (for windows build)

## Game Controls
- WASD to Control the submarine
- E to catch fish (when the fish is reachable by the submarine)
- Mouseclick to upgrade the submarine's stats

To start the game, just move the submarine to the lifebuoy near the "start game" panel.
To return to the surface, move the submarine to the lifebuoy near the "return to surface" panel.
After a successful dive, to sell the caught fish, move the submarine to the lifebuoy near the "sell fish" panel.
To upgrade a stat, simply press the button near the desired stat using a mouse.

## Game Information
- Engine & version used: Unity 6000.0.83f1
- Build location: /Build/ or [link]

## Technical Decisions
Here are some technical decisions made during the project:
1. Data-driven design using ScriptableObjects
ScriptableObjects such as FishData, HazardData, ZoneData and etc. allows new content to be added easily (e.g new zones, new hazards, new fish)
without code changes/minor code changes. With that, it allows a designer to freely balance/create new asset without having to touch the coding part of the game.

2. Event-driven decoupling
Nearly every manager inside the game exposes C# Event Action rather than being polled or directly called. It keeps systems ignorant of each other's existence. For instance, ResourceManager exposes a lot of events, such as OnCargoChanged, OnOxygenChanged and OnHealthChanged. For this case, the UI (ResourceDisplay) is set to receive those events when broadcasted. The three events are broadcasted when there is a change happening on submarine's resource, after that ResourceDisplay receives the data and then process it in its own way to display the current resource.

This way is much better than having to check the ResourceManager's value every frame and then display it onto the UI.

3. Central Service locator via GameManager.Instance as source of truth
Since there is a lot of scripts relying on Managers (such as ResourceManager, DiveManager), rather than having to setup in a lot of gameobject/scripts that need the object, all of them are set up via the GameManager. So that when a script needed a manager, the script will instead access GameManager.Instance rather than having to 
manually refrence them via the inspeactor.


## What I Would Do With More Time
If i were to had more time with the project, this is what i would do:
- 

## Known Issues
List any bugs or incomplete features you are aware of.
An honest list here is valued over leaving the reviewer to discover them silently.
