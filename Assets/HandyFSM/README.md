# HandyFSM

This is the main entry point for HandyFSM documentation inside the Unity package.

The goal is not only to list APIs. The goal is to explain the full workflow for people who have never touched the package before, while also giving experienced users a solid reference they can come back to when they need to refresh a specific part of the system.

## Recommended Reading Order

1. [Getting Started](Docs/01-Getting-Started.md)
2. [Core Concepts](Docs/02-Core-Concepts.md)
3. [FSMBrain and Machine Flow](Docs/03-FSMBrain-and-Machine-Flow.md)
4. [Creating States](Docs/04-Creating-States.md)
5. [Transitions and Flow Control](Docs/05-Transitions-and-Flow-Control.md)
6. [Triggers](Docs/06-Triggers.md)
7. [Blackboard with Simple Blackboard](Docs/07-Blackboard-with-Simple-Blackboard.md)
8. [Character Controller Pro](Docs/08-Character-Controller-Pro.md)
9. [Debug, History, and Visualizer](Docs/09-Debug-History-and-Visualizer.md)
10. [Best Practices and FAQ](Docs/10-Best-Practices-and-FAQ.md)

## What HandyFSM Solves

HandyFSM is a package-first finite state machine solution for Unity focused on:

- straightforward authoring
- runtime performance
- a clean separation between class-based states and ScriptableObject-based states
- optional integrations with external packages
- editor-side debugging and visualization tools

## Main Package Surfaces

- `FSMBrain`: the central machine orchestrator
- `State`: the base class for runtime states implemented as regular C# classes
- `ScriptableState`: the base class for asset-authored states
- `StateProvider`: the loader, lookup, and initialization layer for states
- `StateTransition`: a transition rule containing a condition, a target state, and a priority
- `TriggersProvider`: a lightweight named-event channel
- optional integrations: Simple Blackboard and Character Controller Pro
- editor tooling: custom inspector, state visualizer, and history tracking

## Quick Choice: Which State Authoring Style Should You Use?

Use `ScriptableState` when:

- you want asset-based authoring in the inspector
- designers are going to configure the graph with assets
- you want to reference states through the `FSMBrain` scriptable state list
- you want to use `CreateAssetMenu`

Use `State` when:

- you want purely code-driven states
- you prefer automatic discovery by inheritance
- you want to reduce asset dependency for a simpler runtime-state workflow
- you want a strong base class for feature-specific runtime states

## Quick Choice: Which Brain Should You Use?

Use plain `FSMBrain` when:

- your main workflow is based on `ScriptableState`
- the default state comes from the inspector
- the state list also comes from the inspector

Use a brain derived from `GenericHandyFSMBrain<TBaseState>` or `GenericHandyFSMBrain<TBaseState, TDefaultState>` when:

- your main workflow is based on `State`
- you want to load states automatically by inheritance
- you want to constrain discovery to a dedicated runtime-state base type

## Optional Integrations

- Blackboard: explained in [07 - Blackboard with Simple Blackboard](Docs/07-Blackboard-with-Simple-Blackboard.md)
- Character Controller Pro: explained in [08 - Character Controller Pro](Docs/08-Character-Controller-Pro.md)

## Debug and Visualization

The package already includes tooling to inspect the current active path and to review the last captured play-mode history session. That part is documented in [09 - Debug, History, and Visualizer](Docs/09-Debug-History-and-Visualizer.md).

## Reading by Goal

If your goal is "I want to make it work today," read:

1. [01 - Getting Started](Docs/01-Getting-Started.md)
2. [04 - Creating States](Docs/04-Creating-States.md)
3. [05 - Transitions and Flow Control](Docs/05-Transitions-and-Flow-Control.md)

If your goal is "I want to integrate it with other systems," read:

1. [06 - Triggers](Docs/06-Triggers.md)
2. [07 - Blackboard with Simple Blackboard](Docs/07-Blackboard-with-Simple-Blackboard.md)
3. [08 - Character Controller Pro](Docs/08-Character-Controller-Pro.md)

If your goal is "I want better debugging and inspection," read:

1. [03 - FSMBrain and Machine Flow](Docs/03-FSMBrain-and-Machine-Flow.md)
2. [09 - Debug, History, and Visualizer](Docs/09-Debug-History-and-Visualizer.md)
3. [10 - Best Practices and FAQ](Docs/10-Best-Practices-and-FAQ.md)
