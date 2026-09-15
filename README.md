# 🌊 Depth Charge

*A 2D underwater roguelike about greed, oxygen, and knowing when to turn back.*

---

## 📖 Game Overview

**Depth Charge** is a 2D roguelike where you pilot a submarine into the deep ocean to catch fish, then race back to the surface before your resources run out.

Sell your catch, earn money, and upgrade your submarine's stats — better oxygen tanks, sturdier hull, bigger cargo hold — so you can dive deeper and bring back more valuable fish.

But the deeper you go, the longer the trip home. Push too far and you risk running dry before you reach the surface — and if you don't make it back, **your entire catch is lost. Forever.**

> **The core tension:** every extra meter down is a better fish and a longer road home.

---

## ⚙️ Resources

Three resources govern every dive. Run out of any of them, and it's an instant trip back to the lobby — empty-handed.

| Resource | Icon | What it does | Fails when... |
|----------|:---:|---------------|----------------|
| **Oxygen** | 🫧 | Constantly depletes over time, regardless of what you do. Your hard time limit underwater. | It hits zero |
| **Health** | ❤️ | Drops when the submarine collides with a hazard. | It hits zero |
| **Cargo** | 🐟 | Fills up as you catch fish — heavier fish take up more space. | You can't fit another catch |

---

## 🕹️ Controls

| Input | Action |
|-------|--------|
| `W` `A` `S` `D` | Move the submarine |
| `E` | Catch a fish *(when in range)* |
| Mouse Click | Purchase a submarine upgrade |

---

## ▶️ How to Run

1. Extract the game's ZIP file.
2. Run the `BumiStudioTest.exe` file *(Windows build)*.

---

## 🎯 Gameplay Loop

```
Lobby → Dive → Explore & Fish → Manage Resources → Return → Sell → Upgrade → Repeat
```

- **Start a dive** — steer the submarine to the lifebuoy near the *"Start Game"* panel.
- **Return to the surface** — steer to the lifebuoy near the *"Return to Surface"* panel.
- **Sell your catch** — after a successful dive, move to the lifebuoy near the *"Sell Fish"* panel.
- **Upgrade a stat** — click the button next to the stat you want to improve.

---

## 🛠️ Game Information

| | |
|---|---|
| **Engine** | Unity `6000.0.83f1` |
| **Build Location** |  `https://sofutobekkusu.itch.io/depth-charge` |

---

## 🏗️ Technical Decisions

### 1. Data-Driven Design via ScriptableObjects
Content like `FishData`, `HazardData`, and `ZoneData` lives entirely in ScriptableObject assets. This means new fish, hazards, and zones can be added — or rebalanced — with little to no code changes, letting a designer iterate freely without touching gameplay code.

### 2. Event-Driven Decoupling
Nearly every manager exposes C# `event Action`s instead of being polled or called directly, keeping systems ignorant of one another.

For example, `ResourceManager` broadcasts `OnCargoChanged`, `OnOxygenChanged`, and `OnHealthChanged` whenever a resource changes. `ResourceDisplay` listens for these events and updates the UI accordingly — no per-frame polling required.

### 3. Central Service Locator via `GameManager.Instance`
Rather than manually wiring manager references into every script that needs them via the Inspector, all systems fetch what they need through `GameManager.Instance`, which acts as the single source of truth.

---

## 🚀 What I Would Do With More Time

| # | Feature | Why it matters |
|---|---------|-----------------|
| 1 | **Inventory Discard System** | Let players discard fish from cargo to make room for better catches. |
| 2 | **Fish Behavior / Movement** | Static fish trivialize catching — moving fish that flee would add challenge. |
| 3 | **Enemies & Behavior** | Hazards alone are easy to avoid; active enemies would raise the stakes. |
| 4 | **Zone-Dependent Oxygen Drain** | Deeper zones should drain oxygen faster, mirroring real-world depth pressure. |
| 5 | **Terrain Generation** | Dive zones are currently open space — procedural terrain would add texture and challenge. |
| 6 | **Game Polish** | See below. |

**Polish backlog:**
- No indicator showing whether a fish is currently catchable (e.g. an "E" prompt).
- No visual distinction between hazards and fish (e.g. a red outline on hazards).
- A proper death screen.
- Low-resource warning indicators (oxygen/health nearing zero).
- SFX and background music.

---

## 🐞 Known Issues

- **No hazard indicator** > hazards are hard to spot and avoid before it's too late.
- **Low player clarity** > no visual cue for when a fish is within catching range.
- **Catched Fish Resets on Quit** > when player catcher a fish and quits the game, the fish in the cargo isnt saved onto the persistent data.
---
