# GOAP Dungeon

[![Unity](https://img.shields.io/badge/Unity-2018.3+-black?logo=unity&logoColor=white)](https://unity.com/)
[![C#](https://img.shields.io/badge/C%23-7.0%2B-239120?logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![AI](https://img.shields.io/badge/AI-GOAP-blueviolet)](#)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-PC-lightgrey)](#)
[![Status](https://img.shields.io/badge/Project-Type%3A%20Academic-blue)](#)
[![Made With ❤️](https://img.shields.io/badge/Made%20with-%E2%9D%A4-red)](#)

**Authors:** Lautaro Bravo de la Serna, Andrea Alonso  
**Year:** 2018–2019  
**Language:** C# (Unity 3D)  
**Type:** Academic AI Project  

---

## Overview

**GOAP Dungeon** is a Unity project created as a final assignment for the Artificial Intelligence II course.  
Its main goal was to implement a complete **Goal-Oriented Action Planning (GOAP)** system controlling an autonomous agent inside a procedural dungeon environment.

The AI agent explores the dungeon, gathers resources, avoids threats, and executes a plan to fulfill high-level goals such as **defeating all bosses** or **rescuing all hostages** — all while discovering the world dynamically, without prior map knowledge.

---

## Dungeon World

The dungeon is divided into **zones**, each containing:
- **Enemies:** Kobolds, Undeads, and Dragonids (ranked by difficulty).
- **Objects:** Potions, energy orbs, hostages, and color-coded keys.
- **Bosses:** Dragon (Yellow door), Golem (Red door), Hydra (Blue door).

Zones are connected in a network structure.  
When the agent enters a new zone, it can **investigate**, **collect**, **fight**, or **move** to an adjacent one.

---

## ⚙️ GOAP Implementation

The project integrates a classic **GOAP planner** inspired by:
- *Goal-Oriented Action Planning – GDC Talk*  
  https://www.youtube.com/watch?v=gm7K68663rA  
- *Three States and a Plan: The AI of F.E.A.R.*  
  https://www.gamedevs.org/uploads/three-states-plan-ai-of-fear.pdf  

Each action has:
- **Cost**
- **Preconditions**
- **Effects**

Example actions:

| Action | Cost | Preconditions | Effect |
|--------|------|----------------|---------|
| Investigate | 1 | Zone not investigated | Marks zone as explored |
| PickUp | 2 | Objects present, no enemies | Adds collected items |
| UsePotion | 3 | Potions > 0, HP < 60% | Restores HP |
| MeleeKill | 5 | Enemies > 0, HP > 0 | Damages or clears enemies |
| RangeKill | 3 | Energy > 0.5, Enemies > 0 | Uses energy, clears enemies |
| Flee | 6 | HP < 20% | Returns to last safe zone |
| OpenDoor | 1 | Has key | Unlocks new zone |
| KillBoss | 1 | Boss alive, HP > 0 | Damages or kills boss |
| Revive | 100 | HP ≤ 0 | Respawns in last cleared zone |
| SwitchZone | 45 | Zone investigated, no enemies | Moves to adjacent zone |

The planner links these actions to a **Finite State Machine (FSM)** controlling animations and transitions in Unity, enabling the character to react dynamically to the environment.

---

## Implementation Notes

During development, we faced and solved multiple challenges:

- **Invalid action linking:** Fixed using strict preconditions (e.g., can’t pick up items before investigating).  
- **Heuristic tuning:** Adjusted cost weights for better path quality.  
- **Planning time:** Reduced by removing redundant ToList() calls and introducing lazy evaluation.  
- **World state mutation:** Solved by cloning dictionaries instead of referencing originals.  
- **Combat planning:** Merged melee and ranged into a single action to reduce graph complexity.  
- **Failure handling:** Added re-planning when actions fail due to unexpected world changes.

These optimizations reduced the planning time from hours to seconds while maintaining plan validity.

---

## Key Concepts Demonstrated

- **GOAP (Goal-Oriented Action Planning)**
- **Heuristic cost-based planning**
- **Dynamic world state mutation**
- **Finite State Machine integration**
- **Zone-based knowledge discovery**

---

## Technologies Used

- **Unity 3D (2018.3)**
- **C#**
- **Custom GOAP system**
- **Finite State Machine (FSM)**

---

## Folder Structure

```

/Assets/Scripts/
│
├── GOAP/
│   ├── ActionPlanner.cs
│   ├── Goal.cs
│   ├── WorldState.cs
│   └── ...
│
├── AI/
│   ├── Agent.cs
│   ├── PlayerStateMachine.cs
│   └── ...
│
└── Game/
├── DungeonZone.cs
├── Enemy.cs
├── Collectable.cs
└── Boss.cs

```

---

## 📖 License

This project is licensed under the **MIT License**.  
You are free to use, modify, and distribute it for educational or research purposes.

---

## Notes

This project was created before the widespread use of large language models such as ChatGPT.  
All design, planning, and debugging were performed manually using traditional AI and gameplay programming techniques.

---

## Contact

**Lautaro Bravo de la Serna**  
[lautarobravo.com](https://lautarobravo.com)  
[@TaaroBravo](https://github.com/TaaroBravo)
