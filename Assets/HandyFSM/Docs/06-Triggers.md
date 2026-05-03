# Triggers

Triggers in HandyFSM are a lightweight string-keyed communication mechanism.

They exist to decouple whoever fires an event from whoever reacts to it.

## What Exists in Practice

`FSMBrain` exposes a `TriggersProvider` through `Brain.Triggers`.

That provider allows you to:

- register callbacks without payloads
- register callbacks with payloads derived from `TriggerData`
- fire a trigger by key using `Squeeze(...)`
- remove registered callbacks

## Essential API

### Register a trigger without payload

```csharp
Brain.Triggers.RegisterCallback("combat.start", OnCombatStart);
```

### Register a trigger with payload

```csharp
Brain.Triggers.RegisterCallback("damage.received", OnDamageReceived);
```

### Fire a trigger without payload

```csharp
Brain.Triggers.Squeeze("combat.start");
```

### Fire a trigger with payload

```csharp
Brain.Triggers.Squeeze("damage.received", new IntTriggerData(10));
```

### Remove a registration

```csharp
Brain.Triggers.UnregisterCallback("combat.start", OnCombatStart);
Brain.Triggers.UnregisterCallback("damage.received", OnDamageReceived);
```

## Ready-Made Payload Types

The package already includes a few concrete `TriggerData` types:

- `FloatTriggerData`
- `IntTriggerData`
- `StringTriggerData`
- `BoolTriggerData`
- `ObjectTriggerData`
- `StateTriggerData`

## Full Example of a State Listening to a Trigger

```csharp
using UnityEngine;
using UnityEngine.Events;

namespace IndieGabo.HandyFSM.Examples
{
    [CreateAssetMenu(
        fileName = "WaitingForStartState",
        menuName = "HandyFSM/Examples/Waiting For Start State")]
    public sealed class WaitingForStartState : ScriptableState
    {
        private PlayingState _playingState;

        private void OnInit()
        {
            _playingState = Brain.GetState<PlayingState>();
        }

        private void OnEnter()
        {
            Brain.Triggers.RegisterCallback("game.start", OnGameStart);
        }

        private void OnExit()
        {
            Brain.Triggers.UnregisterCallback("game.start", OnGameStart);
        }

        private void OnGameStart()
        {
            CompleteState(_playingState);
        }
    }
}
```

And an external component firing that trigger:

```csharp
using UnityEngine;

namespace IndieGabo.HandyFSM.Examples
{
    public sealed class StartGameButtonDriver : MonoBehaviour
    {
        [SerializeField]
        private FSMBrain _brain;

        public void StartGame()
        {
            if (_brain == null)
            {
                return;
            }

            _brain.Triggers.Squeeze("game.start");
        }
    }
}
```

## Example with Payload

```csharp
using UnityEngine;

namespace IndieGabo.HandyFSM.Examples
{
    public sealed class DamageReceiverState : State
    {
        private int _lastDamage;

        private void OnEnter()
        {
            Brain.Triggers.RegisterCallback("damage.received", OnDamageReceived);
        }

        private void OnExit()
        {
            Brain.Triggers.UnregisterCallback("damage.received", OnDamageReceived);
        }

        private void OnDamageReceived(TriggerData data)
        {
            if (data is not IntTriggerData damage)
            {
                return;
            }

            _lastDamage = damage.Value;
            Debug.Log($"Damage received: {_lastDamage}");
        }
    }
}
```

## When to Register in `OnEnter` and Remove in `OnExit`

This is the ideal pattern when the state should only listen to the trigger while it is active.

Use this pattern for:

- contextual input
- temporary prompts
- reactions that only make sense in the current state

## When to Register in `OnInit`

Register in `OnInit` only if the callback is valid for the full lifetime of the state instance.

Even then, do it with discipline. If the callback can be fired while the state is inactive and that would be a problem, the design is wrong.

## Important Warnings

### Avoid registering the same callback repeatedly without removing it

If you register in `OnEnter` and forget to remove it in `OnExit`, the same callback will accumulate registrations and may fire multiple times.

### Standardize your keys

Good keys:

- `game.start`
- `combat.attack`
- `ui.pause.open`

Bad keys:

- `A`
- `myTrigger`
- `thing`

### Do not use triggers for everything

Triggers are good for events. Do not use them as a substitute for shared state, data caching, or complex synchronization.

For shared data, see [07 - Blackboard with Simple Blackboard](07-Blackboard-with-Simple-Blackboard.md).
