# SFX System - API Reference

**Version:** 3.1.0
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** October 2026

**Complete API documentation for the SFX System**

---

## Table of Contents

1. [AudioManager](#audiomanager)
2. [AudioEvent](#audioevent)
3. [AudioHandle & AudioMultiHandle](#audiohandle--audiomultihandle)
    - [Parameters: AudioParameter, AudioAttenuation, AudioEmitter & AudioSwitchGroup](#parameters)
4. [AudioContainer (Base)](#audiocontainer-base)
5. [Container Types](#container-types)
    - [GeneratorContainer (Unity 6000.3 and later)](#generatorcontainer-unity-60003-and-later)
6. [AudioBus](#audiobus)
7. [AudioState](#audiostate)
8. [AudioVoice & AudioVoiceEnhanced](#audiovoice--audiovoiceenhanced)
9. [AudioExtensions & Utilities](#audioextensions--utilities)
10. [AudioEventRegistry](#audioeventregistry)
11. [Multi-Position Emitters](#multi-position-emitters)
12. [Editor Tools](#editor-tools)
    - [AudioAuditionWindow and AudioAudition](#audioauditionwindow-and-audioaudition)
    - [Live View, AudioEmitterRecorder and AudioParameterRecording](#audioliveviewwindow-audioemitterrecorder-and-audioparameterrecording)
    - [LoopAnalysisWindow](#loopanalysiswindow)
    - [EngineBuilderWindow](#enginebuilderwindow)
    - [EngineGrainsWindow](#enginegrainswindow)
    - [Container Validator](#audiocontainervalidator-container-validator)
    - [Audio Import Settings and folder rules](#audio-import-settings-and-folder-rules)
    - [SoundBuilderSimpleWindow](#soundbuildersimplewindow)
    - [AudioEventEditor](#audioeventeditor)
    - [AudioEventRegistryEditor](#audioeventregistryeditor)
13. [Propagation](#propagation)
    - [PropagationManager](#propagationmanager)
    - [AudioZone](#audiozone)
    - [AudioPortal](#audioportal)
    - [AmbientSource](#ambientsource)
    - [AmbientEmitter](#ambientemitter)
    - [AudioListenerZoneTracker](#audiolistenerzonetracker)
    - [IPortalDoorSource](#iportaldoorsource)
    - [PropagationProximityCuller](#propagationproximityculler)
14. [Occlusion Mixer Slots](#occlusion-mixer-slots)
    - [OcclusionLayout](#occlusionlayout)
    - [OcclusionSlot](#occlusionslot)
    - [AudioManager Slot Pool API](#audiomanager-slot-pool-api)
15. [Reverb Send Buses](#reverb-send-buses)
    - [ReverbSendBus](#reverbsendbus)
    - [ReverbSendBusRegistry](#reverbsendbusregistry)
    - [AudioReverbProfile](#audioreverbprofile)
    - [AudioZone reverb fields](#audiozone-reverb-fields)
    - [AudioManager Reverb-Sends API](#audiomanager-reverb-sends-api)
16. [Enums & Data Structures](#enums--data-structures)
17. [Loading](#loading)
    - [Always available (SFXSystem.Runtime)](#always-available-sfxsystemruntime)
    - [Gated by com.unity.addressables](#gated-by-comunityaddressables)
18. [Netcode (Multiplayer)](#netcode-multiplayer)
    - [NetworkedAudioManager](#networkedaudiomanager)
    - [NetworkedSoundHandle](#networkedsoundhandle)
    - [NetworkMusicTransport & NetworkClock](#networkmusictransport--networkclock)
    - [NetworkAudioSettings](#networkaudiosettings)
    - [NetworkAudioEvent & NetworkAudioScope](#networkaudioevent--networkaudioscope)

---

## AudioManager

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour` (partial class)
**Access:** Singleton via `AudioManager.Instance`

### Inspector Configuration

The core AudioManager fields. Occlusion-related inspector fields (slot pools, ray count, smoothing taus) live in the [AudioManager Slot Pool API](#audiomanager-slot-pool-api) section. Reverb-sends fields live in the [AudioManager Reverb-Sends API](#audiomanager-reverb-sends-api) section.

#### Lifecycle

| Field                  | Default | Description |
|------------------------|---------|-------------|
| `persistAcrossScenes`  | true    | On: `DontDestroyOnLoad`, as before 3.1 (the manager must be a root object), and a second manager removes its whole GameObject. Off: no `DontDestroyOnLoad`; the manager and its voices belong to its scene, and a second manager removes only its own component, with a warning. Read as `PersistAcrossScenes`. |

The voices and every coroutine Pepper runs live on "AudioVoices", a root object of Pepper's own (kept or scene-bound like the manager, destroyed with it), so switching the manager's GameObject or a parent off changes nothing.

#### Audio Configuration

| Field                                | Default | Description                                                                              |
|--------------------------------------|---------|------------------------------------------------------------------------------------------|
| `masterMixerGroup`                   | null    | Optional master mixer group handle. Not directly routed by AudioManager; exposed for project-side scripts. |
| `masterVolume` (range 0–1)           | 1       | Global linear volume multiplier applied to every voice. Combines with per-bus volumes.   |
| `muteAll`                            | false   | Global mute switch. When ON, every voice's output is forced to zero.                     |

#### Voices

| Field                  | Default | Description                                                                                                   |
|------------------------|---------|---------------------------------------------------------------------------------------------------------------|
| `maxRealVoices`        | 32      | Maximum voices playing through real AudioSources at once. Beyond this, the least important plays are kept virtual, a whole play at a time, and made real again when they matter more. Pepper warns at startup if this is above Unity's own Max Real Voices. |
| `maxVirtualVoices`     | 64      | Maximum virtual voices tracked behind the real-voice cap. Real + Virtual is the budget: when it is full, a new play makes room by ending whole plays (never persistent ones), or is skipped if it can't. |
| `stealHeadroom`        | 4       | Spare AudioSources beyond Real + Virtual, for stolen sounds still fading out. |
| `stealFadeSeconds`     | 0.025   | How long a stolen sound that can be heard fades out (pool steals, Max Instances steals, a Switch's Restart), never fewer than three frames; 0 = the old cut (3.1.0-dev.8). Read and set as `StealFadeSeconds`. |
| `stealDeclickSeconds`  | 0.005   | With `stealFadeSeconds` 0 only: how far ahead on the DSP clock a stolen voice is cut. |
| `voiceUpdateInterval`  | 0.1     | Seconds between voice priority / virtualization passes (default 10 Hz).                                       |
| `spatialMode`          | Unity   | Who pans and attenuates 3D sounds: Unity, or Pepper from every listening point (3.1.0-dev.8). See [Listening Point Methods](#listening-point-methods). |
| `updateMode`           | Automatic | When Pepper's per-frame step (parameter ramps, Blend layers, virtual voices) runs. See [UpdateMode / Tick](#updatemode--tick). |

#### Occlusion / LOD

| Field                       | Default | Description                                                                              |
|-----------------------------|---------|------------------------------------------------------------------------------------------|
| `occlusionMask`             | ~0      | Physics layer mask used by the occlusion raycast. The rays ignore trigger colliders, and AudioZone/AudioPortal make their colliders triggers when authored, so their layer doesn't need excluding. |
| `occlusionUpdateInterval`   | 0.2     | Seconds between OcclusionUpdate ticks (raycast cadence, default 5 Hz). Per-frame smoothing handles the in-between. |
| `enableLOD`                 | true    | Voice virtualization: once more than `maxRealVoices` (or a bus's `MaxRealVoices`) would be heard, the least important plays (distance × volume × the priority's weight, from a play's loudest voice) are virtualized, a whole play at a time. Off = nothing is virtualized for the budget (a Blend's Silent Layers still are). |
| `lodDistances`              | `{10, 25, 50, 100}` | Not used in this version; hidden in the inspector. There are no distance tiers. |

See the slot-pool subsection for `occlusionRayCount`, `occlusionRaySpreadMeters`, `occlusionGainSmoothingSeconds`, `occlusionCutoffSmoothingSeconds`, `voiceMixer`, `occlusionLayout`, and `defaultOcclusionSlotsPerBus`. See the reverb-sends subsection for `reverbSendBusRegistry` and `reverbSendsUpdateInterval`.

---

### Properties

#### Instance
```csharp
public static AudioManager Instance { get; }
```
**Description:** Singleton instance of AudioManager.
**Returns:** The active AudioManager in the scene.

#### Inert
```csharp
public static bool Inert { get; set; }
```
**Description:** For a dedicated server: set it once at boot, before any AudioManager starts. While true, every post (`AudioEvent.Post`, `PostMultiPosition`, `AudioManager.Post` / `PostEvent`, `AudioEmitter.Post`) returns one shared, already-finished handle without allocating or logging, with an AudioManager or without one; an AudioManager that starts builds no voices, coroutines or per-frame step (it still becomes `Instance` and loads the event registry); the manager's parameter, switch and RTPC calls do nothing; the ambient system doesn't create itself. Setting it while an AudioManager runs warns and keeps the value. False again at the start of each play session. See USER_GUIDE §17.11.

#### PersistAcrossScenes
```csharp
public bool PersistAcrossScenes { get; }
```
**Description:** The `persistAcrossScenes` setting (see Lifecycle above), read in Awake.

---

### Event Methods

#### PostEvent
```csharp
public AudioHandle PostEvent(string eventName, GameObject source = null, Vector3 position = default)
```
**Description:** Triggers an audio event by name.

**Parameters:**
- `eventName` - Name of the event to trigger (must exist in registry or Resources/Audio/Events/)
- `source` - (Optional) GameObject to attach sound to
- `position` - (Optional) 3D world position for spatial audio

**Returns:** `AudioHandle` for controlling playback, or `null` if event not found.

**Example:**
```csharp
AudioHandle handle = AudioManager.Instance.PostEvent("Play_Explosion");
AudioHandle handle2 = AudioManager.Instance.PostEvent("Play_Footstep", gameObject, transform.position);
```

#### GetEvent
```csharp
public AudioEvent GetEvent(string eventName)
```
**Description:** Gets an AudioEvent by name.

**Parameters:**
- `eventName` - Event name

**Returns:** `AudioEvent`, or `null` if not found.

#### GetEventContainer
```csharp
public AudioContainer GetEventContainer(string eventName)
```
**Description:** Gets the first container referenced by an event.

**Parameters:**
- `eventName` - Event name

**Returns:** `AudioContainer`, or `null` if not found.

---

### Posting by Reference

```csharp
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter)
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter, string point)
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter, PostValues values)
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter, string point, PostValues values)
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter, Vector3 localOffset, PostValues values)
public AudioHandle Post(AudioEvent audioEvent, AudioEmitter emitter, double startDsp, double startOffsetSeconds,
                        PostValues values = default)
public AudioHandle Post(AudioEvent audioEvent, GameObject source = null, Vector3 position = default)
```
**Description:** Post an event you hold a reference to: the same as `audioEvent.Post(emitter)`, `audioEvent.PostAtOffset(emitter, localOffset, values)` (an impact at the contact point), `audioEvent.Post(emitter, startDsp, startOffsetSeconds, values)` (a moment of the sound at a DSP time) and `audioEvent.Post(source, position)`; see [AudioEvent Post](#post) and [PostValues](#postvalues). Returns `null` (with a warning) when `audioEvent` is null. Write `Post(evt)` rather than `Post(evt, null)`: a bare `null` matches both overloads.

### Random Numbers

```csharp
public AudioRandomSource RandomNumbers { get; set; }   // Pepper (default) or Unity
```
**Description:** Where sounds get random numbers (Random containers' picks, a Sequence's random order, volume and pitch randomization, multi-position offsets). **Pepper**: a sound posted on an [AudioEmitter](#audioemitter) draws from that emitter's own sequence (its `RandomSeed`), other sounds from one shared Pepper sequence; `UnityEngine.Random`'s state is never touched. **Unity**: `UnityEngine.Random`, as before 3.1. Inside a networked post's `AudioRandomScope`, `UnityEngine.Random` is used (seeded, then put back) either way, so every machine picks the same variation.

---

### Parameter Methods

Parameters are explained in USER_GUIDE Manual §3.4. These methods use the **global** scope: every sound with no emitter or play value reads it. Per-emitter values are on [AudioEmitter](#audioemitter), per-play values on [AudioHandle](#audiohandle). The string RTPC methods below are the same global values addressed by name: a name and an [AudioParameter](#audioparameter) asset with that Parameter Name share one value.

#### SetParameter / ClearParameter / GetParameter / SnapParameter
```csharp
public void SetParameter(AudioParameter parameter, float value)
public void ClearParameter(AudioParameter parameter)
public float GetParameter(AudioParameter parameter)
public void SnapParameter(AudioParameter parameter)
```
**Description:** Set, forget, read or snap the global value. Sounds follow a value set through the asset with the parameter's [smoothing](#parameterslew); the first one is heard at once. `GetParameter` returns what sounds hear now (after smoothing), else the default, clamped to the parameter's range. `ClearParameter` falls back to the default, easing there if smoothed. `SnapParameter` skips the smoothing now. `SetParameter` tells `IRTPCListener`s the written value, by the parameter's name; `ClearParameter` doesn't. A write outside the range is stored as written and logs one warning per parameter. A value that isn't a number (NaN, an infinity) is refused at every scope and by `SetRTPC`, ramps and `PostValues` too: sounds keep the last value, listeners aren't told, and the parameter warns once.

#### RampParameter
```csharp
public void RampParameter(AudioParameter parameter, float target, float seconds)
```
**Description:** Move the written global value to `target` over `seconds` (unscaled time, linear), from the value written before (else the default). Smoothing applies on top. A new ramp, a Set, a Snap or a Clear on the same parameter replaces it. Listeners are told every frame it moves.

#### SetRTPC
```csharp
public void SetRTPC(string parameterName, float value)
```
**Description:** Set the global value of the parameter named `parameterName`. Every sound without an emitter or play value follows it at once: values set by name are never smoothed, even if a Parameter asset of that name is. Blend layers pick it up in Pepper's per-frame step later in the same frame. Ends a `TransitionRTPC` running on the same name. Registered `IRTPCListener`s are told at once, inside the call.

**Parameters:**
- `parameterName` - Parameter name (case-sensitive)
- `value` - Parameter value. A name has no range of its own; a Blend that names it reads it clamped to 0–1.

**Example:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", 0.75f);
AudioManager.Instance.SetRTPC("EngineRPM", 0.5f);
```

#### GetRTPC
```csharp
public float GetRTPC(string parameterName)
```
**Description:** The global value set under this name, as written (not smoothed, not clamped).

**Returns:** The value, or `0f` if none has been set. A Parameter asset's default does not apply here.

#### TransitionRTPC
```csharp
public void TransitionRTPC(string parameterName, float targetValue, float duration)
```
**Description:** Moves the global value to a target over time (unscaled, linear), starting from `GetRTPC`. **A second call on the same name replaces the first**, and so does `SetRTPC`; before 3.1 both transitions kept running and fought. States with a transition time use this.

**Parameters:**
- `parameterName` - Parameter name
- `targetValue` - Target value
- `duration` - Transition duration in seconds (0 or less = `SetRTPC`)

**Example:**
```csharp
AudioManager.Instance.TransitionRTPC("CombatIntensity", 1.0f, 2f);
```

#### GetAllRTPCs
```csharp
public IReadOnlyDictionary<string, float> GetAllRTPCs()
```
**Description:** Every global value that has been set, by name and as written: values set with `SetRTPC` and global writes through Parameter assets. A live, read-only view. Enumerating it allocates, so use it in tools, not per frame. Writing a value while enumerating doesn't throw.

**Returns:** Parameter names mapped to their global values.

#### RegisterRTPCListener / UnregisterRTPCListener
```csharp
public void RegisterRTPCListener(IRTPCListener listener)
public void UnregisterRTPCListener(IRTPCListener listener)
```
**Description:** Registers/unregisters an object to receive callbacks when a **global** value changes: `SetRTPC`, `SetParameter`, and every frame of a `TransitionRTPC` or global `RampParameter`. Called synchronously. Emitter and play values don't notify.

**Example:**
```csharp
// Implement IRTPCListener
public class MyComponent : MonoBehaviour, IRTPCListener
{
    void OnEnable() => AudioManager.Instance.RegisterRTPCListener(this);
    void OnDisable() => AudioManager.Instance.UnregisterRTPCListener(this);

    public void OnRTPCChanged(string parameter, float value)
    {
        if (parameter == "EngineRPM") UpdateEngine(value);
    }
}
```

### Listening Point Methods

```csharp
public AudioListeningPoint AddListeningPoint(Transform follow = null)
public void RemoveListeningPoint(AudioListeningPoint point)
public IReadOnlyList<AudioListeningPoint> ListeningPoints { get; }
public void SetPrimaryListeningPoint(AudioListeningPoint point)   // null = the first registered (3.1.0-dev.8)
public AudioListeningPoint PrimaryListeningPoint { get; }          // 3.1.0-dev.8
public AudioSpatialMode SpatialMode { get; set; }                  // Unity (default) | Pepper (3.1.0-dev.8)
public void DriveUnityListener(AudioListener listener)    // null stops it
public AudioListener DrivenUnityListener { get; }
```
**Description:** Tell Pepper where the game listens from (see [AudioListeningPoint](#audiolisteningpoint)). While any point is registered the scene is never searched; with none, Pepper falls back to the scene's AudioListener (see [ListenerUtil](#listenerutil)). Points stay registered if the manager is replaced. A point whose followed transform is destroyed is removed, with one warning a session (since 3.1.0-dev.8; it used to stay where it last was).

**The primary point** (3.1.0-dev.8): the one the game picks with `SetPrimaryListeningPoint`, else the first registered. `DriveUnityListener` moves an AudioListener to it in every step, before Unity mixes, so Unity's 3D panning hears from the same place (skipped when the listener is on the transform the point follows); `ListenerUtil.Get` / `GetPosition`, the ambient system and SingleEmitter posts read it; with Spatial Mode = Unity it is the only point Pepper listens from. Picking another doesn't make sounds jump: what Pepper measures from it (the built-in Distance, Approach Speed and Angle, so Doppler and the curves on them; attenuation filters) glides to the new one over 100 ms. A point that isn't registered is refused, with a warning.

**SpatialMode** (3.1.0-dev.8; inspector: **Spatial Mode**). `Unity` (the default): Unity's 3D from its AudioListener, as before; weights and pan widths are unused. `Pepper`: every 3D voice plays at Spatial Blend 0 and Pepper sets its level (`GainStack.SpatialGain`) and `panStereo` from every point each frame. For each point, the voice's attenuation there (its Attenuation asset's volume curve, else Unity's rolloff for its Min and Max Distance) × the point's `Weight` is its contribution; the level is the largest, the pan Σ contribution × `PanWidth` × the point's pan ÷ Σ contribution, eased over about 50 ms. At one point of weight 1 and width 1 it reproduces Unity's 3D (rolloffs, constant-power pan, Spread; measured on 6000.0 and 6000.3). Each play is measured from the point that hears it loudest (changing only when another is 1 dB louder): its built-ins, Doppler, attenuation filters, occlusion rays, the Furthest steal. It goes virtual past Max Distance only from every point. Doppler is always Pepper's (a container on Unity's gets Pepper's at its Doppler Level). Levels ease over 100 ms after the points change. No spatializer plugins; the project's Doppler Factor isn't read; a stereo clip is balanced, not placed. A change applies to the sounds started after it. USER_GUIDE §9.3.

**Example:**
```csharp
var point = AudioManager.Instance.AddListeningPoint(mainCamera.transform);
// ... when the camera is destroyed:
AudioManager.Instance.RemoveListeningPoint(point);
```

#### UpdateMode / Tick
```csharp
public AudioUpdateMode UpdateMode { get; set; }   // Automatic (default) | Manual
public void Tick()
```
**Description:** When Pepper's per-frame step runs. In order: parameter ramps and smoothing (global, then emitters, then plays); bus parameter curves; the emitters and listening points (velocities, `DriveUnityListener`), with voices that follow their emitter moved to it; every dynamic play's tree (Blend layer levels, curves, pitch follow, switches); with Spatial Mode = Pepper, every 3D voice's level and pan; then virtual voices' positions, the fade around virtualization and the steal fade. **Automatic** runs it once a frame from a PlayerLoop step just before `PostLateUpdate.UpdateAudio`, after every LateUpdate, so values set anywhere in this frame's game code are heard this frame. It starts with the frame after the manager's Awake. **Manual** runs it only when you call `Tick()`, once a frame after your last write; ramps advance by `Time.unscaledDeltaTime`. Calling `Tick()` in Automatic mode advances ramps twice. Inspector field: **Update Mode**.

---

### Switch Methods

#### SetSwitch
```csharp
public void SetSwitch(string group, string value)
public void SetSwitch(AudioSwitchGroup group, string value)
```
**Description:** Sets the GLOBAL value of a switch group: every sound with no value of its own (handle) and none on its emitter reads it. A name and an [AudioSwitchGroup](#audioswitchgroup) asset with that name are one switch. A value not in the asset's Values warns once; a group Driven By a parameter ignores it, with one warning (by name, only when the asset is loaded).

**Parameters:**
- `group` - Switch group name (e.g., "Surface_Type") or asset
- `value` - Switch value (e.g., "Metal", "Wood")

**Example:**
```csharp
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
AudioManager.Instance.SetSwitch(surfaceType, "Metal");   // the same switch, through its asset
```

#### ClearSwitch
```csharp
public void ClearSwitch(AudioSwitchGroup group)
public void ClearSwitch(string group)
```
**Description:** Forgets the global value: sounds read the group's Default Value (a Switch that only names its group: none, so its Default Container).

#### GetSwitch
```csharp
public string GetSwitch(string group)
public string GetSwitch(AudioSwitchGroup group)
```
**Description:** By name: the global value set, or empty string if not set. By asset: what a sound with no handle or emitter value reads (the global value, else the Default Value; for a driven group, what the parameter's global value gives).

#### GetAllSwitches
```csharp
public IReadOnlyDictionary<string, string> GetAllSwitches()
```
**Description:** Gets all current switch values.

**Returns:** Dictionary mapping switch groups to their current values.

---

### State Methods

#### SetState
```csharp
public void SetState(string stateName, float transitionTime = 0.5f)
```
**Description:** Activates an audio state; the group's previous state ends. A state whose `VolumeMode` is `Offset` moves its buses by its dB values (their `StateOffsetDb`), and the previous state's offsets go back, over `transitionTime` (unscaled, in Pepper's step). A `SetLevel` state (the default) sets the buses' Volume and puts nothing back, as before.

**Parameters:**
- `stateName` - Name of state to activate
- `transitionTime` - Duration of transition in seconds (default: 0.5s)

**Example:**
```csharp
AudioManager.Instance.SetState("Underwater", 1.5f);
AudioManager.Instance.SetState("Paused", 0.3f);
```

#### ClearState
```csharp
public void ClearState(string stateGroup, float transitionTime = 0.5f)
```
**Description:** Leaves `stateGroup`'s state without setting another (3.1.0-dev.7): an `Offset` state's bus offsets go back over `transitionTime`; a `SetLevel` state's changes stay. The group has no active state afterwards.

#### SetStateInGroup
```csharp
public void SetStateInGroup(string stateGroup, string stateName, float transitionTime = 0.5f)
```
**Description:** Sets a state within a specific state group.

**Parameters:**
- `stateGroup` - State group name
- `stateName` - State name within that group
- `transitionTime` - Transition duration

**Example:**
```csharp
AudioManager.Instance.SetStateInGroup("Location", "Underwater", 1.5f);
```

#### GetActiveState
```csharp
public AudioState GetActiveState(string stateGroup)
```
**Description:** Gets the currently active state for a state group.

**Returns:** Active `AudioState`, or `null` if no state active.

#### GetActiveStateName
```csharp
public string GetActiveStateName(string stateGroup)
```
**Description:** Gets the name of the currently active state.

**Returns:** State name, or empty string if none active.

#### GetAllActiveStates
```csharp
public IReadOnlyDictionary<string, AudioState> GetAllActiveStates()
```
**Description:** Gets all active states across all groups.

**Returns:** Dictionary mapping state group to active state.

---

### Bus Methods

#### SetBusVolume
```csharp
public void SetBusVolume(string busName, float volumeDb, float transitionTime = 0f)
```
**Description:** Sets a bus's Volume in decibels (the asset's value; an empty name is the master bus). **A name no bus has changes nothing and warns once per name** (since 3.1.0-dev.7; it used to set the master bus). For a player's volume from a Settings page, use [AudioBus.PlayerVolumeDb](#audiobus) instead: a state can't undo it and it isn't saved to the asset.

**Parameters:**
- `busName` - Name of the bus
- `volumeDb` - Target volume in dB (0 = no change, -6 = half, -80 = silence)
- `transitionTime` - Transition duration in seconds (0 = instant)

**Example:**
```csharp
AudioManager.Instance.SetBusVolume("Music", -6f, 1f);  // Duck to half volume over 1 second
AudioManager.Instance.SetBusVolume("SFX", 0f, 0.5f);    // Return to full over 0.5s
```

#### TransitionBusVolume
```csharp
public void TransitionBusVolume(AudioBus bus, float targetVolumeDb, float transitionTime)
```
**Description:** Transitions a bus to a target volume over time using the bus reference directly.

**Parameters:**
- `bus` - The AudioBus reference
- `targetVolumeDb` - Target volume in dB
- `transitionTime` - Transition duration

#### SetBusEffectProperty
```csharp
public void SetBusEffectProperty(AudioBus bus, string propertyName, float value, float transitionTime = 0f)
```
**Description:** Sets an effect property on a bus with optional transition.

**Parameters:**
- `bus` - Target bus
- `propertyName` - Effect property name
- `value` - Target value
- `transitionTime` - Transition duration (0 = instant)

#### GetBus
```csharp
public AudioBus GetBus(string busName)
```
**Description:** Gets a bus by name.

**Returns:** `AudioBus`, or the master bus for an empty name. For a name no bus has it also returns the master bus, as before, and warns once per name (since 3.1.0-dev.7).

#### GetAllBuses
```csharp
public List<AudioBus> GetAllBuses()
```
**Description:** Gets all registered buses.

**Returns:** List of all buses.

---

### Crossfade Methods

#### CrossFade
```csharp
public void CrossFade(AudioContainer from, AudioContainer to, float duration,
    AudioEvent.CrossfadeType type = AudioEvent.CrossfadeType.EqualPower)
```
**Description:** Crossfades between two containers.

**Parameters:**
- `from` - Container to fade out
- `to` - Container to fade in
- `duration` - Crossfade duration in seconds
- `type` - Crossfade curve type (Linear or EqualPower)

Linear fades in with `x` and out with `1 − x`; EqualPower fades in with `sin(x·π/2)` and out with `cos(x·π/2)`. The incoming voices play unbussed (through the container's Mixer Group); the CrossFade event action routes them through its Target Bus instead, and places a 3D container at the post's position or source GameObject. Fades only the "from" voices playing now: what the outgoing sound was still going to start (a delayed Play, a Sequence's next clip) still starts. Use the handle overload below for that.

**Example:**
```csharp
AudioManager.Instance.CrossFade(oldMusicContainer, newMusicContainer, 2f);
```

#### CrossFade (handle)
```csharp
public AudioHandle CrossFade(AudioHandle from, AudioEvent to, float duration,
    AudioEvent.CrossfadeType type = AudioEvent.CrossfadeType.EqualPower,
    GameObject source = null, Vector3 position = default)
```
**Description:** Fades out every voice of `from` and cancels what it was still going to start (delayed Play actions, a Sequence's next clip), posts `to` and fades in the voices that post started. Returns the new handle (null when `to` is null).

- `from` finishes (`OnFinished`) when its fade-out ends; `OnStopping` does not fire.
- `to` is an ordinary post: bus, priority, cooldown and Max Instances apply. If it is the same event as `from` and Max Instances is reached, the old play is ended at once by the instance limit.
- Voices `to` starts later are not faded in.
- Either argument may be null.

**Example:**
```csharp
music = AudioManager.Instance.CrossFade(music, combatMusicEvent, 2f);
```

---

### Volume & Global Control Methods

#### SetMasterVolume
```csharp
public void SetMasterVolume(float volume)
```
**Description:** Sets master volume (global volume multiplier).

**Parameters:**
- `volume` - Volume in linear scale (0-1)

**Example:**
```csharp
AudioManager.Instance.SetMasterVolume(0.75f);  // 75% volume
```

#### GetMasterVolume
```csharp
public float GetMasterVolume()
```
**Description:** Gets current master volume.

**Returns:** Master volume (0-1).

#### MasterVolumeDb / SetMasterVolumeDb
```csharp
public float MasterVolumeDb { get; set; }
public void SetMasterVolumeDb(float db)
```
**Description:** The master volume in dB, beside the linear setter (the same value; 3.1.0-dev.7). Above 0 dB stays at full; −80 dB and below is silent.

#### IsAllMuted
```csharp
public bool IsAllMuted { get; }
```
**Description:** True while `MuteAll(true)` has every sound muted (3.1.0-dev.7).

#### MuteAll
```csharp
public void MuteAll(bool mute)
```
**Description:** Mutes or unmutes all audio.

**Parameters:**
- `mute` - `true` to mute, `false` to unmute

**Example:**
```csharp
AudioManager.Instance.MuteAll(true);   // Mute everything
AudioManager.Instance.MuteAll(false);  // Unmute everything
```

#### StopAllSounds
```csharp
public void StopAllSounds()
```
**Description:** Stops all currently playing sounds.

#### PauseAll
```csharp
public void PauseAll()
```
**Description:** Pauses all currently playing sounds, at once. Sounds posted afterwards play. For a game's pause, where a pause menu must still sound, pause buses instead ([AudioBus.Pause](#pause--resume--ispaused)).

#### UnpauseAll
```csharp
public void UnpauseAll()
```
**Description:** Resumes all paused sounds.

---

### Voice Management Methods

#### GetVoice
```csharp
public AudioVoice GetVoice()
```
**Description:** Allocates a voice from the pool. **Internal use only.**

**Returns:** Available voice, or `null` if pool exhausted.

#### ReturnVoice
```csharp
public void ReturnVoice(AudioVoice voice)
```
**Description:** Returns a voice to the pool. **Internal use only.**

#### GetStatistics
```csharp
public AudioStatistics GetStatistics()
```
**Description:** Gets current audio system statistics.

**Returns:** `AudioStatistics` with voice counts and system info.

**Example:**
```csharp
var stats = AudioManager.Instance.GetStatistics();
Debug.Log($"Real voices: {stats.activeVoices}/{stats.realVoiceBudget}");
```

---

### Debug Methods

#### GetVirtualVoiceCount
```csharp
public int GetVirtualVoiceCount()
```
**Description:** Gets the number of currently virtualized voices.

#### GetActiveVoicesDebug
```csharp
public List<AudioVoiceDebugInfo> GetActiveVoicesDebug()
```
**Description:** Gets detailed debug info for all active voices: for a virtual one, why (`virtualReasonKind`, see `VoiceVirtualReason`); its level, pitch and region.

**Returns:** List of `AudioVoiceDebugInfo` for each active voice.

#### GetPlaysDebug
```csharp
public void GetPlaysDebug(List<AudioPlayDebugInfo> into)
```
**Description:** Fills `into` with every play that has a live voice, each with its voices, ordered by play number: what the editor's Live View (USER_GUIDE §10.6) shows per emitter. A play with nothing sounding right now (a Switch with no choice, a Sequence between clips, a Delay still running) isn't listed. Allocates; for tools and debug overlays, not per-frame game code.

#### ReasonText
```csharp
public static string ReasonText(VoiceVirtualReason reason)
```
**Description:** The words the debug windows show for a reason: "over the cap", "silent layer", "past Max Distance", "held by code", "bus paused", "emitter paused", "stopping"; empty for `None`.

---

## AudioEvent

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

#### EventName
```csharp
public string EventName { get; }
```
**Description:** Unique name identifying this event.

#### Actions
```csharp
public List<EventAction> Actions { get; }
```
**Description:** List of actions to execute when this event is posted.

#### Priority
```csharp
public VoicePriority Priority { get; }
```
**Description:** How important this event's sounds are (Low, Medium, High, Critical): a weight in each voice's importance, which decides what goes virtual and what is stolen first. Critical is the strongest weight, not an exemption; to keep a sound from ever being stolen, tick `Persistent`.

#### MaxInstances
```csharp
public int MaxInstances { get; }
```
**Description:** Maximum concurrent plays (0 = unlimited). One play is everything a single Play action starts: all of a multi-clip Routing container's clips, or all of a Blend's layers, count as one instance.

#### StealBehavior
```csharp
public VoiceStealBehavior StealBehavior { get; }
```
**Description:** Which play to end when the limit is reached (Oldest, Quietest, Furthest, LowestPriority). A steal ends the whole play, meaning every voice it started. Oldest means the play that started first, judged by start time rather than playback position. Quietest uses each play's loudest voice. Furthest uses each play's nearest voice.

#### Cooldown
```csharp
public float Cooldown { get; }
```
**Description:** Minimum time between triggers in seconds.

#### MaxInstancesPerEmitter / CooldownPerEmitter
```csharp
public int MaxInstancesPerEmitter { get; }   // 0 = no limit per emitter
public float CooldownPerEmitter { get; }     // seconds; 0 = none
```
**Description:** The same limits for one [AudioEmitter](#audioemitter), on top of the game-wide ones: a post on an emitter already playing `MaxInstancesPerEmitter` plays of this event ends one of **that emitter's** plays, chosen by `StealBehavior`; a post sooner than `CooldownPerEmitter` after the last one on that emitter returns a finished handle. Other emitters aren't affected. Posts without an emitter aren't counted per emitter. On the event's `TimeMode`, like `Cooldown`.

#### TimeMode
```csharp
public AudioTimeMode TimeMode { get; set; }   // Scaled (default) or Unscaled
```
**Description:** Inspector: **Time**. The clock this event's timing runs on: `Cooldown`, `CooldownPerEmitter`, Delay actions, its sounds' Sequence gaps, its CrossFade action and `AudioManager.CrossFade(handle, thisEvent)`. Scaled is game time: it stops at time scale 0, as before. Unscaled is real time, for UI and pause-menu sounds. Changing it mid-session compares the next post with a time from the old clock once.

```csharp
public enum AudioTimeMode { Scaled, Unscaled }
```

#### Persistent
```csharp
public bool Persistent { get; }
```
**Description:** Inspector: **Persistent**. This event's plays are never stolen by the voice pool to make room for other sounds (an engine). They can still go virtual, and this event's own Max Instances can still end one. If the pool is full of persistent plays, a new sound is skipped with a warning. Their voices get Unity's highest `AudioSource.priority` (0).

---

### Methods

#### Post
```csharp
public AudioHandle Post(GameObject source = null, Vector3 position = default)
```
**Description:** Posts the event. Optionally attach to a GameObject and/or specify a 3D position.

**Parameters:**
- `source` - (Optional) GameObject to attach sound to
- `position` - (Optional) 3D world position for spatial audio

**Returns:** `AudioHandle` for controlling playback.

**Example:**
```csharp
// Non-spatial (2D)
AudioHandle handle = myEvent.Post();

// Attached to a GameObject at its position
AudioHandle handle = myEvent.Post(gameObject, transform.position);

// At a specific world position, not attached
AudioHandle handle = myEvent.Post(null, explosionPosition);
```

`Post(GameObject, Vector3, double scheduledDspTime)` starts the voices at an exact DSP time instead of the next buffer (tempo-driven callers).

#### Post (on an emitter)
```csharp
public AudioHandle Post(AudioEmitter emitter)
public AudioHandle Post(AudioEmitter emitter, double scheduledDspTime)
```
**Description:** Posts the event on an [AudioEmitter](#audioemitter). 3D voices are placed and attached exactly as with `Post(emitter.gameObject, emitter.transform.position)`, and the sounds read the emitter's parameter values (after the handle's own, before the global ones). A null or destroyed emitter logs a warning and returns a finished handle with nothing playing. Same as `emitter.Post(evt)`.

> **Source change in 3.1:** `myEvent.Post(null)` with a bare `null` no longer compiles: it matches both this overload and `Post(GameObject, ...)`. Write `myEvent.Post()` for a 2D post.

#### Post (with values for that sound)
```csharp
public AudioHandle Post(AudioEmitter emitter, PostValues values)
public AudioHandle Post(AudioEmitter emitter, string point, PostValues values)
public AudioHandle Post(GameObject source, Vector3 position, PostValues values)
```
**Description:** Posts with [PostValues](#postvalues) for this sound only: they are set on the handle's play scope before any Play action runs, so the first sample hears them (an impact's strength, a footstep's surface). A delayed Play action reads them too.

#### PostAtOffset (an emitter, anywhere on it)
```csharp
public AudioHandle PostAtOffset(AudioEmitter emitter, Vector3 localOffset, PostValues values = default)
```
**Description:** Posts on `emitter` at `localOffset` (metres, in its own space), as at a named point but anywhere: an impact at the contact point (3.1.0-dev.7). The sound moves and turns with the emitter, reads its values, counts toward its Max Instances Per Emitter and Cooldown Per Emitter, and draws from its random numbers. Move it later with `AudioHandle.SetLocalOffset`. Same as `emitter.Post(evt, localOffset, values)`. It isn't an overload of `Post`, so that `Post(null, position, values)` keeps compiling.

#### Post (a moment of the sound at a DSP time)
```csharp
public AudioHandle Post(AudioEmitter emitter, double startDsp, double startOffsetSeconds, PostValues values = default)
public AudioHandle Post(GameObject source, Vector3 position, double startDsp, double startOffsetSeconds,
                        PostValues values = default)
```
**Description:** `startOffsetSeconds` into the sound (seconds of its clips at their recorded pitch) is heard at the DSP time `startDsp` (3.1.0-dev.7): a pass-by whoosh whose peak lands on the pass. If `startDsp` has passed, or is nearer than the next DSP buffer, the sound starts at the next buffer that much further in (at its pitch), so the same moment still lands on time; a one-shot already past its end plays nothing and its handle finishes; a loop wraps. Every voice the Play actions start with takes the offset (a Blend's layers stay aligned); a Sequence's later clips start from their beginning; a Play action with a Delay keeps its own timing. `startDsp` 0 = as soon as possible, from the offset. Same as `emitter.Post(evt, startDsp, startOffsetSeconds, values)`.

#### EstimateDuration
```csharp
public AudioDuration EstimateDuration(PostValues values = default)
public AudioDuration EstimateDuration(AudioEmitter emitter, PostValues values = default)
```
**Description:** How long the event plays if posted now with `values` (3.1.0-dev.7), before posting it. Random container: its shortest and longest clip. Switch: the choice the values pick (the given values, then the emitter's, then global, then the group's Default). Blend and Routing: their longest layer (a Blend's Loop Mode decides whether it loops). Sequence: the entry it plays next, or with Auto Advance the rest of the run (a run that starts again loops). Every Play action counts, its Delay added; the longest wins. A custom container: the shortest and longest of the clips it lists. Clips are measured at their recorded pitch; an addressable clip that isn't loaded is unknown. Draws no random numbers.

### AudioDuration

**Namespace:** `AudioSystem` (readonly struct)

```csharp
public float Shortest { get; }   // seconds
public float Longest { get; }    // seconds; for a loop, one time through
public bool Loops { get; }       // plays until stopped
public bool IsKnown { get; }     // false when nothing would play (the lengths are 0)
```

### PostValues

**Namespace:** `AudioSystem` (struct)

```csharp
public const int MaxParameters = 4;
public const int MaxSwitches = 2;
public static PostValues Of(AudioParameter parameter, float value)
public static PostValues Of(AudioSwitchGroup group, string value)
public PostValues With(AudioParameter parameter, float value)   // a later value for the same parameter wins
public PostValues With(AudioSwitchGroup group, string value)
public static PostValues VolumeOffset(float db)
public PostValues WithVolumeOffset(float db)          // the handle's SetVolumeOffset, before the first sample
public PostValues WithPitchOffset(float semitones)    // the handle's SetPitchOffset, before the first sample
public int ParameterCount { get; }
public int SwitchCount { get; }
public bool IsEmpty { get; }
```
**Description:** Values a post gives its sound (the play scope, like `AudioHandle.SetParameter` / `SetSwitch`), applied before its first voice starts, and its own volume and pitch offsets (3.1.0-dev.7), which also reach the voices it starts later. Building one allocates nothing. More than 4 parameters or 2 switches: the rest are dropped with one warning; set them on the returned handle.

```csharp
car.Post(impactEvent, "FrontAxle", PostValues.Of(strength, 0.8f).With(surface, "Grass"));
```

#### PostMultiPosition
```csharp
public AudioMultiHandle PostMultiPosition(Vector3[] positions, GameObject source = null)
public AudioMultiHandle PostMultiPosition(Transform[] transforms, GameObject source = null)
public AudioMultiHandle PostMultiPosition(AudioMultiPositionEmitterParent emitterParent, GameObject source = null)
```
**Description:** Posts event in multi-position mode (synchronized across multiple positions or emitters).

**Parameters:**
- `positions` - Array of 3D world positions
- `transforms` - Array of transforms to track
- `emitterParent` - Multi-position emitter parent component
- `source` - (Optional) GameObject to attach the voices to. On a 3D container each voice still plays at its own position and moves with `source`.

**Returns:** `AudioMultiHandle` for controlling all voices.

**Example:**
```csharp
// From position array
Vector3[] speakerPositions = { pos1, pos2, pos3 };
AudioMultiHandle mh = myEvent.PostMultiPosition(speakerPositions);

// From transforms
Transform[] speakers = GetSpeakerTransforms();
AudioMultiHandle mh = myEvent.PostMultiPosition(speakers);

// From emitter parent component
AudioMultiHandle mh = myEvent.PostMultiPosition(emitterParent);
```

#### GetContainer
```csharp
public AudioContainer GetContainer()
```
**Description:** Gets the first container referenced in this event's actions.

**Returns:** `AudioContainer`, or `null` if no Play action exists.

#### GetFirstActiveSource
```csharp
public AudioSource GetFirstActiveSource()
```
**Description:** Gets the first active AudioSource for this event.

**Returns:** `AudioSource`, or `null` if no voice is active.

#### CreateNew (Editor Only)
```csharp
public static AudioEvent CreateNew(string name, AudioContainer container, AudioBus bus)
```
**Description:** Factory method to create a new AudioEvent asset with pre-configured Play action.

---

### EventAction (Nested Class)

```csharp
[Serializable]
public class EventAction
{
    public ActionType type;
    public ActionScope scope;           // Stop/Pause/Resume: All (default) or ThisEmitter
    public AudioContainer container;
    public AudioBus targetBus;          // Play/CrossFade routing; TriggerDucking's trigger bus
    public float delay;
    public float fadeDuration;          // Stop fade-out; CrossFade/SetRTPC/SetState transition;
                                        // TriggerDucking hold time. Play ignores it.
    public AnimationCurve fadeCurve;    // Not used by any action in this version
    public string switchGroup;
    public string switchValue;
    public string rtpcName;
    public float rtpcValue;
    public string stateName;
    public AudioContainer fadeFromContainer;
    public CrossfadeType crossfadeType;

    public enum ActionType
    {
        Play, Stop, Pause, Resume,
        SetSwitch, SetRTPC, SetState,
        TriggerDucking, CrossFade
    }

    public enum ActionScope { All, ThisEmitter }
}
```

**`scope`:** which of this event's sounds a Stop, Pause or Resume action acts on. `All` (the default, as before): every one playing, on every emitter. `ThisEmitter`: only those posted on the [AudioEmitter](#audioemitter) the event is posted on; posted without one, it acts on all.

### CrossfadeType (Nested Enum)

```csharp
public enum CrossfadeType
{
    Linear,      // Linear crossfade
    EqualPower   // Maintains perceived loudness
}
```

---

## AudioHandle & AudioMultiHandle

### AudioHandle

**Namespace:** `AudioSystem`
**Description:** Controls a single audio playback instance: every voice the post started, including voices that start later. A Routing container with several clips, every layer of a Blend, and each clip an auto-advancing Sequence goes on to play all belong to the handle, so `Stop`, `Pause`, the volume and pitch calls, `SeekTo` and `SetLoop` reach the whole sound. `OnFinished` fires only when nothing is sounding and nothing more is coming: after a Sequence's last clip (not during a Delay After gap), and after any delayed Play actions. `Stop()` also cancels whatever was still to come.

#### Properties

```csharp
public string EventName { get; }
public AudioEvent SourceEvent { get; }
public AudioEmitter Emitter { get; } // The emitter it was posted on, or null
public bool IsPaused { get; }
public bool isPlaying { get; }  // True while any voice is playing
public float time { get; }      // Playback time of the primary (first) voice, in seconds
public float duration { get; }  // Clip duration of the primary (first) voice, in seconds
```

---

#### Events

```csharp
public event System.Action OnStarted;
public event System.Action OnLoop;      // declared, but not raised in this version
public event System.Action OnStopping;
public event System.Action OnFinished;
```
**Description:** Callbacks for playback lifecycle events.
- `OnStarted`: a voice of this post started.
- `OnStopping`: a `Stop(fadeTime)` fade-out has begun. Not raised for `Stop(0)` or by crossfades.
- `OnFinished`: the post has finished: every voice has ended and nothing is still to come. Subscribing after it finished invokes the handler at once.
- `OnLoop`: declared but never raised in this version. Don't rely on it.
- Subscribing `OnStarted`, `OnStopping` or `OnLoop` once the handle has finished does nothing: they can't fire any more. (An inert process's posts all return one such shared handle; see `AudioManager.Inert`.)

**Example:**
```csharp
AudioHandle handle = myEvent.Post();
handle.OnStarted += () => Debug.Log("Started");
handle.OnFinished += () => Debug.Log("Finished");
```

---

#### Methods

##### SetVolumeScale
```csharp
public void SetVolumeScale(float scale)
```
**Description:** Scales the sound relative to its authored volume. `1` = as authored, `0.5` = half. Calling it twice with 0.5 means 0.5, not 0.25. Container volume randomization and bus levels still apply on top. Reaches the voices playing now and the ones the sound starts later: a Sequence's next clip, a playing Switch's new choice, a delayed Play action (since 3.1.0-dev.7; before, only the voices playing then). `VolumeScale` reads it back.

**Parameters:**
- `scale` - Multiplier on the authored volume

##### SetVolumeOffset
```csharp
public void SetVolumeOffset(float db)
public float VolumeOffsetDb { get; }
```
**Description:** `SetVolumeScale` in dB (3.1.0-dev.7): `-3` plays the sound 3 dB below how it was authored, now and in the voices it starts later. The same slot as `SetVolumeScale`: the last call wins. Settable at the post with `PostValues.WithVolumeOffset`. Composes with the emitter's Volume Offset. A raise can't take a voice past its AudioSource's full volume.

##### SetVolumeAbsolute
```csharp
public void SetVolumeAbsolute(float linear)
```
**Description:** Replaces the authored volume with an exact level (linear, 1 = full scale), so `SetVolumeAbsolute(1f)` can be louder than the sound was authored. Use `SetVolumeScale` unless you mean to ignore the authored level.

> `SetVolume(float)` is obsolete. It has always been absolute, so it forwards to `SetVolumeAbsolute`. Switch to `SetVolumeScale` if you wanted a multiplier.

##### SetPitchOffset
```csharp
public void SetPitchOffset(float semitones)
```
**Description:** Shifts the voices by `semitones` on top of their own pitch: container pitch randomization and the other pitch-stack slots still apply. Idempotent: `SetPitchOffset(2f)` twice means +2, not +4. Clears an absolute pitch. Reaches the voices playing now and the ones the sound starts later (the same as `SetVolumeScale`, since 3.1.0-dev.7). Settable at the post with `PostValues.WithPitchOffset`. `PitchOffset` reads it back.

**Parameters:**
- `semitones` - Pitch offset (-12 to +12 typical range)

##### SetPitchAbsolute
```csharp
public void SetPitchAbsolute(float semitones)
```
**Description:** Plays the voices at exactly `semitones` from their recorded pitch, replacing container pitch randomization and every other pitch-stack slot (Doppler included). Use `SetPitchOffset` unless you mean to discard the authored variation. Unlike the offset, it reaches only the voices playing now (as does `SetVolumeAbsolute`).

##### SetLocalOffset / SetWorldPosition
```csharp
public void SetLocalOffset(Vector3 offset)
public void SetWorldPosition(Vector3 position)
```
**Description:** Move a playing sound (3.1.0-dev.7): a scrape loop following the contact along a wall. `SetLocalOffset` puts it at `offset` (metres) from the object it was posted on (its emitter, or the GameObject), in that object's space; it keeps moving and turning with it, attached or following (On Disable). `SetWorldPosition` puts it at a world position: on a sound posted on an object that becomes an offset from it; a sound posted at a world position moves there (and `SetLocalOffset` on it is from the world's origin). Voices the sound starts later, and Play actions still waiting out a Delay, start where it was moved to. Takes effect at once.

> `SetPitch(float)` is obsolete. It has always replaced the randomized pitch, so it forwards to `SetPitchAbsolute`. Switch to `SetPitchOffset` for a shift on top of the authored pitch.

##### Parameters (the play scope)
```csharp
public void SetParameter(AudioParameter parameter, float value)
public void ClearParameter(AudioParameter parameter)
public void RampParameter(AudioParameter parameter, float target, float seconds)
public void SnapParameter(AudioParameter parameter)
public void SnapAllParameters()
public void SetSlewOverride(AudioParameter parameter, ParameterSlew slew)
public void ClearSlewOverride(AudioParameter parameter)
public bool TryGetParameter(AudioParameter parameter, out float value)
public float GetEffectiveParameter(AudioParameter parameter)

// For tools: the values on this handle, as on an emitter (the Live View lists them)
public int ParameterCount { get; }
public string GetParameterNameAt(int index)
public float GetParameterValueAt(int index)     // as heard now (after smoothing)
public float GetParameterTargetAt(int index)    // as written
public bool IsParameterMovingAt(int index)      // ramping, smoothing or easing away
```
**Description:** A value for this sound only: it wins over its emitter's and the global value. Use it for values that differ between sounds on one emitter (each axle's slide speed). It is smoothed separately from the emitter's value; the first value set here is heard at once. `ClearParameter` falls back to the emitter's or global value, easing over if smoothed. `RampParameter` moves the written value in a straight line (unscaled time), smoothing on top; a new ramp, a Set, a Snap or a Clear replaces it. `SnapParameter` / `SnapAllParameters` skip the smoothing now. `SetSlewOverride` smooths this sound's value with other settings. `TryGetParameter` returns the value set on this handle, as written. `GetEffectiveParameter` returns what the sound hears now: its own value after smoothing, else its emitter's, else the global value, else the default, clamped to the range.

```csharp
AudioHandle frontAxle = car.Post(tyreEvent);
frontAxle.SetParameter(slideSpeed, 3.5f);
```

##### Attenuation (this play)
```csharp
public void SetAttenuation(AudioAttenuation attenuation)   // null = ClearAttenuation
public void ClearAttenuation()
```
**Description:** Play this sound's 3D voices with another [AudioAttenuation](#audioattenuation) (a hero car heard further away), now and for voices it starts later; `ClearAttenuation` goes back to its containers' own. Changes this play only, never an asset. `GetEffectiveParameter` on a built-in parameter (Distance, Approach Speed, Angle) returns this sound's value.

##### Switches (the play scope)
```csharp
public void SetSwitch(AudioSwitchGroup group, string value)
public void ClearSwitch(AudioSwitchGroup group)
public bool TryGetSwitch(AudioSwitchGroup group, out string value)
public string GetEffectiveSwitch(AudioSwitchGroup group)
```
**Description:** A switch value for this sound only (one axle's surface): it wins over its emitter's and the global value. A Switch Container in the sound follows it as its `WhenSwitchChanges` says (USER_GUIDE §4.5). `GetEffectiveSwitch` returns what the sound reads: its own value, else its emitter's, else the global one, else the group's Default Value; for a group driven by a parameter, what the parameter gives in this sound's scopes (without a playing Switch's hysteresis). A driven group ignores `SetSwitch`, with one warning.

##### Stop
```csharp
public void Stop(float fadeTime = 0.1f)
```
**Description:** Stops playback with optional fade.

**Parameters:**
- `fadeTime` - Fade-out duration in seconds

##### Pause
```csharp
public void Pause()
```
**Description:** Pauses playback (can be resumed).

##### Resume
```csharp
public void Resume()
```
**Description:** Resumes paused playback.

##### Dispose
```csharp
public void Dispose()
```
**Description:** Cleans up event handlers to prevent memory leaks.

**Note:** `Dispose()` is called automatically when `OnFinished` is triggered. Manual disposal is only needed if you want to clean up before the sound finishes (e.g., when destroying a GameObject that subscribed to events).

---

### AudioMultiHandle

**Namespace:** `AudioSystem`
**Description:** Controls multiple synchronized voices (multi-position playback).

#### Properties

```csharp
public string EventName { get; }
public AudioEvent SourceEvent { get; }
public bool IsPaused { get; }
public bool isPlaying { get; }
public float time { get; }
public float duration { get; }
public int VoiceCount { get; }
public bool HasVoices { get; }
public MultiPositionType PositionType { get; }
```

---

#### Events

```csharp
public event System.Action OnStarted;
public event System.Action OnLoop;      // declared, but not raised in this version
public event System.Action OnStopping;
public event System.Action OnFinished;
```
Same meaning as on `AudioHandle`.

---

#### Global Methods (All Voices)

##### SetVolumeScale / SetVolumeAbsolute
```csharp
public void SetVolumeScale(float scale)
public void SetVolumeAbsolute(float linear)
```
**Description:** Same as on `AudioHandle`, applied to every voice. `SetVolumeAbsolute` keeps each emitter's relative balance. `SetVolume(float)` is obsolete and forwards to `SetVolumeAbsolute`.

##### SetPitchOffset / SetPitchAbsolute
```csharp
public void SetPitchOffset(float semitones)
public void SetPitchAbsolute(float semitones)
```
**Description:** Same as on `AudioHandle`, applied to every voice. The offset keeps each voice's randomization and Decorrelated nudge; the absolute pitch replaces them. `SetPitch(float)` is obsolete and forwards to `SetPitchAbsolute`.

##### Stop
```csharp
public void Stop(float fadeTime = 0.1f)
```
**Description:** Stops all voices with fade.

##### Pause / Resume
```csharp
public void Pause()
public void Resume()
```
**Description:** Pauses/resumes all voices.

##### Dispose
```csharp
public void Dispose()
```
**Description:** Cleans up event handlers and voice references.

---

#### Per-Emitter Methods

These methods take an **emitter index**: the position's index in the array (or emitter list) the post was given. Each one acts on every voice that emitter started. If the container starts several voices per play (a multi-clip Routing, a Blend), all of an emitter's layers are affected together. The index does not shift when other voices end.

##### SetVoiceVolumeScale
```csharp
public void SetVoiceVolumeScale(int emitterIndex, float scale)
```
**Description:** Scales one emitter relative to how it was authored (idempotent). `SetVoiceVolume` is deprecated and forwards here.

##### SetVoicePitchOffset / SetVoicePitchAbsolute
```csharp
public void SetVoicePitchOffset(int emitterIndex, float semitones)
public void SetVoicePitchAbsolute(int emitterIndex, float semitones)
```
**Description:** The pitch methods for one emitter. `SetVoicePitch` is obsolete and forwards to `SetVoicePitchAbsolute`.

##### StopVoice
```csharp
public void StopVoice(int emitterIndex, float fadeTime = 0.1f)
```
**Description:** Stops one emitter with fade.

##### PauseVoice / ResumeVoice
```csharp
public void PauseVoice(int emitterIndex)
public void ResumeVoice(int emitterIndex)
```
**Description:** Pauses/resumes one emitter.

---

#### Position Methods

##### UpdatePositions
```csharp
public void UpdatePositions(Vector3[] newPositions)
public void UpdatePositions(Transform[] newTransforms)
```
**Description:** Updates emitter positions, one entry per emitter. Every voice an emitter started moves with it.

##### RefreshPositions
```csharp
public void RefreshPositions()
```
**Description:** Re-reads positions from tracked transforms.

##### GetVoicePosition
```csharp
public Vector3? GetVoicePosition(int emitterIndex)
```
**Description:** Gets the position of one emitter.

**Returns:** Position, or `null` if that emitter has no live voice.

##### GetAllPositions
```csharp
public Vector3[] GetAllPositions()
```
**Description:** Gets positions of all voices, one entry per voice. An emitter whose container started several voices appears once for each of them.

---

## Parameters

Explained in USER_GUIDE Manual §3.4. A sound reads a parameter's value from its play (handle), else its emitter, else the global scope, else the parameter's default.

### AudioParameter

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Parameter

| Field | Description |
|---|---|
| `valueFrom` | Where the value comes from: `Game` (default: set by code, in three scopes), or a built-in Pepper works out per sound: `Distance` (m to the listening point), `ApproachSpeed` (m/s closing, + coming closer, eased over 50 ms), `Angle` (° on the horizontal plane: 0 ahead, + right, ±180 behind). A built-in ignores game writes (one warning per asset), clamps to the range, has no smoothing, and reads its default where there is no sound to measure. |
| `parameterName` | The name it answers to in the string API and in Blends that name a parameter. Empty = the asset's name. |
| `unitLabel` | Display only: RPM, m/s, 0-1. |
| `minValue`, `maxValue` | The range. Values are clamped when read, kept as written. One warning per asset for a write outside it. |
| `defaultValue` | Read when no scope has a value. |
| `slew` | Smoothing: how fast what sounds hear follows a written value ([ParameterSlew](#parameterslew)). Applies to writes through the asset, in every scope separately. The string API is never smoothed. |
| `bypassSlewWhile` | Optional on/off parameter: while it reads 0.5 or more in the same scope, the smoothing is skipped. |

```csharp
public string ParameterName { get; }   // parameterName, or the asset's name when empty
public string UnitLabel { get; }
public float Min { get; }              // swapped min/max are read the right way round
public float Max { get; }
public float Default { get; }          // clamped to the range
public ParameterSlew Slew { get; }
public AudioParameter BypassSlewWhile { get; }
public ParameterSource ValueFrom { get; }   // Game, Distance, ApproachSpeed, Angle
public bool IsBuiltIn { get; }             // ValueFrom isn't Game
public float Clamp(float value)
```

### AudioAttenuation

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Attenuation

How a 3D sound changes with distance from the listening point, shared between containers (USER_GUIDE §4.1, "Attenuation assets"). Set in a container's 3D Settings, where it replaces Min Distance, Max Distance and Rolloff; children take it with Uses Parent Positioning.

| Field | Description |
|---|---|
| `maxDistance` (m) | Where the curves end. Past it a voice is made virtual (with LOD on), and real again inside 95% of it. The curves' last values hold beyond it. |
| `volumeDb` | Volume in dB against metres. Default: 0 dB to 2 m, −6 dB per doubling, −60 dB at 100 m, straight lines between keys. |
| `lowPass`, `lowPassHz` | Optional low-pass cutoff in Hz against metres (octaves between keys). |
| `highPass`, `highPassHz` | Optional high-pass cutoff in Hz against metres. |
| `spread`, `spreadCurve` | Optional spread 0 (a point) to 1 (fully wide) against metres. |

```csharp
public float MaxDistance { get; }
public AnimationCurve VolumeDb { get; }
public bool UsesLowPass { get; }   public AnimationCurve LowPassHz { get; }
public bool UsesHighPass { get; }  public AnimationCurve HighPassHz { get; }
public bool UsesSpread { get; }    public AnimationCurve Spread { get; }
public bool HasFilters { get; }               // low- or high-pass on: its voices are updated every frame
public float VolumeDbAt(float metres)
public float GainAt(float metres)             // linear 0..1
public float LowPassAt(float metres)          // open (22000) when off
public float HighPassAt(float metres)         // open (10) when off
public float SpreadAt(float metres)           // 0 when off
```

**How it plays (Unity spatial mode, the default).** A 3D voice with an asset plays with `rolloffMode = Custom`, `maxDistance` = the asset's, and the volume and spread curves converted to Unity's custom curves (`AudioSource.SetCustomCurve`, over distance ÷ Max Distance; spread as angle ÷ 360, so 1 = 180°), set only when the voice's asset changes. Unity applies them while mixing, from its AudioListener; keep it on the listening point (`DriveUnityListener`). Pepper sets the low- and high-pass from each voice's own distance to the listening point every frame (eased about 20 ms), combined with Parameter Curves (lowest low-pass, highest high-pass). The voice's importance uses `GainAt(distance)`. The pool puts the source back to Logarithmic and spread 0.

### ParameterSlew

**Namespace:** `AudioSystem` (serializable struct)

```csharp
public SlewMode mode;        // None, Rate, Ease
public float upSeconds;      // Ease: seconds to close 95% of a rise (0 = at once)
public float downSeconds;    // Ease: seconds to close 95% of a fall
public float upRate;         // Rate: units per second rising (0 = at once)
public float downRate;       // Rate: units per second falling

public static ParameterSlew None { get; }
public static ParameterSlew Ease(float upSeconds, float downSeconds)
public static ParameterSlew Rate(float upRate, float downRate)
public bool IsSmoothing { get; }
public float Step(float current, float target, float dt)   // one step, for tools and tests
```

**Ease** moves `current += (target - current) * (1 - 0.05^(dt / T))`, so the gap closes by 95% every `T` seconds however the time is split into frames. **Rate** moves at most `rate * dt` per step and stops on the target. Smoothing runs in the manager's per-frame step on unscaled time. The first value a scope gets is heard at once; `Snap` skips the smoothing.

### AudioEmitter

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour` (sealed; one per GameObject)
**Menu:** Add Component > Audio System > Audio Emitter

An object that makes sounds and holds parameter values for them (one car). Sounds posted on it read its values before the global ones.

```csharp
public AudioHandle Post(AudioEvent audioEvent)    // same as audioEvent.Post(this)
public AudioHandle Post(AudioEvent audioEvent, string point)   // at a named point; same as audioEvent.Post(this, point)
public AudioHandle Post(AudioEvent audioEvent, int point)      // by number (FindPoint); -1 = the origin
public AudioHandle Post(AudioEvent audioEvent, PostValues values)                // values for that sound only
public AudioHandle Post(AudioEvent audioEvent, string point, PostValues values)
public AudioHandle Post(AudioEvent audioEvent, Vector3 localOffset, PostValues values = default)   // anywhere on it (an impact)
public AudioHandle Post(AudioEvent audioEvent, double startDsp, double startOffsetSeconds,
                        PostValues values = default)   // a moment of the sound at a DSP time (a pass-by)

// Level and pitch against the other emitters (inspector: Volume Offset, Pitch Offset), on every sound on it, now and later
public float VolumeOffsetDb { get; set; }         // dB, 0 = as authored; can't lift a sound past full volume
public float PitchOffsetCents { get; set; }       // cents, 0 = as recorded

// Random numbers (AudioManager.RandomNumbers = Pepper): this emitter's own sequence
public int RandomSeed { get; }                    // 0 = a different sequence every run
public void SetRandomSeed(int seed)               // start again from seed: the same seed replays the same picks

// Named points: offsets in this object's own space (a car's "Exhaust", "Intake", "FrontAxle")
public IReadOnlyList<EmitterPoint> Points { get; }  // EmitterPoint { string name; Vector3 offset; }
public int FindPoint(string point)                // its number, or -1 (the first, if two share a name)
public int SetPoint(string point, Vector3 offset) // add, or move an existing one; later posts only
public Vector3 GetPointOffset(int point)
public Vector3 GetPointPosition(int point)        // world space, now

// Velocity, shared by every sound on it (read by Approach Speed and Pepper's Doppler)
public Vector3 Velocity { get; }                  // measured in Pepper's per-frame step, unless given
public void SetVelocity(Vector3 metresPerSecond)  // give it yourself (a rigidbody's); stops measuring
public void ClearVelocity()                       // measure again
public void Teleport()                            // it jumped (a reset, a respawn): the jump isn't speed

// Switched off or destroyed (inspector: On Disable, Stop Fade Seconds)
public EmitterDisableMode WhenDisabled { get; set; }   // Keep (default, as before), Stop, Pause; read at each post
public float StopFadeSeconds { get; set; }             // seconds, default 0.1

public void SetParameter(AudioParameter parameter, float value)
public void ClearParameter(AudioParameter parameter)
public void RampParameter(AudioParameter parameter, float target, float seconds)
public void SnapParameter(AudioParameter parameter)
public void SnapAllParameters()
public void SetSlewOverride(AudioParameter parameter, ParameterSlew slew)
public void ClearSlewOverride(AudioParameter parameter)
public bool TryGetParameter(AudioParameter parameter, out float value)   // set here, as written
public float GetEffectiveParameter(AudioParameter parameter)             // heard now: here, else global, else default; clamped

// Switches (one car's surface); a driven group ignores them, with a warning
public void SetSwitch(AudioSwitchGroup group, string value)
public void ClearSwitch(AudioSwitchGroup group)
public bool TryGetSwitch(AudioSwitchGroup group, out string value)       // set here
public string GetEffectiveSwitch(AudioSwitchGroup group)                 // here, else global, else the Default Value

// For tools: the values on this emitter
public int ParameterCount { get; }
public string GetParameterNameAt(int index)
public float GetParameterValueAt(int index)     // as heard now (after smoothing)
public float GetParameterTargetAt(int index)    // as written
public bool IsParameterMovingAt(int index)      // ramping, smoothing or easing away
public int SwitchCount { get; }
public string GetSwitchGroupAt(int index)
public string GetSwitchValueAt(int index)

// For tools: every Audio Emitter enabled now (the Live View, a game's debug overlay). Don't change it.
public static IReadOnlyList<AudioEmitter> AllEnabled { get; }
```

- `SetParameter`: the emitter's sounds follow the value through the parameter's smoothing; the first value set here is heard at once.
- `ClearParameter`: the sounds go back to the global value, easing over if the parameter is smoothed.
- `RampParameter` moves the written value in a straight line from the one written before (or what the sounds hear, if none), on unscaled time; smoothing applies on top. A new ramp, a Set, a Snap or a Clear replaces it.
- `SnapParameter` / `SnapAllParameters`: the sounds hear the written value now (a ramp jumps to its end). For a reset or a teleport. Values set on the sounds' handles have their own `SnapAllParameters`.
- `SetSlewOverride`: smooth this emitter's value with other settings than the parameter's (a per-car attack). Affects this emitter's value only.

Values work without an AudioManager (an emitter can be set up before one exists); ramps and smoothing need one to advance.

**Named points.** A sound posted at a point is attached to the emitter at the point's offset (in its own space, so its scale applies), its later voices too (a Sequence's next clip), so it moves and turns with the object; a 2D container ignores the place. It reads the emitter's parameter values like any sound on it. An unknown name warns once per name and plays at the origin. `SetPoint` changes later posts; sounds already playing stay where they were attached. `AudioEvent.Post(AudioEmitter, string point, double scheduledDspTime = 0.0)` and `Post(AudioEmitter, int point, double scheduledDspTime)` (the DSP time has no default there: `Post(emitter, 1)` is the older overload with a DSP time) and `AudioManager.Post(audioEvent, emitter, point)` do the same. In the inspector, the points are listed and can be dragged in the Scene view while the object is selected.

**Posting at an offset** (3.1.0-dev.7). `Post(evt, localOffset, values)` is a named point anywhere: an impact at the contact point, with this emitter's values, Max Instances Per Emitter, Cooldown Per Emitter and random numbers (`AudioEvent.PostAtOffset`). A playing sound moves with `AudioHandle.SetLocalOffset` (a scrape following the contact). `Post(evt, startDsp, startOffsetSeconds, values)` is [AudioEvent's timed post](#post-a-moment-of-the-sound-at-a-dsp-time).

**Level and pitch** (3.1.0-dev.7). `VolumeOffsetDb` and `PitchOffsetCents` reach every sound on the emitter, playing or posted later, through their own gain and pitch slots (`GainStack.EmitterGain`, `PitchStack.Emitter`), and compose with a sound's own offset. Set them in the inspector or from code, also in Play mode. An AudioSource's volume stops at 1, so a raise can't lift a sound already at full level.

**Teleport** (3.1.0-dev.7). After a jump that isn't motion (a reset to the track, a respawn, a remote car placed), `Teleport()`: Pepper's next step takes the new position and measures nothing from the jump, so Approach Speed and Doppler don't spike. The last velocity stays until a new one is measured; a velocity given with `SetVelocity` is kept. It doesn't touch parameter values: call `SnapAllParameters` too if they jump.

**Switched off or destroyed.** `WhenDisabled` (`EmitterDisableMode`):
- `Keep` (default): as before 3.1. 3D sounds are attached to the object: switched off, Unity stops them with it (a loop is never reclaimed); destroyed, they are cut with a warning and Pepper rebuilds their voices.
- `Stop`: switched off or destroyed, every sound posted on it fades out over `StopFadeSeconds` where it is, with whatever it was still going to start (Delay actions, a Sequence's next clip), and goes back to the pool.
- `Pause`: switched off, its sounds fade out over about 25 ms and hold their place, keeping no real voice; switched on, they carry on from there, in phase. Destroyed, they stop with the fade.

With `Stop` or `Pause`, its sounds aren't attached: they follow the object (position and rotation, at their point), moved by Pepper every frame after every LateUpdate and before Unity mixes, so a fade can outlive the object. While it is off, posts on it return a finished handle and play nothing. The setting is read at each post; sounds already playing keep the way they were attached.

**Example:**
```csharp
AudioEmitter car = GetComponent<AudioEmitter>();
AudioHandle engine = car.Post(engineEvent);
car.SetParameter(rpm, 4200f);   // only this car's engine

car.WhenDisabled = EmitterDisableMode.Stop;   // removed from car select: its sounds fade out, not cut
```

```csharp
public enum EmitterDisableMode { Keep, Stop, Pause }
```

### AudioSwitchGroup

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Switch Group

A switch that Switch Containers read (a surface, a weather), with its values. A sound reads its handle's value, else its emitter's, else the global one, else `Default` (USER_GUIDE §4.5).

| Field | Description |
|---|---|
| `groupName` | The name it answers to in the string API (`AudioManager.SetSwitch`, state and event actions) and in Switches that name a group. Empty = the asset's name. |
| `values` | The values it can take. Switch inspectors offer them; a value set that isn't here warns once. Empty = any. |
| `defaultValue` | Read when nothing is set. Empty = none (the Switch plays its Default Container). |
| `drivenBy` | Optional Parameter asset that picks the value through `ranges`, read in each sound's scopes. Values set directly are then ignored, with one warning. |
| `ranges` | `SwitchRange { string value; float from; }`: each value from its start up to the next one's; the first also covers below. Kept sorted. |
| `hysteresis` | How far past a boundary (in the parameter's unit) the parameter must go to change the value. Each playing sound remembers its own range. |

```csharp
public string GroupName { get; }
public IReadOnlyList<string> Values { get; }
public string Default { get; }
public AudioParameter DrivenBy { get; }
public IReadOnlyList<SwitchRange> Ranges { get; }
public float Hysteresis { get; }
public bool IsDriven { get; }                  // DrivenBy set and at least one range
public bool HasValue(string value)             // in Values, or Values is empty
public string ValueFor(float parameterValue)   // the ranges, without hysteresis
```

---

## AudioContainer (Base)

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Abstract:** Yes (use specific container types)

### Properties

```csharp
public string ContainerName { get; }
public string Description { get; }
public List<string> Tags { get; }
public AudioMixerGroup MixerGroup { get; set; }
public bool Is3D { get; }
public float MinDistance { get; }
public float MaxDistance { get; set; }      // the setter writes a runtime override, never the asset
public AudioRolloffMode RolloffMode { get; }
public AudioAttenuation Attenuation { get; } // a shared distance asset (a runtime override first); replaces the three above
public float DopplerLevel { get; set; }     // Unity's Doppler on 3D voices: 1 = Unity's default, 0 = none (0–5)
public DopplerMode Doppler { get; set; }    // Unity (default: DopplerLevel) or Pepper
public float DopplerExaggeration { get; set; }  // Pepper Doppler: ×1–3
public float DopplerMaxCents { get; set; }      // Pepper Doppler: the largest shift either way (0–2400 cents)
public float PitchLimitSemitones { get; }   // 0 = no limit; see PitchStack
public bool HasOverrides { get; }
public bool UsesParentPositioning { get; set; }  // inside a Blend or Switch: take the parent's 3D settings
public bool UsesParentOutput { get; set; }       // inside a Blend or Switch: play through the parent's Mixer Group
public List<ParameterCurve> ParameterCurves { get; }  // volume, pitch and filters driven by parameters
```

`ParameterCurves`: see [ParameterCurve](#parametercurve-curveproperty). A container with curves makes its plays update every frame.

`DopplerLevel` (inspector: **Doppler Level**, in 3D Settings) is written to every 3D voice's `AudioSource.dopplerLevel` while `Doppler` is Unity; the pool puts 1 back. Unity's Doppler changes a voice's speed without Pepper seeing it, so a virtual voice can't follow it: on looping layers that must stay in step (engine layers) use Pepper's Doppler, or set 0. The Sound Builder's Doppler Level slider writes it.

`Doppler` (3.1.0-dev.7; inspector: **Doppler**, with **Doppler Exaggeration** and **Doppler Max Cents**): with `DopplerMode.Pepper`, Unity's Doppler Level is 0 on the voices and Pepper writes `PitchStack.Doppler` every step: ratio = c ÷ (c − v), c = 343 m/s, v the play's closing speed on the listening point (its Approach Speed: the emitter's velocity, or the play's measured motion, against the point's, eased over 50 ms), in cents × `DopplerExaggeration`, clamped to ± `DopplerMaxCents`. A play using it joins the per-frame pass. 50 m/s relative swings about 5.1 semitones at ×1. It is applied after the Pitch Limit and kept in a virtual voice's place. See [DopplerMath](#dopplermath).

`UsesParentPositioning` and `UsesParentOutput` (inspector: **Parent Settings**) are off by default. When on, a container played inside a Blend or Switch uses that parent's 3D settings (3D on or off, Min/Max Distance, Rolloff, Doppler Level, Doppler and its two settings) or Mixer Group instead of its own; if the parent passes the question on too, it goes up the play's tree until a container answers it. A container's own occlusion, reverb and propagation options always apply. A Blend's or Switch's own settings otherwise never reach its children.

#### Routing / Slot Pool Opt-Ins

```csharp
public bool UseOcclusion { get; }          // raycast occlusion
public bool UsePropagation { get; }        // propagation cutoff (composes with raycast via min-wins)
public bool AllowReverbSend { get; }       // per-zone reverb sends
public float ReverbSendLevelDb { get; }    // per-container reverb send level (dB)
public bool StaticEmitter { get; }         // hint: this voice doesn't move during playback
public ReverbSendBus ExplicitReverbBus { get; }  // override zone-driven send routing
```

Setting any of `UseOcclusion`, `UsePropagation`, or `AllowReverbSend` to true causes the container to acquire an [OcclusionSlot](#occlusionslot) at play time. The slot carries the low-pass and the reverb sends; occlusion's and propagation's volume drop is written to the voice and applies even when no slot is available. See [Manual chapter 14](USER_GUIDE.md#14-occlusion-mixer-slot-pool) for the slot pool architecture and chapter 15 for per-zone reverb sends.

`StaticEmitter` and `ExplicitReverbBus` are modifiers, not feature flags — they refine the behavior of the reverb-send driver but don't on their own cause a slot to be acquired.

---

### Methods

#### Play
```csharp
public abstract AudioVoice Play(Vector3 position = default, GameObject parent = null)
public AudioVoice Play(Vector3 position, GameObject parent, List<AudioVoice> spawned)
```
**Description:** Plays the container. Containers that start several voices at once (a Routing container with more than one clip, every layer of a Blend, whatever a Switch selects) start all of them in both overloads. The first overload returns only the **primary** (first) voice. The second also appends **every** voice it started to `spawned`, primary first. Use the second when you need to control the whole sound. It also sets `AudioVoice.RootContainer` to this container on each of those voices.

**Returns:** The primary `AudioVoice`, or `null` if nothing played.

**Custom containers:** override the single-voice `Play`. If your container starts several voices per play, also override `protected virtual AudioVoice PlayInto(List<AudioVoice> spawned, Vector3 position, GameObject parent)`, and implement the single-voice `Play` as `Play(position, parent, new List<AudioVoice>())`. Override `EstimateVoiceCount` too (below).

#### EstimateVoiceCount
```csharp
public virtual int EstimateVoiceCount()
```
**Description:** How many voices one play of this container starts at once, at most. An event makes room for this many before it plays the container, and skips the play without stealing anything if it can't. 1 by default (a Sequence plays one clip at a time); a Routing container counts its entries with a clip or an address, a Blend adds up its layers' estimates, a Switch takes the largest of its choices. A play that starts more voices than its estimate still gets them, each stealing a whole play if the pool is full.

**One play, one start time.** All the voices a play starts are scheduled on one DSP time: one buffer ahead for a single voice, two for a play of several (so a walk through many layers can't straddle a buffer), or the post's explicit DSP time. A voice a container starts later (a Sequence's next clip) starts one buffer ahead of when it starts.

#### Stop / StopImmediate
```csharp
public abstract void Stop()
public abstract void StopImmediate()
```
**Description:** Stops **every play** of this container asset (with or without fade), including paused and virtualized voices. A SequenceContainer also ends its auto-advance run. Only voices the container still owns are stopped; a voice that has since been reused by another sound is left alone. To stop **one** play, stop its `AudioHandle`.

**Concurrent plays:** every container can be playing several times at once. BlendContainer used to stop its previous play on every post; it no longer does (set Max Instances 1 on the event to restart instead). A SequenceContainer keeps one position and one auto-advance run per asset; see USER_GUIDE §4.4.

#### Runtime Override Methods
```csharp
public void SetOverride(ContainerOverrides overrides)
public void SetOverride3D(bool value)
public void ClearOverrides()
```
**Description:** Apply or clear non-serialized runtime 3D setting overrides. These do not persist to the asset.

**Example:**
```csharp
// Override 3D settings at runtime
var overrides = new AudioContainer.ContainerOverrides
{
    is3D = true,
    minDistance = 2f,
    maxDistance = 30f
};
myContainer.SetOverride(overrides);

// Or just toggle 3D
myContainer.SetOverride3D(true);

// Clear all overrides
myContainer.ClearOverrides();
```

#### Voice Query Methods
```csharp
public AudioSource GetFirstActiveSource()
public bool HasActiveVoices()
public int GetActiveVoiceCount()
```
**Description:** Query the state of voices currently playing from this container.

---

### ContainerOverrides (Nested Struct)

```csharp
public struct ContainerOverrides
{
    public bool? is3D;
    public float? minDistance;
    public float? maxDistance;
    public AudioRolloffMode? rolloffMode;
    public AudioAttenuation attenuation;   // in place of the container's own (null = no override)
    public void Clear();
}
```
Overrides are never saved to the asset. `MaxDistance`'s setter writes `maxDistance` here.

---

## Container Types

### RoutingContainer

**Menu:** Create > Audio System > Routing Container

#### Properties

```csharp
public List<AudioClip> AudioClips { get; }
public float Volume { get; }
public bool Loop { get; }
```

**Description:** Plays all assigned clips simultaneously. Use for layered sounds (e.g., explosion = blast + debris + shockwave).

**Factory Method (Editor Only):**
```csharp
public static RoutingContainer CreateNew(string name, AudioClip clip, float vol = 1f, bool looping = false, bool is3D = false)
```

---

### RandomContainer

**Menu:** Create > Audio System > Random Container

#### Properties

```csharp
public List<WeightedAudioClip> AudioClips { get; }
public int AvoidRepeatLast { get; }
public bool UseWeighting { get; }
public float Volume { get; }
public bool Loop { get; }
```

#### Methods

```csharp
public void ClearHistory()
```
**Description:** Clears the repeat-avoidance history: the shared one and every emitter's.

**Avoid Repeat Last** keeps one history per [AudioEmitter](#audioemitter) the container is posted on, so a car never repeats its own last clips whatever other cars played, and one shared history for posts without one. A networked post (inside an `AudioRandomScope`) ignores and doesn't record it. Picks draw from Pepper's random numbers (see `AudioManager.RandomNumbers`) and allocate nothing. At or above the clip count it does nothing (every clip stays available); `OnValidate` warns about it from two clips up.

**Description:** Randomly selects one clip per play, with weighting and repeat avoidance.

---

### SequenceContainer

**Menu:** Create > Audio System > Sequence Container

#### Properties

```csharp
public List<SequenceEntry> Entries { get; }
public SequenceContainer.PlaybackMode SequencePlaybackMode { get; }
public bool LoopSequence { get; }
public bool AutoAdvance { get; set; }
public float Volume { get; }
```

#### PlaybackMode (Nested Enum)

```csharp
public enum PlaybackMode
{
    Forward,   // 0->1->2->3->0...
    Reverse,   // 3->2->1->0->3...
    PingPong,  // 0->1->2->3->2->1->0...
    Random     // Random order
}
```

#### Methods

```csharp
public void ResetSequence()
public void SetIndex(int index)
public AudioVoice PlayNext(Vector3 position = default, GameObject parent = null)
```

**Description:** Plays clips in sequence. `PlayNext()` advances the sequence and plays the next clip. `AutoAdvance` controls whether the sequence auto-advances after each clip finishes.

---

### SwitchContainer

**Menu:** Create > Audio System > Switch Container

#### Properties

```csharp
public AudioSwitchGroup SwitchGroup { get; }       // the group asset, or null (then switchGroupName)
public string SwitchGroupName { get; }             // the asset's GroupName, or the name field
public List<SwitchEntry> SwitchEntries { get; }
public AudioContainer DefaultContainer { get; }
public SwitchChangeMode WhenSwitchChanges { get; } // NextPlayOnly (default), Crossfade, Restart
public float CrossfadeTime { get; }                // Crossfade only, seconds (unscaled); default 0.15
public float Volume { get; }
```

#### Methods

```csharp
public void SetSwitch(string switchValue)          // sets the group's GLOBAL value (also before the first play)
public string GetCurrentSwitch()                   // the value the last play of this asset picked
public AudioContainer GetActiveContainer()         // the container the last play of this asset picked
```

**Description:** Selects a child container by the switch group's value in the play's scopes (handle, emitter, global, the group's Default). The child plays as part of the same play, under its own node in the play's tree: a Blend under it reads its parameter in that play's scopes, and a Sequence under it reports later clips to the same handle. Every voice the child starts, later ones included, gets the Switch's Volume.

**When the value changes while it plays** (`WhenSwitchChanges`): `NextPlayOnly` keeps the choice (the next play picks the new value). `Crossfade` and `Restart` put the play in the per-frame pass, which reads the value every frame. On a change, the new choice plays from its start under a new node of the same play, as late voices of the handle (no second `OnStarted`); with Crossfade both fade along equal-power curves in `GainStack.SwitchGain`, with Restart the old one fades out as a steal does (the Audio Manager's Steal Fade Seconds, 0.025 s by default and at least three frames; 0 cuts it a few ms ahead) and the new one starts at full level. A choice still fading out that is chosen again fades back up. A choice faded to silence stops two DSP buffers later, and nothing more starts in it (a Sequence there stands down). Room is made for the new voices without stealing this play; if the pool can't, the old voices stop at once and the new ones take them. A virtual play's new voices start virtual. A paused play doesn't follow until resumed. A value with nothing to play fades the play to silence and holds it alive. A Switch only follows if its first choice played something.

---

### BlendContainer

**Menu:** Create > Audio System > Blend Container
**Implements:** `IRTPCListener`

#### Properties

```csharp
public List<BlendEntry> BlendEntries { get; }
public AudioParameter BlendParameter { get; }   // wins over the name when set
public string BlendParameterName { get; }       // the asset's ParameterName when one is set
public float Volume { get; }
public BlendLoopMode LoopMode { get; set; }           // FromLayers (default), ForceLoop, ForceOneShot
public SilentLayerBehaviour SilentLayers { get; set; } // KeepPlaying (default) or Virtualize
public float SilentThresholdDb { get; set; }          // Virtualize: a layer below this goes virtual (-60; -90..-20)
public const float SilentHysteresisDb = 6f;           // ...and comes back this far above it
[Obsolete] public bool Loop { get; }                  // never read; see LoopMode
public BlendLayout Layout { get; set; }               // VolumeCurves (default) or Regions
public CrossfadeShape Crossfade { get; set; }         // Regions: Linear, EqualPower (default), SCurve
public AudioParameter PitchFollowParameter { get; set; } // each layer plays at value ÷ its rootValue
public float AutoLayoutFade { get; }                  // share of each gap between roots a crossfade covers
public float TuningWarningSemitones { get; }          // the inspector's tuning check threshold
public IReadOnlyList<AudioVoice> LayerVoices { get; }
```

#### Methods

```csharp
public float GetCurrentBlendValue()             // what a play with no emitter or handle value reads
public void AutoLayout()                        // regions from the entries' rootValue (Regions layout)
[Obsolete] public void UpdateBlend(float blendValue)   // sets the parameter's global value
public void OnRTPCChanged(string parameter, float value)
```

**Layouts.** `VolumeCurves`: each entry's `volumeCurve` sets its level (0–1) against the Blend's parameter, as before. `Regions`: each entry plays between `regionMin` and `regionMax`, fading in over `fadeInWidth` and out over `fadeOutWidth` in the `Crossfade` shape; `AutoLayout` places the regions half way between neighbouring entries' `rootValue`s, each crossfade covering `AutoLayoutFade` of the gap, the lowest reaching the parameter's Min and the highest its Max. Equal Power keeps the summed power at 1 (unrelated recordings); Linear and S-Curve keep the summed level at 1.

**Pitch follow.** With `PitchFollowParameter` set, each entry with `followPitch` and a `rootValue` plays at value ÷ `rootValue`, clamped to ±`followRangeSemitones`, in the pitch stack's `Follow` slot. Entries' `curves` apply to that layer only.

**Loop Mode.** `FromLayers`: each layer loops as its own container says (as before). `ForceLoop` / `ForceOneShot`: every layer's voices loop / don't, later voices included. A nested Blend left at `FromLayers` takes the outer one's. The old `loop` field is hidden and never read; Sound Builder (Simple) writes `LoopMode` (Loop ticked = `ForceLoop`, unticked = `FromLayers`).

**Silent layers.** `Virtualize`: each frame, a layer voice whose own level (`BaseGain × MultiplierGain × RtpcGain × CurveGain`: its volume, the crossfade and curves; not bus, occlusion, fades or crossfades) is below `SilentThresholdDb` is made virtual and held so while it stays below `SilentThresholdDb + SilentHysteresisDb`; it keeps its place in the clip and comes back in step. A layer silent at the start starts virtual. It is never stolen on its own: steals take whole plays. A nested Blend left at `KeepPlaying` takes the outer one's.

**One start time.** Every voice of a play is scheduled on the play's start time: two DSP buffers ahead for a play of several voices (one for a single voice), or the post's explicit DSP time.

**Description:** Plays multiple containers, each layer at a level set by its curve against one parameter. **Each play reads the parameter in its own scopes** (handle, emitter, global, default), so two copies posted on two emitters blend independently. The layers follow the value once a frame, in Pepper's per-frame step (see `AudioManager.UpdateMode`). With a `BlendParameter` asset, the curves' X axis is in its unit and values are clamped to its range; with only a name, values are clamped to 0–1. Any container can be a layer, Blends and Switches included: each nested Blend reads its own parameter in the same play's scopes, and a layer under several Blends gets the product of their curves. Voices a layer starts later (a Sequence's next clip) get the curves and the Blend's Volume too. Only plays with a Blend in their tree are evaluated each frame.

The Blend no longer registers as an RTPC listener. `OnRTPCChanged` is kept for code that calls it: with the Blend's parameter name it sets that parameter's global value. `UpdateBlend` is obsolete and does the same.

---

### GeneratorContainer (Unity 6000.3 and later)

**Namespace:** `AudioSystem.Generators` · **Assembly:** `SFXSystem.Generators`, which compiles only on Unity 6000.3 and
later (its `SFXSYSTEM_GENERATORS` define comes from the editor version). On earlier versions nothing here exists.
**Implements:** `UnityEngine.Audio.IAudioGenerator`

An abstract container whose sound is computed while it plays instead of read from a clip: the container is itself the
Unity 6.3 generator its voice's AudioSource plays. Pepper's engine generators (the granular engine and the additive
layer, in development) derive from it; so can your own.

```csharp
public float Volume { get; }
public abstract int ParameterCount { get; }                 // at most GeneratorValues.Capacity (8)
public abstract AudioParameter GetParameter(int index);     // value i of what each voice is sent; null reads 0
protected abstract GeneratorInstance CreateGenerator(ControlContext context, AudioFormat? nestedFormat,
                                                     ProcessorInstance.CreationParameters creationParameters);
```

**How it plays.** A voice from Pepper's pool, like any container, so buses, volume, the pitch stack, Doppler, Pepper's
panning, the voice filters, stealing, pause, virtual voices and Blend nesting all apply. The AudioSource's `loop` is
on: it plays until it is stopped (`EstimateDuration` reports it as a loop). It reports `isRealtime` false, so the
AudioSource's pitch resamples it. A virtual voice is paused: it isn't processed, and it carries on from where it was.
When the voice goes back to the pool its generator is cleared.

**Values.** Every step, after the plays are evaluated, each playing voice is sent a `GeneratorValues` message:
`GetParameter(i)` read in that voice's own play (its handle, then its emitter, then the global value, then the
default; after smoothing; clamped; a built-in from the play's place). Two cars on one engine asset each get their own
RPM. A paused voice is sent nothing until it resumes. A value is heard about one DSP buffer after it is sent. Each
message also carries `Seconds`, the unscaled time of the step that sent it.

**Writing a generator.** Allocate an unmanaged real-time struct with Pepper's control half:

```csharp
protected override GeneratorInstance CreateGenerator(ControlContext context, AudioFormat? nestedFormat,
                                                     ProcessorInstance.CreationParameters creationParameters)
    => context.AllocateGenerator(new MyRealtime(), new GeneratorControl<MyRealtime>(), nestedFormat, creationParameters);
```

- **`IGeneratorRealtime`** (extends Unity's `GeneratorInstance.IRealtime`): `Configure(int sampleRate)` before the first
  buffer, `Dispose()` when the instance goes, plus Unity's `Process` (fill the buffer, mono) and `Update`. In `Update`,
  `GeneratorValues.Receive(context, pipe, ref latest, out float seconds)` returns true when new values have arrived,
  with the time all the steps that sent them covered (a buffer often brings two frames' values, or none). Move from the
  old values to the new ones over that time, plus what was left of the last move, not across one buffer: frames and
  buffers don't line up, so a ramp per buffer moves a value the game changes every frame in uneven steps. Pepper's
  engines do this. Allocate nothing managed on this side.
- **`GeneratorControl<TRealtime>`**: sets the output to mono at the system's rate and passes each `GeneratorValues`
  straight down to the real-time half.
- **`GeneratorValues`**: `Count`, an indexer, `Capacity` = 8, `Seconds` (the step's unscaled time; after
  `3.1.0-dev.11`), and `Receive` (with or without `seconds`).

#### GrainTable

One engine sweep cut into grains, one engine cycle (two turns, 120 ÷ RPM seconds) each, for the granular player. Made
by the Engine Grains window, not by hand. It keeps a reference to the sweep clip (which must be Decompress On Load) and,
per grain, where it starts and how long it is (frames at the clip's rate) and the RPM its length means.

```csharp
public AudioClip Sweep { get; }
public IReadOnlyList<GrainTable.Grain> Grains { get; }   // Start, Length, Rpm; in the sweep's order
public int PeriodsPerCycle { get; }                       // periods of the fundamental in one cycle
public float MinRpm { get; }
public float MaxRpm { get; }
public GrainTable.RpmSource Source { get; }               // Known (typed) or FromPitch
public float FromRpm { get; }
public float ToRpm { get; }
public GrainTable.RpmCurve Curve { get; }                 // Known: StraightLine or EvenPitch
public int Cylinders { get; }
```

`RpmCurve` is how a typed RPM moves between From and To: **StraightLine** (the same RPM every second) or **EvenPitch**
(the same number of semitones every second, as most synthesisers sweep: the RPM multiplies by the same factor).

#### GranularEngine

**Menu:** Create > Audio System > Granular Engine (Unity 6.3+) · **Extends:** GeneratorContainer

A car engine played grain by grain from Grain Tables. At the end of each grain it takes the grain whose RPM is nearest
the car's, picked at random among up to Variation grains either side that are within half a semitone (never the same
one twice in a row), and resamples it to land exactly on the RPM, sample by sample by the RPM the sweep had there (a
line through the grain's RPM at its middle, at the pace its neighbours climb), so a held RPM holds its pitch through
the grain. The RPM and load move to each step's values over that step's time. Each grain fades into the next over
Crossfade Ms with gains that sum to 1 (the grains are cut in phase). The on-load and off-load sweeps blend by load at equal power;
the idle loop takes over below the sweeps' lowest RPM, at equal power from Idle Rpm. The AudioSource's pitch (Doppler,
pitch offsets) applies on top.

```csharp
public AudioParameter Rpm { get; }        // value 0: the car's RPM
public AudioParameter Load { get; }       // value 1: 0 off load to 1 on load; null = on load only
public GrainTable OnLoad { get; }         // the climbing sweep
public GrainTable OffLoad { get; }        // optional: the falling sweep
public AudioClip Idle { get; }            // optional: an idle loop (Decompress On Load)
public float IdleRpm { get; }             // the RPM the idle loop was recorded at (900)
public float CrossfadeMs { get; }         // 1–10, default 3
public int Variation { get; }             // 0–8, default 2
```

The sweeps' and idle's samples are loaded into memory when a voice starts and stay loaded while the engine asset is;
they are read again after a new analysis. With no Grain Table (and no idle) a post plays nothing and warns once.

#### AdditiveEngine

**Menu:** Create > Audio System > Additive Engine (Unity 6.3+) · **Extends:** GeneratorContainer

An engine's firing orders as sine tones, for weight under a loop engine or a granular one (as a layer in a Blend); not a
whole engine. Order 1 is the firing frequency, RPM × Cylinders ÷ 120. Each order's level moves from its off-load to its
on-load level with the load. A little noise, low-passed at Noise Brightness times the firing frequency, takes the edge
off. Orders at 45% of the output's sample rate or above are left out; at RPM 0 it is silent.

```csharp
public AudioParameter Rpm { get; }                       // value 0
public AudioParameter Load { get; }                      // value 1; null = on load
public int Cylinders { get; }                            // 1–16: the scale of order 1
public IReadOnlyList<AdditiveEngine.Order> Orders { get; } // order, offLoad, onLoad (0–1); the first 16 play
public float NoiseOffLoad { get; }                       // the noise's RMS off load
public float NoiseOnLoad { get; }                        // and on load
public float NoiseBrightness { get; }                    // 1–32 × the firing frequency
public const int MaxOrders = 16;
```

#### GeneratorCost

What Pepper's generators cost on the mixing thread, measured where they run, a build included (for the Steam Deck).
While `Measuring` is off, each generator buffer only checks a flag.

```csharp
public static bool Measuring { get; set; }              // switching it on starts a new measurement
public static long Buffers { get; }                     // buffers measured, all generators together
public static double Milliseconds { get; }              // their time on the mixing thread
public static double MicrosecondsPerBuffer { get; }
public static double Seconds { get; }                   // since the measurement started
public static double CoreSharePerInstance(int instances) // 0–1
public static string Report(int instances)              // one line for the log
public static void Reset()
```

Generator containers also show in the tools: the Audition gives their parameters sliders and plays them outside Play
mode, the Container Validator checks them (and counts them as loops for its Persistent note), and the debug windows
name a generator voice "(generated)".

---

## AudioBus

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

```csharp
public string BusName { get; }
public AudioBus ParentBus { get; }
public AudioMixerGroup MixerGroup { get; }
public float VolumeDb { get; }                        // the asset's Volume (SetVolume)
public float VolumeMultiplier { get; }
public float StateOffsetDb { get; }                   // what Offset states add now (runtime only)
public float PlayerVolumeDb { get; set; }             // the player's volume, -80…+24 dB (runtime only)
public bool Mute { get; }
public bool IsMuted { get; }                          // this bus or a parent is muted
public bool Solo { get; }
public IReadOnlyList<EffectSend> Sends { get; }
public bool EnableDucking { get; }
public IReadOnlyList<DuckingTarget> DuckingTargets { get; }
public float DuckingAttack { get; }
public float DuckingRelease { get; }
public AudioTimeMode DuckingTimeMode { get; set; }    // the clock a duck on this bus runs on: Scaled (default) or Unscaled
public BusPauseBehaviour WhilePaused { get; set; }    // a sound posted while paused: DropOneShots (default) or Hold
public bool IsPaused { get; }                         // this bus or a parent is paused
public List<ParameterCurve> ParameterCurves { get; }   // Volume (dB), Low Pass and High Pass (Hz); global scope
public string LowPassParameter { get; }               // the exposed mixer cutoff Low Pass curves write
public string HighPassParameter { get; }              // the exposed mixer cutoff High Pass curves write
public float CurveVolumeGain { get; }                 // the curves' level now, linear (part of GetFinalVolume)
public float LowPassHz { get; }                       // the low-pass cutoff Pepper sets now (22000 = open)
public float HighPassHz { get; }                      // the high-pass cutoff Pepper sets now (10 = open)
public int MaxRealVoices { get; set; }                // its real-voice cap, 0 = none (3.1.0-dev.8)
public int RealVoiceCount { get; }                    // real voices of its sounds and its child buses' now
```

**Max Real Voices** (3.1.0-dev.8, USER_GUIDE §6.9; inspector: **Voices > Max Real Voices**): the most real voices the sounds whose Target Bus is this bus or one of its children may use at once; 0 (the default) = no cap. Over it, the least important of them go virtual, a whole sound at a time, at a sound's start and in the voice pass, like the Audio Manager's Max Real Voices, which still applies; a single sound bigger than the cap keeps its most important voices. It never ends a sound.

**Parameter Curves** (USER_GUIDE §6.6) are evaluated in Pepper's per-frame step from each parameter's global value, after smoothing. Volume multiplies into `GetFinalVolume` (child buses and every assigned voice's `BusGain` follow; the serialized `VolumeDb` is untouched). Low Pass and High Pass ease in octaves over about 20 ms and are written with `AudioMixer.SetFloat` to the bus's Mixer Group under `LowPassParameter` / `HighPassParameter` when they move; with no Mixer Group or no such exposed parameter the bus logs one warning and stops trying. Pitch curves are ignored. The bus inspector's **Add Low Pass / High Pass to Mixer Group** buttons add a Lowpass Simple / Highpass Simple effect and expose its `Cutoff freq`.

---

### Methods

#### Volume & Mute
```csharp
public void SetVolume(float newVolumeDb)
public float GetFinalVolume()
public void SetMute(bool newMute)
public void SetSolo(bool newSolo)
```
**Description:** `GetFinalVolume()` returns the final linear volume including the entire parent bus hierarchy: `VolumeDb + StateOffsetDb + PlayerVolumeDb` as a gain, times `VolumeMultiplier`, the ducking and the curves, times the parent's. It is 0 when this bus or a parent is muted, and while another bus is soloed unless this bus or one of its parents is soloed too. That includes a parent of a soloed bus: it passes the soloed bus through, but its own `GetFinalVolume()` is 0, so the sounds posted straight to it are silent. `SetVolume` writes the asset's Volume (in the editor a Play-mode change stays in the asset).

`SetSolo` works as on a mixing desk: while any bus is soloed, the soloed buses and their child buses are heard, through their parents (whose volume, mute and ducking still apply), and every other bus is silenced. **Sounds posted straight to a parent of a soloed bus are silenced too**; solo the parent to hear them. Mute still wins. A solo change reaches every bus's voices, paused and virtual ones too. Before 3.1.0 (after `3.1.0-dev.12`) a bus was heard only if it was soloed itself, so soloing a child bus silenced it too and soloing a parent silenced its children.

**Three levels** (3.1.0-dev.7, USER_GUIDE §6.8 and §7): **Volume** is the asset's (the mix); **StateOffsetDb** is what the active Offset states give the bus, moved by `AudioManager.SetState` / `ClearState`; **PlayerVolumeDb** is a Settings page's, set by the game. The last two are runtime only: never written to the asset, 0 at the start of each play session. A state never undoes the player's volume, nor the player a state.

#### Voice Management
```csharp
public void AssignToVoice(AudioVoice voice)
public void RemoveVoice(AudioVoice voice)
public AudioSource GetFirstActiveSource()
public void GetActiveSources(List<AudioSource> outList)
public void GetActiveSources(List<AudioSource> outList, bool includeChildBuses)
```
**Description:** `GetActiveSources` lists the AudioSources of the sounds playing now whose Target Bus is this bus (the list is cleared first). With `includeChildBuses: true` (3.1.0, after dev.12) it also lists the sounds on its child buses, at any depth: everything the bus carries. The Mixer window's meters use that one.

#### Ducking
```csharp
public void TriggerDucking(float holdDuration = 0f)
public void ApplyDucking(float amountDb, float attack, float hold, float release)
```
**Description:** `TriggerDucking()` uses the bus's configured ducking settings. `ApplyDucking()` applies custom ducking parameters (`amountDb` is negative dB; times in seconds). Ducking is triggered, not driven by the bus's output. **The deepest duck wins** (since 3.1.0-dev.7): each trigger is a duck of its own, and while several run on a bus it follows the deepest at each moment, so none cuts another short (it used to be one envelope per bus, replaced by each trigger). A new duck starts from the level the bus is at, so re-triggering one duck sounds as it always did. Each duck runs on the ducked bus's `DuckingTimeMode` as it was when triggered: Scaled (the default) freezes at time scale 0; Unscaled completes during a pause. At most 16 run at once on a bus (the one ending first gives way).

#### Pause / Resume / IsPaused
```csharp
public void Pause()
public void Resume()
public bool IsPaused { get; }
public BusPauseBehaviour WhilePaused { get; set; }

public enum BusPauseBehaviour { DropOneShots, Hold }
```
**Description:** For a game's pause: pause the world's buses and the pause menu's buses play on. `Pause()` holds every sound on this bus and its child buses: each fades out over about 25 ms and keeps its place, holding no real voice. `Resume()` brings them back from there, fading in, every layer of a sound in the same frame so they stay in phase. A child bus is paused while it or a parent is (`IsPaused`), and `Resume()` on a child doesn't override its parent. A sound posted to a paused bus starts held; with `DropOneShots` (the default) one whose voices don't loop is dropped before its first sample and its handle finishes at once, with `Hold` it plays on resume. Separate from `AudioHandle.Pause` and `AudioManager.PauseAll` (which still pause at once): a sound plays only when none of them pauses it. Pausing a paused bus, or resuming one that isn't, does nothing. Not paused at the start of each play session.

#### Factory Methods (Editor Only)
```csharp
public static AudioBus CreateNew(string name, AudioBus parent = null, AudioMixerGroup mixer = null)
public void EditorSetConfiguration(string name, AudioBus parent = null, AudioMixerGroup mixer = null)
```

---

### EffectSend (Nested Class)

```csharp
[Serializable]
public class EffectSend
{
    public string sendName;
    public float sendLevel;
    public bool prePost;
}
```

### DuckingTarget (Nested Class)

```csharp
[Serializable]
public class DuckingTarget
{
    public AudioBus targetBus;
    public float duckAmount = -6f;   // attenuation in dB, -80..0
}
```

---

## AudioState

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

```csharp
public string StateName { get; }
public string StateGroup { get; }
public StateVolumeMode VolumeMode { get; set; }       // SetLevel (default, as before) or Offset
public IReadOnlyList<BusVolumeProperty> BusVolumes { get; }
public IReadOnlyList<SwitchProperty> SwitchValues { get; }
public IReadOnlyList<RTPCProperty> RtpcValues { get; }
public IReadOnlyList<EffectProperty> EffectProperties { get; }

public enum StateVolumeMode { SetLevel, Offset }
```

**VolumeMode** (3.1.0-dev.7; inspector: **Bus Volume Mode**). `SetLevel`: each bus is set to its `volumeDb` (`AudioBus.SetVolume`, the asset's Volume), and leaving the state puts nothing back, as before. `Offset`: each bus moves by its `volumeDb` in `AudioBus.StateOffsetDb`, on top of its Volume and the player's volume, never touching the asset; when another state in the group is set, or `AudioManager.ClearState` clears the group, the offsets go back over that transition time. Offset states in several groups add up on one bus.

---

### Methods

#### Apply
```csharp
public void Apply(float transitionTime)
```
**Description:** Applies this state's properties (bus volumes, RTPCs, switches, effects). An `Offset` state's bus volumes are left to `AudioManager.SetState`, which knows which states are active.

---

### Nested Property Classes

```csharp
[Serializable]
public class BusVolumeProperty
{
    public AudioBus bus;
    public float volumeDb;
}

[Serializable]
public class SwitchProperty
{
    public string switchGroup;
    public string switchValue;
}

[Serializable]
public class RTPCProperty
{
    public string parameterName;
    public float value;
}

[Serializable]
public class EffectProperty
{
    public AudioBus bus;
    public string propertyName;
    public float value;
}
```

---

## AudioVoice & AudioVoiceEnhanced

### AudioVoice (Base)

**Namespace:** `AudioSystem`
**Description:** Base voice representation.

#### Properties

```csharp
public AudioSource source { get; }
public bool isPlaying { get; }
public float volumeMultiplier { get; set; }
```

#### Methods

```csharp
public void SetVolume(float volume)
public void UpdateMasterVolume(float masterVolume)
public void Stop(float fadeTime = 0f)
public void StopImmediate()
```

---

### AudioVoiceEnhanced

**Namespace:** `AudioSystem`
**Inheritance:** `AudioVoice`
**Description:** Enhanced voice with gain stack, virtualization, and scheduling. **Not typically accessed directly.**

#### Properties

```csharp
public GainStack GainStack { get; }
public AudioEvent SourceEvent { get; }
public AudioContainer Container { get; }        // the innermost container that configured this voice (its
                                                // occlusion/reverb/propagation flags drive it)
public AudioContainer RootContainer { get; }    // the container that was played (e.g. the Blend around a layer)
public AudioBus Bus { get; }
public int Priority { get; }
public bool IsVirtual { get; }
public float VirtualTime { get; }               // where a virtual voice is in its clip now, in seconds
public double VirtualSamples { get; }           // the same in the clip's samples; written back on MakeReal
public double ScheduledStartTime { get; }
public double ScheduledEndTime { get; }
public OcclusionSlot OcclusionSlot { get; }     // mixer slot held while playing (null when not occluding)
```

#### Methods

```csharp
public GainStack GainStack { get; }
public PitchStack PitchStack { get; }
public void UpdateFinalVolume()
public void UpdateFinalPitch()    // writes PitchStack.GetFinal(container's Pitch Limit) to source.pitch
public void MakeVirtual()         // from your code: held virtual until MakeReal (the budget won't undo it);
                                  // an audible voice fades out over ~25 ms, then its source pauses
public void MakeReal()            // seeks to VirtualSamples while paused, resumes, fades in over ~25 ms;
                                  // over Max Real Voices, the least important other sounds go virtual
public void UpdateVirtualTime(float deltaTime)   // advance by deltaTime at the voice's pitch; only for a
                                                 // voice no AudioManager steps. No-op while paused; wraps
public float GetImportance()      // distance × GainStack.GetGainWithoutRealize() × priority
```

**Virtual position.** While a voice is virtual, the AudioManager's per-frame step moves `VirtualSamples` on by the audio clock's advance (`AudioSettings.dspTime`) × the voice's pitch (`source.pitch`, which the pitch stack writes), and wraps it for a loop. A pitch change counts from the next step, when Unity first plays it. Paused voices and starts still scheduled ahead don't advance. A virtual one-shot ends once its position passes the end of its clip. `AudioHandle.SeekTo` on a virtual voice moves the position it comes back at. Unity's own Doppler isn't seen; see `AudioContainer.DopplerLevel`.

**The fade.** `MakeVirtual` on a voice that can be heard (playing, started, not muted, above −80 dB) fades `GainStack.RealizeGain` to 0 over about 25 ms, in at least three steps of at most one per mixed DSP buffer (two volume changes in one buffer can leave Unity's own short ramp unfinished), then pauses the source; any other voice pauses at once. `MakeReal` fades it back to 1 the same way. A pause during the fade out finishes it at once. A `Stop(fadeTime)` during it lets it finish and then ends the voice. A voice with no AudioManager has no fade.

---

### GainStack (Nested Class)

```csharp
public class GainStack
{
    public float BaseGain { get; set; }       // Container/Event volume
    public float BusGain { get; set; }        // Bus hierarchy
    public float OcclusionGain { get; set; }  // Occlusion attenuation
    public float RtpcGain { get; set; }       // Blend curves. RESERVED: sole writer is the Blend's
                                              // per-frame step (product of every enclosing Blend's curve).
                                              // No second writer — see the reservation note in GainStack.cs.
    public float SchedulerGain { get; set; }  // Crossfade/transition
    public float FadeGain { get; set; }       // Stop(fadeTime) ramp, 1 → 0. RESERVED: sole writer is
                                              // AudioVoice's fade-out. Reset to 1 at the pool boundary.
    public float MultiplierGain { get; set; } // Per-voice multiplicative — volume randomization,
                                              // SwitchContainer/BlendContainer outer Volume,
                                              // and AudioExtensions.PlayWithVolume write here.
    public float UserGain { get; set; }       // AudioHandle.SetVolumeScale / SetVolumeOffset — idempotent runtime scale,
                                              // given to every voice of the handle's plays, later ones too.
    public float EmitterGain { get; set; }    // the Audio Emitter's Volume Offset; sole writer: the manager.
    public float DuckingGain { get; set; }    // (Deprecated) ducking is applied at the bus, not the voice.
                                              // Settable for back-compat but NOT included in GetFinalGain.

    public float CurveGain { get; set; }      // volume curves (ParameterCurve), summed in dB; sole writer: the play tree
    public float RealizeGain { get; set; }    // the fade around virtualization, 0 while parked; sole writer:
                                              // AudioVoice's virtualization. Reset to 1 at the pool boundary.
    public float SwitchGain { get; set; }     // a playing Switch's crossfade (equal power); sole writer: the play
                                              // tree. Left out of the silent-layer check. Reset to 1 at the pool boundary.
    public float SpatialGain { get; set; }    // Spatial Mode = Pepper: the level from the listening points (largest
                                              // weight × attenuation, less what Spread takes); 1 in Unity mode. Sole
                                              // writer: Pepper's spatial step (3.1.0-dev.8). Reset to 1 at the pool boundary.
    public float GetGainWithoutRealize()      // BaseGain * BusGain * OcclusionGain * RtpcGain * CurveGain * SchedulerGain * FadeGain * MultiplierGain * UserGain * SwitchGain * EmitterGain * SpatialGain
    public float GetFinalGain()               // GetGainWithoutRealize() * RealizeGain
    public void ApplyToSource(AudioSource source)
    public void Reset()
}
```

### PitchStack (Nested Class)

The pitch counterpart of `GainStack`: every contributor to a voice's playback rate owns one slot, and only `AudioVoice.UpdateFinalPitch()` writes `AudioSource.pitch`. Every slot is a ratio (1 = unchanged).

```csharp
public class PitchStack
{
    public const float MaxRatio = 3f;           // Unity's AudioSource.pitch ceiling
    public const float MinRatio = 0.001f;       // floor: a voice is never stopped by its pitch

    public float Randomization { get; set; }    // container pitch randomization
    public float Decorrelation { get; set; }    // Decorrelated multi-position nudge
    public float Follow { get; set; }           // a Blend's pitch follow (value ÷ root), set by the play tree
    public float Curve { get; set; }            // pitch curves (ParameterCurve, cents), set by the play tree
    public float Doppler { get; set; }          // Pepper's Doppler (a container with Doppler = Pepper), set by the play tree;
                                                // applied after the limit
    public float TimeScale { get; set; }        // reserved: slow motion (planned)
    public float User { get; set; }             // AudioHandle.SetPitchOffset (every voice of its plays, later ones too)
    public float Emitter { get; set; }          // the Audio Emitter's Pitch Offset

    public bool HasAbsolute { get; }            // SetPitchAbsolute / the obsolete SetPitch
    public float Absolute { get; }
    public void SetAbsolute(float ratio);       // play at exactly this ratio, ignoring every slot
    public void ClearAbsolute();

    public float GetFinal(float limitSemitones = 0f);   // product of the slots (or the absolute ratio), clamped;
                                                        // the limit leaves Doppler out (it comes after)
    public void Reset();
    public static float SemitonesToRatio(float semitones);
}
```

### DopplerMath

```csharp
public enum DopplerMode { Unity, Pepper }

public static class DopplerMath
{
    public const float SpeedOfSound = 343f;    // m/s
    public static float Cents(float closingSpeed, float exaggeration, float maxCents);   // 1200·log2(c ÷ (c − v)) × exaggeration, within ±maxCents
    public static float Ratio(float closingSpeed, float exaggeration, float maxCents);   // the same as a playback-rate ratio
}
```
**Description:** Pepper's Doppler shift (3.1.0-dev.7), as a container set to `DopplerMode.Pepper` applies it. `closingSpeed` is positive when the sound and the listener close in; a speed at or past 90% of the speed of sound reads as 90%. 50 m/s is about +272.7 cents, −50 m/s about −235.7.

---

## AudioExtensions & Utilities

### AudioExtensions

**Namespace:** `AudioSystem`
**Type:** Static utility class

#### Extension Methods

```csharp
public static AudioVoice PlayAtPosition(this AudioContainer container, Vector3 position)
public static AudioVoice PlayAttached(this AudioContainer container, GameObject target)
public static AudioVoice PlayWithVolume(this AudioContainer container, float volumeScale, Vector3 position = default)
```

All three return the primary voice. `PlayWithVolume` multiplies `volumeScale` into every voice the play started, on top of volume randomization and any Switch/Blend outer volume.

**Example:**
```csharp
myContainer.PlayAtPosition(explosionPos);
myContainer.PlayAttached(player);
myContainer.PlayWithVolume(0.5f, transform.position);
```

#### Conversion Methods

```csharp
public static float DbToLinear(float db)      // 10^(db/20)
public static float LinearToDb(float linear)   // 20 * log10(linear), or -80 if linear <= 0
public static float CentsToPitch(float cents)  // 2^(cents/1200)
```

**Example:**
```csharp
float linear = AudioSystem.AudioExtensions.DbToLinear(-6f);  // ~0.5
float db = AudioSystem.AudioExtensions.LinearToDb(0.5f);      // ~-6dB
float pitch = AudioSystem.AudioExtensions.CentsToPitch(100f); // 1.059 (1 semitone up)
```

> **Unity 6.3 and newer** has its own `UnityEngine.AudioExtensions`. In a script with both `using UnityEngine;` and `using AudioSystem;`, the short name `AudioExtensions` is ambiguous there and won't compile (error CS0104). Write `AudioSystem.AudioExtensions`, as above, or add `using AudioExtensions = AudioSystem.AudioExtensions;` at the top of the file. The extension methods (`PlayAtPosition` and the others) aren't affected.

---

### ListenerUtil

**Namespace:** `AudioSystem`
**Type:** Static utility class

```csharp
public static Vector3 GetPosition()                    // the listener's position (zero if none)
public static bool TryGetPosition(out Vector3 position)
public static Transform Get()                          // the primary point's Follow transform, or the scene listener
public static void Set(Transform listener)             // registers a point that follows it; null removes it
public static void Invalidate()                        // removes Set's point; the next lookup searches again
```

**Description:** Where the listener is, for Pepper's distance-based features (voice importance, occlusion rays, the Furthest steal, multi-position picks). With a [listening point](#audiolisteningpoint) registered, the primary point (`AudioManager.SetPrimaryListeningPoint`, else the first) is the listener and nothing is searched for. With none, the scene is searched for an AudioListener with a one-time warning; a found listener is kept until destroyed, and while there is none the search runs at most once a frame. `Set` registers a point, which a point added with `AudioManager.AddListeningPoint` takes priority over; if its transform is destroyed, the search takes over again. `Get` returns null for a point placed with `SetPose`: use `GetPosition`.

---

### AudioListeningPoint

**Namespace:** `AudioSystem`
**Type:** class (created by `AudioManager.AddListeningPoint`)

```csharp
public Transform Follow { get; set; }      // read the pose from this transform; null = SetPose
public Vector3 Position { get; }
public Quaternion Rotation { get; }
public Vector3 Velocity { get; }           // measured each step, unless SetVelocity was called
public float Weight { get; set; }          // 0-1: how much it counts, with Spatial Mode = Pepper
public float PanWidth { get; set; }        // 0 mono … 1 stereo, with Spatial Mode = Pepper
public bool IsRegistered { get; }
public bool IsPrimary { get; }             // 3.1.0-dev.8
public void SetPose(Vector3 position, Quaternion rotation)   // ignored while Follow is alive
public void SetVelocity(Vector3 metresPerSecond)             // from then on, not measured
public void ClearVelocity()                                  // measure again, from where it is (a camera cut)
```

`Velocity` feeds Approach Speed and Pepper's Doppler. `ClearVelocity` (3.1.0-dev.7) goes back to measuring and starts again from the point's place, so a camera cut isn't read as speed; the last velocity stays until a new one is measured. Pointing `Follow` at another transform does the same (3.1.0-dev.8), keeping a velocity given with `SetVelocity`.

**Description:** Where the game listens from. While `Follow` is set and alive, the point reads that transform; **if it is destroyed, the point is removed** (`IsRegistered` false), with one warning a session (3.1.0-dev.8; it used to stay where it last was). Set the pose in Update or LateUpdate: Pepper's step runs after every LateUpdate. With the Audio Manager's Spatial Mode = Unity, Pepper listens from the primary point (`IsPrimary`; see [Listening Point Methods](#listening-point-methods)); with Pepper, every point counts by its `Weight` (a sound's level is the largest weight × its attenuation from a point) and `PanWidth` (0 gives the sounds it hears no pan). Changing `Follow`, `Weight` or `PanWidth` eases Pepper's levels over 100 ms.

---

### BeatScheduler

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour`
**File:** `RunTime/Utilities/BeatScheduler.cs`

**Description:** Fires an AudioEvent on a tempo grid (BPM) whose rate can be changed at runtime **without affecting pitch**. Each beat is a fresh one-shot post of the event asset (`AudioEvent.Post` with the beat's grid time as its scheduled start), not a time-stretch of a continuous clip. Timing is anchored to `AudioSettings.dspTime` so cadence is stable under frame-rate fluctuation.

**Use cases:** heartbeats, metronomes, ticking clocks, pulsing ambiences, sonar pings — any rhythmic SFX whose rate must vary independently of pitch.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| `audioEvent` (Inspector: **Audio Event**) | `AudioEvent` | The AudioEvent asset posted each beat. Also the `Event` property. |
| `emitter` | `GameObject` | Optional emitter used for the posted event's source and position. Defaults to the BeatScheduler's own GameObject. |
| `bpm` | `float` (1–600) | Beats per minute. Also writable at runtime via the `Bpm` property. |
| `maxCatchUpBeatsPerFrame` | `int` (1–16) | Cap on `OnBeat` callbacks per Update if the scheduler falls behind (frame hitch, very high BPM). Audio always plays at most ONCE per Update regardless of this value — overdue audio beats are dropped to prevent voice-stacking thump. Only affects `OnBeat` consumers that count beats for game logic (`every 4th beat trigger X`), keeping them synchronized with the grid across hitches. |
| `playOnEnable` | `bool` | Start scheduling automatically when enabled. |
| `fireImmediatelyOnStart` | `bool` | Fire the first beat on the next frame (true) or wait one interval (false). |

#### Properties

```csharp
public AudioEvent Event { get; set; }   // The event posted each beat (the Audio Event field)
public float Bpm { get; set; }          // Clamp-guarded, min 1. Setter applies to the next beat onward.
public bool IsRunning { get; }
public double BeatInterval { get; }     // 60 / Bpm, in seconds
```

#### Methods

```csharp
public void StartScheduling();          // Safe to call while already running (no-op).
public void StopScheduling();           // Stops firing. Does not stop voices already posted.
public void ResyncPhase();              // Reset phase: next beat fires at now + BeatInterval.
```

#### Events

```csharp
public event System.Action OnBeat;      // Fired on the main thread on every beat, after the event is posted.
```

Exceptions thrown by subscribers are caught and logged — they will not break the scheduler.

#### Example

```csharp
using UnityEngine;
using AudioSystem;

public class StressHeartbeat : MonoBehaviour
{
    [SerializeField] private BeatScheduler heartbeat;
    [SerializeField] private float calmBpm = 60f;
    [SerializeField] private float panicBpm = 160f;

    [Range(0f, 1f)] public float stress;

    private void Update()
    {
        heartbeat.Bpm = Mathf.Lerp(calmBpm, panicBpm, stress);
    }
}
```

#### Notes

- **Frame-level accuracy, not sample-level.** Actual playback lands on the next audio callback (a few ms after the frame). For sample-accurate musical grid scheduling, use `AudioSource.PlayScheduled` directly.
- **BPM changes apply to the next beat.** The beat already queued is not rewound — this is deliberate so tempo ramps sound natural.
- **Multiple schedulers are independent.** Two `BeatScheduler` components at the same BPM are not phase-locked to each other.
- See also: *Manual §10.4 Beat-Scheduled SFX* and *Cookbook → Rhythmic SFX*.

---

### IRTPCListener (Interface)

```csharp
public interface IRTPCListener
{
    void OnRTPCChanged(string parameter, float value);
}
```

**Description:** Implement this interface to receive notifications when a parameter's **global** value changes (`SetRTPC`, `SetParameter`, and each frame of a transition or global ramp), by name and synchronously. Emitter and play values don't notify. Register via `AudioManager.Instance.RegisterRTPCListener()`. Nested in `AudioManager` (`AudioManager.IRTPCListener`).

---

## AudioEventRegistry

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Access:** Singleton via `AudioEventRegistry.Instance`

### Description

Central registry for AudioEvents and AudioStates. Eliminates the Resources folder requirement by tracking assets in a single registry.

**Purpose:**
- Store references to all Events and States
- Auto-detect and use instead of Resources.LoadAll
- Allow Events/States to live anywhere in project
- Backwards compatible (falls back to Resources if no registry)

### Properties

```csharp
public static AudioEventRegistry Instance { get; }
public IReadOnlyList<AudioEvent> AudioEvents { get; }
public IReadOnlyList<AudioState> AudioStates { get; }
```

### Methods

```csharp
public void RegisterEvent(AudioEvent audioEvent)
public void RegisterState(AudioState audioState)
public void AutoPopulateFromResources()   // Editor only
public void AutoPopulateFromProject()     // Editor only
public void ValidateRegistry()            // Editor only
```

### Setup Example

```csharp
// 1. Create registry asset:
// Right-click -> Create > Audio System > Audio Event Registry

// 2. Place in: Assets/Audio/Resources/AudioEventRegistry.asset

// 3. Populate registry (in Inspector):
//    Click "Populate from Resources" or "Populate from Entire Project"

// 4. AudioManager automatically uses registry at startup
//    (Falls back to Resources.LoadAll if registry not found)
```

**Migration:** See [Migration](USER_GUIDE.md#5-migration) in the User Guide for full details.

---

## Multi-Position Emitters

### AudioMultiPositionEmitterParent

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour`

**Description:** Parent component that manages a group of child emitters for multi-position audio playback.

#### Properties

```csharp
public MultiPositionType PositionType { get; }
public float PositionLerpTime { get; }
public bool UseSpreadCurve { get; }
public float MinSpatialBlend { get; }
public float MaxRandomOffset { get; }
public float MaxPitchVariationCents { get; }
public IReadOnlyList<AudioMultiPositionEmitterChild> AllEmitters { get; }
public IReadOnlyList<AudioMultiPositionEmitterChild> ActiveEmitters { get; }
public int ActiveEmitterCount { get; }
public bool HasActiveEmitters { get; }
```

#### Methods

```csharp
public Vector3[] GetActivePositions()
public Transform[] GetActiveTransforms()
public void RefreshChildren()
public void EnableAllEmitters()
public void DisableAllEmitters()
public void SetAllVolumes(float volume)
```

**Example:**
```csharp
var emitterParent = GetComponent<AudioMultiPositionEmitterParent>();
AudioMultiHandle mh = myEvent.PostMultiPosition(emitterParent);
```

---

### AudioMultiPositionEmitterChild

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour`

**Description:** Individual emitter point in a multi-position setup.

#### Properties

```csharp
public bool IsActive { get; set; }
public float VolumeMultiplier { get; set; }
public bool UseDirectionality { get; set; }   // Scene-view cone gizmo only; not read during playback
public float ConeAngle { get; set; }          // that gizmo's angle, 0–360
public Vector3 Position { get; }
public Quaternion Rotation { get; }
public Vector3 Forward { get; }
```

`UseDirectionality` and `ConeAngle` only draw a cone in the Scene view. They don't change how an emitter sounds.

---

## Editor Tools

**Note:** These tools are Unity Editor extensions and not available at runtime.

### AudioAuditionWindow and AudioAudition

**Access:** `Window > Audio System > Audition` (USER_GUIDE §10.5)

Plays a container tree, or an event's Play actions, outside Play mode with a slider for every parameter it reads (Bypass Slew While parameters included), its switch values, any number of sweeps, a recording's replay and a live table of its layers; it follows the selected container or event, keeps playing while it is edited, and reads the tree again when it changes in the Inspector. The window runs an `AudioAudition` (namespace `AudioSystem`, in the Runtime assembly but used only outside Play mode):

```csharp
public enum SweepMode { Once, Repeat, BackAndForth }

public sealed class AudioAudition : System.IDisposable
{
    public static AudioAudition Current { get; }
    public static AudioAudition Begin(out string whyNot);   // the running one; null in Play mode or with another Audio Manager
    public AudioContainer Container { get; }                // what plays: a container...
    public AudioEvent Event { get; }                        // ...or an event
    public int SkippedActions { get; }                      // the event's actions not auditioned
    public bool IsPlaying { get; }                          // also while an event's delayed Play is to come
    public float Distance { get; set; }                     // m from the listening point at the origin
    public float Angle { get; set; }                        // °, 0 ahead, + right
    public void Play(AudioContainer container);
    public void Play(AudioEvent audioEvent);
    public void Stop();                                     // immediate
    public void Tick(float dt);                             // call every editor update

    public void SetParameter(AudioParameter p, float v);  public void SetParameter(string name, float v);   // takes it off its sweep or the replay
    public void SnapParameter(AudioParameter p);
    public float GetParameter(AudioParameter p);           public float GetParameter(string name);          // heard
    public float GetWrittenParameter(AudioParameter p);    public float GetWrittenParameter(string name);   // written: what a slider shows

    public void Sweep(AudioParameter p, float from, float to, float seconds, SweepMode mode);
    public void Sweep(string name, float from, float to, float seconds, SweepMode mode);
    public void Sweep(AudioParameter p, float from, float to, float seconds, bool backAndForth = false);   // as before
    public void Sweep(string name, float from, float to, float seconds, bool backAndForth = false);
    public void StopSweep();                                // every sweep
    public void StopSweep(AudioParameter p);              public void StopSweep(string name);
    public bool IsSweeping { get; }                         // any
    public bool IsSweepingParameter(AudioParameter p);    public bool IsSweepingParameter(string name);

    public void Replay(AudioParameterRecording recording);  // from its start, looping; null stops it
    public AudioParameterRecording Recording { get; }
    public bool IsReplaying { get; }
    public bool ReplayLooping { get; set; }                 // default true; false stops at the end
    public float ReplayTime { get; set; }                   // set = scrub (snapped)
    public void SetReplayLoop(float start, float end);      // end <= start = all of it
    public float ReplayLoopStart { get; }  public float ReplayLoopEnd { get; }
    public void StopReplay();
    public bool IsReplayed(AudioParameter p);              public bool IsReplayed(string name);

    public void SetSwitch(AudioSwitchGroup g, string v);  public void SetSwitch(string group, string v);
    public string GetSwitch(AudioSwitchGroup g);          public string GetSwitch(string group);
    public void GetLayers(List<AuditionLayer> into);       // Container, Clip, LevelDb, PitchSemitones, LowPassHz, Virtual, Bus
    public static void FindInputs(AudioContainer root, AuditionInputs into);   // Parameters, ParameterNames, SwitchGroups, SwitchGroupNames, SwitchValues
    public static void FindInputs(AudioEvent audioEvent, AuditionInputs into); // every Play action's tree
    public void Dispose();
}
```

**How it runs.** `Begin` creates a hidden (`HideAndDontSave`) Audio Manager in Manual update mode and starts it without `Awake` (edit mode doesn't call it): the voice pool only, no coroutines, no PlayerLoop step, no event, bus or occlusion loading. It registers a listening point at the origin facing +z and a hidden source object that `Distance` and `Angle` move. Values are set on that manager's global scope: assets through their smoothing, names through the string API. `Tick` runs the event's delayed actions, the replay, the manager's step, the sweeps and, every 0.1 s, the voice pass. A play is placed in 3D but heard 2D (spatial blend 0). A Next Play Only Switch in the tree plays again when a switch value is set by hand (not by a replay). Built-in parameters and driven Switch Groups follow the place and their parameter, not `SetParameter`/`SetSwitch`. Outside Play mode a Sequence plays its first clip only and a stop is immediate. `Dispose` stops everything and destroys the hidden objects and the listening point; the window calls it before Play mode, before a script reload and when it closes.

**Sweeps** run one per parameter, any number at once: a new sweep of a parameter replaces its earlier one, and setting a parameter by hand stops only its own. **Once** ends at `to` (the written value stays there); **Repeat** starts again from `from`; **BackAndForth** turns round.

**An event** (`Play(AudioEvent)`) runs its Play actions as a post would: through each action's Target Bus (volume, mute), with the event's priority and Persistent flag, after the action's Delay; its Set RTPC and Set Switch actions set the audition's values; the rest are counted in `SkippedActions`. Max Instances and cooldowns don't apply, and the event's own state is never touched.

**A replay** writes every recorded value at once, as the game wrote it, through the parameter's smoothing, holding each key until the next; the recorded switch values are set as the game set them. Going round the loop and setting `ReplayTime` snap the values (heard at once). A parameter set by hand or swept is taken off the replay; `Replay` again restores it.

### AudioLiveViewWindow, AudioEmitterRecorder and AudioParameterRecording

**Access:** `Window > Audio System > Live View` (USER_GUIDE §10.6), in Play mode

Every Audio Emitter with its values (written and heard) and switches, and each play on it (`AudioManager.GetPlaysDebug`): event, container, bus, Persistent, real or virtual, its own values (`AudioHandle.ParameterCount` and the listing beside it) and each voice's level, pitch, region and reason for being virtual. **Record** and **Stop and Save** record the chosen emitter:

```csharp
public sealed class AudioEmitterRecorder
{
    public static AudioEmitterRecorder Begin(AudioEmitter emitter, out string whyNot);   // Play mode only
    public AudioParameterRecording End();     // in memory; save it with AssetDatabase.CreateAsset
    public AudioEmitter Emitter { get; }
    public AudioParameterRecording Recording { get; }
    public float Seconds { get; }             // unscaled
    public bool IsRecording { get; }          // false after End, or once the emitter is destroyed
}

public sealed class AudioParameterRecording : ScriptableObject
{
    public string EmitterName { get; }
    public float Length { get; }              // seconds
    public IReadOnlyList<ParameterTrack> Parameters { get; }   // parameter (asset or null), parameterName, times, values; ValueAt(t): NaN = not set
    public IReadOnlyList<SwitchTrack> Switches { get; }        // group, times, values; ValueAt(t): "" = not set
    public ParameterTrack Find(AudioParameter p);  public ParameterTrack Find(string name);
}
```

A recorder samples its emitter at the start of each of Pepper's steps, after the frame's game code: every value set on it as written (a ramp's position while it runs), Bypass While ones included, and its switch values, keeping a key only when one changes; a value the game clears is a NaN key. Values set on a play's handle and global values aren't recorded. While nothing records, the step does nothing for it.

### LoopAnalysisWindow

**Access:** `Window > Audio System > Loop Analysis` (USER_GUIDE §10.7)

Per clip (Decompress On Load): its fundamental by the YIN method and the Root Value it gives, RPM = fundamental × 120 ÷ cylinders (a four-stroke engine), with ×2 and ÷2 for an octave slip; a seam check at the loop point (the first sample against the line through the last two, over the clip's own RMS second difference: over 4× clicks) and the level of the first against the last 50 ms; a preview of the seam; RMS and peak in dBFS and integrated loudness in LUFS (ITU-R BS.1770-4, a mono clip as one channel). A Regions Blend's Inspector has **Detect Root Values**, which sets each layer's Root Value from its clip (with Undo) for the cylinder count set beside it. Cylinders is a scale, one setting shared by the three tools. A reading more than half an octave from the RPM in its clip's name is flagged; when most readings sit 2 semitones or more from them, the tools name the cylinder count that puts them within a semitone (**Use N** in the windows; a Console warning from Detect Root Values, which still writes the raw reading).

### EngineBuilderWindow

**Access:** `Window > Audio System > Engine Builder` (USER_GUIDE §10.8)

From selected loops (each one's set, OnLoad or OffLoad, and RPM guessed from its name, typed, or detected with its own Cylinders slider, shared with Loop Analysis; a detected RPM far from its name's is flagged): one looping single-clip container per loop (Uses Parent Positioning and Output, Avoid Repeat Last 0); an on-load and an off-load Regions Blend on RPM (Root Values, Auto Layout, Equal Power, Pitch Follow, Silent Layers Virtualize, Force Loop); a Regions Blend on load crossfading them at constant power from 0.3 to 0.7 (3D, Doppler Level 0); one Persistent event on the chosen Target Bus. Empty RPM and Load fields make EngineRpm (0–9000 RPM) and EngineLoad01 (0–1). The clips are set to mono and Decompress On Load after a confirmation listing them; no asset is ever overwritten. With one set only, the RPM Blend is the root and there is no load Blend; the window and its dialog say so.

### EngineGrainsWindow

**Access:** `Window > Audio System > Engine Grains` (Unity 6000.3 and later)

Cuts an engine sweep (Decompress On Load) into a [GrainTable](#graintable). The sweep's RPM is **Known**, typed from one
RPM to another over the whole clip along a **Curve** (Straight Line or Even Pitch: a synthesised sweep), or measured
**From Pitch** with the Cylinders scale shared with Loop Analysis, looked for between the two RPMs typed. Each cut is
one engine cycle long at that RPM, then moved, by less than half a period of the fundamental and at most 6% of a cycle,
to where the waveform best matches the start of the grain before it, stretched to the new pitch. With a typed RPM, the
periods of the fundamental per cycle are measured against it, and the sweep's own curve is measured from a strong
harmonic followed through it: when it disagrees with the one typed (the other curve, or the RPM changing by more than 3%
more or less over the clip), the window says so and offers the curve that fits, with the two ranges that keep one typed
end, each cut on trial and the one whose cuts match better first. The report gives the range covered and warns of gaps
(no grain over 250 RPM), a fast sweep (under 10 grains per 1000 RPM), a typed curve the grains don't follow (over 3% off
on average) and cuts that found nothing like the cycle before within reach (over 20% of them below a 0.5 match); it notes
held stretches (under 50 RPM of change for 0.3 s or more). **One Grain At** loops the grain nearest an RPM, unchanged, to
hear whether its cuts are clean. Saving makes `<sweep>_Grains.asset` beside the clip, or updates the table it was given
(with Undo).

### AudioContainerValidator (Container Validator)

**Namespace:** `AudioSystem.Editor`
**Access:** `Window > Audio System > Container Validator` (USER_GUIDE §10.9)

```csharp
public static void Validate(AudioContainer container, List<ValidationIssue> issues);
public static void ValidateEvent(AudioEvent audioEvent, List<ValidationIssue> issues);
public static void ApplyFix(ValidationIssue issue);   // under Undo
public class ValidationIssue { AudioContainer container; Object target; string issue; MessageType severity; Action fix; }
```

Every container and event in the project: empty or null entries and zero weights; a Routing container with no clip; Regions gaps, dips and 0–0 regions; a layer following pitch with no Root Value; pitch past a Blend's Tuning Warning (Pitch Follow over the follow parameter's own range, plus pitch curves); a 3D loop with Doppler in a Virtualize Blend; stereo clips on 3D layers and streaming clips on layers that go virtual; 3D distances, or the Attenuation asset's curve at its Max Distance; a Play action with no Target Bus (a warning without a Mixer Group, else Info); a looping event that isn't Persistent (Info). `target` is the event for an event's issue (`container` is null then).

### Audio Import Settings and folder rules

**Namespace:** `AudioSystemEditor.Import`

The project's `AudioImportSettings` asset (Create ▸ Audio System ▸ Import Settings, under Assets; the first by path when there are several, never one inside a package) holds the category presets and **Folder Rules**:

```csharp
public class FolderImportRule { string name; string folder; bool includeSubfolders; bool enabled; CategoryPreset settings; }
public List<FolderImportRule> FolderRules { get; }
public FolderImportRule RuleFor(string assetPath);   // the most specific enabled folder; null when none
```

`AudioImportRulesPostprocessor` applies a clip's rule when the clip is first imported (a new file); later reimports keep the clip's settings. The inspector's **Apply to Existing Clips** re-applies a rule to the clips already in its folder.

### SoundBuilderSimpleWindow

**Namespace:** `AudioSystemEditor`
**Type:** EditorWindow
**Access:** `Window > Audio System > Sound Builder (Simple)`

#### Description

One-click sound setup wizard that creates Bus + Container + Event with all references pre-filled.

**Features:**
- 5 creation modes: Complete Setup, Container Only, Event Only, Bus Only, Batch Import
- All 5 container types supported
- Auto-create folder structure
- Validate Resources placement

#### Usage

1. `Window > Audio System > Sound Builder (Simple)`
2. Drop AudioClip(s) in field
3. Choose a creation mode and container type
4. Click "Create Complete Sound Setup"
5. Done!

---

### AudioEventEditor

**Namespace:** `AudioSystemEditor`
**Type:** Custom Inspector (Editor Only)
**Applies To:** AudioEvent assets

#### Description

Custom Inspector for AudioEvent: whether the event can be found by name or id, and the stable id.

**Features:**
- Says whether the event is in the AudioEventRegistry (or, with no registry, in Resources/Audio/Events), with **Add to Registry** or **Move to Resources** buttons. Only posting by name or id (`AudioManager.PostEvent`, networked posts) needs that: an event held in a field (`myEvent.Post()`) plays wherever it is. Earlier versions said such an event "will NOT load at runtime" and warned in the Console on import; both were wrong and are gone.
- Bakes the asset's GUID into the event on import (the stable network id).

---

### AudioEventRegistryEditor

**Namespace:** `AudioSystemEditor`
**Type:** Custom Inspector (Editor Only)
**Applies To:** AudioEventRegistry assets

#### Description

Custom Inspector for AudioEventRegistry with utility buttons and migration guide.

**Buttons:**
- **Populate from Resources Folders** - Fast for existing Resources-based projects
- **Populate from Entire Project** - Searches entire project
- **Validate Registry (Remove Nulls)** - Cleans up missing references
- **Clear All** - Removes all registered assets (with confirmation)

---

### Occlusion Layout Builder Window

**Namespace:** `AudioSystemEditor.Occlusion`
**Type:** EditorWindow
**Access:** `Window > Audio System > Occlusion Layout`

Authors the per-bus occlusion slot hierarchy on the project's voice mixer and serializes it to an `OcclusionLayout` asset. See [Manual §14.3](USER_GUIDE.md#143-authoring-the-slot-layout).

**Top-bar fields:**
- **Voice Mixer** — the mixer to author into
- **Occlusion Layout** — the asset to populate (or click "Create New")
- **Reverb Send Bus Registry** (optional) — when assigned, each generated slot also receives one Send effect per registered reverb bus
- **Slots Per Bus** — concurrent occluded voices per bus (default 6)

**Actions:**
- **Scan, Auto-Create, Refresh** — walks the project for containers with `useOcclusion`, `allowReverbSend` or `usePropagation` (each takes a slot at play time) and ambient sources with `useOcclusion`, collects the unique mixer groups they play through (a layer with `usesParentOutput` counts its parent's), authors `<Bus>_OcclusionLayer` + slot groups under each, exposes their cutoff parameters and (optional) Send level parameters, writes the result to the OcclusionLayout asset.
- **Scan only (no mixer changes)** — read-only sanity check.
- **Diagnose Reflection** — dumps a reflection report to the Console for the audio internals (debug aid when the auto-creator misbehaves on a new Unity version).

A foldout at the bottom shows the manual schema for designers who prefer to hand-author the slot hierarchy.

---

### Reverb Send Buses Window

**Namespace:** `AudioSystemEditor.Reverb`
**Type:** EditorWindow
**Access:** `Window > Audio System > Reverb Send Buses`

Project-wide overview of `ReverbSendBus` assets. See [Manual §15.6](USER_GUIDE.md#156-authoring-workflow-reverbautocreator--reverb-send-buses-window).

**Top-bar actions:**
- **Create New Bus** — Save File panel for authoring a fresh `ReverbSendBus` asset
- **Refresh** — re-scans the project for `t:ReverbSendBus` and re-runs Validate per asset
- **Validate All** — same as Refresh but with a summary HelpBox
- **Generate All Missing** — runs `ReverbAutoCreator.EnsureSendBus` on every bus whose `ReverbBus` is null and whose `ReverbMixer` is assigned

Per-row UI: bus name (link-label, pings the asset), assigned mixer, assigned bus group, and a foldable issue list from `ReverbAutoCreator.Validate`. Auto-refreshes on window focus.

---

### ReverbSendBusInspector

**Namespace:** `AudioSystemEditor.Reverb`
**Type:** Custom Inspector (Editor Only)
**Applies To:** `ReverbSendBus` assets

Adds four top-of-inspector action buttons that drive `ReverbAutoCreator`:

| Button                    | What it does                                                                                                  |
|---------------------------|---------------------------------------------------------------------------------------------------------------|
| **Generate Mixer Group**  | Authors / refreshes the mixer group + SFX Reverb effect + exposed parameters. Disabled until `Reverb Mixer` is assigned. |
| **Validate Wiring**       | Read-only check; reports "Wiring OK" or a bullet list of issues. Always enabled.                              |
| **Apply Params → Mixer**  | Pushes the SO's parametric values into the mixer effect without re-running the group/expose work. Enabled once `Reverb Bus` is generated. |
| **Pull Params ← Mixer**   | Copies the mixer effect's current values back onto the SO.                                                    |

The ten Advanced parametric reverb fields live behind an EditorPrefs-backed foldout (closed by default); the four Basic fields stay visible. EditorPrefs key is project-wide so the toggle survives selection changes.

---

### AudioPortalEditor / AudioZoneEditor / PropagationManagerEditor

**Namespace:** `AudioSystemEditor.Propagation`
**Type:** Custom Inspectors (Editor Only)

Diagnostic inspectors for the propagation subsystem's runtime components.

- **AudioPortalEditor** — adds a Portal Diagnostics panel showing door source status, current transmission / cutoff (live in play mode), and the auto-detected blend-axis side mapping. Includes a "Re-detect zone sides" button.
- **AudioZoneEditor** — adds a Zone Diagnostics panel showing live reverb profile values, volume collider counts, and the list of `AudioPortal`s in loaded scenes referencing this zone.
- **PropagationManagerEditor** — adds a Runtime Debug panel (play mode only) showing registered zone/portal/source counts, active zone/portal counts after culling, the listener's zone stack, blend-portal membership, pending hysteresis events, and emitter pool counters.

All three refresh every repaint while in play mode (via `RequiresConstantRepaint`).

---

## Propagation

**Namespace:** `AudioSystem.Propagation`
**Added in:** v2.3.0
**Location:** `Assets/Scripts/SFX_System/RunTime/Propagation/`

Zone/portal graph-based routing for long-running ambient beds. Additive — does not modify the SFX event pipeline. See [Manual Chapter 13](USER_GUIDE.md#13-ambient-propagation-subsystem) for architecture and design rationale.

---

### PropagationManager

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`
**Access:** `PropagationManager.Instance` (lazy-instantiated at play time)

Singleton orchestrator. Owns zone/portal/source registries, the Dijkstra solver, the emitter pool, and per-frame smoothing.

#### Properties

```csharp
public static PropagationManager Instance { get; }
public static bool HasInstance { get; }
```

`Instance` returns null in Edit mode (to prevent phantom GameObject creation on inspector edits). `HasInstance` is null-safe for use in `OnDisable` during scene teardown.

#### Inspector Configuration

| Field                            | Default | Description                                                                                                 |
|----------------------------------|---------|-------------------------------------------------------------------------------------------------------------|
| `solveRateHz`                    | 15      | Solves per second. Per-frame emitter smoothing handles the in-between.                                      |
| `distancePenaltyDbPerMeter`      | 0       | Extra dB cost per meter of portal-to-portal distance. 0 = pure transmission routing.                        |
| `silenceOutsideGraph`            | false   | When false, voices outside every AudioZone play at full audibility (whitelist mode). When true, they fall silent and maximally muffled (complete-coverage mode). See [Manual §13.4](USER_GUIDE.md#134-the-solve-pipeline). |
| `transmissionSkipThreshold`      | 1e-5    | Linear-amplitude floor below which the solver treats a portal as fully closed and skips it. Raise to cull near-closed doors earlier. |
| `drawListenerZoneOverlay`        | false   | Editor-only debug overlay. When ON, draws a scene-view label above the listener showing the current zone name and inside-portal count. |
| `maxAmbientEmitters`             | 16      | Global cap. Farthest emitters silenced first when exceeded.                                                 |
| `prewarmPoolSize` (range 0–64)   | 0       | Pre-allocate this many AmbientEmitter GameObjects at Start. Clamped to maxAmbientEmitters. 0 = lazy.        |
| `emitterSmoothSpeed`             | 4       | Per-frame smoothing speed for volume/cutoff/position.                                                       |
| `maxPathsPerSource` (range 1–6)  | 1       | How many simultaneous propagation paths each ambient source may voice. 1 = original single-best-path. Raise to hear an ambience leak through multiple openings at once. |
| `enableObstruction`              | false   | Per-emitter listener→virtual-emitter raycast obstruction on top of portal-based propagation.                |
| `obstructionLayerMask`           | ~0      | Physics layer mask for the obstruction raycast.                                                             |
| `obstructionAttenuationDb` (0–30)| 6       | Extra dB applied when the obstruction ray is blocked.                                                       |
| `obstructionCutoffHz` (100–22000)| 2000    | Upper bound on the low-pass cutoff under obstruction. Combines with solver cutoff via min-wins.             |
| `zoneDwellSeconds` (0–1)         | 0.15    | Dwell time before a listener zone enter/exit commits. Absorbs straddle-jitter on boundary crossings. 0 = instant commit. |
| `culler`                         | —       | Nested `PropagationProximityCuller` configuration (see below).                                              |
| `listenerOverride`               | null    | Optional explicit listener transform. Without one, the listening point registered with Pepper (`AudioManager.AddListeningPoint`); only with no point, the scene's `AudioListener`, then `Camera.main`. |

#### Registration API

```csharp
public void RegisterZone(AudioZone z);
public void UnregisterZone(AudioZone z);
public void RegisterPortal(AudioPortal p);
public void UnregisterPortal(AudioPortal p);
public void RegisterSource(AmbientSource s);
public void UnregisterSource(AmbientSource s);
```

Zones, portals, and ambient sources self-register via their `OnEnable` / `OnDisable`. Manual calls are safe (idempotent).

#### Notification API (typically called only by `AudioListenerZoneTracker`)

```csharp
public void NotifyListenerEnteredZone(AudioZone z);
public void NotifyListenerExitedZone(AudioZone z);
public void NotifyListenerEnteredPortal(AudioPortal p);
public void NotifyListenerExitedPortal(AudioPortal p);
public void NotifyPortalParamsChanged(AudioPortal p);
```

`NotifyPortalParamsChanged` triggers an immediate re-solve instead of waiting for the next scheduled tick. Called from `AudioPortal.HandleDoorChanged` when the door source fires `OnChanged`.

#### Queries

```csharp
public AudioZone GetCurrentListenerZone();
public AudioZone GetZoneContainingPoint(Vector3 worldPoint);
public float GetSourceAudibility(AudioZone sourceZone);
public float GetSourcePropagationCutoff(AudioZone sourceZone);
public AudioZone GetResolvedListenerReverbProfile(Vector3 listenerPos, AudioReverbProfile dest);
public AudioZone GetResolvedListenerReverbProfile(AudioReverbProfile dest);
public bool SilenceOutsideGraph { get; }
```

- `GetCurrentListenerZone` — top of the listener zone stack, or null if the listener isn't inside any registered zone.
- `GetZoneContainingPoint` — first registered zone whose `ContainsPoint(worldPoint)` returns true, or null. Used by the reverb-send driver to resolve source zones.
- `GetSourceAudibility` / `GetSourcePropagationCutoff` — per-zone reads from the listener-audibility cache, populated each solve tick. Same-zone = 1.0 / 22 kHz; through-portals = chain-of-portals weight; unreachable = 0 / 500 Hz.
- `GetResolvedListenerReverbProfile` — fills the provided `AudioReverbProfile` with the listener's current zone's reverb *character*, resolved zone → `ReverbBus` → `ReverbSendBus` SO (portal-blend lerped when inside a blend region). Returns the canonical zone that was resolved. Still used by the per-source ambient path (`AmbientEmitter`); the per-bus parametric driver no longer calls it.

---

### AudioZone

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`
**Required Components:** `BoxCollider` (auto-forced to `isTrigger = true`)

A volume of world space tagged as one acoustic room. Nodes of the propagation graph.

#### Inspector

| Field              | Default | Description                                                                      |
|--------------------|---------|----------------------------------------------------------------------------------|
| `zoneId`           | ""      | Optional debug identifier. Not used at runtime.                                 |
| `baseVolumeDb`     | 0       | Optional per-zone attenuation baseline added to ambient source volume.          |
| `activationRadius` | 50      | Distance from listener beyond which this zone is culled. `Infinity` = never cull. The culler preserves transitive reachability — a zone connected to the listener's zone through near portals is force-included even when individually out of range. |
| `entryFadeMeters` (Min 0) | 0 | Soft membership band measured inward from the trigger surface. 0 = hard boundary (binary in/out). When > 0, a point at the surface has membership 0 and a point this many meters inside has membership 1; lerps linearly in between. Used to fade reverb sends, listener-side reverb character, and `BaseVolumeDb` smoothly across the boundary. |
| `reverbBus`        | null    | The mixer reverb bus voices physically in this zone send to (source-side). Drag in the `ReverbBus` field from a `ReverbSendBus` asset. Null = no reverb-send routing for sources in this zone. The bus's *character* lives on its `ReverbSendBus` asset, not here. See [Reverb Send Buses](#reverb-send-buses). |

#### Properties

```csharp
public string ZoneId { get; }
public float BaseVolumeDb { get; }
public float ActivationRadius { get; }
public float EntryFadeMeters { get; }
public AudioMixerGroup ReverbBus { get; }          // null when no bus assigned; routing only (character lives on the ReverbSendBus SO)
public IReadOnlyList<BoxCollider> VolumeColliders { get; }
```

`VolumeColliders` is populated from `GetComponents<BoxCollider>()` in `Awake` — add multiple BoxColliders for L-shapes, they're OR-unioned.

#### Virtual Methods (override for custom shapes)

```csharp
public virtual bool ContainsPoint(Vector3 worldPoint);
public virtual Vector3 ClosestSurfacePoint(Vector3 worldPoint);
public virtual float GetMembershipFactor(Vector3 worldPoint);  // 0..1; binary when entryFadeMeters == 0
```

Default implementations iterate all volume colliders. Subclass to support sphere-shaped or mesh-shaped zones without touching the solver. Subclasses that override `GetMembershipFactor` must respect the contract "0 at surface, 1 once fully inside."

---

### AudioPortal

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`
**Required Components:** `BoxCollider` (auto-forced to `isTrigger = true`)

Edge of the propagation graph. Connects two zones, optionally modulated by an `IPortalDoorSource`, and doubles as the listener blend region.

#### Inspector — Zones

| Field              | Default | Description                                                                                  |
|--------------------|---------|----------------------------------------------------------------------------------------------|
| `zoneA`            | null    | One of the two connected zones.                                                              |
| `zoneB`            | null    | The other connected zone.                                                                    |
| `autoDetectZones`  | false   | On first enable, probe the portal's own trigger-collider world bounds and pick the two `AudioZone`s whose volumes overlap. Lets designers drop a portal into a doorway without hand-assigning the zone refs. One-time scan at OnEnable; manually-assigned refs are kept as fallback if fewer than two are found. |

`zoneA` and `zoneB` are required (either assigned manually or filled by auto-detect). `OnValidate` errors if either is null or if `zoneA == zoneB`.

#### Inspector — Door

| Field           | Description                                                                       |
|-----------------|-----------------------------------------------------------------------------------|
| `doorSourceRaw` | MonoBehaviour implementing `IPortalDoorSource`. Null = always open.               |

`OnValidate` clears the reference and logs an error if the assigned MonoBehaviour doesn't implement the interface.

#### Inspector — Transmission

| Field                | Default | Description                                                      |
|----------------------|---------|------------------------------------------------------------------|
| `baseTransmission`   | 1.0     | Amplitude multiplier when fully open (`OpenProgress = 1`).       |
| `closedTransmission` | 0.1     | Amplitude multiplier when fully closed (`OpenProgress = 0`).     |
| `openLowPassHz`      | 12000   | Low-pass cutoff when fully open. Slightly muffled but audibly distinct from transparent — a door-less portal at least makes itself heard.    |
| `closedLowPassHz`    | 600     | Low-pass cutoff when fully closed.                                |

`OnValidate` warns if `closedTransmission > baseTransmission` or `closedLowPassHz > openLowPassHz` (values likely swapped).

#### Inspector — Blend Region

| Field                 | Default          | Description                                                              |
|-----------------------|------------------|--------------------------------------------------------------------------|
| `transitionAxisLocal` | `Vector3.forward`| Local-space axis pointing through the opening. Used for blend-factor computation. |
| `activationRadius`    | 50               | Distance from listener beyond which this portal is culled.               |

#### Properties

```csharp
public AudioZone ZoneA { get; }
public AudioZone ZoneB { get; }
public IPortalDoorSource DoorSource { get; }
public float ActivationRadius { get; }
public BoxCollider TriggerCollider { get; }
public event Action<AudioPortal> OnPortalParamsChanged;
```

#### Methods

```csharp
public float GetOpenProgress();                          // 0..1; returns 1 if no door source
public float GetTransmissionMultiplier();                // linear amplitude, dB-interpolated
public float GetCutoffHz();                              // log-frequency-interpolated
public Vector3 GetVirtualEmitterPosition(Vector3 listenerWorldPos);
public float GetBlendFactor(Vector3 listenerWorldPos);   // 0..1 along transition axis
public AudioZone GetZoneAtBlendFactorZero();             // zone on the blend=0 axis side
public AudioZone GetZoneAtBlendFactorOne();              // zone on the blend=1 axis side
public AudioZone GetOpposite(AudioZone z);               // solver graph traversal
```

`GetZoneAtBlendFactorZero/One` auto-detect which zone sits on each side of the transition axis (lazy-cached on first call). Designers never configure this — they just orient the GameObject's `+Z` through the opening.

---

### AmbientSource

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`

Declares an ambient bed. Does not play audio directly — the `PropagationManager` drives a pooled `AmbientEmitter` on its behalf.

#### Inspector — Source

| Field                  | Default | Description                                                                          |
|------------------------|---------|--------------------------------------------------------------------------------------|
| `sourceZone`           | null    | The zone where this ambience originates (required).                                  |
| `loopingClip`          | null    | The looping audio clip. Must be loop-authored (no pops at boundary).                 |
| `ambienceMixerGroup`   | null    | `AudioMixerGroup` to route through. Use the Ambience bus for bus ducking / mix fades.|
| `useOcclusion`         | false   | Flag for the Occlusion Layout Builder scan. When true, this source's mixer group is included so a slot layer is generated under it. Does not change emitter audio behavior — propagation drives the per-emitter low-pass directly. |

#### Inspector — Level

| Field                  | Default | Description                                                                          |
|------------------------|---------|--------------------------------------------------------------------------------------|
| `sourceBaseVolumeDb`   | 0       | Baseline volume in dB. Propagation attenuation is subtracted from this. Clamped to [-60, 12] in the inspector. |
| `playOnStart`          | true    | Auto-register with `PropagationManager` on enable. Set false for sources controlled manually (weather toggling rain). |

#### Inspector — 3D Rolloff (optional override)

| Field                  | Default            | Description                                                                          |
|------------------------|--------------------|--------------------------------------------------------------------------------------|
| `overrideRolloff`      | false              | Master switch for the override block. When false, the emitter uses neutral defaults (Linear / 1m / 500m), tuned for ambient beds whose spatial position is dominated by the propagation solver. |
| `rolloffMode`          | `Linear`           | Distance falloff curve. Linear = evenly distributed; Logarithmic = matches Unity's default. Custom not supported here. |
| `minDistance` (Min 0.01)| 1                 | Distance at which the source is at full volume.                                       |
| `maxDistance` (Min 0.1) | 500               | Distance at which the source becomes inaudible (Linear) or quiet (Log).               |

Override settings apply to the AudioSource component on the managed emitter each time it's acquired from the pool. Leave `overrideRolloff = false` (the default) when the source's role is "propagation-routed bed" — the solver's per-emitter LPF and the portal-driven virtual-emitter position handle spatialisation. Turn on for tight point-source ambiences (a humming machine that needs a 10m maxDistance, etc.).

#### Properties

```csharp
public AudioZone SourceZone { get; }
public AudioClip LoopingClip { get; }
public AudioMixerGroup MixerGroup { get; }
public float SourceBaseVolumeDb { get; }
public bool UseOcclusion { get; }
```

#### Methods

```csharp
public void SetBaseVolumeDb(float db);   // clamped to [-80, 20]; picked up on next solve
public void Register();                  // manual registration (when playOnStart is false)
public void Unregister();                // fades out cleanly, doesn't abruptly stop
public void ApplyRolloffTo(AudioSource s);  // applies override (or defaults) to the emitter's AudioSource;
                                            // called by AmbientEmitter.Initialize on acquire
```

---

### AmbientEmitter

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour` (sealed)
**Menu:** Hidden from Add Component menu — managed programmatically by `PropagationManager`

Pooled persistent looping voice. Owns its own `AudioSource` and `AudioLowPassFilter`. Bypasses the SFX voice pool by design — routed through the designated Ambience mixer group for bus integration.

You generally don't interact with this component directly; the `PropagationManager` pool manages lifecycle.

#### Properties

```csharp
public AmbientSource OwningSource { get; }
public AudioPortal OwningPortal { get; }     // null for direct same-zone playback
public bool IsSilent { get; }                // safe-to-recycle check
public float TargetVolumeLinear { get; }
public float CurrentVolumeLinear { get; }
public Vector3 TargetPosition { get; }
```

#### Methods (called by PropagationManager)

```csharp
public void Initialize(AmbientSource owner, AudioClip clip, AudioMixerGroup group, float smoothSpeed);
public void FadeInTo(AudioPortal portal, float targetVolLinear, float targetCutoffHz, Vector3 pos);
public void SetTarget(float volumeLinear, float cutoffHz, Vector3 pos);
public void SetOwningPortal(AudioPortal portal);
public void FadeOut();
public bool Tick(float dt);                  // returns true when safe to release
public void ReleaseImmediate();              // unconditional stop + state reset
```

---

### AudioListenerZoneTracker

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`

Attach to the GameObject that carries the player's Collider, usually the player root, not the camera that holds the `AudioListener`: Unity only sends trigger events to a GameObject with a Collider. Watches for trigger enter/exit on `AudioZone` and `AudioPortal` colliders and forwards events to `PropagationManager`.

Auto-adds a kinematic `Rigidbody` if none is present — required for Unity trigger events on static zone/portal colliders.

#### Inspector

| Field            | Default | Description                                                                |
|------------------|---------|----------------------------------------------------------------------------|
| `audioZoneLayer` | Everything | Optional layer mask filter. Limits trigger detection to specified layers. |

---

### IPortalDoorSource

**Namespace:** `AudioSystem.Propagation`
**Type:** Interface

Minimal contract any MonoBehaviour can implement to drive an `AudioPortal`'s open/closed state. Portable — propagation does not depend on any project-specific door system.

```csharp
public interface IPortalDoorSource
{
    float OpenProgress { get; }      // 0 = closed, 1 = open
    event System.Action OnChanged;   // fire when OpenProgress changes meaningfully
}
```

Firing `OnChanged` triggers an immediate re-solve on the subscribed portal. Spamming it every frame is safe but redundant; prefer threshold-based dispatch (e.g. `Δ ≥ 0.02`).

#### Adapter Pattern

Projects typically bridge their own door state (animator float, ScriptableObject channel, physics hinge) with a thin adapter MonoBehaviour:

```csharp
using AudioSystem.Propagation;

public class DoorAnimatorAdapter : MonoBehaviour, IPortalDoorSource
{
    public float OpenProgress => animator.GetFloat("openAmount");
    public event Action OnChanged;
    // ... fire OnChanged on threshold changes
}
```

---

### PropagationProximityCuller

**Namespace:** `AudioSystem.Propagation`
**Type:** `[Serializable]` class (sub-component of `PropagationManager`, not a separate MonoBehaviour)

Trims the active solver graph to zones and portals within range of the listener. Scales propagation to large levels without modifying solver logic.

#### Inspector (nested under PropagationManager)

| Field                     | Default | Description                                                                      |
|---------------------------|---------|----------------------------------------------------------------------------------|
| `cullCheckFrameInterval`  | 10      | Frames between active-set recomputations. 0 = culler disabled (everything active).|
| `globalMaxActiveZones`    | 64      | Hard cap on active zones after radius filter. Farthest dropped first.            |
| `globalMaxActivePortals`  | 128     | Hard cap on active portals after radius filter.                                  |

#### Properties

```csharp
public bool Enabled { get; }     // true if cullCheckFrameInterval > 0
```

The culler's first tick runs immediately regardless of interval, so the solver never sees an empty graph during startup.

---

## Occlusion Mixer Slots

Pre-built per-bus pools of mixer groups used as routing targets for SFX voices that need cutoff modulation. Replaces per-source `AudioLowPassFilter` (which clicked on Stop→Play biquad reset). See [Manual chapter 14](USER_GUIDE.md#14-occlusion-mixer-slot-pool) for architecture and design rationale.

---

### OcclusionLayout

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Occlusion Layout
**Editor window:** Window > Audio System > Occlusion Layout

Generated layout describing which mixer groups serve as occlusion slots for each user-authored bus. Authored at edit time by the Occlusion Layout Builder; consumed at runtime by `AudioManager` to build per-bus slot pools.

One asset per project. Reference it from `AudioManager` ▸ Occlusion Layout.

#### Properties

```csharp
public AudioMixer VoiceMixer { get; }
public IReadOnlyList<BusEntry> Buses { get; }
```

`VoiceMixer` is the mixer the layout was generated against. At runtime, `AudioManager` refuses to build slot pools if its assigned `voiceMixer` doesn't match this — re-run the builder against the correct mixer.

#### Nested Classes

```csharp
[Serializable]
public sealed class BusEntry
{
    public AudioMixerGroup Bus;   // user-authored bus (e.g. Footsteps)
    public SlotEntry[] Slots;     // generated occlusion slots under this bus
}

[Serializable]
public sealed class SlotEntry
{
    public AudioMixerGroup MixerGroup;   // the slot's mixer group (voice's outputAudioMixerGroup)
    public string CutoffParam;           // exposed parameter name for the slot's Lowpass cutoff
    public SendEntry[] Sends;            // per-ReverbSendBus send params, indexed by registry order
}

[Serializable]
public sealed class SendEntry
{
    public string RoomId;          // ReverbSendBus.BusName key
    public string SendLevelParam;  // exposed mixer parameter name for the Send level (dB)
}
```

---

### OcclusionSlot

**Namespace:** `AudioSystem`
**Inheritance:** Plain `public sealed class` (not a Unity component)

One slot inside a per-bus pool. Voices borrow a slot at acquire time (`AudioManager.GetVoice(container)`) and return it on `AudioManager.ReturnVoice(voice)`. While held, the voice's `outputAudioMixerGroup` is set to the slot's mixer group and any subsystem can modulate muffle by writing to the slot's exposed parameters.

#### Fields

```csharp
public readonly AudioMixerGroup MixerGroup;            // voice routes through this
public readonly string CutoffParam;                    // exposed Lowpass cutoff (Hz)
public readonly int IndexInPool;                       // position inside the BusSlotPool
public readonly AudioMixerGroup OwningBus;             // the user-authored bus this slot belongs to
public readonly Dictionary<string, string> SendLevelParams;  // ReverbSendBus.BusName → exposed Send level param (dB)
```

All fields are `readonly` — the slot is immutable once authored. Modulating the slot at runtime means writing to the mixer via `voiceMixer.SetFloat(slot.CutoffParam, hz)` or `voiceMixer.SetFloat(slot.SendLevelParams[roomId], levelDb)`.

`SendLevelParams` is empty when no `ReverbSendBusRegistry` was assigned at layout-generation time.

---

### AudioManager Slot Pool API

The acquire/release surface lives on `AudioManager`. Most projects never call these directly — `AudioManager.GetVoice(container)` handles slot lifecycle automatically based on the container's opt-in flags. The public API is exposed for diagnostics and custom routing scenarios.

#### Inspector Fields

| Field                                  | Default | Description                                                                              |
|----------------------------------------|---------|------------------------------------------------------------------------------------------|
| `voiceMixer` (`VoiceMixer`)            | null    | The mixer asset hosting the occlusion slot groups. Must match `OcclusionLayout.VoiceMixer`. |
| `occlusionLayout` (`OcclusionLayout`)  | null    | The generated layout asset. Required for slot pools to build at startup.                 |
| `defaultOcclusionSlotsPerBus`          | 6       | Default slots-per-bus used by the Occlusion Layout Builder. Per-bus overrides live on the layout asset. |
| `occlusionRayCount` (range 1–5)        | 3       | Rays per voice per occlusion tick. 1 = binary, 3+ = partial occlusion via blocked fraction. |
| `occlusionRaySpreadMeters` (range 0–2) | 0.5     | Width of the side-ray spread at the source end (meters).                                 |
| `occlusionGainSmoothingSeconds`        | 0.06    | Tau for per-frame exponential smoothing of `GainStack.OcclusionGain`.                    |
| `occlusionCutoffSmoothingSeconds`      | 0.04    | Tau for per-frame exponential smoothing of the slot's mixer cutoff parameter.            |
| `occlusionUpdateInterval`              | 0.2     | Seconds between OcclusionUpdate ticks (raycast cadence). Per-frame smoothing handles the in-between. |
| `occlusionMask`                        | ~0      | Physics layer mask used by the occlusion raycast.                                        |

#### Properties

```csharp
public AudioMixer VoiceMixer { get; }
public OcclusionLayout OcclusionLayout { get; }
public int DefaultOcclusionSlotsPerBus { get; }
public bool OcclusionSlotPoolsReady { get; }
```

`OcclusionSlotPoolsReady` is `true` once `InitializeOcclusionSlotPools` ran successfully at `Awake` — i.e. both `voiceMixer` and `occlusionLayout` are assigned, they agree on which mixer the layout targets, and at least one bus entry produced a slot pool.

#### Methods

##### AcquireOcclusionSlot

```csharp
public OcclusionSlot AcquireOcclusionSlot(AudioMixerGroup bus)
```

Acquires a free slot from the pool owned by `bus`. Returns null if (a) no pool exists for the bus (the bus wasn't in the layout), (b) the slot pool isn't ready, or (c) all slots are in use (pool exhausted — voice plays with no low-pass and no reverb sends, though occlusion's volume drop still applies; a throttled warning is logged once per second per bus). This method logs nothing for cases (a) and (b); `GetVoice(container)` warns once per bus for case (a) when an Occlusion Layout is assigned, since the layout is then out of date.

Caller is responsible for calling `ReleaseOcclusionSlot` exactly once. The voice pool's `ReturnVoice` handles this for voices acquired via `GetVoice(container)`.

##### ReleaseOcclusionSlot

```csharp
public void ReleaseOcclusionSlot(OcclusionSlot slot)
```

Returns the slot to its pool. Resets the slot's cutoff to 22 kHz and silences all reverb-send parameters so the next voice on this slot starts clean. Safe to call with null.

##### GetOcclusionSlotsInUse / GetOcclusionSlotCapacity

```csharp
public int GetOcclusionSlotsInUse(AudioMixerGroup bus)
public int GetOcclusionSlotCapacity(AudioMixerGroup bus)
```

Diagnostic counters. Both return `-1` if no pool exists for the given bus.

**Example — runtime diagnostic HUD:**

```csharp
foreach (var bus in interestingBuses)
{
    int inUse = AudioManager.Instance.GetOcclusionSlotsInUse(bus);
    int cap   = AudioManager.Instance.GetOcclusionSlotCapacity(bus);
    GUI.Label(rect, $"{bus.name}: {inUse}/{cap} slots");
}
```

---

## Reverb Send Buses

The "wet path" complement to occlusion. Source-side: voices in a tagged zone send to a per-zone reverb bus. Listener-side: each bus's character is defined by its own `ReverbSendBus` asset (the single source of truth) and driven onto the mixer each tick — parametric buses via six exposed SFX Reverb params, convolution buses via a native impulse-response plugin (§15.14). See [Manual chapter 15](USER_GUIDE.md#15-reverb-send-buses--per-zone-reverb) for architecture and design rationale.

---

### ReverbSendBus

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Reverb Send Bus

One acoustic-space identity per asset. Owns one mixer group on the project's voice mixer, hosting one SFX Reverb effect with six exposed parameters that the listener-side driver writes per tick. The asset's `name` is the bus id.

#### Properties — Mixer Wiring

```csharp
public AudioMixer ReverbMixer { get; }           // mixer this bus lives on
public AudioMixerGroup ReverbBus { get; }        // mixer group this bus owns (auto-filled by Generate)
public AudioMixerGroup OutputDestination { get; } // optional cross-mixer output target
public AudioMixerGroup ParentGroup { get; }      // optional parent for the "ReverbsWet" return-bus convention
public string MixerGroupGuid { get; }            // hidden idempotency key for re-Generate rename-safety
```

#### Properties — Parametric Defaults

The 14 SFX Reverb parameters split into Basic and Advanced. Basic surfaces in the inspector by default; Advanced lives behind an EditorPrefs-backed foldout.

```csharp
// Basic (levels in millibels, mB, like Unity's SFX Reverb: 0 = full, -10000 = silent)
public float DryLevel { get; }       // -10000..0 mB. Default -10000 (wet-only).
public float Room { get; }           // -10000..0 mB. Default 0 (audible).
public float DecayTime { get; }      //   0.1..20 s. Default 1.49
public float ReverbLevel { get; }    // -10000..2000 mB. Default 200

// Advanced
public float RoomHF { get; }            // -10000..0 mB. Default 0
public float RoomLF { get; }            // -10000..0 mB. Default 0
public float DecayHFRatio { get; }      // 0.1..2 ratio. Default 0.83
public float ReflectionsLevel { get; }  // -10000..1000 mB. Default -2602
public float ReflectionsDelay { get; }  // 0..0.3 s. Default 0.007
public float ReverbDelay { get; }       // 0..0.1 s. Default 0.011
public float HFReference { get; }       // 1000..20000 Hz. Default 5000
public float LFReference { get; }       // 20..1000 Hz. Default 250
public float Diffusion { get; }         // 0..100 %. Default 100
public float Density { get; }           // 0..100 %. Default 100
```

Only the six listener-relevant parameters (`ReverbLevel`, `DecayTime`, `Room`, `RoomHF`, `DecayHFRatio`, `ReflectionsLevel`) are exposed to the runtime driver. The other eight describe bus identity and stay at the SO-authored snapshot defaults.

#### Properties — Convolution (Phase 3.2, Windows)

When `UseConvolution` is true the bus uses the native convolution plugin instead of SFX Reverb, and the parametric defaults above are ignored. See [Manual §15.14](USER_GUIDE.md#1514-convolution-reverb-windows-beta) for the authoring walkthrough and beta caveats.

```csharp
public bool UseConvolution { get; }          // false = parametric SFX Reverb; true = convolution plugin
public AudioClip ImpulseResponse { get; }    // the IR to convolve; must be CPU-readable (Decompress On Load / PCM)
public float ConvolutionWetTrimDb { get; }   // -80..+20 dB wet trim, 0 = unity
public int ConvolutionSlot { get; }          // index in the plugin's IR slot table; -1 = unassigned (hidden; Generate assigns it)
```

`ConvolutionSlot` is `[HideInInspector]` — `ReverbAutoCreator` assigns a stable free slot at Generate, and both the authored "IR Slot" effect param and the runtime IR upload (`AudioManager.UploadConvolutionIRs`) read it. Designers never set it.

#### Identity Helpers

```csharp
public string BusName { get; }                    // sanitized form of asset.name, safe for mixer param ids
public string ParamName(string suffix);           // "Reverb_<BusName>_<suffix>"

public const string SuffixWet           = "Wet";           // → ReverbLevel
public const string SuffixDecay         = "Decay";         // → DecayTime
public const string SuffixRoom          = "Room";          // → Room
public const string SuffixRoomHF        = "RoomHF";        // → RoomHF
public const string SuffixDecayHFRatio  = "DecayHFRatio";  // → DecayHFRatio
public const string SuffixReflections   = "Reflections";   // → ReflectionsLevel

public void GetDefaultProfile(AudioReverbProfile dest);   // non-allocating copy of SO defaults into dest
public static string SanitizeBusName(string raw);
```

`ParamName("Wet")` for a bus named "Bathroom" returns `"Reverb_Bathroom_Wet"` — that's the exposed mixer parameter the runtime driver writes to.

`GetDefaultProfile` is the **primary** (and only) source for `AudioManager.DriveReverbBusParams`: every parametric bus is driven from its own SO character each tick, unconditionally — no zone lookup. (Before the 2026-05-21 source-of-truth unification this was only a fallback "when no zone feeds the bus"; the zone-driven path was removed.) Convolution buses skip the driver entirely.

#### Editor Surface

```csharp
#if UNITY_EDITOR
public void EditorSetReverbBus(AudioMixerGroup group);
public void EditorSetOutputDestination(AudioMixerGroup group);
public void EditorSetMixerGroupGuid(string guid);
public void EditorSetConvolutionSlot(int slot);   // ReverbAutoCreator assigns the IR slot at Generate
#endif
```

Called by the per-asset inspector's Generate path. Runtime treats the SO as read-only.

---

### ReverbSendBusRegistry

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Menu:** Create > Audio System > Reverb Send Bus Registry

Flat list of every `ReverbSendBus` asset in the project. One per project. Reference it from `AudioManager` ▸ Reverb Send Bus Registry.

#### Properties

```csharp
public IReadOnlyList<ReverbSendBus> Buses { get; }
public int BusCount { get; }
```

#### Methods

```csharp
public ReverbSendBus FindByName(string busName);  // case-sensitive lookup by ReverbSendBus.BusName
```

#### Editor Surface

```csharp
#if UNITY_EDITOR
public void EditorSetBuses(List<ReverbSendBus> entries);
#endif
```

Called by the Reverb Send Buses window's Refresh button after re-scanning the project for `t:ReverbSendBus` assets.

---

### AudioReverbProfile

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `[Serializable] public class`

Reverb character parameters — six fields, each matching one of the listener-relevant SFX Reverb parameters. Authored on a `ReverbSendBus` SO (the source of truth) and copied into a non-allocating scratch buffer by the bus-params driver; also interpolatable across portal blend regions by the per-source ambient path.

#### Fields

```csharp
[Range(-10000f, 0f)]   public float Room             = -10000f;  // dB, overall wet gain
[Range(-10000f, 0f)]   public float RoomHF           = 0f;       // dB, HF tilt
[Range(0.1f, 20f)]     public float DecayTime        = 1.49f;    // seconds
[Range(0.1f, 2f)]      public float DecayHFRatio     = 0.83f;    // ratio
[Range(-10000f, 1000f)] public float ReflectionsLevel = -2602f;  // dB, early reflections
[Range(-10000f, 2000f)] public float ReverbLevel      = 200f;    // dB, late reverb tail
```

Default (`Room = -10000`) is **inaudible** — zones that don't author a profile produce zero behavior change. Set `Room` toward 0 dB to enable. This default is the profile's own: a new `ReverbSendBus` asset starts at `Room = 0` (audible).

#### Methods

```csharp
public static void Lerp(AudioReverbProfile a, AudioReverbProfile b, float t, AudioReverbProfile dest);
public void CopyFrom(AudioReverbProfile src);
public bool IsEffectivelySilent { get; }   // true when Room ≤ -9999 dB
public static readonly AudioReverbProfile Off;  // shared singleton, Room = -10000
```

`Lerp` and `CopyFrom` write into `dest` rather than returning a fresh instance, so the manager can blend across portal regions without per-frame allocation.

---

### AudioZone reverb fields

`AudioZone` (see [Propagation section](#audiozone)) has one reverb-relevant field — **routing only**:

```csharp
public AudioMixerGroup ReverbBus { get; }       // routing target (source-side); null when unassigned
```

- **`ReverbBus`** — the mixer group voices physically in this zone send to. Drag in a `ReverbSendBus`'s `ReverbBus` field. Leave null for zones with no dedicated reverb (the source-side send is skipped for sources in that zone). Returns null at runtime when explicitly left empty.

> **Removed 2026-05-21.** `AudioZone.ReverbProfile` (the per-zone character field) was deleted in the source-of-truth unification. A bus's character now lives entirely on its `ReverbSendBus` SO; the zone only routes. The `entryFadeMeters` field (general propagation, see the AudioZone inspector table) still governs how the send fades across the zone boundary.

Multi-zone-to-bus aggregation is supported — several zones can share a `ReverbBus`. Because character is per-bus now, all zones feeding a given bus produce that bus's single SO character (no per-zone variation on a shared bus).

---

### AudioManager Reverb-Sends API

#### Inspector Fields

| Field                                                    | Default | Description                                                                                  |
|----------------------------------------------------------|---------|----------------------------------------------------------------------------------------------|
| `reverbSendBusRegistry` (`ReverbSendBusRegistry`)        | null    | Project's bus registry. Reverb-sends driver is gated on this being assigned and non-empty.   |
| `reverbSendsUpdateInterval` (range 0.005–0.5)            | 0.05    | Seconds between reverb-sends + bus-params driver ticks (default ≈ 20 Hz).                    |

#### Property

```csharp
public ReverbSendBusRegistry ReverbSendBusRegistry { get; }
```

#### Synchronous Send Write

```csharp
public void WriteReverbSendsImmediate(AudioVoiceEnhanced voice, AudioContainer container);
```

Synchronous per-voice send-level write, called by `AudioContainer.ConfigureAudioSource` right after the source has been positioned and routed. Ensures a voice's first audio frame already carries correct reverb routing, without waiting for the next driver tick (≈ 50 ms latency).

Safe to call when the registry is empty / mixer missing / voice has no slot / container opts out — the internal guards short-circuit and the call becomes a no-op.

Most projects never call this directly — `AudioContainer.ConfigureAudioSource` does. Surfaced publicly for custom playback paths that bypass the standard `Play()` route.

---

## Enums & Data Structures

### VoicePriority
```csharp
public enum VoicePriority
{
    Low = 0,       // Least important: weighs 1/8
    Medium = 64,   // Standard priority: weighs 1/4
    High = 128,    // Important sounds: weighs 1/2
    Critical = 255 // The strongest weight, 1: last to go, but still stealable (Persistent is the exemption)
}
```

A voice's importance (`AudioVoice.GetImportance`) is how loud it is where it is heard times its priority's weight: priority ÷ 255, but at least 1/8, so a Low sound's distance and loudness count (since 3.1.0-dev.8; Low weighed 0, and always went first).

---

### AudioSpatialMode
```csharp
public enum AudioSpatialMode
{
    Unity,   // Unity's 3D from its AudioListener (default, as before 3.1)
    Pepper   // Pepper pans and attenuates from every listening point, weighted
}
```
`AudioManager.SpatialMode` (3.1.0-dev.8); see [Listening Point Methods](#listening-point-methods).

---

### VoiceStealBehavior
```csharp
public enum VoiceStealBehavior
{
    Oldest,          // Steal longest-playing voice
    Quietest,        // Steal quietest voice
    Furthest,        // Steal most distant voice
    LowestPriority   // Steal by priority only
}
```

---

### MultiPositionType
```csharp
public enum MultiPositionType
{
    SingleEmitter,  // Single voice, dynamically positioned at dominant emitter
    Decorrelated    // All emitters play with phase decorrelation
}
```

---

### WeightedAudioClip
```csharp
[Serializable]
public class WeightedAudioClip
{
    public AudioClip clip;
    public float weight = 1f;           // Selection probability (0-10)
    public float volumeMultiplier = 1f; // Per-clip volume (0-1)
}
```

---

### SequenceEntry
```csharp
[Serializable]
public class SequenceEntry
{
    public AudioClip clip;
    public float volumeMultiplier = 1f;  // Per-clip volume
    public bool loop = false;             // Loop this clip
    public float delayAfter = 0f;         // Delay before next (0-5s)
}
```

---

### SwitchEntry
```csharp
[Serializable]
public class SwitchEntry
{
    public string switchValue = "";     // Value to match
    public AudioContainer container;     // Container to play
}
```

### ParameterSource
```csharp
public enum ParameterSource { Game, Distance, ApproachSpeed, Angle }   // AudioParameter.ValueFrom
```

### AudioRandomSource
```csharp
public enum AudioRandomSource { Pepper, Unity }   // AudioManager.RandomNumbers
```

### SwitchChangeMode, SwitchRange
```csharp
public enum SwitchChangeMode { NextPlayOnly, Crossfade, Restart }   // SwitchContainer.WhenSwitchChanges

[Serializable]
public struct SwitchRange                // AudioSwitchGroup.Ranges
{
    public string value;                 // the switch value in this range
    public float from;                   // where it starts, in the driving parameter's unit
    public SwitchRange(string value, float from)
}
```

---

### BlendEntry
```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;                                    // Layer container
    public AnimationCurve volumeCurve = AnimationCurve.Linear(0,0,1,1); // VolumeCurves layout: value -> level (0-1).
                                                                        // X: 0-1 for a named parameter, the
                                                                        // asset's unit for a Parameter asset.
    public float rootValue;                     // the value it was recorded at (pitch follow, Auto Layout); 0 = none
    public float regionMin, regionMax;          // Regions layout: where it plays
    public float fadeInWidth, fadeOutWidth;     // Regions layout: fade lengths inside the region
    public bool followPitch = true;             // pitch follow applies to this layer
    public float followRangeSemitones = 12f;    // pitch follow's limit, ±semitones
    public List<ParameterCurve> curves;         // curves for this layer only
}
```

---

### ParameterCurve, CurveProperty
```csharp
[Serializable]
public sealed class ParameterCurve
{
    public AudioParameter parameter;   // X axis; wins over parameterName
    public string parameterName;       // X axis by name (not clamped; 0 when unset)
    public CurveProperty property;     // VolumeDb, PitchCents, LowPassHz, HighPassHz
    public AnimationCurve curve;       // X: the parameter's unit; Y: dB, cents or Hz
    public float Evaluate(float x);    // filters: straight in octaves between keys
}

public enum CurveProperty { VolumeDb, PitchCents, LowPassHz, HighPassHz }
```
On `AudioContainer.ParameterCurves` (every sound below that container) or a `BlendEntry`'s `curves` (that layer). Read in the play's scopes every frame. On one container, and down the tree: dB and cents add (into `GainStack.CurveGain` and `PitchStack.Curve`), the lowest low-pass and the highest high-pass win. Filter curves switch on the `AudioLowPassFilter` / `AudioHighPassFilter` every pooled voice has had, switched off, since the pool was built, ease the cutoff over about 20 ms (set at once before the first sample), and switch it off when fully open (22000 Hz / 10 Hz).

### BlendLayout, CrossfadeShape, BlendLoopMode, SilentLayerBehaviour
```csharp
public enum BlendLayout { VolumeCurves, Regions }
public enum CrossfadeShape { Linear, EqualPower, SCurve }  // -6 dB, -3 dB, eased; each pair complementary
public enum BlendLoopMode { FromLayers, ForceLoop, ForceOneShot }   // BlendContainer.LoopMode
public enum SilentLayerBehaviour { KeepPlaying, Virtualize }        // BlendContainer.SilentLayers
```

---

### SlewMode
```csharp
public enum SlewMode
{
    None, // heard at once
    Rate, // fixed speed in units per second, rising and falling
    Ease  // closes 95% of the gap in a set time, rising and falling
}
```
See [ParameterSlew](#parameterslew).

---

### AudioUpdateMode
```csharp
public enum AudioUpdateMode
{
    Automatic, // Pepper's per-frame step runs after every LateUpdate, before the audio is mixed
    Manual     // it runs only when you call AudioManager.Tick()
}
```

---

### AudioStatistics
```csharp
public class AudioStatistics
{
    public int activeVoices;            // real voices now
    public int availableVoices;         // free in the pool
    public int totalVoices;             // Max Real + Max Virtual
    public int liveRealVoices;          // every pooled AudioSource alive: playing, virtual or free
    public int realVoiceBudget;         // Max Real Voices
    public int poolSize;                // Max Real + Max Virtual + Steal Headroom: what liveRealVoices stays at
    public int voicesLostToDestruction;
    public int activeLoops;
    public int registeredContainers;
}
```

**Usage:**
```csharp
var stats = AudioManager.Instance.GetStatistics();
Debug.Log($"Real voices: {stats.activeVoices}/{stats.realVoiceBudget}, pool {stats.liveRealVoices}/{stats.poolSize}");
```

---

### AudioVoiceDebugInfo
```csharp
[Serializable]
public class AudioVoiceDebugInfo
{
    public int id;
    public string containerName;
    public string eventName;
    public string clipName;
    public string busName;
    public bool isPlaying;
    public bool isVirtual;
    public bool isLooping;
    public float volume;
    public float volumeDb;
    public float pitch;
    public float time;
    public float length;
    public float playTime;
    public bool is3D;
    public float distance;
    public float distanceToListener;
    public Vector3 position;
    public AudioVoice voice;
    public int priority;
    public int playId;                          // one number per play: all of an engine's layers share it
    public bool persistent;
    public VoiceVirtualReason virtualReasonKind;  // None while real
    public string virtualReason;                // its words, plus ", fading" while it fades down
    public float levelDb;                       // its whole gain stack, in dB: Pepper's gain, before Unity's own 3D fade and the mixer ("Gain dB" in the windows)
    public float pitchSemitones;                // from the recording
    public string region;                       // a Regions Blend layer's region and where the value is
    public int listeningPoint;                  // its play's point, an index into ListeningPoints (-1: none) (3.1.0-dev.8)
    public bool pepperPanned;                   // Spatial Mode = Pepper pans it
    public float spatialGainDb;                 // Pepper's level from the points, dB
    public float pan;                           // Pepper's pan, -1 left … 1 right
}

public enum VoiceVirtualReason
{
    None, OverTheCap, SilentLayer, PastMaxDistance, HeldByCode, BusPaused, EmitterPaused, Stopping
}

public sealed class AudioPlayDebugInfo           // AudioManager.GetPlaysDebug
{
    public int playId;
    public string eventName, containerName, busName;
    public AudioEmitter emitter;                 // null: posted without one
    public AudioHandle handle;                   // its own values: handle.ParameterCount and the listing beside it
    public bool persistent, isVirtual;
    public List<AudioVoiceDebugInfo> voices;
}
```

**Why a voice is virtual** (`VoiceVirtualReason`, checked in this order): **Stopping** (it ends once faded down), **BusPaused** (its bus or a parent is paused), **EmitterPaused** (its emitter is switched off with On Disable = Pause), **HeldByCode** (`AudioVoice.MakeVirtual`), **PastMaxDistance** (its Attenuation asset's), **SilentLayer** (a Virtualize Blend's layer below its threshold), else **OverTheCap** (more would be heard than Max Real Voices, and its play is among the least important).

**Usage:**
```csharp
var voices = AudioManager.Instance.GetActiveVoicesDebug();
foreach (var v in voices)
    Debug.Log($"{v.eventName}: {v.clipName} {v.levelDb:F1} dB {(v.isVirtual ? "virtual: " + v.virtualReason : "real")}");
```

---

## Loading

**Namespaces:** `AudioSystem.Loading` (always available) · `AudioSystem.Loading.Addressables` (gated by `com.unity.addressables`)

The clip-loading layer: import policy, warming, and the optional Addressables backend. See the [User Guide → Audio Loading & Memory](USER_GUIDE.md#16-audio-loading--memory) for the concepts and setup.

### Always available (SFXSystem.Runtime)

```csharp
// The seam every container resolves clips through. Default = DirectClipSource.
interface IAudioClipSource {
    bool TryGetClip(in AudioClipRef clipRef, out AudioClip clip); // hot path; false = cold → drop
    bool SupportsAddressables { get; }   // false on Direct — tells a real misconfig from a cold-drop
    void Pin(string address);            // hold an addressable clip resident (persistent holders); no-op on Direct
    void Unpin(string address);
}

// Optional companion an addressable source also implements (Direct does not).
interface IVoiceLifecycleListener {
    void OnVoiceBound(AudioVoice voice);     // source.clip just assigned → voice refcount++
    void OnVoiceReleased(AudioVoice voice);  // voice returning to pool → voice refcount--
}

readonly struct AudioClipRef {
    AudioClip DirectClip; string Address;    // an entry is EITHER a direct clip OR an address
                                             // Address = the Addressables address (what a bank label resolves to),
                                             // not the asset GUID unless the address IS the GUID
    bool HasAddress; bool IsEmpty;
    AudioClipRef(AudioClip clip);
    AudioClipRef(AudioClip clip, string address);
}

static class AudioClipSources {
    static IAudioClipSource Active { get; set; }          // default DirectClipSource; assigning null reverts to it
    static IVoiceLifecycleListener VoiceListener { get; } // cached; the active source's listener, if any
    // Resolve + diagnose: warns once-per-address when an address can't resolve for lack of a backend
    // (vs an expected cold-drop). Containers / AmbientSource use this instead of Active.TryGetClip.
    static bool TryResolve(in AudioClipRef clipRef, Object context, out AudioClip clip);
}

// Pre-load clip audio data ahead of play (coroutines). No Addressables dependency.
static class AudioClipWarmer {
    static bool IsWarm(AudioClip);
    static IEnumerator Warm(AudioClip);
    static IEnumerator WarmAll(IEnumerable<AudioClip>);    // parallel
    static bool Unload(AudioClip);                         // blunt — no live-voice check (Addressables handles that)
}

// AudioManager convenience hosts (it's the persistent coroutine runner). Return the coroutine.
Coroutine AudioManager.WarmClip(AudioClip);
Coroutine AudioManager.WarmClips(IEnumerable<AudioClip>);
Coroutine AudioManager.WarmContainer(AudioContainer);      // uses EnumerateClips()

// Every clip a container could play (cycle-safe; recurses Switch/Blend children):
IEnumerable<AudioClip> AudioContainer.EnumerateClips();

// Optional addressable key per entry (empty for direct entries):
class WeightedAudioClip { AudioClip clip; float weight; float volumeMultiplier; string address; } // Random
class SequenceEntry     { AudioClip clip; …; string address; }                                    // Sequence
class RoutingEntry      { AudioClip clip; string address; }                                        // Routing

// AmbientSource: addressable loop clip — pins the clip on the active source for its active life
// (and moves the pin if the active source changes, e.g. the Addressables backend starting later).
string   AmbientSource.LoopingClipAddress;   // serialized field; Inspector: "Looping Clip Address"
AudioClip AmbientSource.PlayableClip;         // resolved clip; null while an addressable clip is loading

// Raised on genuine listener zone-membership change (the addressable bank binder subscribes).
// Always paired one-to-one: an Exit is also raised when ResyncListenerAfterTeleport leaves a zone,
// when a zone the listener is in is disabled/destroyed/unloaded, and when the manager itself is destroyed.
static event Action<AudioZone> PropagationManager.ListenerEnteredZone;
static event Action<AudioZone> PropagationManager.ListenerExitedZone;
```

### Gated by com.unity.addressables

Namespace `AudioSystem.Loading.Addressables`; assembly `SFXSystem.Addressables` (compiles only when the package is present).

```csharp
// Refcounted addressable clip source. Installed as AudioClipSources.Active when AddressableAudioRuntime starts the backend.
sealed class AddressableClipSource : IAudioClipSource, IVoiceLifecycleListener {
    float LingerSeconds;                    // keep-warm after refcount 0 (default 5)
    void BankAcquire(string address);       // called by AudioBank
    void BankRelease(string address);
    void Tick(float unscaledNow);           // linger sweep (driven by AddressableAudioRuntime)
    void ReleaseAll();                       // teardown hard-reset (truth is zero)
    void GetDebugEntries(List<DebugEntry>);  // overlay readout
}

// A bank = an Addressables label. Preload loads/refcounts every clip under it; Release drops them.
sealed class AudioBank {
    AudioBank(string label, AddressableClipSource source);
    string Label; bool IsLoaded;
    void Preload();   // idempotent (refcounted per bank; survives re-entered zones)
    void Release();
}

// MonoBehaviours (Add Component ▸ Audio ▸ Loading):
ZoneAudioBank            // on an AudioZone GameObject; holds the bank label (BankLabel)
AddressableAudioRuntime  // starts the backend: installs the source, binds zone events, ticks linger.
                         // The backend persists across scene loads (own hidden DontDestroyOnLoad object);
                         // one runtime is enough — later ones (other scenes) log and do nothing.
```

**Refcount model:** `resident(address) = (banks holding it) + (live voices bound to it) + (pins) > 0`. Bank `Release`, `OnVoiceReleased`, and `Unpin` each decrement; the real unload fires at 0, after `LingerSeconds`. A clip a bank or pin loads also has its audio data loaded, so its first play doesn't hitch. Teardown (`ReleaseAll`) force-releases everything regardless of count; it runs only when the backend is destroyed (application quit / leaving Play Mode), not on scene loads.

**Cold-post policy (v1):** a not-yet-resident addressable clip makes `TryGetClip` return false → the play is dropped (like a null clip, but without the missing-clip warning). A Sequence container stays on a cold entry, so its next play tries that entry again; an Auto Advance run ends there. Bank preload is the warm path; there is no defer-and-play-late in v1.

**Editor tooling:** **Window ▸ Audio System ▸ Addressable Audio Authoring** (Convert/Revert containers; gated assembly `SFXSystem.Addressables.Editor`) and **Window ▸ Audio System ▸ Loading Validator** (always-on; runs via reflection even without the package).

---

## Netcode (Multiplayer)

**Namespace:** `AudioSystem.Netcode`
**Assemblies:** `SFXSystem.Netcode` (core, references Runtime only) · `SFXSystem.Netcode.Ngo` / `.FishNet` (adapters, auto-gated on their package)

Backend-agnostic networked audio. See the [User Guide → Multiplayer (Netcode)](USER_GUIDE.md#17-multiplayer-netcode) for concepts, setup, and the mental model. No audio crosses the wire — only a ~36-byte descriptor.

### NetworkedAudioManager

The static gameplay façade.

| Method | Description |
|---|---|
| `AudioHandle PostNetworkedEvent(string eventName, Vector3 position, GameObject emitter = null, bool? independentVariation = null)` | One-shot: plays locally now (echo) + relays to other peers. Returns the **local** handle. `emitter` attaches it to a moving networked object (null = static). `independentVariation`: `null` = the event's authored default; `true`/`false` = override. Warns + plays local-only if the event has no stable ID. |
| `AudioHandle PostNetworkedEvent(int stableId, …)` | Same, by stable ID. |
| `NetworkedSoundHandle PostNetworkedLoop(string eventName, Vector3 position, GameObject emitter = null, bool? independentVariation = null)` | Starts a **shared persistent (loop)** sound tracked for stop, culling + late-join (phase per the event's Loop Sync Tier). Returns a handle carrying the shared instance ID + the local echo. On a dedicated server, warns and redirects to `PostServerLoop`. |
| `void StopNetworked(NetworkedSoundHandle handle)` | Stops the local echo now **and** broadcasts the stop (scope `All` for server-authored, else `Others`). Safe on a null / pure-local handle. |
| `AudioHandle PostServerEvent(string / int, …)` | **Server-authored** one-shot to everyone (scope `All`), no local echo. Warns + no-ops on a client. Off-session → plain local play. |
| `NetworkedSoundHandle PostServerLoop(string eventName, …)` | Server-authored loop to everyone; handle's `Local` is null, `ServerAuthored` is true. Tracked for late-join. |
| `AudioHandle PostNetworkedScheduled(string / int, Vector3 position, double networkTime, …)` | Fire at a shared **network time**; returns the scheduled local echo. Warns if the event name/id is unknown. |
| `void PostServerScheduled(string / int, …, double networkTime, …)` | Server-authored scheduled one-shot (scope `All`). |
| `AudioHandle PostNetworkedQuantized(string, Vector3, Quantize, …)` | Schedule on the next beat/bar of the shared musical grid. Requires a transport (`SetTempo`); warns + returns null otherwise. |
| `void PostServerQuantized(string, Vector3, Quantize, …)` | Server-authored quantized cue. |
| `void ResyncClock()` | Force the network clock to hard-re-sample its offset next tick. |

### NetworkedSoundHandle

| Member | Description |
|---|---|
| `ulong InstanceId` | Shared cross-client identity, `(ownerClientId << 32 | counter)`. `0` = not a networked instance. Server-authored loops use a reserved `0xFFFFFFFF` owner namespace. |
| `AudioHandle Local` | The instigator's local-echo handle. Null off-session and for server-authored / Tier-2 loops. |
| `bool IsValid` | True when `InstanceId != 0`. |
| `bool ServerAuthored` | True for `PostServerLoop` — selects the stop scope (`All`). |

### NetworkMusicTransport & NetworkClock

`NetworkMusicTransport` (static) — shared tempo/phase:

| Member | Description |
|---|---|
| `void SetTempo(double bpm, int beatsPerBar)` | **Server only.** Author the transport; networked on change, replayed to late joiners. A mid-session change re-anchors seamlessly at the next bar. |
| `bool IsSet` · `double Bpm` · `int BeatsPerBar` · `double PhaseAnchor` | Current grid state (`PhaseAnchor` = network time of beat 0). |
| `double NextBeatNetworkTime(double from)` / `NextBarNetworkTime(double from)` | The next grid slot ≥ `from`, in network time. |

`NetworkClock` (static) — network-time ↔ local dsp:

| Member | Description |
|---|---|
| `double NetworkTime` | This machine's current network time (synced server time; local dsp off-session). |
| `double NetworkToLocalDsp(double t)` | Map a network-time target to local `AudioSettings.dspTime` for `PlayScheduled`. |
| `void ResyncClock()` | Force a hard re-sample of the offset. |

### NetworkAudioSettings

MonoBehaviour — scene config + game-supplied hooks.

| Member | Description |
|---|---|
| `bool ProximityCulling` | Opt into culling. Default off. |
| `Func<ulong, Vector3?> ListenerPositionProvider` | Game-supplied per-client listener position (server calls it). Null → culling inert. Assigned in code. |
| `NetworkAudioValidator ServerValidator` | Server-side authority gate. Null → allow-all. Assigned in code. |
| `float DefaultAudibleRadius` · `float LoopReevalSeconds` | Culling fallback radius + loop re-eval rate. |
| clock/lead tunables | `ClockSmoothing`, `ClockResyncThreshold`, `ScheduleLeadFloor`, `ScheduleLeadRttFactor`. |

### NetworkAudioEvent & NetworkAudioScope

`NetworkAudioEvent` (struct) — the wire payload:

| Field | Description |
|---|---|
| `int EventId` | The `AudioEvent.StableId` to play. |
| `Vector3 Position` | World position — also the emitter fallback/anchor. |
| `ulong EmitterNetId` | Optional network-object id to attach to. `0` = static. |
| `ulong InstanceId` | Shared identity for a persistent/loop sound. `0` = a one-shot. |
| `int Seed` | Variation seed. Non-zero forces the SAME variant on every client; `0` = independent. |

Plus `Pack` / `Unpack` — the canonical **36-byte** wire format (round-trip covered by `NetworkAudioEventPackTests`).

`NetworkAudioScope` (enum): `Others` (default — everyone but the sender) · `All` (server-authored) · `Owner` *(reserved)* · `Proximity` *(reserved enum value; culling is realised on the `Others`/`All` paths as "don't send")*. Passing an unhonored scope logs a warning and relays as `Others`.

**Per-event authoring** (`AudioEvent`, Multiplayer foldout, shown only with a backend installed): `Unreliable Delivery`, `Independent Variation (default)`, `Loop Sync Tier` (0/1/2; Tier 2 is **experimental** — see [User Guide §17.4](USER_GUIDE.md#174-local-vs-networked--the-mental-model)). For the `INetworkAudioTransport` boundary — implementing a new backend — see [User Guide §17.9](USER_GUIDE.md#179-extending-writing-a-new-backend-adapter).

---

## Quick Reference

### Common Patterns

**Play Sound:**
```csharp
myEvent.Post();
myEvent.Post(gameObject, transform.position);
```

**Control:**
```csharp
AudioHandle h = myEvent.Post();
h.SetVolumeScale(0.5f);
h.SetPitchOffset(2f);
h.Stop(1f);
```

**Bus:**
```csharp
AudioManager.Instance.SetBusVolume("Music", -6f, 1f);
```

**State:**
```csharp
AudioManager.Instance.SetState("Underwater", 1.5f);
```

**Switch:**
```csharp
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
```

**RTPC:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", 0.75f);
AudioManager.Instance.TransitionRTPC("CombatIntensity", 1.0f, 2f);
```

**Global Controls:**
```csharp
AudioManager.Instance.StopAllSounds();
AudioManager.Instance.PauseAll();
AudioManager.Instance.UnpauseAll();
AudioManager.Instance.MuteAll(true);
```

**Multi-Position:**
```csharp
AudioMultiHandle mh = myEvent.PostMultiPosition(emitterParent);
mh.SetVoiceVolumeScale(0, 0.5f);  // Per-emitter control
mh.UpdatePositions(newTransforms);
```

---

## Version History

**v3.1.0** - November 2026
- Released 14 November 2026 (the free version; the full version follows on the Unity Asset Store), after dev snapshots `3.1.0-dev.1` to `3.1.0-dev.12`. See `CHANGELOG.md` at the package root and "Changes since 3.0.0" in `BETA_KNOWN_ISSUES.md` for what changed.
- **Scoped parameters:** [AudioParameter](#audioparameter) assets, [AudioEmitter](#audioemitter), per-play values on `AudioHandle`, global `SetParameter`/`RampParameter`, `AudioEvent.Post(AudioEmitter)`, `AudioManager.Post`, and `AudioManager.UpdateMode`/`Tick`. The string RTPC API is the global scope. Blend Containers read their parameter per play, once a frame; `BlendContainer.UpdateBlend` is obsolete.
- **Parameter curves, regions and pitch follow:** [ParameterCurve](#parametercurve-curveproperty) (volume dB, pitch cents, low- and high-pass Hz) on any container and Blend entry; Blend `Layout` (Volume Curves or Regions), `Crossfade` shape and `AutoLayout`; `PitchFollowParameter` with per-entry `rootValue`, `followPitch` and `followRangeSemitones`; `GainStack.CurveGain`; per-voice filters for filter curves.
- **Nested containers:** every play keeps a tree of the containers it reached, so nested Blends read their parameters per play and late voices get their Blend's curve and Volume. New `AudioContainer.UsesParentPositioning` / `UsesParentOutput`.
- **Listening points:** `AudioManager.AddListeningPoint` / `RemoveListeningPoint` / `ListeningPoints` / `DriveUnityListener`, [AudioListeningPoint](#audiolisteningpoint), and `ListenerUtil.TryGetPosition`. With a point registered the scene is never searched; without one, the search warns once and runs at most once a frame.
- **Parameter smoothing:** [ParameterSlew](#parameterslew) (None, Rate, Ease with separate rise and fall) on `AudioParameter`, per-emitter and per-play overrides, `SnapParameter` / `SnapAllParameters`, and the Bypass Slew While parameter.
- **Virtual voices keep their place:** positions in samples on the audio clock at the voice's pitch (`AudioVoice.VirtualSamples`), a short fade around virtualizing and realizing (`GainStack.RealizeGain`, `GetGainWithoutRealize`), and `AudioContainer.DopplerLevel`.
- **The play is the unit of the voice pool:** whole-play steals after making room (`AudioContainer.EstimateVoiceCount`), plays real or virtual as a whole with stickiness, one start time per play, `BlendContainer.LoopMode` / `SilentLayers` / `SilentThresholdDb` (`Loop` obsolete), `AudioEvent.Persistent`, Unity voice-limit warnings and `AudioSource.priority`.
- **No forced multiplayer code:** the `SFXSYSTEM_DISABLE_NETCODE` define leaves the netcode assemblies out; `PropagationManager` hears from the registered listening point.
- **Named emitter points:** [AudioEmitter](#audioemitter) `Points`, `Post(event, "Exhaust")` / `Post(event, number)`, `FindPoint`, `SetPoint`, `GetPointOffset` / `GetPointPosition`, `EmitterPoint`; `Velocity`, `SetVelocity`, `ClearVelocity`; `AudioEvent.Post(AudioEmitter, point)` and `AudioManager.Post(event, emitter, point)`.
- **Switches per sound, following while they play:** [AudioSwitchGroup](#audioswitchgroup) assets (names still work), with `DrivenBy`, `Ranges` and `Hysteresis`; `SetSwitch` / `ClearSwitch` / `TryGetSwitch` / `GetEffectiveSwitch` on `AudioEmitter` and `AudioHandle`; `AudioManager.SetSwitch(AudioSwitchGroup, value)`, `ClearSwitch`, `GetSwitch(AudioSwitchGroup)`; `SwitchContainer.SwitchGroup`, `WhenSwitchChanges` and `CrossfadeTime`; `GainStack.SwitchGain`.
- **Distance assets and built-in values:** [AudioAttenuation](#audioattenuation) (volume, low-pass, high-pass and spread over metres, Max Distance), `AudioContainer.Attenuation`, `ContainerOverrides.attenuation`, `AudioHandle.SetAttenuation` / `ClearAttenuation`; `AudioParameter.ValueFrom` (`ParameterSource`: Distance, ApproachSpeed, Angle) and `IsBuiltIn`. `AudioContainer.MaxDistance`'s setter writes a runtime override.
- **Audition outside Play mode:** Window > Audio System > Audition and [AudioAudition](#audioauditionwindow-and-audioaudition) (`AuditionLayer`, `AuditionInputs`): play a container tree with parameter sliders, switch values, a sweep and a layer readout while editing it.
- **One-shots per emitter:** `AudioEvent.MaxInstancesPerEmitter` / `CooldownPerEmitter`; [PostValues](#postvalues) and the `Post` overloads that take it (event, emitter, manager); `AudioManager.RandomNumbers` (`AudioRandomSource`: Pepper's own streams by default, or Unity); `AudioEmitter.RandomSeed` / `SetRandomSeed`; Random containers' Avoid Repeat Last per emitter.
- **Bus property curves:** `AudioBus.ParameterCurves` (Volume, Low Pass, High Pass on global values), `LowPassParameter` / `HighPassParameter` (exposed mixer cutoffs), `CurveVolumeGain`, `LowPassHz`, `HighPassHz`; the bus inspector's Add Low Pass / High Pass buttons.
- **Lifecycle, teardown and pause (3.1.0-dev.5):** `AudioManager.PersistAcrossScenes` and Pepper's own "AudioVoices" root (switching the manager off changes nothing); `AudioManager.Inert` for dedicated servers; `AudioEmitter.WhenDisabled` (`EmitterDisableMode`: Keep, Stop, Pause) and `StopFadeSeconds`; `EventAction.scope` (`ActionScope`: All, ThisEmitter); `AudioBus.Pause` / `Resume` / `IsPaused` / `WhilePaused` (`BusPauseBehaviour`); `AudioEvent.TimeMode` and `AudioBus.DuckingTimeMode` (`AudioTimeMode`: Scaled, Unscaled). Finished handles ignore new `OnStarted` / `OnStopping` / `OnLoop` subscriptions.
- **Tools (3.1.0-dev.6):** the Audition plays events too, runs any number of sweeps (`SweepMode`), replays recordings and shows written values ([AudioAudition](#audioauditionwindow-and-audioaudition)); the [Live View](#audioliveviewwindow-audioemitterrecorder-and-audioparameterrecording) with `AudioManager.GetPlaysDebug`, `AudioPlayDebugInfo`, `VoiceVirtualReason`, the new `AudioVoiceDebugInfo` fields, `AudioHandle`'s value listing and `AudioEmitter.AllEnabled`; `AudioEmitterRecorder` and `AudioParameterRecording`; [Loop Analysis](#loopanalysiswindow); the [Engine Builder](#enginebuilderwindow); the [Container Validator](#audiocontainervalidator-container-validator)'s project-wide rules and `ValidateEvent`; [folder import rules](#audio-import-settings-and-folder-rules) (no settings asset ships in the package); `AudioStatistics.poolSize`; undoable bus faders.
- **Routing, mix and positions (3.1.0-dev.7):** offsets that stick: `AudioEmitter.VolumeOffsetDb` / `PitchOffsetCents`, `AudioHandle.SetVolumeOffset` / `VolumeOffsetDb` / `VolumeScale` / `PitchOffset`, `PostValues.WithVolumeOffset` / `WithPitchOffset` / `VolumeOffset`, `GainStack.EmitterGain`, `PitchStack.Emitter`, and a handle's `SetVolumeScale` / `SetPitchOffset` reaching voices it starts later; posting anywhere on an emitter (`AudioEmitter.Post(evt, localOffset, values)`, `AudioEvent.PostAtOffset`, `AudioManager.Post(evt, emitter, localOffset, values)`) and moving a playing sound (`AudioHandle.SetLocalOffset` / `SetWorldPosition`); timed posts (`Post(emitter, startDsp, startOffsetSeconds, values)` on the event, emitter and manager) and `AudioEvent.EstimateDuration` (`AudioDuration`); Pepper's Doppler (`AudioContainer.Doppler`, `DopplerExaggeration`, `DopplerMaxCents`, `DopplerMode`, `DopplerMath`), `AudioEmitter.Teleport`, `AudioListeningPoint.ClearVelocity`; ducks combining by the deepest (`AudioBus.ApplyDucking`); `AudioState.VolumeMode` (`StateVolumeMode`: SetLevel, Offset), `AudioManager.ClearState`, `AudioBus.StateOffsetDb` / `PlayerVolumeDb` / `IsMuted`, `AudioManager.MasterVolumeDb` / `SetMasterVolumeDb` / `IsAllMuted`; `SetBusVolume` with an unknown name changes nothing.
- **Many cars and split screen (3.1.0-dev.8):** `AudioManager.SpatialMode` ([AudioSpatialMode](#audiospatialmode): Unity, Pepper), Pepper panning and attenuating 3D sounds from every listening point by its `Weight` and `PanWidth` (`GainStack.SpatialGain`), reproducing Unity's 3D at one point; `AudioManager.SetPrimaryListeningPoint` / `PrimaryListeningPoint`, `AudioListeningPoint.IsPrimary`; a point whose transform is destroyed is removed; each play measured from its own point, gliding over 100 ms when it changes; `AudioBus.MaxRealVoices` / `RealVoiceCount`; `AudioManager.StealFadeSeconds` (steals fade); Low priority weighs 1/8 in importance; every pooled voice carries its low- and high-pass from the start; `AudioVoiceDebugInfo.listeningPoint` / `pepperPanned` / `spatialGainDb` / `pan`.

**v3.0.0** - July 2026
- **New subsystem: Multiplayer (Netcode)** — backend-agnostic networked audio. Post a sound on one machine and every player hears it; no audio crosses the wire, only a ~36-byte descriptor each machine plays locally. Install one netcode stack and its adapter lights up automatically; a backend you didn't install never compiles. See [Manual chapter 17](USER_GUIDE.md#17-multiplayer-netcode) and [API → Netcode (Multiplayer)](#netcode-multiplayer).
  - `NetworkedAudioManager` façade — `PostNetworkedEvent` / `PostNetworkedLoop` / `StopNetworked`, server-authored `PostServer*`, scheduled/quantized cues
  - Emitter binding (sound follows a moving `NetworkObject`), shared loop lifetime + late-join snapshot, cross-client variation parity (seed on the wire)
  - Competitive hardening — server authority/validation, proximity culling (= don't-send), per-event `Unreliable Delivery`
  - Musical sync — `NetworkClock` (network-time ↔ local dsp) + `NetworkMusicTransport` (tempo/phase), loop phase-lock tiers (Tier 2 **experimental** — not for production yet)
  - `NetworkAudioSettings` scene component; **Network Audio Debug** window; per-event **Multiplayer** foldout on `AudioEvent`
  - Adapters: NGO 2.x (`SFXSYSTEM_NGO`) and FishNet v4 (`FISHNET`). *Verified on FishNet, one PC and two PCs; NGO has had light testing only.*
- **New subsystem: Audio Loading & Memory** — tiered, opt-in control over clip RAM footprint and load timing. Default direct-reference path is unchanged and zero-overhead. See [Manual chapter 16](USER_GUIDE.md#16-audio-loading--memory) and [API → Loading](#loading).
  - `AudioClipWarmer` + `AudioManager.WarmClip/WarmClips/WarmContainer` — front-load clip audio data ahead of first play (no extra package)
  - `IAudioClipSource` seam (`AudioClipSources.Active`, default `DirectClipSource`) — containers resolve clips through it; per-entry `address` on `WeightedAudioClip` / `SequenceEntry` / `RoutingEntry`
  - Optional **Addressables backend** (gated on `com.unity.addressables`): load/unload a region's clips by zone via `ZoneAudioBank` + `AddressableAudioRuntime`, voice-safe refcounted residency (banks + voices + pins), drop-on-cold, linger
  - **Addressable Audio Authoring** window (convert clips in one click) + **Loading Validator** window. *Voice-hold correctness verified; build-scale RAM profiling pending.*
- **Additive** — no v2.5.0 APIs changed. Both subsystems are inert until opted into (netcode: install a stack + add a transport; loading: use the warmer or install Addressables).

**v2.5.0** - May 2026
- **New subsystem: Occlusion Mixer Slot Pool** — per-bus pre-built mixer-slot pools replace per-source `AudioLowPassFilter`s. Eliminates the per-acquire biquad-reset click; cutoff modulation is a stable mixer-side write. See [Manual chapter 14](USER_GUIDE.md#14-occlusion-mixer-slot-pool).
  - `OcclusionLayout` + `OcclusionSlot` types
  - `AudioManager.AcquireOcclusionSlot` / `ReleaseOcclusionSlot` and slot-pool diagnostics
  - Multi-ray partial occlusion (`occlusionRayCount`, `occlusionRaySpreadMeters`) — N-level edge gradient instead of binary
  - Per-frame exponential smoothing via `occlusionGainSmoothingSeconds` / `occlusionCutoffSmoothingSeconds`
  - Occlusion Layout Builder window (`Window > Audio System > Occlusion Layout`)
- **New subsystem: Reverb Send Buses & Per-Zone Reverb** — source-side routing + listener-side bus-character driver. SFX voices contribute to the reverb of the room they're fired in; each bus's character is defined by its `ReverbSendBus` asset (the single source of truth, since the 2026-05-21 unification). See [Manual chapter 15](USER_GUIDE.md#15-reverb-send-buses--per-zone-reverb).
  - `ReverbSendBus` (one acoustic-space identity per asset) + `ReverbSendBusRegistry`
  - `AudioReverbProfile` (six SFX Reverb character params, authored on the bus SO)
  - `AudioZone.ReverbBus` (routing target; character lives on the `ReverbSendBus` SO, not the zone)
  - **Convolution reverb option (Phase 3.2, Windows x64, beta)** — `ReverbSendBus.UseConvolution` swaps the parametric SFX Reverb for a native impulse-response convolution plugin. See [Manual §15.14](USER_GUIDE.md#1514-convolution-reverb-windows-beta).
  - `AudioManager.WriteReverbSendsImmediate` — synchronous on-Play write so first audio frame carries correct routing
  - `AudioContainer` opt-in flags: `AllowReverbSend`, `ReverbSendLevelDb`, `StaticEmitter`, `ExplicitReverbBus`
  - Reverb Send Buses window (`Window > Audio System > Reverb Send Buses`) + per-asset Generate/Validate/Apply/Pull inspector
- **AudioContainer routing flags** — `UseOcclusion`, `UsePropagation`, `AllowReverbSend` opt SFX containers into the new slot pool. `UseOcclusion` enables raycast occlusion; `UsePropagation` composes propagation cutoff into the same voice via most-muffled-wins (min cutoff, multiplicative gain). The two routing/muffling subsystems now integrate cleanly with the propagation subsystem from v2.3.0.
- **Soft zone boundaries** — `AudioZone.entryFadeMeters` smooths reverb sends, listener-side reverb character, and `BaseVolumeDb` across the zone surface. 0 (default) preserves the v2.3.0 binary boundary.
- **Outside-the-graph behavior** — `PropagationManager.silenceOutsideGraph` toggle picks between whitelist mode (outside = audible, default) and complete-coverage mode (outside = silent + muffled).
- **Reachability-preserving culler** — `PropagationProximityCuller` now keeps zones reachable through near portals active even when individually out of activation-radius range. Two-zone-via-portal scenes no longer fall silent on tight radii.
- **Higher default solve rate** — `PropagationManager.solveRateHz` default 8 → 15 (≈ 67 ms cache step). Fast zone-trigger crossings and partially-closed-portal transitions no longer surface audible step changes.
- **Lower default open low-pass** — `AudioPortal.openLowPassHz` default 18000 → 12000. A portal without a door source is now audibly distinct from full transparency on typical speakers, surfacing authoring mistakes early.

**v2.3.0** - April 2026
- **New subsystem: `AudioSystem.Propagation`** — zone/portal graph-based routing for ambient beds
- `AudioZone` / `AudioPortal` MonoBehaviours (authoring)
- `AmbientSource` / `AmbientEmitter` (declaration + pooled persistent voice)
- `PropagationManager` singleton + `AudioListenerZoneTracker`
- `IPortalDoorSource` interface for portable door integration
- `PropagationProximityCuller` for large-level scale (per-node activation radius + global caps)
- Pure Dijkstra solver with reusable buffers (allocation-free hot path)
- Blend-region crossfading for pop-free doorway traversal
- Additive — does not modify any v2.2.0 APIs

**v2.2.0** - March 2026
- Runtime container override system (3D settings)
- Per-voice control on AudioMultiHandle
- RTPC transitions with `TransitionRTPC()`
- `StopAllSounds()`, `PauseAll()`, `UnpauseAll()`
- `MuteAll(bool)` replaces separate Mute/Unmute methods
- AudioStatistics replaces Statistics struct
- VoiceStealBehavior: added `Furthest`, removed `None`
- GainStack: added `DuckingGain`, `ApplyToSource()`, `Reset()`
- SequenceContainer: `AutoAdvance`, `PlayNext()`
- AudioBus: ducking system, effect sends
- AudioState: structured property classes
- Multi-position emitter components
- `IRTPCListener` interface
- `ListenerUtil` utility class
- `AudioVoiceDebugInfo` for runtime debugging

**v2.0.0** - January 2026
- AudioEventRegistry system
- Quick Sound Setup Wizard (5 modes), now Sound Builder (Simple)
- AudioEvent Inspector validation
- Batch import support

**v1.0** - Initial release
- Complete audio middleware system
- 5 container types
- Event system with multi-actions
- Hierarchical bus mixing
- State management
- Multi-position audio
- Voice virtualization & LOD
- Occlusion system

---

**End of API Reference**

---

*For tutorials, see [Cookbook](USER_GUIDE.md#3-cookbook) in the User Guide*
*For deep knowledge, see [Manual](USER_GUIDE.md#4-manual) in the User Guide*
*For quick start, see [Quick Start](USER_GUIDE.md#2-quick-start) in the User Guide*

*SFX System v3.0.0 - Professional Audio Middleware for Unity*
