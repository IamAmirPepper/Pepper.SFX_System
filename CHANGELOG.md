# Changelog

## [2.5.0] - 2026-05-18

### Added
- **Occlusion Mixer Slot Pool** — voice-side occlusion now runs on pre-built mixer-slot pools per bus (no more per-source AudioLowPassFilter clicks). Authored one-click via `Window > Audio System > Occlusion Layout`.
- **Reverb Send Buses** — per-room reverb routing via new `ReverbSendBus` assets + project-wide `ReverbSendBusRegistry`. Authored via the new `Window > Audio System > Reverb Send Buses` window and the per-asset custom inspector (Generate / Validate / Apply / Pull buttons).
- **Per-Zone Reverb** — `AudioZone` now carries `ReverbProfile` (six listener-driven SFX Reverb params) and `ReverbBus` routing. Source-side: voices in a zone send to that zone's bus. Listener-side: bus character is driven from the listener's current zone profile per tick.
- **Multi-ray partial occlusion** — fanned raycasts produce a smooth gradient across wall edges instead of a binary step. Tunable via `occlusionRayCount` (default 3) and `occlusionRaySpreadMeters` (default 0.5 m).
- **Per-frame occlusion smoothing** — `occlusionGainSmoothingSeconds` + `occlusionCutoffSmoothingSeconds` tau fields. LOS transitions are perceptually continuous; settling time scales with the tau, not the raycast tick.
- **`AudioZone.entryFadeMeters`** — soft membership band across zone boundaries. Fades reverb sends, listener-side reverb character, and `BaseVolumeDb` across the surface. 0 (default) preserves the previous hard boundary.
- **`PropagationManager.silenceOutsideGraph`** — whitelist mode (default, outside the graph plays normally) vs complete-coverage mode (outside falls silent + muffled).
- **Reachability-preserving proximity culler** — zones connected to the listener's zone through near portals are kept active even when individually out of activation-radius range. Two-zone scenes never trip culler-induced silence.
- **AudioContainer routing flags** — `UseOcclusion`, `UsePropagation`, `AllowReverbSend`, `StaticEmitter`, `ExplicitReverbBus`, `ReverbSendLevelDb`. Any of the first three causes a slot to be acquired at play time.
- **`GainStack.MultiplierGain`** — fixes silently-broken volume features. Volume randomization, `SwitchContainer.Volume`, `BlendContainer.Volume`, and `AudioExtensions.PlayWithVolume` now actually take effect on enhanced voices.
- **`AudioEvent.Post(GameObject, Vector3, double)`** — scheduled-DSP overload for tempo-driven callers; threads through to a new `AudioVoiceEnhanced.Start(double scheduledDspTime)` overload.
- **`AudioManager.WriteReverbSendsImmediate`** — synchronous reverb-send write at play time so the first audio frame already carries correct routing (no dry blip on transients).
- **New runtime types**: `OcclusionLayout`, `OcclusionSlot`, `ReverbSendBus`, `ReverbSendBusRegistry`, `AudioReverbProfile`.
- **Custom inspectors** with live diagnostics on `AudioZone`, `AudioPortal`, and `PropagationManager` (play-mode panels for registered counts, zone stack, blend-portal membership, emitter pool, etc.).

### Changed
- **BeatScheduler hitch policy rewritten.** Audio fires exactly once per Update anchored to the most recent grid time; `OnBeat` callbacks still catch up per missed beat so game-logic counters stay synced. No more amplitude bumps on frame stalls.
- **BeatScheduler sample-accurate spacing.** Scheduled DSP time threads `BeatScheduler → AudioEvent.Post → AudioVoiceEnhanced.Start` so the audible onset lands on the chosen grid, not on the firing frame's `dspTime + buffer`. No more tempo wobble across BPM ramps.
- **BeatScheduler BPM-change detection.** Typing a BPM in the inspector or writing the `Bpm` property now correctly re-anchors the next beat. Previously, partial keystrokes pushed the next beat 60s into the future.
- **BeatScheduler start lead-in.** One DSP buffer of lead on `StartScheduling` when `fireImmediatelyOnStart=true`. Beat 1 is no longer overdue at fire time; the first 1–2 beats no longer feel rushed.
- **Grid phase preserved across stalls.** The catch-up snap that reset the grid to "now" is gone; phase is anchored to the original schedule.
- **`AudioBus.AssignToVoice`** — sticky mixer-group write avoids click-producing rebuild when a voice's bus is already correct.
- **`AudioManager.ReturnVoice`** — `OcclusionRaycastGain` now resets alongside the cutoff reset.
- **`AudioPortal.openLowPassHz` default lowered 18000 → 12000.** Door-less portals are now audibly distinct from a fully transparent path.
- **`PropagationManager.solveRateHz` default raised 8 → 15** (~67 ms cache step). Fast zone crossings and partially-closed-portal transitions no longer surface audible step changes.
- **Listener-zone reverb integration** — each reverb bus's six runtime params are driven per-tick from the listener's current zone profile.
- **Documentation consolidated** — six legacy guides folded into two: `USER_GUIDE.md` (Introduction → Quick Start → Cookbook → Manual → Migration, all in one file) and `API_REFERENCE.md` (mechanical reference).
- **Production-grade tooltip pass** across the public-surface inspector fields, custom inspectors, and editor windows. No more dev-text leaks ("PLAN §10.X", "Phase 3 step Y") in designer-visible strings.
- **Convolution schema on `ReverbSendBus`** marked `[HideInInspector]` (reserved for a future release; existing SO assets won't need migration when convolution ships).
- **"Tools > Audio System > Phase 3 — …" diagnostic menu items** renamed to drop the phase prefix.

### Removed
- **`AudioVoiceEnhanced.LowPassFilter` property** — replaced by the new `OcclusionSlot` mixer-side architecture.
- **Global "Enable Occlusion" toggle on AudioManager** — occlusion is per-container now via the `Use Occlusion` flag on AudioContainer assets.
- **5 legacy documentation files**: `1_QUICK_START.md`, `2_COOKBOOK.md`, `3_MANUAL.md`, `5_ADVANCED_MIGRATION_GUIDE.md`, `6_CHANGELOG_WORKFLOW_IMPROVEMENTS.md` — content folded into `USER_GUIDE.md`.

### Compatibility note
All API changes are additive — existing code continues to work without modification. Two behavior changes are worth a re-listen on existing scenes:
- `SwitchContainer.Volume` and `BlendContainer.Volume` previously did nothing on enhanced voices; they now take effect. Projects that authored non-default values expecting them to be ignored will hear them now.
- `AudioPortal.openLowPassHz` and `PropagationManager.solveRateHz` defaults changed (see above). Existing assets with explicit values keep them; only newly-created assets pick up the new defaults.

---

## [2.3.3] - 2026-05-07

Beat Scheduler - fixed issue with inspector field type
Audio Voice - fixed issue with voices playing "pop" sound by adding a 5ms fade in as a default - it's actually a configurable per container voice attack ramp".
Switch container - "Default Container" now allows selection only from the allotted slots
Enhanced Sound Setup - now allows to create a switch container without an allotted audioclip - when containers are assigned

***working feature*** - added an audio mixer 

For questions or suggestions, see existing documentation
"https://github.com/IamAmirPepper/Pepper.SFX_System/tree/main/Documentation~"
###