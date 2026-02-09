# SFX System - Quick Start Guide

**Version:** 2.1.3
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** January 2026

**Get your first sound playing in 2-10 minutes**

---

## 📚 Documentation Roadmap

Welcome! Here's how our documentation is organized:

1. **Quick Start** (you are here) - Get started fast (2-10 min)
2. **[Cookbook](2_COOKBOOK.md)** - Practical recipes for common tasks
3. **[Manual](3_MANUAL.md)** - Deep understanding of the system
4. **[API Reference](4_API_REFERENCE.md)** - Complete code documentation
5. **[Advanced Migration](5_ADVANCED_MIGRATION_GUIDE.md)** - Registry system migration
6. **[Changelog](6_CHANGELOG_WORKFLOW_IMPROVEMENTS.md)** - Technical improvements

---

## What is the SFX System?

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

## Choose Your Path

Pick the setup method that fits your workflow:

### Method 1: Quick Wizard ⭐ (2 minutes)
**Best for:** Beginners, rapid prototyping
**Time:** 2 minutes
**Skill:** Easy

[Jump to Quick Wizard](#method-1-quick-wizard)

### Method 2: Manual Setup (10 minutes)
**Best for:** Learning the system, custom control
**Time:** 10 minutes
**Skill:** Medium

[Jump to Manual Setup](#method-2-manual-setup)

### Method 3: Registry System (Advanced)
**Best for:** Large projects, eliminating Resources folder
**Time:** Setup + migration
**Skill:** Advanced

[See Manual: Registry System](3_MANUAL.md#registry-system) for full details.

---

## Method 1: Quick Wizard

**The absolute fastest way to add sounds - perfect for beginners!**

### Step 1: Add AudioManager to Scene (30 seconds)
1. Create empty GameObject in your scene
2. Add `AudioManager` component
3. Done! (Auto-configures itself)

### Step 2: Create Sound Assets (1 minute)
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

### Step 3: Play in Code (30 seconds)
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

**What's Next?** See [Cookbook](2_COOKBOOK.md) for more recipes or jump to [Troubleshooting](#troubleshooting).

---

## Method 2: Manual Setup

**Traditional workflow - full control over every step**

### Step 1: Add AudioManager (2 minutes)

1. In your scene, create a new empty GameObject
2. Name it "AudioManager"
3. Add the `AudioManager` component
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

**Why Resources folder?** AudioManager auto-loads Events and States from `Resources/Audio/Events` and `Resources/Audio/States` at startup.

📘 **Note:** See [Manual: Folder Requirements](3_MANUAL.md#folder-requirements) for a detailed explanation of why this structure is needed.

---

### Step 3: Create Your First Bus (2 minutes)

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

⚠️ **Important:** The Inspector will show a warning if the event is not in the Resources folder. Click the "Move to Resources" button if you see this warning.

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

---

## 🎉 Congratulations!

You've successfully set up the complete audio pipeline:

**Your Code** → **AudioEvent** → **Container** → **Voice** → **Bus** → **Speaker**

---

## Troubleshooting

### "AudioEvent not playing!"
**Check Inspector warnings:**
- Open your AudioEvent asset
- Look for red/yellow warning boxes
- If it says "NOT in Resources folder" → Click "Move to Resources/Audio/Events/"

### "AudioManager not found!"
- Ensure AudioManager is in scene
- Check it persists (DontDestroyOnLoad is automatic)

### "No AudioEvents found in Resources/Audio/Events"
- Events MUST be in `Assets/Audio/Resources/Audio/Events/` folder
- OR use [AudioEventRegistry system](3_MANUAL.md#registry-system)

### Sound Too Quiet/Loud?
**Check:**
1. Container volume (0-1)
2. Bus volume (in dB, 0 = no change)
3. Master volume on AudioManager
4. Unity's AudioListener volume

---

## Quick Reference

### Minimal Code Example
```csharp
using AudioSystem;

public AudioEvent mySound;

void Start()
{
    mySound.Post(gameObject, transform.position);
}
```

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

---

## What's Next?

### Level 1 - You are here! ✓
- Basic sound playback
- Simple events and containers
- Bus volume control

### Level 2 - [Cookbook](2_COOKBOOK.md)
- Footsteps with variations
- UI sound systems
- Weapon audio
- Music systems
- 3D spatial audio

### Level 3 - [Manual](3_MANUAL.md)
- Deep system understanding
- Advanced features
- Performance optimization
- Registry system

### Level 4 - [API Reference](4_API_REFERENCE.md)
- Complete API documentation
- All classes and methods
- Advanced use cases

---

## Comparison: Which Method Should I Use?

| Method | Best For | Setup Time | Flexibility | Learning |
|--------|----------|------------|-------------|----------|
| **Quick Wizard** | Beginners, rapid prototyping | 2 min | Low | Easy |
| **Manual Setup** | Learning system, custom control | 10 min | Medium | Best |
| **Registry System** | Large projects, no Resources | 15 min | High | Advanced |

**Recommendation:**
- **New users:** Start with Quick Wizard
- **Learning the system:** Use Manual Setup
- **After 10+ sounds:** Consider Registry System (see [Manual](3_MANUAL.md#registry-system))

---

## Key Concepts to Remember

1. **Events are in Resources** - Must be in `Assets/Audio/Resources/Audio/Events/` to auto-load (or use registry)
2. **States are in Resources** - Must be in `Assets/Audio/Resources/Audio/States/` to auto-load (or use registry)
3. **One AudioManager per project** - It persists between scenes
4. **Buses use dB** - 0 dB = no change, -6 dB ≈ half as loud, -80 dB = silence
5. **Voice limit** - 32 real voices by default (configurable)

---

**Happy Sound Design!** 🎵

*For more examples, see [Code Examples](../Examples/) folder*
