# SFX System - API Reference

**Version:** 3.0.0
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** July 2026

**Complete API documentation for SFX System v3.0.0**

---

## Table of Contents

1. [AudioManager](#audiomanager)
2. [AudioEvent](#audioevent)
3. [AudioHandle & AudioMultiHandle](#audiohandle--audiomultihandle)
4. [AudioContainer (Base)](#audiocontainer-base)
5. [Container Types](#container-types)
6. [AudioBus](#audiobus)
7. [AudioState](#audiostate)
8. [AudioVoice & AudioVoiceEnhanced](#audiovoice--audiovoiceenhanced)
9. [AudioExtensions & Utilities](#audioextensions--utilities)
10. [AudioEventRegistry](#audioeventregistry)
11. [Multi-Position Emitters](#multi-position-emitters)
12. [Editor Tools](#editor-tools)
    - [QuickSoundSetupWizard](#quicksoundsetupwizard)
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

#### Audio Configuration

| Field                                | Default | Description                                                                              |
|--------------------------------------|---------|------------------------------------------------------------------------------------------|
| `masterMixerGroup`                   | null    | Optional master mixer group handle. Not directly routed by AudioManager; exposed for project-side scripts. |
| `masterVolume` (range 0–1)           | 1       | Global linear volume multiplier applied to every voice. Combines with per-bus volumes.   |
| `muteAll`                            | false   | Global mute switch. When ON, every voice's output is forced to zero.                     |

#### Voices

| Field                  | Default | Description                                                                                                   |
|------------------------|---------|---------------------------------------------------------------------------------------------------------------|
| `maxRealVoices`        | 32      | Maximum voices playing through real AudioSources at once. Voices beyond this are kept virtual and promoted when slots free up. |
| `maxVirtualVoices`     | 64      | Maximum virtual voices tracked behind the real-voice cap. Hard ceiling on concurrent voices the system is aware of. |
| `voiceUpdateInterval`  | 0.1     | Seconds between voice priority / virtualization passes (default 10 Hz).                                       |

#### Occlusion / LOD

| Field                       | Default | Description                                                                              |
|-----------------------------|---------|------------------------------------------------------------------------------------------|
| `occlusionMask`             | ~0      | Physics layer mask used by the occlusion raycast. Exclude AudioZone trigger layers.      |
| `occlusionUpdateInterval`   | 0.2     | Seconds between OcclusionUpdate ticks (raycast cadence, default 5 Hz). Per-frame smoothing handles the in-between. |
| `enableLOD`                 | true    | Master switch for distance-based voice LOD.                                              |
| `lodDistances`              | `{10, 25, 50, 100}` | Distance thresholds (meters) at which the LOD system steps down voice quality. Values must be ascending. |

See the slot-pool subsection for `occlusionRayCount`, `occlusionRaySpreadMeters`, `occlusionGainSmoothingSeconds`, `occlusionCutoffSmoothingSeconds`, `voiceMixer`, `occlusionLayout`, and `defaultOcclusionSlotsPerBus`. See the reverb-sends subsection for `reverbSendBusRegistry` and `reverbSendsUpdateInterval`.

---

### Properties

#### Instance
```csharp
public static AudioManager Instance { get; }
```
**Description:** Singleton instance of AudioManager.
**Returns:** The active AudioManager in the scene.

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

### RTPC Methods

#### SetRTPC
```csharp
public void SetRTPC(string parameterName, float value)
```
**Description:** Sets a Real-Time Parameter Control value.

**Parameters:**
- `parameterName` - Parameter name (case-sensitive)
- `value` - Parameter value (typically 0-1, but depends on usage)

**Example:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", 0.75f);
AudioManager.Instance.SetRTPC("EngineRPM", 0.5f);
```

#### GetRTPC
```csharp
public float GetRTPC(string parameterName)
```
**Description:** Gets current value of an RTPC parameter.

**Returns:** Current value, or `0f` if parameter doesn't exist.

#### TransitionRTPC
```csharp
public void TransitionRTPC(string parameterName, float targetValue, float duration)
```
**Description:** Smoothly transitions an RTPC parameter to a target value over time.

**Parameters:**
- `parameterName` - Parameter name
- `targetValue` - Target value
- `duration` - Transition duration in seconds

**Example:**
```csharp
AudioManager.Instance.TransitionRTPC("CombatIntensity", 1.0f, 2f);
```

#### GetAllRTPCs
```csharp
public IReadOnlyDictionary<string, float> GetAllRTPCs()
```
**Description:** Gets all current RTPC values.

**Returns:** Dictionary mapping parameter names to values.

#### RegisterRTPCListener / UnregisterRTPCListener
```csharp
public void RegisterRTPCListener(IRTPCListener listener)
public void UnregisterRTPCListener(IRTPCListener listener)
```
**Description:** Registers/unregisters an object to receive RTPC change callbacks.

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

---

### Switch Methods

#### SetSwitch
```csharp
public void SetSwitch(string group, string value)
```
**Description:** Sets a switch value for a switch group.

**Parameters:**
- `group` - Switch group name (e.g., "Surface_Type")
- `value` - Switch value (e.g., "Metal", "Wood")

**Example:**
```csharp
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
```

#### GetSwitch
```csharp
public string GetSwitch(string group)
```
**Description:** Gets current switch value for a group.

**Returns:** Current switch value, or empty string if not set.

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
**Description:** Activates an audio state.

**Parameters:**
- `stateName` - Name of state to activate
- `transitionTime` - Duration of transition in seconds (default: 0.5s)

**Example:**
```csharp
AudioManager.Instance.SetState("Underwater", 1.5f);
AudioManager.Instance.SetState("Paused", 0.3f);
```

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
**Description:** Sets bus volume in decibels.

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

**Returns:** `AudioBus`, or master bus if not found.

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

**Example:**
```csharp
AudioManager.Instance.CrossFade(oldMusicContainer, newMusicContainer, 2f);
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
**Description:** Pauses all currently playing sounds.

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
Debug.Log($"Active voices: {stats.activeVoices}/{stats.totalVoices}");
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
**Description:** Gets detailed debug info for all active voices.

**Returns:** List of `AudioVoiceDebugInfo` for each active voice.

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
**Description:** Voice stealing priority (Low, Medium, High, Critical).

#### MaxInstances
```csharp
public int MaxInstances { get; }
```
**Description:** Maximum concurrent instances (0 = unlimited).

#### StealBehavior
```csharp
public VoiceStealBehavior StealBehavior { get; }
```
**Description:** How to steal voices when max reached (Oldest, Quietest, Furthest, LowestPriority).

#### Cooldown
```csharp
public float Cooldown { get; }
```
**Description:** Minimum time between triggers in seconds.

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
- `source` - (Optional) Source GameObject

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
    public AudioContainer container;
    public AudioBus targetBus;
    public float delay;
    public float fadeDuration;
    public AnimationCurve fadeCurve;
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
}
```

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
**Description:** Controls a single audio playback instance.

#### Properties

```csharp
public string EventName { get; }
public AudioEvent SourceEvent { get; }
public bool IsPaused { get; }
public bool isPlaying { get; }
public float time { get; }      // Current playback time in seconds
public float duration { get; }  // Total clip duration in seconds
```

---

#### Events

```csharp
public event System.Action OnStarted;
public event System.Action OnLoop;
public event System.Action OnFinished;
```
**Description:** Callbacks for playback lifecycle events.

**Example:**
```csharp
AudioHandle handle = myEvent.Post();
handle.OnStarted += () => Debug.Log("Started");
handle.OnLoop += () => Debug.Log("Looped");
handle.OnFinished += () => Debug.Log("Finished");
```

---

#### Methods

##### SetVolume
```csharp
public void SetVolume(float linear)
```
**Description:** Sets playback volume.

**Parameters:**
- `linear` - Volume multiplier (0-1)

##### SetPitch
```csharp
public void SetPitch(float semitones)
```
**Description:** Sets pitch offset in semitones.

**Parameters:**
- `semitones` - Pitch offset (-12 to +12 typical range)

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
public event System.Action OnLoop;
public event System.Action OnFinished;
```

---

#### Global Methods (All Voices)

##### SetVolume
```csharp
public void SetVolume(float linear)
```
**Description:** Sets volume for all voices.

##### SetPitch
```csharp
public void SetPitch(float semitones)
```
**Description:** Sets pitch for all voices.

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

#### Per-Voice Methods

##### SetVoiceVolume
```csharp
public void SetVoiceVolume(int voiceIndex, float volumeMultiplier)
```
**Description:** Sets volume for a specific voice by index.

##### SetVoicePitch
```csharp
public void SetVoicePitch(int voiceIndex, float semitones)
```
**Description:** Sets pitch for a specific voice by index.

##### StopVoice
```csharp
public void StopVoice(int voiceIndex, float fadeTime = 0.1f)
```
**Description:** Stops a specific voice with fade.

##### PauseVoice / ResumeVoice
```csharp
public void PauseVoice(int voiceIndex)
public void ResumeVoice(int voiceIndex)
```
**Description:** Pauses/resumes a specific voice.

---

#### Position Methods

##### UpdatePositions
```csharp
public void UpdatePositions(Vector3[] newPositions)
public void UpdatePositions(Transform[] newTransforms)
```
**Description:** Updates emitter positions for all voices.

##### RefreshPositions
```csharp
public void RefreshPositions()
```
**Description:** Re-reads positions from tracked transforms.

##### GetVoicePosition
```csharp
public Vector3? GetVoicePosition(int voiceIndex)
```
**Description:** Gets position of a specific voice.

**Returns:** Position, or `null` if index out of range.

##### GetAllPositions
```csharp
public Vector3[] GetAllPositions()
```
**Description:** Gets positions of all voices.

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
public float MaxDistance { get; set; }
public AudioRolloffMode RolloffMode { get; }
public bool HasOverrides { get; }
```

#### Routing / Slot Pool Opt-Ins

```csharp
public bool UseOcclusion { get; }          // raycast occlusion
public bool UsePropagation { get; }        // propagation cutoff (composes with raycast via min-wins)
public bool AllowReverbSend { get; }       // per-zone reverb sends
public float ReverbSendLevelDb { get; }    // per-container reverb send level (dB)
public bool StaticEmitter { get; }         // hint: this voice doesn't move during playback
public ReverbSendBus ExplicitReverbBus { get; }  // override zone-driven send routing
```

Setting any of `UseOcclusion`, `UsePropagation`, or `AllowReverbSend` to true causes the container to acquire an [OcclusionSlot](#occlusionslot) at play time. See [Manual chapter 14](USER_GUIDE.md#14-occlusion-mixer-slot-pool) for the slot pool architecture and chapter 15 for per-zone reverb sends.

`StaticEmitter` and `ExplicitReverbBus` are modifiers, not feature flags — they refine the behavior of the reverb-send driver but don't on their own cause a slot to be acquired.

---

### Methods

#### Play
```csharp
public abstract AudioVoice Play(Vector3 position = default, GameObject parent = null)
```
**Description:** Plays the container. **Implemented by subclasses.**

**Returns:** `AudioVoice` instance.

#### Stop / StopImmediate
```csharp
public abstract void Stop()
public abstract void StopImmediate()
```
**Description:** Stops all active voices from this container (with or without fade).

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
    public void Clear();
}
```

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
**Description:** Clears the repeat-avoidance history.

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
public string SwitchGroupName { get; }
public List<SwitchEntry> SwitchEntries { get; }
public AudioContainer DefaultContainer { get; }
public float Volume { get; }
```

#### Methods

```csharp
public void SetSwitch(string switchValue)
public string GetCurrentSwitch()
public AudioContainer GetActiveContainer()
```

**Description:** Selects child container based on switch value.

---

### BlendContainer

**Menu:** Create > Audio System > Blend Container
**Implements:** `IRTPCListener`

#### Properties

```csharp
public List<BlendEntry> BlendEntries { get; }
public string BlendParameterName { get; }
public float Volume { get; }
public bool Loop { get; }
public IReadOnlyList<AudioVoice> LayerVoices { get; }
```

#### Methods

```csharp
public void UpdateBlend(float blendValue)
public float GetCurrentBlendValue()
public void OnRTPCChanged(string parameter, float value)
```

**Description:** Plays multiple containers with RTPC-driven crossfading. Automatically registered as an RTPC listener when playing.

---

## AudioBus

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

```csharp
public string BusName { get; }
public AudioBus ParentBus { get; }
public AudioMixerGroup MixerGroup { get; }
public float VolumeDb { get; }
public float VolumeMultiplier { get; }
public bool Mute { get; }
public bool Solo { get; }
public IReadOnlyList<EffectSend> Sends { get; }
public bool EnableDucking { get; }
public IReadOnlyList<DuckingTarget> DuckingTargets { get; }
public float DuckingAttack { get; }
public float DuckingRelease { get; }
```

---

### Methods

#### Volume & Mute
```csharp
public void SetVolume(float newVolumeDb)
public float GetFinalVolume()
public void SetMute(bool newMute)
public void SetSolo(bool newSolo)
```
**Description:** `GetFinalVolume()` returns the final linear volume including the entire parent bus hierarchy.

#### Voice Management
```csharp
public void AssignToVoice(AudioVoiceEnhanced voice)
public void RemoveVoice(AudioVoiceEnhanced voice)
public AudioSource GetFirstActiveSource()
```

#### Ducking
```csharp
public void TriggerDucking(float holdDuration = 0f)
public void ApplyDucking(float amountDb, float attack, float hold, float release)
```
**Description:** `TriggerDucking()` uses the bus's configured ducking settings. `ApplyDucking()` applies custom ducking parameters.

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
    public float duckAmount;
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
public IReadOnlyList<BusVolumeProperty> BusVolumes { get; }
public IReadOnlyList<SwitchProperty> SwitchValues { get; }
public IReadOnlyList<RTPCProperty> RtpcValues { get; }
public IReadOnlyList<EffectProperty> EffectProperties { get; }
```

---

### Methods

#### Apply
```csharp
public void Apply(float transitionTime)
```
**Description:** Applies this state's properties (bus volumes, RTPCs, switches, effects).

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
public AudioContainer Container { get; }
public AudioBus Bus { get; }
public int Priority { get; }
public bool IsVirtual { get; }
public float VirtualTime { get; }
public double ScheduledStartTime { get; }
public double ScheduledEndTime { get; }
public OcclusionSlot OcclusionSlot { get; }     // mixer slot held while playing (null when not occluding)
```

#### Methods

```csharp
public void UpdateFinalVolume()
public void MakeVirtual()
public void MakeReal()
public void UpdateVirtualTime(float deltaTime)
public float GetImportance()
```

---

### GainStack (Nested Class)

```csharp
public class GainStack
{
    public float BaseGain { get; set; }       // Container/Event volume
    public float BusGain { get; set; }        // Bus hierarchy
    public float OcclusionGain { get; set; }  // Occlusion attenuation
    public float RtpcGain { get; set; }       // RTPC modulation. RESERVED: sole writer is
                                              // BlendContainer.UpdateBlend (per-layer blend curve).
                                              // No second writer — see reservation note in AudioVoiceEnhanced.cs.
    public float SchedulerGain { get; set; }  // Crossfade/transition
    public float MultiplierGain { get; set; } // Per-voice multiplicative — volume randomization,
                                              // SwitchContainer/BlendContainer outer Volume,
                                              // and AudioExtensions.PlayWithVolume write here.
    public float DuckingGain { get; set; }    // (Deprecated) ducking is applied at the bus, not the voice.
                                              // Settable for back-compat but NOT included in GetFinalGain.

    public float GetFinalGain()               // BaseGain * BusGain * OcclusionGain * RtpcGain * SchedulerGain * MultiplierGain
    public void ApplyToSource(AudioSource source)
    public void Reset()
}
```

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
float linear = AudioExtensions.DbToLinear(-6f);  // ~0.5
float db = AudioExtensions.LinearToDb(0.5f);      // ~-6dB
float pitch = AudioExtensions.CentsToPitch(100f); // 1.059 (1 semitone up)
```

---

### ListenerUtil

**Namespace:** `AudioSystem`
**Type:** Static utility class

```csharp
public static Transform Get()
public static Vector3 GetPosition()
public static void Set(Transform listener)
public static void Invalidate()
```

**Description:** Cached AudioListener lookup. Use to efficiently get the listener position for distance calculations.

---

### BeatScheduler

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour`
**File:** `RunTime/Utilities/BeatScheduler.cs`

**Description:** Fires an AudioEvent on a tempo grid (BPM) whose rate can be changed at runtime **without affecting pitch**. Each beat is a fresh one-shot post via `AudioManager.PostEvent`, not a time-stretch of a continuous clip. Timing is anchored to `AudioSettings.dspTime` so cadence is stable under frame-rate fluctuation.

**Use cases:** heartbeats, metronomes, ticking clocks, pulsing ambiences, sonar pings — any rhythmic SFX whose rate must vary independently of pitch.

#### Inspector Fields

| Field | Type | Description |
|---|---|---|
| `eventName` | `string` | Name of the AudioEvent to post each beat. Looked up via `AudioManager.Instance.PostEvent`. |
| `emitter` | `GameObject` | Optional emitter used for the posted event's source and position. Defaults to the BeatScheduler's own GameObject. |
| `bpm` | `float` (1–600) | Beats per minute. Also writable at runtime via the `Bpm` property. |
| `maxCatchUpBeatsPerFrame` | `int` (1–16) | Cap on `OnBeat` callbacks per Update if the scheduler falls behind (frame hitch, very high BPM). Audio always plays at most ONCE per Update regardless of this value — overdue audio beats are dropped to prevent voice-stacking thump. Only affects `OnBeat` consumers that count beats for game logic (`every 4th beat trigger X`), keeping them synchronized with the grid across hitches. |
| `playOnEnable` | `bool` | Start scheduling automatically when enabled. |
| `fireImmediatelyOnStart` | `bool` | Fire the first beat on the next frame (true) or wait one interval (false). |

#### Properties

```csharp
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
public event System.Action OnBeat;      // Fired on the main thread on every beat, after PostEvent.
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

**Description:** Implement this interface to receive RTPC change notifications. Register via `AudioManager.Instance.RegisterRTPCListener()`.

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
public bool UseDirectionality { get; set; }
public float ConeAngle { get; set; }
public Vector3 Position { get; }
public Quaternion Rotation { get; }
public Vector3 Forward { get; }
```

---

## Editor Tools

**Note:** These tools are Unity Editor extensions and not available at runtime.

### QuickSoundSetupWizard

**Namespace:** `AudioSystemEditor`
**Type:** EditorWindow
**Access:** `Window > Audio System > Quick Sound Setup Wizard`

#### Description

One-click sound setup wizard that creates Bus + Container + Event with all references pre-filled.

**Features:**
- 5 creation modes: Complete Setup, Container Only, Event Only, Bus Only, Batch Import
- All 5 container types supported
- 8 preset templates
- Auto-create folder structure
- Validate Resources placement

#### Usage

1. `Window > Audio System > Quick Sound Setup Wizard`
2. Drop AudioClip(s) in field
3. Choose preset (e.g., "Simple SFX")
4. Click "Create Complete Sound Setup"
5. Done!

---

### AudioEventEditor

**Namespace:** `AudioSystemEditor`
**Type:** Custom Inspector (Editor Only)
**Applies To:** AudioEvent assets

#### Description

Custom Inspector for AudioEvent with automatic validation and one-click fixes.

**Features:**
- Real-time Resources folder validation
- Visual warnings if placed incorrectly
- One-click "Move to Resources" button
- Console warnings on asset creation

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
- **Scan, Auto-Create, Refresh** — walks the project for containers / ambient sources opted into occlusion, collects unique mixer groups, authors `<Bus>_OcclusionLayer` + slot groups under each, exposes their cutoff parameters and (optional) Send level parameters, writes the result to the OcclusionLayout asset.
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
| `listenerOverride`               | null    | Optional explicit listener transform. Falls back to `AudioListener` or `Camera.main`.                       |

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
public AudioZone FindFirstZoneFeedingBus(AudioMixerGroup busGroup);
public float GetSourceAudibility(AudioZone sourceZone);
public float GetSourcePropagationCutoff(AudioZone sourceZone);
public AudioZone GetResolvedListenerReverbProfile(Vector3 listenerPos, AudioReverbProfile dest);
public AudioZone GetResolvedListenerReverbProfile(AudioReverbProfile dest);
public bool SilenceOutsideGraph { get; }
```

- `GetCurrentListenerZone` — top of the listener zone stack, or null if the listener isn't inside any registered zone.
- `GetZoneContainingPoint` — first registered zone whose `ContainsPoint(worldPoint)` returns true, or null. Used by the reverb-send driver to resolve source zones.
- `FindFirstZoneFeedingBus` — first registered zone whose `ReverbBus` matches the given mixer group, or null. Used by the listener-side bus-params driver as the fallback canonical zone when the listener is outside any bus-feeding zone.
- `GetSourceAudibility` / `GetSourcePropagationCutoff` — per-zone reads from the listener-audibility cache, populated each solve tick. Same-zone = 1.0 / 22 kHz; through-portals = chain-of-portals weight; unreachable = 0 / 500 Hz.
- `GetResolvedListenerReverbProfile` — fills the provided `AudioReverbProfile` with the listener's current zone profile (portal-blend lerped when inside a blend region). Returns the canonical zone whose profile was written.

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
| `reverbProfile`    | silent  | `AudioReverbProfile` driving the listener-side bus character when the listener is in this zone. Defaults to silent (`Room = -10000`). See [Reverb Send Buses](#reverb-send-buses). |
| `reverbBus`        | null    | The mixer reverb bus voices physically in this zone send to (source-side). Drag in the `ReverbBus` field from a `ReverbSendBus` asset. Null = no reverb-send routing for sources in this zone. |

#### Properties

```csharp
public string ZoneId { get; }
public float BaseVolumeDb { get; }
public float ActivationRadius { get; }
public float EntryFadeMeters { get; }
public AudioReverbProfile ReverbProfile { get; }   // never null at runtime; defaults to AudioReverbProfile.Off
public AudioMixerGroup ReverbBus { get; }          // null when no bus assigned
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

Attach to the GameObject carrying the `AudioListener`. Watches for trigger enter/exit on `AudioZone` and `AudioPortal` colliders and forwards events to `PropagationManager`.

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

Acquires a free slot from the pool owned by `bus`. Returns null if (a) no pool exists for the bus (the bus wasn't in the layout), (b) the slot pool isn't ready, or (c) all slots are in use (pool exhausted — voice plays unoccluded; a throttled warning is logged once per second per bus).

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

The "wet path" complement to occlusion. Source-side: voices in a tagged zone send to a per-zone reverb bus. Listener-side: each bus's parameters are driven from the listener's current zone profile. See [Manual chapter 15](USER_GUIDE.md#15-reverb-send-buses--per-zone-reverb) for architecture and design rationale.

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
// Basic
public float DryLevel { get; }       // -10000..0 dB. Default -10000 (wet-only).
public float Room { get; }           // -10000..0 dB
public float DecayTime { get; }      //   0.1..20 s
public float ReverbLevel { get; }    // -10000..2000 dB

// Advanced
public float RoomHF { get; }            // -10000..0 dB
public float RoomLF { get; }            // -10000..0 dB
public float DecayHFRatio { get; }      // 0.1..2 ratio
public float ReflectionsLevel { get; }  // -10000..1000 dB
public float ReflectionsDelay { get; }  // 0..0.3 s
public float ReverbDelay { get; }       // 0..0.1 s
public float HFReference { get; }       // 1000..20000 Hz
public float LFReference { get; }       // 20..1000 Hz
public float Diffusion { get; }         // 0..100 %
public float Density { get; }           // 0..100 %
```

Only the six listener-relevant parameters (`ReverbLevel`, `DecayTime`, `Room`, `RoomHF`, `DecayHFRatio`, `ReflectionsLevel`) are exposed to the runtime driver. The other eight describe bus identity and stay at the SO-authored snapshot defaults.

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

`GetDefaultProfile` is the fallback path used by `AudioManager.DriveReverbBusParams` when no scene zone feeds this bus — the bus reverts to its SO-authored character.

#### Editor Surface

```csharp
#if UNITY_EDITOR
public void EditorSetReverbBus(AudioMixerGroup group);
public void EditorSetOutputDestination(AudioMixerGroup group);
public void EditorSetMixerGroupGuid(string guid);
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

Per-zone reverb parameters, interpolatable across portal blend regions and copy-able into a non-allocating scratch buffer. Six fields, each matching one of the listener-relevant SFX Reverb parameters.

#### Fields

```csharp
[Range(-10000f, 0f)]   public float Room             = -10000f;  // dB, overall wet gain
[Range(-10000f, 0f)]   public float RoomHF           = 0f;       // dB, HF tilt
[Range(0.1f, 20f)]     public float DecayTime        = 1.49f;    // seconds
[Range(0.1f, 2f)]      public float DecayHFRatio     = 0.83f;    // ratio
[Range(-10000f, 1000f)] public float ReflectionsLevel = -2602f;  // dB, early reflections
[Range(-10000f, 2000f)] public float ReverbLevel      = 200f;    // dB, late reverb tail
```

Default (`Room = -10000`) is **inaudible** — zones that don't author a profile produce zero behavior change. Set `Room` toward 0 dB to enable.

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

`AudioZone` (see [Propagation section](#audiozone)) has two reverb-relevant fields:

```csharp
public AudioMixerGroup ReverbBus { get; }       // routing target (source-side)
public AudioReverbProfile ReverbProfile { get; } // character (listener-side)
```

- **`ReverbBus`** — the mixer group voices physically in this zone send to. Drag in a `ReverbSendBus`'s `ReverbBus` field. Leave null for zones with no dedicated reverb (the source-side send is skipped for sources in that zone). Returns null at runtime when explicitly left empty.
- **`ReverbProfile`** — drives the bus's six runtime parameters when the listener is in this zone. Never null at runtime (defaults to `AudioReverbProfile.Off` when unassigned).

Multi-zone-to-bus aggregation is supported — several zones can share a `ReverbBus`. The listener-side driver picks the canonical zone (listener's own if it feeds the bus, else first registered via `PropagationManager.FindFirstZoneFeedingBus`).

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
    Low = 0,       // Least important
    Medium = 64,   // Standard priority
    High = 128,    // Important sounds
    Critical = 255 // Never steal
}
```

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

---

### BlendEntry
```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;                                    // Layer container
    public AnimationCurve volumeCurve = AnimationCurve.Linear(0,0,1,1); // RTPC -> Volume mapping
}
```

---

### AudioStatistics
```csharp
public class AudioStatistics
{
    public int activeVoices;
    public int availableVoices;
    public int totalVoices;
    public int activeLoops;
    public int registeredContainers;
}
```

**Usage:**
```csharp
var stats = AudioManager.Instance.GetStatistics();
Debug.Log($"Voices: {stats.activeVoices}/{stats.totalVoices}");
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
    public AudioVoiceEnhanced voice;
    public int priority;
}
```

**Usage:**
```csharp
var voices = AudioManager.Instance.GetActiveVoicesDebug();
foreach (var v in voices)
    Debug.Log($"{v.eventName}: {v.clipName} vol={v.volume:F2} dist={v.distanceToListener:F1}m");
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

// AmbientSource: addressable loop clip — pins the clip on the active source for its active life.
string   AmbientSource.LoopingClipAddress;   // serialized field; Inspector: "Looping Clip Address"
AudioClip AmbientSource.PlayableClip;         // resolved clip; null while an addressable clip is loading

// Raised on genuine listener zone-membership change (the addressable bank binder subscribes):
static event Action<AudioZone> PropagationManager.ListenerEnteredZone;
static event Action<AudioZone> PropagationManager.ListenerExitedZone;
```

### Gated by com.unity.addressables

Namespace `AudioSystem.Loading.Addressables`; assembly `SFXSystem.Addressables` (compiles only when the package is present).

```csharp
// Refcounted addressable clip source. Installed as AudioClipSources.Active by AddressableAudioRuntime.
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
AddressableAudioRuntime  // one per scene; installs the source, binds zone events, ticks linger, teardown-resets
```

**Refcount model:** `resident(address) = (banks holding it) + (live voices bound to it) + (pins) > 0`. Bank `Release`, `OnVoiceReleased`, and `Unpin` each decrement; the real unload fires at 0, after `LingerSeconds`. Teardown (`ReleaseAll`) force-releases everything regardless of count.

**Cold-post policy (v1):** a not-yet-resident addressable clip makes `TryGetClip` return false → the play is dropped (same as a null clip). Bank preload is the warm path; there is no defer-and-play-late in v1.

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

**Per-event authoring** (`AudioEvent`, Multiplayer foldout, shown only with a backend installed): `Unreliable Delivery`, `Independent Variation (default)`, `Loop Sync Tier` (0/1/2). For the `INetworkAudioTransport` boundary — implementing a new backend — see [User Guide §17.9](USER_GUIDE.md#179-extending-writing-a-new-backend-adapter).

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
h.SetVolume(0.5f);
h.SetPitch(2f);
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
mh.SetVoiceVolume(0, 0.5f);  // Per-voice control
mh.UpdatePositions(newTransforms);
```

---

## Version History

**v3.0.0** - July 2026
- **New subsystem: Multiplayer (Netcode)** — backend-agnostic networked audio. Post a sound on one machine and every player hears it; no audio crosses the wire, only a ~36-byte descriptor each machine plays locally. Install one netcode stack and its adapter lights up automatically; a backend you didn't install never compiles. See [Manual chapter 17](USER_GUIDE.md#17-multiplayer-netcode) and [API → Netcode (Multiplayer)](#netcode-multiplayer).
  - `NetworkedAudioManager` façade — `PostNetworkedEvent` / `PostNetworkedLoop` / `StopNetworked`, server-authored `PostServer*`, scheduled/quantized cues
  - Emitter binding (sound follows a moving `NetworkObject`), shared loop lifetime + late-join snapshot, cross-client variation parity (seed on the wire)
  - Competitive hardening — server authority/validation, proximity culling (= don't-send), per-event `Unreliable Delivery`
  - Musical sync — `NetworkClock` (network-time ↔ local dsp) + `NetworkMusicTransport` (tempo/phase), loop phase-lock tiers
  - `NetworkAudioSettings` scene component; **Network Audio Debug** window; per-event **Multiplayer** foldout on `AudioEvent`
  - Adapters: NGO 2.x (`SFXSYSTEM_NGO`) and FishNet v4 (`FISHNET`). *Verified on FishNet one-PC MPPM; two-PC + full NGO sweep pending.*
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
- **New subsystem: Reverb Send Buses & Per-Zone Reverb** — source-side routing + listener-side bus-character driver. SFX voices contribute to the reverb of the room they're fired in; each bus's character is driven from the listener's current zone profile. See [Manual chapter 15](USER_GUIDE.md#15-reverb-send-buses--per-zone-reverb).
  - `ReverbSendBus` (one acoustic-space identity per asset) + `ReverbSendBusRegistry`
  - `AudioReverbProfile` (six listener-relevant SFX Reverb params)
  - `AudioZone.ReverbBus` (routing target) + `AudioZone.ReverbProfile` (character)
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
- Quick Sound Setup Wizard (5 modes, 8 presets)
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
