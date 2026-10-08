# Spell System (Unity / C#)

Code I wrote during my internship at Say It Labs for a mechanic that was cut from the project (a 2D action roguelike mobile game).

> This is a code excerpt, not a runnable project. It depends on other classes from the original game (`Player`, `Player_Health`, `AudioPlayer`, `Pickup`, etc.) that are not included.

## What it does
The player can hold one spell at a time. Spells are picked up from the world, cast from a UI button, have limited uses and a cooldown, and are dropped back into the world (keeping their remaining uses) when the player picks up a different one.

## Files
| File | Purpose |
|---|---|
| `Spell.cs` | ScriptableObject holding a spell's data: cooldown, number of uses, sprite and the spell prefab. |
| `ISpell.cs` | Interface every spell implements. `CanSpellLogicRun()` lets a spell decide whether it can be cast in the current game context. |
| `Regeneration_Spell.cs` | Example spell: heals the player over time with a coroutine and plays a particle effect. It can't be cast at full health. |
| `SpellManager.cs` | Central manager: holds the current spell, checks casting conditions, handles cooldown and uses, drops spells, and notifies the UI through events. |
| `SpellPickup.cs` | World pickup that gives a random spell, or a specific dropped spell with its remaining uses. |
| `SpellButtonUI.cs` | UI button: cooldown slider, uses counter, sounds, and add/remove animations. It only reacts to events from the manager. |

## Patterns and concepts used
- **Data-driven design with ScriptableObjects**: spells are assets, so designers can add or tweak spells without touching code (loaded with `Resources.LoadAll`).
- **Observer pattern / event-driven architecture**: `SpellManager` raises C# events (`OnSpellCast`, `OnAddSpell`, `OnRemoveSpell`) and the UI subscribes in `OnEnable`/`OnDisable`. The game logic and the UI don't know about each other, which is **separation of concerns**.
- **Interface-based design and polymorphism**: the manager works with any spell through `ISpell`, so new spells need no changes to the manager.
- **Singleton**: global access to the `SpellManager`.
- **Prefab-based composition**: a spell's effect is a prefab with its own logic, instantiated when cast.
- **Addressables**: the pickup prefab is loaded asynchronously with a completion callback.
- **Coroutines**: heal-over-time, cooldown timers and UI sequences.
- **Inheritance**: `SpellPickup` extends a base `Pickup` class and overrides its lifecycle methods.
- **State handling**: the UI queues a new spell if one arrives while the remove animation is still playing.
- **Time-scale-aware cooldown**: the UI cooldown uses the game's own time scale instead of raw time, so it respects pausing and slow-motion.
