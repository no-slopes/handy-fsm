# TODO

This file is the mutable task list for the HandyFSM workstream.
Update it whenever priorities change.

- [x] Repair the editor visual tooling in Assets/HandyFSM/Editor by restoring the missing UI Toolkit documents and fixing the broken graph-building flow.
  - [x] Current State: restore the missing visualizer UI Toolkit documents and make the window open reliably from the FSM Brain inspector and the Unity menu.
  - [x] Current State: render the loaded-state inheritance tree from the FSM owner down to every loaded state, highlighting the active path and the current state.
  - [x] Current State: support both edit mode preview and play mode inspection by discovering configured scriptable states and generic runtime states when the machine is not initialized yet.
  - [x] History: add a dedicated history view that records transitions for the latest captured play session and keeps the last session available after leaving play mode.
  - [x] History: extend the runtime transition pipeline with explicit transition-reason metadata so the history can explain whether a state exited voluntarily, was interrupted, or was requested externally.
- [x] Define and harden the OpenUPM modularization strategy for the optional CCPro integration so HandyFSM core remains independent of com.lightbug.character-controller-pro.
- [x] Profile and optimize runtime hotspots in HandyFSM, with emphasis on state initialization, reflection and delegate setup, transition evaluation, and avoidable allocations.
  - [x] Runtime hot paths were reduced in transition evaluation, lifecycle hook caching, state discovery, and trigger dispatch.
  - [x] Added a repeatable editor benchmark runner at `Tools/HandyFSM/Generate Runtime Benchmark Report` that writes `docs/handyfsm-runtime-benchmark-report.md`.
