# SFX System — User Guide

**Version:** 2.5.0
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** May 2026

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Quick Start](#2-quick-start)
3. [Cookbook](#3-cookbook)
4. [Manual](#4-manual)
5. [Migration](#5-migration)

---

## 1. Introduction

### What is the SFX System?

The SFX System is professional audio middleware for Unity. Instead of manually managing AudioSources and juggling clips, you work with high-level concepts: **Events**, **Containers**, and **Buses**.

**In one sentence:** Play sounds through events, not AudioSources.

### The Problem It Solves

**Before (Standard Unity):**
```csharp
public AudioClip footstepClip;
public AudioSource audioSource;

void PlayFootstep()
{
    audioSource.clip = footstepClip;
    audioSource.Play();
}
```

❌ No variations • ❌ Manual management • ❌ No pooling • ❌ No volume control structure

**After (SFX System):**
```csharp
public AudioEvent footstepEvent;

void PlayFootstep()
{
    footstepEvent.Post(gameObject, transform.position);
}
```

✅ Built-in variations • ✅ Automatic pooling • ✅ Professional mixing • ✅ Easy to organize

---

## 2. Quick Start

### Choose Your Path

Pick the setup method that fits your workflow:

#### Method 1: Quick Wizard ⭐ (2 minutes)
**Best for:** Beginners, rapid prototyping
**Time:** 2 minutes
**Skill:** Easy

[Jump to Quick Wizard](#method-1-quick-wizard)

#### Method 2: Manual Setup (10 minutes)
**Best for:** Learning the system, custom control
**Time:** 10 minutes
**Skill:** Medium

[Jump to Manual Setup](#method-2-manual-setup)

#### Method 3: Registry System (Advanced)
**Best for:** Large projects, eliminating Resources folder
**Time:** Setup + migration
**Skill:** Advanced

[See Migration](#5-migration) for full details.

---

### Method 1: Quick Wizard

**The absolute fastest way to add sounds - perfect for beginners!**

#### Step 1: Add AudioManager to Scene (30 seconds)
1. Create empty GameObject in your scene
2. Add `AudioManager` component
3. Done! (Auto-configures itself)

#### Step 2: Create Sound Assets (1 minute)
1. In Unity menu: `Window > Audio System > Quick Sound Setup Wizard`
2. **Choose Creation Mode:**
   - **Complete Setup** - Creates Bus + Container + Event all at once (recommended)
   - **Container Only** - Just create a container
   - **Event Only** - Just create an event (with existing container or blank)
   - **Bus Only** - Just create a bus
   - **Batch Import** - Import multiple clips at once
3. Enter sound name (e.g., "ButtonClick")
4. **Choose Preset** (8 available):
   - Simple SFX, Music, UI Sound
   - Ambience, Dialogue, Footsteps
   - 3D Environmental, Weapon
5. **Select Container Type:**
   - **Routing** - Direct playback (plays all clips simultaneously if multiple added)
   - **Random** - Random selection with weights
   - **Sequence** - Sequential playback (Forward/Reverse/PingPong)
   - **Switch** - Switch-based selection
   - **Blend** - RTPC-driven real-time blending
6. Add one or more AudioClips
7. Configure advanced options if needed (pitch/volume randomization, 3D settings)
8. Click **"Create Complete Sound Setup"**

✅ Done! The wizard creates:
- AudioBus (with custom name)
- Container (any type: Routing, Random, Sequence, Switch, or Blend)
- AudioEvent (with selectable action type)
- All references pre-filled

💡 **Pro Tips:**
- The wizard auto-detects selected audio clips and suggests batch import for multiple selections
- Use "Auto-Create Standard Folder Structure" to set up all required folders
- Each preset automatically configures appropriate settings (3D, pitch variation, etc.)
- Routing containers can layer multiple clips for complex sounds (e.g., gunshot with shell eject)

#### Step 3: Play in Code (30 seconds)
```csharp
using AudioSystem;

public class MyScript : MonoBehaviour
{
    public AudioEvent buttonClickSound;

    void OnButtonPressed()
    {
        buttonClickSound.Post(gameObject, transform.position);
    }
}
```

Drag the created AudioEvent into the script field, and you're done!

**What's Next?** See the [Cookbook](#3-cookbook) chapter for more recipes or jump to [Troubleshooting](#troubleshooting).

---

### Method 2: Manual Setup

**Traditional workflow - full control over every step**

#### Step 1: Add AudioManager (2 minutes)

1. In your scene, create a new empty GameObject
2. Name it "AudioManager"
3. Add the `AudioManager` component
4. **Leave all default settings** - they're optimized for most games

**Inspector Settings (defaults are fine):**
- Master Volume: 1.0
- Max Real Voices: 32
- Max Virtual Voices: 64
- Enable LOD: ✓

Occlusion is opted into per-container via the `Use Occlusion` toggle on AudioContainer assets, not via a global AudioManager switch — see the Occlusion chapter in the Manual.

**Important:** AudioManager persists between scenes automatically via `DontDestroyOnLoad`.

---

#### Step 2: Create Folder Structure (1 minute)

Right-click in Project window → `Create > Audio System > Quick Sound Setup` → Click **"Auto-Create Standard Folder Structure"**

Or manually create:

```
Assets/Audio/
├── Resources/
│   └── Audio/
│       ├── Events/    ← AudioEvents MUST go here
│       └── States/    ← AudioStates MUST go here
├── Containers/        ← Container assets
├── Buses/             ← Bus assets
└── AudioClips/        ← Your .wav/.mp3 files
    ├── SFX/
    ├── Music/
    └── Ambience/
```

**Why Resources folder?** AudioManager auto-loads Events and States from `Resources/Audio/Events` and `Resources/Audio/States` at startup. (See the [Migration](#5-migration) chapter to skip this requirement via `AudioEventRegistry`.)

---

#### Step 3: Create Your First Bus (2 minutes)

1. Right-click in `Assets/Audio/Buses/`
2. Select `Create > Audio System > Audio Bus`
3. Name the file: "SFX_Bus"
4. Select it and configure in the Inspector:
   - **Bus Name:** "SFX"
   - **Volume Db:** 0
   - **Parent Bus:** (leave empty for now)
   - **Mixer Group:** (optional)

**What just happened?** You created a volume control group for sound effects. Later you can adjust all SFX together.

💡 **Tip:** The Quick Wizard creates this automatically when you use a preset.

---

#### Step 4: Create Your First Container (2 minutes)

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

#### Step 5: Create Your First Event (2 minutes)

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

⚠️ **Important:** The Inspector will show a warning if the event is not in the Resources folder. Click the "Move to Resources" button if you see this warning.

---

#### Step 6: Play It From Code (1 minute)

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

---

### 🎉 Congratulations!

You've successfully set up the complete audio pipeline:

**Your Code** → **AudioEvent** → **Container** → **Voice** → **Bus** → **Speaker**

---

### Troubleshooting

#### "AudioEvent not playing!"
**Check Inspector warnings:**
- Open your AudioEvent asset
- Look for red/yellow warning boxes
- If it says "NOT in Resources folder" → Click "Move to Resources/Audio/Events/"

#### "AudioManager not found!"
- Ensure AudioManager is in scene
- Check it persists (DontDestroyOnLoad is automatic)

#### "No AudioEvents found in Resources/Audio/Events"
- Events MUST be in `Assets/Audio/Resources/Audio/Events/` folder
- OR use the [AudioEventRegistry system](#5-migration)

#### Sound Too Quiet/Loud?
**Check:**
1. Container volume (0-1)
2. Bus volume (in dB, 0 = no change)
3. Master volume on AudioManager
4. Unity's AudioListener volume

---

### Quick Reference

#### Minimal Code Example
```csharp
using AudioSystem;

public AudioEvent mySound;

void Start()
{
    mySound.Post(gameObject, transform.position);
}
```

#### Common Methods

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

**Global controls:**
```csharp
AudioManager.Instance.StopAllSounds();
AudioManager.Instance.PauseAll();
AudioManager.Instance.UnpauseAll();
AudioManager.Instance.MuteAll(true);   // mute
AudioManager.Instance.MuteAll(false);  // unmute
```

**Bus control:**
```csharp
AudioManager.Instance.SetBusVolume("Music", volumeDb: -6f, transitionTime: 1f);
```

**State changes:**
```csharp
AudioManager.Instance.SetState("Underwater", transitionTime: 1.5f);
```

**RTPC with transitions:**
```csharp
AudioManager.Instance.SetRTPC("CombatIntensity", 0.75f);
AudioManager.Instance.TransitionRTPC("CombatIntensity", 1.0f, duration: 2f);
```

---

### What's Next?

#### Level 1 - You are here! ✓
- Basic sound playback
- Simple events and containers
- Bus volume control

#### Level 2 - [Cookbook](#3-cookbook)
- Footsteps with variations
- UI sound systems
- Weapon audio
- Music systems
- 3D spatial audio

#### Level 3 - [Manual](#4-manual)
- Deep system understanding
- Advanced features
- Performance optimization
- Registry system

#### Level 4 - [API Reference](API_REFERENCE.md)
- Complete API documentation
- All classes and methods
- Advanced use cases

---

### Comparison: Which Method Should I Use?

| Method | Best For | Setup Time | Flexibility | Learning |
|--------|----------|------------|-------------|----------|
| **Quick Wizard** | Beginners, rapid prototyping | 2 min | Low | Easy |
| **Manual Setup** | Learning system, custom control | 10 min | Medium | Best |
| **Registry System** | Large projects, no Resources | 15 min | High | Advanced |

**Recommendation:**
- **New users:** Start with Quick Wizard
- **Learning the system:** Use Manual Setup
- **After 10+ sounds:** Consider Registry System (see [Migration](#5-migration))

---

### Key Concepts to Remember

1. **Events are in Resources** - Must be in `Assets/Audio/Resources/Audio/Events/` to auto-load (or use registry)
2. **States are in Resources** - Must be in `Assets/Audio/Resources/Audio/States/` to auto-load (or use registry)
3. **One AudioManager per project** - It persists between scenes
4. **Buses use dB** - 0 dB = no change, -6 dB ≈ half as loud, -80 dB = silence
5. **Voice limit** - 32 real voices by default (configurable)

---

### 🆕 Ambient Propagation (v2.3.0)

The SFX System now ships with an **optional** zone/portal-based ambient propagation subsystem. Use it when you want ambient beds (rain, wind, machinery) to route *through geometry* — muffled through a closed door, swelling as it opens, heard *from the window* when the listener is in another room.

Propagation is **additive** — it doesn't touch the SFX event pipeline you just learned. Skip this section if your project only plays discrete SFX.

#### 2-Minute Setup

```
1. Create empty GameObject → BoxCollider (isTrigger forced on) → Add "Audio/Propagation/Audio Zone"
   → Size collider to fit the room. Repeat for each acoustic space.

2. Create empty GameObject at a doorway → BoxCollider → Add "Audio/Propagation/Audio Portal"
   → Drag the two adjacent AudioZones into ZoneA and ZoneB.

3. Create empty GameObject inside the source zone → Add "Audio/Propagation/Ambient Source"
   → Assign Source Zone, Looping Clip, and Ambience Mixer Group.

4. On your AudioListener GameObject → Add "Audio/Propagation/Audio Listener Zone Tracker".
   (A kinematic Rigidbody is auto-added if needed for trigger events.)
```

That's it. The `PropagationManager` auto-instantiates on first reference. Walk through the doorway and you'll hear the bed transition smoothly between zones.

#### Door-Driven Transmission

To let a door swing affect audio, implement `IPortalDoorSource` on any MonoBehaviour:

```csharp
using AudioSystem.Propagation;

public class MyDoor : MonoBehaviour, IPortalDoorSource
{
    public float OpenProgress => animator.GetFloat("openAmount"); // 0..1
    public event System.Action OnChanged;
}
```

Drag it into the portal's `Door Source` field. Closed → `closedTransmission` amplitude + `closedLowPassHz` filtering; open → the base values. Interpolation is continuous — no discrete snap.

#### Where to Learn More

- **Cookbook** → [Ambient Propagation recipes](#ambient-propagation)
- **Manual** → [Chapter 13: Ambient Propagation Subsystem](#13-ambient-propagation-subsystem)
- **API Reference** → [Propagation](API_REFERENCE.md#propagation)

---

## 3. Cookbook

This section is organized around tasks, not features. Each recipe walks one outcome from asset creation through code.

**Three ways to read this section — every recipe serves all three:**
- 🏃 **Skimmer:** Read the recipe titles and the bolded step labels. Skip the prose. The ordered lists at the top of each topic are the map.
- 📖 **Learner:** Read each recipe top to bottom. Each one is self-contained and explains *why* alongside *how*.
- 🔍 **Reference reader:** Jump to the recipe whose title matches your task. Use the Table of Contents.

> ## ⚠️ Read this before doing anything spatial
>
> **Occlusion, Per-Zone Reverb, and container-level Ambient Propagation all share one piece of infrastructure: the Occlusion Slot Pool.**
>
> On every play, the system checks:
>
> ```
> wantsSlot = container.UseOcclusion || container.AllowReverbSend || container.UsePropagation
> ```
>
> If *any* of those three flags is on and the slot pool has not been authored, the voice silently falls back to direct-to-bus routing. No muffling. No reverb sends. No container propagation. **No error.** You will configure everything else correctly and hear nothing change.
>
> **You must author the Occlusion Slot Pool first**, before touching reverb buses, zones, or container flags. Do it here: [Occluding SFX Through Geometry → One-Time Project Setup](#recipe-one-time-project-setup-5-minutes).
>
> Exception: the `AmbientSource` propagation path (rain, wind, ambient beds) uses a separate emitter pool and does *not* require slots. Only container-level `UsePropagation` does.

### Setup & Organization

#### Recipe: First-Time Project Setup (10 minutes)

📋 **What:** Complete setup from scratch to playing first sound

##### Step 1: Create AudioManager (2 min)

1. Create empty GameObject in scene: "AudioManager"
2. Add component: `AudioManager` (Component → Audio System → Audio Manager)
3. Configure Inspector:
   - Master Volume: 1.0
   - Max Real Voices: 32
   - Max Virtual Voices: 64
   - Enable LOD: ✓

   Occlusion is opted into per-container (see the `Use Occlusion` toggle on AudioContainer assets), not via a global AudioManager switch.

##### Step 2: Create Folder Structure (2 min)

Right-click in Project, create folders:

```
Assets/
├── Resources/
│   └── Audio/
│       ├── Events/  ← AudioEvents MUST go here
│       └── States/  ← AudioStates MUST go here
└── Audio/
    ├── Containers/  ← Container assets
    ├── Buses/       ← Bus assets
    └── AudioClips/  ← Your .wav/.mp3 files
        ├── SFX/
        ├── Music/
        └── Ambience/
```

##### Step 3: Create Master Bus (1 min)

1. Right-click `Buses/` → Create → Audio System → Audio Bus
2. Name: "Master_Bus"
3. Inspector:
   - Bus Name: "Master"
   - Volume Db: -3
   - Parent Bus: (none)

##### Step 4: Create SFX Bus (1 min)

1. Right-click `Buses/` → Create → Audio System → Audio Bus
2. Name: "SFX_Bus"
3. Inspector:
   - Bus Name: "SFX"
   - Volume Db: 0
   - Parent Bus: Master_Bus

##### Step 5: Create First Container (2 min)

1. Right-click `Containers/` → Create → Audio System → Routing Container
2. Name: "Test_Click_RC"
3. Inspector:
   - Container Name: "TestClick"
   - Audio Clips: Drag in a sound
   - Volume: 1.0
   - Enable Volume Randomization: ✓ (Min: -1, Max: 1)
   - Enable Pitch Randomization: ✓ (Min: -50, Max: 50)

##### Step 6: Create First Event (2 min)

1. Right-click `Assets/Resources/Audio/Events/` → Create → Audio System → Audio Event
2. Name: "Play_TestClick"
3. Inspector:
   - Event Name: "Play_TestClick"
   - Priority: High
   - Max Instances: 3
   - Actions → Size: 1
   - Action [0]:
     - Type: Play
     - Container: Test_Click_RC
     - Target Bus: SFX_Bus

✅ **Result:** Complete audio pipeline ready to use!

---

#### Recipe: Recommended Bus Hierarchy

📋 **What:** Professional bus structure for games

⚙️ **Setup:**

**Create Master Bus:**
- Name: Master_Bus
- Bus Name: "Master"
- Volume Db: -3
- Parent: (none)

**Create SFX Bus:**
- Name: SFX_Bus
- Bus Name: "SFX"
- Volume Db: 0
- Parent: Master_Bus

**Create SFX Sub-Buses:**

1. **Player_Bus**
   - Bus Name: "Player"
   - Parent: SFX_Bus
   - Volume Db: 0

2. **Weapons_Bus**
   - Bus Name: "Weapons"
   - Parent: Player_Bus
   - Volume Db: 0

3. **Footsteps_Bus**
   - Bus Name: "Footsteps"
   - Parent: Player_Bus
   - Volume Db: -6

4. **Enemies_Bus**
   - Bus Name: "Enemies"
   - Parent: SFX_Bus
   - Volume Db: -3

5. **UI_Bus**
   - Bus Name: "UI"
   - Parent: SFX_Bus
   - Volume Db: 0

6. **Ambience_Bus**
   - Bus Name: "Ambience"
   - Parent: SFX_Bus
   - Volume Db: -6

**Create Music Bus:**
- Name: Music_Bus
- Bus Name: "Music"
- Parent: Master_Bus
- Volume Db: -6

**Create Dialogue Bus:**
- Name: Dialogue_Bus
- Bus Name: "Dialogue"
- Parent: Master_Bus
- Volume Db: 0
- Enable Ducking: ✓
- Ducking Targets:
  - Music_Bus, Amount: 0.7
  - SFX_Bus, Amount: 0.5

**Final Hierarchy:**
```
Master (-3dB)
├── SFX (0dB)
│   ├── Player (0dB)
│   │   ├── Weapons (0dB)
│   │   └── Footsteps (-6dB)
│   ├── Enemies (-3dB)
│   ├── UI (0dB)
│   └── Ambience (-6dB)
├── Music (-6dB)
└── Dialogue (0dB) [Ducks Music & SFX]
```

✅ **Result:** Professional mixing structure with ducking!

---

### UI Sounds

#### Recipe: Button Click Sound

📋 **What:** Button with click sound and subtle variations

🎯 **Use Case:** UI buttons, menu navigation

##### Asset Setup

**1. Create Container:**
- Right-click `Containers/UI/` → Create → Random Container
- Name: "UI_ButtonClick_RnC"
- Inspector:
  - Audio Clips: Add 2-3 click sound variations
  - Avoid Repeat Last: 1
  - Use Weighting: ✓ (all weights = 1.0)
  - Volume: 0.8
  - Enable Volume Randomization: ✓ (Min: -1, Max: 1)
  - Enable Pitch Randomization: ✓ (Min: -50, Max: 50)

**2. Create Event:**
- Right-click `Assets/Resources/Audio/Events/UI/` → Create → Audio Event
- Name: "Play_UI_ButtonClick"
- Inspector:
  - Event Name: "Play_UI_ButtonClick"
  - Priority: Critical
  - Max Instances: 2
  - Steal Behavior: Oldest
  - Cooldown: 0.05
  - Actions → Size: 1
  - Action [0]:
    - Type: Play
    - Container: UI_ButtonClick_RnC
    - Target Bus: UI_Bus

##### Code

```csharp
using UnityEngine;
using UnityEngine.UI;
using AudioSystem;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField] private AudioEvent clickEvent;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }

    void OnButtonClick()
    {
        if (clickEvent != null)
        {
            clickEvent.Post();
        }
    }
}
```

**Usage:**
1. Attach to Button GameObject
2. Drag "Play_UI_ButtonClick" into Click Event field
3. Click button to hear sound

✅ **Result:** Button clicks with subtle variation, no spam!

---

#### Recipe: Settings Menu with Volume Sliders

📋 **What:** Complete audio settings menu with persistence

🎯 **Use Case:** Options menu, settings screen

💻 **Code:**

```csharp
using UnityEngine;
using UnityEngine.UI;
using AudioSystem;

public class AudioSettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider dialogueSlider;

    [Header("Audio Events")]
    [SerializeField] private AudioEvent testSFXEvent;

    private const string MASTER_KEY = "Audio_MasterVolume";
    private const string SFX_KEY = "Audio_SFXVolume";
    private const string MUSIC_KEY = "Audio_MusicVolume";
    private const string DIALOGUE_KEY = "Audio_DialogueVolume";

    void Start()
    {
        LoadSettings();
        SetupListeners();
        ApplySettings();
    }

    void LoadSettings()
    {
        masterSlider.value = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        sfxSlider.value = PlayerPrefs.GetFloat(SFX_KEY, 1f);
        musicSlider.value = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        dialogueSlider.value = PlayerPrefs.GetFloat(DIALOGUE_KEY, 1f);
    }

    void SetupListeners()
    {
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        dialogueSlider.onValueChanged.AddListener(OnDialogueVolumeChanged);
    }

    void ApplySettings()
    {
        OnMasterVolumeChanged(masterSlider.value);
        OnSFXVolumeChanged(sfxSlider.value);
        OnMusicVolumeChanged(musicSlider.value);
        OnDialogueVolumeChanged(dialogueSlider.value);
    }

    void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat(MASTER_KEY, value);
        PlayerPrefs.Save();
    }

    void OnSFXVolumeChanged(float value)
    {
        float db = AudioExtensions.LinearToDb(value);
        AudioManager.Instance.SetBusVolume("SFX", db, 0.1f);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    void OnMusicVolumeChanged(float value)
    {
        float db = AudioExtensions.LinearToDb(value);
        AudioManager.Instance.SetBusVolume("Music", db, 0.1f);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save();
    }

    void OnDialogueVolumeChanged(float value)
    {
        float db = AudioExtensions.LinearToDb(value);
        AudioManager.Instance.SetBusVolume("Dialogue", db, 0.1f);
        PlayerPrefs.SetFloat(DIALOGUE_KEY, value);
        PlayerPrefs.Save();
    }

    // Call this from a "Test" button
    public void PlayTestSound()
    {
        if (testSFXEvent != null)
        {
            testSFXEvent.Post();
        }
    }
}
```

**UI Setup:**
1. Create 4 Sliders (Master, SFX, Music, Dialogue)
2. Set slider range: 0 to 1
3. Create "Test Sound" button
4. Attach script to settings panel
5. Assign references

✅ **Result:** Full audio settings with persistence and test button!

---

### Footsteps & Movement

#### Recipe: Basic Footsteps with Variations

📋 **What:** Player footsteps with natural variation

🎯 **Use Case:** Character locomotion

##### Asset Setup

**1. Create Container:**
- Create → Random Container
- Name: "Player_Footsteps_RnC"
- Inspector:
  - Audio Clips: Add 6-8 footstep sounds
  - Avoid Repeat Last: 2
  - Use Weighting: ✓ (equal weights)
  - Volume: 0.6
  - Enable Volume Randomization: ✓ (Min: -2, Max: 2)
  - Enable Pitch Randomization: ✓ (Min: -100, Max: 100)
  - Is3D: ✓
  - Min Distance: 1
  - Max Distance: 15

**2. Create Event:**
- Create → Audio Event
- Name: "Play_Player_Footstep"
- Inspector:
  - Priority: High
  - Max Instances: 4
  - Actions → Play Player_Footsteps_RnC → Footsteps_Bus

##### Code (Animation Events)

```csharp
using UnityEngine;
using AudioSystem;

public class PlayerFootsteps : MonoBehaviour
{
    [SerializeField] private AudioEvent footstepEvent;

    // Called from animation event on footfall frames
    public void PlayFootstep()
    {
        if (footstepEvent != null)
        {
            footstepEvent.Post(gameObject, transform.position);
        }
    }
}
```

**Setup:**
1. Attach to player GameObject
2. Assign footstep event
3. In Animation window, add Animation Event at each footfall
4. Set Function: "PlayFootstep"

✅ **Result:** Natural-sounding footsteps synced to animation!

---

#### Recipe: Surface-Dependent Footsteps

📋 **What:** Different footstep sounds per surface type

🎯 **Use Case:** Realistic environmental feedback

##### Asset Setup

**1. Create Surface Containers:**

**Grass:**
- Random Container: "Footsteps_Grass_RnC"
- 6 grass footstep clips
- Avoid Repeat Last: 2

**Metal:**
- Random Container: "Footsteps_Metal_RnC"
- 5 metal footstep clips
- Volume: 0.8 (louder)

**Wood:**
- Random Container: "Footsteps_Wood_RnC"
- 6 wood footstep clips
- Volume: 0.7

**Water:**
- Random Container: "Footsteps_Water_RnC"
- 5 water splash clips
- Volume: 0.65

**2. Create Switch Container:**
- Create → Switch Container
- Name: "Player_Footsteps_Switch_SwC"
- Inspector:
  - Switch Group Name: "Surface_Type"
  - Switch Entries:
    - [0] "Grass" → Footsteps_Grass_RnC
    - [1] "Metal" → Footsteps_Metal_RnC
    - [2] "Wood" → Footsteps_Wood_RnC
    - [3] "Water" → Footsteps_Water_RnC
  - Default Container: Footsteps_Grass_RnC
  - Is3D: ✓
  - Min Distance: 1
  - Max Distance: 15

**3. Create Event:**
- Event: "Play_Player_Footstep"
- Action: Play Player_Footsteps_Switch_SwC → Footsteps_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class SurfaceFootsteps : MonoBehaviour
{
    [SerializeField] private AudioEvent footstepEvent;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float raycastDistance = 2f;

    // Called from animation event
    public void PlayFootstep()
    {
        string surface = DetectSurface();
        AudioManager.Instance.SetSwitch("Surface_Type", surface);

        if (footstepEvent != null)
        {
            footstepEvent.Post(gameObject, transform.position);
        }
    }

    private string DetectSurface()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        if (Physics.Raycast(origin, Vector3.down, out hit, raycastDistance, groundLayers))
        {
            // Method 1: By tag
            if (hit.collider.CompareTag("Metal")) return "Metal";
            if (hit.collider.CompareTag("Wood")) return "Wood";
            if (hit.collider.CompareTag("Water")) return "Water";

            // Method 2: By material name
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                string matName = renderer.sharedMaterial.name.ToLower();

                if (matName.Contains("metal")) return "Metal";
                if (matName.Contains("wood")) return "Wood";
                if (matName.Contains("water")) return "Water";
            }

            // Method 3: By layer
            int hitLayer = hit.collider.gameObject.layer;
            if (hitLayer == LayerMask.NameToLayer("Metal")) return "Metal";
            if (hitLayer == LayerMask.NameToLayer("Wood")) return "Wood";
        }

        return "Grass"; // Default
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);
    }
}
```

**Surface Tagging Methods:**

**Option 1 - Tags:**
```
Tag ground objects: "Metal", "Wood", "Grass", "Water"
Pro: Simple
Con: Limited to small number of tags
```

**Option 2 - Material Names:**
```
Name materials: "Floor_Metal", "Terrain_Grass", etc.
Pro: Unlimited surfaces
Con: Requires naming convention
```

**Option 3 - Layers:**
```
Create layers: Metal, Wood, Grass
Pro: Clean, performant
Con: Limited layer count
```

✅ **Result:** Realistic surface-dependent footsteps!

---

#### Recipe: Footstep Timer (No Animation Events)

📋 **What:** Timer-based footsteps for procedural animation or simple movement

🎯 **Use Case:** Games without foot animation events, procedural animation

💻 **Code:**

```csharp
using UnityEngine;
using AudioSystem;

[RequireComponent(typeof(CharacterController))]
public class FootstepTimer : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioEvent footstepEvent;

    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.3f;
    [SerializeField] private float minSpeedForFootsteps = 0.5f;

    private CharacterController controller;
    private float stepTimer = 0f;
    private bool wasGroundedLastFrame = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        // Check if moving and grounded
        float speed = new Vector3(controller.velocity.x, 0, controller.velocity.z).magnitude;
        bool isGrounded = controller.isGrounded;

        if (isGrounded && speed > minSpeedForFootsteps)
        {
            // Determine interval based on speed
            bool isRunning = speed > 5f; // Adjust threshold as needed
            float interval = isRunning ? runStepInterval : walkStepInterval;

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = interval;
            }
        }
        else
        {
            // Reset timer when not moving or airborne
            stepTimer = 0f;
        }

        // Landing sound
        if (isGrounded && !wasGroundedLastFrame && speed > 1f)
        {
            PlayFootstep();
            stepTimer = (speed > 5f ? runStepInterval : walkStepInterval);
        }

        wasGroundedLastFrame = isGrounded;
    }

    void PlayFootstep()
    {
        if (footstepEvent != null)
        {
            footstepEvent.Post(gameObject, transform.position);
        }
    }
}
```

**Tuning Tips:**
```
Walk Interval: 0.4-0.6s
Run Interval: 0.25-0.35s
Min Speed: 0.3-0.8 units/sec
```

✅ **Result:** Automatic footsteps based on movement speed!

---

### Weapons & Combat

#### Recipe: Weapon Fire Sound

📋 **What:** Weapon shot with variations and instance limiting

🎯 **Use Case:** Guns, projectile weapons

##### Asset Setup

**1. Create Container:**
- Random Container: "Weapon_RifleShot_RnC"
- Audio Clips: 3-5 firing variations
- Avoid Repeat Last: 1
- Volume: 1.0
- Volume Randomization: ±1 dB
- Pitch Randomization: ±75 cents
- Is3D: ✓
- Min Distance: 5
- Max Distance: 50

**2. Create Event:**
- Event: "Play_Weapon_RifleShot"
- Priority: High
- Max Instances: 8
- Steal Behavior: Oldest
- Actions: Play Weapon_RifleShot_RnC → Weapons_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class WeaponAudio : MonoBehaviour
{
    [Header("Audio Events")]
    [SerializeField] private AudioEvent fireEvent;
    [SerializeField] private AudioEvent reloadEvent;
    [SerializeField] private AudioEvent emptyEvent;
    [SerializeField] private AudioEvent equipEvent;

    public void OnFire()
    {
        if (fireEvent != null)
        {
            fireEvent.Post(gameObject, transform.position);
        }
    }

    public void OnReload()
    {
        if (reloadEvent != null)
        {
            reloadEvent.Post(gameObject, transform.position);
        }
    }

    public void OnEmpty()
    {
        if (emptyEvent != null)
        {
            emptyEvent.Post(gameObject, transform.position);
        }
    }

    public void OnEquip()
    {
        if (equipEvent != null)
        {
            equipEvent.Post(gameObject, transform.position);
        }
    }
}
```

**Integration:**
```csharp
// In your weapon controller
public class WeaponController : MonoBehaviour
{
    private WeaponAudio weaponAudio;

    void Start()
    {
        weaponAudio = GetComponent<WeaponAudio>();
    }

    void Fire()
    {
        if (HasAmmo())
        {
            // Fire logic...
            weaponAudio?.OnFire();
        }
        else
        {
            weaponAudio?.OnEmpty();
        }
    }

    void Reload()
    {
        // Reload logic...
        weaponAudio?.OnReload();
    }
}
```

✅ **Result:** Professional weapon audio with variations!

---

#### Recipe: Impact Sounds by Material

📋 **What:** Different impact sounds based on hit material

🎯 **Use Case:** Bullet impacts, melee hits, projectile collisions

##### Asset Setup

**1. Create Material Containers:**

- "Impact_Metal_RnC" (5-6 metal impact clips)
- "Impact_Wood_RnC" (5-6 wood impact clips)
- "Impact_Concrete_RnC" (5-6 concrete clips)
- "Impact_Flesh_RnC" (4-5 flesh/organic clips)
- "Impact_Default_RnC" (generic impacts)

**2. Create Switch Container:**
- Switch Container: "Impact_Switch_SwC"
- Switch Group Name: "ImpactMaterial"
- Entries:
  - "Metal" → Impact_Metal_RnC
  - "Wood" → Impact_Wood_RnC
  - "Concrete" → Impact_Concrete_RnC
  - "Flesh" → Impact_Flesh_RnC
- Default: Impact_Default_RnC

**3. Create Event:**
- Event: "Play_Impact"
- Priority: Medium
- Max Instances: 12
- Actions: Play Impact_Switch_SwC → SFX_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class ProjectileImpact : MonoBehaviour
{
    [SerializeField] private AudioEvent impactEvent;
    [SerializeField] private GameObject impactVFXPrefab;

    void OnCollisionEnter(Collision collision)
    {
        HandleImpact(collision);
    }

    void HandleImpact(Collision collision)
    {
        // Determine material
        string material = GetMaterialType(collision.collider);

        // Set switch
        AudioManager.Instance.SetSwitch("ImpactMaterial", material);

        // Play impact sound at contact point
        if (impactEvent != null && collision.contacts.Length > 0)
        {
            Vector3 impactPoint = collision.contacts[0].point;
            impactEvent.Post(gameObject, impactPoint);
        }

        // Spawn VFX
        if (impactVFXPrefab != null && collision.contacts.Length > 0)
        {
            Instantiate(impactVFXPrefab, collision.contacts[0].point,
                       Quaternion.LookRotation(collision.contacts[0].normal));
        }

        // Destroy projectile
        Destroy(gameObject);
    }

    string GetMaterialType(Collider hitCollider)
    {
        // Method 1: Check tag
        if (hitCollider.CompareTag("Metal")) return "Metal";
        if (hitCollider.CompareTag("Wood")) return "Wood";
        if (hitCollider.CompareTag("Enemy") || hitCollider.CompareTag("Player"))
            return "Flesh";

        // Method 2: Check layer
        int layer = hitCollider.gameObject.layer;
        if (layer == LayerMask.NameToLayer("Metal")) return "Metal";
        if (layer == LayerMask.NameToLayer("Wood")) return "Wood";

        // Method 3: Custom component
        var surfaceID = hitCollider.GetComponent<SurfaceIdentifier>();
        if (surfaceID != null)
            return surfaceID.MaterialType;

        return "Default";
    }
}

// Optional: Attach to surfaces for explicit material identification
public class SurfaceIdentifier : MonoBehaviour
{
    public string MaterialType = "Concrete";
}
```

✅ **Result:** Dynamic impact sounds matching hit materials!

---

### Music Systems

#### Recipe: Background Music (Intro + Loop)

📋 **What:** Music with intro that transitions to seamless loop

🎯 **Use Case:** Level music, menu music

##### Asset Setup

**1. Create Containers:**

**Intro:**
- Routing Container: "Music_MainTheme_Intro_RC"
- Audio Clips: [Intro clip]
- Loop: ☐ (no loop)
- Volume: 1.0
- Is3D: ☐ (2D)

**Loop:**
- Routing Container: "Music_MainTheme_Loop_RC"
- Audio Clips: [Loop clip]
- Loop: ✓ (loop enabled)
- Volume: 1.0
- Is3D: ☐ (2D)

**2. Create Event:**
- Event: "Music_Start_MainTheme"
- Priority: Medium
- Max Instances: 1
- Actions → Size: 2

**Action [0]:**
- Type: Play
- Container: Music_MainTheme_Intro_RC
- Target Bus: Music_Bus
- Delay: 0

**Action [1]:**
- Type: Play
- Container: Music_MainTheme_Loop_RC
- Target Bus: Music_Bus
- Delay: 8.5 ← **SET TO YOUR INTRO LENGTH IN SECONDS**

**3. Create Stop Event:**
- Event: "Music_Stop_All"
- Actions:
  - Stop Music_MainTheme_Loop_RC, Fade: 2.0

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioEvent startMusicEvent;
    [SerializeField] private AudioEvent stopMusicEvent;
    [SerializeField] private bool playOnStart = true;

    private AudioHandle musicHandle;

    void Start()
    {
        if (playOnStart)
        {
            StartMusic();
        }
    }

    public void StartMusic()
    {
        if (startMusicEvent != null)
        {
            startMusicEvent.Post();
        }
    }

    public void StopMusic(float fadeTime = 2f)
    {
        if (stopMusicEvent != null)
        {
            stopMusicEvent.Post();
        }
    }

    void OnDestroy()
    {
        StopMusic(1f);
    }
}
```

**Setup:**
1. Create empty GameObject: "MusicManager"
2. Attach MusicManager script
3. Assign events
4. Mark as DontDestroyOnLoad if needed

✅ **Result:** Intro plays once, then seamless looping music!

---

#### Recipe: Dynamic Combat Music (3 Layers)

📋 **What:** Music that responds to combat intensity with layered crossfading

🎯 **Use Case:** Action games, dynamic gameplay music

##### Asset Setup

**1. Create Layer Containers:**

**Ambient Layer:**
- Routing Container: "Music_Ambient_RC"
- Clip: Calm ambient music loop
- Loop: ✓
- Volume: 1.0

**Tension Layer:**
- Routing Container: "Music_Tension_RC"
- Clip: Tense percussion/strings loop
- Loop: ✓
- Volume: 1.0

**Combat Layer:**
- Routing Container: "Music_Combat_RC"
- Clip: Full combat music loop
- Loop: ✓
- Volume: 1.0

**2. Create Blend Container:**
- Blend Container: "Music_Dynamic_Combat_BC"
- Blend Parameter Name: "CombatIntensity"
- Volume: 1.0
- Loop: ✓

**Blend Entries:**

**Entry [0] - Ambient:**
- Container: Music_Ambient_RC
- Volume Curve:
  - Key (0.0, 1.0) ← Full volume at calm
  - Key (1.0, 0.0) ← Silent at combat

**Entry [1] - Tension:**
- Container: Music_Tension_RC
- Volume Curve:
  - Key (0.0, 0.0) ← Silent at calm
  - Key (0.5, 1.0) ← Peak at medium intensity
  - Key (1.0, 0.0) ← Silent at full combat

**Entry [2] - Combat:**
- Container: Music_Combat_RC
- Volume Curve:
  - Key (0.0, 0.0) ← Silent at calm
  - Key (1.0, 1.0) ← Full at combat

**3. Create Event:**
- Event: "Music_Start_DynamicCombat"
- Actions: Play Music_Dynamic_Combat_BC → Music_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class DynamicCombatMusic : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioEvent startMusicEvent;

    [Header("Settings")]
    [SerializeField] private float transitionSpeed = 0.5f;
    [SerializeField] private float enemyDetectionRadius = 20f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Intensity Thresholds")]
    [SerializeField] private int calmEnemyCount = 0;
    [SerializeField] private int tensionEnemyCount = 2;
    [SerializeField] private int combatEnemyCount = 4;

    private float currentIntensity = 0f;
    private float targetIntensity = 0f;

    void Start()
    {
        StartMusic();
    }

    void StartMusic()
    {
        if (startMusicEvent != null)
        {
            startMusicEvent.Post();
        }
        AudioManager.Instance.SetRTPC("CombatIntensity", 0f);
    }

    void Update()
    {
        UpdateMusicIntensity();
    }

    void UpdateMusicIntensity()
    {
        // Calculate target intensity based on enemy count
        int enemyCount = CountNearbyEnemies();
        targetIntensity = CalculateIntensity(enemyCount);

        // Smooth transition
        if (Mathf.Abs(currentIntensity - targetIntensity) > 0.01f)
        {
            currentIntensity = Mathf.MoveTowards(
                currentIntensity,
                targetIntensity,
                transitionSpeed * Time.deltaTime
            );

            AudioManager.Instance.SetRTPC("CombatIntensity", currentIntensity);
        }
    }

    int CountNearbyEnemies()
    {
        Collider[] enemies = Physics.OverlapSphere(
            transform.position,
            enemyDetectionRadius,
            enemyLayer
        );
        return enemies.Length;
    }

    float CalculateIntensity(int enemyCount)
    {
        if (enemyCount <= calmEnemyCount)
            return 0f; // Ambient only

        if (enemyCount <= tensionEnemyCount)
            return 0.5f; // Tension layer

        if (enemyCount <= combatEnemyCount)
            return 0.75f; // Ramping up

        return 1f; // Full combat
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyDetectionRadius);

        // Show intensity
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f,
            $"Music Intensity: {currentIntensity:F2}");
        #endif
    }
}
```

**Setup:**
1. Attach to player or game manager
2. Configure detection radius and thresholds
3. Set enemy layer mask
4. Tune transition speed (0.3-0.8 recommended)

✅ **Result:** Music dynamically responds to combat!

---

### Rhythmic SFX

Use `BeatScheduler` (`AudioSystem.BeatScheduler`, MonoBehaviour in `RunTime/Utilities/`) for any SFX whose **rate must vary at runtime without changing pitch**: heartbeats, metronomes, clocks, sonar pings, reactor pulses. Instead of time-stretching one long clip (which shifts pitch), it re-posts a one-shot AudioEvent on a `60 / BPM` interval, anchored to `AudioSettings.dspTime` for stable cadence.

#### Recipe: Stress-Driven Heartbeat

📋 **What:** Heartbeat whose BPM rises as the player sees an enemy or takes damage, with no pitch artefacts.

🎯 **Use Case:** Horror / survival / stealth — "I just saw something". Same pattern works for clock tension, reactor meltdown, alert pulse.

##### Asset Setup

**1. Create Container:**
- Random Container: "SFX_Heartbeat_Thump_RnC"
- Audio Clips: 2–3 thump variations (`heartbeat_01.wav`, `heartbeat_02.wav`, …) — each a single short thump, **not** a looping heartbeat loop
- Avoid Repeat Last: 1
- Loop: ☐
- Volume: 0.8
- Is3D: ☐ (2D — it's in the player's head)

> **Why one-shots, not a loop?** BPM is controlled by firing rate, not playback speed. If you use a loop, `source.pitch` is your only speed knob and it will rise in pitch alongside the tempo.

**2. Create Event:**
- Event: "Heartbeat_Thump"
- Priority: Medium-High (don't let combat SFX starve it)
- Max Instances: 4 (lets overlap at panic BPM without voice-stealing the ongoing thump)
- Actions: Play SFX_Heartbeat_Thump_RnC → Player_SFX_Bus

##### Scene Setup

1. On the player (or a dedicated child GameObject), add a `BeatScheduler` component
2. Set `Event Name` = `Heartbeat_Thump`
3. Set `Bpm` = 60 (calm baseline)
4. Leave `Play On Enable` = ✓, `Fire Immediately On Start` = ✓

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class StressHeartbeat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BeatScheduler heartbeat;

    [Header("Tempo Range")]
    [SerializeField] private float calmBpm = 60f;
    [SerializeField] private float panicBpm = 160f;
    [SerializeField] private float smoothing = 2f; // BPM ramp rate

    [Header("Stress Inputs")]
    [SerializeField] private float sightRadius = 15f;
    [SerializeField] private LayerMask enemyMask;

    private float currentStress;  // 0..1, internal
    private float targetStress;

    private void Update()
    {
        // 1. Compute target stress from game state.
        targetStress = ComputeStress();

        // 2. Smooth it so BPM ramps instead of snapping.
        currentStress = Mathf.MoveTowards(currentStress, targetStress, smoothing * Time.deltaTime);

        // 3. Drive the scheduler. The NEXT beat uses this BPM; the one already queued fires at the old rate — this is musically natural.
        heartbeat.Bpm = Mathf.Lerp(calmBpm, panicBpm, currentStress);
    }

    private float ComputeStress()
    {
        // Example: closest enemy inside sight radius => stress 0..1.
        var hits = Physics.OverlapSphere(transform.position, sightRadius, enemyMask);
        if (hits.Length == 0) return 0f;

        float closest = sightRadius;
        foreach (var h in hits)
            closest = Mathf.Min(closest, Vector3.Distance(transform.position, h.transform.position));

        return 1f - Mathf.Clamp01(closest / sightRadius);
    }

    // Call these from your damage / sanity / stealth systems.
    public void Spike(float amount)    => targetStress = Mathf.Clamp01(targetStress + amount);
    public void ForceCalm()            => targetStress = 0f;
}
```

##### Variations

**Volume swell with BPM.** The thumps should also get a bit louder as you panic. Add a bus volume RTPC bound to stress, or simply:
```csharp
heartbeat.GetComponent<AudioSource>(); // no — BeatScheduler doesn't own an AudioSource.
// Instead, modulate the bus:
AudioManager.Instance.SetRTPC("Heartbeat_Intensity", currentStress);
```
Wire the RTPC to `Player_SFX_Bus` volume in the bus asset.

**Pause the heartbeat cleanly.** Disable the component (`heartbeat.enabled = false`) or `heartbeat.StopScheduling()`. Already-posted thumps finish naturally; no new ones fire.

**React on-beat with gameplay.** Subscribe to the scheduler's event to pulse a vignette, camera shake, or UI indicator in perfect sync with the thump:
```csharp
void OnEnable()  => heartbeat.OnBeat += PulseVignette;
void OnDisable() => heartbeat.OnBeat -= PulseVignette;
void PulseVignette() { /* flash post-processing, etc. */ }
```

✅ **Result:** Heartbeat cadence scales smoothly from 60 → 160 BPM. The thump itself always sounds like a thump — no chipmunk pitch-up, no CPU cost from real-time time-stretching.

---

#### Recipe: Metronome / Rhythm Tick

📋 **What:** Fixed or adjustable tempo tick for rhythm games, timing tutorials, or in-engine practice tools.

##### Setup

- `BeatScheduler` component, `Event Name` = `Metronome_Tick`
- For an accented downbeat (every 4th tick louder), subscribe to `OnBeat` and post a second event every 4 beats:

```csharp
public class Metronome : MonoBehaviour
{
    [SerializeField] private BeatScheduler beat;
    [SerializeField] private AudioEvent accentEvent; // e.g. Metronome_Accent
    [SerializeField, Range(1, 16)] private int beatsPerMeasure = 4;

    private int beatCount;

    private void OnEnable()  => beat.OnBeat += HandleBeat;
    private void OnDisable() => beat.OnBeat -= HandleBeat;

    private void HandleBeat()
    {
        if (beatCount % beatsPerMeasure == 0 && accentEvent != null)
            accentEvent.Post(gameObject, transform.position);
        beatCount++;
    }
}
```

✅ **Result:** Standard tick on every beat, accented thump on the 1.

---

### 3D Spatial Audio

#### Recipe: Looping Campfire Sound

📋 **What:** 3D looping ambient sound attached to object

🎯 **Use Case:** Fire, machines, environmental loops

##### Asset Setup

**1. Create Container:**
- Random Container: "Ambience_Campfire_RnC"
- Audio Clips: 2-3 fire crackling loops
- Avoid Repeat Last: 1
- Loop: ✓
- Volume: 0.7
- Is3D: ✓
- Min Distance: 2
- Max Distance: 20
- Rolloff: Logarithmic

**2. Create Event:**
- Event: "Play_Ambience_Campfire"
- Priority: Low
- Max Instances: 10
- Actions: Play Ambience_Campfire_RnC → Ambience_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class CampfireAudio : MonoBehaviour
{
    [SerializeField] private AudioEvent fireEvent;
    [SerializeField] private bool playOnStart = true;

    private AudioHandle fireHandle;

    void Start()
    {
        if (playOnStart)
        {
            StartFire();
        }
    }

    void StartFire()
    {
        if (fireEvent != null)
        {
            fireHandle = fireEvent.Post(gameObject, transform.position);
        }
    }

    void StopFire(float fadeTime = 1f)
    {
        if (fireHandle != null)
        {
            fireHandle.Stop(fadeTime);
            fireHandle = null;
        }
    }

    // Optional: Adjust volume based on fire intensity
    public void SetIntensity(float intensity)
    {
        if (fireHandle != null)
        {
            fireHandle.SetVolume(intensity);
        }
    }

    void OnDisable()
    {
        StopFire(0.5f);
    }

    void OnDestroy()
    {
        StopFire(0.2f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 2f);   // Min distance
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 20f);  // Max distance
    }
}
```

**Setup:**
1. Attach to campfire GameObject
2. Assign event
3. Sound follows object if it moves

✅ **Result:** Realistic 3D ambient loop!

---

### States & RTPCs

#### Recipe: Underwater State

📋 **What:** Complete underwater audio effect

🎯 **Use Case:** Swimming, diving, underwater sections

##### Asset Setup

**1. Create States:**

**Normal State:**
- Audio State: "State_Normal"
- Save to: Assets/Resources/Audio/States/
- Inspector:
  - State Name: "Normal"
  - State Group: "Location"
  - (Leave all properties empty/default)

**Underwater State:**
- Audio State: "State_Underwater"
- Save to: Assets/Resources/Audio/States/
- Inspector:
  - State Name: "Underwater"
  - State Group: "Location"

**Bus Volumes:**
- [0] Bus: SFX_Bus, Volume Db: -9
- [1] Bus: Music_Bus, Volume Db: -12
- [2] Bus: Dialogue_Bus, Volume Db: -6

**Switch Values:**
- [0] Switch Group: "Location", Value: "Underwater"

**RTPC Values:**
- [0] Parameter: "Underwater", Value: 1.0

**2. Create Underwater Ambience:**
- Routing Container: "Ambience_Underwater_RC"
- Clip: Underwater bubbles/muffled sound
- Loop: ✓
- Volume: 0.8
- Is3D: ☐ (2D)

**3. Create Event:**
- Event: "Play_Ambience_Underwater"
- Actions: Play Ambience_Underwater_RC → Ambience_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class UnderwaterVolume : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioEvent underwaterAmbienceEvent;
    [SerializeField] private float transitionTime = 1.5f;

    [Header("Optional VFX")]
    [SerializeField] private GameObject underwaterVFX;

    private AudioHandle ambienceHandle;
    private bool isUnderwater = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isUnderwater)
        {
            EnterUnderwater();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (isUnderwater)
        {
            ExitUnderwater();
        }
    }

    void EnterUnderwater()
    {
        isUnderwater = true;

        // Activate underwater state
        AudioManager.Instance.SetState("Underwater", transitionTime);

        // Set location switch
        AudioManager.Instance.SetSwitch("Location", "Underwater");

        // Play underwater ambience
        if (underwaterAmbienceEvent != null)
        {
            ambienceHandle = underwaterAmbienceEvent.Post();
        }

        // Enable VFX
        if (underwaterVFX != null)
        {
            underwaterVFX.SetActive(true);
        }

        Debug.Log("Entered underwater");
    }

    void ExitUnderwater()
    {
        isUnderwater = false;

        // Return to normal state
        AudioManager.Instance.SetState("Normal", transitionTime);

        // Reset location switch
        AudioManager.Instance.SetSwitch("Location", "Normal");

        // Stop underwater ambience
        if (ambienceHandle != null)
        {
            ambienceHandle.Stop(transitionTime);
            ambienceHandle = null;
        }

        // Disable VFX
        if (underwaterVFX != null)
        {
            underwaterVFX.SetActive(false);
        }

        Debug.Log("Exited underwater");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawCube(transform.position, transform.localScale);
    }
}
```

**Setup:**
1. Create BoxCollider trigger volume for water
2. Attach UnderwaterVolume script
3. Assign underwater ambience event
4. Tag player as "Player"

✅ **Result:** Smooth transition to muffled underwater audio!

---

#### Recipe: Vehicle Engine RPM

📋 **What:** Engine sound that changes with speed/RPM

🎯 **Use Case:** Cars, motorcycles, aircraft

##### Asset Setup

**1. Create Layer Containers:**

- "Engine_Idle_RC" (idle loop)
- "Engine_Load_RC" (medium RPM loop)
- "Engine_Redline_RC" (high RPM loop)

**2. Create Blend Container:**
- Blend Container: "Vehicle_Engine_BC"
- Blend Parameter: "EngineRPM"
- Loop: ✓

**Entry [0] - Idle:**
- Container: Engine_Idle_RC
- Curve: (0.0, 1.0), (0.3, 0.5), (1.0, 0.0)

**Entry [1] - Load:**
- Container: Engine_Load_RC
- Curve: (0.0, 0.0), (0.5, 1.0), (1.0, 0.3)

**Entry [2] - Redline:**
- Container: Engine_Redline_RC
- Curve: (0.0, 0.0), (0.7, 0.2), (1.0, 1.0)

**3. Create Event:**
- Event: "Start_Vehicle_Engine"
- Actions: Play Vehicle_Engine_BC → SFX_Bus

##### Code

```csharp
using UnityEngine;
using AudioSystem;

[RequireComponent(typeof(Rigidbody))]
public class VehicleEngineAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioEvent engineStartEvent;
    [SerializeField] private AudioEvent engineStopEvent;

    [Header("RPM Settings")]
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float rpmTransitionSpeed = 3f;
    [SerializeField] private float idleRPM = 0.2f;

    [Header("Pitch Modulation")]
    [SerializeField] private bool modulatePitch = true;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.4f;

    private Rigidbody rb;
    private AudioHandle engineHandle;
    private float currentRPM = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartEngine();
    }

    void StartEngine()
    {
        if (engineStartEvent != null)
        {
            engineHandle = engineStartEvent.Post(gameObject, transform.position);
        }
    }

    void Update()
    {
        UpdateEngineRPM();
    }

    void UpdateEngineRPM()
    {
        // Calculate target RPM from speed
        float speed = rb.velocity.magnitude;
        float targetRPM = Mathf.Clamp01(speed / maxSpeed);

        // Keep minimum idle RPM when not moving
        if (targetRPM < idleRPM)
            targetRPM = idleRPM;

        // Smooth transition
        currentRPM = Mathf.Lerp(
            currentRPM,
            targetRPM,
            rpmTransitionSpeed * Time.deltaTime
        );

        // Update RTPC for blend container
        AudioManager.Instance.SetRTPC("EngineRPM", currentRPM);

        // Optional: Modulate pitch
        if (modulatePitch && engineHandle != null)
        {
            float pitchSemitones = Mathf.Lerp(
                AudioExtensions.LinearToDb(minPitch) * 12f / 20f,
                AudioExtensions.LinearToDb(maxPitch) * 12f / 20f,
                currentRPM
            );
            engineHandle.SetPitch(pitchSemitones);
        }
    }

    void OnDestroy()
    {
        if (engineStopEvent != null)
        {
            engineStopEvent.Post();
        }
        else if (engineHandle != null)
        {
            engineHandle.Stop(0.5f);
        }
    }
}
```

✅ **Result:** Realistic engine sound that responds to speed!

---

### Multi-Position Audio

#### Recipe: Nightclub Speaker Setup

📋 **What:** 8 speakers playing synchronized music

🎯 **Use Case:** Nightclubs, concerts, speaker arrays

##### Asset Setup

**1. Create Container:**
- Routing Container: "Music_Nightclub_RC"
- Clip: Club music loop
- Loop: ✓
- Volume: 1.0
- Is3D: ✓
- Min Distance: 3
- Max Distance: 40

**2. Create Event:**
- Event: "Music_Start_Nightclub"
- Enable Multi-Position: ✓
- Priority: Medium
- Max Instances: 1
- Actions: Play Music_Nightclub_RC → Music_Bus

##### Scene Setup

**1. Create Parent:**
- GameObject: "NightclubSpeakers"
- Add Component: AudioMultiPositionEmitterParent
- Inspector:
  - Max Emitters: 8
  - Auto Find Children: ✓
  - Auto Update Children: ✓

**2. Create Speakers (Manual or Auto):**

**Auto (Recommended):**
- Select NightclubSpeakers
- Inspector → Context Menu → "Create 8 Child Emitters (Circle Pattern)"
- Adjust radius and positions as needed

**Manual:**
- Create 8 child GameObjects
- Position in circle around dance floor
- Add AudioMultiPositionEmitterChild to each
- Configure:
  - Is Active: ✓
  - Volume Multiplier: 1.0
  - Use Directionality: ✓
  - Cone Angle: 90
  - Rotate to face inward

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class NightclubAudio : MonoBehaviour
{
    [SerializeField] private AudioEvent musicEvent;
    [SerializeField] private bool playOnStart = true;

    private AudioMultiHandle musicHandle;
    private AudioMultiPositionEmitterParent emitterParent;

    void Start()
    {
        emitterParent = GetComponent<AudioMultiPositionEmitterParent>();

        if (playOnStart)
        {
            StartMusic();
        }
    }

    void StartMusic()
    {
        if (musicEvent != null && emitterParent != null)
        {
            musicHandle = musicEvent.PostMultiPosition(emitterParent);

            Debug.Log($"Playing music on {musicHandle.VoiceCount} speakers");
        }
    }

    public void StopMusic(float fadeTime = 2f)
    {
        if (musicHandle != null)
        {
            musicHandle.Stop(fadeTime);
            musicHandle = null;
        }
    }

    // Optional: Control individual speakers
    public void SetSpeakerVolume(int index, float volume)
    {
        var emitters = emitterParent.AllEmitters;
        if (index >= 0 && index < emitters.Count)
        {
            emitters[index].VolumeMultiplier = volume;
        }
    }

    public void ToggleSpeaker(int index, bool active)
    {
        var emitters = emitterParent.AllEmitters;
        if (index >= 0 && index < emitters.Count)
        {
            emitters[index].IsActive = active;
        }
    }

    void OnDestroy()
    {
        StopMusic(1f);
    }
}
```

✅ **Result:** 8 speakers playing perfectly synchronized music!

---

### Performance & Debugging

#### Recipe: Voice Count Display

📋 **What:** Real-time voice statistics HUD

🎯 **Use Case:** Development, profiling, debugging

💻 **Code:**

```csharp
using UnityEngine;
using AudioSystem;

public class AudioDebugHUD : MonoBehaviour
{
    [SerializeField] private bool showDebug = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;

    private GUIStyle style;

    void Start()
    {
        style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.fontSize = 14;
        style.padding = new RectOffset(10, 10, 10, 10);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            showDebug = !showDebug;
        }
    }

    void OnGUI()
    {
        if (!showDebug || AudioManager.Instance == null) return;

        var stats = AudioManager.Instance.GetStatistics();

        int y = 10;
        int lineHeight = 20;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"<color=yellow>AUDIO SYSTEM DEBUG (F1 to toggle)</color>",
            style);
        y += lineHeight + 5;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Active Voices: {stats.activeVoices} / {stats.totalVoices}",
            style);
        y += lineHeight;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Available Voices: {stats.availableVoices}",
            style);
        y += lineHeight;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Active Loops: {stats.activeLoops}",
            style);
        y += lineHeight;

        // Voice usage bar
        float usagePercent = (float)stats.activeVoices / stats.totalVoices;
        DrawUsageBar(new Rect(10, y, 300, 20), usagePercent);
        y += 30;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Active Events: {stats.activeEvents}",
            style);
    }

    void DrawUsageBar(Rect rect, float percent)
    {
        // Background
        GUI.color = Color.black;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        // Fill
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * percent, rect.height);

        if (percent < 0.7f)
            GUI.color = Color.green;
        else if (percent < 0.9f)
            GUI.color = Color.yellow;
        else
            GUI.color = Color.red;

        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

        GUI.color = Color.white;
    }
}
```

**Setup:**
1. Attach to any GameObject
2. Press F1 to toggle
3. Monitor voice usage in real-time

✅ **Result:** Development HUD showing audio system stats!

---

#### Recipe: Event Callbacks for Debugging

📋 **What:** Log when sounds start/finish

🎯 **Use Case:** Debugging event flow, testing

💻 **Code:**

```csharp
using UnityEngine;
using AudioSystem;

public class AudioEventLogger : MonoBehaviour
{
    [SerializeField] private AudioEvent eventToMonitor;
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool logToScreen = false;

    private string lastLog = "";
    private float logDisplayTime = 0f;

    void Start()
    {
        TestEvent();
    }

    void TestEvent()
    {
        if (eventToMonitor == null) return;

        AudioHandle handle = eventToMonitor.Post(gameObject, transform.position);

        if (handle != null)
        {
            handle.OnStarted += () => Log($"[{handle.EventName}] STARTED");
            handle.OnLoop += () => Log($"[{handle.EventName}] LOOPED");
            handle.OnFinished += () => Log($"[{handle.EventName}] FINISHED");
        }
    }

    void Log(string message)
    {
        if (logToConsole)
        {
            Debug.Log($"<color=cyan>{message}</color>");
        }

        if (logToScreen)
        {
            lastLog = message;
            logDisplayTime = Time.time;
        }
    }

    void OnGUI()
    {
        if (logToScreen && Time.time - logDisplayTime < 3f)
        {
            GUI.Label(new Rect(10, Screen.height - 30, 500, 20), lastLog);
        }
    }
}
```

✅ **Result:** Detailed event lifecycle logging!

---

### Occluding SFX Through Geometry

SFX voices can be muffled when walls or props stand between the listener and the emitter. The mechanism is the occlusion mixer slot pool (see Manual chapter 14 for the architecture). This section walks the authoring once, then shows how to opt individual containers into the system.

> 🧱 **This is the foundation for two other features too.** Per-Zone Reverb (`AllowReverbSend`) and container-level Ambient Propagation (`UsePropagation`) both acquire the **same Occlusion Slot** at play time. If you skip the slot-pool authoring in *One-Time Project Setup* below, none of the three features will work — voices silently fall back to direct-to-bus routing with no muffling, no sends, no propagation, and no error message. Author the layout once; opt individual containers in afterward.

> ⚠️ **Silent failures to know up front**
> - No `OcclusionLayout` authored → containers with `Use Occlusion` (or `Allow Reverb Send`, or `Use Propagation`) play without effect. No error.
> - Container's `Mixer Group` not in the layout's bus list → slot acquisition skipped silently. Re-run the builder after adding new buses.
> - 2D voices (Is3D = ✗) ignore raycast occlusion. The flag still costs a slot acquire — turn it off for UI/2D containers.
> - Slot pool exhausted → new voices route direct-to-bus (no occlusion) and a console warning fires. Bump *Slots Per Bus* if you see it.

#### Setup Path

Follow these in order. Steps 1–2 are required; the rest are per-container or per-feature opt-ins.

1. **One-Time Project Setup** — author the slot pool. *Do this once per project.*
2. **Occlude a Container** — flip `Use Occlusion` on each container that should be muffled.
3. *(Optional)* **Soften the Edge with Multi-Ray** — multi-ray smoothing.
4. *(Optional)* **Compose with Propagation** — let one container respect both walls and doors.

---

#### Recipe: One-Time Project Setup (5 minutes)

📋 **What:** Author the slot layout so any container with `Use Occlusion = true` can be muffled at runtime.

🎯 **Use Case:** You're shipping SFX that should sound muffled behind cover, walls, or interior geometry. This is the prerequisite for every occluded sound in your project — author it once.

##### Asset Setup

**1. Create an OcclusionLayout asset:**
- Right-click in your Audio assets folder → Create → Audio System → Occlusion Layout
- Name it `OcclusionLayout` (one per project)

**2. Open the builder window:**
- Menu: Window → Audio System → Occlusion Layout

**3. Configure:**
- **Voice Mixer**: your project's voice mixer (where SFX are routed)
- **Occlusion Layout**: drag in the asset you just created
- **Slots Per Bus**: leave at 6 (the default sizes well for most SFX-heavy scenes; bump to 8–12 for weapon-dense gameplay)
- (Optional) **Reverb Send Bus Registry**: assign if you also use per-zone reverb sends — covered in the Per-Zone Reverb recipes below

**4. Click "Scan, Auto-Create, Refresh":**
- The builder walks the project for containers with `Use Occlusion = true`, collects the unique buses they route to, and authors:
  - One `<BusName>_OcclusionLayer` child group per bus
  - Six slot groups under each (`<BusName>_Slot_00` through `<BusName>_Slot_05`)
  - A Lowpass effect with exposed cutoff parameter on each slot
- The results panel shows which buses got authored and any that failed (typically because the bus doesn't exist yet on the mixer)

**5. Wire AudioManager:**
- On your scene's AudioManager GameObject:
  - **Voice Mixer**: the same mixer asset
  - **Occlusion Layout**: the `OcclusionLayout` asset
  - **Default Occlusion Slots Per Bus**: 6 (matches the builder)
- Press Play. The Console should print `Occlusion slot pools ready — N bus(es), M slot(s) total.`

✅ **Result:** The infrastructure is live. Any container with `Use Occlusion = true` will now acquire a slot on Play, get its cutoff modulated by the raycast occlusion path every frame, and release the slot on completion.

---

#### Recipe: Occlude a Weapon Fire Sound

📋 **What:** Turn on occlusion for a single container.

🎯 **Use Case:** Bullet impacts, weapon shots, footsteps — any SFX whose perceived loudness and brightness should drop when the listener can't see the emitter.

##### Asset Setup

**1. Open your container** (Random / Routing / Sequence — any type):
- Find the **"Use Occlusion"** toggle (under Mixer Settings)
- Set it to ✓ (true)

**2. Make sure the container is 3D:**
- Is3D: ✓
- Min Distance / Max Distance / Rolloff: set as appropriate
- Occlusion only modulates 3D-positioned sounds; 2D voices ignore the raycast

**3. Verify the container's Mixer Group is one of the buses the layout authored.** If not, re-run the Occlusion Layout Builder.

##### What Happens at Runtime

```csharp
weaponEvent.Post(gameObject, transform.position);
```

On Play:
1. `AudioManager.GetVoice(container)` acquires a free slot from the container's bus pool.
2. The voice's `outputAudioMixerGroup` is set to the slot's mixer group.
3. The OcclusionUpdate coroutine fires raycasts from listener → source every ~200 ms.
4. `LateUpdate` smooths the resulting cutoff target into the slot's exposed mixer parameter every frame.

When the listener walks behind a wall:
- The linecast hits geometry → blocked fraction climbs from 0 toward 1
- Gain target lerps from 1.0 → 0.3 (linear amplitude, ≈ -10 dB)
- Cutoff target lerps from 22 kHz → 500 Hz
- Both axes settle in ~60 ms (configurable via `Occlusion Gain/Cutoff Smoothing Seconds` on AudioManager)

✅ **Result:** The weapon sound muffles when the listener is behind cover. No code changes — just the `Use Occlusion` toggle.

---

#### Recipe: Soften the Occlusion Edge with Multi-Ray

📋 **What:** Fan 3–5 rays at the source to produce a continuous gradient instead of a binary in/out step.

🎯 **Use Case:** You want occlusion to feel natural as the listener walks past a wall edge, not "snap" the moment line-of-sight breaks.

##### Setup

On the AudioManager component:

- **Occlusion Ray Count**: 3 (default) or 5
- **Occlusion Ray Spread Meters**: 0.5 (default), 1–2 for larger emitters

With `RayCount = 3` and `Spread = 0.5 m`, the system fires a center ray plus two side rays at ±0.25 m perpendicular to the listener-source axis. The blocked fraction (0/3, 1/3, 2/3, or 3/3) lerps the gain and cutoff targets accordingly; the per-frame smoother rides over the discrete levels for continuous perception.

##### Tuning Notes

- **Ray Count = 1**: identical to the original binary occlusion path. Use when you want hard edges (cover-shooter rooms with tight corner-peeking).
- **Ray Count = 3**: four-level gradient. Best balance for most outdoor and large-interior scenes.
- **Ray Count = 5**: six-level gradient. Smoothest, but the cost is 5× raycasts — usually unnecessary above 3.
- **Spread Meters**: scale with emitter size. A door-sized emitter (1 m) wants 0.5 m spread; a vehicle-sized emitter (3 m) wants 1.5 m.

✅ **Result:** Walking past a wall edge produces a smooth gradient of muffling and uncovering, not a step at the exact moment line-of-sight breaks.

---

#### Recipe: Compose Occlusion with Propagation

📋 **What:** Make an SFX container respect both raycast occlusion (walls) AND propagation routing (doors).

🎯 **Use Case:** A door-knock sound that should be both muffled by the door (propagation) and additionally muffled by any clutter between the listener and the door (raycast).

##### Setup

On the container:

- **Use Occlusion**: ✓ (raycast path)
- **Use Propagation**: ✓ (propagation path)

##### What Happens

Both paths feed `LateUpdate` independently. The composition is **most-muffled-wins**:

```
gainTarget   = raycastGain × propagationGain          (multiplicative)
cutoffTarget = min(raycastCutoff, propagationCutoff)  (min-wins for cutoff)
```

- If line-of-sight to the emitter is clear but the door is closed → propagation dominates (the cutoff comes from the door).
- If line-of-sight is blocked but the door is open → raycast dominates.
- If both are partly blocked → both contribute; the tightest cutoff wins.

When the listener is outside every registered AudioZone, propagation has no opinion. Default behavior keeps the SFX audible (`propGain = 1, propCutoff = 22kHz`); flip `PropagationManager.Silence Outside Graph` if you want a strict "outside means silent" coverage model instead.

✅ **Result:** A single SFX container that handles both routing-based muffling (doors, openings) and clutter-based occlusion (walls, props) in one place.

---

#### Troubleshooting

**"My container has Use Occlusion = true but the sound doesn't muffle."**
- Did you assign `Voice Mixer` AND `Occlusion Layout` on AudioManager?
- Did you re-run the Occlusion Layout Builder after adding this container?
- Is the container's `Mixer Group` one of the buses in the layout? If you check the OcclusionLayout asset's `Buses` list and the container's bus isn't there, the layout is stale.
- Is the container 3D? Occlusion uses the listener-to-source linecast; 2D voices have no spatial position to test.

**"Console warns 'Occlusion slot pool exhausted on bus X'."**
- More concurrent occluded voices on that bus than slots configured. Bump `Slots Per Bus` on the Occlusion Layout asset, re-run Generate, and verify the warning stops.

**"The first frame of audio clicks."**
- Almost never the slot system — slots are acquired before `Play()` precisely to avoid this. Suspect the clip itself: leading sample boundary, encoder pre-roll, or a Stop→Play on a non-pooled AudioSource elsewhere in the project.

**"My slot has a Lowpass on it but the cutoff parameter never changes."**
- Check `voiceMixer.GetFloat(slot.CutoffParam, …)` in a debug script. If it stays at 22000, the voice never opted in (container's `Use Occlusion` is off, container has no slot, or `OcclusionPerf` is skipping it due to a null container reference).
- The exposed-parameter name on the mixer must match `slot.CutoffParam` exactly. Re-run the layout builder if you've manually renamed mixer groups.

---

### Per-Zone Reverb

The "wet path" sibling of the occlusion section. SFX voices send a copy of their dry signal to a per-zone reverb bus; each bus's character is defined by its `ReverbSendBus` asset, which is the single source of truth (the zone only decides *which* bus a voice sends to). See Manual chapter 15 for the architecture.

> 🧱 **Prerequisite: the Occlusion Slot Pool must already be authored.** Reverb sends are written into the voice's occlusion slot — no slot, no sends. If you haven't done it yet, jump to [Occluding SFX Through Geometry → One-Time Project Setup](#recipe-one-time-project-setup-5-minutes) first. Come back here once the layout exists.

> ⚠️ **Silent failures to know up front**
> - A bus's character lives on its `ReverbSendBus` asset, **not** the zone. A bus whose SO `Room` is at `-10000 dB` (the default) is silent until you raise it (try `0` dB). Tagging a zone and pointing its `Reverb Bus` at that asset routes voices there, but the *sound* comes from the asset.
> - The `Reverb Bus` field on `AudioZone` wants the **mixer group reference** inside the SendBus asset — not the SendBus asset itself. Drag from the SendBus's `ReverbBus` field, not from the Project window.
> - `AudioManager.Reverb Send Bus Registry` must be assigned. Without it, the registry is empty and nothing routes.
> - After adding a new bus, **re-run the Occlusion Layout Builder** so existing slots get the new bus's Send parameter authored. Skipping this means existing containers can't send to the new bus.

#### Setup Path

Follow these in order. Each recipe assumes the previous one is done.

1. **Author a Reverb Send Bus** — once per acoustic archetype (small room, large hall, outdoors).
2. **Tag a Zone to Use This Bus** — for each physical room in your scene.
3. **Send an SFX Container Into the Reverb** — flip the per-sound `Allow Reverb Send` flag.
4. *(Optional)* **Override Routing for Music or Narration** — for sounds with authored, non-zone-driven character.
5. *(Optional)* **Tune a Bus's Character While Editing** — iteration workflow.

---

#### Recipe: Author a Reverb Send Bus

📋 **What:** Create a single reverb bus and wire it up so voices physically in a tagged zone send to it.

🎯 **Use Case:** First step for any per-zone reverb in the project. Authoring is one-time per acoustic space.

##### Asset Setup

**1. Create the SendBus asset:**
- Project ▸ Create ▸ Audio System ▸ Reverb Send Bus
- Name it after the room: `Reverb_Bathroom` (the asset name becomes the bus id and the basis for exposed parameter names)

**2. Configure the SendBus:**
- **Reverb Mixer**: drag in your voice mixer asset (the same one assigned to AudioManager ▸ Voice Mixer)
- Leave **Reverb Bus** blank — it auto-fills on Generate
- Leave **Output Destination** blank for single-mixer setups (the wet tail flows up the group hierarchy naturally)
- (Optional) **Parent Group**: assign a `ReverbsWet` return-bus group if you use the DAW-style convention — Generate will nest this SendBus under it
- **Parametric reverb — Basic**:
  - Dry Level: -10000 (the default; SendBus groups are wet-only by design)
  - Room: 0 (overall wet gain; lower to attenuate)
  - Decay Time: tune for the space — bathroom ≈ 0.8 s, cathedral ≈ 5 s, cave ≈ 4 s
  - Reverb Level: 200 (Unity's SFX Reverb default)

**3. Generate:**
- Select the SendBus asset
- In the inspector, click **Generate Mixer Group**
- Console should log success: "Generated 'Bathroom': groups +1, effects +1, exposed params +6, values applied 14/14, output set: no"
- The mixer now has a `Bathroom` mixer group with an SFX Reverb effect; `Reverb_Bathroom_Wet`, `_Decay`, `_Room`, `_RoomHF`, `_DecayHFRatio`, `_Reflections` are exposed for the runtime driver

**4. Register the bus in the project registry:**
- Open Window ▸ Audio System ▸ Reverb Send Buses
- If no registry exists yet, create one: Project ▸ Create ▸ Audio System ▸ Reverb Send Bus Registry
- Drop the registry into AudioManager ▸ Reverb Send Bus Registry
- Click Refresh in the window — your new SendBus appears in the list

**5. Sanity-check:**
- Click Validate Wiring on the SendBus asset. It should say "Wiring OK."

✅ **Result:** The reverb bus exists on the mixer, exposes the right runtime parameters, and is registered. Voices won't send to it yet — that requires tagging an `AudioZone` (next recipe).

---

#### Recipe: Tag a Zone to Use This Bus

📋 **What:** Tell an `AudioZone` "voices in here send to this reverb bus, and listeners in here hear this acoustic character."

🎯 **Use Case:** After authoring a SendBus, every zone that should produce that bus's reverb needs to be tagged.

##### Asset Setup

**1. Select the AudioZone GameObject in your scene.**

**2. In the AudioZone component:**
- **Reverb Bus**: drag in the `Reverb_Bathroom` SendBus's `ReverbBus` field (open the SendBus inspector and drag from there — it's the mixer group reference). This is the source-side routing target.
- **Reverb Profile** (expand the foldout):
  - Room: tune toward 0 dB to enable. -10000 dB is "silent" and means no reverb on the listener side even if the bus is wired.
  - Room HF: usually 0 (no HF tilt); negative values for soft, carpeted rooms
  - Decay Time: ideally matches the SendBus's authored Decay Time, but you can vary it per-zone for "this version of the bathroom is darker than the canonical authored bus"
  - Decay HF Ratio: ~0.83 (most rooms); raise toward 1.0 for tile/glass, lower toward 0.5 for carpeted
  - Reflections Level: -2602 (Unity default)
  - Reverb Level: 200 (Unity default)

##### What Happens at Runtime

- **Source-side**: voices in this zone whose container has `Allow Reverb Send = ✓` send a copy of their signal to `Reverb_Bathroom` at their authored level. The send level scales by the listener's audibility to the zone (1.0 same-zone, <1 through portals).
- **Listener-side**: when the listener is in this zone, the bus's six runtime-driven parameters get written from this zone's `Reverb Profile` every reverb-sends tick. Walk into the bathroom, the `Reverb_Bathroom` bus reflects the bathroom's authored character.

✅ **Result:** The bathroom zone is now acoustically alive. Voices in the bathroom contribute to the bathroom bus; the bathroom bus's reverb parameters are driven by this zone's profile.

---

#### Recipe: Send an SFX Container Into the Reverb

📋 **What:** Flip an existing SFX container into the per-zone reverb path.

🎯 **Use Case:** Take a gunshot / footstep / impact container and have its voices contribute to the reverb of the room they're fired in.

##### Asset Setup

On the AudioContainer:

- **Allow Reverb Send**: ✓
- **Reverb Send Level Db**: per-sound tuning
  - 0 dB — full wet contribution. Right for big impacts (gunshots, explosions) that should ring out the room.
  - -6 dB — half-loud wet contribution. Good baseline for most SFX.
  - -12 dB — quiet wet contribution. Right for small detail sounds (footstep grain, weapon foley) that should colour the mix but not dominate the reverb.
  - -80 dB — effectively silent. Use `Allow Reverb Send = off` instead if you want zero contribution.
- **Static Emitter**: ✓ if the voice doesn't move during its lifetime (impacts, stingers, fixed-prop ambiences). Off for vehicle sounds, footsteps parented to a character, etc.

##### What Happens at Runtime

1. The container's `Use Occlusion` and `Allow Reverb Send` flags both feed the same slot decision: the voice acquires an OcclusionSlot on play (see chapter 14 §14.4).
2. `WriteReverbSendsImmediate` runs synchronously when the voice is configured, so its very first audio frame already has the correct send level.
3. The reverb-sends driver re-evaluates every ~50 ms while the voice is playing — sends respond to listener motion (audibility changes) even on static emitters.

✅ **Result:** This sound, fired anywhere in the world, contributes to whichever zone's reverb bus its source position is in — without any code changes.

---

#### Recipe: Override Routing for Music or Narration

📋 **What:** Make a specific container *always* send to a fixed reverb bus, regardless of where the listener stands or what zone the source is in.

🎯 **Use Case:** Music that should always carry a "cathedral choir" reverb. Narrator VO that should always have a tight studio reverb. UI tails that should bypass per-zone routing. Anything whose acoustic character should be authored, not derived.

##### Asset Setup

On the AudioContainer:

- **Allow Reverb Send**: ✓
- **Explicit Reverb Bus**: drag in the target `ReverbSendBus` asset
- **Reverb Send Level Db**: as desired

##### What Happens at Runtime

Per-tick, the reverb-sends driver sees the explicit bus reference and takes the **explicit-bus short-circuit**:

- The named bus gets the container's `ReverbSendLevelDb` at all times (no audibility or membership scaling).
- Every other registered bus is silenced for this voice.
- Source zone resolution is bypassed entirely.

The voice's character is now authored, not zone-derived. Listener motion changes nothing about the routing.

✅ **Result:** A music container that always plays with cathedral reverb, regardless of whether the player is inside a cathedral or outdoors.

---

#### Recipe: Tune a Bus's Character While Editing

📋 **What:** Iterate on a reverb's sound by tweaking values on the SendBus asset and pushing them to the mixer without re-running the full Generate.

🎯 **Use Case:** Dialing in a room's character during gameplay-tuning sessions.

##### Workflow

**Option A — Edit the SO, push to mixer:**
1. Select the `ReverbSendBus` asset
2. Tweak Decay Time, Room HF, Reflections Level, etc. on the SO
3. Click **Apply Params → Mixer**
4. Enter play mode and listen

Use when the SO is your source of truth. Apply Params skips the group/effect/expose work and only writes the parametric values — fast iteration.

**Option B — Edit the mixer window, pull back to SO:**
1. Open the Audio Mixer window, select the SendBus's group
2. Edit the SFX Reverb effect's parameters directly (snapshot must be the target snapshot, which is the default for fresh mixers)
3. Back on the SendBus asset inspector, click **Pull Params ← Mixer**
4. The SO now reflects your mixer-window tweaks

Use when the mixer window's UI is more convenient (it has the per-parameter sliders right there). Pull ensures the SO catches up so re-running Generate later doesn't undo your work.

⚠️ **Caveat:** Generate always overwrites the mixer effect's parameter values from the SO. If you've been tuning in the mixer window without pulling, Generate will wipe your tweaks. The safe workflow is Edit-mixer → Pull → Generate, or just stay on Option A.

✅ **Result:** Live iteration on reverb character without breaking the schema or losing changes.

---

#### Troubleshooting

**"My voice has `Allow Reverb Send = ✓` but I hear no wet tail."**
- Confirm `AudioManager.ReverbSendBusRegistry` is assigned and contains at least one bus.
- Confirm the source's zone has its `ReverbBus` field set to a bus that's in the registry.
- Confirm the bus's `ReverbSendBus` asset isn't silent: a parametric bus with `Room` at -10000 dB (the default) produces no tail — raise it toward 0. A convolution bus with no `Impulse Response` assigned is likewise silent.
- Confirm the listener can reach the source's zone (unreachable zones produce audibility = 0).
- Confirm the container's `Mixer Group` is a bus that appears in the Occlusion Layout's bus list — without a slot, there are no Send level params to write to.

**"Several zones share one bus and they all sound identical."**
- This is **by design** as of the source-of-truth unification: a bus has one acoustic identity, defined by its `ReverbSendBus` asset, regardless of which zone routed the voice to it. Per-zone variation on a *shared* bus is no longer supported.
- If you need Cathedral and Cathedral_Crypt to sound different, give them **separate** `ReverbSendBus` assets and point each zone's `ReverbBus` at its own.

**"I added a new ReverbSendBus but it doesn't appear in the dropdowns."**
- Open Window ▸ Audio System ▸ Reverb Send Buses and click Refresh. The registry only updates when refreshed.
- After refreshing, re-run the Occlusion Layout Builder so existing occlusion slots get the new bus's Send param authored.

**"Voices on this bus sound dry on the first frame, then reverb kicks in."**
- This was a previous bug — voices used to play dry for up to one driver tick (50 ms) before sends were written. The fix is `WriteReverbSendsImmediate`, which is called synchronously by `AudioContainer.ConfigureAudioSource`. If you're seeing this on a custom playback path that bypasses `ConfigureAudioSource`, call `WriteReverbSendsImmediate(voice, container)` yourself right after setting `voice.OcclusionSlot`.

**"My mixer-window tweaks vanish whenever I re-Generate."**
- Generate is non-destructive *only if the SO already matches the mixer*. Click Pull Params ← Mixer first to sync the SO to your live tweaks, then Generate becomes a no-op on those values. Or stay in the SO-first workflow and use Apply Params → Mixer for iteration.

---

### Ambient Propagation

**New in v2.3.0.** Zone/portal routing for long-running ambient beds (rain, wind, machinery, river). The propagation subsystem ships *alongside* the SFX event pipeline — both systems coexist on the same AudioManager and mixer. Use SFX events for discrete sounds, propagation for persistent environmental beds that need to route through geometry.

> 🧱 **Two propagation paths, only one needs slots.**
> - **`AmbientSource` path** (the recipes in this section: rain, doors, weather) uses its own `AmbientEmitter` pool. **No occlusion slots required.** Author zones, portals, and ambient sources and you're done.
> - **Container `Use Propagation` path** (an `AudioContainer` with the `Use Propagation` flag — see [Compose Occlusion with Propagation](#recipe-compose-occlusion-with-propagation) in the Occlusion topic) acquires an Occlusion Slot exactly like reverb sends do. **That path needs the Occlusion Slot Pool authored first.**

> ⚠️ **Silent failures to know up front**
> - Listener GameObject missing the `Audio Listener Zone Tracker` component → listener's zone is always null, all propagation is silent.
> - Portal collider doesn't overlap both zone colliders by ≥ 5 cm → portal is rejected in `OnValidate`. Check the inspector for the red error.
> - Portal +Z not pointing through the opening → blend axis is wrong, sound doesn't pan toward the gap.
> - Loop boundary click in your clip → the emitter loops as-authored, Unity won't fix a bad seam. Crossfade the WAV.
> - Outdoor source zone with finite `activationRadius` → source culls when the listener walks deep indoors. Use `float.PositiveInfinity` for sky/world sources.

#### Setup Path

For each ambient bed you want, follow these in order:

1. **Define Zones** — author one `AudioZone` per acoustic space.
2. **Connect with Portals** — one `AudioPortal` per doorway/window between zones.
3. **Add an Ambient Source** — looping clip + source zone.
4. *(Optional)* **Door Source** — drive a portal's open/closed state from a game system.
5. *(Optional)* **Intensity Driver** — script-control the source's base volume.

**Scene prerequisites** (one-time): `AudioManager` + `AudioListener` in the scene; an `Ambience` mixer group (or any group of your choice); the listener has the `Audio Listener Zone Tracker` component. Recipes below assume these are in place.

---

#### Recipe: Rain That Leaks Through a Window

📋 **What:** Outdoor rain audible inside a room only through an open window — quieter, dull, and panned toward the window as the listener walks along the opposite wall.

🎯 **Use Case:** Any ambient bed originating outside a closed interior — weather, traffic, a distant waterfall.

##### Scene Setup

**1. Two AudioZones:**
- `Zone_Outdoors` — huge BoxCollider enveloping the outdoor space
- `Zone_LivingRoom` — BoxCollider filling the room interior
- On each: Add → `Audio/Propagation/Audio Zone`
- For the outdoor zone, set `activationRadius = Infinity` (so the source doesn't get culled when the player is deep inside the house)

**2. One AudioPortal at the window:**
- Empty GameObject at the window frame
- BoxCollider sized ~1m × 1.2m × 0.4m spanning the opening
- Add → `Audio/Propagation/Audio Portal`
- Drag `Zone_Outdoors` into `ZoneA`, `Zone_LivingRoom` into `ZoneB`
- Orient the GameObject so its **+Z points through the opening**
- Inspector settings:
  - `Base Transmission = 0.6` (open window ≈ -4 dB)
  - `Closed Transmission = 0.1` (≈ -20 dB; doesn't matter if Door Source is null)
  - `Open Low Pass Hz = 12000`
  - `Closed Low Pass Hz = 600`
  - Leave `Door Source` **null** — windows don't swing

**3. The rain source:**
- Empty GameObject anywhere outside (say, above the house roof)
- Add → `Audio/Propagation/Ambient Source`
- Inspector:
  - `Source Zone = Zone_Outdoors`
  - `Looping Clip = Rain_Heavy_Loop.wav` (must be loop-authored!)
  - `Ambience Mixer Group = Ambience` (any mixer group you like)
  - `Source Base Volume Db = 0`

##### Code

None needed. The subsystem self-boots:
- `PropagationManager` auto-instantiates on the first `AudioZone.OnEnable`
- The listener tracker enters/exits zones on trigger crossings
- The solver runs at 15 Hz and drives the pooled `AmbientEmitter`

✅ **Result:**
- Standing outside → rain plays flat at 0 dB, no filtering
- Walking indoors → rain muffles down to ~-20 dB, high-frequency content rolled off by the window
- As you walk past the window wall, the virtual emitter slides along the frame so the rain pans naturally

---

#### Recipe: Door That Swings Between Muffled and Clear

📋 **What:** A bedroom door connecting a hallway to the street. As the door animates open/closed, propagation parameters interpolate smoothly in real time.

🎯 **Use Case:** Any door (physical or magical) that can change state during gameplay.

##### Scene Setup

**1. Three zones** (same pattern as above): `Zone_Street`, `Zone_Hallway`, `Zone_Bedroom`.

**2. Two portals:**
- Portal at the bedroom door → connects `Zone_Hallway` ↔ `Zone_Bedroom`, **has a Door Source**
- Portal at the front entrance → connects `Zone_Street` ↔ `Zone_Hallway`, can have its own Door Source or be null

**3. The door adapter:** Propagation doesn't know about your door system. Write a thin adapter that exposes `OpenProgress` and fires `OnChanged` when the state changes:

```csharp
using System;
using UnityEngine;
using AudioSystem.Propagation;

[RequireComponent(typeof(Animator))]
public class AnimatorDoorToPortal : MonoBehaviour, IPortalDoorSource
{
    [SerializeField] private string openParameterName = "openAmount";

    private Animator animator;
    private float lastReported = -1f;

    public float OpenProgress => animator != null
        ? Mathf.Clamp01(animator.GetFloat(openParameterName))
        : 0f;

    public event Action OnChanged;

    void Awake() => animator = GetComponent<Animator>();

    void Update()
    {
        // Only fire OnChanged on meaningful deltas — the portal re-solves
        // immediately whenever we raise this event, so don't spam every frame.
        float now = OpenProgress;
        if (Mathf.Abs(now - lastReported) >= 0.02f)
        {
            lastReported = now;
            OnChanged?.Invoke();
        }
    }
}
```

**4. Hook it up:** Drop the `AnimatorDoorToPortal` component on whatever GameObject drives your door (usually the door model root). Drag it into the bedroom `AudioPortal`'s `Door Source` field.

##### Code

```csharp
// Nothing — the door animation alone drives audio.
// If you want to script-toggle the door:
void OpenDoor()  => GetComponent<Animator>().SetBool("open", true);
void CloseDoor() => GetComponent<Animator>().SetBool("open", false);
```

✅ **Result:**
- Door closed → bedroom sits at near-silent muffled roar (~-26 dB + 600 Hz low-pass applied to the already-filtered path)
- Door animates open → transmission and cutoff interpolate every frame the animator changes `openAmount`
- Door slams closed mid-step → smooth attenuation, no pops

**Gotcha:** If your animator parameter is stepped (0/1 only), the portal transitions will still be smooth on the audio side thanks to per-frame emitter smoothing — but they'll be less perceptually natural than an animation that ramps. Prefer a continuous float parameter on the animator when possible.

---

#### Recipe: L-Shaped Room with Multiple Colliders

📋 **What:** A corridor that bends at a right angle, or a room with an alcove. Single BoxCollider won't cover the shape cleanly.

🎯 **Use Case:** Any non-rectangular acoustic space.

##### Scene Setup

1. On the AudioZone GameObject, **add multiple BoxCollider components** (one for each leg of the L, each with `isTrigger = true`)
2. Size them to cover the real interior. A small overlap at the corner is fine — `ContainsPoint` is OR-across all colliders
3. Do NOT add multiple `AudioZone` components; just more colliders on the same GameObject

```
GameObject "Zone_LShapedCorridor"
  ├─ AudioZone               (single component)
  ├─ BoxCollider (horizontal leg)
  └─ BoxCollider (vertical leg)
```

✅ **Result:** The zone behaves as one logical room. Authoring error detection (`OnValidate`) treats all triggers uniformly. Portals that overlap *any* of the zone's colliders are accepted.

**Future-proofing:** `AudioZone.ContainsPoint` and `ClosestSurfacePoint` are `virtual` — if you later need sphere or mesh shapes, subclass `AudioZone` and override those two methods. No changes to the solver or manager required.

---

#### Recipe: Weather Intensity Drives Rain Volume

📋 **What:** A `Weather_RainIntensity` global value (0..1) smoothly crossfades the rain bed from silent to 0 dB.

🎯 **Use Case:** Dynamic weather systems, seasonal changes, gameplay intensity scaling.

##### Code

```csharp
using UnityEngine;
using AudioSystem.Propagation;

public class RainIntensityDriver : MonoBehaviour
{
    [SerializeField] private AmbientSource rainSource;
    [Range(-60f, 0f)] [SerializeField] private float quietDb = -40f;
    [Range(-60f, 0f)] [SerializeField] private float loudDb  = 0f;

    /// <summary>Call this from your weather / gameplay system. 0..1.</summary>
    public void SetRainIntensity(float t)
    {
        if (rainSource == null) return;
        float db = Mathf.Lerp(quietDb, loudDb, Mathf.Clamp01(t));
        rainSource.SetBaseVolumeDb(db);
    }
}
```

The base volume is picked up on the next solve tick (15 Hz default ≈ 67 ms). Propagation attenuation is subtracted from it, so lowering base volume during a lull will still produce the correct filtered-and-quieter result through doorways.

✅ **Result:** One knob → rain swells or fades globally, while all of the per-portal routing, blending, and filtering stays correct.

---

#### Troubleshooting

**"Rain sounds full-volume indoors."**
- Check the portal's `+Z` points through the opening. If the portal's forward is parallel to the wall, the blend axis is wrong.
- Inspector error on the portal GameObject? Zone colliders must overlap the portal collider by ≥ 5 cm.

**"I hear a pop when crossing a doorway."**
- Clip isn't loop-authored. The emitter is set to `loop = true` — Unity won't fix a boundary click in the clip itself. Edit the clip, crossfade the seams.

**"No sound at all — AudioZones are registered but silent."**
- Did you add the `Audio Listener Zone Tracker` to your AudioListener? Without it, the listener's zone is always null.
- Confirm the `Ambience Mixer Group` isn't muted or at -∞ dB.
- Check the `activationRadius` on your source zone. Outdoor source zones should usually use `float.PositiveInfinity`.

**"Opening the door doesn't change anything."**
- Your `IPortalDoorSource` implementation needs to fire `OnChanged`. Without it, portals poll via the solve tick (~67 ms max latency at the default 15 Hz); with it, solves run immediately on state change.

**"The listener spawns inside a room and hears nothing until they walk out and back in."**
- Make sure the listener GameObject has a Rigidbody (the tracker auto-adds a kinematic one) and the zone collider's `isTrigger = true` (the zone's `OnValidate` enforces this).
- Propagation scans for listener membership on first frame — if that still misses, it's likely a layer/collision-matrix issue.

---

### Appendix: Quick Reference

#### Common Tasks Cheat Sheet

**Play Sound:**
```csharp
myEvent.Post();
myEvent.Post(gameObject, transform.position);
```

**Control Playback:**
```csharp
AudioHandle h = myEvent.Post();
h.SetVolume(0.5f);
h.SetPitch(2f);
h.Stop(1f);
h.Pause();
h.Resume();
```

**Bus Volume:**
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
AudioMultiHandle mh = myEvent.PostMultiPosition(emitterParent);
mh.SetVolume(0.8f);
mh.SetVoiceVolume(0, 0.5f);  // Per-voice control
mh.Stop(2f);
```


---

## 4. Manual

The Manual is the system's deep reference — architecture, design rationale, and chapter-by-chapter coverage of every subsystem. Read it once front-to-back to build a mental model, or jump straight to the chapter you need.

#### Manual Chapters

1. [Introduction](#1-introduction) — what this manual covers, prerequisites, how to read it
2. [System Architecture](#2-system-architecture) — design philosophy, the audio pipeline, component hierarchy
3. [Core Components](#3-core-components) — AudioManager, voice pool, GainStack, scheduling
4. [Audio Containers](#4-audio-containers) — Routing / Random / Sequence / Switch / Blend
5. [Events System](#5-events-system) — AudioEvent, EventAction, posting and lifecycle
6. [Bus & Mixing](#6-bus--mixing) — bus hierarchy, ducking, effect sends
7. [State System](#7-state-system) — states, state groups, transitions
8. [Multi-Position Audio](#8-multi-position-audio) — synchronized playback across multiple positions
9. [Performance & Optimization](#9-performance--optimization) — voice budgets, virtualization, LOD
10. [Advanced Features](#10-advanced-features) — DSP scheduling, runtime overrides, custom containers
11. [Best Practices](#11-best-practices) — recommended patterns for production projects
12. [Troubleshooting](#12-troubleshooting) — common failure modes and diagnosis
13. [Ambient Propagation Subsystem](#13-ambient-propagation-subsystem) — zone/portal graph routing for long-running beds
14. [Occlusion Mixer Slot Pool](#14-occlusion-mixer-slot-pool) — per-bus slot pools, multi-ray occlusion, propagation composition
15. [Reverb Send Buses & Per-Zone Reverb](#15-reverb-send-buses--per-zone-reverb) — source-side routing and listener-side bus-character driver

Appendices: [Glossary](#appendix-a-glossary), [Quick Reference](#appendix-b-quick-reference).

---

### 1. Introduction

#### 1.1 What This Manual Covers

This manual provides complete technical documentation of the SFX System. By the end, you'll understand:

- **System Architecture** - How everything fits together
- **Component Deep-Dives** - Every class, method, and property explained
- **Design Patterns** - Industry-standard audio middleware concepts
- **Performance Optimization** - Voice management, LOD, virtualization
- **Best Practices** - Proven patterns from professional audio design
- **Troubleshooting** - Solutions to common problems

#### 1.2 Prerequisites

Before reading this manual, you should:

- Have basic Unity knowledge (GameObjects, Components, Scripts)
- Understand audio concepts (dB, Hz, spatialization)
- Have read **QUICK_START.md** and successfully played a sound
- Know basic C# programming

#### 1.3 How to Use This Manual

This manual is dense by design — every chapter has to serve three different readers:

- 🏃 **Skimmer:** Read each chapter's intro paragraph and the §X.1 ("Why...") subsection. That builds a map. Come back for details when you actually need them.
- 📖 **Learner:** Read chapters 1–6 in order to build a complete mental model, then read the subsystem chapters (13, 14, 15) as you adopt those features.
- 🔍 **Reference reader:** Jump to the section whose title matches your question. Use the Table of Contents at the top.

**For Troubleshooting:** Skip to Section 12 for common issues and diagnoses.

> ⚠️ **Spatial audio (Occlusion, Reverb Sends, container Propagation) shares one piece of infrastructure: the Occlusion Slot Pool (Chapter 14).** If you plan to use any of these features, author the slot pool first or none of them will work. The Cookbook's [spatial-audio prerequisite callout](#3-cookbook) explains why.

#### 1.4 Companion Documents

- **QUICK_START.md** - Get running in 10 minutes
- **COOKBOOK.md** - Step-by-step recipes for common tasks
- **API_REFERENCE.md** - Complete API documentation

---

### 2. System Architecture

#### 2.1 Design Philosophy

The SFX System follows three core principles:

##### Principle 1: Data-Driven Design

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

##### Principle 2: Event-Based Triggering

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

##### Principle 3: Hierarchical Organization

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

#### 2.2 The Audio Pipeline

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

#### 2.3 Component Hierarchy

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

#### 2.4 Memory & Lifecycle

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

### 3. Core Components

#### 3.1 AudioManager

**Location:** `Core/AudioManager.cs` (partial class across multiple files)

**Purpose:** Central singleton managing all audio operations.

##### Responsibilities

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

##### Singleton Pattern

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

##### Inspector Configuration

| Property | Description | Default | When to Change |
|----------|-------------|---------|----------------|
| `masterVolume` | Global volume multiplier (0-1) | 1.0 | User settings menu |
| `muteAll` | Mute all audio | false | Debug/testing |
| `maxRealVoices` | Max physically playing voices | 32 | Mobile (16-24), High-end (48-64) |
| `maxVirtualVoices` | Max virtualized voices | 64 | Complex scenes with many sounds |
| `voiceUpdateInterval` | Voice management update rate (seconds) | 0.1 | Performance tuning |
| `occlusionMask` | Layers that block sound | Everything | Ignore player, small objects |
| `occlusionUpdateInterval` | Occlusion raycast rate (seconds) | 0.2 | Performance tuning |
| `enableLOD` | Distance-based virtualization | true | Always leave enabled |
| `lodDistances` | Distance thresholds for LOD | [10, 25, 50, 100] | Scale to world size |

##### Public API Overview

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

#### 3.2 AudioVoice & AudioVoiceEnhanced

**Location:** `Events/AudioVoiceEnhanced.cs`

**Purpose:** Represents a single playing audio instance with advanced features.

##### Class Hierarchy

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
    public OcclusionSlot OcclusionSlot;      // Mixer-side occlusion slot (null when not held)
}
```

##### The GainStack System

**Concept:** Multiple volume layers multiply together for final volume.

```csharp
public class GainStack
{
    public float BaseGain = 1f;       // Container/Event base volume
    public float BusGain = 1f;        // Bus hierarchy contribution
    public float OcclusionGain = 1f;  // Raycast-based attenuation
    public float RtpcGain = 1f;       // RTPC-driven modulation
    public float SchedulerGain = 1f;  // Crossfade/transition gain
    public float MultiplierGain = 1f; // Per-voice multiplicative — volume randomization,
                                      // Switch/Blend container outer Volume, PlayWithVolume scale.
    public float DuckingGain = 1f;    // (Deprecated) ducking is applied at the bus, not the voice.
                                      // Settable for back-compat but NOT in GetFinalGain.

    public float GetFinalGain()
    {
        return BaseGain * BusGain * OcclusionGain * RtpcGain * SchedulerGain * MultiplierGain;
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
- Crossfade/scheduler sets SchedulerGain
- Volume randomization, SwitchContainer/BlendContainer outer Volume, and `PlayWithVolume` write MultiplierGain
- Ducking is applied at the bus (the voice-level `DuckingGain` field is deprecated and not part of the final gain calculation)

**Example:**
```
BaseGain:        1.0   (container volume)
BusGain:         0.707 (bus at -3dB)
OcclusionGain:   0.5   (partially occluded)
RtpcGain:        0.8   (RTPC lowering volume)
SchedulerGain:   0.9   (fading in)
MultiplierGain:  0.9   (volume randomization -1 dB)

FinalGain = 1.0 * 0.707 * 0.5 * 0.8 * 0.9 * 0.9 = 0.229
```

##### Voice Priority System

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

##### Voice Virtualization

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

##### DSP Scheduling

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

#### 3.3 AudioHandle & AudioMultiHandle

**Purpose:** Provide playback control after posting an event.

##### AudioHandle (Single Voice)

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

##### AudioMultiHandle (Multiple Voices)

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

### 4. Audio Containers

Containers define how audio clips are organized and played. All containers inherit from `AudioContainer` base class.

#### 4.1 Container Base Class

**Location:** `Containers/audio-container-base.cs`

##### Common Properties

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

##### Common Methods

```csharp
public abstract AudioVoice Play(Vector3 position = default, GameObject parent = null);
public abstract void Stop();
public abstract void StopImmediate();
```

##### Randomization Implementation

```csharp
protected void ApplyRandomization(AudioVoice voice)
{
    if (enableVolumeRandomization)
    {
        float dbOffset = UnityEngine.Random.Range(volumeRandomMin, volumeRandomMax);
        float linearMultiplier = AudioExtensions.DbToLinear(dbOffset);
        // Enhanced voices route through the GainStack — the legacy
        // AudioVoice.volumeMultiplier field is ignored by the enhanced volume
        // calc, so writing only there used to silently no-op. MultiplierGain is
        // the per-voice slot that composes with the rest of the stack.
        if (voice is AudioVoiceEnhanced ev)
            ev.GainStack.MultiplierGain *= linearMultiplier;
        else
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

#### 4.2 RoutingContainer

**Menu:** Create > Audio System > Routing Container

**Purpose:** Simplest container. Plays all assigned clips simultaneously.

##### Properties

```csharp
List<AudioClip> AudioClips { get; }  // Clips to play
float Volume { get; }                 // Base volume (0-1)
bool Loop { get; }                    // Loop all clips
```

##### When to Use

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

##### Implementation Details

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

#### 4.3 RandomContainer

**Menu:** Create > Audio System > Random Container

**Purpose:** Picks one random clip per trigger, with weighting and repeat avoidance.

##### Properties

```csharp
List<WeightedAudioClip> AudioClips { get; }  // Clips with weights
int AvoidRepeatLast { get; }                  // Don't repeat last N clips (0-10)
bool UseWeighting { get; }                    // Enable weighted selection
float Volume { get; }                          // Base volume
bool Loop { get; }                             // Loop selected clip
```

##### WeightedAudioClip Structure

```csharp
[Serializable]
public class WeightedAudioClip
{
    public AudioClip clip;
    public float weight = 1f;           // Selection probability (0-10)
    public float volumeMultiplier = 1f; // Per-clip volume (0-1)
}
```

##### Weighted Selection Algorithm

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

##### Repeat Avoidance

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

##### When to Use

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

#### 4.4 SequenceContainer

**Menu:** Create > Audio System > Sequence Container

**Purpose:** Plays clips in a defined order with various playback modes.

##### Properties

```csharp
List<SequenceEntry> Entries { get; }                       // Ordered clip list
SequenceContainer.PlaybackMode SequencePlaybackMode { get; } // Forward/Reverse/PingPong/Random
bool LoopSequence { get; }                                   // Loop entire sequence
bool AutoAdvance { get; set; }                               // Auto-advance after clip finishes
float Volume { get; }                                        // Base volume
```

##### SequenceEntry Structure

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

##### Playback Modes

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

##### Implementation

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

##### Public Methods

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

##### When to Use

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

#### 4.5 SwitchContainer

**Menu:** Create > Audio System > Switch Container

**Purpose:** Selects child container based on game state switch value.

##### Properties

```csharp
string SwitchGroupName { get; }              // Which switch to monitor
List<SwitchEntry> SwitchEntries { get; }     // Value→Container mappings
AudioContainer DefaultContainer { get; }      // Fallback if no match
float Volume { get; }                         // Volume multiplier
```

##### SwitchEntry Structure

```csharp
[Serializable]
public class SwitchEntry
{
    public string switchValue;       // Switch value to match (e.g., "Metal")
    public AudioContainer container; // Container to play
}
```

##### How It Works

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
        // Apply this SwitchContainer's outer Volume on top of whatever the inner
        // container already set up. Enhanced voices route through GainStack;
        // the legacy AudioVoice.volumeMultiplier field is bypassed by the
        // enhanced volume calc, so writing only there silently no-ops.
        if (voice is AudioVoiceEnhanced ev)
        {
            ev.GainStack.MultiplierGain *= Volume;
            ev.UpdateFinalVolume();
        }
        else
        {
            voice.volumeMultiplier *= Volume;
        }
    }

    return voice;
}
```

##### Switch Groups Concept

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

##### When to Use

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

#### 4.6 BlendContainer

**Menu:** Create > Audio System > Blend Container

**Purpose:** Plays multiple containers simultaneously with RTPC-driven crossfading.

##### Properties

```csharp
List<BlendEntry> BlendEntries { get; }          // Layers to blend
string BlendParameterName { get; }              // RTPC to monitor
float Volume { get; }                            // Master volume
bool Loop { get; }                               // Loop all layers
IReadOnlyList<AudioVoice> LayerVoices { get; }  // Active layer voices
```

##### BlendEntry Structure

```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;    // Layer container
    public AnimationCurve volumeCurve;  // RTPC value (0-1) → Volume (0-1)
}
```

##### How It Works

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

        // Apply BlendContainer's outer Volume on top of the inner layer's gain.
        // Same reasoning as SwitchContainer — enhanced voices write to the
        // per-voice MultiplierGain slot, the legacy field is a fallback for
        // non-enhanced voices.
        if (voice is AudioVoiceEnhanced ev)
        {
            ev.GainStack.MultiplierGain *= Volume;
            ev.UpdateFinalVolume();
        }
        else
        {
            voice.volumeMultiplier *= Volume;
        }
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

##### Designing Volume Curves

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

##### When to Use

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

### 5. Events System

#### 5.1 AudioEvent Overview

**Location:** `Events/AudioEvent.cs`

**Purpose:** Triggers audio with complex behaviors and multiple actions.

##### Core Properties

```csharp
string EventName { get; }                    // Unique identifier
List<EventAction> Actions { get; }           // Actions to execute
VoicePriority Priority { get; }              // Voice stealing priority
int MaxInstances { get; }                    // Concurrent instance limit (0 = unlimited)
VoiceStealBehavior StealBehavior { get; }    // How to steal when max reached
float Cooldown { get; }                      // Minimum time between triggers
```

#### 5.2 Event Actions

**Concept:** One event can execute multiple actions sequentially or with delays.

##### Available Action Types

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

##### EventAction Structure

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

##### Multi-Action Example

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

#### 5.3 Instance Limiting

**Purpose:** Prevent event spam (e.g., 100 footsteps playing simultaneously).

##### Configuration

```csharp
[SerializeField] private int maxInstances = 0;  // 0 = unlimited
[SerializeField] private VoiceStealBehavior stealBehavior = VoiceStealBehavior.Oldest;
```

##### Steal Behaviors

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

##### Implementation

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

#### 5.4 Cooldown System

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

#### 5.5 Multi-Position Mode

**What:** Play the same event from multiple emitters simultaneously, perfectly synchronized.

##### Configuration

```csharp
[SerializeField] private bool enableMultiPosition = false;
```

##### How It Works

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

### 6. Bus & Mixing

#### 6.1 Bus Architecture

**Location:** `Events/AudioBus.cs`

**Purpose:** Hierarchical volume control and mixing.

##### Properties

```csharp
string BusName { get; }                  // Identifier
AudioBus ParentBus { get; }              // Hierarchy
AudioMixerGroup MixerGroup { get; }      // Unity AudioMixer integration
float VolumeDb { get; }                  // Volume in decibels
float VolumeMultiplier { get; }          // Linear multiplier (0-1)
bool Mute { get; set; }                  // Mute this bus
bool Solo { get; set; }                  // Solo this bus
```

#### 6.2 Decibel System

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

#### 6.3 Hierarchical Volume

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

#### 6.4 Ducking System

**What:** Automatically lower volume of target buses when this bus plays.

**Use Case:** Music ducks when dialogue plays.

##### Configuration

```csharp
bool EnableDucking { get; }                    // Enable ducking feature
List<DuckingTarget> DuckingTargets { get; }    // Buses to duck
float DuckingAttack { get; }                    // Fade down time (default 0.05s)
float DuckingRelease { get; }                   // Fade up time (default 0.5s)
```

##### DuckingTarget Structure

```csharp
[Serializable]
public class DuckingTarget
{
    public AudioBus targetBus;        // Bus to duck
    public float duckAmount = 0.5f;   // How much to duck (0-1, where 1 = full duck)
}
```

##### Example Setup

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

#### 6.5 Unity AudioMixer Integration

**Setup:**
1. Create AudioMixerGroup in Unity's AudioMixer window
2. Assign to bus's `mixerGroup` field
3. System routes voices through mixer automatically

**Benefits:**
- Use Unity's built-in effects (reverb, EQ, etc.)
- Visual mixing interface
- Snapshots and ducking (separate from SFX System ducking)

---

### 7. State System

#### 7.1 AudioState Overview

**Location:** `Events/AudioState.cs`

**Purpose:** Change multiple audio parameters based on game mode.

##### Properties

```csharp
string StateName { get; }                         // Unique identifier
string StateGroup { get; }                        // Mutually exclusive group
List<BusVolumeProperty> BusVolumes { get; }      // Bus volume changes
List<SwitchProperty> SwitchValues { get; }        // Switch changes
List<RTPCProperty> RTPCValues { get; }            // RTPC changes
List<EffectProperty> EffectProperties { get; }    // Mixer effect changes
```

#### 7.2 State Groups

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

#### 7.3 State Properties

##### BusVolumeProperty

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

##### SwitchProperty

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

##### RTPCProperty

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

##### EffectProperty

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

#### 7.4 State Transitions

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

#### 7.5 Common State Patterns

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

### 8. Multi-Position Audio

#### 8.1 Concept

**What:** Multiple speakers playing synchronized audio at different 3D positions.

**Real-World Example:** Nightclub with 8 speakers playing the same music.

**Challenge:** Without synchronization, speakers have phasing issues (sounds canceling out).

**Solution:** DSP-scheduled playback ensures sample-accurate synchronization.

#### 8.2 Architecture

##### AudioMultiPositionEmitterParent

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

##### AudioMultiPositionEmitterChild

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

#### 8.3 DSP Synchronization

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

#### 8.4 Setup Tutorial

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

#### 8.5 Per-Emitter Control

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

#### 8.6 Use Cases

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

#### 8.7 Performance Considerations

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

### 9. Performance & Optimization

#### 9.1 Voice Management

##### Voice Limits

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

##### Voice Stealing Strategies

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

##### Voice Virtualization

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

#### 9.2 Memory Optimization

##### AudioClip Import Settings

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

##### Resource Loading Strategy

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

#### 9.3 CPU Optimization

##### Update Intervals

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

##### Occlusion Optimization

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

**Per-Voice Occlusion:**
```
Only enable for important 3D sounds
Disable for:
  - 2D sounds (UI, music)
  - Distant ambience
  - Non-critical sounds
```

##### Distance-Based LOD

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

##### Listener Cache Management

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

#### 9.4 Mobile Optimization

**Recommended Settings:**
```csharp
maxRealVoices = 16;               // Lower limit
maxVirtualVoices = 32;            // Lower limit
voiceUpdateInterval = 0.15f;      // Slower updates
occlusionUpdateInterval = 0.4f;   // Much slower
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

#### 9.5 Profiling

##### Unity Profiler

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

##### Custom Debug Tools

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

### 10. Advanced Features

#### 10.1 Occlusion System

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

                // Low-pass cutoff is now written through the voice's OcclusionSlot mixer
                // group (see the Occlusion Mixer Slot Pool chapter), not via a per-voice
                // AudioLowPassFilter component.
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

#### 10.2 Ducking

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

#### 10.3 Crossfading

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

#### 10.4 Beat-Scheduled SFX (BeatScheduler)

**Purpose:** Fire an AudioEvent on a tempo grid (BPM) whose rate can be changed at runtime *without* affecting pitch.

Unity's `AudioSource.pitch` scales both speed and pitch together — doubling it lifts the clip an octave. For rhythmic SFX (heartbeats, metronomes, ticking clocks, pulsing UI, sonar pings) you usually want the opposite: faster cadence, same timbre. `BeatScheduler` solves this by **retriggering a one-shot event** on an interval of `60 / BPM` seconds instead of time-stretching a continuous clip. Pitch never changes because the clip itself never changes — only how often it is re-posted.

**Class:** `AudioSystem.BeatScheduler` (MonoBehaviour, `RunTime/Utilities/BeatScheduler.cs`).

##### When to use it

| Use case | Fit |
|---|---|
| Heartbeat whose BPM rises with stress | ✅ ideal |
| Metronome / rhythm game tick | ✅ ideal |
| Clock "tick-tock" | ✅ ideal |
| Pulse ambience (reactor hum, sonar) | ✅ good |
| Musical content that must stay on-grid | ⚠️ works for single-hit beats; for layered music use a dedicated music system |
| Continuous loop that should play slower without pitch drop | ❌ use a mixer Pitch Shifter effect with `source.pitch` inverse instead |

##### Timing model

Beats are scheduled against `AudioSettings.dspTime`, not `Time.time`. Each `Update()` checks whether the DSP clock has reached the next grid time; if so, it posts the AudioEvent with that grid time as the scheduled DSP start (`AudioEvent.Post(scheduledDspTime)` → `AudioSource.PlayScheduled`), so each beat's audible onset is grid-anchored rather than firing-frame-anchored. Spacing between beats is exactly `BeatInterval` regardless of frame-rate jitter.

**Hitch policy.** If `Update()` runs while one or more grid times are already overdue (frame stall, very high BPM, scene-load slowness), the scheduler fires audio **at most once per Update** — anchored to the most-recent overdue grid time — and drops earlier missed beats. This prevents voice-stacking on recovery, which used to surface as a +6 to +12 dB thump when multiple overdue voices landed on the same DSP buffer boundary. The `OnBeat` callback still fires once per missed beat up to `maxCatchUpBeatsPerFrame`, so beat-counting game logic (e.g. "every 4th beat") stays synchronized with the grid even across hitches. The grid phase is preserved across stalls: post-recovery, beats land at the original `T₀ + N × BeatInterval` rather than at `dspTime + interval`.

Changing `Bpm` at runtime (inspector field, property setter, or keystroke into the BPM number field) re-anchors the next beat to `dspTime + new BeatInterval` so the new tempo is audible within one beat. Earlier authoring did not handle intermediate keystroke values (typing "120" passed through `bpm=1` which would make the scheduler appear silent until a 60-second stale tick elapsed); this is now handled automatically.

##### Inspector fields

| Field | Purpose |
|---|---|
| `Event Name` | AudioEvent posted on every beat. Look up via `AudioManager.Instance.PostEvent`. |
| `Emitter` | Optional GameObject used as the sound source and spatial position. Defaults to the BeatScheduler's GameObject. |
| `Bpm` | Beats per minute. Also writable at runtime via the `Bpm` property. |
| `Max Catch-Up Beats Per Frame` | Cap on `OnBeat` callbacks per Update if the scheduler falls behind. Audio always fires at most ONCE per Update regardless — only `OnBeat` consumers (beat-counters driving game logic) see catch-up, keeping their count synced to the grid. Grid phase is preserved across stalls. |
| `Play On Enable` | Start scheduling when the component is enabled. |
| `Fire Immediately On Start` | If true, the first beat fires on the next frame. If false, it waits one full interval. |

##### Public API

```csharp
public float Bpm { get; set; }          // Clamp-guarded, min 1
public bool IsRunning { get; }          // True while scheduling
public double BeatInterval { get; }     // Seconds per beat, 60 / Bpm

public void StartScheduling();
public void StopScheduling();
public void ResyncPhase();              // Reset: next beat = now + BeatInterval

public event System.Action OnBeat;      // Main-thread callback per beat
```

##### Minimal example — stress-driven heartbeat

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

Set the BeatScheduler's `Event Name` to `Heartbeat_Thump` (a normal AudioEvent with a `RandomContainer` of 2–3 thump variations). No other plumbing required.

##### Common patterns

**Quantize gameplay to beats.** Subscribe to `OnBeat` to drive visual pulses, damage ticks, flashlight flicker, etc. The event fires on the main thread so Unity API calls are safe.

**Pause on gameplay pause.** Toggle the component's `enabled` or call `StopScheduling()` / `StartScheduling()`. The phase resets on restart unless you snapshot `BeatInterval` yourself.

**Multiple tempos simultaneously.** Add multiple `BeatScheduler` components to a GameObject, each pointing at a different event. They run independently and don't share a phase.

##### Limitations

- **Sample-accurate spacing, frame-bounded latency.** Each beat is scheduled to its grid DSP time via `PlayScheduled`, so spacing between consecutive beats is exactly `BeatInterval` regardless of frame-rate jitter. The audible onset of each beat can lag its grid time by up to one DSP buffer (Unity rounds past-time `PlayScheduled` to the next buffer boundary), but that lag is approximately constant across beats and doesn't affect perceived tempo. Suitable for rhythmic gameplay SFX and single-track musical-grid scheduling; for sample-aligned multi-track musical content, schedule `PlayScheduled` directly with explicit DSP times.
- **No musical phase alignment across schedulers.** Two schedulers at the same BPM are not guaranteed to fire on the same DSP frame.
- **Does not track segment/bar/measure structure.** It emits an unbroken stream of beats. For song-form authoring (intro / loop / outro), use an interactive-music layer above the SFX system.

---

### 11. Best Practices

#### 11.1 Project Organization

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

#### 11.2 Naming Conventions

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

#### 11.3 Bus Hierarchy Design

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

#### 11.4 Container Design

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

#### 11.5 Event Design

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

#### 11.6 State Design

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

#### 11.7 Common Pitfalls

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

### 12. Troubleshooting

#### 12.1 Sound Not Playing

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

#### 12.2 No Variations

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

#### 12.3 3D Sound Not Spatial

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

#### 12.4 Switch Container Not Switching

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

#### 12.5 Occlusion Not Working

**Symptoms:** Sounds not muffled by walls.

**Checklist:**

1. **Occlusion mask includes geometry?**
```
Check AudioManager.occlusionMask
Should include layer with walls/geometry
Shouldn't include player layer
```

2. **Container `Use Occlusion = true`?**
```
Each container opts in per-instance via the Use Occlusion toggle.
Voice Mixer + OcclusionLayout must also be assigned on AudioManager.
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

#### 12.6 Blend Container Not Blending

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

#### 12.7 Performance Issues

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

#### 12.8 Multi-Position Sync Issues

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

### 13. Ambient Propagation Subsystem

**New in v2.3.0.** An optional, additive subsystem that ships in the same package as the rest of the SFX_System. It is designed specifically for **long-running ambient beds** (rain, wind, crowd murmur, machinery, rivers) that must route through world geometry in a physically plausible way — leaking *from* the window when the listener is in another room, muffling behind closed doors, swelling as openings widen.

Propagation is **not** a replacement for the SFX event pipeline. It lives next to it, sharing the same `AudioManager`, the same mixer group structure, and the same `AudioListener`. SFX events continue to handle discrete sounds (footsteps, weapons, UI). Propagation handles the persistent acoustic background.

#### 13.1 Why This Subsystem Exists

**Standard 3D audio falls apart for ambient beds indoors.** Unity's built-in distance attenuation and a single `AudioLowPassFilter` raycast can occlude a sound, but they can't express:

- "The rain source is 100m away in the sky, but the listener should hear it *from the 2m-wide window*"
- "The door is halfway open, so the transmission is halfway between muffled and clear"
- "Walking through a doorway should smoothly crossfade the listener's acoustic perspective"

These are **routing** problems, not gain problems. Propagation solves them by modeling rooms as graph nodes and openings as edges, then running a per-frame shortest-path solve from every ambient source to the listener.

#### 13.2 Mental Model

```
Zone_Outdoors ─────[Window]───── Zone_LivingRoom ─────[Door]───── Zone_Hallway
   (rain source)                      (listener may be here)        (or here)
```

- **Zones** are volumes of world space tagged as one acoustic room. They're the graph's nodes.
- **Portals** are openings that connect two zones. They're the graph's edges.
- **Ambient sources** declare "I exist in this zone with this clip." They don't play audio themselves.
- The **solver** finds the best path from each source's zone to the listener's zone, accumulating transmission loss in dB and the minimum cutoff frequency along the way.
- The **emitter** — a persistent, pooled `AudioSource` — is positioned at the portal the listener is hearing the source *through*, with volume and low-pass driven by the accumulated path.

When the listener moves between zones, the path changes, the emitter glides to a new portal, and the filtering/volume update smoothly. When a door opens, the portal's transmission multiplier increases and the next solve propagates that change end-to-end.

#### 13.3 Architecture

**Namespace:** `AudioSystem.Propagation`
**Location:** `Assets/Scripts/SFX_System/RunTime/Propagation/`

```
Propagation/
├── Core/
│   ├── AudioZone.cs          ← MonoBehaviour, graph node
│   └── AudioPortal.cs        ← MonoBehaviour, graph edge + blend region
├── Runtime/
│   ├── PropagationManager.cs     ← singleton, orchestrates solve + emitters
│   ├── AmbientSource.cs          ← declares "there is an ambient here"
│   ├── AmbientEmitter.cs         ← pooled persistent voice
│   ├── AudioListenerZoneTracker.cs  ← attached to listener GO
│   └── PropagationProximityCuller.cs ← scale groundwork
├── Interfaces/
│   └── IPortalDoorSource.cs  ← minimal door contract (portable)
└── Internal/
    ├── PropagationSolver.cs  ← pure Dijkstra
    └── PropagationPath.cs    ← solve result struct
```

#### 13.4 The Solve Pipeline

Every `1 / solveRateHz` seconds (default 15 Hz, ≈ 67 ms), or immediately on any of several "dirty" triggers:

1. **Culling tick** (`PropagationProximityCuller.Tick`) — recomputes which zones and portals are within `activationRadius` of the listener. Runs every `cullCheckFrameInterval` frames (default 10). On the first tick after startup it's forced to run so the solver doesn't see an empty graph.

2. **Listener-zone resolution** — top-of-stack from `listenerZoneStack`. The stack uses last-entered-wins semantics so nested zones (e.g. Kitchen ⊂ House) resolve to the innermost zone the listener most recently crossed into.

3. **Blend portal selection** — if the listener is physically inside any `AudioPortal`'s trigger volume, that portal becomes the **blend portal**. If multiple overlap, the nearest wins.

4. **Per-source solve:**
   - **Blend region active?** Run Dijkstra **twice** — once targeting the zone on the portal's `blend=0` side, once targeting the `blend=1` side — then blend the two results using the listener's position along the portal's transition axis.
   - **Not in a blend region?** Single solve: source zone → listener zone.

5. **Apply results** to the source's `EmitterSlot`:
   - No active emitter yet → acquire one from the pool and `FadeInTo(…)`
   - Same portal as before → in-place retarget (smooth glide)
   - Different portal → move the current emitter to the fading slot, acquire a fresh one, crossfade

6. **Per-frame smoothing** (`AmbientEmitter.Tick`) — runs every frame regardless of solve rate. Smooths volume linearly, cutoff in log-frequency space, position in world space. Framerate-independent via `1 - exp(-smoothSpeed * dt)`.

**Dirty triggers that force an out-of-schedule solve:**
- Zone or portal registered / unregistered
- Listener entered / exited a zone or portal
- Ambient source registered / unregistered
- `IPortalDoorSource.OnChanged` fires
- Culler active set changes

**Outside-the-graph handling.** When the listener or source falls outside every registered `AudioZone`, propagation has no opinion. The `PropagationManager.silenceOutsideGraph` inspector toggle picks one of two readings:

- **Off (default — "whitelist mode").** `propGain = 1`, `propCutoff = 22 kHz`. Voices outside the graph play at full audibility. Right when AudioZones are a *whitelist* for bespoke acoustics and "outside the zones" means "the open world plays normally."
- **On ("complete-coverage mode").** `propGain = 0`, `propCutoff = 500 Hz`. Voices outside the graph fall silent and maximally muffled. Right when every walkable space is meant to be inside *some* zone and "outside" means the listener escaped intended geometry. The silence makes the gap obvious rather than masking the bug under full audibility.

The default is Off because surprise silence is harder to debug than surprise audibility. Affects only voices opted into `UsePropagation` — AmbientSources always behave as in whitelist mode regardless of the toggle.

#### 13.5 Solver Math

The solver is plain Dijkstra over the zone graph. The twist is what goes into the edge cost.

**Edge cost** (portal `p`):
```
cost(p) = -20 * log10(transmissionMultiplier(p))
        + distancePenaltyDbPerMeter * distance
```

Transmission is amplitude-space (`[0, 1]`). Converting to dB makes losses *additive* along a path — summing edge costs in dB is equivalent to multiplying amplitudes, which is the physically correct composition of cascaded openings.

**Distance penalty** is optional (`0` by default). Raise it slightly to prefer geographically closer portals when transmission is similar, e.g. so a listener in a long corridor routes rain through the nearer of two equally-open windows.

**Cutoff aggregation** is by **minimum** across the path, not sum. A chain of low-pass filters is dominated by the tightest one — later filters cannot restore frequencies earlier ones removed. The solver tracks this separately from the Dijkstra cost and reads out the final value once it reaches the listener zone.

#### 13.6 Blend Region Crossfading

The naive approach to doorway transitions — "if listener is in Zone A, play path A; if in Zone B, play path B" — produces an audible snap at the exact moment the listener crosses the threshold. Real rooms don't work that way; the listener's ears mix contributions from both spaces continuously across the doorway.

**The fix**, implemented in `PropagationManager.ApplyBlendedResult`:

1. Compute blend factor `t ∈ [0, 1]` from the listener's position along the portal's transition axis.
2. Solve twice — once targeting `GetZoneAtBlendFactorZero()`, once targeting `GetZoneAtBlendFactorOne()`.
3. **Volume**: blend in dB space (`Mathf.Lerp(aDb, bDb, t)`, then `10^(db/20)`) so the perceptual ramp is even.
4. **Cutoff**: blend in log-frequency space (`exp(lerp(log(aHz), log(bHz), t))`).
5. **Virtual emitter position**: linear world-space `Lerp` between the two path endpoints.
6. **Owning portal**: remains the blend portal throughout — never flipped mid-doorway, which would cause an unnecessary pool swap.

The result is a continuous, pop-free traversal of any doorway, regardless of how fast the listener walks through.

##### Auto-detecting blend axis orientation

Zone ordering (`ZoneA` vs `ZoneB`) is arbitrary — designers can drag them in any order. To avoid the manager needing to know which side is which, the portal auto-detects this in `DetectPositiveAxisIsZoneB`: it probes points slightly outside the trigger on each axis end and tests which zone's `ContainsPoint` returns true. The result is cached on first query. Designers never configure this directly — they just orient the GameObject's `+Z` through the opening.

##### Zone entry fades

Portals handle the "smooth doorway crossing" case via the blend region described above. **The interior of a zone has its own complementary mechanism**: `AudioZone.entryFadeMeters` (default 0). When set above 0, a point at the trigger surface has membership = 0 and a point that many world meters inside has membership = 1; in between, membership ramps linearly. Multi-collider zones take the max across colliders so overlapping boxes don't dip at internal junctions.

The runtime uses this membership to fade three things across the zone surface so designers don't hear a flip at the boundary:

- **Reverb-send level** (source-side) — a voice entering the zone tapers its contribution to the zone's reverb bus from 0 to full as it crosses the fade band.
- **Listener-side reverb profile** — when the listener crosses into a zone, the bus's `Room` parameter lerps from silent toward the zone's authored value across the band. Portal blend regions are unaffected; that path already has its own continuous crossfade.
- **`AudioZone.BaseVolumeDb` contribution** — a zone with non-zero baseline attenuation applies that baseline scaled by source membership, so AmbientSources crossing the surface ease in/out rather than stepping.

`entryFadeMeters = 0` (the default) preserves the original hard boundary — useful for tight rooms where you want instantaneous switching, or for any zone whose acoustic identity is binary by design. For non-rectangular zones built from multiple overlapping BoxColliders, set the fade smaller than the overlap depth to avoid dips at internal junctions.

#### 13.7 Emitter Pool Design

Propagation emitters are **persistent looping voices**, not short-fire SFX. That has two important consequences:

**1. They bypass the SFX voice pool by design.** `AudioManager.OcclusionPerf` iterates only the pool-owned `realVoices` list and applies raycast-based occlusion + low-pass filtering. If propagation emitters were registered with that pool, they'd receive **two** filters stacked — propagation's portal-based low-pass *plus* occlusion's raycast low-pass — sounding far too dull. By owning their own `AudioSource`, propagation emitters sidestep this without any change to the SFX path.

**2. The Ambience mixer group still applies.** The emitters route `AudioSource.outputAudioMixerGroup = ambienceMixerGroup`, so bus ducking, master volume, scene-unload mixer fades, and effect sends all still function. They're decoupled from the voice pool, not from the mix bus.

**Pool lifecycle per source:**

```
EmitterSlot {
    Active  : AmbientEmitter   // currently voicing the best path
    Fading  : AmbientEmitter   // previous emitter, ramping down after swap
}
```

On a portal change:
1. `Fading` is recycled to the pool (only if already silent — otherwise keep it and skip the swap, to avoid a pop from abruptly stopping an audible source)
2. Old `Active` → `Fading`, `FadeOut()` called
3. New `Active` acquired from pool, `FadeInTo(newPortal, …)` with minimum 50 ms ramp

**Global cap:** `maxAmbientEmitters = 16` (inspector). When exceeded — pathological scenes with many simultaneous sources — the farthest emitter is faded out first.

#### 13.8 Per-Frame Smoothing

Solves run at 15 Hz, but the ear is sensitive to parameter discontinuities at any rate. Every frame, `AmbientEmitter.Tick` pulls current values toward targets using:

```
k = 1 - exp(-smoothSpeed * dt)
current = lerp(current, target, k)
```

This is framerate-independent (doesn't drift at 30 FPS vs 120 FPS) and gives exponential approach with a time constant of `1 / smoothSpeed`. Default `smoothSpeed = 4` → ~250 ms to reach ~98% of a target. Values in the 3–5 range cover most use cases; higher values feel snappier but expose solve-tick boundaries.

**Pop-prevention rules**:
- Never start at full volume. First-frame rise is clamped to `dt / MinFadeSeconds` (50 ms).
- Never stop at non-zero volume. `Tick` only calls `Stop()` when both current and target are below the silence threshold AND `fadingOutForRelease` is true.
- Retargeting to an audible value clears the release flag, cancelling any in-progress fade-out.

#### 13.9 Door Abstraction (`IPortalDoorSource`)

Propagation is shipping inside the SFX_System package, which is consumed by many projects. It cannot depend on any specific project's door system. The solution:

```csharp
namespace AudioSystem.Propagation
{
    public interface IPortalDoorSource
    {
        float OpenProgress { get; }   // 0 = closed, 1 = open
        event System.Action OnChanged;
    }
}
```

**Any MonoBehaviour** that implements this interface can be dragged into an `AudioPortal`'s `Door Source` field. Projects bridge their own door system (ScriptableObject channels, event buses, animator parameters, physics-driven hinges) with a thin adapter MonoBehaviour. The portal subscribes to `OnChanged` and requests an immediate re-solve when the event fires — so fast door swings don't wait for the next solve tick.

Portals with `doorSource == null` are treated as always open — windows, archways, vents.

#### 13.10 Authoring Rules (Enforced by OnValidate)

Many propagation bugs are authoring mistakes. The inspector catches them:

- **Zone colliders must be triggers.** `AudioZone.OnValidate` force-sets `isTrigger = true` on every BoxCollider on the GameObject and logs a warning.
- **Portal colliders must overlap both zone colliders by ≥ 5 cm.** Without overlap, the listener can fall into a "no zone" gap at the doorway and emitters flicker. `AudioPortal.OnValidate` computes the AABB overlap and logs an inspector error on each zone that fails.
- **`closedTransmission` must be ≤ `baseTransmission`**, and `closedLowPassHz` must be ≤ `openLowPassHz`. Swapping these is a common mistake — the portal warns if detected.
- **Both zones required.** Null zone references log an inspector error. Zone self-loops (zoneA == zoneB) also logged.
- **Activation radius must be > 0.** Use `float.PositiveInfinity` to opt out of culling; zero is never valid.

#### 13.11 Scale: The Proximity Culler

Naïve Dijkstra over 200 zones + 400 portals is O(V·E) per solve, per source. The `PropagationProximityCuller` ensures the solver only sees what matters:

- Every `cullCheckFrameInterval` frames (default 10), recompute the active set based on per-node `activationRadius` distance from listener.
- Global hard caps (`globalMaxActiveZones = 64`, `globalMaxActivePortals = 128`) drop the farthest when the radius filter still yields too many.
- The listener's current zone is **always** included even if outside its radius — prevents silent emitters for a listener standing next to a culled-out zone.
- Opt out per node: `activationRadius = float.PositiveInfinity` (e.g. outdoor ambience zones that should never cull).
- Opt out globally: `cullCheckFrameInterval = 0`.

**Startup**: the culler forces its first recompute to run immediately regardless of interval, so the solver never sees an empty graph during the first 10 frames of play.

#### 13.12 Integration with Existing SFX_System Features

| Feature                          | Interaction with propagation                                                                          |
|----------------------------------|-------------------------------------------------------------------------------------------------------|
| `AudioManager` singleton         | Read-only consumer. Propagation does not modify AudioManager or add RTPC/state requirements.          |
| `SetRTPC` / `IRTPCListener`      | Reserved for global/weather parameters. Propagation drives per-emitter `AudioSource.volume` directly. |
| `AudioBus` + `AudioMixerGroup`   | AmbientEmitters route through a designated Ambience mixer group. Bus ducking and master fades still apply. |
| `AudioState`                     | Not wired in — designers can still author "Player_Outdoors" states that tweak the ambience bus.       |
| `AudioManager.OcclusionPerf`     | AmbientEmitters bypass the pool (no double-filtering). SFX voices that opt in via the container's `Use Propagation` flag compose propagation cutoff with their raycast cutoff (most-muffled-wins). |
| `AudioMultiPositionEmitterParent`| Orthogonal. Future `AmbientSource` refinement could delegate source-position selection to one.       |
| SFX events (`AudioEvent`)        | Containers can opt SFX voices into propagation cutoff (`Use Propagation`) and per-zone reverb sends (`Allow Reverb Send`). When neither flag is set, SFX is untouched by propagation. |

#### 13.13 Performance

- **Solver**: ~0.1 ms per source per solve for a culled graph of ~20 zones / ~40 portals (Editor, mid-range desktop). Reusable `PropagationSolver.Buffers` eliminate per-tick GC.
- **Solve cadence**: 15 Hz default (≈ 67 ms per tick). Mobile can safely drop to 8 Hz — the emitter's per-frame smoothing hides the lower rate for walking-speed movement. Faster zone-trigger crossings or partially-closed-portal transitions benefit from the higher default; drop below 10 Hz only if CPU profiling justifies it.
- **Emitters**: cost = `maxAmbientEmitters` AudioSource channels. Default cap is 16 — well within voice headroom even on mobile.
- **Allocation-free** in the hot path: solver buffers, scan scratch list, culler scratch list, and emitter pool are all reused.

#### 13.14 Design Limits

Known limits:

1. **Single-best-path only.** The solver picks one path per source. If rain leaks through both a window and an open skylight simultaneously, you hear it through whichever has lower transmission loss. Multi-path playback with per-portal emitters is a future enhancement.
2. **No diffraction.** Real sound bends around corners; the solver assumes straight-line zone adjacency. For small spaces this is rarely noticeable; for huge open-plan spaces you may want to subdivide into more zones.
3. **No raycast occlusion on AmbientEmitters.** AmbientEmitter routing is purely zone/portal topology — the pooled persistent voices do not participate in `AudioManager.OcclusionPerf`'s raycast pass. SFX voices opted into `Use Propagation` *do* compose both (propagation cutoff + raycast cutoff, min-wins). If you need clutter-based occlusion on top of an ambient bed (e.g. a crate blocking line of sight to the emitter), model the bed as an SFX-event-driven loop rather than an `AmbientSource`.
4. **Blend factor updates at solve rate, not per-frame.** For extremely fast doorway traversals (sub-100 ms) you may want to raise `solveRateHz` toward 20–30. For walking speeds the default 15 Hz is imperceptible.
5. **Box-shaped zones by default.** L-shaped zones work via multi-BoxCollider OR-union. For sphere/mesh-shaped zones, subclass `AudioZone` and override `ContainsPoint` / `ClosestSurfacePoint` — both are `virtual` for this reason.

#### 13.15 Troubleshooting

**Sound appears flat or doesn't propagate through doorways.**
- Confirm zones and portals are registered: `Debug.Log(PropagationManager.Instance)` in play mode; inspector shows the manager component if auto-instantiated.
- Zone colliders must have `isTrigger = true` (usually auto-forced). Check the physics layer matrix — if the listener's layer doesn't interact with the zone's layer, trigger events never fire.
- `AudioListenerZoneTracker` must be on the same GameObject as the `AudioListener`.

**Emitter flickers at a doorway.**
- Portal collider doesn't overlap one of the zone colliders by ≥ 5 cm. Inspector error will say which zone. Extend one or both to overlap.

**Pops or clicks.**
- Almost always an un-looped clip. The emitter sets `AudioSource.loop = true`; Unity replays the clip from sample 0 with no crossfade. The clip itself must be seamless.

**Door animation doesn't affect audio in real time.**
- The `IPortalDoorSource` implementation must fire `OnChanged` on value changes. Without it, the portal only picks up the new value on the next solve tick (≤ ~67 ms latency at the default 15 Hz).
- Spamming `OnChanged` every frame is fine — each fire triggers one re-solve, and the solve cost is modest — but prefer a threshold (e.g. `Δ ≥ 0.02`) to avoid redundant work during continuous animation.

**First solve after loading is silent for ~150 ms.**
- Fixed as of v2.3.0 — the culler's first tick runs immediately regardless of interval. If you still see this, confirm you're on v2.3.0 or later.

---

### 14. Occlusion Mixer Slot Pool

The system for muffling SFX voices behind walls, geometry, and (via propagation) through doorways. Ships alongside the propagation subsystem from chapter 13 and composes with it cleanly — propagation handles "rooms and openings," this chapter handles "stuff in the way between the listener and the emitter."

#### 14.1 Why Mixer Slots, Not Per-Source Filters

The obvious way to muffle a 3D sound is to attach an `AudioLowPassFilter` to its `AudioSource` and modulate the cutoff. We tried that first. It clicks.

The cause: `AudioLowPassFilter` carries internal biquad filter state — the running coefficients of the IIR. When the host AudioSource is stopped and re-played (which happens every time the voice pool reuses an `AudioSource`), Unity resets the biquad to default state at the next sample boundary. Whatever signal is in the source's playback buffer hits an uninitialised filter and produces a sample-boundary discontinuity audible as a per-acquire click. Pool-driven SFX, especially anything firing fast like footsteps or weapon shots, surface this as a rhythmic ticking layered under the actual sound.

Mixer effects don't have this problem. `AudioMixerEffectController` instances live on `AudioMixerGroup`s — they don't reset when an AudioSource starts and stops. If we author N mixer groups per logical bus, each with a Lowpass effect whose cutoff is an exposed mixer parameter, voices can be **routed through** one of those groups for the duration of their playback. The voice's own AudioSource has no filter component at all. Muffling becomes a mixer write to a stable, long-lived effect — no biquad reset, no click.

That's the slot pool. **One mixer group per slot, N slots per bus.** A pre-built pool of routing destinations that voices borrow and return.

#### 14.2 Mental Model

```
   ┌────────────────────────────────────────────────────────┐
   │  Voice Mixer (Unity AudioMixer asset)                   │
   │                                                          │
   │   Master                                                 │
   │     ├── Footsteps              ← logical bus            │
   │     │     └── Footsteps_OcclusionLayer                   │
   │     │           ├── Footsteps_Slot_00 ←─┐                │
   │     │           ├── Footsteps_Slot_01    │ voices borrow │
   │     │           ├── Footsteps_Slot_02    │ ONE of these │
   │     │           ├── Footsteps_Slot_03    │ at acquire   │
   │     │           ├── Footsteps_Slot_04    │ time         │
   │     │           └── Footsteps_Slot_05 ←─┘                │
   │     │                                                    │
   │     ├── Weapons                ← another logical bus     │
   │     │     └── Weapons_OcclusionLayer                     │
   │     │           ├── Weapons_Slot_00                      │
   │     │           …                                        │
   │     ⋮                                                    │
   └────────────────────────────────────────────────────────┘
```

- **Slots are pre-built**: the mixer groups, their Lowpass effects, and the exposed cutoff parameters are all authored at edit time. The runtime never adds or removes groups.
- **Voices acquire at play time**: when a container with `Use Occlusion` plays, `AudioManager.GetVoice(container)` calls `AcquireOcclusionSlot(bus)` to lease one of the free slots. The voice's `outputAudioMixerGroup` is set to the slot's mixer group for its entire lifetime.
- **Voices release on completion**: `ReturnVoice` pushes the slot back to the free queue. The released slot's cutoff is reset to 22 kHz (transparent) so the next voice that grabs it starts unmuffled.
- **Muffling is a mixer write**: every frame, `OcclusionPerf.LateUpdate` writes the smoothed cutoff target to the held slot's exposed parameter via `voiceMixer.SetFloat(slot.CutoffParam, hz)`. No filter component on the voice itself.

#### 14.3 Authoring the Slot Layout

Open **Window ▸ Audio System ▸ Occlusion Layout** to author the slot hierarchy:

1. **Voice Mixer** — assign the project's voice mixer asset.
2. **Occlusion Layout** — assign or create an `OcclusionLayout` ScriptableObject. This is where the per-bus slot map gets serialized; the runtime reads it at startup.
3. **Reverb Send Bus Registry** (optional, covered in chapter 15) — when assigned, each generated slot also receives one Send effect per registered reverb bus.
4. **Slots Per Bus** — how many concurrent occluded voices each bus can host (default 6).
5. **Scan, Auto-Create, Refresh** — walks the project for `AudioContainer` assets and `AmbientSource` components with `Use Occlusion = true`, collects the unique mixer groups they route to, and authors one `<BusName>_OcclusionLayer` child group per bus, each containing N slot groups (`<BusName>_Slot_00`, `_Slot_01`, …) with a Lowpass effect and exposed cutoff parameter on each.

After Generate, the `OcclusionLayout` asset holds the layout. Drop it into `AudioManager` ▸ Occlusion Layout, and the runtime builds its slot pools from it at `Awake`.

The "Scan only" button runs the same scan without mutating the mixer — useful for sanity-checking which buses *would* be authored without committing changes.

#### 14.4 Container Opt-In Flags

A voice acquires a slot when its container opts into any of three features:

| Container flag        | What it does                                                                       | Requires slot? |
|-----------------------|------------------------------------------------------------------------------------|----------------|
| `Use Occlusion`       | Raycast occlusion: muffle voices behind walls via listener→source linecast.        | Yes            |
| `Use Propagation`     | Propagation cutoff: muffle voices through portals via the propagation graph.       | Yes            |
| `Allow Reverb Send`   | Per-zone reverb sends (chapter 15).                                                | Yes            |
| `Static Emitter`      | Hint that this container's voices don't move. Speeds up the reverb-send driver.    | No (modifier)  |
| `Explicit Reverb Bus` | Override the zone-driven reverb send routing for this container.                   | No (modifier)  |

A container with none of the three feature flags routes voices directly to its bus (no slot). A container with at least one feature flag tries to acquire a slot when it plays; the AcquireOcclusionSlot call is upfront, before the AudioSource's `Play()`, because mid-playback `outputAudioMixerGroup` reassignment is click-prone in Unity.

**Pool exhaustion**: if all slots on the bus are in use when a new voice tries to acquire, `AcquireOcclusionSlot` returns null and the voice routes directly to the bus (unoccluded, no muffle, no reverb sends). A throttled warning is logged once per second per bus — bump Slots Per Bus on the layout asset if you see this consistently.

#### 14.5 Runtime Lifecycle

The slot lifecycle per voice:

```
container.Play(position)
  ↓
AudioManager.GetVoice(container)
  ↓
  if (UseOcclusion || UsePropagation || AllowReverbSend)
      AcquireOcclusionSlot(container.MixerGroup) → slot
  ↓
ConfigureAudioSource(voice)
  ↓
  source.outputAudioMixerGroup = slot.MixerGroup  (or container.MixerGroup if no slot)
  ↓
source.Play()
  ↓
  ... [voice plays] ...
  ↓
  Every OcclusionUpdate tick (default 5 Hz):
    OcclusionRaycastGain = ...        ← raycast target
    OcclusionRaycastCutoff = ...      ← raycast cutoff target
  ↓
  Every frame, LateUpdate:
    Read propGain / propCutoff from PropagationManager cache (if UsePropagation)
    Compose: gainTarget = raycastGain × propGain
    Compose: cutoffTarget = min(raycastCutoff, propCutoff)
    Exponentially smooth voice.GainStack.OcclusionGain toward gainTarget
    voiceMixer.SetFloat(slot.CutoffParam, smoothedCutoff)
  ↓
source.Stop() or natural completion
  ↓
AudioManager.ReturnVoice(voice)
  ↓
  ReleaseOcclusionSlot(slot)
    ↓
    voiceMixer.SetFloat(slot.CutoffParam, 22000)   ← reset cutoff for next voice
    voiceMixer.SetFloat(slot.SendLevelParams[*], -80)  ← silence reverb sends
    pool.freeIndices.Enqueue(slot.IndexInPool)
```

Two key invariants: **the slot is acquired before the first audio sample plays**, and **the slot is reset before the next voice borrows it**. Both are click-prevention guarantees.

#### 14.6 The Raycast Path (Use Occlusion)

For voices with `Use Occlusion = true`, the `OcclusionUpdate` coroutine fires a fan of rays from the listener toward the source on each tick (default 5 Hz, configurable via `Occlusion Update Interval`).

**Single ray** (`Occlusion Ray Count = 1`): the original behavior. A single linecast from listener to source. If blocked, the voice clamps to `raycastGain = 0.3` and `raycastCutoff = 500 Hz`; if clear, both stay at unity / 22 kHz. Binary step.

**Multi-ray partial occlusion** (`Occlusion Ray Count > 1`, default 3): fans N rays toward N points spread perpendicular to the listener→source axis at the source end, with spread width `Occlusion Ray Spread Meters` (default 0.5 m). Counts how many of the N rays are blocked, computes a fraction, and lerps gain/cutoff targets by that fraction. With `RayCount = 3` you get four discrete blocked levels (0/3, 1/3, 2/3, 3/3) which the per-frame smoother rides over for perceptually continuous edge transitions — walking past a wall edge produces a gradient instead of a 10 dB step.

This is **edge diffraction approximated**: when the listener is behind a wall whose far edge is near the source, the wall-side ray is blocked but the edge-side ray clears, producing a partial occlusion fraction that crossfades the source up as you walk into line of sight. It's not a true geometric diffraction solver, but it sidesteps the worst artefact of binary occlusion (the "sudden snap" as the listener crosses an edge) at the cost of N raycasts instead of 1.

Raycasts run with `QueryTriggerInteraction.Ignore` so `AudioZone` and `AudioPortal` trigger volumes don't act as occluders. The source's own collider hierarchy and the listener's collider hierarchy are filtered out, so the emitter's primitive and the player's capsule don't self-occlude the line.

#### 14.7 Composition With Propagation (Use Propagation)

For voices with `Use Propagation = true`, the `LateUpdate` smoother also reads the propagation subsystem's per-zone cache:

- `propGain` = `PropagationManager.GetSourceAudibility(sourceZone)` — the chain-of-portals linear amplitude weight from listener to source.
- `propCutoff` = `PropagationManager.GetSourcePropagationCutoff(sourceZone)` — the minimum cutoff across all portals on the path.

These compose with the raycast targets:

```
gainTarget   = raycastGain × propGain               (multiplicative)
cutoffTarget = min(raycastCutoff, propCutoff)       (most-muffled-wins)
```

Why min, not multiply, for cutoff? Cascaded low-pass filters are dominated by the tightest one — later filters can't restore frequencies an earlier one removed. The solver chains cutoffs the same way; raycast and propagation compose the same way; everything stays consistent.

**Outside-the-graph handling**: when the listener or source falls outside every registered `AudioZone`, propagation has no opinion. `PropagationManager.SilenceOutsideGraph` picks:

- **Off (default)**: `propGain = 1`, `propCutoff = 22 kHz`. Voices outside the graph play at full audibility. Use when AudioZones are a whitelist for bespoke acoustics and "outside" means "the open world plays normally."
- **On**: `propGain = 0`, `propCutoff = 500 Hz`. Voices outside the graph fall silent and maximally muffled. Use when every walkable space is meant to be inside *some* zone, and "outside" means "this is a level-design escape — surface the bug."

The default is Off because surprise silence is harder to debug than surprise audibility.

#### 14.8 Per-Frame Smoothing

The slow OcclusionUpdate tick (5 Hz default = 200 ms) is too coarse for perceptual continuity — a 0.3→1.0 gain step at the tick boundary surfaces as a click. The fix: **slow tick stores step targets, per-frame `LateUpdate` smooths toward them**.

Two inspector-tunable time constants on AudioManager:

| Field                                  | Default | What it controls                                       |
|----------------------------------------|---------|--------------------------------------------------------|
| `Occlusion Gain Smoothing Seconds`     | 0.06    | Tau for the gain (volume) axis. Settles to 95% in ~3τ. |
| `Occlusion Cutoff Smoothing Seconds`   | 0.04    | Tau for the lowpass cutoff axis.                       |

Smoothing is framerate-independent: `k = 1 - exp(-dt / tau)`, then `current = lerp(current, target, k)`. At 60 fps with `tau = 0.06`, `k ≈ 0.24` per frame.

**Keep the two taus close to each other.** Large mismatches read as "the tone opens before the volume" (or vice versa) across an occlusion edge. The defaults (0.06 gain, 0.04 cutoff) are deliberately close.

**Unity's own `AudioSource.volume` ramp**: Unity applies its own ~40 ms ramp on every `volume` write at default DSP buffer size. That sets a floor on how snappy occlusion gain can feel regardless of our smoother. Set Project Settings ▸ Audio ▸ DSP Buffer Size to "Best Latency" to drop the floor to ~10 ms; designers chasing sub-50 ms perceived occlusion will need this.

#### 14.9 Performance & Scaling

- **OcclusionUpdate cost**: `realVoiceCount × occlusionRayCount` raycasts per tick. At default settings (5 Hz × 3 rays × 20 voices = 300 rays/sec ≈ 5 raycasts per frame at 60 fps), this is negligible. Raycasts are microseconds each.
- **LateUpdate cost**: O(realVoiceCount) per frame for the smoother. One mixer SetFloat per voice with a slot. Tens of voices is free; hundreds is still cheap.
- **Pool sizing**: `defaultOcclusionSlotsPerBus` (default 6) means up to 6 concurrent occluded voices per bus. A scene firing more than 6 footsteps simultaneously on one bus will see pool exhaustion warnings — bump to 8–12 if needed. The cost per extra slot is a mixer group + one Lowpass effect + one exposed parameter, negligible at runtime.
- **Allocation-free** in the hot path: slot acquisition is queue dequeue (O(1), no allocation); cutoff writes are direct mixer calls.

#### 14.10 Design Limits

1. **Slot exhaustion → unoccluded fallback.** When the pool is empty, new voices play through the bus directly (no slot, no muffle). This is intentional — graceful degradation beats failing to play. Watch the console for the throttled exhaustion warning and bump slots per bus accordingly.
2. **Multi-ray is not true diffraction.** It approximates edge softening for the most common case (wall with a near edge) but doesn't handle wraparound, multiple reflections, or frequency-dependent diffraction.
3. **Mixer is the cutoff source of truth.** There's no per-voice cached "current cutoff" — `LateUpdate` reads back via `voiceMixer.GetFloat` and lerps. Other code that writes to the same exposed parameter (a custom inspector slider during play, an editor script) will interleave with the smoother.
4. **One layout per project.** The slot pool architecture assumes a single voice mixer. Multi-mixer projects need separate AudioManagers (rarely needed).

#### 14.11 Troubleshooting

**Voices play unmuffled even though `Use Occlusion` is on.**
- Is `Voice Mixer` assigned on AudioManager? Without it, slot pools never build and the warning at `Awake` will tell you so.
- Is `Occlusion Layout` assigned? Same diagnosis path.
- Did you re-run the Occlusion Layout Builder after adding the container? Containers added since the last Generate aren't in the layout yet.
- Inspect `AudioManager.OcclusionSlotPoolsReady` at play time — false means slot pools didn't build (mixer-layout mismatch, empty layout, etc.).

**Console says `Occlusion slot pool exhausted on bus 'X'`.**
- Self-explanatory: more concurrent occluded voices on that bus than slots configured. Increase `Slots Per Bus` on the layout asset and re-run Generate. The default 6 is sized for typical SFX density; weapon-heavy or footstep-dense scenes can want 10–16.

**Sound clicks on the first frame of playback.**
- If the voice has a slot, the click is almost never from the system — check clip authoring (leading sample boundary, encoder pre-roll). If you can repro on a slotless container by toggling `Use Occlusion`, file a bug.

**Bus routing looks wrong in the mixer.**
- After Generate, each slot's mixer group should be a child of `<BusName>_OcclusionLayer`, which is a child of the user-authored bus. If you see slots floating at the master root or under a different bus, the layout asset is stale relative to the current mixer — re-run Generate.

**A specific container's voices ignore occlusion entirely.**
- Confirm the container has `Use Occlusion = true`. Confirm its `Mixer Group` is set to a bus that appears in the layout asset's bus list (containers whose bus isn't in the layout will be skipped silently at runtime, since there's no pool for that bus).

---

### 15. Reverb Send Buses & Per-Zone Reverb

The "wet path" complement to the occlusion chapter. Where chapter 14 controlled how voices are *muffled* through geometry, this chapter controls how voices *contribute to* and *hear* the acoustic character of the rooms they're in. Two subsystems, one shared infrastructure.

#### 15.1 Why Per-Zone Reverb

A single global reverb sounds wrong everywhere. A bathroom and a cathedral need very different decay times, reflection densities, and room sizes; sticking one preset across the whole game means most rooms feel like the same generic space. Per-zone reverb fixes that — but naively, in two distinct ways that don't compose.

**Approach 1: per-source reverb.** Each ambient bed and each SFX voice carries its own reverb settings. Right for ambient beds that should sound like the room they originate in (rain heard *through a window* should carry the outdoor character, not the room's). Wrong for SFX that should sound like the room the *source* is in (a gunshot fired in the bathroom should keep its bathroom-reverb tail even when the listener walks into the hallway). And expensive: N reverb instances running simultaneously.

**Approach 2: per-listener reverb.** A single reverb whose settings update based on the room the listener is currently in. Cheap (one reverb), and right for some sounds — but completely wrong for "a gunshot fired in the bathroom while the player is in the hallway should still sound like a bathroom gunshot." This approach makes every active sound sound like wherever the listener happens to be standing.

Neither is correct alone. Real acoustics combine both:

- **Routing is source-driven**: a voice's contribution goes to the reverb of the room it's *physically in*.
- **Character is room-driven**: each reverb bus represents one acoustic space, and its parameters don't change just because the listener walked to a different room.

The Reverb Send Bus subsystem implements this split. **Source-side: which bus a voice sends to is determined by the voice's zone.** **Listener-side: each bus's character is its own — defined by the bus's `ReverbSendBus` asset, the single source of truth.** Walking from the bathroom to the cathedral doesn't change which bus the bathroom gunshot is sending to — and doesn't change what the bathroom bus sounds like. Only the listener's *audibility weight* to that bus changes (the muffled "I can still hear it through the wall" attenuation), and that's handled by the propagation subsystem feeding the source-side audibility cache.

#### 15.2 Mental Model

```
   Two independent axes per voice:

   ROUTING (source-side) — set on the AudioZone, decides which bus voices send to:
     AudioZone "Bathroom"   .ReverbBus = Reverb_Bathroom   ──►  send to Reverb_Bathroom bus
     AudioZone "Cathedral"  .ReverbBus = Reverb_Cathedral  ──►  send to Reverb_Cathedral bus
     AudioZone "Hallway"    .ReverbBus = (none)            ──►  no reverb sends from this zone

   CHARACTER (listener-side) — set on the ReverbSendBus asset, the single source of truth:
     ReverbSendBus "Reverb_Bathroom"   ──►  parametric params  OR  impulse response
     ReverbSendBus "Reverb_Cathedral"  ──►  parametric params  OR  impulse response
     (a bus's sound never depends on listener position or on any zone)
```

- **Voices** in the Bathroom zone whose container has `Allow Reverb Send = ✓` send a copy of their dry signal to `Reverb_Bathroom` at the container's authored level, scaled by the listener's audibility weight (1.0 same-zone, <1 through portals, 0 unreachable).
- **The `Reverb_Bathroom` bus** is one mixer group on the project's voice mixer. For a parametric bus it carries an SFX Reverb effect whose six listener-relevant parameters are exposed to the runtime via `AudioMixer.SetFloat`; for a convolution bus it carries the native convolution plugin (see §15.14).
- **The bus's character** comes entirely from its `ReverbSendBus` asset. For a parametric bus, the runtime driver writes the asset's six parameters onto the mixer effect each tick. The character does not depend on the listener's position or on any zone's profile — the asset *is* the room's identity.

#### 15.3 The Two Halves Working Together

The clean separation between routing and character produces these expected behaviors:

| Scenario                                                | Source-side (which bus) | Listener-side (bus character) | Audibility scaling |
|---------------------------------------------------------|-------------------------|-------------------------------|---------------------|
| Listener in bathroom, gunshot in bathroom               | Bathroom                | Bathroom bus character        | 1.0 (same zone)     |
| Listener in hallway, gunshot in bathroom (door open)    | Bathroom                | Bathroom bus character        | ~0.3 (portal loss)  |
| Listener in hallway, gunshot in bathroom (door closed)  | Bathroom                | Bathroom bus character        | ~0.05 (closed door) |
| Listener in cathedral, gunshot in bathroom (no path)    | Bathroom                | Bathroom bus character        | 0 (unreachable)     |
| Listener in cathedral, choir voice in cathedral         | Cathedral               | Cathedral bus character       | 1.0                 |

The bathroom gunshot keeps sounding like a bathroom gunshot regardless of where the listener stands — only its loudness changes based on the listener's audibility to the bathroom. Two different voices in two different zones send to two different reverb buses, both running simultaneously, each carrying its own room's character.

#### 15.4 The ReverbSendBus Asset

One ScriptableObject per acoustic space identity in the project. Create via Project ▸ Create ▸ Audio System ▸ Reverb Send Bus, name it after the room (e.g. `Reverb_Bathroom`).

Three layers of fields:

**Mixer wiring** (top of inspector):

| Field            | Purpose                                                                                                  |
|------------------|----------------------------------------------------------------------------------------------------------|
| `ReverbMixer`    | The voice mixer this bus lives on. Required — the bus's mixer group is auto-created under this asset.    |
| `ReverbBus`      | Auto-filled by Generate. The mixer group this SendBus owns. Leave null on first authoring.               |
| `OutputDestination` | Optional. The mixer group this bus's wet output routes to. Vestigial in single-mixer setups; left null is normal. |
| `ParentGroup`    | Optional. Drops the auto-created group under this parent instead of directly under master. The "ReverbsWet" return-bus convention. |

**Parametric reverb defaults** (the SFX Reverb effect's 14 parameters, split into Basic and Advanced):

- **Basic (4)**: `DryLevel`, `Room`, `DecayTime`, `ReverbLevel` — the parameters 80% of tuning happens on.
- **Advanced (10)**, behind an EditorPrefs-backed foldout that's closed by default: `RoomHF`, `RoomLF`, `DecayHFRatio`, `ReflectionsLevel`, `ReflectionsDelay`, `ReverbDelay`, `HFReference`, `LFReference`, `Diffusion`, `Density`.

These parametric defaults apply only when `Use Convolution` is **off**. When it's on, the bus ignores them and uses the impulse response instead.

**Convolution** (the alternative to the parametric reverb — see §15.14 for the full walkthrough):

| Field                   | Purpose                                                                                                  |
|-------------------------|----------------------------------------------------------------------------------------------------------|
| `Use Convolution`       | Off by default. When on, the bus uses a real impulse response (native convolution plugin) instead of the parametric SFX Reverb. The parametric fields above are ignored. |
| `Impulse Response`      | The IR `AudioClip` to convolve with. Must be CPU-readable — set its import **Load Type** to *Decompress On Load* or *PCM*. Mono or stereo (downmixed to mono in v1). Required when `Use Convolution` is on, or the bus is silent. |
| `Convolution Wet Trim`  | Wet output trim in dB (range -80…+20). 0 = unity. The live, cheap level control for the convolution wet path. |

A fourth convolution field, the IR *slot index*, is auto-assigned by Generate and hidden from the inspector — designers never set it.

The asset's **name is the bus id**. Sanitization strips characters that would produce invalid Unity exposed-parameter identifiers; a bus named `Cave (small)` becomes `Cavesmall` for parameter naming. Rename the asset and the bus identity follows.

#### 15.5 The ReverbSendBusRegistry Asset

One per project. A flat list of every `ReverbSendBus` asset in the project, used by:

- The runtime drivers, to enumerate which buses exist and which mixer parameters to write to.
- The Occlusion Layout Builder, to author one Send effect per registered bus on every occlusion slot.

Create via Project ▸ Create ▸ Audio System ▸ Reverb Send Bus Registry. Open the Reverb Send Buses window (Window ▸ Audio System ▸ Reverb Send Buses) and click Refresh — the registry auto-populates from every `ReverbSendBus` asset in the project.

Drop the registry into `AudioManager` ▸ Reverb Send Bus Registry. The runtime gates the reverb-sends driver on the registry being assigned and non-empty.

#### 15.6 Authoring Workflow (ReverbAutoCreator + Reverb Send Buses Window)

Two complementary editor UIs:

**Per-asset inspector** — select a `ReverbSendBus` asset to see the custom inspector. Four buttons:

| Button                    | What it does                                                                                                            |
|---------------------------|-------------------------------------------------------------------------------------------------------------------------|
| **Generate Mixer Group**  | Creates or finds the bus's mixer group and backfills `ReverbBus` on the SO. For a **parametric** bus: drops an SFX Reverb effect, writes the SO's parametric defaults to the target snapshot, and exposes the six runtime-driven parameters under schema names. For a **convolution** bus (`Use Convolution = on`): drops the native SFX Convolution Reverb plugin instead, assigns a free IR slot, and authors its IR Slot / Wet Trim / IR Gain params. Switching the toggle and re-generating auto-cleans the previous effect. Idempotent. |
| **Validate Wiring**       | Read-only health check. Reports "Wiring OK" or a bullet list of issues (missing mixer, missing effect, missing exposed param, mismatched OutputDestination, ParentGroup mismatch). |
| **Apply Params → Mixer**  | Pushes the SO's parametric values into the existing mixer effect without re-running the group/expose work. Use after tweaking parametric values on the SO that you want reflected on the mixer side. |
| **Pull Params ← Mixer**   | Copies the mixer effect's current values back onto the SO. Use after tweaking the mixer-window sliders that you want reflected on the SO side. |

**Project-wide window** — Window ▸ Audio System ▸ Reverb Send Buses:

| Button                            | What it does                                                                       |
|-----------------------------------|------------------------------------------------------------------------------------|
| **Create New Bus**                | Save File panel; creates a fresh `ReverbSendBus` asset at the chosen path.        |
| **Refresh**                       | Re-scans the project for SendBus assets and re-runs Validate on each.             |
| **Validate All**                  | Same as Refresh but with a summary HelpBox.                                       |
| **Generate All Missing**          | For every bus whose `ReverbBus` is null but whose `ReverbMixer` is assigned, runs `EnsureSendBus`. Buses with `ReverbBus` already filled are left alone (per-asset re-generation lives on the inspector). |

Each row in the list shows the bus's name, its assigned Mixer, its assigned Bus group (or `(not generated)`), and any Validate issues (collapsible).

#### 15.7 Six Exposed Parameters, Not Fourteen

Unity's SFX Reverb effect has 14 parameters. Generate exposes six:

| Suffix              | Parameter                  | What it drives                                      |
|---------------------|----------------------------|-----------------------------------------------------|
| `_Wet`              | `Reverb` (late reverb dB)  | The diffuse tail level                              |
| `_Decay`            | `Decay Time` (seconds)     | Reverb tail length                                  |
| `_Room`             | `Room` (dB)                | Overall wet-signal gain                             |
| `_RoomHF`           | `Room HF` (dB)             | High-frequency tilt on the wet path                 |
| `_DecayHFRatio`     | `Decay HF Ratio` (ratio)   | How fast highs decay relative to mids               |
| `_Reflections`      | `Reflections` (dB)         | Early reflections gain                              |

These match the 1:1 field set on `AudioReverbProfile` — they're the six parameters that **describe a room's identity in a way that varies meaningfully between rooms**. Cathedral and bathroom differ on these six.

The other eight (DryLevel, RoomLF, ReflectionsDelay, ReverbDelay, HFReference, LFReference, Diffusion, Density) are static shaping rather than runtime-driven character — they describe *how this reverb is shaped* and stay at the SO-authored snapshot values, not written each tick.

The exposed-parameter names follow the schema `Reverb_<BusName>_<Suffix>`. The constants live on `ReverbSendBus` (`SuffixWet`, `SuffixDecay`, …) so authoring and runtime can't drift out of name agreement.

This whole six-parameter mechanism applies to **parametric** buses only. A convolution bus exposes no SFX Reverb params (its character is the impulse response) and is skipped by the listener-side driver — see §15.14.

#### 15.8 The AudioZone Side

The zone's connection to the reverb subsystem is **routing only** — a single field:

```csharp
public AudioMixerGroup ReverbBus;       // routing target (source-side)
```

- **`ReverbBus`** is the mixer group voices physically in this zone send to (when their container has `Allow Reverb Send = ✓`). Drag in the `ReverbBus` field from a `ReverbSendBus` asset. Leave null for zones that don't have a dedicated reverb (the source-side send is then skipped for sources in that zone).

> **Source-of-truth note (changed 2026-05-21).** The zone no longer carries any reverb *character* — the old `AudioZone.ReverbProfile` field was removed. A bus's character lives entirely on its `ReverbSendBus` asset, which is now the single source of truth. To make a room "sound reverberant," tune the parameters (or assign an IR) on the bus asset, not on the zone. The zone only decides *which* bus voices route to (and, via `entryFadeMeters`, how that send fades across the zone boundary).

Multiple zones can feed the same bus (e.g. `Cathedral` and `Cathedral_Crypt` both pointing at `Reverb_Cathedral`) — useful for routing several rooms to one shared acoustic identity. Because character is now per-bus, **every zone feeding a given bus produces that bus's single character**; there is no per-zone variation on a shared bus. If you need two rooms to sound different, give them separate bus assets.

#### 15.9 The Source-Side Runtime: `AudioManager.ReverbSends`

The "which bus, how loud" half. Runs at `reverbSendsUpdateInterval` (default 50 ms = 20 Hz) from a coroutine alongside the listener-side driver. Per tick, per voice with a slot:

1. **No container or `AllowReverbSend = false`**: every authored send on this slot gets silenced (so stale levels from a prior tick or prior voice don't leak).
2. **`ExplicitReverbBus` set**: the explicit-bus short-circuit. The voice sends at its container's `ReverbSendLevelDb` to the explicit bus only; all other registered buses get silenced. Use for sounds that need fixed acoustic character regardless of where the listener stands — music, narrator VO, scripted UI tails, cathedral choir always sounding like a cathedral.
3. **Otherwise — zone-driven routing**:
   - Resolve the source's current zone (cached for static emitters, re-resolved each tick for movers).
   - Look up listener audibility to that zone from the propagation cache: same-zone = 1.0, through portals = the linear-amplitude chain product, unreachable = 0.
   - Fold in source membership (`AudioZone.GetMembershipFactor`) so voices near a zone's surface taper smoothly across the boundary instead of flipping.
   - For each registered bus, compute `db = baseDb + 20·log10(audibility × membership)` if and only if the source's zone feeds *this* bus; else write the silent floor (-80 dB).

The per-tick `AudioMixer.SetFloat` on the slot's `SendLevelParams[bus.BusName]` writes the result.

**Immediate-on-Play write**: `AudioContainer.ConfigureAudioSource` calls `WriteReverbSendsImmediate` synchronously, so a voice's first audio frame already carries correct reverb routing. Without this, transient sounds (gunshots, single footstep clicks) on static emitters would play dry for up to one driver tick.

#### 15.10 The Listener-Side Runtime: `AudioManager.ReverbBusParams`

The "what does each bus sound like" half. Same coroutine, same cadence. Per tick, per registered bus:

1. **Skip convolution buses** — their character is the impulse response, not parametric params (the six `Reverb_<Bus>_*` params aren't exposed on them).
2. **Read the bus's own character** from its `ReverbSendBus` asset via `bus.GetDefaultProfile(scratch)` into a non-allocating scratch profile. No zone lookup happens — the asset is the source of truth.
3. **Write six `AudioMixer.SetFloat`s** to the bus's exposed parameters (`Reverb_<Bus>_Wet`, `_Decay`, `_Room`, `_RoomHF`, `_DecayHFRatio`, `_Reflections`).

Each bus has one stable acoustic identity that never depends on listener position — only the source-side audibility (loudness) changes when the listener moves. Re-driving every tick (rather than once) is belt-and-suspenders against exposed-param fallback ambiguity; it's six `SetFloat`s per parametric bus, negligible for a handful of buses.

> **Changed 2026-05-21.** This driver used to resolve a "canonical zone" per bus (listener's zone, else first-registered via `FindFirstZoneFeedingBus`, else SO defaults) and write *that zone's* `ReverbProfile`. The source-of-truth unification replaced all of that with the unconditional `bus.GetDefaultProfile` read above. `FindFirstZoneFeedingBus` is no longer used by this path.

#### 15.11 Container Opt-Ins, Revisited

The container flags that interact with this subsystem (defined in chapter 14, repeated here for context):

| Container flag              | Effect on the reverb-send path                                                                |
|-----------------------------|-----------------------------------------------------------------------------------------------|
| `Allow Reverb Send`         | Master opt-in. When off, voices from this container never send to any reverb bus.             |
| `Reverb Send Level Db`      | Per-container baseline level (dB) for the send. 0 dB = "as loud as dry", -∞ dB ≈ silent.      |
| `Static Emitter`            | Optimization: skip the source-zone resolve every tick for non-moving voices.                  |
| `Explicit Reverb Bus`       | Override the zone-driven routing entirely. Voice always sends to this bus at the authored dB. |

The `Reverb Send Level Db` slider on the container is the right place to dial individual sounds in or out of the wet mix without touching the zone's character — gunshots usually want a healthy send (0 to -3 dB), footsteps usually want quieter (-12 to -20 dB), UI clicks want either no reverb at all (`Allow Reverb Send = off`) or a tight UI-only reverb via `Explicit Reverb Bus`.

#### 15.12 Performance

- **Reverb-sends driver cost**: O(voiceCount × busCount) per tick. Default 20 Hz × 32 real voices × (say) 5 buses = 3200 `SetFloat`s/sec. Negligible.
- **Bus-params driver cost**: O(busCount) per tick — six `SetFloat`s per parametric bus, convolution buses skipped, no zone scan (the old `FindFirstZoneFeedingBus` walk was removed in the source-of-truth unification). 5 buses × 20 Hz ≈ 600 `SetFloat`s/sec. Negligible.
- **Static-emitter cache**: `voice.CachedSourceZone` avoids the O(zoneCount) `GetZoneContainingPoint` walk every tick for voices marked `StaticEmitter`. Footsteps parented to the player don't get this — moving emitters re-resolve every tick.
- **`reverbSendsUpdateInterval`** (default 0.05 s = 20 Hz) trades responsiveness for CPU. Drop to 0.02 s (50 Hz) only if you have fast projectiles crossing zone boundaries and have measured the CPU is fine.

#### 15.13 Design Limits

1. **One reverb effect per bus.** The schema assumes a single SFX Reverb on each generated mixer group. Stacking multiple reverbs on a bus is possible but not surfaced through the SO; you'd lose the Apply/Pull round-trip with the SO and the auto-exposed parameter set.
2. **Six driven parameters, eight static.** If you need `Diffusion` or `Density` to vary per-room, you'd extend `AudioReverbProfile` and add new `Suffix*` constants to `ReverbSendBus`. The architecture supports it; v1 just doesn't ship it.
3. **One character per bus.** A bus has a single acoustic identity (its `ReverbSendBus` asset). Several zones can route to one bus, but they all get that one character — there is no per-zone variation on a shared bus, and no mixing-by-distance between zones. If two rooms must sound different, author two bus assets.
4. **Convolution is Windows-only and in beta.** The native convolution path (§15.14) ships for Windows x64 only — macOS and consoles are Phase 4. It's feature-complete and build-validated for that scope, but carries known caveats: no runtime IR swap (the IR is fixed at author/scene-load time), ambient emitters can't render convolution (they fall back to inert parametric defaults — §15.14), and the convolver rebuilds on the audio thread (fine at load time, not yet ready for in-game IR changes).

#### 15.14 Convolution Reverb (Windows, beta)

Convolution is the **alternative engine** for a reverb bus. Instead of Unity's parametric SFX Reverb (which approximates a room from a handful of knobs), a convolution bus convolves the dry send with a real **impulse response** (IR) — a recording of how an actual space (or an external hardware/plugin reverb) responds to an impulse. This captures the true character of cathedrals, stairwells, parking garages, or a favorite Lexicon patch in a way the parametric reverb can't.

It's an **opt-in, per-bus** choice: parametric stays the cheap default for ordinary zones; convolution is the path for hero spaces.

> **Status: beta, Windows x64 only.** Feature-complete and validated in a standalone Windows build, but see the caveats at the end of this section before relying on it. macOS and consoles are Phase 4.

**When to use which:**

| Use parametric (SFX Reverb)                       | Use convolution                                            |
|---------------------------------------------------|------------------------------------------------------------|
| Generic / cheap zones, lots of them               | Hero spaces where the room's identity matters              |
| You want to tune by ear with knobs                | You have (or can capture) a real IR of the space           |
| Cross-platform today                              | Windows only for now                                       |
| Live parameter changes are fine                   | IR is fixed at author/scene-load time (no in-game swap)    |

**Authoring a convolution bus:**

1. On the `ReverbSendBus` asset, tick **Use Convolution**. The parametric fields are ignored from here on.
2. Assign an **Impulse Response** clip. Set its import **Load Type** to *Decompress On Load* or *PCM* so the CPU can read its samples — without this the runtime upload fails (`GetData failed`). Mono or stereo (downmixed to mono in v1); ~4 s at 48 kHz is a typical ceiling for SFX.
3. Set **Convolution Wet Trim** (dB) to taste — this is the live, cheap level control for the wet path.
4. Click **Generate Mixer Group** on the asset. This drops the native *SFX Convolution Reverb* plugin on the bus group (instead of SFX Reverb), assigns the bus a free IR slot, and authors the plugin's IR Slot / Wet Trim / IR Gain params.
5. **Wire the send path** the same way as a parametric bus: run the Occlusion Layout Builder's **Auto-Create** so each slot gets a Send and the bus group gets a Receive. *Without this the bus is silent with no error* — see §15.6 / the workflow note. (This step is identical for parametric and convolution buses; convolution didn't change the send/receive plumbing.)

**Runtime behavior:** the IR samples are uploaded into the plugin's slot table at `AudioManager` startup (`UploadConvolutionIRs()` in `Awake`). The slot table is volatile, so the IRs re-upload cleanly on every play / scene reload. The authored slot index + Wet Trim / IR Gain params live in the mixer asset; the IR *samples* come from this runtime upload. Nothing depends on driving exposed params from script at runtime.

**Requirements:** the native plugin `SFXConvolutionReverb.dll` must be present under `RunTime/Plugins/Windows/x86_64/` with its Plugin Importer set to **Load on startup = true** (without it the effect silently never registers in the mixer — see `CRITICAL_UnityAudio.md` §14). It ships in the repo; you only rebuild it if you change the C++ source under `NativeAudio/SFXConvolutionReverb/`.

**Caveats (beta):**

- **No runtime IR swap.** The IR is fixed once uploaded. Changing IRs during gameplay (zone-driven IR selection, crossfade) is Phase 4 and requires moving the convolver rebuild off the audio thread first.
- **Ambients can't render convolution.** `AmbientEmitter` uses a per-source *parametric* `AudioReverbFilter`, not the convolution plugin. An ambient whose listener-zone routes to a convolution bus gets that asset's (inert) parametric defaults — **not** the impulse response. Treat ambients as parametric-only for now, or route them through the bus's send/receive.
- **Convolver rebuild is on the audio thread.** Fine because IRs only (re)assign at author / scene-load time, where the one-time hitch is inaudible. It would glitch if IRs swapped mid-gameplay — hence the no-runtime-swap caveat above.
- **Malformed/empty IR clips aren't fully guarded yet.** Trust your IR clips; a bad clip's behavior is undefined pending an input-validation pass.

#### 15.15 Troubleshooting

**"Voices play but no reverb tail."**
- Confirm `Allow Reverb Send = ✓` on the container.
- Confirm the container's `Reverb Send Level Db` isn't at -∞ / -80 dB.
- Confirm `AudioManager.ReverbSendBusRegistry` is assigned and non-empty.
- Confirm the source's zone has `ReverbBus` pointing at a `ReverbSendBus` whose `ReverbBus` (the mixer group) is in the registry.
- Confirm the listener can reach the source's zone (a zone unreachable via portals produces audibility = 0).

**"A bus has no character — no decay, no reflections."**
- Did you run Generate on its `ReverbSendBus` asset? Until you do, the mixer group has no effect on it.
- Click Validate Wiring on the asset. It'll list every missing piece.
- Parametric bus: check the asset's `Room` value — at `-10000 dB` (the default) the reverb is intentionally silent. Drag toward 0 to enable.
- Convolution bus: check it has an `Impulse Response` assigned and that the clip is CPU-readable (Load Type = Decompress On Load / PCM). See §15.14.

**"The bus sounds like the wrong room."**
- Character comes from the bus's own `ReverbSendBus` asset, not from any zone, so "wrong room" means the wrong *asset* is wired. Confirm the zone's `ReverbBus` points at the bus group you think it does, and that you tuned (or assigned the IR to) that same asset.
- Remember several zones routing to one bus all share that bus's single character — that's by design, not a bug.

**"After re-generating, my mixer-window manual tweaks were wiped."**
- Generate runs `WriteParametricDefaults`, which pushes the SO's values into the mixer effect's target snapshot. If you've been tuning in the mixer window directly, run **Pull Params ← Mixer** *before* the next Generate to bring the SO into sync. After that, Generate is non-destructive — it writes back what's already there.

**"Voices in zone A still send to bus A's reverb after I changed zone A's `ReverbBus`."**
- The reverb-send driver re-resolves source zones every tick (or once-on-acquire for static emitters). Static-emitter voices in flight at the moment of the change still hold their cached zone. Stop and re-play them, or set `Static Emitter = ✓` only on voices that genuinely don't change zones.

**"Pool exhaustion warning on a bus that previously worked."**
- Slot pool exhaustion is the same warning shape from chapter 14 — see that troubleshooting block. Reverb-send voices and occlusion voices share the same per-bus slot pool, so a `weapons` bus configured for 6 slots can handle 6 *combined* occluded-or-reverb-sending voices, not 6 of each.

---

### Appendix A: Glossary

**AmbientEmitter:** Pooled persistent looping voice managed by the propagation subsystem. Bypasses the SFX voice pool; routed through an AudioMixerGroup.

**AmbientSource:** Component declaring "there is an ambient bed in this zone with this clip." Doesn't play audio directly — the propagation manager drives a pooled AmbientEmitter on its behalf.

**AudioContainer:** ScriptableObject defining how audio clips are organized and played

**AudioEvent:** ScriptableObject triggering audio actions

**AudioHandle:** Control interface for single voice playback

**AudioMultiHandle:** Control interface for multi-voice playback

**AudioPortal:** Propagation subsystem edge — connects two AudioZones and models an opening (door, window, archway) with transmission and low-pass cutoff driven by an optional IPortalDoorSource.

**AudioVoice:** Single playing audio instance

**AudioZone:** Propagation subsystem node — a volume of world space tagged as one acoustic room. Supports multiple BoxColliders for L-shapes.

**Bus:** Volume control group in hierarchical structure

**Crossfade:** Smooth transition between two containers

**Decibel (dB):** Logarithmic unit of volume

**Ducking:** Automatic volume reduction of target buses

**DSP Time:** Digital Signal Processor time for sample-accurate scheduling

**GainStack:** Layered volume control system

**IPortalDoorSource:** Interface any MonoBehaviour can implement to drive an AudioPortal's open/closed state. Decouples propagation from any specific project's door system.

**Instance Limiting:** Maximum concurrent playbacks of same event

**LOD:** Level of Detail, distance-based audio optimization

**Multi-Position:** Synchronized playback across multiple emitters

**Occlusion:** Raycast-based sound blocking

**Portal:** See AudioPortal.

**Priority:** Voice stealing priority (Low, Medium, High, Critical)

**Propagation:** Zone/portal graph-based routing of long-running ambient beds through world geometry.

**PropagationManager:** Singleton orchestrator of the propagation subsystem. Owns registries, the solver, and the emitter pool. Auto-instantiated on first reference.

**RTPC:** Real-Time Parameter Control, dynamic audio parameter

**State:** Game mode affecting multiple audio parameters

**Switch:** Conditional container selection

**Virtualization:** Time-tracking without audio output

**Voice:** Playing audio instance

**Voice Stealing:** Stopping voice to make room for higher priority

**Zone:** See AudioZone.

---

### Appendix B: Quick Reference

#### Common Code Patterns

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

## 5. Migration

### Why Migrate?

#### Current Pain Points (Resources-based)
❌ Events MUST be in `Assets/Audio/Resources/Audio/Events/` (confusing, easy to mess up)
❌ States MUST be in `Assets/Audio/Resources/Audio/States/`
❌ Resources folder increases build size
❌ No validation when placed incorrectly (silent failures)
❌ Tedious folder management

#### Benefits After Migration (Registry-based)
✅ Place Events/States ANYWHERE in your project
✅ Better organization
✅ Smaller build size
✅ Clear validation and errors
✅ Version control friendly
✅ Backwards compatible (no breaking changes)

---

### Migration Process (5-10 minutes)

#### Step 1: Create AudioEventRegistry

1. Right-click in Project → `Create > Audio System > Audio Event Registry`
2. Name it exactly: **"AudioEventRegistry"**
3. Place in: `Assets/Audio/Resources/AudioEventRegistry.asset`

**Important:** The registry itself should be in a Resources folder so AudioManager can auto-find it at runtime.

#### Step 2: Populate Registry from Existing Assets

1. Select the `AudioEventRegistry` asset
2. In Inspector, click **"Populate from Resources Folders"**
3. Verify: Registry should now show all your existing Events and States

Console output:
```
✓ Registry populated: 45 events, 12 states
```

#### Step 3: Test That Everything Still Works

1. Enter Play Mode
2. Check Console for:
   ```
   Loading audio assets from AudioEventRegistry
   ✓ Loaded from Registry: 45 events, 12 states
   ```
3. Test your sounds - they should work exactly as before

**No code changes needed!** AudioManager automatically detects the registry.

#### Step 4: Move Events/States Out of Resources (Optional)

**This step is optional but recommended for organization:**

1. Create new folders outside Resources:
   ```
   Assets/Audio/
   ├── Events/          ← Move Events here (outside Resources)
   ├── States/          ← Move States here (outside Resources)
   ├── Containers/      ← (unchanged)
   └── Buses/           ← (unchanged)
   ```

2. Drag Events from `Assets/Audio/Resources/Audio/Events/` → `Assets/Audio/Events/`
3. Drag States from `Assets/Audio/Resources/Audio/States/` → `Assets/Audio/States/`

4. Update Registry:
   - Select `AudioEventRegistry`
   - Click **"Populate from Entire Project"**
   - Verify all Events/States are still registered

5. Delete empty Resources folders (if desired)

#### Step 5: Validate Registry

1. Select `AudioEventRegistry`
2. Click **"Validate Registry (Remove Nulls)"**
3. Check Console:
   ```
   ✓ Registry validated: No issues found
   ```

---

### Migration Checklist

- [ ] AudioEventRegistry created
- [ ] Registry placed in Resources folder
- [ ] Registry populated from Resources
- [ ] Tested in Play Mode (sounds work)
- [ ] (Optional) Moved Events/States out of Resources
- [ ] (Optional) Updated registry from entire project
- [ ] Registry validated

---

### Backwards Compatibility

**The system is fully backwards compatible:**

| Scenario | What Happens |
|----------|--------------|
| Registry exists | AudioManager uses registry (new way) |
| No registry found | AudioManager uses Resources.LoadAll (old way) |
| Both registry and Resources | Registry takes priority |

**This means:**
- Old projects without registry continue working
- New projects can use registry from day 1
- Gradual migration is safe

---

### Adding New Sounds After Migration

#### Option 1: Quick Wizard (Recommended)
1. `Window > Audio System > Quick Sound Setup Wizard`
2. Create sound assets
3. Open `AudioEventRegistry`
4. Click **"Populate from Entire Project"**

#### Option 2: Manual
1. Create AudioEvent anywhere in project
2. Open `AudioEventRegistry`
3. Drag new Event into "Registered Events" array
4. OR click **"Populate from Entire Project"**

**Tip:** Set up a post-processing script to auto-update registry when Events are created.

---

### Common Migration Issues

#### Issue: "No AudioEvents found in Resources/Audio/Events"
**Cause:** Registry not found, falling back to Resources.LoadAll
**Fix:** Ensure registry is named "AudioEventRegistry" and placed in `Assets/Audio/Resources/AudioEventRegistry.asset`

#### Issue: "Loaded 0 events" after migration
**Cause:** Registry is empty
**Fix:** Select registry → Click "Populate from Entire Project"

#### Issue: Some sounds don't play after migration
**Cause:** Events not registered
**Fix:** Open registry → Click "Validate Registry" → Re-populate

#### Issue: Registry has null references
**Cause:** Events were deleted but registry not updated
**Fix:** Click "Validate Registry (Remove Nulls)"

---

### Performance Comparison

#### Before (Resources-based)
- `Resources.LoadAll<AudioEvent>("Audio/Events")` at startup
- All Resources assets included in build
- Load time: ~50-200ms (project dependent)

#### After (Registry-based)
- Direct reference lookup (no scanning)
- Only registered assets in build
- Load time: ~5-20ms (instant)

**Result:** Faster startup, smaller builds

---

### Team Workflow

#### Before Migration
❌ "Where do Events go again? Assets/Audio/Resources/Audio/Events?"
❌ "Why isn't my sound playing?" (placed in wrong folder)
❌ "Do I need Resources nested inside Audio folder?"

#### After Migration
✅ "Events can go anywhere, just update the registry"
✅ Inspector shows clear warnings if Event not registered
✅ Single registry asset, auto-detected

---

### Advanced: Custom Registry Locations

If you don't want registry in Resources, you can manually assign it:

```csharp
// In your bootstrap script
void Awake()
{
    var customRegistry = Resources.Load<AudioEventRegistry>("MyCustomPath/AudioEventRegistry");
    // AudioManager will find it via Resources.Load
}
```

OR create a custom singleton accessor:
```csharp
public class MyAudioConfig : MonoBehaviour
{
    [SerializeField] private AudioEventRegistry registry;

    void Awake()
    {
        // Inject registry into AudioManager
        // (requires minor AudioManager modification)
    }
}
```

---

### Rollback Plan

If migration causes issues, you can instantly rollback:

1. Delete or move `AudioEventRegistry` out of Resources
2. AudioManager will fallback to old `Resources.LoadAll` method
3. Everything works as before

**No code changes needed for rollback!**

---

### Summary

#### What Changes:
- AudioEvents/States can live anywhere (not just Resources)
- AudioEventRegistry tracks all assets
- AudioManager auto-detects registry

#### What Stays the Same:
- All existing code works unchanged
- AudioEvent.Post() API identical
- Container/Bus workflow unchanged
- Old Resources-based projects still work

#### Time Investment:
- Initial migration: **5-10 minutes**
- Per new sound after: **+10 seconds** (update registry)

#### ROI:
- Eliminates #1 confusion point for new users
- Better project organization
- Faster builds and startup

---

### Next Steps

After migration:
1. ✅ Update team documentation
2. ✅ Use Quick Sound Setup Wizard for new sounds
3. ✅ Consider removing Resources folders entirely
4. ✅ Update registry when adding/removing Events

---

*For API signatures and full type listings, see [API_REFERENCE.md](API_REFERENCE.md).*

*SFX System v2.5.0 — Professional Audio Middleware for Unity*
