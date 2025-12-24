# SFX System - API Reference

**Version:** 1.2
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** December 2025

**Complete API documentation for SFX System v1.0**

---

## Table of Contents

1. [AudioManager](#audiomanager)
2. [AudioEvent](#audioevent)
3. [AudioHandle & AudioMultiHandle](#audiohandle--audiomultihandle)
4. [AudioContainer (Base)](#audiocontainer-base)
5. [Container Types](#container-types)
6. [AudioBus](#audiobus)
7. [AudioState](#audiostate)
8. [AudioVoiceEnhanced](#audiovoiceenhanced)
9. [AudioExtensions](#audioextensions)
10. [Enums & Data Structures](#enums--data-structures)

---

## AudioManager

**Namespace:** `AudioSystem`
**Inheritance:** `MonoBehaviour`
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
public AudioHandle PostEvent(string eventName)
public AudioHandle PostEvent(string eventName, GameObject parent, Vector3 position)
```
**Description:** Triggers an audio event by name.

**Parameters:**
- `eventName` - Name of the event to trigger (must exist in Resources/Audio/Events/)
- `parent` - (Optional) GameObject to attach sound to
- `position` - (Optional) 3D world position for spatial audio

**Returns:** `AudioHandle` for controlling playback, or `null` if event not found.

**Example:**
```csharp
AudioHandle handle = AudioManager.Instance.PostEvent("Play_Explosion");
AudioHandle handle2 = AudioManager.Instance.PostEvent("Play_Footstep", gameObject, transform.position);
```

---

### RTPC Methods

#### SetRTPC
```csharp
public void SetRTPC(string parameter, float value)
```
**Description:** Sets a Real-Time Parameter Control value.

**Parameters:**
- `parameter` - Parameter name (case-sensitive)
- `value` - Parameter value (typically 0-1, but depends on usage)

**Example:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", 0.75f);
AudioManager.Instance.SetRTPC("EngineRPM", 0.5f);
```

#### GetRTPC
```csharp
public float GetRTPC(string parameter)
```
**Description:** Gets current value of an RTPC parameter.

**Parameters:**
- `parameter` - Parameter name

**Returns:** Current value, or `0f` if parameter doesn't exist.

**Example:**
```csharp
float intensity = AudioManager.Instance.GetRTPC("CombatIntensity");
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
AudioManager.Instance.SetSwitch("Weather", "Rain");
```

#### GetSwitch
```csharp
public string GetSwitch(string group)
```
**Description:** Gets current switch value for a group.

**Parameters:**
- `group` - Switch group name

**Returns:** Current switch value, or empty string if not set.

**Example:**
```csharp
string surface = AudioManager.Instance.GetSwitch("Surface_Type");
```

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

**Parameters:**
- `stateGroup` - State group name

**Returns:** Active `AudioState`, or `null` if no state active.

#### GetActiveStateName
```csharp
public string GetActiveStateName(string stateGroup)
```
**Description:** Gets the name of the currently active state.

**Parameters:**
- `stateGroup` - State group name

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

#### GetBus
```csharp
public AudioBus GetBus(string busName)
```
**Description:** Gets a bus by name.

**Parameters:**
- `busName` - Bus name

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

### Volume Control Methods

#### SetMasterVolume
```csharp
public void SetMasterVolume(float linear)
```
**Description:** Sets master volume (global volume multiplier).

**Parameters:**
- `linear` - Volume in linear scale (0-1)

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
public void MuteAll()
```
**Description:** Mutes all audio.

#### UnmuteAll
```csharp
public void UnmuteAll()
```
**Description:** Unmutes all audio.

---

### Voice Management Methods

#### GetVoice
```csharp
public AudioVoiceEnhanced GetVoice()
```
**Description:** Allocates a voice from the pool. **Internal use only.**

**Returns:** Available voice, or `null` if pool exhausted.

#### ReturnVoice
```csharp
public void ReturnVoice(AudioVoiceEnhanced voice)
```
**Description:** Returns a voice to the pool. **Internal use only.**

#### GetStatistics
```csharp
public Statistics GetStatistics()
```
**Description:** Gets current audio system statistics.

**Returns:** `Statistics` struct with voice counts, etc.

**Example:**
```csharp
var stats = AudioManager.Instance.GetStatistics();
Debug.Log($"Real voices: {stats.realVoices}/{stats.maxRealVoices}");
```

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
**Read-only.**

#### Priority
```csharp
public VoicePriority Priority { get; }
```
**Description:** Voice stealing priority (Low, Medium, High, Critical).
**Read-only.**

#### MaxInstances
```csharp
public int MaxInstances { get; }
```
**Description:** Maximum concurrent instances (0 = unlimited).
**Read-only.**

#### StealBehavior
```csharp
public VoiceStealBehavior StealBehavior { get; }
```
**Description:** How to steal voices when max reached.
**Read-only.**

#### Cooldown
```csharp
public float Cooldown { get; }
```
**Description:** Minimum time between triggers in seconds.
**Read-only.**

---

### Methods

#### Post (Basic)
```csharp
public AudioHandle Post()
```
**Description:** Posts the event at origin (0,0,0), non-spatial.

**Returns:** `AudioHandle` for controlling playback.

**Example:**
```csharp
AudioHandle handle = myEvent.Post();
```

#### Post (With Parent)
```csharp
public AudioHandle Post(GameObject parent)
```
**Description:** Posts the event attached to a GameObject.

**Parameters:**
- `parent` - GameObject to attach to

**Returns:** `AudioHandle`.

**Example:**
```csharp
AudioHandle handle = myEvent.Post(gameObject);
```

#### Post (Full)
```csharp
public AudioHandle Post(GameObject parent, Vector3 position)
```
**Description:** Posts the event at specific 3D position, optionally attached.

**Parameters:**
- `parent` - GameObject to attach (or null)
- `position` - 3D world position

**Returns:** `AudioHandle`.

**Example:**
```csharp
AudioHandle handle = myEvent.Post(gameObject, transform.position);
AudioHandle handle2 = myEvent.Post(null, explosionPosition);
```

#### PostMulti
```csharp
public AudioMultiHandle PostMulti(AudioMultiPositionEmitterParent parent)
```
**Description:** Posts event in multi-position mode (synchronized across multiple emitters).

**Parameters:**
- `parent` - Multi-position emitter parent component

**Returns:** `AudioMultiHandle` for controlling all voices.

**Example:**
```csharp
AudioMultiHandle multiHandle = myEvent.PostMulti(emitterParent);
```

---

## AudioHandle & AudioMultiHandle

### AudioHandle

**Namespace:** `AudioSystem`
**Description:** Controls a single audio playback instance.

#### Properties

##### EventName
```csharp
public string EventName { get; }
```
**Description:** Name of the originating event.

##### SourceEvent
```csharp
public AudioEvent SourceEvent { get; }
```
**Description:** Reference to the source event.

##### IsPaused
```csharp
public bool IsPaused { get; }
```
**Description:** Whether playback is paused.

##### isPlaying
```csharp
public bool isPlaying { get; }
```
**Description:** Whether audio is currently playing.

##### time
```csharp
public float time { get; }
```
**Description:** Current playback time in seconds.

##### duration
```csharp
public float duration { get; }
```
**Description:** Total clip duration in seconds.

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

**Example:**
```csharp
handle.SetVolume(0.5f);  // Half volume
```

##### SetPitch
```csharp
public void SetPitch(float semitones)
```
**Description:** Sets pitch offset in semitones.

**Parameters:**
- `semitones` - Pitch offset (-12 to +12 typical range)

**Example:**
```csharp
handle.SetPitch(2f);   // 2 semitones up
handle.SetPitch(-3f);  // 3 semitones down
```

##### Stop
```csharp
public void Stop(float fadeTime = 0.1f)
```
**Description:** Stops playback with optional fade.

**Parameters:**
- `fadeTime` - Fade-out duration in seconds

**Example:**
```csharp
handle.Stop();      // Quick 0.1s fade
handle.Stop(2f);    // 2 second fade
```

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

---

### AudioMultiHandle

**Namespace:** `AudioSystem`
**Description:** Controls multiple synchronized voices (multi-position playback).

#### Properties

##### VoiceCount
```csharp
public int VoiceCount { get; }
```
**Description:** Number of voices in this multi-handle.

##### IsPlaying
```csharp
public bool IsPlaying { get; }
```
**Description:** Whether any voice is playing.

---

#### Methods

##### SetVolume
```csharp
public void SetVolume(float linear)
```
**Description:** Sets volume for all voices.

**Parameters:**
- `linear` - Volume multiplier (0-1)

##### SetPitch
```csharp
public void SetPitch(float semitones)
```
**Description:** Sets pitch for all voices.

**Parameters:**
- `semitones` - Pitch offset in semitones

##### Stop
```csharp
public void Stop(float fadeTime = 0.1f)
```
**Description:** Stops all voices with fade.

**Parameters:**
- `fadeTime` - Fade duration

##### StopImmediate
```csharp
public void StopImmediate()
```
**Description:** Stops all voices immediately (no fade).

##### Pause
```csharp
public void Pause()
```
**Description:** Pauses all voices.

##### Resume
```csharp
public void Resume()
```
**Description:** Resumes all paused voices.

---

## AudioContainer (Base)

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`
**Abstract:** Yes (use specific container types)

### Properties

#### ContainerName
```csharp
public string ContainerName { get; }
```
**Description:** Unique container identifier.

#### Description
```csharp
public string Description { get; }
```
**Description:** Designer notes/documentation.

#### Tags
```csharp
public List<string> Tags { get; }
```
**Description:** Searchable tags for organization.

#### MixerGroup
```csharp
public AudioMixerGroup MixerGroup { get; set; }
```
**Description:** Optional AudioMixerGroup override.

#### Is3D
```csharp
public bool Is3D { get; }
```
**Description:** Whether this container uses 3D spatialization.

#### MinDistance
```csharp
public float MinDistance { get; }
```
**Description:** Distance for full volume (3D only).

#### MaxDistance
```csharp
public float MaxDistance { get; }
```
**Description:** Distance for zero volume (3D only).

#### RolloffMode
```csharp
public AudioRolloffMode RolloffMode { get; }
```
**Description:** Volume attenuation curve (Logarithmic/Linear/Custom).

---

### Methods

#### Play
```csharp
public abstract AudioVoice Play(Vector3 position = default, GameObject parent = null)
```
**Description:** Plays the container. **Implemented by subclasses.**

**Parameters:**
- `position` - 3D world position
- `parent` - GameObject to attach to

**Returns:** `AudioVoice` instance.

#### Stop
```csharp
public abstract void Stop()
```
**Description:** Stops all active voices from this container.

#### StopImmediate
```csharp
public abstract void StopImmediate()
```
**Description:** Immediately stops all voices (no fade).

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

**Description:** Plays all assigned clips simultaneously.

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

**Description:** Randomly selects one clip per play, with weighting and repeat avoidance.

---

### SequenceContainer

**Menu:** Create > Audio System > Sequence Container

#### Properties

```csharp
public List<SequenceEntry> Entries { get; }
public PlaybackMode Mode { get; }
public bool LoopSequence { get; }
public float Volume { get; }
```

#### Methods

```csharp
public void ResetSequence()
public void SetIndex(int index)
```

**Description:** Plays clips in sequence (forward, reverse, ping-pong, or random order).

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
```

**Description:** Plays multiple containers with RTPC-driven crossfading.

---

## AudioBus

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

#### BusName
```csharp
public string BusName { get; }
```
**Description:** Bus identifier.

#### ParentBus
```csharp
public AudioBus ParentBus { get; }
```
**Description:** Parent bus in hierarchy (null if root).

#### MixerGroup
```csharp
public AudioMixerGroup MixerGroup { get; }
```
**Description:** Unity AudioMixerGroup for this bus.

#### VolumeDb
```csharp
public float VolumeDb { get; }
```
**Description:** Volume in decibels.

#### VolumeMultiplier
```csharp
public float VolumeMultiplier { get; }
```
**Description:** Linear volume multiplier (0-1).

#### Mute
```csharp
public bool Mute { get; set; }
```
**Description:** Mute this bus.

#### Solo
```csharp
public bool Solo { get; set; }
```
**Description:** Solo this bus (mute all others).

---

### Methods

#### SetVolume
```csharp
public void SetVolume(float volumeDb)
```
**Description:** Sets bus volume in dB.

**Parameters:**
- `volumeDb` - Volume in decibels

#### GetLinearVolume
```csharp
public float GetLinearVolume()
```
**Description:** Gets final linear volume (including parent hierarchy).

**Returns:** Combined volume (0-1).

---

## AudioState

**Namespace:** `AudioSystem`
**Inheritance:** `ScriptableObject`

### Properties

#### StateName
```csharp
public string StateName { get; }
```
**Description:** State identifier.

#### StateGroup
```csharp
public string StateGroup { get; }
```
**Description:** State group (mutually exclusive states).

---

### Methods

#### Apply
```csharp
public void Apply(float transitionTime = 0.5f)
```
**Description:** Applies this state's properties (bus volumes, RTPCs, switches, effects).

**Parameters:**
- `transitionTime` - Transition duration in seconds

---

## AudioVoiceEnhanced

**Namespace:** `AudioSystem`
**Description:** Internal voice representation. **Not typically accessed directly.**

### Properties

```csharp
public AudioSource source { get; }
public GainStack GainStack { get; }
public AudioEvent SourceEvent { get; }
public AudioContainer Container { get; }
public AudioBus Bus { get; }
public int Priority { get; }
public bool IsVirtual { get; }
public float VirtualTime { get; }
public double ScheduledStartTime { get; }
public AudioLowPassFilter LowPassFilter { get; }
```

---

## AudioExtensions

**Namespace:** `AudioSystem`
**Type:** Static utility class

### Extension Methods

#### PlayAtPosition
```csharp
public static AudioVoice PlayAtPosition(this AudioContainer container, Vector3 position)
```
**Description:** Plays container at specified position.

**Example:**
```csharp
myContainer.PlayAtPosition(explosionPos);
```

#### PlayAttached
```csharp
public static AudioVoice PlayAttached(this AudioContainer container, GameObject target)
```
**Description:** Plays container attached to GameObject.

**Example:**
```csharp
myContainer.PlayAttached(player);
```

#### PlayWithVolume
```csharp
public static AudioVoice PlayWithVolume(this AudioContainer container, float volumeScale,
    Vector3 position = default)
```
**Description:** Plays container with volume multiplier.

**Parameters:**
- `volumeScale` - Volume multiplier (0-1)
- `position` - Optional 3D position

**Example:**
```csharp
myContainer.PlayWithVolume(0.5f, transform.position);
```

---

### Conversion Methods

#### DbToLinear
```csharp
public static float DbToLinear(float db)
```
**Description:** Converts decibels to linear volume.

**Formula:** `10^(db/20)`

**Example:**
```csharp
float linear = AudioExtensions.DbToLinear(-6f);  // ~0.5
```

#### LinearToDb
```csharp
public static float LinearToDb(float linear)
```
**Description:** Converts linear volume to decibels.

**Formula:** `20 * log10(linear)` (or -80 if linear ≤ 0)

**Example:**
```csharp
float db = AudioExtensions.LinearToDb(0.5f);  // ~-6dB
```

#### CentsToPitch
```csharp
public static float CentsToPitch(float cents)
```
**Description:** Converts cents to pitch multiplier.

**Formula:** `2^(cents/1200)`

**Example:**
```csharp
float pitch = AudioExtensions.CentsToPitch(100f);  // 1.059 (1 semitone up)
```

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
    None,            // Don't steal, ignore new trigger
    Oldest,          // Steal longest-playing voice
    Quietest,        // Steal quietest voice
    LowestPriority   // Steal by priority only
}
```

---

### CrossfadeType
```csharp
public enum CrossfadeType
{
    Linear,      // Linear crossfade
    EqualPower   // Maintains perceived loudness
}
```

---

### PlaybackMode
```csharp
public enum PlaybackMode
{
    Forward,   // 0→1→2→3→0...
    Reverse,   // 3→2→1→0→3...
    PingPong,  // 0→1→2→3→2→1→0...
    Random     // Random order
}
```

---

### WeightedAudioClip
```csharp
[Serializable]
public class WeightedAudioClip
{
    public AudioClip clip;
    public float weight;           // Selection probability (0-10)
    public float volumeMultiplier; // Per-clip volume (0-1)
}
```

---

### SequenceEntry
```csharp
[Serializable]
public class SequenceEntry
{
    public AudioClip clip;
    public float volumeMultiplier;  // Per-clip volume
    public bool loop;                // Loop this clip
    public float delayAfter;         // Delay before next (0-5s)
}
```

---

### SwitchEntry
```csharp
[Serializable]
public class SwitchEntry
{
    public string switchValue;      // Value to match
    public AudioContainer container; // Container to play
}
```

---

### BlendEntry
```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;    // Layer container
    public AnimationCurve volumeCurve;  // RTPC → Volume mapping
}
```

---

### GainStack
```csharp
public class GainStack
{
    public float BaseGain;       // Container/Event volume
    public float BusGain;        // Bus hierarchy
    public float OcclusionGain;  // Occlusion attenuation
    public float RTPCGain;       // RTPC modulation
    public float SchedulerGain;  // Crossfade/transition

    public float GetFinalGain(); // Returns product of all gains
}
```

---

### Statistics
```csharp
public struct Statistics
{
    public int realVoices;
    public int maxRealVoices;
    public int virtualVoices;
    public int activeEvents;
}
```

**Usage:**
```csharp
var stats = AudioManager.Instance.GetStatistics();
Debug.Log($"Voices: {stats.realVoices}/{stats.maxRealVoices}");
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
```

**Multi-Position:**
```csharp
AudioMultiHandle mh = event.PostMulti(emitterParent);
```

---

## Version History

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

*For tutorials, see COOKBOOK.md*
*For deep knowledge, see MANUAL.md*
*For quick start, see QUICK_START.md*

*SFX System v1.0 - Professional Audio Middleware for Unity*
