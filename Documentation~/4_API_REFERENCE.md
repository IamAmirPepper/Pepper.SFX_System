# SFX System - API Reference

**Version:** 2.3.0
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** April 2026

**Complete API documentation for SFX System v2.3.0**

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
14. [Enums & Data Structures](#enums--data-structures)

---

## AudioManager

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour` (partial class)
**Access:** Singleton via `AudioManager.Instance`

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
public AudioLowPassFilter LowPassFilter { get; }
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
    public float RtpcGain { get; set; }       // RTPC modulation
    public float SchedulerGain { get; set; }  // Crossfade/transition
    public float DuckingGain { get; set; }    // Ducking attenuation

    public float GetFinalGain()               // Returns product of all gains
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

**Migration:** See [Advanced Migration Guide](5_ADVANCED_MIGRATION_GUIDE.md) for full details.

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

## Propagation

**Namespace:** `AudioSystem.Propagation`
**Added in:** v2.3.0
**Location:** `Assets/Scripts/SFX_System/RunTime/Propagation/`

Zone/portal graph-based routing for long-running ambient beds. Additive — does not modify the SFX event pipeline. See [MANUAL.md Chapter 13](3_MANUAL.md#13-ambient-propagation-subsystem) for architecture and design rationale.

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

| Field                        | Default | Description                                                                                                 |
|------------------------------|---------|-------------------------------------------------------------------------------------------------------------|
| `solveRateHz`                | 8       | Solves per second. Per-frame emitter smoothing handles the in-between.                                      |
| `distancePenaltyDbPerMeter`  | 0       | Extra dB cost per meter of portal-to-portal distance. 0 = pure transmission routing.                        |
| `maxAmbientEmitters`         | 16      | Global cap. Farthest emitters silenced first when exceeded.                                                 |
| `emitterSmoothSpeed`         | 4       | Per-frame smoothing speed for volume/cutoff/position.                                                       |
| `culler`                     | —       | Nested `PropagationProximityCuller` configuration.                                                          |
| `listenerOverride`           | null    | Optional explicit listener transform. Falls back to `AudioListener` or `Camera.main`.                       |

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
```

Returns the top of the listener zone stack, or null if the listener isn't inside any registered zone.

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
| `activationRadius` | 50      | Distance from listener beyond which this zone is culled. `Infinity` = never cull. |

#### Properties

```csharp
public string ZoneId { get; }
public float BaseVolumeDb { get; }
public float ActivationRadius { get; }
public IReadOnlyList<BoxCollider> VolumeColliders { get; }
```

`VolumeColliders` is populated from `GetComponents<BoxCollider>()` in `Awake` — add multiple BoxColliders for L-shapes, they're OR-unioned.

#### Virtual Methods (override for custom shapes)

```csharp
public virtual bool ContainsPoint(Vector3 worldPoint);
public virtual Vector3 ClosestSurfacePoint(Vector3 worldPoint);
```

Default implementations iterate all volume colliders. Subclass to support sphere-shaped or mesh-shaped zones without touching the solver.

---

### AudioPortal

**Namespace:** `AudioSystem.Propagation`
**Inheritance:** `MonoBehaviour`
**Required Components:** `BoxCollider` (auto-forced to `isTrigger = true`)

Edge of the propagation graph. Connects two zones, optionally modulated by an `IPortalDoorSource`, and doubles as the listener blend region.

#### Inspector — Zones

| Field    | Description                              |
|----------|------------------------------------------|
| `zoneA`  | One of the two connected zones.          |
| `zoneB`  | The other connected zone.                |

Both required. `OnValidate` errors if either is null or if `zoneA == zoneB`.

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
| `openLowPassHz`      | 18000   | Low-pass cutoff when fully open.                                  |
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

#### Inspector

| Field                  | Description                                                                          |
|------------------------|--------------------------------------------------------------------------------------|
| `sourceZone`           | The zone where this ambience originates (required).                                  |
| `loopingClip`          | The looping audio clip. Must be loop-authored (no pops at boundary).                 |
| `ambienceMixerGroup`   | `AudioMixerGroup` to route through. Use the Ambience bus for bus ducking / mix fades.|
| `sourceBaseVolumeDb`   | Baseline volume in dB. Default 0.                                                    |
| `playOnStart`          | Auto-register with `PropagationManager` on enable. Default true.                     |

#### Properties

```csharp
public AudioZone SourceZone { get; }
public AudioClip LoopingClip { get; }
public AudioMixerGroup MixerGroup { get; }
public float SourceBaseVolumeDb { get; }
```

#### Methods

```csharp
public void SetBaseVolumeDb(float db);   // clamped to [-80, 20]; picked up on next solve
public void Register();                  // manual registration (when playOnStart is false)
public void Unregister();                // fades out cleanly, doesn't abruptly stop
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

*For tutorials, see [COOKBOOK.md](2_COOKBOOK.md)*
*For deep knowledge, see [MANUAL.md](3_MANUAL.md)*
*For quick start, see [QUICK_START.md](1_QUICK_START.md)*

*SFX System v2.3.0 - Professional Audio Middleware for Unity*
