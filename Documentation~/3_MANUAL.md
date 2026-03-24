# SFX System - Complete Technical Manual

**Version:** 2.2.0
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** March 2026

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [System Architecture](#2-system-architecture)
3. [Core Components](#3-core-components)
4. [Audio Containers](#4-audio-containers)
5. [Events System](#5-events-system)
6. [Bus & Mixing](#6-bus--mixing)
7. [State System](#7-state-system)
8. [Multi-Position Audio](#8-multi-position-audio)
9. [Performance & Optimization](#9-performance--optimization)
10. [Advanced Features](#10-advanced-features)
11. [Best Practices](#11-best-practices)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Introduction

### 1.1 What This Manual Covers

This manual provides complete technical documentation of the SFX System. By the end, you'll understand:

- **System Architecture** - How everything fits together
- **Component Deep-Dives** - Every class, method, and property explained
- **Design Patterns** - Industry-standard audio middleware concepts
- **Performance Optimization** - Voice management, LOD, virtualization
- **Best Practices** - Proven patterns from professional audio design
- **Troubleshooting** - Solutions to common problems

### 1.2 Prerequisites

Before reading this manual, you should:

- Have basic Unity knowledge (GameObjects, Components, Scripts)
- Understand audio concepts (dB, Hz, spatialization)
- Have read **QUICK_START.md** and successfully played a sound
- Know basic C# programming

### 1.3 How to Use This Manual

**For New Users:**
Read sections 1-6 in order to build a complete mental model of the system.

**For Experienced Users:**
Jump to specific sections as needed. Use the Table of Contents.

**For Troubleshooting:**
Skip to Section 12 for common issues and solutions.

### 1.4 Companion Documents

- **QUICK_START.md** - Get running in 10 minutes
- **COOKBOOK.md** - Step-by-step recipes for common tasks
- **API_REFERENCE.md** - Complete API documentation

---

## 2. System Architecture

### 2.1 Design Philosophy

The SFX System follows three core principles:

#### Principle 1: Data-Driven Design

**Traditional Approach:**
```csharp
// Hard-coded, tightly coupled
AudioSource.PlayClipAtPoint(explosionClip, transform.position);
```

**SFX System Approach:**
```csharp
// Data-driven, decoupled
explosionEvent.Post(gameObject, transform.position);
```

**Why It Matters:**
- Audio designers work independently in Unity Editor
- No code changes to modify sounds
- ScriptableObjects enable version control and team collaboration
- Iteration is instant (no recompile)

#### Principle 2: Event-Based Triggering

**What:** Code triggers events, not direct audio playback.

**Benefits:**
- **Decoupling:** Game logic doesn't know about audio implementation
- **Flexibility:** Same event can trigger different sounds based on game state
- **Organization:** Events are named, searchable assets
- **Power:** Events can execute multiple actions (play, stop, set RTPC, etc.)

**Example:**
```csharp
// One event, multiple outcomes based on state
footstepEvent.Post();
// → Plays grass sound if on grass
// → Plays metal sound if on metal
// → Controlled by switch system
```

#### Principle 3: Hierarchical Organization

**Buses:** Tree structure for mixing
**States:** Grouped for mutual exclusivity
**Containers:** Composable playback behaviors

**Example Hierarchy:**
```
Master Bus (-3dB)
├── SFX Bus (0dB)
│   ├── Player (0dB)
│   ├── Enemies (-3dB)
│   └── Ambience (-6dB)
├── Music Bus (-6dB)
└── Dialogue Bus (0dB)
```

### 2.2 The Audio Pipeline

Understanding the complete flow from code to speaker:

```
1. Game Code
   ↓ myEvent.Post(gameObject, position)

2. AudioEvent (ScriptableObject)
   ↓ Execute Actions (Play, SetRTPC, SetState, etc.)

3. Container Selection
   ↓ Via Switch/State/Direct reference

4. AudioContainer (ScriptableObject)
   ↓ container.Play(position, parent)

5. AudioManager
   ↓ GetVoice() from pool

6. AudioVoiceEnhanced
   ↓ Configure AudioSource, apply GainStack

7. AudioBus
   ↓ Route through bus hierarchy
   ↓ Apply volume, ducking, effects

8. Unity AudioSource
   ↓ Play AudioClip

9. AudioMixer (optional)
   ↓ Effects, routing

10. Speaker
    ↓ Final audio output
```

**Key Insight:** No direct AudioSource manipulation. The system manages the entire pipeline automatically.

### 2.3 Component Hierarchy

**ScriptableObject Assets (Design-Time):**
- AudioEvent
- AudioContainer (5 types)
- AudioBus
- AudioState

**Runtime Components:**
- AudioManager (MonoBehaviour singleton)
- AudioVoiceEnhanced (pooled instances)
- AudioHandle / AudioMultiHandle (playback control)

**Static Utilities:**
- AudioExtensions (helper methods)
- ListenerUtil (AudioListener caching)

### 2.4 Memory & Lifecycle

**Startup (Scene Load):**
1. AudioManager.Awake()
   - Singleton registration
   - DontDestroyOnLoad
   - Voice pool creation (32 real + 64 virtual by default)
   - Bus initialization
   - Load events from Resources/Audio/Events
   - Load states from Resources/Audio/States
   - Start coroutines (voice management, occlusion)

**Runtime (Playing):**
1. Event.Post() → Allocate voice from pool
2. Configure voice (clip, bus, position, gain stack)
3. Play AudioSource
4. Monitor (voice management coroutine)
5. Return to pool when finished

**Cleanup:**
- Voices return to pool automatically
- No runtime allocation/deallocation
- Predictable garbage collection

---

## 3. Core Components

### 3.1 AudioManager

**Location:** `Core/AudioManager.cs` (partial class across multiple files)

**Purpose:** Central singleton managing all audio operations.

#### Responsibilities

1. **Voice Lifecycle**
   - Create voice pool on startup
   - Allocate voices on demand
   - Track active/virtual voices
   - Return voices to pool when finished

2. **Bus Management**
   - Hierarchical volume control
   - Ducking automation
   - Mixer integration

3. **Event Processing**
   - Load events from Resources
   - Dispatch event actions
   - Instance limiting
   - Cooldown tracking

4. **State Management**
   - Load states from Resources
   - Track active state per group
   - Apply state transitions

5. **RTPC System**
   - Store parameter values
   - Notify listeners on changes
   - Drive blend containers

6. **Performance Systems**
   - Voice virtualization (LOD)
   - Voice stealing
   - Occlusion raycasting
   - Distance-based culling

#### Singleton Pattern

```csharp
public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<AudioManager>();
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeCore();
        // ...
    }
}
```

**Key Points:**
- Only one AudioManager exists
- Persists between scene loads
- Accessible globally via `AudioManager.Instance`

#### Inspector Configuration

| Property | Description | Default | When to Change |
|----------|-------------|---------|----------------|
| `masterVolume` | Global volume multiplier (0-1) | 1.0 | User settings menu |
| `muteAll` | Mute all audio | false | Debug/testing |
| `maxRealVoices` | Max physically playing voices | 32 | Mobile (16-24), High-end (48-64) |
| `maxVirtualVoices` | Max virtualized voices | 64 | Complex scenes with many sounds |
| `voiceUpdateInterval` | Voice management update rate (seconds) | 0.1 | Performance tuning |
| `enableOcclusion` | Raycast-based occlusion | true | Disable if not needed |
| `occlusionMask` | Layers that block sound | Everything | Ignore player, small objects |
| `occlusionUpdateInterval` | Occlusion raycast rate (seconds) | 0.2 | Performance tuning |
| `enableLOD` | Distance-based virtualization | true | Always leave enabled |
| `lodDistances` | Distance thresholds for LOD | [10, 25, 50, 100] | Scale to world size |

#### Public API Overview

**Events:**
```csharp
AudioHandle PostEvent(string eventName)
AudioHandle PostEvent(string eventName, GameObject parent, Vector3 position)
```

**RTPCs:**
```csharp
void SetRTPC(string parameter, float value)
float GetRTPC(string parameter)
```

**Switches:**
```csharp
void SetSwitch(string group, string value)
string GetSwitch(string group)
```

**States:**
```csharp
void SetState(string stateName, float transitionTime = 0.5f)
AudioState GetActiveState(string stateGroup)
```

**Buses:**
```csharp
void SetBusVolume(string busName, float volumeDb, float transitionTime = 0f)
AudioBus GetBus(string busName)
```

**Crossfading:**
```csharp
void CrossFade(AudioContainer from, AudioContainer to, float duration,
    AudioEvent.CrossfadeType type = AudioEvent.CrossfadeType.EqualPower)
```

### 3.2 AudioVoice & AudioVoiceEnhanced

**Location:** `Events/AudioVoiceEnhanced.cs`

**Purpose:** Represents a single playing audio instance with advanced features.

#### Class Hierarchy

```csharp
public class AudioVoice
{
    public AudioSource source;
    public GameObject parent;
    public bool isPlaying;
    public float volumeMultiplier;
    // Basic voice functionality
}

public class AudioVoiceEnhanced : AudioVoice
{
    public GainStack GainStack;              // Layered volume control
    public AudioEvent SourceEvent;           // Originating event
    public AudioContainer Container;         // Source container
    public AudioBus Bus;                     // Assigned bus
    public int Priority;                     // Voice stealing priority
    public bool IsVirtual;                   // Virtualization state
    public float VirtualTime;                // Time tracking when virtual
    public double ScheduledStartTime;        // DSP scheduled start
    public double ScheduledEndTime;          // DSP scheduled end
    public AudioLowPassFilter LowPassFilter; // Cached filter component
}
```

#### The GainStack System

**Concept:** Multiple volume layers multiply together for final volume.

```csharp
public class GainStack
{
    public float BaseGain = 1f;       // Container/Event base volume
    public float BusGain = 1f;        // Bus hierarchy contribution
    public float OcclusionGain = 1f;  // Raycast-based attenuation
    public float RtpcGain = 1f;       // RTPC-driven modulation
    public float SchedulerGain = 1f;  // Crossfade/transition gain
    public float DuckingGain = 1f;    // Ducking attenuation

    public float GetFinalGain()
    {
        return BaseGain * BusGain * OcclusionGain * RtpcGain * SchedulerGain * DuckingGain;
    }

    public void ApplyToSource(AudioSource source) { ... }
    public void Reset() { ... }
}
```

**Why Layers?**
Each system can independently control volume without knowing about others:
- Container sets BaseGain
- Bus hierarchy sets BusGain
- Occlusion system sets OcclusionGain
- RTPC system sets RtpcGain
- Ducking system sets DuckingGain
- Crossfade/scheduler sets SchedulerGain

**Example:**
```
BaseGain:       1.0   (container volume)
BusGain:        0.707 (bus at -3dB)
OcclusionGain:  0.5   (partially occluded)
RtpcGain:       0.8   (RTPC lowering volume)
DuckingGain:    1.0   (no ducking active)
SchedulerGain:  0.9   (fading in)

FinalGain = 1.0 * 0.707 * 0.5 * 0.8 * 0.9 * 1.0 = 0.254
```

#### Voice Priority System

**Enum Values:**
```csharp
public enum VoicePriority
{
    Low = 0,       // Background ambience, least important
    Medium = 64,   // Standard gameplay sounds
    High = 128,    // Important feedback (damage, pickups)
    Critical = 255 // UI, dialogue, never steal
}
```

**Voice Stealing:**
When voice count exceeds the pool limit, the system steals based on an importance score combining distance, volume, and priority:
1. Priority (steal lowest first)
2. If tied, use VoiceStealBehavior:
   - **Oldest:** Steal voice playing longest
   - **Quietest:** Steal quietest voice
   - **Furthest:** Steal most distant voice
   - **LowestPriority:** Only steal by priority

**Critical Voices:**
Never stolen, always play. Use for UI feedback and dialogue.

#### Voice Virtualization

**What Is It?**
A virtual voice tracks time but doesn't actually play audio.

**When Does It Happen?**
1. Voice count exceeds maxRealVoices (automatic)
2. Distance exceeds LOD thresholds (automatic)
3. Low priority + voice pressure (automatic)

**How It Works:**
```csharp
public void MakeVirtual()
{
    IsVirtual = true;
    VirtualTime = source.time; // Save current playback time
    source.Pause();            // Stop audio output
}

public void MakeReal()
{
    IsVirtual = false;
    source.time = VirtualTime; // Restore playback position
    source.UnPause();          // Resume audio output
}
```

**Virtual Time Tracking:**
```csharp
public void UpdateVirtualTime(float deltaTime)
{
    if (IsVirtual && source.clip != null)
    {
        VirtualTime += deltaTime;
        if (source.loop)
        {
            VirtualTime %= source.clip.length; // Loop correctly
        }
    }
}
```

**Key Insight:**
Virtualization is seamless. When a voice becomes real again, it resumes from the correct time. The player never notices.

#### DSP Scheduling

**Purpose:** Sample-accurate synchronization for multi-position audio.

```csharp
double dspTime = AudioSettings.dspTime;
ScheduledStartTime = dspTime + 0.1; // Start in 0.1 seconds
source.PlayScheduled(ScheduledStartTime);
```

**Use Cases:**
- Multi-position audio (all emitters start at exact same sample)
- Music layer synchronization
- Rhythmic audio sequences

### 3.3 AudioHandle & AudioMultiHandle

**Purpose:** Provide playback control after posting an event.

#### AudioHandle (Single Voice)

```csharp
public class AudioHandle
{
    // Properties
    public string EventName { get; }
    public AudioEvent SourceEvent { get; }
    public bool IsPaused { get; }
    public bool isPlaying { get; }
    public float time { get; }      // Current playback time
    public float duration { get; }  // Total clip length

    // Events
    public event System.Action OnStarted;
    public event System.Action OnLoop;
    public event System.Action OnFinished;

    // Methods
    public void SetVolume(float linear)
    public void SetPitch(float semitones)
    public void Stop(float fadeTime = 0.1f)
    public void Pause()
    public void Resume()
    public void Dispose()  // Clean up event handlers
}
```

**Usage:**
```csharp
AudioHandle handle = musicEvent.Post();
handle.OnStarted += () => Debug.Log("Music started");
handle.OnFinished += () => Debug.Log("Music finished");

// Later...
handle.SetVolume(0.5f);  // Half volume
handle.SetPitch(2f);     // 2 semitones up
handle.Stop(1f);         // Fade out over 1 second
```

#### AudioMultiHandle (Multiple Voices)

**Purpose:** Control multiple voices from multi-position playback.

```csharp
public class AudioMultiHandle
{
    // Properties
    public int VoiceCount { get; }
    public bool HasVoices { get; }
    public bool isPlaying { get; }
    public MultiPositionType PositionType { get; }

    // Global control (all voices)
    public void SetVolume(float linear)
    public void SetPitch(float semitones)
    public void Stop(float fadeTime = 0.1f)
    public void Pause()
    public void Resume()

    // Per-voice control
    public void SetVoiceVolume(int voiceIndex, float volumeMultiplier)
    public void SetVoicePitch(int voiceIndex, float semitones)
    public void StopVoice(int voiceIndex, float fadeTime = 0.1f)
    public void PauseVoice(int voiceIndex)
    public void ResumeVoice(int voiceIndex)

    // Position management
    public void UpdatePositions(Vector3[] newPositions)
    public void UpdatePositions(Transform[] newTransforms)
    public void RefreshPositions()
    public Vector3? GetVoicePosition(int voiceIndex)
    public Vector3[] GetAllPositions()
}
```

**Usage:**
```csharp
AudioMultiHandle multiHandle = myEvent.PostMultiPosition(emitterParent);
Debug.Log($"Playing on {multiHandle.VoiceCount} speakers");
multiHandle.SetVolume(0.75f);            // All speakers to 75% volume
multiHandle.SetVoiceVolume(0, 0.5f);     // First speaker to 50%
multiHandle.UpdatePositions(newTransforms); // Update positions dynamically
```

---

## 4. Audio Containers

Containers define how audio clips are organized and played. All containers inherit from `AudioContainer` base class.

### 4.1 Container Base Class

**Location:** `Containers/audio-container-base.cs`

#### Common Properties

Every container has these properties:

**Identity:**
```csharp
string ContainerName { get; }     // Unique identifier
string Description { get; }       // Designer notes
List<string> Tags { get; }        // Searchable organization tags
```

**Mixer Integration:**
```csharp
AudioMixerGroup MixerGroup { get; set; }  // Optional mixer routing
```

**Randomization:**
```csharp
bool enableVolumeRandomization;           // Add volume variation
float volumeRandomMin;                    // Min offset in dB (-12 to 12)
float volumeRandomMax;                    // Max offset in dB (-12 to 12)

bool enablePitchRandomization;            // Add pitch variation
float pitchRandomMin;                     // Min offset in cents (-1200 to 1200)
float pitchRandomMax;                     // Max offset in cents (-1200 to 1200)
```

**3D Spatialization:**
```csharp
bool Is3D { get; }                        // Enable 3D positioning
float MinDistance { get; }                 // Full volume distance
float MaxDistance { get; }                 // Zero volume distance
AudioRolloffMode RolloffMode { get; }     // Logarithmic/Linear/Custom
```

#### Common Methods

```csharp
public abstract AudioVoice Play(Vector3 position = default, GameObject parent = null);
public abstract void Stop();
public abstract void StopImmediate();
```

#### Randomization Implementation

```csharp
protected void ApplyRandomization(AudioVoice voice)
{
    if (enableVolumeRandomization)
    {
        float dbOffset = UnityEngine.Random.Range(volumeRandomMin, volumeRandomMax);
        float linearMultiplier = AudioExtensions.DbToLinear(dbOffset);
        voice.volumeMultiplier *= linearMultiplier;
    }

    if (enablePitchRandomization)
    {
        float centsOffset = UnityEngine.Random.Range(pitchRandomMin, pitchRandomMax);
        float pitchMultiplier = AudioExtensions.CentsToPitch(centsOffset);
        voice.source.pitch *= pitchMultiplier;
    }
}
```

**Why Randomization Matters:**
Prevents "machine gun effect" where repeated sounds become obviously repetitive.

**Recommended Values:**
- **Subtle variation:** Volume ±1-2 dB, Pitch ±50-100 cents
- **Dramatic variation:** Volume ±4-6 dB, Pitch ±200-300 cents

### 4.2 RoutingContainer

**Menu:** Create > Audio System > Routing Container

**Purpose:** Simplest container. Plays all assigned clips simultaneously.

#### Properties

```csharp
List<AudioClip> AudioClips { get; }  // Clips to play
float Volume { get; }                 // Base volume (0-1)
bool Loop { get; }                    // Loop all clips
```

#### When to Use

**Single One-Shot Sounds:**
```
Use Case: Button click
Setup: 1 AudioClip, no loop
```

**Multi-Layer Sounds:**
```
Use Case: Explosion
Setup: 3 AudioClips (blast + debris + shockwave)
Result: All play simultaneously for rich sound
```

**Background Loops:**
```
Use Case: Generator hum
Setup: 1-2 AudioClips, loop enabled
```

#### Implementation Details

```csharp
public override AudioVoice Play(Vector3 position = default, GameObject parent = null)
{
    if (AudioClips.Count == 0) return null;

    AudioVoice primaryVoice = null;

    // Play all clips
    foreach (var clip in AudioClips)
    {
        if (clip == null) continue;

        var voice = audioManager.GetVoice();
        if (voice == null) continue;

        voice.source.clip = clip;
        voice.source.loop = Loop;
        voice.source.volume = Volume;

        ConfigureAudioSource(voice.source, position, parent);
        ApplyRandomization(voice);

        voice.source.Play();
        activeVoices.Add(voice);

        if (primaryVoice == null) primaryVoice = voice;
    }

    return primaryVoice;
}
```

**Key Insight:** Returns first voice as "primary" for handle control, but all clips play.

### 4.3 RandomContainer

**Menu:** Create > Audio System > Random Container

**Purpose:** Picks one random clip per trigger, with weighting and repeat avoidance.

#### Properties

```csharp
List<WeightedAudioClip> AudioClips { get; }  // Clips with weights
int AvoidRepeatLast { get; }                  // Don't repeat last N clips (0-10)
bool UseWeighting { get; }                    // Enable weighted selection
float Volume { get; }                          // Base volume
bool Loop { get; }                             // Loop selected clip
```

#### WeightedAudioClip Structure

```csharp
[Serializable]
public class WeightedAudioClip
{
    public AudioClip clip;
    public float weight = 1f;           // Selection probability (0-10)
    public float volumeMultiplier = 1f; // Per-clip volume (0-1)
}
```

#### Weighted Selection Algorithm

```csharp
private AudioClip SelectWeightedClip()
{
    // Calculate total weight of available clips
    float totalWeight = 0f;
    foreach (var entry in availableClips)
        totalWeight += entry.weight;

    // Random value in range [0, totalWeight]
    float random = UnityEngine.Random.Range(0f, totalWeight);

    // Find clip at this weighted position
    float cumulative = 0f;
    foreach (var entry in availableClips)
    {
        cumulative += entry.weight;
        if (random <= cumulative)
            return entry.clip;
    }

    return availableClips[0].clip; // Fallback
}
```

**Example Weighting:**
```
Clip A: weight = 5.0  → 50% chance
Clip B: weight = 3.0  → 30% chance
Clip C: weight = 2.0  → 20% chance
Total:  10.0
```

#### Repeat Avoidance

```csharp
private List<AudioClip> recentlyPlayed = new List<AudioClip>();

private void AvoidRepeats()
{
    // Remove recently played clips from selection pool
    var available = AudioClips
        .Where(clip => !recentlyPlayed.Contains(clip.clip))
        .ToList();

    // If too many excluded, reset
    if (available.Count == 0)
    {
        recentlyPlayed.Clear();
        available = AudioClips.ToList();
    }

    return available;
}

private void TrackPlayed(AudioClip clip)
{
    recentlyPlayed.Add(clip);

    // Keep only last N
    while (recentlyPlayed.Count > AvoidRepeatLast)
        recentlyPlayed.RemoveAt(0);
}
```

#### When to Use

**Footsteps:**
```
Setup: 6-8 clips, avoidRepeatLast = 2-3, equal weights
Result: Natural variation, never obvious repetition
```

**Weapon Fire:**
```
Setup: 3-5 clips, avoidRepeatLast = 1, equal weights
Result: Each shot sounds different
```

**Voice Lines:**
```
Setup: 10+ clips, avoidRepeatLast = 3, custom weights
Example: Common phrases weight = 5, rare phrases weight = 1
```

**Best Practice:** Always set `avoidRepeatLast` to at least 1-2 for any repeated sound.

### 4.4 SequenceContainer

**Menu:** Create > Audio System > Sequence Container

**Purpose:** Plays clips in a defined order with various playback modes.

#### Properties

```csharp
List<SequenceEntry> Entries { get; }                       // Ordered clip list
SequenceContainer.PlaybackMode SequencePlaybackMode { get; } // Forward/Reverse/PingPong/Random
bool LoopSequence { get; }                                   // Loop entire sequence
bool AutoAdvance { get; set; }                               // Auto-advance after clip finishes
float Volume { get; }                                        // Base volume
```

#### SequenceEntry Structure

```csharp
[Serializable]
public class SequenceEntry
{
    public AudioClip clip;
    public float volumeMultiplier = 1f;  // Per-clip volume
    public bool loop;                     // Loop this specific clip
    public float delayAfter = 0f;        // Delay before next clip (0-5s)
}
```

#### Playback Modes

**Forward:**
```
Sequence: [A, B, C, D]
Playback: A → B → C → D → (loop to A)
```

**Reverse:**
```
Sequence: [A, B, C, D]
Playback: D → C → B → A → (loop to D)
```

**PingPong:**
```
Sequence: [A, B, C, D]
Playback: A → B → C → D → C → B → A → B → ...
```

**Random:**
```
Sequence: [A, B, C, D]
Playback: B → A → D → C → B → ...  (random each time)
```

#### Implementation

```csharp
private int currentIndex = 0;
private bool pingPongDirection = true; // true = forward

public override AudioVoice Play(Vector3 position = default, GameObject parent = null)
{
    if (Entries.Count == 0) return null;

    var entry = Entries[currentIndex];
    var voice = PlayEntry(entry, position, parent);

    AdvanceIndex();

    return voice;
}

private void AdvanceIndex()
{
    switch (Mode)
    {
        case PlaybackMode.Forward:
            currentIndex = (currentIndex + 1) % Entries.Count;
            break;

        case PlaybackMode.Reverse:
            currentIndex--;
            if (currentIndex < 0) currentIndex = Entries.Count - 1;
            break;

        case PlaybackMode.PingPong:
            if (pingPongDirection)
            {
                currentIndex++;
                if (currentIndex >= Entries.Count - 1)
                {
                    pingPongDirection = false;
                    currentIndex = Entries.Count - 1;
                }
            }
            else
            {
                currentIndex--;
                if (currentIndex <= 0)
                {
                    pingPongDirection = true;
                    currentIndex = 0;
                }
            }
            break;

        case PlaybackMode.Random:
            currentIndex = UnityEngine.Random.Range(0, Entries.Count);
            break;
    }
}
```

#### Public Methods

```csharp
public void ResetSequence()
{
    currentIndex = 0;
    pingPongDirection = true;
}

public void SetIndex(int index)
{
    currentIndex = Mathf.Clamp(index, 0, Entries.Count - 1);
}

public AudioVoice PlayNext(Vector3 position = default, GameObject parent = null)
{
    // Advances the sequence and plays the next entry
}
```

#### When to Use

**Alarm Sequences:**
```
Setup: [Beep, Silence, Beep, Silence]
Mode: Forward, Loop
delayAfter: 0.5s on each
```

**Musical Sequences:**
```
Setup: [Note1, Note2, Note3, Note4]
Mode: PingPong or Forward
Use for melodic patterns
```

**Tutorial Steps:**
```
Setup: [Step1VO, Step2VO, Step3VO]
Mode: Forward, No Loop
Called manually for each step
```

### 4.5 SwitchContainer

**Menu:** Create > Audio System > Switch Container

**Purpose:** Selects child container based on game state switch value.

#### Properties

```csharp
string SwitchGroupName { get; }              // Which switch to monitor
List<SwitchEntry> SwitchEntries { get; }     // Value→Container mappings
AudioContainer DefaultContainer { get; }      // Fallback if no match
float Volume { get; }                         // Volume multiplier
```

#### SwitchEntry Structure

```csharp
[Serializable]
public class SwitchEntry
{
    public string switchValue;       // Switch value to match (e.g., "Metal")
    public AudioContainer container; // Container to play
}
```

#### How It Works

```csharp
public override AudioVoice Play(Vector3 position = default, GameObject parent = null)
{
    // Get current switch value from AudioManager
    string switchValue = audioManager.GetSwitch(SwitchGroupName);

    // Find matching container
    AudioContainer containerToPlay = DefaultContainer;

    foreach (var entry in SwitchEntries)
    {
        if (entry.switchValue == switchValue && entry.container != null)
        {
            containerToPlay = entry.container;
            break;
        }
    }

    if (containerToPlay == null)
    {
        Debug.LogWarning($"No container for switch '{switchValue}'");
        return null;
    }

    // Play selected container
    var voice = containerToPlay.Play(position, parent);
    if (voice != null)
    {
        voice.volumeMultiplier *= Volume;
    }

    return voice;
}
```

#### Switch Groups Concept

**What:** Organizational concept for related switches.

**Example:**
```
Switch Group: "Surface_Type"
Values: "Grass", "Metal", "Wood", "Water", "Dirt"

Switch Group: "Weather"
Values: "Clear", "Rain", "Snow", "Fog"

Switch Group: "CharacterType"
Values: "PlayerMale", "PlayerFemale", "EnemyA", "EnemyB"
```

**Setting Switches:**
```csharp
// Set before posting event
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
footstepEvent.Post(gameObject, transform.position);
// → Plays metal footsteps automatically
```

#### When to Use

**Surface-Dependent Footsteps:**
```
Switch Group: "Surface_Type"
Entries:
  - "Grass" → GrassFootstepsContainer (RandomContainer with 6 grass clips)
  - "Metal" → MetalFootstepsContainer (RandomContainer with 5 metal clips)
  - "Wood" → WoodFootstepsContainer (RandomContainer with 6 wood clips)
Default: GenericFootstepsContainer
```

**Weather-Dependent Ambience:**
```
Switch Group: "Weather"
Entries:
  - "Clear" → ClearAmbienceContainer
  - "Rain" → RainAmbienceContainer
  - "Snow" → SnowAmbienceContainer
```

**Character-Specific Voice:**
```
Switch Group: "CharacterType"
Entries:
  - "PlayerMale" → MaleVoiceContainer
  - "PlayerFemale" → FemaleVoiceContainer
```

**Best Practice:** Always provide a `DefaultContainer` as fallback for unknown switch values.

### 4.6 BlendContainer

**Menu:** Create > Audio System > Blend Container

**Purpose:** Plays multiple containers simultaneously with RTPC-driven crossfading.

#### Properties

```csharp
List<BlendEntry> BlendEntries { get; }          // Layers to blend
string BlendParameterName { get; }              // RTPC to monitor
float Volume { get; }                            // Master volume
bool Loop { get; }                               // Loop all layers
IReadOnlyList<AudioVoice> LayerVoices { get; }  // Active layer voices
```

#### BlendEntry Structure

```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;    // Layer container
    public AnimationCurve volumeCurve;  // RTPC value (0-1) → Volume (0-1)
}
```

#### How It Works

**Startup:**
```csharp
public override AudioVoice Play(Vector3 position = default, GameObject parent = null)
{
    // Start ALL layers simultaneously
    AudioVoice primaryVoice = null;

    foreach (var entry in BlendEntries)
    {
        if (entry.container == null) continue;

        var voice = entry.container.Play(position, parent);
        if (voice == null) continue;

        voice.volumeMultiplier *= Volume;
        _layerVoices.Add(voice);

        if (primaryVoice == null) primaryVoice = voice;
    }

    // Register for RTPC updates
    audioManager.RegisterRTPCListener(this);

    // Initial blend value
    float initialValue = audioManager.GetRTPC(BlendParameterName);
    UpdateBlend(initialValue);

    return primaryVoice;
}
```

**Runtime Updates (RTPC Listener):**
```csharp
public void OnRTPCChanged(string parameter, float value)
{
    if (parameter == BlendParameterName)
        UpdateBlend(value);
}

public void UpdateBlend(float blendValue)
{
    blendValue = Mathf.Clamp01(blendValue);

    for (int i = 0; i < BlendEntries.Count && i < _layerVoices.Count; i++)
    {
        if (_layerVoices[i] != null && _layerVoices[i].isPlaying)
        {
            // Evaluate curve: RTPC value → Layer volume
            float layerVolume = BlendEntries[i].volumeCurve.Evaluate(blendValue);
            _layerVoices[i].SetVolume(layerVolume * Volume);
        }
    }
}
```

#### Designing Volume Curves

**Concept:** AnimationCurve maps RTPC value (0-1) to layer volume (0-1).

**Example: Combat Intensity Music (3 layers)**

**Layer 0 - Ambient:**
```
Curve:
  (0.0, 1.0) → Full volume at calm
  (1.0, 0.0) → Silent at combat
```

**Layer 1 - Tension:**
```
Curve:
  (0.0, 0.0)  → Silent at calm
  (0.5, 1.0)  → Full volume at medium intensity
  (1.0, 0.0)  → Silent at full combat
```

**Layer 2 - Combat:**
```
Curve:
  (0.0, 0.0) → Silent at calm
  (1.0, 1.0) → Full volume at combat
```

**Result:**
```
RTPC = 0.0:   Ambient: 100%, Tension: 0%,   Combat: 0%    (calm)
RTPC = 0.25:  Ambient: 75%,  Tension: 50%,  Combat: 0%    (alert)
RTPC = 0.5:   Ambient: 50%,  Tension: 100%, Combat: 50%   (tense)
RTPC = 0.75:  Ambient: 25%,  Tension: 50%,  Combat: 75%   (fighting)
RTPC = 1.0:   Ambient: 0%,   Tension: 0%,   Combat: 100%  (intense combat)
```

#### When to Use

**Dynamic Music:**
```
Layers: Ambient, Tension, Combat
RTPC: "CombatIntensity" (driven by enemy count/proximity)
```

**Vehicle Engine:**
```
Layers: Idle, Load, Redline
RTPC: "EngineRPM" (driven by speed/throttle)
```

**Weather Intensity:**
```
Layers: Calm, Moderate, Storm
RTPC: "WeatherIntensity"
```

**Proximity Blending:**
```
Layers: Distant, Medium, Close
RTPC: "DistanceToPlayer"
```

**Best Practice:** Design curves in Unity's Animation Curve editor for visual feedback.

---

## 5. Events System

### 5.1 AudioEvent Overview

**Location:** `Events/AudioEvent.cs`

**Purpose:** Triggers audio with complex behaviors and multiple actions.

#### Core Properties

```csharp
string EventName { get; }                    // Unique identifier
List<EventAction> Actions { get; }           // Actions to execute
VoicePriority Priority { get; }              // Voice stealing priority
int MaxInstances { get; }                    // Concurrent instance limit (0 = unlimited)
VoiceStealBehavior StealBehavior { get; }    // How to steal when max reached
float Cooldown { get; }                      // Minimum time between triggers
```

### 5.2 Event Actions

**Concept:** One event can execute multiple actions sequentially or with delays.

#### Available Action Types

```csharp
public enum ActionType
{
    Play,              // Play a container
    Stop,              // Stop a container
    Pause,             // Pause all voices
    Resume,            // Resume paused voices
    SetSwitch,         // Set a switch value
    SetRTPC,           // Set RTPC parameter
    SetState,          // Activate a state
    TriggerDucking,    // Duck target buses
    CrossFade          // Crossfade between containers
}
```

#### EventAction Structure

```csharp
[Serializable]
public class EventAction
{
    public ActionType actionType;
    public AudioContainer container;         // For Play/Stop/CrossFade
    public AudioBus targetBus;               // For Play (routing)
    public float delay;                      // Delay before executing (seconds)
    public float fadeDuration;               // For Stop/CrossFade

    // For SetSwitch
    public string switchGroup;
    public string switchValue;

    // For SetRTPC
    public string rtpcParameter;
    public float rtpcValue;

    // For SetState
    public string stateName;
    public float stateTransitionTime;

    // For CrossFade
    public AudioContainer fromContainer;
    public AudioContainer toContainer;
    public CrossfadeType crossfadeType;
}
```

#### Multi-Action Example

**Use Case:** Music with intro and loop

```
Event: "Start_MainMusic"

Action [0]:
  - Type: Play
  - Container: Music_Intro_RC
  - Target Bus: Music_Bus
  - Delay: 0s

Action [1]:
  - Type: Play
  - Container: Music_Loop_RC
  - Target Bus: Music_Bus
  - Delay: 8.5s  ← Length of intro
```

**Result:** Intro plays immediately, loop starts exactly when intro ends.

### 5.3 Instance Limiting

**Purpose:** Prevent event spam (e.g., 100 footsteps playing simultaneously).

#### Configuration

```csharp
[SerializeField] private int maxInstances = 0;  // 0 = unlimited
[SerializeField] private VoiceStealBehavior stealBehavior = VoiceStealBehavior.Oldest;
```

#### Steal Behaviors

**Oldest:**
```
Behavior: Stop oldest voice, play new one
Use For: Most sounds (footsteps, weapons)
```

**Quietest:**
```
Behavior: Stop quietest voice, play new one
Use For: Sounds where loudness matters (explosions)
```

**Furthest:**
```
Behavior: Stop most distant voice, play new one
Use For: Spatial sounds where proximity matters (ambience, 3D SFX)
```

**LowestPriority:**
```
Behavior: Stop lowest priority voice
Use For: Mixed priority scenarios
```

#### Implementation

```csharp
private readonly Dictionary<string, List<AudioVoiceEnhanced>> activeEventVoices
    = new Dictionary<string, List<AudioVoiceEnhanced>>();

public AudioHandle PostEvent(string eventName, GameObject parent, Vector3 position)
{
    if (!events.TryGetValue(eventName, out var evt)) return null;

    // Check cooldown
    if (IsOnCooldown(eventName)) return null;

    // Check instance limit
    if (evt.MaxInstances > 0)
    {
        var voices = GetActiveVoicesForEvent(eventName);

        if (voices.Count >= evt.MaxInstances)
        {
            // Steal voice based on configured behavior
            var voiceToSteal = SelectVoiceToSteal(voices, evt.StealBehavior);
            voiceToSteal.Stop(0.05f);
            voices.Remove(voiceToSteal);
        }
    }

    // Execute event actions...
}
```

### 5.4 Cooldown System

**Purpose:** Prevent rapid re-triggering of the same event.

```csharp
private readonly Dictionary<string, float> eventCooldowns = new Dictionary<string, float>();

private bool IsOnCooldown(string eventName)
{
    if (!events.TryGetValue(eventName, out var evt)) return false;
    if (evt.Cooldown <= 0f) return false;

    if (eventCooldowns.TryGetValue(eventName, out float lastTriggerTime))
    {
        if (Time.unscaledTime - lastTriggerTime < evt.Cooldown)
            return true; // Still on cooldown
    }

    eventCooldowns[eventName] = Time.unscaledTime;
    return false;
}
```

**Use Case:**
```
Event: UI_ButtonClick
Cooldown: 0.05s

User clicks rapidly → Only one sound per 50ms
Prevents audio spam and performance issues
```

### 5.5 Multi-Position Mode

**What:** Play the same event from multiple emitters simultaneously, perfectly synchronized.

#### Configuration

```csharp
[SerializeField] private bool enableMultiPosition = false;
```

#### How It Works

```csharp
public AudioMultiHandle PostMultiPosition(AudioMultiPositionEmitterParent emitterParent, GameObject source = null)
{
    if (!enableMultiPosition)
    {
        Debug.LogError("Multi-position not enabled on this event");
        return null;
    }

    var positions = parent.GetActivePositions();
    if (positions.Length == 0) return null;

    // Get DSP time for synchronization
    double dspTime = AudioSettings.dspTime + 0.1; // 100ms latency buffer

    var voices = new List<AudioVoiceEnhanced>();

    foreach (var position in positions)
    {
        var voice = PlayAtPosition(position, dspTime);
        if (voice != null)
            voices.Add(voice);
    }

    return new AudioMultiHandle(voices);
}
```

**Key Insight:** All voices use `PlayScheduled(dspTime)` for sample-accurate sync.

---

## 6. Bus & Mixing

### 6.1 Bus Architecture

**Location:** `Events/AudioBus.cs`

**Purpose:** Hierarchical volume control and mixing.

#### Properties

```csharp
string BusName { get; }                  // Identifier
AudioBus ParentBus { get; }              // Hierarchy
AudioMixerGroup MixerGroup { get; }      // Unity AudioMixer integration
float VolumeDb { get; }                  // Volume in decibels
float VolumeMultiplier { get; }          // Linear multiplier (0-1)
bool Mute { get; set; }                  // Mute this bus
bool Solo { get; set; }                  // Solo this bus
```

### 6.2 Decibel System

**Why dB instead of 0-1?**

Decibels match how humans perceive loudness (logarithmic, not linear).

**Conversion Formulas:**
```csharp
// dB to linear
float linear = Mathf.Pow(10f, db / 20f);

// Linear to dB
float db = 20f * Mathf.Log10(linear);
```

**Key dB Values:**
```
 +6 dB = 2x louder
  0 dB = No change (1.0 linear)
 -6 dB = ~0.5x quieter
-12 dB = ~0.25x quieter
-20 dB = ~0.1x quieter
-80 dB = Silence (effective zero)
```

### 6.3 Hierarchical Volume

**Concept:** Child buses inherit parent volume.

```
Master Bus: -3 dB (0.707 linear)
└── SFX Bus: 0 dB (1.0 linear)
    └── Weapons Bus: +3 dB (1.41 linear)

Final Weapons Volume = 0.707 * 1.0 * 1.41 ≈ 1.0
```

**Implementation:**
```csharp
public float GetLinearVolume()
{
    float volume = AudioExtensions.DbToLinear(VolumeDb) * VolumeMultiplier;

    // Multiply by parent volume
    if (ParentBus != null)
        volume *= ParentBus.GetLinearVolume();

    return volume;
}
```

### 6.4 Ducking System

**What:** Automatically lower volume of target buses when this bus plays.

**Use Case:** Music ducks when dialogue plays.

#### Configuration

```csharp
bool EnableDucking { get; }                    // Enable ducking feature
List<DuckingTarget> DuckingTargets { get; }    // Buses to duck
float DuckingAttack { get; }                    // Fade down time (default 0.05s)
float DuckingRelease { get; }                   // Fade up time (default 0.5s)
```

#### DuckingTarget Structure

```csharp
[Serializable]
public class DuckingTarget
{
    public AudioBus targetBus;        // Bus to duck
    public float duckAmount = 0.5f;   // How much to duck (0-1, where 1 = full duck)
}
```

#### Example Setup

```
Dialogue Bus:
  - enableDucking: true
  - duckingTargets:
      - Music Bus, duckAmount: 0.7 (duck to 30% volume)
      - SFX Bus, duckAmount: 0.5 (duck to 50% volume)
  - duckingAttack: 0.05s (quick duck)
  - duckingRelease: 0.8s (slow return)
```

**Result:** When dialogue plays, music quickly drops to 30%, SFX to 50%. When dialogue stops, they smoothly return over 0.8s.

### 6.5 Unity AudioMixer Integration

**Setup:**
1. Create AudioMixerGroup in Unity's AudioMixer window
2. Assign to bus's `mixerGroup` field
3. System routes voices through mixer automatically

**Benefits:**
- Use Unity's built-in effects (reverb, EQ, etc.)
- Visual mixing interface
- Snapshots and ducking (separate from SFX System ducking)

---

## 7. State System

### 7.1 AudioState Overview

**Location:** `Events/AudioState.cs`

**Purpose:** Change multiple audio parameters based on game mode.

#### Properties

```csharp
string StateName { get; }                         // Unique identifier
string StateGroup { get; }                        // Mutually exclusive group
List<BusVolumeProperty> BusVolumes { get; }      // Bus volume changes
List<SwitchProperty> SwitchValues { get; }        // Switch changes
List<RTPCProperty> RTPCValues { get; }            // RTPC changes
List<EffectProperty> EffectProperties { get; }    // Mixer effect changes
```

### 7.2 State Groups

**Concept:** States in the same group are mutually exclusive.

**Example:**
```
State Group: "Location"
├── State: "Normal"
├── State: "Underwater"
└── State: "Cave"

State Group: "GameplayState"
├── State: "Menu"
├── State: "Playing"
└── State: "Paused"
```

**Behavior:**
- Only one state per group is active at a time
- Setting "Underwater" automatically deactivates "Normal" and "Cave"
- Different groups can have active states simultaneously

### 7.3 State Properties

#### BusVolumeProperty

```csharp
[Serializable]
public class BusVolumeProperty
{
    public AudioBus bus;
    public float volumeDb;
}
```

**Example:**
```
State: "Underwater"
BusVolumes:
  - SFX Bus: -9 dB (muffle sounds)
  - Music Bus: -12 dB (distant music)
  - Dialogue Bus: -6 dB (slightly muffled)
```

#### SwitchProperty

```csharp
[Serializable]
public class SwitchProperty
{
    public string switchGroup;
    public string switchValue;
}
```

**Example:**
```
State: "Underwater"
SwitchValues:
  - switchGroup: "Location", switchValue: "Underwater"
  - switchGroup: "Reverb", switchValue: "LargeHall"
```

#### RTPCProperty

```csharp
[Serializable]
public class RTPCProperty
{
    public string parameterName;
    public float value;
}
```

**Example:**
```
State: "Combat"
RTPCValues:
  - "MusicIntensity": 1.0
  - "HeartbeatIntensity": 0.8
```

#### EffectProperty

```csharp
[Serializable]
public class EffectProperty
{
    public AudioBus bus;
    public string propertyName;
    public float value;
}
```

**Example:**
```
State: "Underwater"
EffectProperties:
  - Bus: Master, Property: "ReverbMix", Value: 0.8
  - Bus: SFX, Property: "LowpassCutoff", Value: 1000.0
```

### 7.4 State Transitions

**Transition Time:** Controls how smoothly parameters change.

```csharp
public void SetState(string stateName, float transitionTime = 0.5f)
{
    if (!states.TryGetValue(stateName, out var nextState)) return;

    // Update active state for this group
    activeStatesByGroup[nextState.StateGroup] = nextState;

    // Apply with transition
    nextState.Apply(transitionTime);
}
```

**Apply Implementation:**
```csharp
public void Apply(float transitionTime)
{
    // Transition bus volumes
    foreach (var busVol in BusVolumes)
    {
        if (busVol.bus != null)
            audioManager.TransitionBusVolume(busVol.bus, busVol.volumeDb, transitionTime);
    }

    // Set switches immediately
    foreach (var sw in SwitchValues)
    {
        audioManager.SetSwitch(sw.switchGroup, sw.switchValue);
    }

    // Set RTPCs with smooth transition
    foreach (var rtpc in RTPCValues)
    {
        audioManager.SetRTPC(rtpc.parameterName, rtpc.value);
    }

    // Transition effect properties
    foreach (var effect in EffectProperties)
    {
        if (effect.bus != null)
            audioManager.SetBusEffectProperty(effect.bus, effect.propertyName, effect.value, transitionTime);
    }
}
```

### 7.5 Common State Patterns

**Underwater State:**
```
StateGroup: "Location"
StateName: "Underwater"

BusVolumes:
  - SFX: -9dB
  - Music: -12dB
  - Ambience: -6dB

SwitchValues:
  - "Location": "Underwater"

RTPCValues:
  - "Underwater": 1.0

TransitionTime: 1.5s
```

**Pause State:**
```
StateGroup: "GameplayState"
StateName: "Paused"

BusVolumes:
  - SFX: -20dB
  - Music: -12dB
  - UI: 0dB (unchanged)

TransitionTime: 0.3s
```

**Combat State:**
```
StateGroup: "CombatState"
StateName: "InCombat"

RTPCValues:
  - "MusicIntensity": 1.0
  - "HeartbeatIntensity": 0.7

TransitionTime: 2.0s
```

---

## 8. Multi-Position Audio

### 8.1 Concept

**What:** Multiple speakers playing synchronized audio at different 3D positions.

**Real-World Example:** Nightclub with 8 speakers playing the same music.

**Challenge:** Without synchronization, speakers have phasing issues (sounds canceling out).

**Solution:** DSP-scheduled playback ensures sample-accurate synchronization.

### 8.2 Architecture

#### AudioMultiPositionEmitterParent

**Component:** Manages child emitters

**Properties:**
```csharp
int maxEmitters = 16;                    // Limit (1-16)
bool autoFindChildren = true;            // Auto-detect children
bool autoUpdateChildren = true;          // Update when hierarchy changes
```

**Methods:**
```csharp
Vector3[] GetActivePositions();          // Positions of active emitters
Transform[] GetActiveTransforms();       // Transforms of active emitters
int ActiveEmitterCount { get; }          // Number active
void EnableAllEmitters();
void DisableAllEmitters();
void SetAllVolumes(float volume);
void RefreshChildren();
```

#### AudioMultiPositionEmitterChild

**Component:** Individual emitter point

**Properties:**
```csharp
bool isActive = true;                    // Enable/disable
float volumeMultiplier = 1f;             // Per-emitter volume (0-1)
bool useDirectionality = false;          // Enable cone emission
float coneAngle = 90f;                   // Emission cone angle (0-360)
```

**Properties (Read-Only):**
```csharp
Vector3 Position { get; }                // World position
Quaternion Rotation { get; }             // World rotation
Vector3 Forward { get; }                 // Forward direction
```

### 8.3 DSP Synchronization

**Problem Without Sync:**
```
Speaker A starts: Frame 1, Sample 0
Speaker B starts: Frame 1, Sample 512  ← Delayed by 512 samples
Speaker C starts: Frame 2, Sample 0    ← Delayed by ~2000 samples

Result: Phasing, echoing, muddy sound
```

**Solution With DSP Scheduling:**
```csharp
double dspTime = AudioSettings.dspTime + 0.1; // Schedule 100ms ahead

foreach (var position in emitterPositions)
{
    var voice = GetVoice();
    voice.source.clip = clip;
    voice.source.transform.position = position;
    voice.scheduledStartTime = dspTime;
    voice.source.PlayScheduled(dspTime);
}
```

**Result:** All speakers start at exact same DSP sample, perfect synchronization.

### 8.4 Setup Tutorial

**Step 1: Create Parent**
```
1. Create empty GameObject: "NightclubSpeakers"
2. Add component: AudioMultiPositionEmitterParent
3. Configure:
   - maxEmitters: 8
   - autoFindChildren: ✓
```

**Step 2: Create Children (Manual)**
```
1. Create child GameObject: "Speaker_01"
2. Position at (10, 2, 0)
3. Add component: AudioMultiPositionEmitterChild
4. Repeat for 8 speakers in circle pattern
```

**Step 2: Create Children (Auto - Inspector Button)**
```
1. Select parent
2. In Inspector, click "Create 8 Child Emitters (Circle Pattern)"
3. Automatically creates 8 speakers in circle
```

**Step 3: Create Event**
```
1. Create AudioEvent
2. Enable "Multi-Position Mode"
3. Add action: Play music container
```

**Step 4: Script**
```csharp
using UnityEngine;
using AudioSystem;

public class NightclubAudio : MonoBehaviour
{
    [SerializeField] private AudioEvent musicEvent;
    private AudioMultiHandle musicHandle;

    void Start()
    {
        var parent = GetComponent<AudioMultiPositionEmitterParent>();
        musicHandle = musicEvent.PostMultiPosition(parent);

        Debug.Log($"Playing on {musicHandle.VoiceCount} speakers");
    }

    void OnDestroy()
    {
        musicHandle?.Stop(2f); // Fade out over 2s
    }
}
```

### 8.5 Per-Emitter Control

**Volume:**
```csharp
// Individually
emitterChild.VolumeMultiplier = 0.75f;

// All at once
emitterParent.SetAllVolumes(0.5f);
```

**Active State:**
```csharp
// Individual
emitterChild.IsActive = false; // Disable this speaker

// All
emitterParent.DisableAllEmitters();
emitterParent.EnableAllEmitters();
```

**Directionality:**
```csharp
emitterChild.UseDirectionality = true;
emitterChild.ConeAngle = 90f;  // 90° emission cone
// Emitter emits in transform.forward direction
```

### 8.6 Use Cases

**Nightclub Speakers:**
```
Setup: 8 speakers in circle
Audio: Looping music
Result: Immersive club environment
```

**Stadium Announcer:**
```
Setup: 12 speakers around stadium
Audio: Announcer voice
Result: Realistic PA system
```

**Vehicle Multi-Speaker:**
```
Setup: 4 speakers (FL, FR, RL, RR)
Audio: Radio/music
Result: Realistic car audio
```

**Ambient Sound Array:**
```
Setup: 6 speakers in outdoor environment
Audio: Wind/nature ambience
Result: Enveloping soundscape
```

### 8.7 Performance Considerations

**Voice Count:**
- 1 multi-position event with 8 emitters = 8 voices
- Monitor total voice count: `AudioManager.Instance.GetStatistics()`

**Instance Limiting:**
```
Event: Multi-position music
MaxInstances: 1  ← Only one instance of entire array
```

**Occlusion:**
- Each emitter can be individually occluded
- Can be expensive with many emitters
- Consider disabling occlusion for some emitters

**Distance Culling:**
- LOD system works per-voice
- Distant emitters virtualize automatically
- No special handling needed

---

## 9. Performance & Optimization

### 9.1 Voice Management

#### Voice Limits

**Default Configuration:**
```csharp
maxRealVoices = 32;      // Physically playing sounds
maxVirtualVoices = 64;   // Virtualized sounds (time-tracking only)
```

**What Happens When Limit Reached:**
1. System attempts to virtualize low-priority voices
2. If all voices are high-priority, steal based on `VoiceStealBehavior`
3. Critical priority voices are never stolen

**Recommended Settings:**
```
Desktop/Console: 32-64 real voices
Mobile: 16-24 real voices
VR: 24-32 real voices (high frame rate priority)
```

#### Voice Stealing Strategies

**By Priority:**
```csharp
public enum VoicePriority
{
    Low = 0,       // Steal first: distant ambience
    Medium = 64,   // Standard gameplay
    High = 128,    // Important: player actions
    Critical = 255 // Never steal: UI, dialogue
}
```

**By Behavior:**
```csharp
public enum VoiceStealBehavior
{
    Oldest,          // Steal longest-playing voice
    Quietest,        // Steal quietest voice
    Furthest,        // Steal most distant voice
    LowestPriority   // Steal by priority only
}
```

**Best Practice:**
```
UI sounds: Critical priority
Player footsteps: High priority
Enemy footsteps: Medium priority
Distant ambience: Low priority
```

#### Voice Virtualization

**What Gets Virtualized:**
1. Voices beyond `maxRealVoices` limit
2. Low-priority voices when system is stressed
3. Voices beyond LOD distances
4. Optionally: looping ambience when not critical

**How It Works:**
```csharp
// Becoming virtual
voice.MakeVirtual();
// - Pauses AudioSource
// - Tracks time internally
// - No audio output
// - Minimal CPU cost

// Becoming real again
voice.MakeReal();
// - Resumes AudioSource at correct time
// - Seamless to player
// - Handles looping correctly
```

**Monitoring:**
```csharp
void OnGUI()
{
    var stats = AudioManager.Instance.GetStatistics();
    GUI.Label(new Rect(10, 10, 400, 20),
        $"Real: {stats.activeVoices}/{stats.totalVoices}, " +
        $"Virtual: {stats.availableVoices}");
}
```

### 9.2 Memory Optimization

#### AudioClip Import Settings

**UI Sounds (Small, Frequent):**
```
Load Type: Decompress On Load
Compression Format: PCM
Quality: 100%

Reason: Small file size, instant playback, minimal CPU
```

**SFX (Medium, Common):**
```
Load Type: Compressed In Memory
Compression Format: Vorbis (mobile) or ADPCM (desktop)
Quality: 70-90%

Reason: Balance of file size, memory, and CPU
```

**Music (Large, Streaming):**
```
Load Type: Streaming
Compression Format: Vorbis
Quality: 70-80%

Reason: Large files stream from disk, low memory cost
```

**Ambience (Large Loops):**
```
Load Type: Streaming
Compression Format: Vorbis
Quality: 60-70%

Reason: Long loops, stream from disk
```

#### Resource Loading Strategy

**Startup (Automatic):**
```csharp
// AudioManager.Awake()
var audioEvents = Resources.LoadAll<AudioEvent>("Audio/Events");
var audioStates = Resources.LoadAll("Audio/States", typeof(AudioState));
```

**Key Insight:** Events and States load once at startup. Containers and clips are referenced, not duplicated.

**Memory Footprint:**
```
Events: Minimal (ScriptableObject metadata)
States: Minimal (ScriptableObject metadata)
Containers: Minimal (references to clips)
Clips: Depends on import settings
Voices: Fixed pool (32 AudioSources = ~32KB)
```

### 9.3 CPU Optimization

#### Update Intervals

**Voice Management:**
```csharp
voiceUpdateInterval = 0.1f; // 10 times per second

// What it does:
// - Check if voices finished
// - Update virtualization
// - Clean up inactive voices
```

**Tuning:**
- Faster (0.05s): More responsive, higher CPU
- Slower (0.2s): Lower CPU, less responsive
- Recommended: 0.1s for most games

**Occlusion:**
```csharp
occlusionUpdateInterval = 0.2f; // 5 times per second

// What it does:
// - Raycast from listener to each voice
// - Update occlusion gain
// - Update low-pass filter
```

**Tuning:**
- Faster (0.1s): Smooth occlusion, high CPU
- Slower (0.3-0.5s): Lower CPU, slight lag
- Recommended: 0.2s for most games, 0.3-0.5s for mobile

#### Occlusion Optimization

**Layer Mask:**
```csharp
// Include only large geometry
occlusionMask = LayerMask.GetMask("Environment", "Buildings");

// Exclude:
// - Small props
// - Moving objects
// - Player
// - Enemies
```

**Disable When Not Needed:**
```csharp
// If game is 2D or top-down
AudioManager.Instance.enableOcclusion = false;
```

**Per-Voice Occlusion:**
```
Only enable for important 3D sounds
Disable for:
  - 2D sounds (UI, music)
  - Distant ambience
  - Non-critical sounds
```

#### Distance-Based LOD

**LOD Distances:**
```csharp
lodDistances = new float[] { 10f, 25f, 50f, 100f };

// Behavior:
// < 10m:  Full fidelity
// 10-25m: Start virtualizing low-priority
// 25-50m: Virtualize medium-priority
// 50-100m: Virtualize most voices
// > 100m: Virtualize all but critical
```

**Tuning:**
- Small levels: [5, 15, 30, 60]
- Large open world: [25, 50, 100, 200]
- Indoor: [5, 10, 20, 40]

#### Listener Cache Management

**Optimization:** The system caches the AudioListener reference to avoid repeated FindObjectOfType calls.

**How It Works:**
```csharp
// Cache is retained until listener is destroyed or null
// No timer-based refresh - only refreshes when actually needed
Transform listener = ListenerUtil.Get();
```

**⚠️ IMPORTANT: When to Manually Refresh**

**Standard Usage (Automatic):**
- ✅ One camera with AudioListener
- ✅ Switching cameras by destroying old one
- ✅ Scene loading (old listener destroyed)
- **Result:** Cache automatically refreshes - no action needed

**Advanced Scenarios (Manual Refresh Required):**

**Scenario 1: Camera swap without destroying old listener**
```csharp
// When switching cameras but keeping old one alive (disabled)
oldCamera.GetComponent<AudioListener>().enabled = false;
newCamera.GetComponent<AudioListener>().enabled = true;

// ⚠️ REQUIRED: Tell audio system about the change
ListenerUtil.Set(newCamera.transform);
```

**Scenario 2: Multiple listeners (unusual but possible)**
```csharp
// If you temporarily have multiple listeners and want specific one
ListenerUtil.Set(primaryCamera.transform);
```

**Scenario 3: Force refresh for debugging**
```csharp
// Clear cache to force next Get() to search
ListenerUtil.Invalidate();
// Next Get() call will find listener again
```

**When Manual Refresh is NOT Needed:**
- ❌ Scene transitions (automatic)
- ❌ Camera destruction/creation (automatic)
- ❌ Listener component added/removed (automatic if destroyed)
- ❌ Standard gameplay (automatic)

**Best Practice:**
Most projects never need manual refresh. Only call `Set()` or `Invalidate()` if you:
1. Swap cameras without destroying the old one
2. Disable/enable AudioListener components directly
3. Have a specific reason the cache might be stale

### 9.4 Mobile Optimization

**Recommended Settings:**
```csharp
maxRealVoices = 16;               // Lower limit
maxVirtualVoices = 32;            // Lower limit
voiceUpdateInterval = 0.15f;      // Slower updates
occlusionUpdateInterval = 0.4f;   // Much slower
enableOcclusion = false;          // Consider disabling
```

**Clip Settings:**
```
All clips: Compressed In Memory or Streaming
Compression: Vorbis
Quality: 60-70% (lower for mobile)
```

**3D Audio:**
```
Use sparingly
Prefer 2D when possible
Limit max distance to reduce spatialization cost
```

**Best Practices:**
```
✓ Use instance limiting aggressively
✓ Lower voice counts
✓ Increase update intervals
✓ Disable occlusion or use very slow interval
✓ Profile on target device
```

### 9.5 Profiling

#### Unity Profiler

**Audio Module:**
```
Shows:
- Active AudioSource count
- AudioClip memory usage
- DSP buffer usage
- Audio thread time
```

**What to Look For:**
```
High voice count → Increase instance limits
High audio memory → Check clip import settings
Audio thread spikes → Reduce update frequencies
```

#### Custom Debug Tools

**Voice Count Display:**
```csharp
public class AudioDebugUI : MonoBehaviour
{
    void OnGUI()
    {
        var stats = AudioManager.Instance.GetStatistics();
        int y = 10;

        GUI.Label(new Rect(10, y, 400, 20),
            $"Real Voices: {stats.activeVoices}/{stats.totalVoices}");
        y += 20;

        GUI.Label(new Rect(10, y, 400, 20),
            $"Virtual Voices: {stats.availableVoices}");
        y += 20;

        GUI.Label(new Rect(10, y, 400, 20),
            $"Active Events: {stats.activeEvents}");
    }
}
```

**Event Logging:**
```csharp
AudioHandle handle = myEvent.Post();
handle.OnStarted += () => Debug.Log($"Started: {handle.EventName}");
handle.OnFinished += () => Debug.Log($"Finished: {handle.EventName}");
```

---

## 10. Advanced Features

### 10.1 Occlusion System

**How It Works:**
1. Raycast from AudioListener to each voice
2. If blocked, reduce volume + apply low-pass filter
3. Smooth transitions via lerp

**Implementation:**
```csharp
private IEnumerator OcclusionUpdate()
{
    while (true)
    {
        if (Time.unscaledTime - lastOcclusionUpdate > occlusionUpdateInterval)
        {
            lastOcclusionUpdate = Time.unscaledTime;
            var listener = ListenerUtil.Get();

            foreach (var voice in realVoices)
            {
                if (voice?.source == null) continue;

                Vector3 a = listener.position;
                Vector3 b = voice.source.transform.position;
                bool occluded = Physics.Linecast(a, b, occlusionMask);

                float targetGain = occluded ? 0.3f : 1f;
                float lerpSpeed = Mathf.Clamp01(occlusionUpdateInterval * 5f);
                voice.GainStack.OcclusionGain = Mathf.Lerp(
                    voice.GainStack.OcclusionGain,
                    targetGain,
                    lerpSpeed);

                voice.UpdateFinalVolume();

                // Low-pass filter
                if (voice.LowPassFilter != null)
                {
                    voice.LowPassFilter.enabled = occluded;
                    if (occluded)
                        voice.LowPassFilter.cutoffFrequency =
                            Mathf.Lerp(22000f, 500f, 1f - targetGain);
                }
            }
        }
        yield return null;
    }
}
```

**Performance Cost:**
```
Cost per frame = (realVoiceCount / updateInterval) raycasts

Example:
20 real voices, 0.2s interval = 100 raycasts/second = ~2 raycasts/frame @ 60fps
```

**Optimization:**
```
✓ Use coarse occlusionMask (only large geometry)
✓ Increase update interval on mobile
✓ Disable for non-critical sounds
✓ Disable entirely for 2D games
```

### 10.2 Ducking

**Automatic Volume Reduction:**

**Setup on Trigger Bus (e.g., Dialogue):**
```csharp
enableDucking = true;
duckingTargets = [
    { targetBus: Music_Bus, duckAmount: 0.7 },  // Duck to 30%
    { targetBus: SFX_Bus, duckAmount: 0.5 }     // Duck to 50%
];
duckingAttack = 0.05f;   // Quick duck
duckingRelease = 0.8f;   // Slow return
```

**Behavior:**
```
Dialogue starts → Music/SFX duck over 0.05s
Dialogue ends → Music/SFX return over 0.8s
```

**Common Patterns:**
```
Dialogue ducks Music: 70-80%
Dialogue ducks SFX: 40-60%
Important SFX ducks Music: 30-50%
UI sounds duck nothing: 0%
```

### 10.3 Crossfading

**Types:**

**Linear:**
```csharp
float CrossfadeLinear(float x)
{
    return x; // Straight line
}
// Results in slight volume dip in middle
```

**Equal-Power (Recommended):**
```csharp
float CrossfadeEqualPower(float x)
{
    return Mathf.Sin(x * Mathf.PI * 0.5f);
}
// Maintains perceived loudness throughout
```

**Usage:**
```csharp
AudioManager.Instance.CrossFade(
    from: oldMusicContainer,
    to: newMusicContainer,
    duration: 2f,
    type: AudioEvent.CrossfadeType.EqualPower
);
```

**Event-Based:**
```
Event: "TransitionMusic"
Action:
  - Type: CrossFade
  - From Container: Music_Combat
  - To Container: Music_Calm
  - Duration: 3.0s
  - Type: EqualPower
```

---

## 11. Best Practices

### 11.1 Project Organization

**Recommended Folder Structure:**
```
Assets/Audio/
├── Resources/Audio/
│   ├── Events/
│   │   ├── UI/
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Ambience/
│   │   └── Music/
│   └── States/
│       ├── Gameplay/
│       └── Location/
├── Containers/
│   ├── UI/
│   ├── Footsteps/
│   ├── Weapons/
│   ├── Impacts/
│   ├── Ambience/
│   └── Music/
├── Buses/
└── AudioClips/
    ├── SFX/
    │   ├── UI/
    │   ├── Footsteps/
    │   └── Weapons/
    ├── Music/
    └── VO/
```

### 11.2 Naming Conventions

**Containers:**
```
Format: Category_Description_Type

Examples:
Weapon_RifleShot_RnC       (Random Container)
UI_ButtonClick_RC          (Routing Container)
Music_MainTheme_BC         (Blend Container)
Footsteps_Switch_SwC       (Switch Container)
Alarm_Sequence_SeqC        (Sequence Container)
```

**Events:**
```
Format: Action_Description

Examples:
Play_WeaponRifleShot
Play_UIButtonClick
Start_MusicMainTheme
Stop_AllMusic
Set_StateUnderwater
```

**Buses:**
```
Format: Category_Bus or Category_Subcategory_Bus

Examples:
Master_Bus
SFX_Bus
SFX_Weapons_Bus
Music_Bus
Dialogue_Bus
```

### 11.3 Bus Hierarchy Design

**Typical Structure:**
```
Master (-3 dB)
├── SFX (0 dB)
│   ├── Player (0 dB)
│   │   ├── Footsteps (-6 dB)
│   │   └── Weapons (0 dB)
│   ├── Enemies (-3 dB)
│   ├── UI (0 dB)
│   └── Ambience (-6 dB)
├── Music (-6 dB)
│   ├── Gameplay (0 dB)
│   └── Menu (-3 dB)
├── Dialogue (0 dB)
└── VO (-3 dB)
```

**Guidelines:**
- **3-4 levels max** - Don't over-complicate
- **Leave headroom** - Master at -3 to -6 dB
- **Group by function** - Not by asset type
- **Plan for ducking** - Dialogue should duck Music/SFX

### 11.4 Container Design

**Variation Counts:**
```
Footsteps: 6-8 variations minimum
Weapon fire: 3-5 variations
Impacts: 4-6 per material
Voice lines: 10+ (more is better)
UI clicks: 2-3 variations
```

**Randomization Guidelines:**
```
Subtle:
  Volume: ±1-2 dB
  Pitch: ±50-100 cents
  Use for: Footsteps, UI

Moderate:
  Volume: ±3-4 dB
  Pitch: ±100-200 cents
  Use for: Weapons, impacts

Dramatic:
  Volume: ±5-6 dB
  Pitch: ±200-400 cents
  Use for: Explosions, special effects
```

**Repeat Avoidance:**
```
Frequent sounds (footsteps): avoidRepeatLast = 2-3
Weapons: avoidRepeatLast = 1-2
Voice lines: avoidRepeatLast = 3-5
```

### 11.5 Event Design

**Instance Limits:**
```
Explosions: 5-10
Gunshots: 8-12
Footsteps: 4-6
UI clicks: 2-3
Dialogue: 1
Music: 1
```

**Priority Assignment:**
```
Critical (255):
  - UI feedback
  - Dialogue
  - Tutorial VO

High (128):
  - Player footsteps
  - Player weapons
  - Important pickups

Medium (64):
  - Enemy sounds
  - Environmental feedback
  - Non-critical UI

Low (0):
  - Distant ambience
  - Background loops
  - Decorative sounds
```

**Cooldowns:**
```
UI clicks: 0.05-0.1s
Footsteps: Usually handled by animation
Weapon fire: Usually handled by game logic
Repeated impacts: 0.05-0.1s
```

### 11.6 State Design

**State Group Organization:**
```
Location:
  - Normal
  - Underwater
  - Cave
  - Indoor
  - Outdoor

GameplayState:
  - Menu
  - Playing
  - Paused
  - GameOver

CombatState:
  - Calm
  - Alert
  - Combat
```

**Transition Times:**
```
Fast (0.1-0.3s):
  - Pause/Unpause
  - UI state changes
  - Instant feedback

Medium (0.5-1.0s):
  - Entering water
  - Room changes
  - Combat state changes

Slow (1.5-3.0s):
  - Time of day
  - Weather changes
  - Gradual transitions
```

### 11.7 Common Pitfalls

**DON'T:**

❌ **Post events every frame**
```csharp
// BAD
void Update()
{
    if (isWalking)
        footstepEvent.Post();  // Spam!
}

// GOOD
public void PlayFootstep()  // Called from animation event
{
    footstepEvent.Post();
}
```

❌ **Forget instance limits**
```csharp
// Without maxInstances, this spams audio
for (int i = 0; i < 100; i++)
    explosionEvent.Post();
```

❌ **Over-use 3D spatialization**
```
Use 2D for: UI, music, non-positional SFX
Use 3D for: Positioned sounds only
```

❌ **Set all buses to 0 dB**
```
Leaves no headroom → clipping when many sounds play
Always leave -3 to -6 dB on Master
```

❌ **Ignore variation**
```
Single clip footsteps → machine gun effect
Always use RandomContainer with 4+ clips
```

**DO:**

✅ **Use cooldowns**
```csharp
Event cooldown prevents spam automatically
```

✅ **Add variations**
```csharp
6-8 clips + randomization = natural sound
```

✅ **Profile voice count**
```csharp
Monitor regularly, adjust maxInstances
```

✅ **Organize assets**
```csharp
Consistent folder structure
Clear naming conventions
```

✅ **Leave headroom**
```csharp
Master: -3 to -6 dB
Prevents clipping
```

---

## 12. Troubleshooting

### 12.1 Sound Not Playing

**Symptoms:** Event.Post() called, no sound.

**Checklist:**

1. **AudioManager exists?**
```csharp
// Check console for "AudioManager not found" error
// Add AudioManager to scene if missing
```

2. **Event in Resources folder?**
```
Events MUST be in: Assets/Audio/Resources/Audio/Events/
Check exact path, case-sensitive
```

3. **Container has clips?**
```
Open container asset
Check AudioClips list is not empty
Verify clips are assigned (not "None")
```

4. **Bus volume not -80 dB?**
```
Check bus hierarchy
Each bus in chain should be > -80 dB
```

5. **Max instances reached?**
```
Check event.MaxInstances
Check VoiceStealBehavior
Increase maxInstances or add stealing
```

6. **Cooldown active?**
```
Check event.Cooldown value
Wait cooldown duration between calls
```

7. **Console errors?**
```
Check for exceptions or warnings
Red errors prevent playback
```

### 12.2 No Variations

**Symptoms:** Same sound plays repeatedly, obviously.

**Checklist:**

1. **Using RandomContainer?**
```
Not RoutingContainer
Must be RandomContainer for variations
```

2. **Multiple clips assigned?**
```
Need 4+ clips for good variation
Check container's AudioClips list
```

3. **avoidRepeatLast set?**
```
Set to 2-3 for best results
Prevents recent clips from repeating
```

4. **Randomization enabled?**
```
Check enableVolumeRandomization
Check enablePitchRandomization
Set in base container settings
```

### 12.3 3D Sound Not Spatial

**Symptoms:** Sound plays but doesn't pan left/right or change volume with distance.

**Checklist:**

1. **Container is3D = true?**
```
Check container settings
3D Settings → is3D checkbox
```

2. **AudioListener in scene?**
```
Only one AudioListener should exist
Usually on MainCamera
Check it's not disabled
```

3. **Spatial Blend = 1?**
```
AudioManager sets this automatically
But check if overridden somehow
```

4. **Min/Max Distance set correctly?**
```
minDistance: Full volume distance (e.g., 1)
maxDistance: Zero volume distance (e.g., 50)
Check values aren't reversed or extreme
```

5. **Playing at correct position?**
```
event.Post(gameObject, transform.position)
Verify position is in world space
Verify not at (0,0,0) always
```

### 12.4 Switch Container Not Switching

**Symptoms:** SwitchContainer always plays same container regardless of switch value.

**Checklist:**

1. **SetSwitch called BEFORE Post?**
```csharp
// CORRECT order
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
footstepEvent.Post();

// WRONG order
footstepEvent.Post();
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");  // Too late!
```

2. **Switch value matches entry?**
```
Case-sensitive matching
"Metal" != "metal"
Check spelling exactly
```

3. **Switch group correct?**
```
Verify switchGroupName matches
"Surface_Type" must match exactly
```

4. **Default container assigned?**
```
Assign default as fallback
Helps debug which code path is taken
```

5. **Debug log switch value:**
```csharp
string value = AudioManager.Instance.GetSwitch("Surface_Type");
Debug.Log($"Current switch value: {value}");
```

### 12.5 Occlusion Not Working

**Symptoms:** Sounds not muffled by walls.

**Checklist:**

1. **enableOcclusion = true?**
```
Check AudioManager Inspector
Occlusion → Enable Occlusion checkbox
```

2. **Occlusion mask includes geometry?**
```
Check AudioManager.occlusionMask
Should include layer with walls/geometry
Shouldn't include player layer
```

3. **Sounds are 3D?**
```
Occlusion only works on 3D sounds
Check container is3D = true
```

4. **AudioListener exists?**
```
Occlusion raycasts from listener position
Check listener component active
```

5. **Update interval too slow?**
```
Default 0.2s is fine
If too slow (>0.5s), noticeable lag
Check AudioManager.occlusionUpdateInterval
```

6. **Geometry has colliders?**
```
Raycast needs colliders to hit
Verify walls have colliders
Check collider layer matches occlusionMask
```

### 12.6 Blend Container Not Blending

**Symptoms:** Blend container plays but all layers same volume, no crossfading.

**Checklist:**

1. **RTPC value being set?**
```csharp
// Verify RTPC updates
float value = AudioManager.Instance.GetRTPC("CombatIntensity");
Debug.Log($"Current RTPC: {value}");

// Set explicitly for testing
AudioManager.Instance.SetRTPC("CombatIntensity", 0.5f);
```

2. **Volume curves configured?**
```
Check BlendEntry.volumeCurve
Must not be flat line
Create varied curves in editor
```

3. **All layers have containers?**
```
Null containers don't play
Verify all entries assigned
```

4. **Loop enabled for music?**
```
Music should loop
Check container Loop = true
```

5. **RTPC name matches?**
```
blendParameterName must match RTPC name exactly
Case-sensitive: "CombatIntensity" != "combatintensity"
```

6. **Listener registered?**
```
BlendContainer implements IRTPCListener
Should auto-register, but verify in code
```

### 12.7 Performance Issues

**Symptoms:** Audio stuttering, frame drops, high CPU.

**Diagnosis:**

**High Voice Count:**
```
Problem: realVoices > 50-60
Solution:
  - Lower maxRealVoices
  - Add maxInstances to events
  - Increase priorities correctly
```

**CPU Spikes:**
```
Problem: Update intervals too fast
Solution:
  - Increase voiceUpdateInterval (0.1 → 0.15s)
  - Increase occlusionUpdateInterval (0.2 → 0.4s)
  - Profile in Unity Profiler
```

**Memory Issues:**
```
Problem: Too many clips loaded
Solution:
  - Check clip import settings
  - Use Streaming for large files
  - Compressed In Memory for SFX
```

**Mobile Performance:**
```
Problem: General slowdown
Solution:
  - Reduce maxRealVoices to 16-24
  - Disable occlusion
  - Increase update intervals
  - Lower clip quality
```

### 12.8 Multi-Position Sync Issues

**Symptoms:** Multiple emitters playing out of sync, phasing.

**Checklist:**

1. **Event has multi-position enabled?**
```
Check event Inspector
Multi-Position Mode checkbox
```

2. **Using PostMultiPosition()?**
```csharp
// CORRECT
event.PostMultiPosition(emitterParent);

// WRONG
event.Post();  // Normal mode, no sync
```

3. **All emitters active?**
```
Check emitterChild.IsActive
Verify parent.ActiveEmitterCount > 0
```

4. **DSP buffering adequate?**
```
Default 0.1s latency usually fine
If still issues, increase buffer
```

---

## Appendix A: Glossary

**AudioContainer:** ScriptableObject defining how audio clips are organized and played

**AudioEvent:** ScriptableObject triggering audio actions

**AudioHandle:** Control interface for single voice playback

**AudioMultiHandle:** Control interface for multi-voice playback

**AudioVoice:** Single playing audio instance

**Bus:** Volume control group in hierarchical structure

**Crossfade:** Smooth transition between two containers

**Decibel (dB):** Logarithmic unit of volume

**Ducking:** Automatic volume reduction of target buses

**DSP Time:** Digital Signal Processor time for sample-accurate scheduling

**GainStack:** Layered volume control system

**Instance Limiting:** Maximum concurrent playbacks of same event

**LOD:** Level of Detail, distance-based audio optimization

**Multi-Position:** Synchronized playback across multiple emitters

**Occlusion:** Raycast-based sound blocking

**Priority:** Voice stealing priority (Low, Medium, High, Critical)

**RTPC:** Real-Time Parameter Control, dynamic audio parameter

**State:** Game mode affecting multiple audio parameters

**Switch:** Conditional container selection

**Virtualization:** Time-tracking without audio output

**Voice:** Playing audio instance

**Voice Stealing:** Stopping voice to make room for higher priority

---

## Appendix B: Quick Reference

### Common Code Patterns

**Play Sound:**
```csharp
myEvent.Post();
myEvent.Post(gameObject, transform.position);
```

**Control Playback:**
```csharp
AudioHandle handle = myEvent.Post();
handle.SetVolume(0.5f);
handle.SetPitch(2f);
handle.Stop(fadeTime: 1f);
```

**Bus Control:**
```csharp
AudioManager.Instance.SetBusVolume("Music", -6f, transitionTime: 1f);
```

**State Change:**
```csharp
AudioManager.Instance.SetState("Underwater", transitionTime: 1.5f);
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
AudioMultiHandle multiHandle = event.PostMultiPosition(emitterParent);
```

---

**End of Manual**

---

*For practical tutorials, see COOKBOOK.md*
*For API documentation, see API_REFERENCE.md*
*For quick start, see QUICK_START.md*

*SFX System v1.0 - Professional Audio Middleware for Unity*
