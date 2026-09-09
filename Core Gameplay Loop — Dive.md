## Core Gameplay Loop

The primary gameplay loop revolves around preparing the submarine, diving underwater, managing limited resources, catching fish, and safely returning to the surface.

### 1. Pre-Dive Preparation

Before starting a dive, the player can purchase upgrades for their submarine using money earned from previous dives.

These upgrades determine the submarine's capabilities for the upcoming dive, such as:

- Maximum Oxygen
- Maximum Fuel
- Cargo Capacity

The player then begins the dive.

### 2. Diving

During a dive, the player directly controls the submarine and explores the underwater environment.

The player can:

- Move around the underwater area.
- Search for and catch fish.
- Manage their available Oxygen, Fuel, and Cargo Capacity.
- Decide whether to continue exploring or return to the surface.

### 3. Fishing

When the player catches a fish, it is added to the submarine's cargo.

Each fish occupies one or more cargo slots depending on its size.

If the cargo is full when the player attempts to catch another fish, the player must choose which fish currently in the cargo to discard before the newly caught fish can be stored.

This creates a resource-management decision where the player must determine which fish are worth keeping.

### 4. Resource Management

Oxygen is continuously depleted while the player remains underwater.

The player must decide whether the potential value of additional fish is worth the risk of continuing the dive.

Fuel is consumed as the submarine explores the underwater environment and limits how far the player can travel.

Cargo capacity limits the amount of fish that can be brought back to the surface.

The three resources therefore create different types of pressure:

- **Oxygen:** How long the player can remain underwater.
- **Fuel:** How far the player can explore.
- **Cargo:** How much value the player can bring home.

### 5. Continue or Return

At any point during the dive, the player can choose to return to the surface.

If the player returns successfully, all fish currently stored in the cargo are brought back and sold automatically.

The money earned can then be used to purchase upgrades before the next dive.

Alternatively, the player can continue exploring in search of more valuable fish while taking the risk of exhausting their resources.

### 6. Oxygen Depletion

Oxygen represents the player's primary time limit underwater.

Oxygen continuously decreases at a fixed rate while diving.

If Oxygen reaches zero before the player returns to the surface, the dive is considered a failure.

All fish caught during that dive are lost.

The player then returns to the pre-dive state without receiving money from the failed dive.

### 7. Successful Dive

A dive is successful when the player voluntarily returns to the surface while still having sufficient Oxygen.

All fish collected during the dive are sold automatically for their respective values.

The player receives the resulting money and can use it to purchase upgrades for the next dive.

### Overall Loop

**Prepare → Dive → Explore → Catch Fish → Manage Resources → Continue or Return → Sell Fish → Upgrade → Next Dive**

## Procedural Stage System

The procedural stage system resolves around how much depth the player has reached, when a run is generated, it is divided into four separate zones,
each having a minimum depth requirement to reach the zone/stage. Each stage has a separate pool of fish to spawn, each having their own rarity and weight.

The player can go deeper he wants, finding better fishes, but the only risk he will ecounter is that getting back to the surface will be much longer,
the player must make a calculated decision if he wants to go deeper to get better fishes.