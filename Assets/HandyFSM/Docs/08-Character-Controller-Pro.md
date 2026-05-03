# Character Controller Pro

HandyFSM supports Character Controller Pro as an optional capability directly on `FSMBrain`.

There is no separate CCPro brain anymore. The integration now lives in the base `FSMBrain` and is activated by configuration when the CCPro package is installed.

## What You Need

To use the integration:

1. the Character Controller Pro package must be installed in the project
2. `FSMBrain` must have the integration enabled in the inspector
3. the required references must be assigned

## How to Enable It in the Inspector

With the package installed:

1. select the `FSMBrain`
2. open `Third Party`
3. enable `Use Character Controller Pro?`
4. assign:
   - `Animator`
   - `Character Actor`
   - `Material Controller`
   - `Character Brain`
   - `Movement Reference`
   - `External Reference` when the selected mode requires it

## What Each Field Means

### `Animator`

Animator used by CCPro-aware states when they need to update parameters or IK.

### `Character Actor`

The main reference for locomotion, physical data, simulation callbacks, and actor properties.

### `Material Controller`

Optional reference for surface and volume multipliers.

### `Character Brain`

Reference used to access CCPro `CharacterActions`.

### `Movement Reference`

The mode used to calculate the movement axis perceived by the state.

Current options:

- `World`
- `External`
- `Character`

### `External Reference`

Transform used when the selected movement reference mode is external.

## What `FSMBrain` Exposes When CCPro Is Active

On the brain, you gain access to:

- `UseCharacterControllerPro`
- `Animator`
- `CharacterActor`
- `MaterialController`
- `CharacterBrain`
- `InputMovementReference`
- `MovementReferenceForward`
- `MovementReferenceRight`
- `ExternalReference`
- `MovementReferenceMode`
- `UseRootMotion`
- `UpdateRootPosition`
- `UpdateRootRotation`
- `ResetIKWeights()`

## How the Loop Changes When CCPro Is Enabled

When the integration is active:

- the brain concentrates the relevant flow inside `FixedUpdate`
- it also listens to `CharacterActor` callbacks for pre-simulation, post-simulation, and IK
- states may implement extra hooks in addition to the normal FSM lifecycle hooks

## CCPro-Specific Base Types

Use:

- `CCProState` for class-based states
- `ScriptableCCProState` for asset-based states

These bases already expose typed shortcuts:

- `Animator`
- `CharacterActor`
- `CharacterBrain`
- `MaterialController`
- `CharacterActions`
- `InputMovementReference`
- `MovementReferenceForward`
- `MovementReferenceRight`

## Extra Hooks Recognized by the CCPro Bases

In addition to `OnInit`, `OnEnter`, `OnExit`, `OnTick`, `OnFixedTick`, and `OnLateTick`, the CCPro bases recognize:

- `OnPreCharacterSimulation(float dt)`
- `OnPostCharacterSimulation(float dt)`
- `OnPreFixedTick()`
- `OnPostFixedTick()`
- `OnTickIK(int layerIndex)`

Those hooks are excellent when you want to integrate the FSM with the CCPro simulation cycle without resorting to hacks.

## Example of `ScriptableCCProState`

```csharp
using UnityEngine;
using IndieGabo.HandyFSM.CCPro;

namespace IndieGabo.HandyFSM.Examples
{
    [CreateAssetMenu(
        fileName = "CCProLocomotionState",
        menuName = "HandyFSM/Examples/CCPro Locomotion State")]
    public sealed class CCProLocomotionState : ScriptableCCProState
    {
        private void OnEnter()
        {
            UseRootMotion(false);
        }

        private void OnFixedTick()
        {
            CharacterActor.PlanarVelocity = InputMovementReference * 4f;
        }

        private void OnPreCharacterSimulation(float dt)
        {
            if (Animator == null)
            {
                return;
            }

            Animator.SetFloat("MoveX", CharacterActions.movement.value.x);
            Animator.SetFloat("MoveY", CharacterActions.movement.value.y);
        }

        private void UseRootMotion(bool value)
        {
            Brain.UseRootMotion = value;
        }
    }
}
```

## Example with `CCProState`

```csharp
using IndieGabo.HandyFSM.CCPro;

namespace IndieGabo.HandyFSM.Examples
{
    public abstract class PlayerCCProState : CCProState
    {
    }

    public sealed class PlayerCCProIdleState : PlayerCCProState
    {
        private void OnFixedTick()
        {
            CharacterActor.PlanarVelocity = InputMovementReference;
        }
    }
}
```

## Validation Performed by the CCPro Bases

`CCProState` and `ScriptableCCProState` automatically validate whether:

- a valid `FSMBrain` exists
- the CCPro usage flag is enabled
- `CharacterActor` was assigned
- `CharacterBrain` was assigned

If something is wrong, they raise `StateFailureException` and the brain enters its recovery path.

## About Movement Reference

`InputMovementReference` represents player input already projected into the selected reference space.

The most important helper vectors are:

- `MovementReferenceForward`
- `MovementReferenceRight`

Use those when the state needs to decide movement relative to the camera, the world, or the character itself.

## Practical Usage Advice

If the state depends heavily on CCPro features, use the CCPro-specific base classes from the beginning. Do not scatter manual casts and component assumptions across every class. That only creates organizational ruin.

## Common Errors

### "The CCPro section does not appear"

The CCPro package was probably not resolved in the current project.

### "The section appears, but the state fails during initialization"

Checklist:

- is `Use Character Controller Pro?` enabled?
- was `Character Actor` assigned?
- was `Character Brain` assigned?
- did you actually derive from `CCProState` or `ScriptableCCProState`?

### "My transitions stopped happening where I expected them to"

When CCPro is active, the relevant flow is tied to `FixedUpdate` and simulation callbacks. Organize the machine like a physics-driven FSM, not like a purely `Update`-driven FSM.
