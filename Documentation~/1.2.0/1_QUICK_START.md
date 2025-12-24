# SFX System - Quick Start Guide

**Version:** 1.2
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** December 2025

**Get up and running in 10 minutes**

---

## What is the SFX System?

The SFX System is professional audio middleware for Unity that transforms how you handle game audio. Instead of manually managing AudioSources and juggling clips, you work with high-level concepts: **Events**, **Containers**, and **Buses**.

**In one sentence:** Play sounds through events, not AudioSources.

---

## The Problem It Solves

### Before (Standard Unity):
```csharp
public AudioClip footstepClip;
public AudioSource audioSource;

void PlayFootstep()
{
    audioSource.clip = footstepClip;
    audioSource.Play();
}
```

**Problems:**
- ❌ No variations (repetitive audio)
- ❌ Manual AudioSource management
- ❌ No pooling (performance issues)
- ❌ No volume control structure
- ❌ Hard to organize

### After (SFX System):
```csharp
public AudioEvent footstepEvent;

void PlayFootstep()
{
    footstepEvent.Post(gameObject, transform.position);
}
```

**Benefits:**
- ✅ Built-in variations (no repetition)
- ✅ Automatic AudioSource pooling
- ✅ Professional bus mixing
- ✅ 3D spatial audio
- ✅ Easy to organize and maintain

---

## What You Get

### Core Features
- **Event-Based Audio** - Trigger sounds without managing AudioSources
- **Smart Variations** - Never hear the same footstep twice
- **Bus Mixing** - Control SFX, Music, and Dialogue volumes separately
- **Voice Pooling** - Pre-allocated AudioSources for peak performance
- **3D Spatial Audio** - Positioned sounds with occlusion support
- **Multi-Position Sync** - Play synchronized audio across multiple speakers
- **State System** - Change audio based on game state (underwater, paused, etc.)
- **RTPCs** - Real-time parameter control (speed → pitch, health → volume)

### Performance Built-In
- Voice virtualization (automatic LOD)
- Voice stealing with priority
- Distance-based culling
- Occlusion with raycasting
- Mobile-optimized

---

## Core Concepts in 30 Seconds

### Containers
**What:** Groups of audio clips with playback rules.

**Example:** A "footstep container" has 6 different footstep sounds and randomly picks one each time.

**Types:**
- **Routing** - Simple playback (single or multiple clips)
- **Random** - Pick random clip with variations
- **Sequence** - Play clips in order
- **Switch** - Change based on game state (grass vs metal footsteps)
- **Blend** - Crossfade layers dynamically (combat music intensity)

---

### Events
**What:** Triggers that play containers.

**Example:** `footstepEvent.Post()` plays the footstep container at your position.

**Why:** Decouples code from audio implementation. Designers can change sounds without touching code.

---

### Buses
**What:** Volume control groups, like a mixing board.

**Example:** Lower all SFX volume, or mute music independently.

**Hierarchy:**
```
Master Bus
├── SFX Bus
│   ├── Weapons
│   └── Footsteps
├── Music Bus
└── Dialogue Bus
```

---

### States
**What:** Game modes that modify audio automatically.

**Example:** "Underwater" state muffles all sounds and adds reverb.

**Use Cases:**
- Underwater effect
- Pause menu (lower gameplay audio)
- Combat mode (raise music intensity)

---

### Voices
**What:** The actual playing sound instances (internal).

**Why It Matters:** System pools voices for performance. You don't manage them, but understanding helps with optimization.

**Key Point:** Limited to 32 real voices by default. Excess voices get virtualized or stolen based on priority.

---

### Multi-Position Audio
**What:** Play the same sound from multiple speakers, perfectly synchronized.

**Example:** Nightclub with 8 speakers all playing the same music, no phasing or timing issues.

**Use Cases:**
- Speaker arrays
- Stadium announcers
- Vehicle multi-speaker systems
- Surround ambience

---

### RTPCs (Real-Time Parameter Control)
**What:** Dynamic audio parameters that change during gameplay.

**Examples:**
- Car speed → Engine pitch
- Player health → Heartbeat intensity
- Combat proximity → Music intensity

**How:** `AudioManager.Instance.SetRTPC("EngineRPM", 0.75f);`

---

## Get Running in 10 Minutes

### Step 1: Add AudioManager (2 minutes)

1. In your scene, create a new empty GameObject
2. Name it "AudioManager"
3. Add the `AudioManager` component (Component → Audio System → Audio Manager)
4. **Leave all default settings** - they're optimized for most games

**Inspector Settings (defaults are fine):**
- Master Volume: 1.0
- Max Real Voices: 32
- Max Virtual Voices: 64
- Enable Occlusion: ✓
- Enable LOD: ✓

**Important:** AudioManager persists between scenes automatically via `DontDestroyOnLoad`.

---

### Step 2: Create Folder Structure (1 minute)

Right-click in your Project window and create this folder structure:

```
Assets/
└── Audio/
    ├── Resources/
    │   └── Audio/
    │       ├── Events/     ← AudioEvents MUST go here
    │       └── States/     ← AudioStates MUST go here
    ├── Containers/         ← Container assets
    ├── Buses/              ← Bus assets
    └── AudioClips/         ← Your .wav/.mp3 files
        ├── SFX/
        ├── Music/
        └── Ambience/
```

**Why Resources folder?** AudioManager auto-loads Events and States from `Resources/Audio/Events` and `Resources/Audio/States` at startup.

---

### Step 3: Create Your First Bus (2 minutes)

1. Right-click in `Assets/Audio/Buses/`
2. Select `Create > Audio System > Audio Bus`
3. Name the file: "SFX_Bus"
4. Select it and configure in the Inspector:
   - **Bus Name:** "SFX"
   - **Volume Db:** 0
   - **Parent Bus:** (leave empty for now)
   - **Mixer Group:** (optional - assign if using Unity's AudioMixer)

**What just happened?** You created a volume control group for sound effects. Later you can adjust all SFX together.

---

### Step 4: Create Your First Container (2 minutes)

1. Right-click in `Assets/Audio/Containers/`
2. Select `Create > Audio System > Routing Container`
3. Name it: "TestSound_RC"
4. Select it and configure in the Inspector:
   - **Container Name:** "TestSound"
   - **Audio Clips:** Drag in 1-3 AudioClips from your project
   - **Volume:** 1.0
   - **Loop:** ☐ (unchecked)

**Optional - Add Variation:**
- **Enable Volume Randomization:** ☑
  - Min: -1, Max: 1
- **Enable Pitch Randomization:** ☑
  - Min: -50, Max: 50

**What just happened?** You created a simple container that plays your clip(s) with optional randomization.

---

### Step 5: Create Your First Event (2 minutes)

1. Right-click in `Assets/Audio/Resources/Audio/Events/`
2. Select `Create > Audio System > Audio Event`
3. Name it: "Play_TestSound"
4. Select it and configure in the Inspector:

**Event Configuration:**
- **Event Name:** "Play_TestSound"

**Voice Settings:**
- **Priority:** Medium
- **Max Instances:** 5
- **Cooldown:** 0

**Actions → Size: 1**

**Action [0]:**
- **Type:** Play
- **Container:** TestSound_RC (drag from Containers folder)
- **Target Bus:** SFX_Bus (drag from Buses folder)
- **Delay:** 0
- **Fade Duration:** 0

**What just happened?** You created an event that plays your container through the SFX bus when triggered.

---

### Step 6: Play It From Code (1 minute)

Create a new C# script called `TestAudio.cs`:

```csharp
using UnityEngine;
using AudioSystem;

public class TestAudio : MonoBehaviour
{
    [SerializeField] private AudioEvent testEvent;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            testEvent.Post(gameObject, transform.position);
            Debug.Log("Playing test sound!");
        }
    }
}
```

**Setup:**
1. Create a new GameObject in your scene (or use an existing one)
2. Attach the `TestAudio` script to it
3. In the Inspector, drag `Play_TestSound` event into the `Test Event` field
4. Press **Play** in Unity
5. Press **Spacebar** to hear your sound!

**What just happened?** You triggered an audio event with one line of code. The system handled everything else: finding a voice, configuring it, routing through the bus, and playing.

---

## 🎉 Congratulations!

You've successfully set up the complete audio pipeline:

**Your Code** → **AudioEvent** → **Container** → **Voice** → **Bus** → **Speaker**

---

## What You Just Learned

✅ AudioManager manages everything (singleton, persists between scenes)
✅ Events go in `Resources/Audio/Events/` (auto-loaded)
✅ Containers organize your clips (variations, randomization)
✅ Buses control volume groups (hierarchical mixing)
✅ One line of code to play audio: `event.Post()`

---

## Next Steps

### Add Variations (5 minutes)
Make your sound more interesting:

1. Open your container (TestSound_RC)
2. Add 3-5 different AudioClips
3. Change container type to `Random Container`:
   - Right-click container → Create → Audio System → **Random Container**
   - Set **Avoid Repeat Last:** 2
   - Set **Use Weighting:** ✓
4. Enable randomization:
   - **Volume Randomization:** -2 to +2 dB
   - **Pitch Randomization:** -100 to +100 cents

Now your sound will never repeat exactly the same way!

---

### Create a Music Bus (3 minutes)

1. Create > Audio System > Audio Bus
2. Name: "Music_Bus"
3. Inspector:
   - Bus Name: "Music"
   - Volume Db: -6 (leave headroom)
4. Create a music event using this bus

Now you can control music volume independently from SFX!

---

### Set Up Footsteps (10 minutes)

**Quick Recipe:**

1. **Create RandomContainer:**
   - Add 6 footstep clips
   - Avoid Repeat Last: 2
   - Volume Randomization: ±2 dB
   - Pitch Randomization: ±100 cents

2. **Create Event:**
   - Priority: Medium
   - Max Instances: 4
   - Action: Play footstep container → SFX bus

3. **Script:**
```csharp
public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioEvent footstepEvent;

    // Call from animation event
    public void PlayFootstep()
    {
        footstepEvent.Post(gameObject, transform.position);
    }
}
```

4. Add animation events to your walk/run animations calling `PlayFootstep()`

---

### Create a Settings Menu (15 minutes)

```csharp
using UnityEngine;
using UnityEngine.UI;
using AudioSystem;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;

    void Start()
    {
        // Load saved settings
        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // Apply settings
        OnMasterVolumeChanged(masterSlider.value);
        OnSFXVolumeChanged(sfxSlider.value);
        OnMusicVolumeChanged(musicSlider.value);

        // Add listeners
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat("MasterVolume", value);
    }

    void OnSFXVolumeChanged(float value)
    {
        float db = AudioExtensions.LinearToDb(value);
        AudioManager.Instance.SetBusVolume("SFX", db, 0.1f);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    void OnMusicVolumeChanged(float value)
    {
        float db = AudioExtensions.LinearToDb(value);
        AudioManager.Instance.SetBusVolume("Music", db, 0.1f);
        PlayerPrefs.SetFloat("MusicVolume", value);
    }
}
```

---

## Ready for More?

### Deep Dive
→ **MANUAL.md** - Complete understanding of the entire system
- System architecture and design philosophy
- Every component explained in detail
- Performance optimization strategies
- Best practices and common pitfalls

### Practical Tutorials
→ **COOKBOOK.md** - Step-by-step recipes for common tasks
- UI sounds
- Footsteps with surface detection
- Weapon systems
- Dynamic music
- 3D spatial audio
- State management
- And much more!

### API Documentation
→ **API_REFERENCE.md** - Complete API documentation
- Every public method and property
- Parameter descriptions
- Return values
- Usage examples

---

## Quick Reference

### Common Methods

**Play a sound:**
```csharp
AudioHandle handle = myEvent.Post();
AudioHandle handle = myEvent.Post(gameObject, transform.position);
```

**Control playback:**
```csharp
handle.Stop(fadeTime: 0.5f);
handle.Pause();
handle.Resume();
handle.SetVolume(0.75f);
handle.SetPitch(semitones: 2f);
```

**Bus control:**
```csharp
AudioManager.Instance.SetBusVolume("Music", volumeDb: -6f, transitionTime: 1f);
```

**State changes:**
```csharp
AudioManager.Instance.SetState("Underwater", transitionTime: 1.5f);
```

**RTPCs:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", value: 0.75f);
```

**Switches:**
```csharp
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
```

---

## Troubleshooting

### Sound Not Playing?

**Checklist:**
1. ✓ Is AudioManager in the scene?
2. ✓ Is the event in `Resources/Audio/Events/`?
3. ✓ Does the container have clips assigned?
4. ✓ Is the bus volume not -80 dB?
5. ✓ Check Console for errors

### No Variations?

**Checklist:**
1. ✓ Using RandomContainer (not RoutingContainer)?
2. ✓ Multiple clips assigned to container?
3. ✓ Randomization enabled in Inspector?

### Sound Too Quiet/Loud?

**Check:**
1. Container volume
2. Bus volume (in dB)
3. Master volume on AudioManager
4. Unity's AudioListener volume

---

## Key Concepts to Remember

1. **Events are in Resources** - Must be in `Resources/Audio/Events/` to auto-load
2. **States are in Resources** - Must be in `Resources/Audio/States/` to auto-load
3. **One AudioManager per project** - It persists between scenes
4. **Buses use dB** - 0 dB = no change, -6 dB ≈ half as loud, -80 dB = silence
5. **Voice limit** - 32 real voices by default (configurable)
6. **Priority matters** - Critical voices never get stolen

---

## What's Next?

You now have a working audio system! Here's the recommended learning path:

**Level 1 - You are here!** ✓
- Basic sound playback
- Simple events and containers
- Bus volume control

**Level 2 - COOKBOOK.md**
- Footsteps with variations
- UI sound systems
- Weapon audio
- Music systems
- 3D spatial audio

**Level 3 - MANUAL.md**
- Deep system understanding
- Advanced features
- Performance optimization
- Architecture patterns

**Level 4 - API_REFERENCE.md**
- Complete API documentation
- Advanced use cases
- Custom extensions

---

**Happy Sound Design!** 🎵

---

*SFX System v1.0 - Professional Audio Middleware for Unity*
