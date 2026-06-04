# Changelog

## [2.5.2.2] - 2026-06-05

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