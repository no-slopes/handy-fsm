# HandyFSM Runtime Profiling

This document defines the repeatable profiling workflow for HandyFSM runtime hotspots.

## Scope

The current benchmark runner focuses on the hot paths that were identified during the runtime audit and optimization pass:

- state discovery and initialization cost for generic runtime machines
- reflection and delegate binding during state initialization
- transition evaluation when states remain stable
- transition evaluation when transitions fire continuously across many machines

## How To Regenerate The Report

1. Open the Unity editor with the HandyFSM workspace loaded.
2. Let the editor finish compiling and settle after any domain reload.
3. Run `Tools/HandyFSM/Generate Runtime Benchmark Report`.
4. Review the generated report at `docs/handyfsm-runtime-benchmark-report.md`.

For headless execution, call Unity with `-executeMethod IndieGabo.HandyFSM.Editor.HandyFSMRuntimeBenchmarkRunner.GenerateRuntimeBenchmarkReport`.

## Interpretation

- The initialization scenarios represent batched setup work for many FSM users in the same scene.
- The steady-state transition scenario measures the no-transition hot path where conditions stay false.
- The forced-transition scenario measures the upper-pressure case where every evaluation advances to another state.
- Allocation data comes from `GC.GetAllocatedBytesForCurrentThread()` and should be interpreted as a comparable editor-thread signal, not as a full-engine memory capture.

## Current Optimization Coverage

- Transition evaluation now short-circuits on the first valid target instead of building candidate lists.
- Runtime state and scriptable-state lifecycle reflection is cached per type.
- Brain lifecycle hook reflection is cached per brain type.
- Generic runtime-state discovery and instantiation are cached in `StateProvider`.
- Trigger dispatch avoids duplicate dictionary lookups and unnecessary enumerator work.

## When To Re-Run

- after changes to `FSMBrain`, `State`, `ScriptableState`, `StateProvider`, or `TriggersProvider`
- after Unity version upgrades
- after changing optional integration behavior that affects initialization or tick-time branching
