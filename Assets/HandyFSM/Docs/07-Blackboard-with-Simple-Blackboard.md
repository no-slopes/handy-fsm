# Blackboard with Simple Blackboard

HandyFSM supports optional integration with the `com.zor.simple-blackboard` package.

That integration is not mandatory. If the package is not installed, the core package continues to work and the corresponding inspector section disappears.

## What the Blackboard Is For

The blackboard is a shared repository of runtime data.

Use it for:

- values that multiple states need to read
- orchestration flags
- shared references
- data that does not belong exclusively to a single state

Avoid using the blackboard for:

- needlessly mirroring a state's private local variable
- storing everything because modeling feels inconvenient
- replacing clearer APIs when the dependency is direct and local

## How to Enable It in the Inspector

When the `Simple Blackboard` package is present:

1. select the `FSMBrain`
2. open the `Third Party` section
3. enable `Use Simple Blackboard?`
4. drag the Simple Blackboard container component into the corresponding field

If the section does not appear, the package was not resolved by the project or the assembly has not recompiled yet.

## What the Brain Exposes

On `FSMBrain`, you have:

- `UseSimpleBlackboard`
- `BlackboardContainer`
- `HasBlackboard`
- `Blackboard`
- `TryGetBlackboardValue<T>()`
- `SetBlackboardValue<T>()`
- `TryGetBlackboardObject()`
- `HasBlackboardValue()`

## What States Get for Free

In both `State` and `ScriptableState`, the base types already expose protected wrappers:

- `HasBlackboard`
- `BlackboardContainer`
- `Blackboard`
- `TryGetBlackboardValue<T>()`
- `SetBlackboardValue<T>()`
- `TryGetBlackboardObject()`
- `HasBlackboardValue()`

In other words: a state does not need to hunt for the container on its own.

## Practical Example

Imagine a combat flow in which one state writes the last valid target and another one consumes it.

```csharp
using UnityEngine;

namespace IndieGabo.HandyFSM.Examples
{
    [CreateAssetMenu(
        fileName = "AcquireTargetState",
        menuName = "HandyFSM/Examples/Acquire Target State")]
    public sealed class AcquireTargetState : ScriptableState
    {
        [SerializeField]
        private string _targetKey = "combat.target";

        private void OnTick()
        {
            GameObject target = GameObject.FindWithTag("Enemy");

            if (target == null)
            {
                return;
            }

            SetBlackboardValue(_targetKey, target);
            CompleteState();
        }
    }
}
```

Consuming that value in another state:

```csharp
using UnityEngine;

namespace IndieGabo.HandyFSM.Examples
{
    [CreateAssetMenu(
        fileName = "ChaseTargetState",
        menuName = "HandyFSM/Examples/Chase Target State")]
    public sealed class ChaseTargetState : ScriptableState
    {
        [SerializeField]
        private string _targetKey = "combat.target";

        private void OnTick()
        {
            if (!TryGetBlackboardValue(_targetKey, out GameObject target) || target == null)
            {
                FailState(null, "Blackboard target was missing.");
                return;
            }

            Vector3 direction = (target.transform.position - Brain.Owner.position).normalized;
            Brain.Owner.position += direction * Time.deltaTime;
        }
    }
}
```

## Typed and Untyped Reads

### Typed read

```csharp
if (TryGetBlackboardValue("movement.speed", out float speed))
{
}
```

### Typed write

```csharp
SetBlackboardValue("movement.speed", 4.5f);
```

### Existence check

```csharp
if (HasBlackboardValue("movement.speed"))
{
}
```

### Read as object

```csharp
if (TryGetBlackboardObject("combat.target", out object value))
{
}
```

## Recommended Usage Pattern

Good blackboard practices in HandyFSM:

- use stable, unambiguous keys
- make it clear which system writes each key
- treat missing values as a normal case when that makes sense
- do not duplicate the same data across many local fields without a real need
- prefer the blackboard as the canonical source for shared orchestration values

## Common Errors

### "My state cannot read anything"

Checklist:

- is the `Simple Blackboard` package installed?
- is `Use Simple Blackboard?` enabled on `FSMBrain`?
- was the container assigned?
- is the key used for reading exactly the same as the key used for writing?
- does the requested type match the stored type?

### "The blackboard section does not appear"

That usually means the optional package was not resolved in the current project.

### "I have duplicated data in the state and in the blackboard"

Decide which one is the canonical source. If the same information guides multiple states, the blackboard is usually the more honest place for it.
