
# Unity DOTS RTS Game Demo

This project is a real-time strategy (RTS) game demo built with Unity's **Data-Oriented Tech Stack (DOTS)**. 
It leverages the **Entity Component System (ECS)**, **C# Job System**, and **Burst compiler** to achieve high performance and scalability. 
The focus is on showcasing complex RTS gameplay systems such as pathfinding, unit production, resource collection, combat, and more implemented in a fully data-oriented way,
while overcoming technical limitations such as the lack of built-in animation support in DOTS.


https://github.com/user-attachments/assets/3f27711d-345b-454e-a194-3081cd2924b9


---

## 🔧 Key Technical Features

### **Data-Oriented ECS Architecture & Performance**
The entire game is built using Unity ECS. Gameplay logic is decoupled into systems, and all runtime data is stored in components. 
Computationally intensive tasks like pathfinding and animation playback are executed using multithreaded jobs and optimized with Burst.

- Systems are designed to minimize allocations and maximize cache efficiency.
- Most logic runs via `IJobEntity` and `SystemBase`, allowing the simulation to scale to hundreds of units without frame drops.

---

### **Custom ECS Character Animation System**
Since Unity DOTS does not support GameObject-based animation, this project introduces a fully custom animation solution:

- Skinned mesh animations are baked into static mesh frames during preprocessing.
- A lightweight ECS-based animation system updates the rendered mesh each frame based on animation state.
- Supports multithreaded playback and integrates seamlessly into ECS rendering pipelines.

This approach enables animated characters to exist entirely within ECS, demonstrating advanced adaptation of engine constraints.

---

### **Flow-Field Pathfinding**
Traditional per-unit pathfinding is replaced with a **flow field algorithm**:

- A vector field is generated over a grid that units consult to determine direction.
- Ideal for large-scale unit movement since it amortizes pathfinding cost.
- Implemented with ECS components and Burst jobs, allowing responsive movement for large groups with minimal overhead.

---

### **Base Building & Unit Production**
The project includes fully functioning RTS base-building mechanics:

- Players can place structures on the map using a placement system with validation and visual feedback.
- Structures like the **Barracks** feature dynamic unit production queues.
- Queues are load-balanced across multiple structures and visually represented in the UI.
- A **Headquarters (HQ)** building serves as the central command center.

---

### **Resource Gathering System**
- Resource nodes (e.g., minerals) are placed across the map.
- Worker Buildings gather resource from them.
- The system includes resource tracking and component-based management for scalability.

---

### **Fog of War (FoW) and Visibility**
The game includes a fully integrated Fog of War system:

- Visibility is tied to unit sight radius.
- Fog is revealed as units explore, and hidden again when out of sight.
---

### **Combat and AI Enemies**
- AI-controlled turrets rotate and fire at nearby enemies.
- Enemy spawners send waves of attackers toward the player base.
- Health and damage systems are implemented via ECS components.
- Ragdoll physics is triggered on death for added realism.

This full ECS-based combat loop (targeting, attack, damage, death) demonstrates robust and scalable combat architecture.
---

### **User Interface & Controls**
- Includes an RTS-style UI: minimap, health bars, resource counters, and unit queues.
- Unit selection supports both click and drag-box.
- **WASD keys control camera movement**, and mouse input allows for zoom and rotation.
- UI updates and interactions are all data-driven, reflecting ECS state in real-time.

---

### **Expandable Formation Mechanics for Unit Control**
This system allows units to move in various formations:

- An abstract `FormationBase` class defines a common interface.
- New formation types are easily added via inheritance (e.g., `CircleFormation`, `SquareFormation`, `FilledArrowFormation`, `OutlineArrowFormation`).
- A `FormationFactory` instantiates formation strategies based on an enum (`FormationType`).
- Enables dynamic switching of formations and supports future extensibility.

This hybrid object-oriented design inside a DOTS-based project demonstrates practical architecture decisions where modularity and clarity are critical.

---

### **Custom Event Systems in Unity ([Unity Event Kit](https://github.com/EmrhnFyz/Unity-Event-Kit))**
This project integrates the My own **Unity Event Kit**, a lightweight and type-safe alternative to Unity’s built-in C# events:

- **Strongly-Typed EventBus:** Type-safe, compiler-checked publish/subscribe system.
- **Zero GC Allocations:** Optimized for real-time games.
- **Thread-Safe Queuing:** Events can be safely queued from background threads.
- **ScriptableObject Channels:** Designer-friendly, inspector-exposed events.
- **Editor Debugger:** Powerful in-editor tooling to track events during runtime.

By using this kit, gameplay systems (like spawning, scoring, death, etc.) remain decoupled and modular, improving code clarity and reusability.

---

### **Event-Like System in DOTS (ECS Architecture)**
Since ECS lacks a built-in event system, a custom one was created:

- **`DOTSEventManager`** (MonoBehaviour) bridges Unity events with ECS by triggering tag components.
- **`ResetEventsSystem`** cleans up event components after one frame.
- ECS systems react to these one-frame components, effectively simulating transient events.

This design pattern enables communication between ECS systems (e.g., sound, scoring, animation triggers) without direct coupling, 
preserving ECS purity while enabling complex interactions.

---

## Summary

The DOTS-RTS project is a comprehensive demonstration of ECS-driven gameplay development in Unity. It combines:

- Scalable system architecture
- Custom solutions to overcome DOTS limitations
- Modular, extendable designs (formations, event systems)
- Efficient, real-time strategies for pathfinding, combat, and resource management

The result is a responsive, highly-performant RTS framework built on modern Unity architecture making it both a technical showcase and a foundation for further RTS development.
