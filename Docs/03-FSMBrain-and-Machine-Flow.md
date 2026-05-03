# FSMBrain and Machine Flow

This document explains `FSMBrain` as a day-to-day component.

## What `FSMBrain` Does

In the normal flow it:

1. prepares the machine in `Awake`
2. initializes the loaded states
3. turns itself on in `Start` if the mode is automatic
4. runs transitions in the loop you enable
5. exposes the public API for state changes and queries

## Most Important Inspector Fields

### `Owner`

Optional reference to the logical owner transform of the machine.

If left empty, `FSMBrain` uses its own `transform`.

### `Initialization Mode`

- `Automatic`: turns on in `Start` using the default state
- `Manual`: waits for you to call `TurnOn(...)`

### `Transitions On Update`

Evaluates transitions in `Update`.

Good for:

- common input
- non-physics logic
- UI-driven or lightweight gameplay states

### `Transitions On Late Update`

Evaluates transitions in `LateUpdate`.

Use it when the decision depends on information that only becomes correct after other systems finish their `Update` work.

### `Transitions On Fixed Update`

Evaluates transitions in `FixedUpdate`.

Use it when the relevant logic is tied to the physics step.

### `Default Scriptable State`

The default state when the brain is used with `ScriptableState`.

If the mode is `Automatic` and no default state can be resolved, the machine logs an error and does not start.

### `Scriptable States`

The list of `ScriptableState` assets the brain should load.

Brutally important reminder: those assets are cloned at runtime.

### `Save History`

Enables editor-side history capture for the visualizer.

That capture flow is explained in detail in [09 - Debug, History, and Visualizer](09-Debug-History-and-Visualizer.md).

### `Third Party`

Section containing optional integrations.

At the moment it includes:

- Simple Blackboard
- Character Controller Pro

## Brain Lifecycle

### `Awake`

In the current runtime flow, the brain:

- resets status and internal caches
- creates `StateProvider`
- loads `ScriptableState` instances from the configured list
- creates `TriggersProvider`
- executes optional hooks such as `BeforeInitialized()` and `AfterInitialized()` on derived brains
- initializes all loaded states

### `Start`

If the mode is `Automatic`, it tries to start on the default state.

### `Update`, `LateUpdate`, `FixedUpdate`

Each enabled loop does this:

1. evaluate transitions when appropriate
2. execute the current state's tick method

When CCPro is enabled, the flow changes. The brain concentrates the relevant work in `FixedUpdate` and in Character Controller Pro simulation callbacks.

## Most Common Public API

### Turn the machine on

```csharp
brain.TurnOn(brain.DefaultState);
```

or

```csharp
brain.TurnOn(typeof(MyScriptableState));
```

### Pause and resume

```csharp
brain.Pause();
brain.Resume();
```

### Stop

```csharp
brain.Stop();
```

### Request an external state change

```csharp
brain.RequestStateChange(targetState);
```

or

```csharp
brain.RequestStateChange<MyState>();
```

### Complete the current state naturally

```csharp
brain.CompleteState();
```

or with an explicit target:

```csharp
brain.CompleteState(targetState);
```

### Fail the current state

```csharp
brain.FailState(null, "The current state cannot continue.");
```

### Fetch loaded states

```csharp
IState stateByType = brain.GetState(typeof(MyState));
IState stateByKey = brain.GetState("combat.attack");
MyState typed = brain.GetState<MyState>();
```

### Load states manually

```csharp
brain.LoadStatesOfType(typeof(MyBaseState));
brain.LoadStatesFromScriptablesList(myStates);
brain.LoadState(typeof(MyRuntimeState));
brain.LoadState(myStateInstance);
```

## Brain Events

The brain exposes two important `UnityEvent`s:

- `StatusChanged`
- `StateChanged`

The editor visualizer uses them as well.

## Transition Reasons

The brain records a `StateTransitionReport` containing a reason and an optional message.

The most practical reasons are:

- `InitialEntry`
- `ExternalRequest`
- `ConditionTransition`
- `NaturalTransition`
- `ErrorTransition`
- `Unknown`

Those reasons feed the debugging history.

## Error Fallback

When a state fails with `StateFailureException`, the brain tries to recover without crashing the application.

The flow is:

1. mark the state as problematic for the current session
2. try to go to the default state if it exists and is not faulted
3. otherwise try the first state that entered successfully
4. if no safe fallback exists, shut the machine down

That means state failure is treated as a controlled runtime event, not as an inevitable crash.

## When to Derive from `FSMBrain`

You derive from `FSMBrain` when you need to:

- load class-based states automatically through `GenericHandyFSMBrain`
- add custom bootstrap logic before initialization
- encapsulate integration with other game components

Example of a simple class-based brain:

```csharp
using IndieGabo.HandyFSM.Implementations;

namespace IndieGabo.HandyFSM.Examples
{
    public abstract class PlayerState : State
    {
    }

    public sealed class PlayerBrain : GenericHandyFSMBrain<PlayerState, PlayerIdleState>
    {
    }
}
```

## Recommended Modeling Rule

Use a dedicated base type when working with runtime states.

Good:

```csharp
public abstract class PlayerState : State
{
}

public sealed class PlayerBrain : GenericHandyFSMBrain<PlayerState, PlayerIdleState>
{
}
```

Bad:

```csharp
public sealed class PlayerBrain : GenericHandyFSMBrain<State>
{
}
```

The second case can pull types into the machine that you never intended to load, as long as they are in the same assembly and inherit from `State`.
