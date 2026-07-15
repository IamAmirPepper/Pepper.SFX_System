# Changelog

## [3.0.0] - 2026-07

The free/full split release. This package is now the **free version** — the complete
single-player audio middleware. A **full version** (multiplayer audio, Addressables-based
zone streaming, pro mixer/profiler tooling, full source code, and a one-click upgrade
migration tool) is coming to the Unity Asset Store. Everything you author in the free
version carries over.

### Added
- **Audio Explorer** (`Window ▸ Audio System ▸ Audio Explorer`) — browse every event,
  container and bus in one window, with inline inspector and play-to-preview.
- **Import Manager** (`Window ▸ Audio System ▸ Import Manager`) — category-driven import
  pipeline; correct Load Type / compression applied automatically per category
  (SFX / Music / UI / Dialogue / Ambience), with project-window icon overlays.
- **Voice Pool Debug** (`Window ▸ Audio System ▸ Voice Pool Debug`) — live table of every
  playing voice (clip, event, bus, volume, distance) with per-voice stop, pool health
  bars, and a "voices lost to destruction" diagnostic.
- **Audio Event IDs** (`Window ▸ Audio System ▸ Audio Event IDs`) — gives every event a
  permanent, rename-proof stable ID and flags collisions; IDs are captured automatically
  on import from here on.
- **Container Validator** (`Window ▸ Audio System ▸ Container Validator`) — scans all
  containers for common authoring mistakes with one-click fixes.
- **Clip warming** — `AudioManager.WarmClip / WarmClips / WarmContainer` pre-load a clip's
  audio data ahead of first play (kills the first-play hitch on streamed music and
  compressed dialogue). `AmbientSource` warms its loop clip automatically on register.
- **Voice pool health surface** — `AudioManager.GetStatistics()` now reports live voice
  objects vs budget and a running voices-lost-to-destruction count; the AudioManager
  inspector shows a live health card during Play, with an opt-out warning
  (`warnOnActiveVoiceDestroyed`) naming the emitter-destroyed-mid-sound cause.

### Changed
- **AudioEvent inspector is registry-aware.** With an `AudioEventRegistry` in the project
  it validates registry membership (with an *Add to Registry* button) instead of pushing
  assets into `Resources/`. Legacy Resources-folder guidance now applies only to projects
  without a registry.
- **Cooldown rejections no longer log a warning** — being rate-limited is the feature
  working; at footstep rates the old warning flooded the console. The call still returns
  an empty handle.
- Voice stealing (`maxInstances` overflow) is now allocation-free — noticeably cheaper
  under rapid-fire instance-capped events.
- Runtime state (cooldown clocks, random anti-repeat history, sequence positions) resets
  cleanly per play session when Enter Play Mode Options skip domain reload.
- Reverb diagnostics moved under `Tools ▸ Audio System ▸ Diagnostics ▸ …`.

### Fixed
- A voice whose emitter GameObject was destroyed mid-play no longer silently shrinks the
  pool unnoticed — it is swept, counted, and (optionally) warned about.
- Handle volume semantics documented: `AudioHandle.SetVolume` sets an **absolute** level
  (replaces the authored base volume) — see the API reference.

### Notes
- No public API was removed or changed — existing `PostEvent` code compiles and behaves
  as before.
- After updating: refresh Package Manager (or delete `Library/PackageCache/com.pepper.sfx@*`)
  to pull the new commit.

## [2.5.3] - 2026-06-05

### Fixed
- **Player builds no longer fail with ILLink error IL1010** ("Assembly 'Assembly-CSharp'
  cannot be loaded ... Failed to resolve assembly: 'UnityEditor.CoreModule'"). The Runtime
  assembly (`SFXSystem.Runtime`) was carrying a hard reference to `UnityEditor.CoreModule`
  because editor-only API was named directly inside `#if UNITY_EDITOR` blocks. Since the
  shipped Runtime DLL is precompiled in an editor context, those blocks — and the
  UnityEditor dependency — were baked into the DLL and survived into player builds.
  All `UnityEditor.*` references have been removed from the Runtime assembly.

### Changed
- Editor-only behaviors invoked from Runtime code (scene-view gizmo labels in
  `PropagationManager`, `AudioMultiPositionEmitterParent`, `AudioMultiPositionEmitterChild`;
  and `AudioEventRegistry`'s asset-locate / auto-populate / validate utilities) now route
  through a delegate bridge instead of calling `UnityEditor` directly. The Editor assembly
  installs the implementations on load via `[InitializeOnLoad]`; in player builds the slots
  stay null and the call sites are null-guarded no-ops. No behavioral change in the editor.

### Added
- `AudioSystem.AudioEditorHooks` — Runtime delegate bridge for editor-only behaviors.
- `AudioEventRegistry.AutoLoadFromResources`, `ClearEntries()`, `RemoveNullEntries()` —
  editor-symbol-free helpers backing the registry's authoring utilities.

### Notes
- The Editor assembly (`SFXSystem.Editor`) must be present in the editor for gizmo labels
  and the registry inspector buttons to function — unchanged from prior versions, but now
  a hard requirement for that editor-side functionality.
- After updating: bump `package.json` to 2.5.3, push, then in consuming projects delete
  `Library/PackageCache/com.pepper.sfx@<hash>/` or refresh Package Manager to pull the new commit.
For questions or suggestions, see existing documentation
"https://github.com/IamAmirPepper/Pepper.SFX_System/tree/main/Documentation~"
###