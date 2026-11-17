# SFX System — User Guide

**Version:** 3.1.1
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** October 2026

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Quick Start](#2-quick-start)
3. [Cookbook](#3-cookbook)
4. [Manual](#4-manual)
5. [Migration](#5-migration)
6. [Build Compatibility](#6-build-compatibility)

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

> 🛠️ **Building for anything other than 64-bit Windows?** Read [6. Build Compatibility](#6-build-compatibility) first. Convolution reverb is 64-bit Windows only, and Web builds have no occlusion muffling or reverb.

---

## 2. Quick Start

**The Welcome window** opens the first time you open a project with Pepper.SFX in it (and once after each update). Its
**Add Audio Manager** and **Open Sound Builder (Simple)** buttons do Method 1's first two steps, and it shows the code
for the third. Open it any time from `Window > PepperSFX > Setup > Welcome`; untick its "Show this window…" box if you
don't want it to open by itself.

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
1. In Unity menu: `Window > PepperSFX > Authoring > Sound Builder (Simple)`
2. **Choose Creation Mode:**
   - **Complete Setup** - Creates Bus + Container + Event all at once (recommended)
   - **Container Only** - Just create a container
   - **Event Only** - Just create an event (with existing container or blank)
   - **Bus Only** - Just create a bus
   - **Batch Import** - Import multiple clips at once
3. Enter sound name (e.g., "ButtonClick")
4. **Select Container Type:**
   - **Routing** - Direct playback (plays all clips simultaneously if multiple added)
   - **Random** - Random selection with weights
   - **Sequence** - Sequential playback (Forward/Reverse/PingPong)
   - **Switch** - Switch-based selection
   - **Blend** - RTPC-driven real-time blending
5. Add one or more AudioClips
6. Configure advanced options if needed (pitch/volume randomization, 3D settings)
7. Click **"Create Complete Sound Setup"**

There are no presets: you set each option yourself, and the defaults suit a simple 2D sound effect.

✅ Done! The wizard creates:
- AudioBus (with custom name)
- Container (any type: Routing, Random, Sequence, Switch, or Blend)
- AudioEvent (with selectable action type)
- All references pre-filled

💡 **Pro Tips:**
- The wizard auto-detects selected audio clips and suggests batch import for multiple selections
- Use "Auto-Create Standard Folder Structure" to set up all required folders
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

**Important:** AudioManager persists between scenes automatically via `DontDestroyOnLoad`, as long as **Persist Across Scenes** is on (the default) and its GameObject is at the top of the hierarchy: Unity keeps only root objects. To keep it under another object (a scene that is never unloaded, a client-only root), turn Persist Across Scenes off; it then belongs to its own scene.

---

#### Step 2: Create Folder Structure (1 minute)

Right-click in Project window → `Create > PepperSFX > Sound Builder (Simple)` → Click **"Auto-Create Standard Folder Structure"**

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
2. Select `Create > PepperSFX > Buses > Audio Bus`
3. Name the file: "SFX_Bus"
4. Select it and configure in the Inspector:
   - **Bus Name:** "SFX"
   - **Volume Db:** 0
   - **Parent Bus:** (leave empty for now)
   - **Mixer Group:** (optional)

**What just happened?** You created a volume control group for sound effects. Later you can adjust all SFX together.

💡 **Tip:** The Quick Wizard creates this automatically in its Complete Setup mode.

---

#### Step 4: Create Your First Container (2 minutes)

1. Right-click in `Assets/Audio/Containers/`
2. Select `Create > PepperSFX > Containers > Routing Container`
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
2. Select `Create > PepperSFX > Audio Event`
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
- Check it persists: with Persist Across Scenes on (the default) it is kept across scenes, but only if its GameObject is a root (Unity warns otherwise). Under a parent, turn Persist Across Scenes off and keep it in a scene that stays loaded.

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
handle.SetVolumeScale(0.75f);   // relative to the authored volume
handle.SetPitchOffset(2f);      // semitones, on top of the authored pitch
```

**Global controls:**
```csharp
AudioManager.Instance.StopAllSounds();
AudioManager.Instance.PauseAll();
AudioManager.Instance.UnpauseAll();
AudioManager.Instance.MuteAll(true);   // mute
AudioManager.Instance.MuteAll(false);  // unmute
worldBus.Pause();                      // a game's pause: this bus and its children hold, a pause menu's buses play
worldBus.Resume();
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
1. Create empty GameObject → BoxCollider (isTrigger forced on) → Add "PepperSFX/Propagation/Audio Zone"
   → Size collider to fit the room. Repeat for each acoustic space.

2. Create empty GameObject at a doorway → BoxCollider → Add "PepperSFX/Propagation/Audio Portal"
   → Drag the two adjacent AudioZones into ZoneA and ZoneB.

3. Create empty GameObject inside the source zone → Add "PepperSFX/Propagation/Ambient Source"
   → Assign Source Zone, Looping Clip, and Ambience Mixer Group.

4. On your player object, the one with the Collider (e.g. the player root's Capsule Collider)
   → Add "PepperSFX/Propagation/Audio Listener Zone Tracker". Not on the camera that holds the
   AudioListener: trigger events only reach a GameObject that has a Collider.
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
> If *any* of those three flags is on and the slot pool has not been authored, the voice silently falls back to direct-to-bus routing. No low-pass muffling. No reverb sends. **No error.** Only the volume part works without a slot: an occluded or propagated sound still gets quieter behind walls and doors, it just keeps its treble, so the setup can sound half right.
>
> **You must author the Occlusion Slot Pool first**, before touching reverb buses, zones, or container flags. Do it here: [Occluding SFX Through Geometry → One-Time Project Setup](#recipe-one-time-project-setup-5-minutes).
>
> Exception: the `AmbientSource` propagation path (rain, wind, ambient beds) uses a separate emitter pool and does *not* require slots. Only container-level `UsePropagation` does.

### Setup & Organization

#### Recipe: First-Time Project Setup (10 minutes)

📋 **What:** Complete setup from scratch to playing first sound

##### Step 1: Create AudioManager (2 min)

1. Create empty GameObject in scene: "AudioManager"
2. Add component: `AudioManager` (Add Component → PepperSFX → Audio Manager)
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

1. Right-click `Buses/` → Create → PepperSFX → Buses → Audio Bus
2. Name: "Master_Bus"
3. Inspector:
   - Bus Name: "Master"
   - Volume Db: -3
   - Parent Bus: (none)

##### Step 4: Create SFX Bus (1 min)

1. Right-click `Buses/` → Create → PepperSFX → Buses → Audio Bus
2. Name: "SFX_Bus"
3. Inspector:
   - Bus Name: "SFX"
   - Volume Db: 0
   - Parent Bus: Master_Bus

##### Step 5: Create First Container (2 min)

1. Right-click `Containers/` → Create → PepperSFX → Containers → Routing Container
2. Name: "Test_Click_RC"
3. Inspector:
   - Container Name: "TestClick"
   - Audio Clips: Drag in a sound
   - Volume: 1.0
   - Enable Volume Randomization: ✓ (Min: -1, Max: 1)
   - Enable Pitch Randomization: ✓ (Min: -50, Max: 50)

##### Step 6: Create First Event (2 min)

1. Right-click `Assets/Resources/Audio/Events/` → Create → PepperSFX → Audio Event
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
        SetPlayerVolume("SFX", value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();
    }

    void OnMusicVolumeChanged(float value)
    {
        SetPlayerVolume("Music", value);
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save();
    }

    void OnDialogueVolumeChanged(float value)
    {
        SetPlayerVolume("Dialogue", value);
        PlayerPrefs.SetFloat(DIALOGUE_KEY, value);
        PlayerPrefs.Save();
    }

    // The player's volume on a bus (Manual §6.8): on top of the mix, never saved into the bus asset,
    // and a mix state can't undo it. Not SetBusVolume, which changes the bus's own Volume.
    void SetPlayerVolume(string busName, float linear)
    {
        var bus = AudioManager.Instance.GetBus(busName);   // an unknown name warns once
        if (bus != null)
            bus.PlayerVolumeDb = AudioSystem.AudioExtensions.LinearToDb(linear);  // qualified: Unity 6.3 has its own AudioExtensions
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

💡 **Why `PlayerVolumeDb` and not `SetBusVolume`:** `SetBusVolume` changes the bus's own Volume, the mix you authored. In the editor a Play-mode change stays in the asset, and a state that sets the bus's level overwrites the player's choice. The player's volume is a separate level on top. `Example08_AudioSettingsUI` is the full version.

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

**3. Stopping it:** keep the handle `Post` returns and call `Stop` on it (see the code below). Don't make a separate "Music_Stop" event with a Stop action: a Stop action only stops sounds its own event started, so it can't stop this music.

##### Code

```csharp
using UnityEngine;
using AudioSystem;

public class MusicManager : MonoBehaviour
{
    [SerializeField] private AudioEvent startMusicEvent;
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
            musicHandle = startMusicEvent.Post();
        }
    }

    public void StopMusic(float fadeTime = 2f)
    {
        if (musicHandle != null)
        {
            // Also cancels the loop if the intro is still playing.
            musicHandle.Stop(fadeTime);
            musicHandle = null;
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
3. Assign the start event
4. Mark as DontDestroyOnLoad if needed

✅ **Result:** Intro plays once, then the loop takes over and repeats.

💡 **How close is the join?** A delayed action waits on a frame timer, so the loop starts within about one frame after the intro ends (≈16 ms at 60 fps; more if that frame is slow). With the event's Time on Scaled (the default), slow motion or a pause also stretches the wait, so set Time = Unscaled (§5.4) for music. On a hard musical cut that small gap can be heard. For a sample-accurate join, make two events (intro and loop, each with Delay 0) and start both on the audio clock yourself:

```csharp
[SerializeField] private AudioEvent introEvent, loopEvent;
[SerializeField] private AudioClip introClip;   // the intro's clip, for its exact length
private AudioHandle introHandle, loopHandle;

public void StartMusic()
{
    double start = AudioSettings.dspTime + 0.1;                           // a little ahead, so both are scheduled in time
    double introLength = (double)introClip.samples / introClip.frequency;  // exact, in seconds
    introHandle = introEvent.Post(gameObject, transform.position, start);
    loopHandle  = loopEvent.Post(gameObject, transform.position, start + introLength);
}
// To stop, Stop both handles.
```

This needs the intro's container to have no pitch randomization (a pitch change changes its length).

💡 **Switching tracks:** keep the handle `Post` returns and crossfade from it: `musicHandle = AudioManager.Instance.CrossFade(musicHandle, nextTrackEvent, 2f);`. That also cancels the delayed loop if the intro is still playing. A container crossfade from the intro would let the loop start anyway (see §10.3).

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
2. Drag the `Heartbeat_Thump` event asset into `Audio Event`
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

**Volume swell with BPM.** The thumps should also get a bit louder as you panic. BeatScheduler doesn't own an AudioSource, so drive the volume from the same stress value through a parameter. Make a global parameter asset `Stress` (0–1), write it each frame, and give it a **Volume** curve on the thump's container (§4.1) or on `Player_SFX_Bus` (bus Parameter Curves, §6.6), for example 0 → -6 dB, 1 → 0 dB:
```csharp
[SerializeField] private AudioParameter stressParam;   // the Stress parameter asset

// In Update, after computing currentStress. A bus curve reads the global value.
AudioManager.Instance.SetParameter(stressParam, currentStress);
```
Stay at or below 0 dB: a voice can't go louder than its authored level. A curve on the container affects only the thumps; one on the bus moves everything on that bus.

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

- `BeatScheduler` component, `Audio Event` = the `Metronome_Tick` event asset
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
            fireHandle.SetVolumeScale(intensity);
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

> A state sets the **global** value of a parameter, by name. To give it a range and a default, create a Parameter asset whose Parameter Name is "Underwater": the state and the asset share one value. Underwater is the same for every sound, so a global value is right here; per-object values go on an Audio Emitter (Manual §3.4).

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
- Tick **Loop** on each of these, or set the Blend's **Loop Mode** to **Force Loop** (step 3).
- For a 3D engine, set **Doppler** to **Pepper** on the Blend (its 3D Settings; or Doppler Level 0 for none) and tick **Uses Parent Positioning** on each layer. Layers that go virtual then come back in step with the others; Unity's Doppler is the one pitch change Pepper can't follow (Manual §3.2, Voice Virtualization; §4.1 Doppler).

**2. Create a Parameter:**
- Parameter: "EngineRPM" (Create > PepperSFX > States and Parameters > Parameter)
- Unit Label: "0-1", Min: 0, Max: 1, Default: 0.2 (idle)
- Smoothing: Ease, Rise 0.3 s, Fall 0.5 s. The engine follows the car's speed gently, the same at any frame rate, so the script below doesn't smooth anything itself.

**3. Create Blend Container:**
- Blend Container: "Vehicle_Engine_BC"
- Blend Parameter: the EngineRPM asset
- Loop Mode: Force Loop
- Silent Layers: Virtualize. Layers that are silent at the current RPM go virtual and free their real voices, coming back in step when the RPM reaches them (Manual §4.6).
- 3D Settings: **Is 3D** ticked and **Doppler** Pepper (or Doppler Level 0). The layers take both through Uses Parent Positioning (step 1); without Is 3D on the Blend the engine plays 2D, in the listener's head.

> **Each car has its own RPM.** The car posts its engine on its own **Audio Emitter** and sets the value there, so two cars playing this Blend each follow their own RPM. A value set globally (`SetRTPC("EngineRPM", ...)` or `AudioManager.SetParameter`) reaches only engines whose emitter has no value of its own. See Manual §3.4.

> **Engine loops recorded at known RPMs?** Use the parameter in real RPM (Min 0, Max 8000, Unit "RPM"), set each layer's **Root Value** to the RPM it was recorded at, switch the Blend's **Layout** to **Regions**, press **Auto Layout**, and set **Pitch Follow Parameter** to the same parameter. Every layer then crossfades at the right RPM and is pitched by RPM ÷ its recording, so the layers stay in tune with each other. In the script below, write the car's real RPM instead of the 0–1 value (`targetRPM` from the engine model, `idleRPM` in RPM, say 900) and untick **Modulate Pitch**: Pitch Follow does the pitching. See Manual §4.6. The **Engine Builder** (Manual §10.8) builds this whole tree, with on-load and off-load sets, from the selected loops, and **Detect Root Values** (§10.7) measures each loop's RPM.

**Entry [0] - Idle:**
- Container: Engine_Idle_RC
- Curve: (0.0, 1.0), (0.3, 0.5), (1.0, 0.0)

**Entry [1] - Load:**
- Container: Engine_Load_RC
- Curve: (0.0, 0.0), (0.5, 1.0), (1.0, 0.3)

**Entry [2] - Redline:**
- Container: Engine_Redline_RC
- Curve: (0.0, 0.0), (0.7, 0.2), (1.0, 1.0)

**4. Create Event:**
- Event: "Start_Vehicle_Engine"
- Actions: Play Vehicle_Engine_BC → SFX_Bus
- Persistent: ticked. The engine is never stolen to make room for other sounds (Manual §5.3).

**5. On the car:** add an **Audio Emitter** component (Add Component > PepperSFX > Audio Emitter) next to the script below, and set its **On Disable** to **Stop**: when the car is destroyed or switched off, its engine fades out over **Stop Fade Seconds** where it is and its voices go back to the pool (Manual §3.4). Left at Keep, the engine's voices sit under the car and are cut with it.

##### Code

```csharp
using UnityEngine;
using AudioSystem;

[RequireComponent(typeof(Rigidbody), typeof(AudioEmitter))]
public class VehicleEngineAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioEvent engineStartEvent;
    [SerializeField] private AudioParameter engineRpm;   // the EngineRPM asset

    [Header("RPM Settings")]
    [SerializeField] private float maxSpeed = 30f;
    [SerializeField] private float idleRPM = 0.2f;

    [Header("Pitch Modulation")]
    [SerializeField] private bool modulatePitch = true;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 1.4f;

    private Rigidbody rb;
    private AudioEmitter emitter;
    private AudioHandle engineHandle;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        emitter = GetComponent<AudioEmitter>();
        StartEngine();
    }

    // Call this when the car is reset or teleported, so the engine doesn't glide across it.
    public void OnCarReset() => emitter.SnapAllParameters();

    void StartEngine()
    {
        if (engineStartEvent != null)
        {
            // Posted on this car's emitter: the engine reads this car's RPM.
            engineHandle = emitter.Post(engineStartEvent);
        }
    }

    void Update()
    {
        UpdateEngineRPM();
    }

    void UpdateEngineRPM()
    {
        // Calculate target RPM from speed
        float speed = rb.linearVelocity.magnitude;
        float targetRPM = Mathf.Clamp01(speed / maxSpeed);

        // Keep minimum idle RPM when not moving
        if (targetRPM < idleRPM)
            targetRPM = idleRPM;

        // This car's RPM, raw. The parameter's Smoothing eases what the engine hears; the Blend
        // picks it up after LateUpdate, before the frame's audio.
        emitter.SetParameter(engineRpm, targetRPM);

        // Optional: Modulate pitch by the smoothed RPM the engine is playing at
        float currentRPM = emitter.GetEffectiveParameter(engineRpm);
        if (modulatePitch && engineHandle != null)
        {
            // SetPitchOffset takes semitones. A playback-rate ratio r is 12 * log2(r) semitones:
            // 0.8 -> -3.9 st, 1.4 -> +5.8 st. It shifts on top of the layers' own pitch
            // randomization, if any, rather than replacing it.
            float pitchSemitones = Mathf.Lerp(
                12f * Mathf.Log(minPitch, 2f),
                12f * Mathf.Log(maxPitch, 2f),
                currentRPM
            );
            engineHandle.SetPitchOffset(pitchSemitones);
        }
    }

    // Engine off while the car stays (a stall, the key turned). Removing the car needs nothing here: its Audio
    // Emitter, set to On Disable = Stop, fades the engine out where it is.
    public void StopEngine()
    {
        engineHandle?.Stop(0.5f);
        engineHandle = null;
    }
}
```

✅ **Result:** Realistic engine sound that responds to speed!

> **Why no `OnDestroy`:** an earlier version posted a stop event there. A Stop action only stops its own event's sounds, so a separate stop event never reached the engine, and a `Stop(0.5f)` from `OnDestroy` can't fade voices that are destroyed with the car on the same frame. The emitter's On Disable = Stop does both jobs.

**Tuning by ear:** open **Window > PepperSFX > Authoring > Audition** with the engine Blend selected, press Play and **Sweep** the RPM (say 900 → 7000 over 4 s, back and forth) while you adjust regions and root values in the Inspector; the Layers table shows each band's level and pitch (§10.5).

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
- Priority: Medium
- Max Instances: 1
- Actions: Play Music_Nightclub_RC → Music_Bus

There's no multi-position setting on the event. Posting it with `PostMultiPosition` is what plays it from every speaker.

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
  - (Optional) Use Directionality ✓ and Cone Angle 90, then rotate to face inward: this only draws a cone in the Scene view to help you aim the speakers. It doesn't change what you hear.

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
            // (OnLoop exists but isn't raised in this version, so it's left out here.)
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

> 🧱 **This is the foundation for two other features too.** Per-Zone Reverb (`AllowReverbSend`) and container-level Ambient Propagation (`UsePropagation`) both acquire the **same Occlusion Slot** at play time. If you skip the slot-pool authoring in *One-Time Project Setup* below, none of the three features works fully — voices silently fall back to direct-to-bus routing with no low-pass and no sends, and no error message. Only occlusion's and propagation's volume drop still applies, because it doesn't need a slot. Author the layout once; opt individual containers in afterward.

> ⚠️ **Silent failures to know up front**
> - No `OcclusionLayout` authored → containers with `Use Occlusion` or `Use Propagation` only get quieter behind walls and doors (no low-pass), and `Allow Reverb Send` does nothing. No error.
> - Container's `Mixer Group` not in the layout's bus list → slot acquisition skipped silently, with the same result. Re-run the builder after adding new buses.
> - Occlusion doesn't check Is3D: a 2D voice is still tested from the listener to wherever its AudioSource sits, and muffled if that line is blocked. The flag also costs a slot — turn it off for UI/2D containers.
> - Slot pool exhausted → new voices route direct-to-bus (no low-pass, no sends; the volume drop still applies) and a console warning fires. Bump *Slots Per Bus* if you see it.

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
- Right-click in your Audio assets folder → Create → PepperSFX → Settings → Occlusion Layout
- Name it `OcclusionLayout` (one per project)

**2. Open the builder window:**
- Menu: Window → PepperSFX → Mixing → Occlusion Layout

**3. Configure:**
- **Voice Mixer**: your project's voice mixer (where SFX are routed)
- **Occlusion Layout**: drag in the asset you just created
- **Slots Per Bus**: leave at 6 (the default sizes well for most SFX-heavy scenes; bump to 8–12 for weapon-dense gameplay)
- (Optional) **Reverb Send Bus Registry**: assign if you also use per-zone reverb sends — covered in the Per-Zone Reverb recipes below

**4. Click "Scan, Auto-Create, Refresh":**
- The builder walks the project for containers with `Use Occlusion`, `Allow Reverb Send` or `Use Propagation` turned on (each of the three takes a slot at play time), and for Ambient Sources with `Use Occlusion` in the open scenes. It collects the unique buses they play through (a layer with `Uses Parent Output` counts its Blend's or Switch's bus) and authors:
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
- Cutoff target lerps from 22 kHz → about 7 kHz (6.95 kHz when every ray is blocked). Propagation (`Use Propagation`) can take it lower: the most-muffled value wins
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
- Does it get quieter behind a wall but keep its treble? Then occlusion runs but the voice has no slot: the volume drop doesn't need one, the low-pass does. Check the next three points.
- Did you assign `Voice Mixer` AND `Occlusion Layout` on AudioManager?
- Did you re-run the Occlusion Layout Builder after adding this container?
- Is the container's `Mixer Group` one of the buses in the layout? If you check the OcclusionLayout asset's `Buses` list and the container's bus isn't there, the layout is stale.
- Is the container 3D? Occlusion casts from the listener to the voice's AudioSource. It doesn't skip 2D voices, but their position rarely means anything, so use it on 3D sounds.

**"Console warns 'Occlusion slot pool exhausted on bus X'."**
- More concurrent occluded voices on that bus than slots configured. Bump `Slots Per Bus` on the Occlusion Layout asset, re-run Generate, and verify the warning stops.

**"Console warns '... on bus X, which has no slots in the Occlusion Layout'."**
- A container with `Use Occlusion`, `Allow Reverb Send` or `Use Propagation` played through a bus the layout has no slots for, so its sounds there get no low-pass and no reverb send. The layout was made before that bus needed slots. Click **Scan, Auto-Create, Refresh** in the Occlusion Layout window again. The warning comes once per bus, and only when an Occlusion Layout is assigned.

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
> - A bus's character lives on its `ReverbSendBus` asset, **not** the zone. A new bus's `Room` is `0` mB (millibels; full level), so it's audible once generated. A bus whose `Room` has been turned down to `-10000` mB is silent until you raise it. Tagging a zone and pointing its `Reverb Bus` at that asset routes voices there, but the *sound* comes from the asset.
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
- Project ▸ Create ▸ PepperSFX ▸ Buses ▸ Reverb Send Bus
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
- Open Window ▸ PepperSFX ▸ Mixing ▸ Reverb Send Buses
- If no registry exists yet, create one: Project ▸ Create ▸ PepperSFX ▸ Settings ▸ Reverb Send Bus Registry
- Drop the registry into AudioManager ▸ Reverb Send Bus Registry
- Click Refresh in the window — your new SendBus appears in the list

**5. Sanity-check:**
- Click Validate Wiring on the SendBus asset. It should say "Wiring OK."

✅ **Result:** The reverb bus exists on the mixer, exposes the right runtime parameters, and is registered. Voices won't send to it yet — that requires tagging an `AudioZone` (next recipe).

---

#### Recipe: Tag a Zone to Use This Bus

📋 **What:** Tell an `AudioZone` "voices in here send to this reverb bus."

🎯 **Use Case:** After authoring a SendBus, every zone that should produce that bus's reverb needs to be tagged.

##### Asset Setup

**1. Select the AudioZone GameObject in your scene.**

**2. In the AudioZone component:**
- **Reverb Bus**: drag in the `Reverb_Bathroom` SendBus's `ReverbBus` field (open the SendBus inspector and drag from there — it's the mixer group reference). This is the only reverb setting on the zone.

The zone has no reverb character of its own. How the room sounds (Room, Room HF, Decay Time, Decay HF Ratio, Reflections, Reverb Level) is set on the `Reverb_Bathroom` SendBus asset from the previous recipe. To make two rooms sound different, give them separate SendBus assets.

##### What Happens at Runtime

- **Source-side**: voices in this zone whose container has `Allow Reverb Send = ✓` send a copy of their signal to `Reverb_Bathroom` at their authored level. The send level scales by the listener's audibility to the zone (1.0 same-zone, <1 through portals).
- **Listener-side**: the bus's six runtime-driven parameters are written from the SendBus asset every reverb-sends tick. They don't change with where the listener stands; only the send level does.

✅ **Result:** The bathroom zone is now acoustically alive. Voices in the bathroom contribute to the bathroom bus, which sounds the way its SendBus asset is authored.

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
- Confirm the bus's `ReverbSendBus` asset isn't silent: a parametric bus with `Room` at -10000 mB produces no tail — raise it toward 0 (the default). A convolution bus with no `Impulse Response` assigned is likewise silent.
- Confirm the listener can reach the source's zone (unreachable zones produce audibility = 0).
- Confirm the container's `Mixer Group` is a bus that appears in the Occlusion Layout's bus list — without a slot, there are no Send level params to write to.

**"Several zones share one bus and they all sound identical."**
- This is **by design** as of the source-of-truth unification: a bus has one acoustic identity, defined by its `ReverbSendBus` asset, regardless of which zone routed the voice to it. Per-zone variation on a *shared* bus is no longer supported.
- If you need Cathedral and Cathedral_Crypt to sound different, give them **separate** `ReverbSendBus` assets and point each zone's `ReverbBus` at its own.

**"I added a new ReverbSendBus but it doesn't appear in the dropdowns."**
- Open Window ▸ PepperSFX ▸ Mixing ▸ Reverb Send Buses and click Refresh. The registry only updates when refreshed.
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
> - No `Audio Listener Zone Tracker` on the player object (the one with the Collider), or it's on the camera that holds the AudioListener instead → listener's zone is always null, all propagation is silent.
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

**Scene prerequisites** (one-time): `AudioManager` + `AudioListener` in the scene; an `Ambience` mixer group (or any group of your choice); the player object (the one with the Collider, not the camera) has the `Audio Listener Zone Tracker` component. Recipes below assume these are in place.

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
- Is the `Audio Listener Zone Tracker` on your player object, the one with the Collider? On the camera that holds the AudioListener it gets no trigger events, so the listener's zone is always null.
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
h.SetVolumeScale(0.5f);
h.SetPitchOffset(2f);
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
mh.SetVolumeScale(0.8f);
mh.SetVoiceVolumeScale(0, 0.5f);  // Per-emitter control
mh.Stop(2f);
```


---

## 4. Manual

The Manual is the system's deep reference — architecture, design rationale, and chapter-by-chapter coverage of every subsystem. Read it once front-to-back to build a mental model, or jump straight to the chapter you need.

#### Manual Chapters

1. [Introduction](#1-introduction) — what this manual covers, prerequisites, how to read it
2. [System Architecture](#2-system-architecture) — design philosophy, the audio pipeline, component hierarchy
3. [Core Components](#3-core-components) — AudioManager, voice pool, GainStack, scheduling, parameters and scopes, emitters (offsets, moving sounds, teleports)
4. [Audio Containers](#4-audio-containers) — Routing / Random / Sequence / Switch / Blend, and engines generated live (Unity 6.3+)
5. [Events System](#5-events-system) — AudioEvent, EventAction, posting (timed posts, length estimates) and lifecycle
6. [Bus & Mixing](#6-bus--mixing) — bus hierarchy, ducking (the deepest wins), effect sends, pausing a bus, the player's volume
7. [State System](#7-state-system) — states, state groups, transitions, offset states for mixes
8. [Multi-Position Audio](#8-multi-position-audio) — synchronized playback across multiple positions
9. [Performance & Optimization](#9-performance--optimization) — voice budgets, virtualization, LOD
10. [Advanced Features](#10-advanced-features) — occlusion, ducking, crossfading, beat scheduling, and the sound tools: Audition, Live View and recording, Loop Analysis, Engine Builder, the validator, Engine Grains
11. [Best Practices](#11-best-practices) — recommended patterns for production projects
12. [Troubleshooting](#12-troubleshooting) — common failure modes and diagnosis
13. [Ambient Propagation Subsystem](#13-ambient-propagation-subsystem) — zone/portal graph routing for long-running beds
14. [Occlusion Mixer Slot Pool](#14-occlusion-mixer-slot-pool) — per-bus slot pools, multi-ray occlusion, propagation composition
15. [Reverb Send Buses & Per-Zone Reverb](#15-reverb-send-buses--per-zone-reverb) — source-side routing and listener-side bus-character driver
16. [Audio Loading & Memory](#16-audio-loading--memory) — import policy, clip warming, and the optional Addressables backend
17. [Multiplayer (Netcode)](#17-multiplayer-netcode) — backend-agnostic networked audio (NGO / FishNet); dedicated servers (Inert)

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
- ListenerUtil (the listener's position: the registered listening point, or a scene AudioListener)

#### 2.4 Memory & Lifecycle

**Startup (Scene Load):**
1. AudioManager.Awake()
   - Singleton registration (a second manager removes itself: its whole GameObject with Persist Across Scenes on, only
     its component with it off)
   - DontDestroyOnLoad, with Persist Across Scenes on (the default; the manager must then be a root object)
   - Voice pool creation: Max Real + Max Virtual + Steal Headroom AudioSources (32 + 64 + 4 by default), under
     "AudioVoices", a root object of Pepper's own
   - Bus initialization
   - Load events and states from the Audio Event Registry, else from Resources/Audio/Events and Resources/Audio/States
   - Start coroutines (voice management, occlusion), on "AudioVoices"

**The manager's own GameObject switched off** (or a parent of it): nothing changes. The voices and every coroutine
(fades, delays, ducks, the voice pass) live on "AudioVoices", so they carry on; Unity would otherwise end the coroutines
and stop the voices for good. To silence the game, use `PauseAll`, pause buses (§6.7) or `StopAllSounds`.

**A dedicated server:** set `AudioManager.Inert = true` at boot, before any AudioManager starts. Then every post returns
one shared, already-finished handle without allocating or logging, with an AudioManager or without one, and an
AudioManager that starts builds no voices, coroutines or per-frame step. See §17.11.

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
   - Load events from the Audio Event Registry (falls back to Resources when there is none)
   - Dispatch event actions
   - Instance limiting
   - Cooldown tracking

4. **State Management**
   - Load states the same way (registry first, then Resources)
   - Track active state per group
   - Apply state transitions

5. **RTPC System**
   - Store parameter values
   - Notify listeners on changes
   - Drive blend containers

6. **Performance Systems**
   - Voice virtualization (only when more than Max Real Voices are live)
   - Voice stealing
   - Occlusion raycasting

##### Singleton Pattern

```csharp
public partial class AudioManager : MonoBehaviour
{
    // Set in Awake, cleared in OnDestroy. There is no lazy lookup and no auto-creation:
    // if no AudioManager is in a loaded scene, Instance is null.
    public static AudioManager Instance { get; private set; }

    private void Awake()
    {
        // A second manager: its whole GameObject with Persist Across Scenes on, only this component with it off.
        if (Instance != null && Instance != this) { RemoveDuplicate(); return; }
        Instance = this;
        if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        if (Inert) { /* a server: load the event registry, build nothing else */ return; }

        InitializeCore();   // the voice pool, under Pepper's own "AudioVoices" root
        // ...
    }
}
```

**Key Points:**
- Only one AudioManager exists; a second one destroys itself
- Persists between scene loads
- Accessible globally via `AudioManager.Instance`, once its `Awake` has run. Put it in your first (bootstrap) scene.

##### Holding a reference instead of `Instance`

Your game never needs `AudioManager.Instance`. Everything a game calls is on the manager itself or on an asset (events, buses, parameters, emitters), so you can hand the manager to the scripts that use it, the way you'd wire any other dependency:

```csharp
public class CarAudio : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;   // assigned in the inspector, or by your bootstrap
    [SerializeField] private AudioEvent engine;
    [SerializeField] private AudioParameter rpm;
    [SerializeField] private AudioEmitter emitter;
    [SerializeField] private Transform cameraTransform;

    public float CurrentRpm { get; set; }                  // your car code writes this

    private void Start()
    {
        audioManager.AddListeningPoint(cameraTransform);   // instead of ListenerUtil, or a scene search
        audioManager.Post(engine, emitter);                // instead of AudioManager.Instance.Post
    }

    private void Update() => emitter.SetParameter(rpm, CurrentRpm);
}
```

The samples in this guide write `AudioManager.Instance.X(...)` to stay short; every one of them works the same as `audioManager.X(...)` on a reference you hold. Pepper itself still uses its one manager internally (containers and events reach it), which is why there can only be one. Register a listening point (§9.3, Listening Points) and neither Pepper nor its ambient system (§13) ever searches the scene or uses `Camera.main`. To leave the multiplayer code out of a game that doesn't use it, see §17.2.

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
| `enableLOD` | Virtualize the least important voices once more than `maxRealVoices` are live | true | Always leave enabled |
| `lodDistances` | Not used in this version (hidden in the inspector) | [10, 25, 50, 100] | — |

##### Public API Overview

**Events:**
```csharp
AudioHandle PostEvent(string eventName)
AudioHandle PostEvent(string eventName, GameObject parent, Vector3 position)
```

**Parameters** (global scope; per-emitter and per-play values are in §3.4):
```csharp
void SetParameter(AudioParameter parameter, float value)
float GetParameter(AudioParameter parameter)
void SetRTPC(string parameter, float value)     // the same global values, by name
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
    public float VirtualTime;                // Where a virtual voice is in its clip, in seconds
    public double VirtualSamples;            // The same, in the clip's samples
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
    public float RtpcGain = 1f;       // Blend curves. RESERVED: sole writer is the Blend's
                                      // per-frame step (the product of every enclosing Blend's
                                      // layer curve). No generic parameter->volume driver writes it.
    public float CurveGain = 1f;      // Volume curves (Parameter Curves, §4.1) on the containers
                                      // above the voice, summed in dB. Sole writer: the same step.
    public float SchedulerGain = 1f;  // Crossfade/transition gain
    public float FadeGain = 1f;       // Stop(fadeTime) ramp, 1 → 0. RESERVED: sole writer is the
                                      // voice's fade-out — see note below.
    public float MultiplierGain = 1f; // Per-voice multiplicative — volume randomization,
                                      // Switch/Blend container outer Volume, PlayWithVolume scale.
    public float UserGain = 1f;       // AudioHandle.SetVolumeScale — idempotent runtime scale.
    public float SwitchGain = 1f;     // A playing Switch's crossfade between its choices (§4.5, When Switch Changes).
    public float EmitterGain = 1f;    // The emitter's and the handle's volume offsets (dB), kept on later voices.
    public float SpatialGain = 1f;    // Spatial Mode = Pepper: the level from the listening points; 1 in Unity mode.
    public float RealizeGain = 1f;    // The short fade around a voice going virtual and coming back.
    public float DuckingGain = 1f;    // (Deprecated) ducking is applied at the bus, not the voice.
                                      // Settable for back-compat but NOT in GetFinalGain.

    public float GetFinalGain() => GetGainWithoutRealize() * RealizeGain;

    // How loud the voice is meant to be, real or virtual: what importance and stealing read.
    public float GetGainWithoutRealize()
    {
        return BaseGain * BusGain * OcclusionGain * RtpcGain * CurveGain * SchedulerGain * FadeGain * MultiplierGain * UserGain *
               SwitchGain * EmitterGain * SpatialGain;
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
- RtpcGain is **reserved for Blend curves**: each frame, a Blend layer's voice gets the product of the curves of every Blend it plays under (§4.6). It is the slot's only writer — there is no generic parameter→volume driver, and a future one must not write this slot for blend voices (it would fight the blend frame-by-frame). See the reservation note on `GainStack.RtpcGain` in `GainStack.cs`.
- Volume curves (Parameter Curves) set CurveGain, separately from the Blend crossfade in RtpcGain
- Crossfade/scheduler sets SchedulerGain
- `Stop(fadeTime)` ramps FadeGain from 1 to 0. Because the fade is a slot like any other, a recompute mid-fade (the occlusion LateUpdate, a bus or master volume change, a duck) keeps the ramp instead of snapping the voice back to full level. It is separate from SchedulerGain so a crossfade and a stop fade on the same voice don't fight.
- Volume randomization, SwitchContainer/BlendContainer outer Volume, and `PlayWithVolume` write MultiplierGain
- `AudioHandle.SetVolumeScale` writes UserGain
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

##### The PitchStack System

**Concept:** Pitch works like the gain stack. Each thing that changes a voice's playback rate owns one slot, the slots multiply, and only `AudioVoice.UpdateFinalPitch()` writes `AudioSource.pitch`. A handle's pitch offset therefore no longer wipes out the container's pitch randomization.

| Slot | Written by |
|---|---|
| `Randomization` | Container pitch randomization, when the voice starts |
| `Decorrelation` | The Decorrelated multi-position mode's per-emitter nudge |
| `User` | `AudioHandle.SetPitchOffset` / `AudioMultiHandle.SetPitchOffset` and `SetVoicePitchOffset` |
| `Emitter` | The Audio Emitter's **Pitch Offset** (§3.4) |
| `Follow` | A Blend's **Pitch Follow** (§4.6): the parameter ÷ the layer's Root Value, every frame |
| `Curve` | **Pitch curves** (§4.1 Parameter Curves) on the containers above the voice, every frame |
| `Doppler` | Pepper's **Doppler** (§4.1, a container set to Doppler: Pepper), every frame |
| `TimeScale` | Reserved for slow motion (planned) |

Every slot is a ratio: 1 = unchanged, 2 = an octave up. The result is clamped to Unity's maximum of 3, and the slots other than Doppler to the container's **Pitch Limit** when one is set. Doppler comes after the limit, which is for the authored pitch; the container's **Doppler Max Cents** clamps it instead.

`SetPitchAbsolute` (and the obsolete `SetPitch`, which always did this) sets an override that ignores every slot, so the voice plays at exactly that pitch, randomization (and Doppler) included.

**Example:**
```
Randomization: +1 st  (container randomization)
User:          +2 st  (handle.SetPitchOffset(2f))

Final pitch = 2^(1/12) × 2^(2/12) = 2^(3/12): 3 semitones up
```

Don't write `voice.source.pitch` yourself: the next pitch-stack update overwrites it. Use the handle methods, or a slot.

##### Voice Priority System

**Enum Values:**
```csharp
public enum VoicePriority
{
    Low = 0,       // Background ambience, least important
    Medium = 64,   // Standard gameplay sounds
    High = 128,    // Important feedback (damage, pickups)
    Critical = 255 // UI, dialogue: the strongest weight
}
```

**What a priority weighs.** A sound's importance is how loud it is where it is heard (distance × volume) times its priority's weight:

| Priority | Weight |
|---|---|
| Low | 1/8 |
| Medium | 1/4 |
| High | 1/2 |
| Critical | 1 |

Each step doubles, which is worth about 6 dB of loudness: a Medium sound counts as much as a Low one twice as loud. Low used to weigh 0, so a Low sound right beside you went before a Medium one you could hardly hear; since 3.1 it weighs 1/8, and the other three are as before.

**Voice Stealing:**
A steal ends a whole **play**, never one voice of it. A play is everything one start of a container produced: all of a Blend's layers, all of a Routing container's clips, a footstep's one voice. So an engine never loses a layer mid-race; it plays on whole or ends whole.

Before an event plays, it makes room for every voice the play will need (`AudioContainer.EstimateVoiceCount()`: a Blend adds up its layers, a Switch takes its largest choice, a Routing container counts its clips). When the pool (Max Real + Max Virtual) is too full, it ends whole plays until there is room:
1. A play that is entirely virtual and silent first. Ending it frees its voices at once.
2. Otherwise the least important play. A play's importance is its loudest voice's (see the weights above).

If even that can't make enough room (too many sounds started in the same frame, or only persistent sounds are left), **nothing is stolen** and the new sound is skipped, with a warning. A sound from an event marked **Persistent** (§5.3) is never stolen.

**A stolen sound fades out.** A stolen sound that can be heard fades out over the Audio Manager's **Steal Fade Seconds** (0.025 by default, and never fewer than three frames: 50 ms at 60 fps), then its voice goes back to the pool. The same fade ends a play an event's Max Instances takes (§5.3) and a Switch set to Restart's old choice. Its handle hears at once that the sound is gone (`OnFinished`). A sound that can't be heard yet (one posted this frame, which starts a buffer ahead) ends at once and frees its voice the same frame. Steal Fade Seconds 0 brings back the old cut, a few ms ahead on the audio clock (Steal Declick Seconds), which can click. While it fades, a stolen sound keeps its AudioSource for three or four frames; the **Steal Headroom** covers that. A handle's `Stop(0)` is still a cut: you asked for it.

`VoiceStealBehavior` (Oldest, Quietest, Furthest, LowestPriority) is not used here. It decides which of an event's own plays to end when that event reaches its Max Instances (§5.3).

**Critical Voices:**
Critical is the strongest priority: it weighs most in a voice's importance, so a Critical sound is among the last to go virtual or be stolen. It can still be stolen when the pool has nothing less important left; "never stolen" would let the pool jam when many sounds are Critical. For a sound that must never drop out (an engine), tick **Persistent** on its event (§5.3): that is the one exemption.

##### Voice Virtualization

**What Is It?**
A virtual voice is still tracked, and keeps its place in the clip, but isn't heard: its AudioSource is paused, so it doesn't use one of Unity's real voices.

**When Does It Happen?**
Only when more than `maxRealVoices` voices would be heard (and `enableLOD` is on), or more than a bus's own **Max Real Voices** (§6.9). It happens a whole **play** at a time: a play's audible voices are all real, or all virtual. Importance is distance × volume × the priority's weight, taken from a play's loudest voice, so far, quiet, low-priority sounds go before near, loud, important ones (for a sound with an Attenuation asset, "distance" is how loud its curve says it is there, §4.1; with Spatial Mode = Pepper, how loud it is heard from the listening points, §9.3). Nothing is virtualized for distance alone while you are under `maxRealVoices`, except a sound past its Attenuation asset's Max Distance.
- **When a sound starts**, once all its voices are set up, the least important plays go virtual until the audible count fits, the new sound included. So a Low sound posted while the real voices are full can start virtual instead of pushing out a more important one.
- **About 10 times a second** (`voiceUpdateInterval`), every play is ranked by importance and given real voices while they fit; the rest are virtual. A real play counts as 1.25 times as important, and a play stays at least half a second in a state, so a sound that only matters slightly more doesn't swap in and out. That is also what brings an important virtual sound back while the real voices are all taken.
- A single play with more audible voices than `maxRealVoices` keeps its most important ones real.
- A Blend with **Silent Layers = Virtualize** (§4.6) also makes its near-silent layers virtual while the rest of its play stays real.

**Keeping its place.**
While a voice is virtual, Pepper keeps the position it would have reached had it kept playing, in the clip's samples (`AudioVoice.VirtualSamples`; `VirtualTime` is the same in seconds). Every frame, in Pepper's per-frame step, the position moves on by the time Unity's audio clock has advanced × the voice's pitch, and wraps for a loop. That clock counts the audio Unity has actually mixed, so it is exactly what real voices move by: it keeps running through a frame-rate hitch and stops when audio stops. So a voice at pitch 1.5 virtual for 2 s moves on 3 s of clip. When it becomes real it starts from that position, within one audio buffer of a twin that stayed real.

- A paused voice (`Pause`, `PauseAll`) holds its position, so a pause menu can't make a virtual one-shot "finish" while the game is paused.
- A virtual one-shot ends when its position passes the end of its clip.
- A start still scheduled ahead doesn't advance until that time.
- `AudioHandle.SeekTo` on a virtual voice moves the position it comes back at.

**No clicks.**
Unity's Pause and UnPause are hard cuts. So a voice that can be heard fades out over about 25 ms (in at least three steps, at most one per audio buffer) before it pauses, and a voice that becomes real fades back in the same way. While it fades out, Unity is still playing it, so for those few frames one more voice than `maxRealVoices` can be heard. A voice that is already silent, paused or not yet started pauses at once. The fade has its own gain slot, `GainStack.RealizeGain`, which isn't counted in a voice's importance, so a virtual voice competes at the level it would play at.

**Unity's Doppler.**
Unity's own Doppler effect changes a moving 3D voice's speed without Pepper seeing it, so a virtual voice can't follow it. For looping layers that must stay in step with each other (engine layers), set the container's **Doppler** to **Pepper** (§4.1): Pepper's Doppler is in the pitch a virtual voice keeps its place at. Or set **Doppler Level** to 0 for no Doppler at all (in its 3D Settings; a layer with Uses Parent Positioning takes its Blend's). The default, Unity at Doppler Level 1, is Unity's.

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
    public event System.Action OnLoop;      // declared, but not raised in this version
    public event System.Action OnStopping;  // a fade-out has begun
    public event System.Action OnFinished;

    // Methods
    public void SetVolumeScale(float scale)       // relative to the authored volume
    public void SetVolumeOffset(float db)         // the same, in dB
    public void SetVolumeAbsolute(float linear)   // replaces the authored volume
    public void SetPitchOffset(float semitones)   // on top of the authored pitch (randomization kept)
    public void SetPitchAbsolute(float semitones) // replaces the authored pitch
    public void SetPitch(float semitones)         // obsolete: same as SetPitchAbsolute
    public void SetLocalOffset(Vector3 offset)    // move it on the object it was posted on (§3.4)
    public void SetWorldPosition(Vector3 position)
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
handle.SetVolumeScale(0.5f);  // Half the authored volume
handle.SetVolumeOffset(-3f);  // Or 3 dB down (the same slot: the last call wins)
handle.SetPitchOffset(2f); // 2 semitones up, on top of any pitch randomization
handle.Stop(1f);         // Fade out over 1 second
```

**The offsets stick.** `SetVolumeScale`, `SetVolumeOffset` and `SetPitchOffset` reach the voices playing now and the ones the sound starts later: a Switch's new choice, a Sequence's next clip, a Play action still waiting out its Delay. A post can set them before the first sample (`PostValues.WithVolumeOffset`, `WithPitchOffset`, §3.4). The absolute setters (`SetVolumeAbsolute`, `SetPitchAbsolute`) still reach only the voices playing now. (Before 3.1.0-dev.7 the offsets didn't reach later voices either.)

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
    public void SetVolumeScale(float scale)       // relative to the authored volume
    public void SetVolumeAbsolute(float linear)   // replaces the authored volume
    public void SetPitchOffset(float semitones)   // on top of the authored pitch
    public void SetPitchAbsolute(float semitones) // replaces the authored pitch (SetPitch is obsolete)
    public void Stop(float fadeTime = 0.1f)
    public void Pause()
    public void Resume()

    // Per-emitter control (index = the position's index in the post's array / emitter list)
    public void SetVoiceVolumeScale(int emitterIndex, float scale)
    public void SetVoicePitchOffset(int emitterIndex, float semitones)
    public void SetVoicePitchAbsolute(int emitterIndex, float semitones)   // SetVoicePitch is obsolete
    public void StopVoice(int emitterIndex, float fadeTime = 0.1f)
    public void PauseVoice(int emitterIndex)
    public void ResumeVoice(int emitterIndex)

    // Position management (one entry per emitter)
    public void UpdatePositions(Vector3[] newPositions)
    public void UpdatePositions(Transform[] newTransforms)
    public void RefreshPositions()
    public Vector3? GetVoicePosition(int emitterIndex)
    public Vector3[] GetAllPositions()      // one entry per voice
}
```

The per-emitter methods act on **every voice that emitter started**. If the event's container starts several voices per play (a multi-clip Routing container, a Blend), all of an emitter's layers are controlled together. The index is stable: it does not shift when other emitters' voices end. `VoiceCount` counts voices, so it equals emitters × voices per play.

**Usage:**
```csharp
AudioMultiHandle multiHandle = myEvent.PostMultiPosition(emitterParent);
Debug.Log($"Playing {multiHandle.VoiceCount} voices");
multiHandle.SetVolumeScale(0.75f);          // All speakers to 75% of authored volume
multiHandle.SetVoiceVolumeScale(0, 0.5f);   // First speaker to 50%
multiHandle.UpdatePositions(newTransforms); // Update positions dynamically
```

#### 3.4 Parameters & Scopes

A **parameter** is a number the game sets and sounds read: an engine's RPM, a car's speed, combat intensity. Blend Containers follow one (§4.6).

##### Parameter assets

**Menu:** Create > PepperSFX > States and Parameters > Parameter

| Field | Meaning |
|---|---|
| Parameter Name | The name it answers to in the string API (`SetRTPC`, `GetRTPC`, state and event RTPC actions) and in Blends that name a parameter. Empty = the asset's name. |
| Unit Label | Shown next to the values: RPM, m/s, 0-1. Display only. |
| Min, Max | The range. A value outside it is **read** clamped; what was written is kept, so widening the range while tuning brings it back. Writing outside the range logs one warning per parameter. |
| Default | What sounds read when nothing has been set. |
| Smoothing | How fast sounds follow a new value: None, Rate or Ease (below). |
| Bypass While | Optional on/off parameter that turns the smoothing off while it reads 0.5 or more. |

A name and an asset with that Parameter Name are **one parameter**: `SetRTPC("EngineRPM", v)` and `SetParameter(engineRpmAsset, v)` write the same global value.

##### Three scopes

A value can be set in three places. A sound reads the first one that has a value:

1. **Play**: one sound only, set on its handle. For values that differ between sounds on one object, such as each axle's tyre slide.
2. **Emitter**: every sound posted on one **Audio Emitter** (one car, one machine).
3. **Global**: every sound with no play or emitter value. This is what `SetRTPC` has always set.
4. Otherwise, the parameter's **Default** (0 for a name with no asset).

```csharp
// Global: every sound without a closer value.
AudioManager.Instance.SetParameter(intensity, 0.7f);
AudioManager.Instance.SetRTPC("CombatIntensity", 0.7f);   // the same thing, by name

// Emitter: every sound posted on this car.
AudioEmitter car = GetComponent<AudioEmitter>();
AudioHandle engine = car.Post(engineEvent);                // or engineEvent.Post(car)
car.SetParameter(rpm, 4200f);

// Play: this sound only, here at the car's front axle (a named point, below).
AudioHandle frontAxle = car.Post(tyreEvent, "FrontAxle");
frontAxle.SetParameter(slideSpeed, 3.5f);

// Fall back to the next scope.
car.ClearParameter(rpm);

// What a sound actually reads, after clamping.
float value = engine.GetEffectiveParameter(rpm);
```

**Audio Emitter** (Add Component > PepperSFX > Audio Emitter) holds the emitter scope. Posting on it places and attaches 3D sounds exactly as posting with its GameObject does. A post on a plain GameObject has no emitter values, as before. In Play mode its inspector lists the values set on it.

Switch values use the same three scopes: `car.SetSwitch(surface, "Kerb")` for one car, `handle.SetSwitch(...)` for one sound (§4.5).

##### Values at post

A value set on a handle after `Post` reaches a sound that has already started. A one-shot that must start right (a heavy impact at a heavy impact's level, a footstep on this foot's surface) carries its values **with the post**: they go on the sound's own scope before any of its voices start, so the first sample hears them.

```csharp
car.Post(impactEvent, PostValues.Of(strength, 0.8f));
car.Post(footstepEvent, "RearAxle", PostValues.Of(surface, "Grass").With(strength, 0.3f));
impactEvent.Post(debrisObject, debrisObject.transform.position, PostValues.Of(strength, 1f));
```

- Up to 4 parameters and 2 switch values per post; `PostValues` is a struct, so building one allocates nothing.
- They are the sound's own values (the play scope, like `handle.SetParameter`), first in line before its emitter's and the global ones, and the handle keeps them for later changes.
- A post can also carry the sound's own **volume and pitch offsets**: `PostValues.VolumeOffset(-3f)`, or `.WithVolumeOffset(dB)` and `.WithPitchOffset(semitones)` on any values. They are the handle's `SetVolumeOffset` and `SetPitchOffset`, set before the first sample: `car.Post(tyreEvent, "RearAxle", PostValues.Of(surface, "Asphalt").WithVolumeOffset(profile.TyresDb))`.

##### Built-in values: Distance, Approach Speed, Angle

A Parameter asset's **Value From** can be a value Pepper works out for each sound instead of one your code sets:

| Value From | Unit | What it is |
|---|---|---|
| **Distance** | m | From the sound to the listening point. |
| **Approach Speed** | m/s | How fast the sound and the listening point close in on each other: positive coming closer, negative moving apart. From the emitter's velocity (measured, or `SetVelocity`), else the sound's own measured motion, minus the listening point's; eased over 50 ms. |
| **Angle** | ° | Around the listening point, on the horizontal plane: 0 ahead, +90 right, -90 left, ±180 behind. |

```csharp
// A pass-by boost: a Parameter asset "ApproachSpeed" (Value From = Approach Speed, -80 to 80 m/s), and on the
// car's engine a volume curve on it rising to +3 dB at 50 m/s.
// A near/far engine: a Blend on a "Distance" asset (0-300 m) crossfading a close and a distant recording.
```

- Use them wherever a parameter goes: Parameter Curves, a Blend's parameter, Pitch Follow, a Switch Group's Driven By (a near/far switch).
- Each sound has its own value, from where its first voice is (before it has one, where it was posted). The asset's range clamps it; its Smoothing doesn't apply.
- Your code can't set them: `SetParameter` on one is ignored, with one warning. `handle.GetEffectiveParameter(distance)` reads a sound's; `emitter.GetEffectiveParameter(distance)` the emitter's own place.

##### Named points

One object often has several sound sources: a car's exhaust at the tailpipe, its intake at the front, a tyre emitter per axle. Give the Audio Emitter **Points**: each a name and an offset in metres in the object's own space (x right, y up, z forward; the object's scale applies). Drag them into place in the Scene view while the object is selected.

```csharp
car.Post(engineExhaustEvent, "Exhaust");     // attached at the exhaust's offset
car.Post(intakeEvent, "Intake");
car.Post(tyreRollEvent, "RearAxle");
car.SetParameter(rpm, 4200f);                // one RPM, read by every sound on the car
```

- A sound posted at a point is attached to the object at that offset, so it moves and turns with it, a Sequence's later clips too. A 2D sound ignores the place.
- Every point shares the emitter's parameter values (and its velocity, below). Values that differ per point, such as each axle's slide speed, go on that sound's handle.
- An unknown name warns once and plays at the object's origin.
- By number is a little faster for sounds posted very often: `int rear = car.FindPoint("RearAxle"); car.Post(skidEvent, rear);`
- `car.SetPoint("Exhaust", offset)` adds or moves a point from code; sounds already playing stay where they were attached.

##### Posting at an offset, and moving a sound

An impact happens where the car hit, not at a named point. Post it at any offset in the object's own space (metres): it is like a point, with the emitter's values, its **Max Instances Per Emitter** and **Cooldown Per Emitter**, and its random numbers.

```csharp
Vector3 local = car.transform.InverseTransformPoint(contact.point);
car.Post(impactEvent, local, PostValues.Of(strength, impactSpeed / 25f));   // or impactEvent.PostAtOffset(car, local, ...)

// A scrape loop that follows the contact along a wall: move it every frame it lasts.
scrape = car.Post(scrapeEvent, local);
// ... each frame:
scrape.SetLocalOffset(car.transform.InverseTransformPoint(contact.point));
```

- `handle.SetLocalOffset(offset)` moves a playing sound to that offset from the object it was posted on (an emitter, or a GameObject); it keeps moving and turning with the object. It takes effect at once, whether the sound is attached (On Disable = Keep) or follows the object (Stop, Pause).
- `handle.SetWorldPosition(position)` puts it at a world position. On a sound posted on an object, that becomes an offset from the object, so it moves with it from there; a sound posted at a world position (no object) just moves there.
- Voices the sound starts later (a Sequence's next clip, a Switch's new choice, a Play action waiting out its Delay) start where it was moved to.
- `SetPoint` still doesn't move sounds already playing at a named point; move them with their handle.

##### Level and pitch per emitter

An Audio Emitter's **Volume Offset** (dB) and **Pitch Offset** (cents) apply to every sound posted on it, the ones playing now and the ones posted or started later: a car's level against the others. They compose with a sound's own offset (`SetVolumeOffset`, `SetPitchOffset`, or at the post).

```csharp
car.VolumeOffsetDb = profile.CarDb;              // this car 2 dB up in the mix
car.Post(engineEvent, PostValues.VolumeOffset(profile.EngineDb));   // and its engine against its tyres
```

- They have gain and pitch slots of their own (`GainStack.EmitterGain`, `PitchStack.Emitter`), set in the inspector or from code, also in Play mode.
- **A raise can't take a sound past full volume:** an AudioSource's volume stops at 1. Author car sounds a few dB below full to leave room for trims up.

**Velocity.** The emitter measures its velocity in Pepper's per-frame step (`car.Velocity`), or you give it with `car.SetVelocity(rigidbody.linearVelocity)` every frame, which is exact for a physics car. Approach Speed (above) and Pepper's Doppler (§4.1) read it.

**Teleports.** When the object jumps (a reset to the track, a respawn, a remote car placed), call `car.Teleport()`: Pepper's next step takes the new position and measures nothing from the jump, so it isn't read as a huge speed (a Doppler shriek). The last velocity stays until a new one is measured, so a car moving on at the same speed has no step in its pitch. A velocity given with `SetVelocity` is kept. Call `SnapAllParameters()` too if its values jump.

##### Switched off or destroyed (On Disable)

What an emitter's sounds do when its GameObject is switched off or destroyed is its **On Disable** setting (`WhenDisabled` from code):

| On Disable | Switched off | Destroyed |
|---|---|---|
| **Keep** (default, as before 3.1) | Its 3D sounds are attached to it, so Unity stops them with it; a loop is never reclaimed. | Its 3D sounds are cut, with a warning, and Pepper rebuilds their voices. |
| **Stop** | Every sound on it fades out over **Stop Fade Seconds** (default 0.1 s) where it is, and goes back to the pool. Delayed parts of its sounds (a Delay action, a Sequence's next clip) are cancelled. | The same: the fade finishes after the object is gone. |
| **Pause** | Its sounds pause (fading out over about 25 ms) and hold their place, keeping no real voice. Switched on again, they carry on from there, every layer of an engine together, in phase. | They stop with the fade. |

- **With Stop or Pause, the sounds aren't attached to the object.** They stay under Pepper's "AudioVoices" root and follow it: Pepper moves each one to its object (and its point) every frame, after every LateUpdate and before Unity mixes, so it sits exactly where an attached one would. That is what lets a fade finish after the object is gone. Unity won't let anything move a child out of an object while it is being switched off.
- **While a Stop or Pause emitter is off, posts on it play nothing** (they return a finished handle, silently): a script on the same object posting in its own `OnDisable` runs either before or after the emitter, so it must not matter.
- **Set On Disable before posting.** It is read when a sound is posted; sounds already playing keep the way they were attached.
- **Use Stop for objects created and removed all the time** (cars on show, cars that leave), and Pause for objects you switch off to bring back.

```csharp
// A car prefab: its sounds fade out over 0.2 s when the car is removed, instead of being cut.
car.WhenDisabled = EmitterDisableMode.Stop;
car.StopFadeSeconds = 0.2f;
```

##### Smoothing

The game usually writes a value every frame (or faster, from physics), and it jumps around. A parameter's **Smoothing** makes what sounds hear follow it gently, the same at 40 fps as at 144 fps:

| Mode | Fields | What it does |
|---|---|---|
| None | | Sounds hear the written value at once. |
| Ease | Rise, Fall (seconds) | Closes 95% of the gap to a new value in that time: a fast start that slows as it arrives. Separate times for rising and falling, for example a quick throttle attack and a slower release. |
| Rate | Rise speed, Fall speed (units per second) | Moves at most that fast, then stops on the value. |

0 in any field means "at once" for that direction.

- **Each scope smooths its own value.** Two cars ease their own RPM; a value on a handle eases separately from its emitter's.
- **The first value a scope gets is heard at once,** so a new engine doesn't glide up from the default.
- **Values set by name are never smoothed.** `SetRTPC` and `TransitionRTPC` behave exactly as before. Smoothing applies to writes through the asset (`SetParameter`, `RampParameter`).
- **Snap** skips the smoothing: sounds hear the written value now, and a running ramp jumps to its end. Use it on a reset or a teleport so nothing glides across it.
- **Per car:** an emitter or a handle can override the smoothing for its own value, for example a different throttle attack per car.
- **Bypass While:** point it at an on/off parameter (a rev limiter). While that reads 0.5 or more in the same scope, the value is heard at once, so the limiter's cuts stay sharp.
- **Clearing a smoothed value** eases it over to the next scope's value before it goes, instead of jumping.

```csharp
// On the asset: Smoothing = Ease, Rise 0.05 s, Fall 0.2 s. Then write the raw value every frame:
car.SetParameter(load, throttle);

// This car only: a sharper attack.
car.SetSlewOverride(load, ParameterSlew.Ease(0.02f, 0.2f));

// The car was reset to the start line: nothing should glide.
car.SnapAllParameters();
```

`GetEffectiveParameter` returns the smoothed value sounds hear now; `TryGetParameter` and `GetRTPC` return what was written. RTPC listeners are told written values, not the smoothed ones.

##### Ramps

`RampParameter(parameter, target, seconds)` exists on the manager (global), an emitter and a handle. It moves the written value in a straight line from what was written before (or, with nothing written, what that scope hears now), on unscaled time; smoothing applies on top. A new ramp, a Set, a Snap or a Clear on the same parameter in the same scope replaces it. `TransitionRTPC(name, value, seconds)` is a global ramp by name: a second call now **replaces** the first instead of the two fighting.

##### When values are heard

Pepper updates parameters (ramps and smoothing) once a frame, **after every LateUpdate** and just before Unity mixes the audio, so a value set anywhere in this frame's game code (Update, LateUpdate, a camera script) is heard this frame. No Script Execution Order setup is needed.

- `SetRTPC` still tells `IRTPCListener`s at once, inside the call. Global writes through an asset tell them too, by the asset's name. Emitter and play values don't.
- A Blend layer picks a change up in that per-frame update, not inside `SetRTPC` as it used to. It is still the same frame.
- **Manual update:** set the Audio Manager's **Update Mode** to Manual and call `AudioManager.Instance.Tick()` once a frame after your last write, if your game runs its own loop.
- The update starts with the frame after the Audio Manager is created. A Blend posted in that very first frame still starts at the right level.

##### Cost

Each parameter gets a small number the first time it is used. After that, a write is an array write: no string lookups and no garbage. One hundred writes and an update allocate nothing.

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
float MaxDistance { get; set; }            // Zero volume distance (setting it changes the running game only)
AudioRolloffMode RolloffMode { get; }     // Logarithmic/Linear/Custom
AudioAttenuation Attenuation { get; }      // A shared distance asset; replaces the three above when set
float DopplerLevel { get; set; }           // Unity's Doppler strength (Doppler = Unity)
DopplerMode Doppler { get; set; }          // Unity (default) or Pepper
float DopplerExaggeration { get; set; }    // Pepper: ×1 to ×3
float DopplerMaxCents { get; set; }        // Pepper: the largest shift either way, in cents
```

##### Doppler

A container's **Doppler** (3D Settings) says who shifts the pitch of its 3D sounds as they move relative to the listener:

| Doppler | What it does |
|---|---|
| **Unity** (default, as before 3.1) | Unity's Doppler, by **Doppler Level** (0 = none, 1 = Unity's default). Pepper can't see it, so a voice that goes virtual comes back out of step with it. |
| **Pepper** | Pepper's own, in the voice's pitch stack: ratio = c ÷ (c − v), c = 343 m/s, v the sound's closing speed on the listening point (positive coming closer). Then times **Doppler Exaggeration** (×1 physical, up to ×3 for an arcade pass) and kept within ± **Doppler Max Cents** (100 cents = a semitone). Unity's Doppler is off on these voices. |

- A car passing at 50 m/s swings about 5 semitones at ×1 (+2.7 coming, −2.4 going) and about 10 at ×2.
- The closing speed is the sound's **Approach Speed** (§3.4): its emitter's velocity (measured, or `SetVelocity`), else how the sound moves, against the listening point's, eased over 50 ms. One value per sound (from its first voice), as for the built-in values.
- **Teleports:** a car reset or placed elsewhere calls `emitter.Teleport()`, so the jump isn't read as speed; a camera cut calls `point.ClearVelocity()`. Without them, Max Cents keeps the shriek bounded.
- It is outside the container's **Pitch Limit** (which is for the authored pitch); a virtual voice keeps its place at the pitch with Doppler in, so layers come back in step.
- **Uses Parent Positioning** carries it down with the other 3D settings: set it once on an engine's top Blend.
- **When Pepper pans** (the Audio Manager's Spatial Mode = Pepper, §9.3), every 3D sound plays 2D for Unity, which then has no Doppler to give it, so every one gets Pepper's: a container left at Doppler = Unity gets Pepper's at its **Doppler Level** (0 = none) as the exaggeration, within its Max Cents. The closing speed is measured from the sound's own listening point, the one that hears it loudest.

##### Attenuation assets

**Create > PepperSFX > Settings > Attenuation** makes a distance asset you can share between sounds: how they change with distance from the listening point, as curves over **metres**.

| Field | Unit | What it does |
|---|---|---|
| **Max Distance** | m | Where the curves end. Past it the sound plays virtually (silent, keeping its place, no real voice), and it comes back inside 95% of it. |
| **Volume** | dB | The level at each distance (0 = as authored, -6 = half). The default: 0 dB to 2 m, 6 dB less per doubling, nearly silent at 100 m. |
| **Low Pass** (optional) | Hz | Dull the sound with distance (22000 = open). |
| **High Pass** (optional) | Hz | Thin it with distance (10 = open). |
| **Spread** (optional) | 0–1 | How wide it sounds: 0 a point (fully panned), 1 fully wide. Close cars wide, far cars narrowing to a point. |

- Set it in a container's **3D Settings** (Attenuation). It replaces that container's Min Distance, Max Distance and Rolloff, which grey out. A container inside a Blend or Switch takes its parent's with **Uses Parent Positioning**, so set it once on the top of an engine. Containers without one work exactly as before.
- **Volume and Spread** are applied by Unity while it mixes (Pepper hands the curves over as Unity's custom curves), so even a fast pass-by fades smoothly at any frame rate. Unity measures from its AudioListener: keep it on the listening point (`AudioManager.DriveUnityListener`, §9.3).
- **Low Pass and High Pass** are set by Pepper from each voice's distance every frame, eased over about 20 ms, and combine with Parameter Curves: the lowest low-pass and the highest high-pass win.
- **Past Max Distance**, a sound starts or goes virtual while the Audio Manager manages voices (LOD on); a persistent one too. With LOD off it keeps playing at the curve's last level.
- Far sounds lose to near ones: a voice's importance (§3.2) comes from how loud its curve says it is at its distance.
- `handle.SetAttenuation(otherAsset)` plays one sound with another asset (a hero car heard further away), and `ClearAttenuation()` goes back. Neither changes an asset.
- Filter curves go in a straight line in octaves between keys, as Parameter Curves do.
- **Editing the asset in Play mode:** with Spatial Mode = Pepper (§9.3), the volume, filter and spread curves are read every frame, so a change is heard at once. With Spatial Mode = Unity, the volume and spread curves and Max Distance are handed to Unity when a sound starts, so a sound already playing (a persistent engine) keeps the old ones until it is posted again, while Pepper's own Max Distance check reads the new one at once. Re-post the sound after changing them.
- The curves' last key holds past it: with the default volume curve (nearly silent at 100 m) and a Max Distance of 150 m, a sound is nearly silent from 100 m but stays a live voice to 150 m. Move the last keys to Max Distance.

**Parent Settings** (for a container played inside a Blend or Switch):
```csharp
bool UsesParentPositioning { get; set; }  // Take the parent's 3D settings instead of its own
bool UsesParentOutput { get; set; }       // Play through the parent's Mixer Group instead of its own
```
Both are off by default, so a child keeps its own settings, as before. A Blend's or Switch's own 3D settings and Mixer Group never reach its children unless a child asks. If the parent itself asks its own parent, the question goes up until a container answers it. Set the 3D settings once on the top Blend and tick Uses Parent Positioning on the layers.

**Parameter Curves:**
```csharp
List<ParameterCurve> ParameterCurves { get; }   // properties driven by parameters
```
Each curve maps one parameter (X, in the parameter's unit) to one property (Y):

| Property | Y unit | Several curves on one container |
|---|---|---|
| Volume | dB (0 = as authored, -6 = half) | add |
| Pitch | cents (100 = one semitone) | add |
| Low Pass | Hz (22000 = open) | the lowest wins |
| High Pass | Hz (10 = open) | the highest wins |

- The parameter is read in each play's scopes (the handle's value, then the emitter's, then global, §3.4), after smoothing, every frame. Curves apply to every sound below the container in the play's tree, and compose down it: volumes and pitches add, the lowest low-pass and highest high-pass win.
- Filter curves go in a straight line in **octaves** between keys (1 kHz to 4 kHz passes 2 kHz half way), and ignore tangents.
- A **Low Pass** or **High Pass** curve switches on the filter every pooled voice has had, switched off, since the pool was built (so nothing is added mid-game); it eases over about 20 ms, is set before the voice's first sample, and switches off when fully open. These are separate from occlusion, which keeps filtering through mixer slots (§14.1).
- A volume curve can't make a sound louder than full scale (an AudioSource's volume tops out at 1). The inspector warns when a curve goes above 0 dB.
- A container with curves is updated every frame; one without costs nothing.

```
Example: the hero car's engine pulls back as speed rises (a racing game).
Engine Blend → Parameter Curves → Speed (m/s): Volume, (0, 0 dB) to (60, -6 dB)
Wind loop    → Parameter Curves → Speed (m/s): Low Pass, (0, 800 Hz) to (60, 12000 Hz)
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
        float dbOffset = AudioRandom.Range(volumeRandomMin, volumeRandomMax);   // Pepper's random numbers, see below
        // MultiplierGain is the gain-stack slot for per-voice variation; it composes with
        // the bus, occlusion, blend and handle slots.
        voice.GainStack.MultiplierGain *= Mathf.Pow(10f, dbOffset / 20f);
    }

    if (enablePitchRandomization)
    {
        float centsOffset = AudioRandom.Range(pitchRandomMin, pitchRandomMax);
        // The pitch stack's Randomization slot, so a handle's SetPitchOffset composes with it.
        voice.PitchStack.Randomization *= Mathf.Pow(2f, centsOffset / 1200f);
        voice.UpdateFinalPitch();
    }
}
```

The volume value is drawn before the pitch value, and both after the clip pick. Networked posts rely on that order to play the same variation on every machine.

##### Random numbers

Every random choice a sound makes (which clip a Random container plays, a Sequence's random order, volume and pitch randomization, multi-position offsets) comes from **Pepper's own random numbers**, not `UnityEngine.Random`, so audio never moves your gameplay's random sequence:

- A sound posted on an **Audio Emitter** draws from that emitter's own sequence. Set the emitter's **Random Seed** (or call `emitter.SetRandomSeed(seed)`) and the same seed replays the same picks, whatever other cars play; 0 (the default) gives a different sequence every run.
- Sounds without an emitter share one Pepper sequence.
- **Networked posts** (§17) still seed `UnityEngine.Random` for the post, so every machine picks the same variation, and put its state back afterwards.
- The Audio Manager's **Random Numbers** setting switches back to `UnityEngine.Random` (Unity), as before 3.1.

**Pitch Limit (semitones):** keeps the container's voices within that many semitones of their recorded pitch, whatever randomization and other pitch changes add up to. 0 (the default) means no limit. `SetPitchAbsolute` ignores it.

**Why Randomization Matters:**
Prevents "machine gun effect" where repeated sounds become obviously repetitive.

**Recommended Values:**
- **Subtle variation:** Volume ±1-2 dB, Pitch ±50-100 cents
- **Dramatic variation:** Volume ±4-6 dB, Pitch ±200-300 cents

#### 4.2 RoutingContainer

**Menu:** Create > PepperSFX > Containers > Routing Container

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

**Menu:** Create > PepperSFX > Containers > Random Container

**Purpose:** Picks one random clip per trigger, with weighting and repeat avoidance.

##### Properties

```csharp
List<WeightedAudioClip> AudioClips { get; }  // Clips with weights
int AvoidRepeatLast { get; }                  // Don't repeat last N clips (0-10)
bool UseWeighting { get; }                    // Enable weighted selection
float Volume { get; }                          // Base volume
bool Loop { get; }                             // Loop selected clip
```

**Avoid Repeat Last** remembers recent clips **per Audio Emitter**: a car never repeats its own last clips, whatever other cars just played. Sounds posted without an emitter share one memory. `ClearHistory()` forgets them all. A networked post ignores and doesn't change the memory, so the seed alone decides its clip. Keep it below the clip count: at or above it the setting does nothing (any clip can play next), and a container of two or more clips warns about it.

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
    float random = AudioRandom.Range(0f, totalWeight);

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

**Menu:** Create > PepperSFX > Containers > Sequence Container

**Purpose:** Plays clips in a defined order with various playback modes.

> **Auto Advance and the handle.** With Auto Advance on, one post plays the whole sequence, and
> every clip it plays belongs to that post's handle:
> - the handle stays alive through Delay After gaps;
> - `OnFinished` fires after the **last** clip;
> - `handle.Stop()` ends the sequence, even in the middle of a gap.
>
> A sequence with Loop Sequence off stops after its final entry, and its next post starts from
> the top again. If a clip is stopped by anything (a Stop, a steal, a crossfade, `StopAllSounds`),
> the sequence ends; only a clip that finishes on its own moves it on. Pausing a clip holds the
> sequence.

> ⚠️ **Hard limit — one logical sequence per asset.** The sequence position and the auto-advance
> run live on the container **asset**, so every emitter that plays the same SequenceContainer
> shares one position. Starting it from a second emitter takes the run over: the first emitter
> keeps its current clip, and its handle finishes when that clip ends. `container.Stop()` stops
> *every* play of the asset, on any container type; to stop one play, stop its handle. If you need
> two independent sequences running at once (two alarm panels, two radios), **duplicate the
> asset** — one SequenceContainer per logical sequence. This is by design for 3.0.

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
            currentIndex = AudioRandom.Range(0, Entries.Count);
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

**Menu:** Create > PepperSFX > Containers > Switch Container

**Purpose:** Selects child container based on game state switch value.

##### Properties

```csharp
AudioSwitchGroup SwitchGroup { get; }        // The Switch Group asset it reads (or null)
string SwitchGroupName { get; }              // Which switch to monitor (the asset's name, or the name field)
List<SwitchEntry> SwitchEntries { get; }     // Value→Container mappings
AudioContainer DefaultContainer { get; }      // Fallback if no match
SwitchChangeMode WhenSwitchChanges { get; }   // Next Play Only, Crossfade or Restart
float CrossfadeTime { get; }                  // Crossfade only, seconds
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

- When the Switch plays, it reads its switch group's value in that sound's scopes, the same three as parameters (§3.4): the sound's own (`handle.SetSwitch`), else its emitter's (`emitter.SetSwitch`), else the global one (`AudioManager.SetSwitch`), else the group's Default Value. It picks the matching entry's container, or the Default Container when none matches.
- What happens when the value changes while the sound plays is the Switch's **When Switch Changes** (below). The default, **Next Play Only**, is how Switches always worked: the playing sound keeps its choice and the next play picks the new value.
- The chosen container plays as part of the same play, under its own place in the play's tree. So a Blend under a Switch reads its parameter in that play (the handle's or emitter's value), and a Sequence under it reports the clips it starts later to the same handle.
- Every voice the chosen container starts, later clips included, gets the Switch's **Volume** on top of its own.
- The Switch's own 3D settings and Mixer Group reach the chosen container only if it ticks Uses Parent Positioning / Uses Parent Output (§4.1).

```csharp
// The tyre tree from a racing game: a Switch on surface over Blends on slide speed.
car.SetSwitch(surface, "Asphalt");          // this car's surface (surface: a Switch Group asset)
AudioHandle frontAxle = car.Post(tyreEvent, "FrontAxle");
frontAxle.SetParameter(slideSpeed, 3.5f);   // the asphalt Blend under the Switch reads it
```

##### When Switch Changes

| Setting | What a playing sound does when its value changes | For |
|---|---|---|
| **Next Play Only** (default) | Nothing; the next play picks the new value. | One-shots: footsteps, impacts. |
| **Crossfade** | The new choice starts from its beginning and fades in while the old one fades out, over **Crossfade Time** (seconds, default 0.15), along equal-power curves so the loudness holds steady. | Loops: a tyre's surface bed, an ambience. |
| **Restart** | The old choice fades out as a stolen sound does, over the Audio Manager's **Steal Fade Seconds** (25 ms by default, at least three frames; 0 cuts it in a few ms), and the new one starts at once, at full level. | Loops that must not overlap. |

- The sound keeps its handle: no second `OnStarted`, and `OnFinished` only when it really ends.
- If the value flips back while the old choice is still fading out (a car clipping a kerb), the old one fades back up; no second copy starts.
- When the old choice has faded to silence, its voices stop. A Sequence in it starts no more clips.
- During a crossfade the sound plays both choices, so it uses their voices together for that moment. If the voice pool is too full for that, the old choice is cut at once and the new one takes its voices.
- A sound that is virtual (far away, or less important than the ones playing) changes virtually: the new choice starts silent and takes no other sound's real voice.
- A paused sound doesn't follow; it catches up when resumed.
- A value with no entry and no Default Container fades the sound out to silence. It stays alive, and comes back when the value has an entry again. That holds once the sound plays: when the value has no entry at the post itself, nothing starts and the post ends at once (its handle is finished). A loop posted once and kept (a tyre bed on a car that may start in the air) needs a Default Container.

```csharp
// Each car's rear tyre bed follows the car's surface; the front axle has its own.
car.SetSwitch(surface, "Kerb");             // the rear bed (and everything else on the car) crossfades to kerb
frontAxle.SetSwitch(surface, "Asphalt");    // the front axle stays on asphalt until it crosses
```

##### Switch Group assets

**Create > PepperSFX > States and Parameters > Switch Group** makes a group asset: its **Group Name** (empty = the asset's name), its **Values** (Asphalt, Kerb, Grass) and a **Default Value** read when nothing is set. Give it to the Switch's **Switch Group** field (the Switch inspector's **Create Switch Group Asset From Name** button makes one from the name and the entries' values), and each entry's value becomes a dropdown.

- A group name and an asset with that name are **one switch**: `AudioManager.SetSwitch("Surface", "Grass")`, state and event SetSwitch actions, and a Switch that only names "Surface" all read and write the asset's switch.
- Setting a value that isn't in the asset's Values warns once per value (a typo would otherwise just play the Default Container).
- The emitter and handle calls take the asset: `car.SetSwitch(surface, "Kerb")`, `handle.SetSwitch(surface, "Kerb")`, `ClearSwitch`, `TryGetSwitch`, and `GetEffectiveSwitch` for what a sound reads.
- A Switch that only names its group reads the values set (by name or through an asset) but not an asset's Default Value: with nothing set, it plays its Default Container, as before.

##### Driven by a parameter

A group can take its value from a parameter: set **Driven By** (a Parameter asset) and its **Ranges**, each a value and where it starts, running up to the next one's start. For example a tyre's slide speed picks Grip from 0, Slide from 0.3 and Skid from 0.7.

- The parameter is read in each sound's scopes, so each axle can drive its own (set the slide speed on that axle's handle).
- **Hysteresis** (in the parameter's unit) is how far past a boundary the parameter must go before the value changes, so a value sitting on 0.3 doesn't flicker between Grip and Slide. Each playing sound remembers its own range.
- A driven group follows only its parameter: a value set directly on it (`SetSwitch` on the manager, an emitter or a handle) is ignored, with one warning.

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

// Per character: on its Audio Emitter, with the group asset
playerEmitter.SetSwitch(surfaceType, "Metal");
playerEmitter.Post(footstepEvent);
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

**Menu:** Create > PepperSFX > Containers > Blend Container

**Purpose:** Plays multiple containers simultaneously, crossfading them by a parameter.

> **Every post is a new copy.** Posting a Blend event again starts another copy of the blend
> alongside the one already playing, like any other container (two engines using the same
> blend). **Each copy reads its own value** (§3.4): posted on two Audio Emitters, two engines
> follow two RPMs. Copies with no emitter or handle value follow the global value together. To
> **restart** instead, set the event's **Max Instances to 1**; the inspector offers a one-click
> button. The old copy then stops instantly, without a fade. `handle.Stop()` stops one copy;
> `blend.Stop()` stops them all. Before 3.0, a second post faded the first copy out, so a Blend
> asset could only play once at a time.

##### Properties

```csharp
List<BlendEntry> BlendEntries { get; }          // Layers to blend
AudioParameter BlendParameter { get; }          // The parameter asset, or null
string BlendParameterName { get; }              // The parameter's name (the asset's, when set)
float Volume { get; }                            // Master volume
BlendLoopMode LoopMode { get; set; }             // FromLayers (default), ForceLoop or ForceOneShot
SilentLayerBehaviour SilentLayers { get; set; }  // KeepPlaying (default) or Virtualize
float SilentThresholdDb { get; set; }            // Virtualize: below this a layer goes virtual (-60)
bool Loop { get; }                               // Obsolete and not read: see LoopMode
BlendLayout Layout { get; set; }                 // VolumeCurves (default) or Regions
CrossfadeShape Crossfade { get; set; }           // Regions: Linear, EqualPower (default) or SCurve
AudioParameter PitchFollowParameter { get; set; } // Pitch follow: each layer plays at value ÷ its Root Value
IReadOnlyList<AudioVoice> LayerVoices { get; }  // Layer voices of every live play of this asset
float GetCurrentBlendValue()                     // The value a copy with no emitter or handle value reads
void AutoLayout()                                // Regions from the layers' Root Values
```

**Choosing the parameter.** Set **Blend Parameter** to a Parameter asset: the curves' X axis is then in its unit (RPM, m/s) and values are clamped to its range. Or leave it empty and type a **Blend Parameter Name**: values are clamped to 0–1, as Blends always read them. The inspector's **Create Parameter Asset From Name** button turns a name into an asset with a 0–1 range, so existing curves read the same; code calling `SetRTPC` with that name keeps working.

The Blend's other settings inherited from the container base (3D, distances, mixer group, occlusion, randomization) are not applied to its layers; each layer uses its own. Only **Volume** and the curves always reach the layers. A layer can take the Blend's 3D settings (**Uses Parent Positioning**) or Mixer Group (**Uses Parent Output**), §4.1: set them once on the Blend and tick the box on each layer.

##### BlendEntry Structure

```csharp
[Serializable]
public class BlendEntry
{
    public AudioContainer container;          // Layer container
    public AnimationCurve volumeCurve;        // Volume Curves layout: parameter value → layer level (0-1)
    public float rootValue;                   // The value it was recorded at (an engine loop's RPM)
    public float regionMin, regionMax;        // Regions layout: where it plays
    public float fadeInWidth, fadeOutWidth;   // Regions layout: how far in from each edge it reaches full level
    public bool followPitch = true;           // Pitch follow on for this layer
    public float followRangeSemitones = 12f;  // Pitch follow never moves it further than this
    public List<ParameterCurve> curves;       // Extra curves for this layer only (§4.1)
}
```

##### Two Layouts: Volume Curves or Regions

**Volume Curves** (the default, and every Blend made before 3.1): each layer's level is its own drawn curve against the Blend's parameter, 0 to 1.

**Regions:** each layer plays in a region of the parameter and crossfades with its neighbours, so nothing has to be drawn:
- Give each layer its **Root Value**: for an engine loop, the RPM it was recorded at.
- Press **Auto Layout** in the inspector. It sorts the layers by root and gives each one a region from half way to the lower neighbour's root to half way to the upper one's; each crossfade is centred on that half-way point and covers **Auto Layout Fade** of the gap (0.5 = half). The lowest layer reaches down to the parameter's Min and the highest up to its Max. Edit the numbers afterwards if you like.
- **Crossfade Shape:** **Equal Power** (the default) keeps the loudness steady when the layers are separate recordings, like engine loops at different RPMs: each is at -3 dB at the middle. **Linear** suits layers recorded together (-6 dB at the middle). **S-Curve** eases in and out.

The inspector lists every layer's region.

##### Pitch Follow

Set **Pitch Follow Parameter** (usually the same RPM parameter) and each layer plays at the parameter's value ÷ its **Root Value**: a loop recorded at 4000 RPM plays at 1.25× at 5000 RPM. Every layer stays in tune with the engine, and so with the others, with no pitch curve to draw.
- **Follow Range** (±12 semitones by default) caps how far a layer is pushed; **Follow Pitch** off leaves a layer alone (a whine that shouldn't move, say).
- Pitch follow goes into the pitch stack's `Follow` slot, so a handle's pitch offset and randomization still apply on top (§3.2).
- **Tuning check:** the inspector shows how far each layer is pitched across the part of the range where it can be heard, and warns past **Tuning Warning** semitones (5 by default). Recordings stretched much further start to sound wrong; add a layer in between or narrow the regions.
- Unity changes pitch once per frame, in a step, so a very fast sweep at a low frame rate steps slightly; parameter smoothing (§3.4) keeps each step small.
- Use pitch follow instead of `SetPitchOffset` on the whole handle for engines: one handle pitch can't keep layers recorded at different RPMs in tune.

##### How It Works

**Starting a play:**
- Every entry's container is played, all with the same position and parent. A layer can start more than one voice (a Routing layer with three clips starts three), and every one of them joins the post's handle.
- Each layer voice gets the Blend's **Volume** multiplied into its `GainStack.MultiplierGain`.
- The play reads its parameter (handle, then emitter, then global, then default) and sets each layer's level, so the layers start at the right level.
- Every layer starts on **one DSP time**, the play's: two DSP buffers ahead (one buffer later than a single voice, so a walk through many layers can't straddle a buffer), or the post's explicit DSP time. The layers are sample-aligned and stay so.

**Every frame:**
- Each live play reads its parameter again (after smoothing, §3.4) and, for each layer voice, works out the level of the entry that **started** that voice (its curve, or its region), writing it into `GainStack.RtpcGain`. Pitch follow and any parameter curves are updated in the same pass. This runs after every LateUpdate, before the frame's audio is mixed (§3.4).
- Paused and virtualized layers are updated too, so the level is right when they resume.

**Nesting.** Any container can be a layer, including another Blend or a Switch. Each play keeps a tree of the containers it reached, so every nested Blend reads its own parameter in that play's scopes (the handle's value, then the emitter's, then global), and a layer under several Blends gets the product of their curves. (Before 3.1, whichever value had changed last won.) For example, an engine: a Blend on load over two Blends on RPM (the on-load and off-load sets); load crossfades the sets while RPM moves inside both, per car.

**Things to know:**
- Voices that a layer starts later (an auto-advancing Sequence's next clip) start at their place in the tree, so they get the Blend's Volume and curve like the first clip. (Before 3.1 they started at full level.)
- Only plays with a Blend somewhere in their tree are updated every frame; a play without one costs nothing per frame.
- A steal takes a whole Blend play or none of it, never one layer (§3.2, Voice Stealing). For a sound that must never be stolen, tick **Persistent** on its event (§5.3).
- `UpdateBlend(value)` is obsolete: it sets the parameter's global value. Set the parameter instead.

**Loop Mode.** Whether the layers loop. **From Layers** (the default): each layer loops as its own container's Loop setting says, as Blends always did. **Force Loop**: every layer loops. **Force One Shot**: none does. A Blend inside another one that leaves its Loop Mode at From Layers takes the outer one's. The old **Loop** checkbox was never read and is gone from the inspector; Sound Builder (Simple) now sets Loop Mode (its Loop ticked = Force Loop).

**Silent Layers.** **Keep Playing** (the default): a layer near silence keeps its real voice, sample-locked with the others, which music stems need. **Virtualize**: a layer whose own level is below **Silent Threshold** (−60 dB by default) goes virtual, freeing its real voice, and comes back once it is 6 dB above it. "Its own level" is its volume, the Blend's crossfade and its curves; the bus, occlusion, a stop fade or a crossfade don't count. A virtual layer keeps its place in the clip (§3.2), so it comes back in step with the others, about 54 dB down, where its short fade-in can't be heard on a slow sweep. The fade-in takes at least three frames (75 ms at 40 fps), so a layer that must be back within a few frames is late at low frame rates: an engine's off-load set, parked at full throttle, against a 50 ms rev-limiter cut. Give such a Blend Keep Playing. A layer that is already silent when the sound starts starts virtual and is never heard starting. A silent layer is never stolen on its own; it lives and ends with its sound. For an engine of 14 layers of which 2–4 are heard at a time, this keeps it to 2–4 real voices. A Blend inside another one that leaves this at Keep Playing takes the outer one's.

##### Designing Volume Curves

**Concept:** AnimationCurve maps the parameter's value (X: 0–1 for a named parameter, the asset's unit for a Parameter asset) to layer level (Y: 0–1).

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

#### 4.7 Generated Engines (Unity 6.3 and later)

An engine can be played from sweeps instead of loops: the **Granular Engine** cuts a recorded or synthesised sweep into
engine cycles and plays them at the car's RPM, so a fast rev glides through every RPM with no band crossfade. The
**Additive Engine** adds the engine's firing orders as sine tones, for weight. Both are containers whose sound is
computed while they play (Unity 6.3's audio generators), on Pepper's voices like any other: buses, panning, Doppler,
filters, pause, virtual voices and Blend nesting all apply. They are in the `SFXSystem.Generators` assemblies, which
compile only on **Unity 6000.3 and later**; on earlier versions they don't exist, and the rest of Pepper is unchanged.

**Which engine when:**
- **A loop blend** (the engine recipe, §3; the Engine Builder, §10.8): steady character, predictable, every behaviour a
  layer you control. Its weakness is fast revs, where each loop is stretched until its neighbour takes over.
- **A granular engine:** the best response and transitions, from two sweeps per car. Its weakness is a held RPM, which
  can sound frozen; it varies the grains to avoid that.
- **Both, or either with an additive layer:** a Blend whose layers are the engines, each at full level. This is how
  most current racing games build theirs.

**1. Make the sweeps.** An **on-load sweep** is the engine climbing at full throttle, idle to redline; an **off-load
sweep** is it falling with the throttle closed. Mono (or folded to mono), steady and slow enough to give at least 10
grains per 1000 RPM: 10 s from 1000 to 9000 RPM is plenty. A synthesised sweep is ideal, because its RPM is known
exactly: note its range and whether it moves in a straight line or evenly in pitch (most synthesisers). Set each clip to
**Decompress On Load**: the engine reads its samples.

**2. Cut them into grains** with **Window > PepperSFX > Engines > Engine Grains** (§10.10). Each sweep becomes a **Grain
Table**: one grain per engine cycle (two turns), tagged with its RPM.

**3. Make the engine:** **Create > PepperSFX > Containers > Granular Engine (Unity 6.3+)**.
- **Rpm** and **Load**: the same parameters a loop engine reads (EngineRpm, EngineLoad01). Without Load it plays on
  load only.
- **On Load** and **Off Load**: the two Grain Tables, blended by load at equal power.
- **Idle** and **Idle Rpm** (optional): an idle loop and the RPM it was recorded at. Below Idle Rpm the idle plays
  alone, pitched to the RPM; between it and the sweeps' lowest RPM the two crossfade.
- **Crossfade Ms** (3): how long each grain fades into the next. The grains are cut in phase, so the fade's gains sum to
  1 and a join isn't heard.
- **Variation** (2): at each grain, a random pick among up to this many neighbours within half a semitone of the RPM,
  so a held RPM never repeats one grain.

Each grain is played by the RPM the sweep had at every point of it, not by its average: the sweep climbed while the
grain was recorded, and played at one speed a held RPM would warble once a cycle (fixed after `3.1.0-dev.11`).

**4. Play it** like any engine: an event with **Persistent** on and a **Target Bus**, posted on the car's Audio Emitter
(§3.4). The game writes RPM and load on the emitter as it does for a loop engine; nothing in the game's code changes.
Each car's voice reads its own values. A generated engine moves to each frame's values over the time that frame took,
so a rev climbs at an even pace whatever the frame rate; it hears them about a frame and two audio buffers late
(25–75 ms).

**5. Tune it by ear** in the **Audition** (§10.5): the Rpm and Load parameters get sliders, and a sweep on Rpm plays a
rev. Generators play outside Play mode.

**An additive layer** (optional): **Create > PepperSFX > Containers > Additive Engine (Unity 6.3+)**. Order 1 is the firing
frequency, RPM × **Cylinders** ÷ 120 (Cylinders is a scale, as in Loop Analysis: 6 under loops or sweeps tuned to notes
at RPM ÷ 20 Hz). Each order (0.5, 1, 1.5, 2… up to 16 of them) has a level **Off Load** and **On Load**, and the load
moves between them; a little noise, low-passed at **Noise Brightness** times the firing frequency, takes the edge off.
It isn't a whole engine: on its own it sounds synthetic. Put it under an engine in a Blend:

```
Car_Engine (Blend, 3D, no blend parameter; each layer at full level)
├── Car_Granular   (Granular Engine)            or the Engine Builder's loop tree
└── Car_Weight     (Additive Engine, Volume 0.3)
```

Leave **Follow Pitch** off for a generator layer: it already plays at its RPM (the Container Validator says so).

**What it costs.** Measured on the development PC, without Burst: a granular engine with both sweeps playing takes
about 0.4% of a CPU core, and an additive layer of 16 orders about the same. To measure on the target (a Steam Deck),
switch the measurement on in a build, drive for a while, and log it:

```csharp
using AudioSystem.Generators;

GeneratorCost.Measuring = true;                      // starts a new measurement
// ... drive for 10 seconds ...
Debug.Log(GeneratorCost.Report(enginesPlaying));     // buffers, µs per buffer, share of a core per instance
GeneratorCost.Measuring = false;
```

**Good to know:**
- A Grain Table keeps a reference to its sweep, not a copy: re-analyse after editing the sweep, and the next play
  reads it again.
- Below the sweeps' lowest RPM with no idle loop, and across a gap in the sweep, the nearest grain is stretched.
- A Sequence container can't hold a generator (its entries are clips).
- The Container Validator checks generator containers: nothing to play, no RPM parameter, a table with no grains, a
  sweep that can't be read, an off-load sweep with no Load parameter, a silent additive layer.

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

##### Timed posts: a moment of the sound at a DSP time

A pass-by whoosh must have its peak land on the moment the cars pass. Post it saying which point of the sound (seconds into its clips, at their recorded pitch) is heard at which DSP time (`AudioSettings.dspTime`):

```csharp
// The whoosh peaks 0.5 s into its clip; the pass is predicted 0.3 s from now. Start 0.2 s in, now.
double pass = AudioSettings.dspTime + 0.3;
float startAt = 0.2f;
otherCar.Post(whooshEvent, pass - (0.5 - startAt), startAt, PostValues.Of(closing, speed));
// Also: whooshEvent.Post(emitter, startDsp, startOffset, values), AudioManager.Post(evt, emitter, ...),
// and whooshEvent.Post(source, position, startDsp, startOffset, values) without an emitter.
```

- **If that time has passed** (or is nearer than the next DSP buffer, as "now" always is), the sound starts at the next buffer, that much further into the clip at its pitch, so the moment still lands on time. A one-shot already past its end plays nothing (its handle finishes at once); a loop wraps round.
- Every voice the event's Play actions start with takes the offset, so a Blend's layers stay together. A Sequence's later clips start from their beginning. A Play action with a **Delay** keeps its own timing and starts from the beginning.
- The older `Post(..., scheduledDspTime)` overloads are unchanged: they start the clip from its beginning, at that time or the next buffer.

**How long will it play?** `evt.EstimateDuration(values)` (or `EstimateDuration(emitter, values)`) says before posting, as an `AudioDuration`: `Shortest` and `Longest` seconds (at the recorded pitch), `Loops`, and `IsKnown` (false when nothing would play). A Random container gives its shortest and longest clip; a Switch the choice the values pick (the post's values, then the emitter's, then global); a Blend or Routing container its longest layer; a Sequence the entry it plays next, or with Auto Advance the rest of the run; each Play action counts with its Delay added. It draws no random numbers, so it can't change what a networked post picks.

```csharp
var length = whooshEvent.EstimateDuration(PostValues.Of(closing, speed));
float preRoll = 0.5f;                                       // where the whoosh peaks, from your own authoring
float startAt = Mathf.Max(0f, preRoll - (float)(pass - AudioSettings.dspTime));   // late into the clip when the warning is short
if (length.IsKnown && startAt < length.Shortest) otherCar.Post(whooshEvent, pass - (preRoll - startAt), startAt);
```

#### 5.2 Event Actions

**Concept:** One event can execute multiple actions sequentially or with delays.

##### Available Action Types

```csharp
public enum ActionType
{
    Play,              // Play a container
    Stop,              // Stop a container (only plays this event started)
    Pause,             // Pause this event's voices
    Resume,            // Resume this event's paused voices
    SetSwitch,         // Set a switch value
    SetRTPC,           // Set RTPC parameter
    SetState,          // Activate a state
    TriggerDucking,    // Duck target buses
    CrossFade          // Crossfade between containers
}
```

Stop, Pause and Resume only affect sounds started by the same event. They can't reach a sound another event started, such as music. To stop that, keep the handle `Post` returned and call `Stop` on it.

**Scope.** A Stop, Pause or Resume action acts on **All** of the event's sounds that are playing, on every object (the default, as before 3.1), or, with **This Emitter**, only on those posted on the Audio Emitter the event is posted on. With This Emitter, posting a car's "engine restart" event (Stop, then Play) on one car no longer stops every other car's engine. Posted without an Audio Emitter, This Emitter acts on all of them.

##### EventAction Structure

```csharp
[Serializable]
public class EventAction
{
    public ActionType actionType;
    public ActionScope scope;                // Stop/Pause/Resume: All (default) or ThisEmitter
    public AudioContainer container;         // For Play/Stop/CrossFade
    public AudioBus targetBus;               // Play/CrossFade routing; TriggerDucking's trigger bus
    public float delay;                      // Delay before executing (seconds)
    public float fadeDuration;               // Stop fade-out; CrossFade/SetRTPC/SetState transition;
                                             // TriggerDucking hold time. Play ignores it.

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

**Result:** Intro plays immediately, and the loop starts within about a frame of the intro's end (a delayed action waits on a frame timer). For a sample-accurate join, see the Music Systems recipe in the Cookbook.

#### 5.3 Instance Limiting

**Purpose:** Prevent event spam (e.g., 100 footsteps playing simultaneously).

##### Configuration

```csharp
[SerializeField] private int maxInstances = 0;  // 0 = unlimited
[SerializeField] private VoiceStealBehavior stealBehavior = VoiceStealBehavior.Oldest;
```

##### What counts as one instance

An instance is one **play**: everything a single Play action starts. Some containers start several voices at once: a Routing container with three clips, a Blend with two layers, a Switch that selects either of those. That is still **one** instance, and it stays live until its **last** voice ends. A steal ends the whole play at once, with every one of its voices stopping together. An event with two Play actions counts two instances per post. A multi-position post counts one instance per emitter, and its limit is `maxInstances × emitter count`.

##### Steal Behaviors

**Oldest:**
```
Behavior: Stop the play that started first, play the new one
Use For: Most sounds (footsteps, weapons)
```
Judged by start time, not playback position. A loop that has run for minutes is the oldest even when it has just wrapped back to the start of its clip. Plays posted in the same frame are tied, and the tie goes to the one posted first.

**Quietest:**
```
Behavior: Stop the quietest play (judged by its loudest voice), play the new one
Use For: Sounds where loudness matters (explosions)
```

**Furthest:**
```
Behavior: Stop the most distant play (judged by its nearest voice), play the new one
Use For: Spatial sounds where proximity matters (ambience, 3D SFX)
```

**LowestPriority:**
```
Behavior: Stop the lowest-priority play
Use For: Mixed priority scenarios
```

The steal is checked when a post arrives and before its actions run: if the event is already at `maxInstances`, one play is stopped immediately to make room.

##### Per Audio Emitter

**Max Instances Per Emitter** limits the plays of the event on **one** Audio Emitter at once, on top of Max Instances for the whole game: every car can crackle, but at most 3 crackles per car. When a car is at its limit, Steal Behavior picks which of **that car's** plays ends; other cars are untouched. The play that ends fades out over the Audio Manager's Steal Fade Seconds, like any steal (§3.2), so a burst of pops doesn't click; it used to stop on the spot. 0 = no limit per emitter. Posts without an Audio Emitter (a GameObject, multi-position) aren't counted per emitter.

```
Event: Car_Afterfire
  Max Instances: 0 (no game-wide limit)
  Max Instances Per Emitter: 3
  Cooldown Per Emitter: 0.08 s

30 posts on one car in one frame → 3 crackles on that car; another car's crackle still plays.
```

##### Persistent

Tick **Persistent** on an event whose sounds must never drop out to make room for others, such as a car's engine. The voice pool never steals its plays (§3.2, Voice Stealing). They can still go **virtual** when other sounds matter more, keeping their place in the clip, so they come back in step. The event's **own** Max Instances can still end one of its persistent plays: that is the event's rule.

If the pool is full and only persistent sounds are left in it, a new sound is skipped with a warning ("the voice pool is full of persistent sounds"). Raise **Max Virtual Voices** on the Audio Manager so the persistent sounds fit with room to spare: 8 cars of 14 layers need 112 voices.

Persistent sounds also get Unity's highest `AudioSource.priority`, so if more voices than Unity's own limit ever play, Unity silences other sounds first (Pepper warns at startup when its counts exceed Unity's, §9.1).

#### 5.4 Cooldown System

**Purpose:** Prevent rapid re-triggering of the same event.

Each event asset keeps the time it was last posted. Every public post entry point (`Post` and the `PostMultiPosition` overloads) checks it first:

```csharp
// On AudioEvent. lastPlayTime starts at -infinity, so the first post is always allowed.
// Now is Time.time, or Time.unscaledTime when the event's Time is Unscaled.
private bool IsOnCooldown() => Cooldown > 0f && Now - lastPlayTime < Cooldown;
```

- A post rejected by cooldown returns a handle that has already finished, so `OnFinished` still fires.
- `lastPlayTime` is stamped after the post runs.
- The cooldown runs on the event's **Time** (Voice Settings). **Scaled** (the default) is game time, so it freezes while the game is paused (`timeScale` 0): right for gameplay sounds. **Unscaled** is real time: use it for UI and pause-menu sounds, or a button with a cooldown plays once per pause. The same setting covers the event's Delay actions, its sounds' Sequence gaps and its crossfades (§10.3).
- The cooldown belongs to the event, so it applies across every object that posts it, not per object.
- **Cooldown Per Emitter** is a second one per Audio Emitter: one car's posts stay that far apart while another car's still play. Both apply when both are set; same clock.

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

There is nothing to switch on in the event. Any event can be posted at several positions by calling one of the `PostMultiPosition` overloads instead of `Post`:

```csharp
public AudioMultiHandle PostMultiPosition(Vector3[] positions, GameObject source = null)
public AudioMultiHandle PostMultiPosition(Transform[] transforms, GameObject source = null)
public AudioMultiHandle PostMultiPosition(AudioMultiPositionEmitterParent emitterParent, GameObject source = null)
```

The array overloads play in **synchronized** mode. The `AudioMultiPositionEmitterParent` overload uses the mode set on that component instead, SingleEmitter or Decorrelated (see §8).

##### How It Works

- At most 16 positions are used; extra ones are ignored with a warning.
- The cooldown is checked once for the whole post.
- **Synchronized mode:** every Play action starts once per position. All of those voices, and all the layers of a multi-voice container, are rescheduled to one shared start time, `AudioSettings.dspTime + 0.1`, so they begin on the same sample.
- **Max Instances** is multiplied by the number of positions.
- **`source`** (optional): on a 3D container, each voice still plays at its own position, attached to `source`. The voices move with it, and stop if it is destroyed. Without a source they stay where they were posted. To follow moving transforms, call `RefreshPositions` on the handle.
- The returned `AudioMultiHandle` controls every voice at once, or one emitter at a time by its index (`SetVoiceVolumeScale`, `StopVoice` and the others).

**Key Insight:** All voices use `PlayScheduled` with one shared DSP time for sample-accurate sync. That adds 100 ms of latency.

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
float VolumeDb { get; }                  // Volume in decibels (the asset's; SetVolume(dB) changes it)
float VolumeMultiplier { get; }          // Linear multiplier (0-1)
float StateOffsetDb { get; }             // What Offset states add now (§7), runtime only
float PlayerVolumeDb { get; set; }       // The player's volume (§6.8), runtime only
int MaxRealVoices { get; set; }          // Its voice cap (§6.9), 0 = none
int RealVoiceCount { get; }              // Real voices of its sounds (and its children's) now
bool Mute { get; }                       // Mute this bus (SetMute)
bool IsMuted { get; }                    // Muted by itself or a parent
bool Solo { get; }                       // Solo this bus and its child buses (SetSolo; §6.3)
```

A bus's level is **Volume + state offset + player volume**, in dB, times its multiplier, its ducking and its curves, times its parent's. Only Volume is in the asset.

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
public float GetFinalVolume()
{
    float gain = ChainGain(out bool soloed);   // this bus's level times its parents', one walk up the chain
    return anySoloed && !soloed ? 0f : gain;   // while any bus is soloed: heard only if it or a parent is
}

private float ChainGain(out bool soloed)
{
    soloed = solo;
    if (mute) return 0f;                       // mute wins, here or on any parent

    float parent = 1f;
    if (parentBus != null)
    {
        parent = parentBus.ChainGain(out bool parentSoloed);
        soloed |= parentSoloed;
    }
    // The asset's Volume, the states' offset and the player's volume (§6.8), in dB: all three reach child buses.
    float linearDb = Mathf.Pow(10f, (volumeDb + stateOffsetDb + playerVolumeDb) / 20f);
    return volumeMultiplier * linearDb * duckingValue * curveGain * parent;
}
```

**Mute and solo.** Mute silences a bus and every bus under it. Solo works as on a mixing desk: while any bus is soloed, the soloed buses and their child buses are heard, through their parents, whose volume, mute and ducking still apply; every other bus is silenced. **A parent of a soloed bus only passes the soloed bus through: sounds posted straight to that parent are silenced while the solo is on**, as a desk mutes the other channels into a group. To hear a parent's own sounds, solo the parent. Mute still wins: a muted soloed bus, or a muted parent of one, is silent. With no solo on, the check costs nothing.

```
Master
├── SFX
│   ├── Engines   ← soloed: heard, through SFX and Master
│   │   └── Turbo     heard (a child of a soloed bus)
│   └── Tyres         silenced
└── Music             silenced
Sounds posted straight to SFX or Master: silenced.
```

Solo is for authoring: the Mixer window's and the Audio Profiler's **S** buttons set it (Alt-click in the Mixer solos one bus and clears the rest), and **Clear Solos** turns them all off. A solo change reaches every sound already playing, paused and virtual ones too.

#### 6.4 Ducking System

**What:** Lower the volume of target buses for a while when this bus's ducking is triggered.

**Use Case:** Music ducks when dialogue plays.

Ducking is **triggered**, not driven by what the bus is playing: an event's **TriggerDucking** action (its Fade Duration is the hold time) or a call to `bus.TriggerDucking(holdSeconds)` starts it (or `targetBus.ApplyDucking(dB, attack, hold, release)` from code). Each target then fades down over the attack, holds, and fades back up over the release. The duck runs on the **target** bus's **Ducking Time**: Scaled (the default, game time, so a duck freezes part way while the game is paused) or Unscaled (real time, so it completes during a pause).

**Ducks combine: the deepest wins.** Each trigger is a duck of its own, and while several run on one bus it follows the deepest at each moment, so none cuts another short: nitro pressed twice gives one continuous duck released after the second press's hold, and a short UI duck during a dialogue duck leaves the dialogue duck as it was. A new duck starts from the level the bus is at, so re-triggering the same duck sounds exactly as one envelope restarting did. (Before 3.1.0-dev.7 the last trigger replaced the duck in progress, so a short or shallow one could cut a long or deep one short.)

##### Configuration

```csharp
bool EnableDucking { get; }                    // Enable ducking feature
List<DuckingTarget> DuckingTargets { get; }    // Buses to duck
float DuckingAttack { get; }                    // Fade down time (default 0.05s)
float DuckingRelease { get; }                   // Fade up time (default 0.2s)
```

##### DuckingTarget Structure

```csharp
[Serializable]
public class DuckingTarget
{
    public AudioBus targetBus;        // Bus to duck
    public float duckAmount = -6f;    // Attenuation in dB, -80..0 (-6 dB is about half the level)
}
```

##### Example Setup

```
Dialogue Bus:
  - enableDucking: true
  - duckingTargets:
      - Music Bus, duckAmount: -10.5 dB (about 30% of the level)
      - SFX Bus, duckAmount: -6 dB (about 50%)
  - duckingAttack: 0.05s (quick duck)
  - duckingRelease: 0.8s (slow return)
Dialogue event: Play the line, plus a TriggerDucking action with Fade Duration = the line's length (the hold)
```

**Result:** When the dialogue event fires, music quickly drops to about 30% of its level and SFX to about 50%. After the hold they return over 0.8s.

#### 6.5 Unity AudioMixer Integration

**Setup:**
1. Create AudioMixerGroup in Unity's AudioMixer window
2. Assign to bus's `mixerGroup` field
3. System routes voices through mixer automatically

**Benefits:**
- Use Unity's built-in effects (reverb, EQ, etc.)
- Visual mixing interface
- Snapshots and ducking (separate from SFX System ducking)

#### 6.6 Parameter Curves on a Bus

A bus's **Parameter Curves** move its mix with a parameter, without code: nitro's punch sweeping the world's low-pass, a split-screen mode taking 3 dB off the hero bus. They are the same curves as a container's (§4.1), with three properties:

| Property | Unit | How it applies |
|---|---|---|
| **Volume** | dB | Multiplies into the bus's level, like ducking: every sound on the bus and on its child buses follows. The bus's own Volume setting is untouched. |
| **Low Pass** | Hz | A Lowpass effect on the bus's Mixer Group: one filter for everything routed through that group (reverb tails and ambience included), however many sounds. |
| **High Pass** | Hz | The same with a Highpass effect. |

- Curves read the parameter's **global** value (`AudioManager.SetParameter`), after its smoothing, every frame. A bus is shared by every listening point, so an emitter's or a sound's own value doesn't move it. A move tied to one car (the hero's engine pulling back as it speeds up) is a curve on that car's sound instead.
- **Filters need a Mixer Group and an exposed cutoff.** In the bus inspector, **Add Low Pass to Mixer Group** (and **Add High Pass**) adds a Lowpass Simple (Highpass Simple) effect to the group, exposes its cutoff as `<Bus>_LowPass` and fills in Low Pass Parameter. Or do it by hand: add the effect, right-click its Cutoff freq, Expose it, and type its exposed name. Without one, the curve logs a warning once and does nothing.
- Pepper eases the cutoff over about 20 ms (in octaves) and writes it to the mixer only when it moves. Pitch curves are ignored on a bus.

```
Example: nitro's punch on the other cars
  Parameter asset: NitroPunch (0-1, global)
  Others bus: Mixer Group "Others"; Add Low Pass to Mixer Group
    Parameter Curve: NitroPunch → Low Pass, 0 → 18000 Hz, 1 → 2000 Hz
  Game code: AudioManager.Instance.RampParameter(nitroPunch, 1f, 0.3f), then back to 0
```

#### 6.7 Pausing a Bus

For a game's pause, pause the buses the world plays on, and leave the pause menu's buses alone:

```csharp
// The game pauses (time scale 0): the cars and the world hold, the pause menu's clicks and music play on.
worldBus.Pause();      // its child buses (Vehicles, Ambience...) pause with it
// ... and on resume:
worldBus.Resume();
```

- **Pausing fades out.** Every sound on the bus and its child buses fades out over about 25 ms and holds its place, keeping no real voice (so the pause menu's sounds get them). `Pause` on its own would be a click.
- **Resuming carries on from where it was,** fading back in, every layer of an engine in the same frame, so they stay in phase.
- **Sounds posted to a paused bus** follow its **While Paused** setting. **Drop One-shots** (the default): a sound that doesn't loop is dropped before its first sample (a crash under the pause menu shouldn't play later), and a loop starts paused and plays on resume. **Hold**: every sound starts paused and plays on resume.
- **A child bus is paused while it or any parent is paused.** `IsPaused` says so; `Resume` on a child doesn't override its parent's pause.
- **It is a separate pause** from `AudioHandle.Pause` and `AudioManager.PauseAll`, which pause the sound at once, as before. A sound plays again only when none of them pauses it.
- **Timing during a pause:** set the pause menu's events to Time = Unscaled (§5.4) and the buses it ducks to Ducking Time = Unscaled (§6.4), so their cooldowns, delays and ducks run while the time scale is 0.
- A paused sound that isn't Persistent can be stolen first if the pool runs short during the pause, since it is silent; a Persistent one never is.

#### 6.8 Player Volume

A Settings page's sliders (master, music, effects) set the **player's volume** on each bus, not its Volume:

```csharp
musicBus.PlayerVolumeDb = AudioExtensions.LinearToDb(musicSlider.value);   // -80 … +24 dB, 0 = as mixed
AudioManager.Instance.MasterVolumeDb = -3f;                                  // or SetMasterVolume(linear)
bool muted = AudioManager.Instance.IsAllMuted;                              // MuteAll's getter
```

- It is a level of its own, on top of the bus's Volume and any state's offset (§7), so a mix state never undoes it and it never undoes one.
- **It is never saved to the asset,** and each play session starts at 0 dB: the game saves the player's settings itself and sets them again at boot. (A bus's Volume, which `SetVolume` and `SetBusVolume` change, is the asset's: in the editor a change made in Play mode stays in it.)
- `AudioManager.GetBus(name)` finds a bus by name. An unknown name returns the master bus, as before, but now warns once; `SetBusVolume` with an unknown name warns once and changes nothing (it used to set the master bus).
- `Example08_AudioSettingsUI` is a complete settings page.

#### 6.9 A Bus's Voice Cap

A bus can cap how many real voices its sounds use at once: **Max Real Voices** in its Voices section (0, the default, is no cap). A racing game caps the bus its other cars play on, so a pack of them can't crowd out your own car:

```csharp
othersBus.MaxRealVoices = 12;
int now = othersBus.RealVoiceCount;   // real voices of its sounds and its child buses' now
```

- **It counts** the real voices of the sounds whose Target Bus is this bus or one of its children.
- **Over it, the least important of those sounds go virtual,** a whole sound at a time, exactly as over the Audio Manager's Max Real Voices (§3.2, Voice Virtualization): when a sound starts, and in the voice pass, with the same stickiness. They keep their place and come back in step when there is room. A single sound with more voices than the cap keeps its most important ones.
- **It never ends a sound,** so it frees no voices in the pool: that is still the pool's business (Real + Virtual + Steal Headroom).
- The Audio Manager's Max Real Voices still applies on top. The bus inspector shows the live count in Play mode.

---

### 7. State System

#### 7.1 AudioState Overview

**Location:** `Events/AudioState.cs`

**Purpose:** Change multiple audio parameters based on game mode.

##### Properties

```csharp
string StateName { get; }                         // Unique identifier
string StateGroup { get; }                        // Mutually exclusive group
IReadOnlyList<BusVolumeProperty> BusVolumes { get; }      // Bus volume changes
IReadOnlyList<SwitchProperty> SwitchValues { get; }       // Switch changes
IReadOnlyList<RTPCProperty> RtpcValues { get; }           // RTPC changes
IReadOnlyList<EffectProperty> EffectProperties { get; }   // Mixer effect changes
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
- Setting "Underwater" makes it the group's active state in place of "Normal" or "Cave"
- Different groups can have active states simultaneously
- Setting a state **applies its own values only**. Nothing from the previous state is undone, so give each state in a group a value for every bus, switch and RTPC its siblings change ("Normal" sets the SFX bus back to 0 dB, for example). The exception is a state whose **Bus Volume Mode** is **Offset** (below): its bus offsets go back when it ends.
- State names are unique across all groups
- Setting the state that is already active does nothing
- `AudioManager.ClearState(group, transitionTime)` leaves a group's state without setting another: an Offset state's offsets go back; nothing else is undone

##### Bus Volume Mode: Set Level or Offset

| Bus Volume Mode | The dB values mean | Leaving the state |
|---|---|---|
| **Set Level** (default, as before 3.1) | Each bus's level: written into its Volume, the same value the bus inspector and `SetBusVolume` set (so in the editor it stays in the asset after Play) | Nothing is put back |
| **Offset** | How far each bus moves, in a slot of its own on top of its Volume and the player's volume (§6.8). The asset is never changed. | The offsets go back, over the next state's (or `ClearState`'s) transition time |

Use **Offset** for mix states, so they never undo a Settings page:

```
State Group "Mode":
  Mix_Single      (Offset, no bus volumes)
  Mix_LocalSplit  (Offset: Hero -3 dB, Speed -3 dB)
AudioManager.Instance.SetState("Mix_LocalSplit", 0.5f);   // Hero moves -3 dB over 0.5 s
AudioManager.Instance.SetState("Mix_Single", 0.5f);       // and back
```

- Offset states in different groups add up on a bus (Split −3 dB and Nitro +2 dB: −1 dB); leaving one takes only its part away.
- The offset moves in a straight line in dB over the transition time, on unscaled time, in Pepper's per-frame step. A bus shows it as `StateOffsetDb` (and in its inspector in Play mode).
- A state's switches, RTPCs and effect properties work the same in both modes.

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
RtpcValues:
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
    if (!states.TryGetValue(stateName, out var nextState)) return;   // logs a warning
    if (activeStatesByGroup.TryGetValue(nextState.StateGroup, out var current) && current == nextState)
        return;                                                       // already active

    activeStatesByGroup[nextState.StateGroup] = nextState;
    nextState.Apply(transitionTime);
}
```

**Apply Implementation** (simplified):
```csharp
public void Apply(float transitionTime)
{
    var manager = AudioManager.Instance;

    // Bus volumes (Set Level): transition over transitionTime, or set at once when it is 0.
    // (An Offset state's bus volumes are moved by SetState and ClearState, which know the group.)
    foreach (var busVol in BusVolumes)
    {
        if (transitionTime > 0f) manager.TransitionBusVolume(busVol.bus, busVol.volumeDb, transitionTime);
        else busVol.bus.SetVolume(busVol.volumeDb);
    }

    // Switches: always immediate (switches don't transition)
    foreach (var sw in SwitchValues)
        manager.SetSwitch(sw.switchGroup, sw.switchValue);

    // RTPCs: transition over transitionTime too, or set at once when it is 0
    foreach (var rtpc in RtpcValues)
    {
        if (transitionTime > 0f) manager.TransitionRTPC(rtpc.parameterName, rtpc.value, transitionTime);
        else manager.SetRTPC(rtpc.parameterName, rtpc.value);
    }

    // Mixer effect properties: transition over transitionTime
    foreach (var effect in EffectProperties)
        manager.SetBusEffectProperty(effect.bus, effect.propertyName, effect.value, transitionTime);
}
```

> **Two quick state changes and RTPCs:** a newer RTPC transition on the same parameter replaces the one running (`TransitionRTPC`, §3.4), so changing state again before the first transition ends moves the value from where it is to the new state's value. Bus volume transitions replace each other the same way.
>
> **Effect properties** are found by name: the mixer must expose a parameter called `<BusName>_<Property>`, for example `SFX_LowpassCutoff`.

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

RtpcValues:
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

RtpcValues:
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
bool useDirectionality = false;          // Draws a cone gizmo in the Scene view (no effect on sound)
float coneAngle = 90f;                   // That gizmo's cone angle (0-360)
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
2. Add action: Play music container
   (No multi-position setting is needed on the event: Step 4 posts it with PostMultiPosition.)
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

**Directionality (Scene view only):**
```csharp
emitterChild.UseDirectionality = true;
emitterChild.ConeAngle = 90f;  // draws a 90° cone along transform.forward
```
These two only draw a cone gizmo to help you aim an emitter. Playback doesn't read them: every active emitter is heard the same in every direction. To make one quieter, use its Volume Multiplier.

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
- Virtualization works per-voice
- Under voice pressure, distant emitters are virtualized first (importance falls with distance)
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
1. Over `maxRealVoices`, or over a bus's own Max Real Voices (§6.9): the least important **plays** go virtual (a play is a whole sound, all of an engine's layers), keeping their place in the clip (§3.2, Voice Virtualization).
2. Over Real + Virtual: an event makes room for its whole new play by ending whole plays, silent virtual ones first, then the least important. Persistent ones (§5.3) are never ended. If there isn't room even then, the new sound is skipped and nothing is stolen. A stolen sound fades out over Steal Fade Seconds (§3.2). `VoiceStealBehavior` only applies to an event's own Max Instances.
3. Critical priority makes a sound the last to go, not exempt: only Persistent plays are never stolen

**Unity's own limits.** Unity has its own Max Real Voices and Max Virtual Voices (Project Settings > Audio; 32 and 512 by default). If Pepper's Max Real Voices is higher than Unity's, or Real + Virtual + Steal Headroom is higher than Unity's virtual count, Pepper warns at startup and the Audio Manager inspector shows the numbers side by side. Over its own real limit, Unity silences sources itself, behind Pepper's back; Pepper sets `AudioSource.priority` from each event's priority (persistent sounds highest) so Unity spares the important ones.

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
    Low = 0,       // Weighs 1/8: distant ambience
    Medium = 64,   // Weighs 1/4: standard gameplay
    High = 128,    // Weighs 1/2: player actions
    Critical = 255 // Weighs 1, the strongest (still stealable; Persistent is the exemption): UI, dialogue
}
```
A sound's importance is how loud it is where it's heard times its priority's weight (§3.2), so a near, loud Low sound can outrank a far, quiet Medium one.

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
1. Voices beyond the `maxRealVoices` limit, or a bus's Max Real Voices, least important first (distance × volume × the priority's weight)
2. Sounds past their Attenuation asset's Max Distance (in Spatial Mode = Pepper, from every listening point)
3. Nothing else: there is no other distance threshold, and under the limits every voice plays

**How It Works:**
```csharp
// Becoming virtual (the voice budget does this itself; you can too)
voice.MakeVirtual();
// - Fades out over about 25 ms if it can be heard, then pauses the AudioSource
// - Keeps its place in the clip, in samples, at its pitch
// - No audio output, minimal CPU cost
// - Called from your code: it stays virtual until MakeReal(); the budget won't bring it back

// Becoming real again
voice.MakeReal();
// - Moves the AudioSource to where it would have been, then resumes it
// - Fades back in over about 25 ms
// - Handles looping correctly
```
A voice comes back in phase at any pitch, Pepper's Doppler included, except for Unity's own Doppler, which Pepper can't see: give looping layers that must stay in step Doppler = Pepper, or a Doppler Level of 0 (§3.2, Voice Virtualization).

**Monitoring:**
```csharp
void OnGUI()
{
    var stats = AudioManager.Instance.GetStatistics();
    GUI.Label(new Rect(10, 10, 400, 20),
        $"Real: {stats.activeVoices}/{stats.realVoiceBudget}, " +
        $"Free in the pool: {stats.availableVoices} of {stats.poolSize}");
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
Voices: Fixed pool of Max Real + Max Virtual + Steal Headroom AudioSources (100 by default), built at start
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

##### Distance and Virtualization

There are no fixed distance tiers. The `lodDistances` field on the AudioManager is not used in this version and is hidden in the inspector. Distance matters only through a voice's importance score: for a 3D sound it falls from 1 at the listener to 0 at the sound's own Max Distance (with an Attenuation asset, its curve's level there; with Spatial Mode = Pepper, the level it is heard at; 2D sounds don't lose importance with distance). When more than `maxRealVoices` are live, the lowest scores are virtualized first, so distant sounds give way before nearby ones. While you are under the limit, every sound plays regardless of distance.

To make a sound more or less likely to be virtualized, change its event's Priority or its container's Max Distance.

##### Listening Points

Everything in Pepper that depends on where the player is listening (voice importance, occlusion rays, the Furthest steal, which multi-position emitter is nearest, and with Spatial Mode = Pepper each 3D sound's level and pan) reads the **listening points**.

**Register one** (recommended). Tell Pepper where you listen from, usually the camera, and it never searches the scene:

```csharp
// Follow a transform: the point reads its pose every frame.
AudioListeningPoint point = AudioManager.Instance.AddListeningPoint(cameraTransform);

// Or move it yourself, in Update or LateUpdate:
AudioListeningPoint point = AudioManager.Instance.AddListeningPoint();
point.SetPose(rigPosition, rigRotation);

// When that camera goes away:
AudioManager.Instance.RemoveListeningPoint(point);
```

- Pepper's per-frame step runs after every LateUpdate, so a camera rig that moves in LateUpdate (Cinemachine) is already in place.
- The point's **velocity** (for Approach Speed and Doppler) is measured from how it moves each frame, unless you give it with `SetVelocity`. `ClearVelocity()` goes back to measuring, starting again from where the point is: call it on a camera cut too, so the cut isn't read as speed. Pointing `Follow` at another camera does that by itself.
- **If the transform a point follows is destroyed,** the point is removed (`IsRegistered` turns false), with a warning once a session. Remove your points when their camera goes; the warning says you didn't. (It used to stay where it last saw the camera, still in use.)
- **Unity's AudioListener** still does Unity's own 3D panning. If it isn't on the transform the point follows, `AudioManager.Instance.DriveUnityListener(listener)` moves it to the primary point every frame, so both hear from the same place.

**The primary point.** With several points, one is primary: the one `DriveUnityListener` keeps Unity's AudioListener on, the one the ambient system (§13) and SingleEmitter posts listen from, and with Spatial Mode = Unity the only one Pepper listens from. It is the first registered until you pick one:

```csharp
AudioManager.Instance.SetPrimaryListeningPoint(carViewPoint);   // menu camera → car view
bool isIt = carViewPoint.IsPrimary;
AudioManager.Instance.SetPrimaryListeningPoint(null);           // back to the first registered
```

Changing it doesn't make sounds jump: what Pepper measures from a point (Distance, Approach Speed and Angle, so Doppler and every curve on them, and attenuation filters) glides to the new one over about 100 ms. With Spatial Mode = Unity, Unity's own levels and pans still jump when its AudioListener moves.

**Several points, weighted: Spatial Mode = Pepper.** One AudioListener can't hear two places, so for split screen Pepper pans and attenuates 3D sounds itself. Set the Audio Manager's **Spatial Mode** to **Pepper** (in the inspector, or `AudioManager.Instance.SpatialMode = AudioSpatialMode.Pepper;` before the sounds start: a change applies to sounds started after it). Then every 3D sound plays at Spatial Blend 0 and, each frame, Pepper sets its level and pan from every point:

- **Level:** for each point, the sound's attenuation there (its Attenuation asset's volume curve, else Unity's own rolloff for its Min and Max Distance) times the point's **Weight** (0–1). The loudest wins. So a sound is as loud as from the point that hears it best.
- **Pan:** where it is, left to right, from each point, blended by how loud each point hears it, and narrowed by each point's **Pan Width** (0 = mono, centred; 1 = full stereo). It eases over about 50 ms.
- **At one point of weight 1 and width 1 it sounds exactly as Unity's 3D does** (the same rolloffs, the same constant-power pan, Spread the same way; measured on Unity 6000.0 and 6000.3). So single player can run Pepper mode too, and hear the code split screen runs.
- **Which point a sound is measured from** (for Distance, Approach Speed, Angle, Doppler, attenuation filters, occlusion rays and the Furthest steal): the one that hears it loudest, changing only when another hears it 1 dB louder, so a car halfway between two players doesn't flip. When it changes, those values glide over about 100 ms.
- **Max Distance:** a sound goes virtual only when it is past its Attenuation asset's Max Distance from every point.
- **Importance** (§3.2) is how loud it is heard, so a car near either player counts as near.
- **Doppler** is always Pepper's (§4.1).
- **After the points change** (one added or removed, the primary, a Follow, Weight or Pan Width), levels move to the new ones over about 100 ms instead of jumping.

```csharp
// Local split: both players' cameras, full weight, mono (top/bottom halves have no left and right).
var p1 = AudioManager.Instance.AddListeningPoint(player1Camera);
var p2 = AudioManager.Instance.AddListeningPoint(player2Camera);
p1.PanWidth = p2.PanWidth = 0f;

// Remote split: your camera in stereo, the second view's faintly (about −9 dB) and mono.
var mine = AudioManager.Instance.AddListeningPoint(myCamera);
var second = AudioManager.Instance.AddListeningPoint(secondViewCamera);
second.Weight = 0.35f;
second.PanWidth = 0f;
```

Pepper mode's limits: no spatializer plugins (HRTF); the project's Doppler Factor isn't read; a stereo clip keeps its two channels, balanced by the pan, where Unity's 3D folds it to mono, so give 3D sounds mono clips; up and down count only by shortening the sideways part. AudioSources that aren't Pepper's (the ambient system's) are still Unity's, heard from the AudioListener: keep it on the primary point with `DriveUnityListener`. The Live View (§10.6) shows each voice's point, level and pan.

**With no point registered,** Pepper searches the scene for an AudioListener, as before, and logs a warning once: "No listening point is registered". A listener it found is kept until it is destroyed. While there is none, it searches at most once a frame (it used to search on every call).

**`ListenerUtil.Set` still works:** it registers a point that follows the transform you give it. A point you add with `AddListeningPoint` takes priority over it. `ListenerUtil.Invalidate()` removes it and makes the next lookup search again. If its transform is destroyed, the search takes over, as before.

```csharp
ListenerUtil.Set(newCamera.transform);   // older code: still fine
Vector3 where = ListenerUtil.GetPosition();
```

`ListenerUtil.Get()` returns the primary point's transform. A point moved with `SetPose` has none, so use `ListenerUtil.GetPosition()` for the position.

In Play mode, the Audio Manager's inspector lists the points (weight, pan width, which is primary), or warns when there are none.

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
            $"Free Voices: {stats.availableVoices}");
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
            // The listening point (§9.3), or the scene's AudioListener when none is registered
            if (!ListenerUtil.TryGetPosition(out Vector3 listenerPosition)) { yield return null; continue; }

            foreach (var voice in realVoices)
            {
                if (voice?.source == null) continue;

                Vector3 a = listenerPosition;
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
    { targetBus: Music_Bus, duckAmount: -10.5f },  // dB: about 30% of the level
    { targetBus: SFX_Bus, duckAmount: -6f }        // dB: about 50%
];
duckingAttack = 0.05f;   // Quick duck
duckingRelease = 0.8f;   // Slow return
```

**Behavior:**
```
Duck triggered (TriggerDucking action, hold = its Fade Duration) → Music/SFX duck over 0.05s
Hold elapses → Music/SFX return over 0.8s
```
Ducking is triggered, not detected from the bus's output; see §6.4.

**Common Patterns:**
```
Dialogue ducks Music: 70-80%
Dialogue ducks SFX: 40-60%
Important SFX ducks Music: 30-50%
UI sounds duck nothing: 0%
```

#### 10.3 Crossfading

**Types:**

For both types, the outgoing sound fades along the incoming sound's curve run backwards in time.

**Linear:**
```csharp
float fadeIn  = x;        // Straight line
float fadeOut = 1f - x;
// Results in slight volume dip in middle
```

**Equal-Power (Recommended):**
```csharp
float fadeIn  = Mathf.Sin(x * Mathf.PI * 0.5f);
float fadeOut = Mathf.Cos(x * Mathf.PI * 0.5f);   // fadeIn² + fadeOut² = 1
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
  - Target Bus: Music_Bus
  - Duration: 3.0s
  - Type: EqualPower
```

Set the action's **Target Bus** to route the incoming music through a bus, the same as a Play action, so it follows that bus's volume, mute and ducking. That includes the clips an auto-advancing Sequence starts later. With no Target Bus (and always when you call `AudioManager.CrossFade` directly), the incoming voices play unbussed, through the container's own Mixer Group. A 3D container plays at the position, or on the GameObject, you pass to `Post`. The incoming voices are not attached to the CrossFade event's handle: that handle finishes straight away. Stop the new music with `container.Stop()`, `StopAllSounds()`, or another CrossFade.

A container crossfade fades the voices of the "from" container that are playing at that moment. It doesn't cancel anything the outgoing sound was still going to start: a delayed Play action (the loop in the intro + loop recipe) or a Sequence's next clip will still come in, at full volume. When you hold the outgoing sound's handle, use a handle crossfade instead.

Changing your mind mid-crossfade is smooth: crossfading back while a sound is still fading in fades it out from the level it had reached. Note that the target always starts again from the top; a crossfade never resumes a sound that is fading out.

**Handle crossfade (recommended for music started by an event):**
```csharp
AudioHandle music = startMainTheme.Post();           // intro now, loop after a delay

// Later — fade the whole main theme out, including the loop it hasn't started yet,
// and fade the combat event in:
music = AudioManager.Instance.CrossFade(music, combatMusicEvent, 2f);
```

`CrossFade(AudioHandle from, AudioEvent to, float duration, CrossfadeType type = EqualPower, GameObject source = null, Vector3 position = default)` fades out every voice of `from` and cancels what it was still going to start (delayed Play actions, a Sequence's next clip). It then posts `to` normally, so its bus, priority, cooldown and Max Instances apply, and fades in the voices that post started. It returns the new handle. The old handle's `OnFinished` fires when its fade-out ends. `OnStopping` doesn't fire, as for every crossfade. Either side may be null. Voices `to` starts later (its own delayed Plays, a Sequence's next clip) come in at their normal level. If `to` is the same event as `from` and its Max Instances is reached, the instance limit ends the old play at once instead of fading it.

**Crossfades and the time scale.** A crossfade runs on its event's **Time** (§5.4): the CrossFade action's own event, or for a handle crossfade `to`'s event (else `from`'s). Scaled, the default, is game time, so a crossfade started while the game is paused (time scale 0) waits for the game to resume. Set the pause menu's music event to Unscaled and its crossfade completes during the pause. A container crossfade called from code has no event and runs on game time.

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
| `Audio Event` | The AudioEvent asset posted on every beat (drag it in; there's no name lookup). From code: the `Event` property. |
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

Drag the `Heartbeat_Thump` event asset into the BeatScheduler's `Audio Event` (a normal AudioEvent with a `RandomContainer` of 2–3 thump variations). No other plumbing required.

##### Common patterns

**Quantize gameplay to beats.** Subscribe to `OnBeat` to drive visual pulses, damage ticks, flashlight flicker, etc. The event fires on the main thread so Unity API calls are safe.

**Pause on gameplay pause.** Toggle the component's `enabled` or call `StopScheduling()` / `StartScheduling()`. The phase resets on restart unless you snapshot `BeatInterval` yourself.

**Multiple tempos simultaneously.** Add multiple `BeatScheduler` components to a GameObject, each pointing at a different event. They run independently and don't share a phase.

##### Limitations

- **Sample-accurate spacing, frame-bounded latency.** Each beat is scheduled to its grid DSP time via `PlayScheduled`, so spacing between consecutive beats is exactly `BeatInterval` regardless of frame-rate jitter. The audible onset of each beat can lag its grid time by up to one DSP buffer (Unity rounds past-time `PlayScheduled` to the next buffer boundary), but that lag is approximately constant across beats and doesn't affect perceived tempo. Suitable for rhythmic gameplay SFX and single-track musical-grid scheduling; for sample-aligned multi-track musical content, schedule `PlayScheduled` directly with explicit DSP times.
- **No musical phase alignment across schedulers.** Two schedulers at the same BPM are not guaranteed to fire on the same DSP frame.
- **Does not track segment/bar/measure structure.** It emits an unbroken stream of beats. For song-form authoring (intro / loop / outro), use an interactive-music layer above the SFX system.

#### 10.5 Audition (outside Play mode)

**Window > PepperSFX > Authoring > Audition** plays a container tree, or an event, while you edit it, so an engine can be tuned by ear without driving the car.

1. Select a container or an event in the Project window (with **Follow Selection** on, the window picks it up), or drag one into **Sound**. Press **Play**.
2. Move the sliders: one for every parameter the tree reads (Blends, pitch follow, Parameter Curves on containers and layers, Switch Group drivers), and for each one's **Bypass Slew While** parameter (an engine cut), in each parameter's range and unit. Values go through each parameter's Smoothing, as in the game; a "heard" line shows where the smoothing has got to. A slider shows the value written: where a sweep or a replay has it, and where a sweep ended.
3. Pick switch values from the dropdowns. A Switch set to Next Play Only plays again to pick up the new value; one set to Crossfade or Restart changes as it would in the game.
4. **Sweeps:** press **Add Sweep** for each parameter to move, and set its range, its seconds and how it carries on: **Once** (stops at the end), **Repeat** (starts again from the start) or **Back And Forth**. Any number run at once, one per parameter (RPM 900 → 7000 over 4 s while load goes 0 → 1 over 3 s). Moving a slider stops only that parameter's sweep.
5. Watch **Layers**: each sound playing, its level (dB), pitch (semitones), low-pass, whether it is real or virtual (silent, keeping its place), and for an event the bus it plays through. Keep the Inspector open beside the window: edits to curves, regions and root values are heard at once, and a curve or parameter added there gets its slider without selecting the sound again.

- **An event** plays its Play actions as a post would: each through its Target Bus (the bus's volume and mute), with the event's priority and Persistent flag, after its Delay. Its Set RTPC and Set Switch actions set the audition's values. Its other actions (Stop, Pause, Resume, states, ducking, crossfades) aren't auditioned; the window says how many it skipped. Max Instances and cooldowns don't apply, and the event itself is never changed.
- **Replay a Recording** (§10.6): pick a recording and press **Replay**. Every recorded value moves at once, as the game wrote it, through each parameter's Smoothing, so a smoothing change is heard on the replay; the recorded switch values are set as the game set them. **Loop** goes round (all of it, or the range you set) and **Time** scrubs: the values where you drag to are heard at once. Moving a recorded parameter's slider takes that one parameter off the recording; **Replay** again puts it back.
- **Distance** and **Angle** place the sound around the listener: they drive Distance, Approach Speed and Angle parameters and an Attenuation asset's filters.
- **Heard 2D.** Outside Play mode there may be no listener for Unity to pan and fade from, so a 3D sound is heard straight, at full level; check the distance fade and panning in Play mode.
- **Outside Play mode** a Sequence plays only its first clip, and Stop is immediate (both run on Unity coroutines, which don't advance there).
- **Generated engines** (Granular and Additive Engines, Unity 6.3 and later, §4.7) play here too: their Rpm and Load get sliders.
- The audition runs on its own hidden Audio Manager. It stops when you enter Play mode, when scripts reload and when the window closes, and it adds nothing to your scene.
- From code (editor tools): `AudioAudition.Begin(out whyNot)`, then `Play(container)` or `Play(audioEvent)`, `SetParameter`, `Sweep(parameter, from, to, seconds, SweepMode)`, `SetSwitch`, `Replay(recording)`, `Tick(dt)` every editor update, `GetLayers`, and `Dispose()`.

#### 10.6 Live View and recording a drive (Play mode)

**Window > PepperSFX > Debug > Live View** shows, while the game runs, what each Audio Emitter (each car) is doing:

- **Its values:** every parameter set on the emitter, as written and as heard after smoothing, with "moving" while it is still on its way; and its switch values.
- **Its sounds:** each play posted on it, with its event and container, bus, whether it is Persistent, and whether it is real or virtual; the play's own values (set on its handle, such as each axle's slide speed); and a row per voice: its level (dB), pitch (semitones), its **region** for a layer of a Regions Blend ("1780–2510 RPM at 2300: in", "fading" or "out"), and, when virtual, **why**: over the cap, silent layer, past Max Distance, held by code (`MakeVirtual`), bus paused, emitter paused, or stopping (", fading" while it fades down).
- **No emitter** lists the sounds posted without one.

The Voice Pool Debug window's "Why virtual" column gives the same reasons.

**Recording.** Choose an emitter and press **Record**; drive; press **Stop and Save** and pick where the recording goes. It keeps what the game wrote to that emitter, every value (Bypass While ones included) and its switch values, from each frame, and only when they change. Replay it in the Audition window (§10.5) to tune a mix on a loop of a real lap. It doesn't keep values set on one sound's handle (a tyre's per-axle values) or global values. A recording still running when Play mode ends is offered for saving; one whose emitter is destroyed (a car reset or removed) stops there, and the Live View offers it for **Save** or **Discard**.

The **Gain dB** column (here and in the Audition window) is Pepper's gain to the AudioSource: volume, curves, bus, offsets, ducking and, in Pepper spatial mode, distance. Unity's own 3D distance fade (Spatial Mode Unity) and the mixer come after it, so it isn't what you hear; it reads -inf while a voice is parked virtual.

From code: `AudioEmitterRecorder.Begin(emitter, out whyNot)` and `End()` give an `AudioParameterRecording`; `AudioManager.GetPlaysDebug(list)` and `AudioEmitter.AllEnabled` are what the Live View reads, for a game's own debug overlay.

#### 10.7 Loop Analysis

**Window > PepperSFX > Engines > Loop Analysis** measures engine loops (any loop, really). Select clips, or containers or a Blend playing them, press **Use Selection**, then **Analyse**:

- **Root:** the loop's fundamental, and the RPM it means for the **Cylinders** you set: a four-stroke engine fires each cylinder once every two turns, so RPM = fundamental × 120 ÷ cylinders (an inline-4 idling at 900 RPM sounds at 30 Hz, a V8 at 60 Hz). If a recording fools it by an octave, **÷2** and **×2** fix the reading. When the clip's name holds an RPM (the last number between 300 and 20000, as the Engine Builder reads it), a reading more than half an octave from it is flagged under the row: the pitch read an octave off, or a name that isn't the loop's RPM.
- **Cylinders is a scale.** Start from the engine's count, but go by the loops: their strongest pitch isn't always the firing rate, and a library tuned to notes has a scale of its own (one tuned to notes at RPM ÷ 20 Hz reads on 6, whatever the engine). When most readings sit 2 semitones or more from the RPMs in their names, the window names the count that fits, with a **Use N** button: a V8 library read at 6 is 5 semitones high, and gets 8. Cylinders is one setting for every engine, so set it again for each library: libraries of different engines, or tuned differently, read on different counts.
- **Loop Point:** whether the loop clicks where it goes round: the first sample against where the end was heading, compared with how much the sound moves sample to sample. "Clean", or "clicks".
- **Level Across It:** the first 50 ms against the last 50 ms; more than 3 dB is a jump to re-cut or crossfade.
- **Level:** RMS and peak in dBFS, and integrated loudness in LUFS (ITU-R BS.1770; a mono clip counts as one channel).
- **Seam ▶** plays the end and the start of the loop round and round, so you hear only the seam.

Clips must be **Decompress On Load** to be read (the right setting for loop layers anyway).

On a Regions Blend's Inspector, **Detect Root Values** measures each layer's clip and sets its Root Value (with Undo), then logs what it found; set **Cylinders** beside it. It writes the raw reading: the **÷2** and **×2** buttons here only change what this window shows, so if a loop reads an octave off, correct its Root Value by hand (halve or double it). The log is a warning when a reading is far from its clip's name, or when most are (it then names the cylinder count that fits).

> **Loops tuned to notes?** If each loop is pitch-corrected to one note, the most exact roots come from the notes, not from a measurement: Root = the note's frequency × 120 ÷ Cylinders (× 20 at 6). A loop named 2500 tuned to B2 (123.47 Hz) has its root at 2469.4, not 2500; neighbours rooted by their names can sit tens of cents apart in a crossfade and beat.

**Auto Layout** puts the layers in Root Value order. Sounds already playing keep the order they started with, so while a sound plays (in Play mode, or in the Audition window) it asks before re-ordering: until they are posted again, layers would play with each other's regions and pitch. Laying out regions without a new order is heard at once.

#### 10.8 Engine Builder

> **Full version only.** In the free version, build the same tree by hand as the Vehicle Engine RPM recipe describes
> (§3); Loop Analysis (§10.7) and the Blend inspector's **Detect Root Values** are in both.

**Window > PepperSFX > Engines > Engine Builder** turns a car's engine loops into the whole tree the engine recipe describes (§3, Vehicle Engine RPM):

1. Select the loops and press **Use Selection**. Each row guesses its set (**OffLoad** when its name says off, decel, coast or overrun; else **OnLoad**) and its RPM (the last number between 300 and 20000 in its name: `Eng_OffLoad_2510`, `v8_on_4960rpm`). Correct them, or press **Detect** (§10.7), which uses the **Cylinders** above the rows (the same setting as Loop Analysis's: a scale, see §10.7). Changing Cylinders re-scales the rows Detect measured and keeps the ones you typed. A row more than half an octave from the RPM in its name is flagged under it; when most rows sit 2 semitones or more from their names, the window names the cylinder count that fits instead, with a **Use N** button.
2. Pick the **RPM** and **Load** parameters, or leave them empty to have EngineRpm (0–9000 RPM, about 15 ms of smoothing) and EngineLoad01 (0–1, up 50 ms, down 150 ms) made. Pick the **Target Bus** and, if you like, an **Attenuation**.
3. Press **Build**. It lists the clips whose import settings it changes, then makes, in the folder you chose:
   - one looping container per loop, with **Uses Parent Positioning** and **Uses Parent Output** (and Avoid Repeat Last 0: one clip has nothing to avoid);
   - an **OnLoad** and an **OffLoad** Blend on RPM: Regions layout, Root Values, Auto Layout, Equal Power, Pitch Follow, Silent Layers Virtualize, Force Loop;
   - the engine Blend on load over the two, crossfading at constant power from 0.3 to 0.7 load, 3D, Doppler Level 0;
   - one **Persistent** event at Priority High playing it on the Target Bus;
   - the clips set to mono and Decompress On Load (their compression is kept). A clip it folds to mono gets Unity's **Normalize** turned off, so the bands keep the levels they were recorded at; its dialog lists every change first.

With loops of one set only, the RPM Blend is the engine and there is no load Blend, so it sounds the same on and off the throttle; the window and its dialog say so. That suits a thin set for other cars; a player's car wants both sets. It never overwrites an asset. The two sets' crossfades fall at the same RPMs when their roots match; move one set's regions by hand to stagger them. Audition the event in §10.5.

#### 10.9 The Container Validator and the other windows

**Window > PepperSFX > Authoring > Container Validator** checks every container and event in the project:

- empty entries, null entries, zero weights; a Routing container with no clip;
- a Regions Blend where nothing plays, or the layers dip below -20 dB, somewhere in its parameter's range, and layers with no region (0–0). Silence from the bottom of the range (a skid at rest, wind at low speed) is a note, not a warning;
- a layer that follows pitch with no Root Value; a layer pitched past the Blend's **Tuning Warning** while it is heard (Pitch Follow over the follow parameter's own range, plus pitch curves on the Blend, the layer and its container);
- a 3D loop with Doppler Level above 0 in a Blend set to Virtualize; a stereo clip on a 3D layer; a streamed clip on a layer that goes virtual. Inside a nested Blend, a clip is judged by what it plays with (the outer Blend's 3D through Uses Parent Positioning, its Virtualize), and reported once;
- 3D distances, or with an Attenuation asset its curve still loud at its Max Distance (the sound goes virtual there);
- a Play action with no Target Bus (a warning when its container has no Mixer Group either: it plays straight to Unity's output); a looping event that isn't Persistent (a hint).

**Fix** and **Auto-Fix All** are undoable. The Blend Inspector shows the same Regions and tuning findings.

- **Audio Profiler:** Voice Usage is real voices against Max Real Voices (100% at the cap).
- **Voice Pool Debug:** Pool Objects against the pool's size (Max Real + Max Virtual + Steal Headroom), which should never change during a session; the real and virtual tables scroll separately.
- **Mixer and Profiler faders, mute and solo** are undoable and saved with the project, in Play mode too: a mix set while the game runs stays.
- **Mixer meters** (full version) show what each bus carries: its own sounds and its child buses', at any depth, so Master reads the whole mix, as on a mixing desk. They move while sounds play on the bus: in Play mode, or while the Audition plays an event through its Target Bus. Outside those they stay empty.

#### 10.10 Engine Grains (Unity 6.3 and later)

**Window > PepperSFX > Engines > Engine Grains** cuts an engine sweep into a **Grain Table** for a Granular Engine (§4.7).

1. Drag the sweep into **Sweep**. It must be **Decompress On Load**; if it isn't, the window offers to set it (its
   compression is kept).
2. Say where its RPM comes from:
   - **Known:** type the RPM at the clip's first sample and at its last (**From → To**), and how it moves between them
     (**Curve**): **Straight Line**, the same RPM every second, or **Even Pitch**, the same number of semitones every
     second (the RPM multiplies by the same factor), as most synthesisers sweep. The number of periods of its pitch in
     one engine cycle is measured against them (6 for a sweep tuned to notes at RPM ÷ 20 Hz).
   - **From Pitch:** the RPM is measured from the sweep's pitch, RPM = pitch × 120 ÷ **Cylinders** (a scale shared with
     Loop Analysis, §10.7), looked for between the two RPMs typed. Use it for a recording, or a sweep that follows
     neither curve.
3. Press **Analyse**. Each grain is one engine cycle (two turns: 120 ÷ RPM seconds); every cut is moved, by less than
   half a period and at most 6% of a cycle, to where the waveform matches the start of the grain before it, so the
   grains join in phase. Each is tagged with the RPM its own length means.

   **The curve typed is checked against the sweep's own,** measured from a strong harmonic followed through it. A curve
   that doesn't fit puts every cut where no cycle ends, so when they disagree (the other curve, or the RPM changing more
   or less over the clip than typed) the window says so and offers buttons: the curve that fits, with the two ranges
   that keep one end you typed (it can't know which end is wrong). Each is cut on trial, and the one whose cuts match
   better comes first, marked **fits better**. Press it, or the other if you know that end is right; it analyses again.
4. Read the **report**: the grains and the RPM range they cover, and:
   - **No grain between A and B RPM:** the sweep jumps there; the player stretches the nearest grain across the gap.
   - **Only N grains per 1000 RPM:** a fast sweep; a slower one gives the player more to choose from.
   - **Holds about X RPM:** the RPM barely moves for a while (a note, not a problem): these grains are what the player
     varies a held RPM with.
   - **The grains are N% off the typed RPMs:** the sweep doesn't follow the curve typed. Check From, To and Curve, or
     try From Pitch.
   - **N% of the cuts found nothing like the cycle before within reach:** the same, seen from the cuts: the grains
     aren't whole engine cycles, and the engine would sound rough.
5. **One Grain At** loops the grain nearest an RPM, unchanged and with no crossfade: a click at its seam is a cut in the
   wrong place.
6. Press **Save**. The table is made beside the sweep as `<sweep>_Grains.asset`, or, when **Grain Table** names one
   (it fills the window with that table's settings), that table is updated, with Undo. Re-analyse whenever the sweep
   changes.

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
Trigger_DuckForDialogue
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

2. **AudioListener in scene, and the listening point where you think?**
```
Only one AudioListener should exist
Usually on MainCamera
Check it's not disabled
```
Unity pans from its AudioListener; Pepper's distance-based features use the listening point (§9.3). If they're on different objects, register the point to follow the listener's transform, or call `AudioManager.Instance.DriveUnityListener(listener)`. In Play mode the Audio Manager's inspector shows the point in use.

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

1. **SetSwitch called BEFORE Post?** With When Switch Changes = Next Play Only (the default), a playing sound keeps its choice. Set Crossfade or Restart on the Switch if a playing loop should follow (§4.5).
```csharp
// CORRECT order
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");
footstepEvent.Post();

// WRONG order
footstepEvent.Post();
AudioManager.Instance.SetSwitch("Surface_Type", "Metal");  // Too late!
```

1b. **A closer value wins.** A value set on the sound's handle or its Audio Emitter wins over the global one; `handle.GetEffectiveSwitch(group)` shows what the sound reads. A group **Driven By** a parameter ignores values set directly (a warning says so).

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
Voice Mixer + OcclusionLayout must also be assigned on AudioManager for the low-pass.
Without a slot the sound only gets quieter behind walls; it keeps its treble.
```

3. **Sounds are 3D?**
```
Occlusion is meant for 3D sounds: rays go from the listener to the voice's position
It doesn't check is3D, but a 2D voice's position rarely means anything
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

1. **Parameter value being set, in the right scope?**
```csharp
// What this play actually reads (handle, then emitter, then global, then default)
float value = handle.GetEffectiveParameter(combatIntensity);
Debug.Log($"Blend reads: {value}");

// Set explicitly for testing
AudioManager.Instance.SetRTPC("CombatIntensity", 0.5f);
```
A value set on an Audio Emitter only reaches sounds **posted on that emitter**; a value on a handle only reaches that sound. A global value is hidden by an emitter or handle value.

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

5. **Parameter name matches?**
```
blendParameterName (or the asset's Parameter Name) must match the name you set exactly
Case-sensitive: "CombatIntensity" != "combatintensity"
```

6. **Value in range?**
```
A named parameter is read clamped to 0-1. With a Parameter asset, the curves' X axis
is in the asset's unit and values are clamped to its Min..Max.
```

7. **Manual update mode?**
```
With the Audio Manager's Update Mode set to Manual, layers only move when Tick() is called.
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

1. **Which mode is the emitter parent in?**
```
Check AudioMultiPositionEmitterParent → Multi Position Type
Decorrelated offsets each emitter on purpose (random start + slight pitch), so it never sounds phase-locked.
SingleEmitter plays one voice at the nearest emitter.
For sample-locked playback at every position, post with a Vector3[] or Transform[] array instead.
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
- The **solver** finds the best path from each source's zone to the listener's zone (or the best few: `Max Paths Per Source`, §13.14), accumulating transmission loss in dB and the minimum cutoff frequency along the way.
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
│   ├── AudioListenerZoneTracker.cs  ← on the player object (needs its Collider)
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

1. **One path per source by default.** With `Max Paths Per Source` at 1 (the default, on `PropagationManager`) the solver voices one path per source: if rain leaks through both a window and an open skylight, you hear it through whichever has lower transmission loss. Raise it (up to 6) to voice several paths at once, each through a different opening on the listener's side and each with its own emitter, so watch `Max Ambient Emitters`. While the listener stands inside a portal's trigger (the blend region), each side still uses one path.
2. **No diffraction.** Real sound bends around corners; the solver assumes straight-line zone adjacency. For small spaces this is rarely noticeable; for huge open-plan spaces you may want to subdivide into more zones.
3. **No raycast occlusion on AmbientEmitters.** AmbientEmitter routing is purely zone/portal topology — the pooled persistent voices do not participate in `AudioManager.OcclusionPerf`'s raycast pass. SFX voices opted into `Use Propagation` *do* compose both (propagation cutoff + raycast cutoff, min-wins). If you need clutter-based occlusion on top of an ambient bed (e.g. a crate blocking line of sight to the emitter), model the bed as an SFX-event-driven loop rather than an `AmbientSource`.
4. **Blend factor updates at solve rate, not per-frame.** For extremely fast doorway traversals (sub-100 ms) you may want to raise `solveRateHz` toward 20–30. For walking speeds the default 15 Hz is imperceptible.
5. **Box-shaped zones by default.** L-shaped zones work via multi-BoxCollider OR-union. For sphere/mesh-shaped zones, subclass `AudioZone` and override `ContainsPoint` / `ClosestSurfacePoint` — both are `virtual` for this reason.

#### 13.15 Troubleshooting

**Sound appears flat or doesn't propagate through doorways.**
- Confirm zones and portals are registered: `Debug.Log(PropagationManager.Instance)` in play mode; inspector shows the manager component if auto-instantiated.
- Zone colliders must have `isTrigger = true` (usually auto-forced). Check the physics layer matrix — if the listener's layer doesn't interact with the zone's layer, trigger events never fire.
- `AudioListenerZoneTracker` must be on a GameObject with a Collider, usually the player root. On the camera that holds the `AudioListener` it never gets trigger events.

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

> **Filter curves are different.** A container's Low Pass and High Pass **Parameter Curves** (§4.1) do put an `AudioLowPassFilter` / `AudioHighPassFilter` on the voices that need them, switched off when fully open. They are meant for long sounds that move slowly through a filter (an engine loop dulling with load, wind opening with speed), where a reset at the voice's start doesn't matter. Occlusion still uses the slots described here, and short, fast-firing sounds should stay away from filter curves.

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

Open **Window ▸ PepperSFX ▸ Mixing ▸ Occlusion Layout** to author the slot hierarchy:

1. **Voice Mixer** — assign the project's voice mixer asset.
2. **Occlusion Layout** — assign or create an `OcclusionLayout` ScriptableObject. This is where the per-bus slot map gets serialized; the runtime reads it at startup.
3. **Reverb Send Bus Registry** (optional, covered in chapter 15) — when assigned, each generated slot also receives one Send effect per registered reverb bus.
4. **Slots Per Bus** — how many concurrent occluded voices each bus can host (default 6).
5. **Scan, Auto-Create, Refresh** — walks the project for `AudioContainer` assets with `Use Occlusion`, `Allow Reverb Send` or `Use Propagation` turned on (any of the three takes a slot at play time, see §14.4) and `AmbientSource` components with `Use Occlusion = true` in the open scenes, collects the unique mixer groups they play through (a layer with `Uses Parent Output` counts its Blend's or Switch's group), and authors one `<BusName>_OcclusionLayer` child group per bus, each containing N slot groups (`<BusName>_Slot_00`, `_Slot_01`, …) with a Lowpass effect and exposed cutoff parameter on each.

After Generate, the `OcclusionLayout` asset holds the layout. Drop it into `AudioManager` ▸ Occlusion Layout, and the runtime builds its slot pools from it at `Awake`.

The "Scan only" button runs the same scan without mutating the mixer — useful for sanity-checking which buses *would* be authored without committing changes.

#### 14.4 Container Opt-In Flags

A voice acquires a slot when its container opts into any of three features:

| Container flag        | What it does                                                                       | Requires slot? |
|-----------------------|------------------------------------------------------------------------------------|----------------|
| `Use Occlusion`       | Raycast occlusion: muffle voices behind walls via listener→source linecast.        | For the low-pass (the volume drop works without one) |
| `Use Propagation`     | Propagation cutoff: muffle voices through portals via the propagation graph.       | For the low-pass (the volume drop works without one) |
| `Allow Reverb Send`   | Per-zone reverb sends (chapter 15).                                                | Yes            |
| `Static Emitter`      | Hint that this container's voices don't move. Speeds up the reverb-send driver.    | No (modifier)  |
| `Explicit Reverb Bus` | Override the zone-driven reverb send routing for this container.                   | No (modifier)  |

A container with none of the three feature flags routes voices directly to its bus (no slot). A container with at least one feature flag tries to acquire a slot when it plays; the AcquireOcclusionSlot call is upfront, before the AudioSource's `Play()`, because mid-playback `outputAudioMixerGroup` reassignment is click-prone in Unity.

**Pool exhaustion**: if all slots on the bus are in use when a new voice tries to acquire, `AcquireOcclusionSlot` returns null and the voice routes directly to the bus (no low-pass, no reverb sends; occlusion's and propagation's volume drop still applies, since it's written to the voice, not the mixer). A throttled warning is logged once per second per bus — bump Slots Per Bus on the layout asset if you see this consistently.

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

**Single ray** (`Occlusion Ray Count = 1`): the original behavior. A single linecast from listener to source. If blocked, the voice clamps to `raycastGain = 0.3` and `raycastCutoff ≈ 6.95 kHz` (the cutoff follows the gain: `Lerp(22000, 500, 1 − gain)`, so it never reaches 500 Hz from rays alone); if clear, both stay at unity / 22 kHz. Binary step.

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

1. **Slot exhaustion → no low-pass fallback.** When the pool is empty, new voices play through the bus directly (no slot, no low-pass; they still get quieter behind walls). This is intentional — graceful degradation beats failing to play. Watch the console for the throttled exhaustion warning and bump slots per bus accordingly.
2. **Multi-ray is not true diffraction.** It approximates edge softening for the most common case (wall with a near edge) but doesn't handle wraparound, multiple reflections, or frequency-dependent diffraction.
3. **Mixer is the cutoff source of truth.** There's no per-voice cached "current cutoff" — `LateUpdate` reads back via `voiceMixer.GetFloat` and lerps. Other code that writes to the same exposed parameter (a custom inspector slider during play, an editor script) will interleave with the smoother.
4. **One layout per project.** The slot pool architecture assumes a single voice mixer. Multi-mixer projects need separate AudioManagers (rarely needed).

#### 14.11 Troubleshooting

**Voices play unmuffled even though `Use Occlusion` is on.**
- If they still get quieter behind walls, the rays work and only the slot is missing: the volume drop doesn't need one, the low-pass does.
- Is `Voice Mixer` assigned on AudioManager? Without it, slot pools never build and the warning at `Awake` will tell you so.
- Is `Occlusion Layout` assigned? Same diagnosis path.
- Did you re-run the Occlusion Layout Builder after adding the container? Containers added since the last Generate aren't in the layout yet.
- Inspect `AudioManager.OcclusionSlotPoolsReady` at play time — false means slot pools didn't build (mixer-layout mismatch, empty layout, etc.).

**Console says `Occlusion slot pool exhausted on bus 'X'`.**
- Self-explanatory: more concurrent occluded voices on that bus than slots configured. Increase `Slots Per Bus` on the layout asset and re-run Generate. The default 6 is sized for typical SFX density; weapon-heavy or footstep-dense scenes can want 10–16.

**Console says `... on bus 'X', which has no slots in the Occlusion Layout`.**
- A container with `Use Occlusion`, `Allow Reverb Send` or `Use Propagation` played through a bus the layout has no slots for (`AudioManager.GetVoice`, once per bus, only when a layout is assigned). The layout predates that bus needing slots: re-run **Scan, Auto-Create, Refresh** (§14.3).

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

One ScriptableObject per acoustic space identity in the project. Create via Project ▸ Create ▸ PepperSFX ▸ Buses ▸ Reverb Send Bus, name it after the room (e.g. `Reverb_Bathroom`).

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

Create via Project ▸ Create ▸ PepperSFX ▸ Settings ▸ Reverb Send Bus Registry. Open the Reverb Send Buses window (Window ▸ PepperSFX ▸ Mixing ▸ Reverb Send Buses) and click Refresh — the registry auto-populates from every `ReverbSendBus` asset in the project.

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

**Project-wide window** — Window ▸ PepperSFX ▸ Mixing ▸ Reverb Send Buses:

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

> **Status: beta, Windows x64 only.** Feature-complete and validated in a standalone Windows build, but see the caveats at the end of this section before relying on it. On every other platform a convolution bus is silent; see [Build Compatibility](#6-build-compatibility).

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

**Requirements:** the native plugin `SFXConvolutionReverb.dll` ships under `RunTime/Plugins/Windows/x86_64/` with **Load on startup** ticked in its import settings. Leave it on: without it the effect never registers in the mixer, and nothing warns you. The plugin runs on 64-bit Windows only; see [Build Compatibility](#6-build-compatibility) for what happens on other platforms.

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
- Parametric bus: check the asset's `Room` value — at `-10000` mB the reverb is silent. Drag it toward 0 (the default).
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

### 16. Audio Loading & Memory

This chapter covers **how clip audio gets into memory and onto a voice** — the RAM footprint of your audio and when the cost of loading it is paid. It's entirely opt-in and tiered. The API surface lives in [API_REFERENCE.md → Loading](API_REFERENCE.md#loading).

#### 16.1 Do I even need this?

| Your situation | What to use | Extra package? |
|---|---|---|
| **Small / medium game** | Nothing — clips are direct references and load with the project (the default). **Skip this chapter.** | — |
| **Lots of audio, occasional hitches on first play** | The **Warmer** (§16.4) — front-load clip data on your schedule. | No |
| **Open-world / live-service, RAM is the constraint** | The **Addressables backend** (§16.5) — load/unload a region's audio by zone. | `com.unity.addressables` |

Nothing here changes behavior for a project that doesn't opt in. The default path — direct `AudioClip` references resolved synchronously — is unchanged and zero-overhead.

#### 16.2 The three layers

Cheapest first. Each is independent; you can use one without the others.

```
1. Import policy   — per-category Load Type / compression. Sets each clip's RAM footprint.   (automatic)
2. Warming         — pre-load a clip's audio DATA ahead of first play, to kill the hitch.     (no package)
3. Addressables    — load/unload the clip OBJECTS themselves by region, so only the current   (gated package)
                     zone's audio is resident.
```

The distinction between layers 2 and 3 matters: **warming** loads the *audio data* of a clip you already hard-reference (the object is already in memory); **Addressables** controls whether the clip *object* is in memory at all. Warming reduces hitches; Addressables reduces peak RAM.

#### 16.3 Import policy (automatic)

The editor import pipeline (`Editor/AudioImport/`) stamps each clip's import settings from its **category preset** (`CategoryPreset.GetDefault`). You don't configure it per clip — you pick the category:

| Category | Load Type | Compression | Preload Audio Data | Why |
|---|---|---|---|---|
| UI | Decompress On Load | PCM | yes | tiny, fired often — cheapest to play |
| SFX | Decompress On Load | ADPCM | yes | short, fired often/overlapping |
| Music | **Streaming** | Vorbis | no | one instance, long — don't hold PCM in RAM |
| Ambience | **Streaming** | Vorbis | no | few instances, long |
| Dialogue | Compressed In Memory | Vorbis | no | many lines, decode on demand |

**Your project's settings.** The presets live in an **Audio Import Settings** asset in your project (Create ▸ PepperSFX ▸ Settings ▸ Import Settings, anywhere under Assets). Until you make one, the code defaults above apply. The package ships none: an installed package is read-only, so one there couldn't be saved. With more than one, the first by path is used and the Console says which.

**Folder rules.** In the settings asset's **Folder Rules**, each rule is a name (its own category: "Engine loops", "Tyres"), a folder, whether it covers the folders below, and import settings. A clip imported into that folder **for the first time** gets the rule's settings, so engine loops dropped into `Assets/Audio/Cars/Engines` come in mono and Decompress On Load without a trip to the Import Manager. The most specific folder wins. Clips already in the project keep their settings, even when they are reimported, so a hand tweak is never undone; **Apply to Existing Clips** on a rule re-applies it. With no rules, imports are Unity's as before.

**The one authoring rule that bites people:** Streaming is **per-instance expensive** — a stream buffer plus disk reads *for every playing voice*. Right for music and long ambience (one or a few instances); **wrong** for a footstep or weapon pool firing many overlapping instances (many disk-streaming voices seeking at once → hitch). The presets already encode the right choice; the mistake is manually setting a short, frequently-fired SFX to Streaming.

| Load Type | RAM | Per-instance cost | Use for |
|---|---|---|---|
| Decompress On Load | high (full PCM) | cheapest | short SFX fired often — footsteps, gunshots |
| Compressed In Memory | medium | decode per play | many medium-length variants |
| Streaming | ~none (reads disk while playing) | stream buffer + disk seek **per voice** | music, long ambience — **one/few instances only** |

#### 16.4 Warming (no extra package)

Categories with `Preload Audio Data = false` (Music/Ambience = Streaming, Dialogue = Compressed-In-Memory) load their audio data on **first play**, which hitches. The **warmer** front-loads that cost on *your* schedule — e.g. on zone-enter or level-load — operating on the clips you already hard-reference, with no Addressables dependency.

```csharp
// On zone-enter / level-load, before the sound is needed:
AudioManager.Instance.WarmContainer(zoneAmbienceContainer); // warms all its clips (recurses Switch/Blend)
AudioManager.Instance.WarmClip(clip);                       // a single clip
AudioManager.Instance.WarmClips(clipList);                  // a set, loaded in parallel
```

- `AmbientSource` warms its loop clip **automatically** on register, so an ambient bed that fades in is typically warm by the time it's audible.
- Biggest win for **Dialogue** (Compressed-In-Memory — decoding a line otherwise hitches) and the **Ambience first-play**. For pure **Streaming** clips the gain is smaller — they stream at play regardless.
- Finer control than the all-or-nothing `Preload Audio Data` import flag: you warm exactly the clips you're about to need, when you choose.

> **Addressables blind spot:** the warmer operates on **direct, hard-referenced** clips only. An addressable-authored entry (null clip + address) is invisible to it. Its loading is owned by **bank preload** (§16.5), which loads both the clip and its audio data, so it doesn't need the warmer. Don't expect `WarmContainer` to warm converted (addressable) content.

`AudioClipWarmer.Unload` exists but is blunt — it frees the data with **no check for live voices**. Only unload data you know nothing is playing. Safe, refcounted residency (that never unloads a clip under a playing voice) is what the Addressables backend adds.

#### 16.5 Addressables backend

Requires **`com.unity.addressables`**. This loads and unloads the clip *objects* themselves by region, so only the current zone's audio is resident. The assembly (`SFXSystem.Addressables`) compiles automatically when the package is present; without it, everything here is dormant and the project runs on direct references — exactly like the netcode adapters (§17).

**Setup:**

1. **Install** `com.unity.addressables` (Package Manager). The `SFXSystem.Addressables` assembly compiles automatically when present.
2. **Mark the region's clips Addressable** and give them a shared **label** (e.g. `zone_crypt`) — easiest with the Authoring window (§16.6), which puts them in the label's own group, `Audio zone_crypt`, with one bundle per clip. By hand in the Addressables Groups window, **don't put every zone's clips in one group** with the default Bundle Mode (Pack Together): Unity unloads a bundle only as a whole, so leaving a zone would free nothing while you stand in another. Give each zone its own group, or set the group's **Bundle Mode** to **Pack Separately** (one bundle per clip). Pack Separately also frees the rest of a zone's clips while one of them, say a loop, plays on across the border. Measured in a test build with three zones: with Pack Separately, 7–11 MB of audio was loaded in any one zone against 26 MB for all three; with one shared group, every zone walked through stayed loaded, as much as loading all three up front.
3. **Author the container entries as addressable.** Easiest is the **Addressable Audio Authoring** window (§16.6): select the containers, pick the label + address style, hit Convert. By hand: set each entry's **address** to the clip's Addressable address, exactly as the Addressables Groups window shows it, and its **clip** to **None**. Use the address, not the asset GUID (unless the address *is* the GUID): a zone bank loads its clips by address, and an entry keyed any other way never loads and stays silent. The Loading Validator flags such keys as unregistered. *Per entry it's either a direct clip or an address — a populated clip would eager-load and defeat the point.*
4. **Add a `Zone Audio Bank`** component to the AudioZone's GameObject (Add Component ▸ *Audio ▸ Loading ▸ Zone Audio Bank*) and set its **Bank Label** to the label from step 2.
5. **Put one `Addressable Audio Runtime`** component in your first scene (Add Component ▸ *Audio ▸ Loading ▸ Addressable Audio Runtime*); a bootstrap scene is ideal. It starts the backend, which installs the addressable clip source and preloads/releases each zone's bank as the listener enters/exits. The backend **persists across scene loads**, so you need only one. A runtime in a later scene finds it already running, logs that, and does nothing (the first runtime's settings apply).

Now a zone's clips **load on enter** and **unload (after a short linger) on exit**. A clip still playing when you leave a zone stays resident until the voice finishes — **no mid-play unload**, including across a scene load. When a scene unloads, its zones count as exited, so their banks release normally. After teleporting the listener, call `PropagationManager.ResyncListenerAfterTeleport()`: the zones you left release their banks, and the ones you land in load theirs.

**Voice-safe residency (the important part).** A zone's bank releasing on exit does **not** immediately unload a clip a voice is still playing — a looping ambience, or a one-shot tail ringing out as you cross the border. Residency is refcounted across three holders:

```
clip resident while  (banks holding it)  +  (live voices bound to it)  +  (explicit pins)  > 0
```

The actual unload fires only when that total hits **0**, and then only after a **linger** window (default **5 s**) to absorb zone-border dithering (step out / step back in). A stuck-high refcount is only wasted RAM, never a click — the design deliberately errs toward holding the ref.

**Debug overlay.** Tick **Show Debug Overlay** on `Addressable Audio Runtime` to see each address's `bank / voice / pin / total` refcounts and warm/cold state live during Play. On zone-exit, `bank` drops to 0 but a playing voice keeps `voice ≥ 1`, so the clip stays warm until the voice ends plus linger. Off by default; zero cost when off.

**Addressable ambiences.** `AmbientSource` can be addressable too: set its **Looping Clip Address** and leave **Looping Clip** empty. The source **pins** the clip loaded for its active life (the `pin` refcount), so it's ready even when the ambience is heard through a portal *before* you enter the zone. Its resolved clip (null while an addressable clip is still loading) is exposed as `PlayableClip`; propagation reads that and defers until it's ready.

#### 16.6 Authoring window — convert clips in one click

**Window ▸ PepperSFX ▸ Loading ▸ Addressable Audio Authoring** does the whole "mark addressable + set address + assign label + null the direct clip" in one click, across Random / Sequence / Routing containers, and keeps the groups tidy afterwards — no Groups-window round-trip. It has two tabs.

**Convert Containers**

1. **Select** one or more AudioContainer assets in the Project.
2. Set options (all in-window):
   - **Bank Label** — dropdown of existing labels, plus *New label…*. This is the bank a `ZoneAudioBank` loads. Optional.
   - **Group** — *Bank's own group* (the default) puts the clips in `Audio <label>`, made on first use with one bundle per clip, so a zone's clips unload when the listener leaves (§16.5 step 2). You can also pick an existing group or *New group…* (also one bundle per clip). If the group you pick packs everything into one bundle and already holds another bank's clips, the window warns you. It also warns when the group isn't in builds (a blank group made in the Addressables Groups window, or one with **Include in Build** off): its clips would play in the editor and be missing from a build.
   - **Address Style** — how the key reads: *Label-scoped* (default) `<label>/<clip>` e.g. `Zone_Crypt/Add-S_1` (readable + organized); *Plain name* `Add-S_1`; *Asset path* `Assets/Audio/Add-S_1.wav`; *GUID* (rename-proof, unreadable). Collisions auto-resolve with a numeric suffix (`…_2`).
3. **Convert to Addressable** — marks each clip addressable (into the group, labeled), writes the readable address onto the container entry, nulls the direct clip. Undo-able for the containers; the Addressables entries stay.
4. **Revert to Direct** — relinks the direct clips from their addresses and clears the addresses. Assets stay marked Addressable by default; tick **Revert: also remove from Addressables** (with a confirmation) to fully un-addressable them — only safe when nothing else uses those addresses.

**Clips added later.** Add the clip to the container as a normal clip, select the container, and Convert again: only the new entries are converted. When the selected containers already have addressable clips, the tab shows their group and label, with a **Use** button, so the new clip joins them.

> Convert skips entries that are already addressable (clip already null). To re-address with a different style, **Revert first, then Convert**. The window logs what it touched.

**Groups**

Pick a group to see its audio clips, each marked **used ×N** or **unused**. Used means a container asset, or an `AmbientSource` in an open scene, points at the clip's address. Scripts, prefabs and closed scenes aren't searched, so check before removing an "unused" clip one of those plays.

- **Add Selected Clips** — puts the AudioClips selected in the Project into the group, with the chosen **Bank Label** (it starts as the label the group's clips share) and the Address Style from the other tab. Use it for a clip that isn't in a container, such as an ambience's loop: paste its address (the **Copy** button) into the AmbientSource's **Looping Clip Address**.
- **Remove** (per clip) and **Remove Unused** — take clips out of Addressables to clear out old ones. The clip assets stay in the project. A removed clip that something still points at goes silent; the confirmation names who. Ctrl+Z doesn't undo it: add the clip again instead.
- **Pack Each Clip Separately** — shown when the group packs several banks into one bundle (for example after converting every zone into the Default Group). It switches the group to one bundle per clip. Rebuild the Addressables content afterwards.
- **Include in Builds** — shown when Addressables leaves the group out of builds: it has no Content Packing & Loading schema (a blank group made in the Addressables Groups window), or its **Include in Build** is off. Its clips play in the editor but play nothing in a build, and Addressables only mentions it while building. The button gives the group the Default Group's build settings it lacks (one bundle per clip) and switches Include in Build on. Leave it if you keep the group out of builds on purpose.

A clip asset you delete from the project leaves its Addressables group by itself, so there's nothing to clean up for those.

#### 16.7 Diagnostics

- **Precise miss warnings.** An address that can't resolve — **no backend active**, a **typo**, or an **unregistered** key — logs a precise, clickable, **once-per-address** warning instead of dropping silently. (An *expected* cold-drop — backend active, clip simply not preloaded yet — is not warned.)
- **Loading Validator.** **Window ▸ PepperSFX ▸ Loading ▸ Loading Validator** → **Validate** scans every container asset and the open scene's `AmbientSource`s and reports: an address but no backend; an unregistered / typo'd address; an entry with *both* a clip and an address; addressable content but no `AddressableAudioRuntime` in the scene. Each finding has a **Select** button. The validator uses reflection, so it runs **even without the Addressables package installed**.

#### 16.8 How it works (architecture)

Same dependency-inversion pattern as the netcode layer: a small seam in the core, with the package-specific weight quarantined in a gated assembly.

```
SFXSystem.Runtime
   IAudioClipSource (seam)  ──default──►  DirectClipSource   (returns the serialized clip, sync, zero overhead)
        ▲
SFXSystem.Addressables  (gated by com.unity.addressables)
   AddressableClipSource  ──►  refcounted Addressables load/unload by region
   AudioBank.Preload()/Release()  (a bank = an Addressables label)
```

- **Containers never read `.clip` directly.** They resolve through `AudioClipSources.Active` (an `IAudioClipSource`). The default `DirectClipSource` returns the serialized clip synchronously. The Addressables backend drops in as an alternative source without re-touching a single `Play()` method.
- **An entry holds EITHER a direct `AudioClip` OR an Addressables address string** — never both. The address is the key a bank's label resolves to, as shown in the Addressables Groups window (the GUID only if the address is the GUID). The core stays free of any Addressables type.
- **The play path stays synchronous on the warm path.** `TryGetClip` returns a resident clip instantly; only a *cold* clip returns false and the play is dropped (drop-on-cold, v1). Bank preload keeps zones warm, so ~every play is warm.
- **Voice-level refcount** is taken at the two chokepoints already in the core: `OnVoiceBound` (when `source.clip` is assigned) and `OnVoiceReleased` (when the voice returns to the pool). That's what makes a bank release safe against a playing voice.
- **Zone binding is decoupled.** `PropagationManager` raises `ListenerEnteredZone` / `ExitedZone` events; the Addressables assembly subscribes. Propagation never depends on Addressables, and vice-versa — the dependency is one-way (Addressables → core).

#### 16.9 Status & limitations

- **Import policy & Warming:** stable; behavior-neutral for existing projects.
- **Addressables backend:** covers Random / Sequence / Routing per-entry addresses, the `AmbientSource` pin path, diagnostics + validator, and the authoring window. Automated tests cover a playing voice keeping its clip loaded after the zone's bank releases, one clip shared by two zones, linger at zone borders, cold entries, teardown, the backend persisting across scene loads, and zone enter/exit staying paired through teleports and unloaded zones.
- **Cold entries in a Sequence:** a cold entry drops that play, and the sequence stays on it, so the next play tries it again. With Auto Advance the run stops at that entry. Keep the zone's bank loaded before the sequence reaches it.
- **Tested in a player build** (Windows, 6000.0.48f1): three zones, walked in and out. Only the zone you stand in stays loaded, a loop started in one zone keeps playing (and keeps only its own clip loaded) after you walk into the next, and nothing stays loaded outside the zones.
- **Test unloading and memory in a build.** With the default Play Mode Script ("Use Asset Database"), Addressables in the editor loads straight from the AssetDatabase, and releasing a clip never actually unloads it. To see real unloads in the editor, build the content (Addressables Groups window ▸ Build ▸ New Build ▸ Default Build Script) and set Play Mode Script to "Use Existing Build"; rebuild the content after changing it. The memory you save only shows properly in a player build: make a Development Build and watch memory with Unity's Memory Profiler as you move between zones.
- **Out of scope (v1):** drop-on-cold (not defer-and-play-late) — keep zones preloaded; only the clip leaf is lazy (events/containers stay eager); zone-driven banks are one Addressables label per `AudioZone`.

#### 16.10 Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| Addressable sound never plays | The clip is **cold** (its zone bank wasn't preloaded) → drop-on-cold. Confirm a `ZoneAudioBank` with the right label is on the zone and an `AddressableAudioRuntime` is in this scene or an earlier one. Check the entry's key is the clip's Addressables **address**, not its GUID (run the Loading Validator). Watch the Debug Overlay. |
| Addressable sound plays in the editor but not in a build | Its group isn't in builds: no Content Packing & Loading schema (a blank group from the Addressables Groups window) or **Include in Build** off. Addressables only says so while building ("does not have any associated AddressableAssetGroupSchemas"). The Authoring window's **Groups** tab flags the group and **Include in Builds** fixes it (§16.6); then rebuild the Addressables content. |
| `references addressable key '…', but no Addressables backend is active` | No backend installed or active: assign a direct `AudioClip` to that entry, or (full version only) install `com.unity.addressables` **and** add an `AddressableAudioRuntime` (Add Component > PepperSFX > Loading > Addressable Audio Runtime). |
| An entry has both a clip and an address | A contradiction — the populated clip eager-loads. The **Loading Validator** flags it; null the direct clip (or Revert the entry). |
| Clip unloads mid-play / click on zone exit | Should not happen — residency is voice-refcounted. Check you didn't call `AudioClipWarmer.Unload` (blunt) or a raw `Addressables.Release` outside the refcounted source. |
| Load/unload churn at a zone border | Increase **Linger Seconds** on `AddressableAudioRuntime`. |
| Memory doesn't drop when you leave a zone (in a build) | The zone's clips share a bundle with clips still in use: one group for every zone, or a zone's loop still playing in a Pack Together group. The Authoring window's **Groups** tab flags such a group and can switch it to **Pack Each Clip Separately** (§16.6); or give each zone its own group. The Debug Overlay's counts going to 0 only tells you the system let go; the bundle frees when nothing else in it is held. |
| `WarmContainer` doesn't warm my converted content | Warming operates on **direct** clips only; addressable entries are owned by bank preload (§16.4). |
| Footsteps/weapons hitch under load | A short, frequently-fired SFX is set to **Streaming** — a per-voice disk stream. Set it to Decompress On Load (§16.3). |
| Addressable ambience silent until I enter the zone | Set the `AmbientSource`'s **Looping Clip Address** (leave **Looping Clip** empty) so it *pins* the clip. |

---

### 17. Multiplayer (Netcode)

Multiplayer support lets you **post a sound on one player's machine and have every player hear it** — without your audio code knowing which netcode stack the project uses, and without changing anything about how local audio works. The API surface lives in [API_REFERENCE.md → Netcode (Multiplayer)](API_REFERENCE.md#netcode-multiplayer).

**Applies to:** `SFXSystem.Netcode` (Phases 0–7). **Verified backends:** FishNet v4 (Phases 0–7 on one PC and on two PCs over a real network) · Netcode for GameObjects (NGO) 2.x (light testing only so far, not the full check FishNet had; expect rough edges) · Mirror 96 or newer (new in 3.1.1: its automated host-mode tests pass, the checks with several players are still to do). **Planned:** NGO 1.x back-compat · position quantization.

#### 17.1 The one thing to understand first

**No audio ever crosses the network.** When you fire a networked sound, all that travels is a tiny **~36-byte descriptor** — "play event #X at position Y (optionally attached to object Z)". Each machine then plays that event **locally** through the normal audio engine. You are syncing *intent*, not *sound*. That's why it's cheap, and why every local feature (occlusion, reverb, propagation, voices) keeps working untouched. Even the musical sync (§17.6) sends only *timing* (tempo + a phase anchor), never samples.

**In one sentence:** *Use `PostNetworkedEvent` instead of `PostEvent` for sounds everyone should hear; everything else stays exactly the same.*

The audio system never references a netcode package. You install **one** stack (NGO, FishNet, …) and the matching **adapter** lights up automatically. A backend you didn't install never compiles, so it can never break your build.

#### 17.2 How it works

```
   PostNetworkedEvent("door_open", pos)
            │
            ├─ plays LOCALLY right now  ──────────────►  you hear it instantly (local echo)
            │
            └─ Broadcast(descriptor) ─► [adapter RPC] ─► server ─► other clients
                                                                      │
                                                         PostEvent(id, pos) on each
```

- **Local echo:** the instigator plays the sound immediately (zero network latency); only *other* peers get it over the wire. No double-play.
- **Authority:** server-authoritative relay — the client sends to the server, the server validates and relays to everyone else. (The adapter implements this; the core doesn't care.)
- **Identity on the wire:** each `AudioEvent` has a **stable ID** (a build-deterministic number derived from the asset's GUID). Both ends resolve the same ID to the same event. *Events must have a captured ID to be networkable* — see §17.3 step 2.
- **Seclusion:** delete the whole `Netcode` folder and your single-player game compiles and behaves identically. Networking is purely additive.
- **Leaving it out without deleting it:** add `SFXSYSTEM_DISABLE_NETCODE` to **Project Settings > Player > Scripting Define Symbols**. Every multiplayer assembly (the core, its editor tools, its tests and the FishNet, NGO and Mirror adapters) is then left out of the build, so none of its code or statics reach your game, and updating the package doesn't bring the folder back. Nothing else changes. Useful when your game does its own networking (a racing game that sends car state, not sound events).

| Layer | What it does |
|---|---|
| `SFXSystem.Runtime` | The audio engine. Knows nothing about networking. |
| `SFXSystem.Netcode` (core) | The agnostic contract: payload, transport interface, router, replicator, clock/transport, and the `NetworkedAudioManager` façade. References Runtime only. |
| `SFXSystem.Netcode.Ngo` / `.FishNet` / `.Mirror` | One tiny adapter per backend — the *only* code that references that stack. Auto-enabled when its package is installed. |

#### 17.3 Quick Start

**Step 1 — Install a netcode backend.** NGO: Package Manager → *Netcode for GameObjects*. FishNet: import from the Asset Store. Mirror: import from the Asset Store (version 96 or newer). The matching adapter (`SFXSystem.Netcode.Ngo` / `.FishNet` / `.Mirror`) enables itself. Install only **one** stack.

**Step 2 — Give your events stable IDs.** Open **Window ▸ PepperSFX ▸ Multiplayer ▸ Audio Event IDs** and click **Capture All Missing IDs**. Every event needs a non-zero **Stable ID** to be networkable. Newly created events get one automatically on import; the sweep back-fills older ones.

**Step 3 — Put the transport in your scene.** Create one networked object and add the matching transport component: NGO → `NetworkObject` + `NgoAudioTransport`; FishNet → `NetworkObject` + `FishNetAudioTransport`; Mirror → `NetworkIdentity` + `MirrorAudioTransport` (**Add Component ▸ PepperSFX ▸ Multiplayer**). **Exactly one** per session, on a persistent networked object (e.g. an `AudioNetwork` GameObject) — **not** on per-player objects. It must be **active** in the scene, and the scene saved. On Mirror, each client also needs a player object (§17.7).

**Step 4 — (optional) Add settings & health-check.** Drop a **`NetworkAudioSettings`** component in the scene to opt into proximity culling, an authority validator, and the clock tunables (§17.5). With none present, sensible defaults apply (validation allow-all, culling off). Open **Window ▸ PepperSFX ▸ Multiplayer ▸ Network Audio Debug** in Play mode to watch live session/clock/loop state.

**Step 5 — Post networked sounds.**
```csharp
using AudioSystem.Netcode;

// Heard by everyone (local echo + relay to others):
NetworkedAudioManager.PostNetworkedEvent("door_open", transform.position);

// Still 100% local — only this client hears it. Unchanged:
AudioManager.Instance.PostEvent("ui_click");
```

With no live session (single-player, not connected), `PostNetworkedEvent` gracefully falls back to a normal local play — so the same call works everywhere.

> **Authoring note:** the per-event **Multiplayer** foldout in the `AudioEvent` inspector (§17.4) only appears once a netcode backend is installed. The fields it exposes live on the Runtime `AudioEvent` and are inert without the `Netcode` layer, so single-player projects stay uncluttered.

#### 17.4 Local vs networked — the mental model

| Call | Who hears it | Use it for |
|---|---|---|
| `AudioManager.Instance.PostEvent(...)` | **only this client** | UI clicks, this player's own HUD, anything client-private |
| `NetworkedAudioManager.PostNetworkedEvent(...)` | **everyone** | gunshots, doors, explosions, world sounds all players share |
| `NetworkedAudioManager.PostServerEvent(...)` | **everyone** (from the server) | round-start stingers, objective sounds the *server* decides — no local instigator |

Rule of thumb: if another player should hear it, use `PostNetworkedEvent`. If the **server** is the authority for it (no single client instigated it), use `PostServerEvent`. Otherwise use `PostEvent`.

Every capability below stays descriptor-only — still no audio on the wire.

**Attach a sound to a moving object (emitter binding).** Pass a networked `GameObject` and the sound **follows it on every client** instead of playing at a frozen point:
```csharp
NetworkedAudioManager.PostNetworkedEvent("grenade_bounce", grenade.transform.position, grenade);
```
The object needs a `NetworkObject`. Remote clients resolve its net-id and parent the voice to it; `position` is the spawn anchor **and** the fallback. If the object isn't resolvable yet (its spawn message may still be in flight), the sound is held for a short retry window (~0.5 s), then falls back to the static position — never a frozen sound at the origin, never an error.

**Shared loops you can stop (networked lifetime).** For a sound that must **start and later stop for everyone** — an alarm, a generator hum, a held ambience — use the loop API. It returns a handle:
```csharp
NetworkedSoundHandle alarm = NetworkedAudioManager.PostNetworkedLoop("alarm_loop", pos, alarmObj);
// …later, stop it on every client:
NetworkedAudioManager.StopNetworked(alarm);
```
- A **late joiner** automatically hears loops already running: the server keeps a registry of active loops and replays them to the new client only.
- **Loop sync tiers** control the joiner's phase — authored per-event (Multiplayer foldout, *Loop Sync Tier*): **0 None** (joiner starts from the beginning; everyone free-runs); **1 Aligned at join** (joiner fast-forwards into the shared phase once, then free-runs); **2 Phase-locked** (continuously grid-anchored and gapless — stays in phase across all machines; use for musical/rhythmic loops). Tier 2 **requires variation parity** — leave *Independent Variation* off, or each client phase-locks a *different* clip (the inspector warns). *Only meaningful for a loop that repeats one clip; an auto-advancing Sequence container is meant to free-run (this guard is not yet tested).*

  > **Tier 2 is experimental.** It is still under construction and shouldn't be relied upon in production in its current state. Two-PC testing shows it holding phase with no measurable drift over 15+ minutes, but a **late joiner can play one or two iterations out of phase** (then go briefly silent) before it locks on. Tiers 0 and 1 are unaffected.
- **Off-session** the same calls degrade to a plain local loop; the handle still stops it. Persistence is an **explicit** choice (this API), never inferred from a container's loop flag.

**Same random variant on every client (variation parity).** A `RandomContainer` normally rolls independently per machine. Networked posts force the **same** variant — clip choice *and* the per-voice pitch/volume roll — on every client by default.
```csharp
NetworkedAudioManager.PostNetworkedEvent("footstep", pos);                              // identical everywhere (default)
NetworkedAudioManager.PostNetworkedEvent("crowd_murmur", pos, independentVariation: true); // each client rolls its own
```
`independentVariation` is a nullable bool: `null` (default) uses the event's authored **Independent Variation (default)** flag; `true`/`false` overrides per call. The seed also covers the parts of a post that run later: a Play action with a Delay, and a `SequenceContainer`'s later clips (in Random order too) each get a seed worked out from the post's, so they pick the same variants on every machine. (Before 3.1, a Random-order Sequence past its first entry, and delayed Plays, escaped the seed.)

**Server-authored sounds.** When the **server** is the authority for a sound with no single client instigator, `PostServer*` originates from the server to **everyone**, with **no local echo**:
```csharp
NetworkedAudioManager.PostServerEvent("round_start", Vector3.zero);          // server only — warns + no-ops on a client
NetworkedSoundHandle hum = NetworkedAudioManager.PostServerLoop("reactor_hum", reactorPos, reactor);
NetworkedAudioManager.StopNetworked(hum);                                    // server-authored stop → All
```
Client-instigated relays run through a server-side **authority validator** first (assign `NetworkAudioSettings.ServerValidator` in code); reject → the sound is dropped for others (the instigator still heard its echo). A client can stop only loops it owns; server-authored loops can only be stopped by the server.

**Proximity culling — *don't send*.** With culling on, the server sends a spatial (3D) sound **only to clients whose listener is within its audible radius** — the packet is *never sent* out of range. That halves fanout and closes an audio-wallhack. 2D sounds are never culled.
```csharp
var settings = FindObjectOfType<NetworkAudioSettings>();
settings.ProximityCulling = true;
settings.ListenerPositionProvider = clientId => PlayerPositions.TryGet(clientId, out var p) ? p : (Vector3?)null;
```
Return `null` from the provider when a position is unknown → that client is **not** culled (fail-safe). A null provider makes culling **inert** (the Network Audio Debug window flags this). Active loops are re-evaluated over time (~33 Hz): a listener walking into range starts hearing the loop.

**Per-event reliability.** Every relay is **reliable** by default. Mark spammy positional one-shots (footsteps, full-auto gunfire) **Unreliable Delivery** in the Multiplayer foldout to send them on the unreliable channel (lower overhead, no head-of-line blocking). Loops, stops, snapshots, and scheduled cues always stay reliable.

#### 17.5 Per-event authoring (Multiplayer foldout)

Shown in the `AudioEvent` inspector only when a netcode backend is installed:

| Field | Description |
|---|---|
| `Unreliable Delivery` | Relay this event's one-shots on the unreliable channel. |
| `Independent Variation (default)` | Default for `independentVariation` when a post passes `null`. |
| `Loop Sync Tier` | 0 None / 1 Aligned-at-join / 2 Phase-locked (**experimental**, see §17.4). Tier 2 needs variation parity, Max Instances 0 (or at least 3), Cooldown 0, and — for a Routing or Blend that plays several clips at once — clips of equal length. The inspector warns about each. |

`NetworkAudioSettings` (a scene component) holds the culling/clock config and the game-supplied hooks (`ListenerPositionProvider`, `ServerValidator`). See [API_REFERENCE.md → Netcode (Multiplayer)](API_REFERENCE.md#netcode-multiplayer) for the full member list.

#### 17.6 Musical sync — scheduled & quantized cues

For cross-machine *rhythmic* timing, the layer adds a shared **network clock** (maps network time onto each machine's local audio clock) and an optional **musical transport** (tempo + phase). Cues land together on every machine — the industry transport-and-quantize model, not varispeed.

```csharp
// Fire on a shared network time (each client maps it to its own dsp clock):
double t = NetworkClock.NetworkTime + 0.5;
NetworkedAudioManager.PostNetworkedScheduled("beep", pos, t);

// Server sets tempo once (networked on change, replayed to late joiners):
NetworkMusicTransport.SetTempo(bpm: 128, beatsPerBar: 4);

// Quantize a cue to the next beat/bar of the shared grid:
NetworkedAudioManager.PostNetworkedQuantized("stinger", pos, Quantize.NextBar);
```

- A scheduled cue is clamped to at least a minimum lead ahead (so even a distant client receives it before its onset); one that still arrives late is dropped rather than fired out-of-phase.
- `PostServerScheduled` / `PostServerQuantized` are the server-authored twins.
- A mid-session `SetTempo` change re-anchors **seamlessly at the next bar** of the current grid.
- **Scheduled/quantized cues are global (non-positional) by design** and bypass proximity culling — they're for music, not positional gameplay.
- `NetworkClock.ResyncClock()` forces a hard re-sample after a known hitch.

#### 17.7 Per-backend notes

**NGO (Netcode for GameObjects) 2.x** — Adapter `NgoAudioTransport`, gated on `com.unity.netcode.gameobjects` (`SFXSYSTEM_NGO`). In-scene `NetworkObject`s auto-spawn on server start. Late-join snapshot fires from `OnClientConnectedCallback` (NGO has no per-observer hook, so it relies on scene-object sync completing before the callback). Server id is 0 and remote clients start at 1. *NGO has had light testing only; the full Phase 4–7 sweep FishNet had is still to do. It compiles and coexists with FishNet.*

**FishNet v4** — Adapter `FishNetAudioTransport`, gated on the `FISHNET` define. The transport's `NetworkObject` **must be active in the loaded scene** — it only registers once the object network-spawns (if a freshly added object doesn't spawn, save the scene). Late-join snapshot fires from `OnSpawnServer(NetworkConnection)` — per-observer, robust timing. Client ids can start at 0, so server-authored loop ids use a reserved `0xFFFFFFFF` owner namespace (handled for you).

**Mirror 96 or newer** — Adapter `MirrorAudioTransport`, gated on Mirror's own `MIRROR_96_OR_NEWER` define. Put it on a scene object with a `NetworkIdentity` in your online scene, and save the scene (Mirror gives scene objects their ids when the scene is saved). It sets that identity to **Force Shown**, so interest management never hides it.

- **Pepper client ids.** A Mirror client never learns its own connection id (only the server knows it), so the transport gives each player an id of its own when it joins: the host 0, then 1, 2, 3 …, never reused. Your `ListenerPositionProvider` and validator receive these ids. On the server, `TryGetConnection(clientId, out conn)` and `TryGetClientId(conn, out clientId)` translate between them and Mirror's connections; `conn.identity` is the player object. `JoinedClientIds` lists everyone who has joined.
- **Joining.** A client asks to join when it loads the transport, and the server's answer brings its id and the late-join snapshot together. Until that answer arrives (one round trip), the client's networked sounds play locally only.
- **A client needs a player object.** Mirror spawns scene objects, this one included, only for clients that have one. The NetworkManager's **Auto Create Player** (on by default) takes care of it. The server logs a warning about a client that stays without one for 5 seconds.
- **The clock** is Mirror's predicted time minus half the round trip: "server now" on every machine. Mirror's own `NetworkTime.time` is deliberately behind and speeds up or slows down to catch up, so it isn't used.
- **Latency.** The server sends once per send interval (the NetworkManager's **Send Rate**, 60 Hz by default), so a relayed sound can leave up to about 17 ms later than on FishNet. Raise Send Rate if that matters.
- *New in 3.1.1. Its automated tests run a host (server and its own player in one process); the checks with several players are still to do.*

> Run **one** backend at a time. Both adapters can be *present* in the project (each is gated), but you start a session with a single stack.

#### 17.8 Current limitations (by design, for now)

- **Networked events should be "play" sounds.** Global state changes (RTPC / Switch / State / ducking) stay **locally driven** by each client's own game logic — networked posting doesn't replicate global audio state. (Variation parity covers randomness *within* a chosen branch, not the switch/state/RTPC input that chooses the branch.)
- **Scheduled / quantized cues are global (non-positional)** and bypass proximity culling by design — they're for musical sync, not positional gameplay.
- **Tier-2 phase-locked loops are experimental** — not for production yet. Known issue: a late joiner can play one or two iterations out of phase before locking on.
- **Tier-2 phase-locked loops are delivered to all peers** (the culler skips them).
- **Cross-machine tightness is bounded by the clock estimate** — quantization buys slot-agreement and drift-immunity, not closeness.
- **Position is full precision** (quantization for smaller packets is a future optimization). **Static / single-context** (one `AudioManager`, one router, one transport per process).
- **Scale.** Built for co-op and competitive sessions, scaling as far as your chosen netcode's authoritative-server model does (typically tens of players). Adds proximity culling but not MMO-grade interest management or sharding; for hundreds+ concurrent players a different replication architecture is required.
- **Breadth still open:** NGO 1.x back-compat. The NGO 2.x adapter has had light testing only, and the Mirror adapter (new in 3.1.1) only its host-mode tests so far.

#### 17.9 Extending: writing a new backend adapter

1. Create a gated assembly under `Netcode/Adapters/<Stack>/` referencing `SFXSystem.Runtime`, `SFXSystem.Netcode`, and that stack's runtime assembly, constrained on the stack's define symbol.
2. Implement `INetworkAudioTransport` on a networked behaviour: **Play** via the stack's RPCs in `Broadcast`, raise `OnEventReceived` on receipt, marshal the `emitter` GameObject to a net-id and implement `TryResolveEmitter`; **Stop** via `BroadcastStop` → `OnStopReceived`; **Scope `All`** originates directly to all clients + host (never a dedicated server); **Scheduled / transport** implement `BroadcastScheduled` / `BroadcastTransport` and raise `OnScheduledReceived` / `OnTransportReceived` / `OnLoopJoinReceived`, expose `ServerTime` + `RoundTripTime`; **Late join** replays `NetworkAudioRegistry.Snapshot(...)` to a joining client; **Register** with `NetworkAudioRouter.Register(this)` / `Unregister(this)` and pump `AudioEventReplicator.Tick()` + `NetworkClock.Tick()` from `Update`.
3. Relay to everyone **except the sender** (it already played the local echo).
4. If the stack's clients don't know their own id (Mirror is one), keep a `NetworkAudioPeerTable` on the server: it hands each player a Pepper id (host 0, then 1, 2, 3 …, never reused, never the reserved namespaces) for the server to send it. The Mirror adapter shows how.

The core and the audio runtime never change.

#### 17.10 Troubleshooting

| Symptom | Likely cause / fix |
|---|---|
| Sound plays locally but not on other clients | Transport component missing/inactive, or not on a spawned `NetworkObject`. Check **Network Audio Debug** — is the session Active? |
| `'<event>' has no stable id` warning | Run **Audio Event IDs ▸ Capture All Missing IDs**. |
| Nothing networks at all | Backend not installed, or no `NetworkManager`/transport in the scene. |
| Hearing the sound twice while testing | Multiplayer Play Mode plays *both* virtual players through one set of speakers (a flam). Expected; on real separate machines each player hears it once. |
| A moving sound plays at a frozen point on remote clients | The emitter object has no `NetworkObject`, or isn't spawned/observed on that client — it fell back to the static position. |
| A late joiner doesn't hear a running loop | Only `PostNetworkedLoop` sounds are snapshotted. On NGO, confirm the joiner synced the transport object before the snapshot fired (§17.7). |
| `StopNetworked` doesn't silence remotes | Keep the `NetworkedSoundHandle` from `PostNetworkedLoop` and pass *that* handle. |
| Culling on but everyone still hears everything | No `ListenerPositionProvider` assigned → culling is inert. Network Audio Debug ▸ Config Health flags this. |
| A Tier-2 loop plays a different clip per client | The event has both *Loop Sync Tier 2* and *Independent Variation* on. Turn Independent Variation off (the inspector warns). |
| A Tier-2 loop cuts out after a cycle or two | Each iteration is an ordinary post of the event, and they overlap. Max Instances 1–2 steals an iteration; a Cooldown rejects one. Use Max Instances 0 and Cooldown 0 (the inspector warns). |
| A layered Tier-2 loop doubles up or gaps every cycle | The Routing/Blend plays clips of different lengths; the loop length comes from the first clip. Make the layers the same length (the inspector warns). |
| A late joiner's Tier-2 loop is out of phase for a cycle or two, goes quiet, then locks on | Known issue — Tier 2 is experimental (§17.4). Tiers 0 and 1 don't have it. |
| One machine's audio slows down, crackles, or drifts badly even on Tier 0 | That machine's audio device is falling behind (its audio clock runs slow), not the network. Check the output device and its buffer size; close audio-heavy apps. Tier 2 re-anchors each cycle and recovers once the device does. |
| Quantized cue does nothing / warns | No musical transport set — call `NetworkMusicTransport.SetTempo(...)` on the server first. |
| On Mirror, one client never hears networked sounds | It has no player object, so Mirror never spawned the transport for it (the server warns after 5 s). Turn on the NetworkManager's **Auto Create Player**, or add a player for that client. |
| Compile errors in `SFXSystem.Netcode.Mirror` after removing Mirror | Mirror leaves its `MIRROR…` defines behind. Remove them in **Project Settings > Player > Scripting Define Symbols**. |
| "Pepper's Mirror adapter needs Mirror 96 or newer" | Update Mirror. The adapter stays off on older versions. |
| FishNet and Mirror in one project: `MirrorUpgrade.cs` doesn't compile | FishNet's tool for moving from Mirror still names a class Mirror 96 removed (`NetworkTransformChild`). Delete `Assets/FishNet/Upgrading/` if you don't need it. |

#### 17.11 Dedicated servers: Inert

A dedicated server runs the same scenes and prefabs as a client, and must play nothing and cost nothing. Set Pepper inert once, at boot, before any Audio Manager starts:

```csharp
// The server's boot code, before the first scene with sounds loads.
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
static void SilenceServer()
{
    if (IsDedicatedServer()) AudioManager.Inert = true;   // your own check
}
```

- **Every post** (`AudioEvent.Post`, `PostMultiPosition`, `AudioManager.Post` and `PostEvent`, `AudioEmitter.Post`) returns one shared, already-finished handle, without allocating and without logging, whether an Audio Manager exists or not. Its `OnFinished` runs at once when you subscribe; `EventName` is empty.
- **An Audio Manager that starts while inert** builds no voices, coroutines or per-frame step, and loads no buses. It still becomes `AudioManager.Instance` and loads the event registry, so code that looks events up (the multiplayer relay) keeps working.
- **The manager's parameter, switch and RTPC calls do nothing.** Values set on emitters and handles are kept as plain data and never stepped. Listening points still register (nothing reads them), so a game's references to them stay valid.
- **The ambient system doesn't create itself** when a zone, portal or ambient source is switched on.
- **It can't change while an Audio Manager runs** (it warns and keeps its value); a server stays a server. It goes back to false at the start of each play session.
- Pepper doesn't detect a server or batch mode by itself (its own tests run in batch mode): your game decides.

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

**LOD:** In this system, the `enableLOD` switch for voice virtualization: once more than Max Real Voices are live, the least important voices are paused and tracked. There are no distance tiers.

**Multi-Position:** Synchronized playback across multiple emitters

**Occlusion:** Raycast-based sound blocking

**Portal:** See AudioPortal.

**Priority:** Voice stealing priority (Low, Medium, High, Critical)

**Propagation:** Zone/portal graph-based routing of long-running ambient beds through world geometry.

**PropagationManager:** Singleton orchestrator of the propagation subsystem. Owns registries, the solver, and the emitter pool. Auto-instantiated on first reference.

**RTPC:** Real-Time Parameter Control, dynamic audio parameter. The string API (`SetRTPC`) sets a parameter's global value by name (§3.4)

**Parameter:** A number the game sets and sounds read (RPM, speed, intensity). Defined by a Parameter asset (unit, range, default) or by name (§3.4)

**Scope (parameter):** Where a parameter value is stored: one play (a handle), one emitter, or global. A sound reads the first that has one

**Audio Emitter:** A component for an object that makes sounds (a car). Sounds posted on it read its parameter values

**Listening point:** Where the game listens from, usually a camera, registered with `AudioManager.AddListeningPoint`. Pepper's distance-based features use it instead of searching the scene; with Spatial Mode = Pepper several weighted points pan and attenuate every 3D sound (split screen) (§9.3)

**Spatial Mode:** Who pans and attenuates 3D sounds: Unity (the default, from its AudioListener) or Pepper (from every listening point) (§9.3)

**Smoothing (slew):** How fast what sounds hear follows a newly written parameter value: Ease (seconds to close 95% of the gap) or Rate (units per second), set on the Parameter asset (§3.4)

**State:** Game mode affecting multiple audio parameters

**Switch:** Conditional container selection

**Virtualization:** Pausing a voice to save a real AudioSource while keeping its place in the clip, so it comes back where it would have been

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
handle.SetVolumeScale(0.5f);
handle.SetPitchOffset(2f);
handle.Stop(fadeTime: 1f);
```

**Bus Control:**
```csharp
AudioManager.Instance.SetBusVolume("Music", -6f, transitionTime: 1f);   // the mix (the asset's Volume)
musicBus.PlayerVolumeDb = -6f;                                           // a Settings page (runtime only)
```

**Cars:**
```csharp
car.VolumeOffsetDb = 2f;                                   // this car's trim, every sound on it
car.Post(impactEvent, localContactPoint, PostValues.Of(strength, 0.8f));
scrape.SetLocalOffset(localContactPoint);                  // a playing sound follows the contact
car.Post(whooshEvent, passDsp - 0.3, 0.2);                 // 0.2 s into the clip, timed to the pass
car.Teleport();                                            // after a reset: no Doppler spike
othersBus.MaxRealVoices = 12;                              // other cars can't crowd out your own
```

**Split screen:**
```csharp
AudioManager.Instance.SpatialMode = AudioSpatialMode.Pepper;          // at boot: Pepper pans, from every point
var p2 = AudioManager.Instance.AddListeningPoint(player2Camera);
p2.Weight = 1f; p2.PanWidth = 0f;                                    // full weight, mono
AudioManager.Instance.SetPrimaryListeningPoint(player1Point);       // where Unity's AudioListener goes
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

1. Right-click in Project → `Create > PepperSFX > Settings > Audio Event Registry`
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
1. `Window > PepperSFX > Authoring > Sound Builder (Simple)`
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
2. ✅ Use Sound Builder (Simple) for new sounds
3. ✅ Consider removing Resources folders entirely
4. ✅ Update registry when adding/removing Events

---

## 6. Build Compatibility

What works on which platform. The SFX System is C# on top of Unity's own audio (AudioSource and AudioMixer) and physics raycasts. Apart from the convolution reverb plugin it has no platform-specific code, so it behaves the same wherever Unity's audio does. The two exceptions are **convolution reverb** and **Web builds**.

### At a Glance

| Platform | What works |
|---|---|
| Windows, 64-bit (x64) | Everything, including convolution reverb |
| macOS, Linux | Everything except convolution reverb |
| iOS, Android, Meta Quest | Everything except convolution reverb |
| Consoles | Everything except convolution reverb |
| Windows on ARM, 32-bit Windows | Everything except convolution reverb |
| Web | Sound plays, but without occlusion muffling, reverb, or propagation filtering. See [Web Builds](#web-builds). |

The free version has the same platform support as the full version.

### Convolution Reverb: 64-bit Windows Only

Convolution reverb ("Use Convolution" on a Reverb Send Bus) runs in a native plugin, `SFXConvolutionReverb.dll`, built for 64-bit Windows only: the Windows editor and Windows x64 player builds.

Everywhere else (macOS, Linux, mobile, consoles, Web, Windows on ARM and 32-bit Windows):

- **A convolution bus is silent.** The dry sound still plays; only that bus's reverb is missing.
- **Unity may log an error** about the missing mixer effect when the game starts.
- **The impulse response isn't uploaded**, and a warning says so.
- **The macOS and Linux editor** don't load the plugin either, so convolution buses are silent there while you work.

Parametric reverb (a Reverb Send Bus without "Use Convolution") works on every platform except Web. If your game ships anywhere other than 64-bit Windows, use parametric buses.

### Web Builds

Unity's Web platform uses its own, smaller audio engine built on the browser's audio. On the AudioMixer it supports groups and their volume, and nothing else: no mixer effects and no sends. Audio filter components (Low Pass, Reverb) aren't on its supported list either. Unity's manual lists the details on its "Audio in Web" page.

What that means for the SFX System:

| Feature | On Web |
|---|---|
| Events, containers, switches, states, RTPCs | Work |
| Buses: volume, mute, solo, ducking | Work |
| Voice pool, virtual voices, priorities, steals | Work |
| Scheduled and beat-synced playback | Work |
| Occlusion | Sounds get quieter behind walls, but aren't muffled. The muffling is a low-pass on mixer effects. |
| Reverb Send Buses, parametric and convolution | No reverb. Sends and reverb are mixer effects. |
| Ambient propagation | Beds still route through zones and portals and follow doors and distance, but aren't muffled and get no reverb. The emitters use Low Pass and Reverb filter components. |
| Multiplayer | Works if your netcode library's transport runs in a browser (WebSockets or WebRTC). The SFX netcode layer sends through that transport and has no network code of its own. |

The browser adds its own rules:

- **No sound until the player interacts.** Browsers block audio until the first click, tap or key press on the page. Sounds posted before that, such as menu music started at scene load, can start late or be missed. Start audio after the first input, or open with a "click to start" screen.
- **No streaming.** Browsers can't stream audio from disk. The Music and Ambience import presets use the Streaming load type, so give those clips a Web override in their import settings (Compressed In Memory).
- **iPhone and iPad.** In Safari on iOS, clips set to Decompress On Load can be silent when the device is in Silent Mode. Use Compressed In Memory for Web.

If Web is a main target, design the mix so it doesn't depend on muffling or reverb, and test in a Web build early.

### Mobile

Every feature works. Occlusion raycasts and the per-voice mixer effects cost more CPU on phones than on desktop. If Unity's Profiler shows audio or physics time climbing, lower **Max Real Voices**, raise **Occlusion Update Interval**, or lower **Occlusion Ray Count** on the AudioManager.

### Requirements

- Unity 6000.0.48f1 or newer.

---

*For API signatures and full type listings, see [API_REFERENCE.md](API_REFERENCE.md).*

*SFX System v3.0.0 — Professional Audio Middleware for Unity*
