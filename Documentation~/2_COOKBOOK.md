# SFX System Cookbook

**Version:** 2.1.3
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** January 2026

**Practical step-by-step recipes for common audio tasks**

---

## How to Use This Guide

Each recipe is **self-contained and copy-paste ready**. Find what you need, follow the steps, adapt to your project.

**Prerequisites:** Read QUICK_START.md first.

**Recipe Format:**
- 📋 **What:** Brief description
- 🎯 **Use Case:** When to use this
- ⚙️ **Setup:** Asset configuration
- 💻 **Code:** Complete scripts
- ✅ **Result:** Expected outcome

---

## Table of Contents

1. [Setup & Organization](#setup--organization)
2. [UI Sounds](#ui-sounds)
3. [Footsteps & Movement](#footsteps--movement)
4. [Weapons & Combat](#weapons--combat)
5. [Music Systems](#music-systems)
6. [3D Spatial Audio](#3d-spatial-audio)
7. [States & RTPCs](#states--rtpcs)
8. [Multi-Position Audio](#multi-position-audio)
9. [Performance & Debugging](#performance--debugging)

---

## Setup & Organization

### Recipe: First-Time Project Setup (10 minutes)

📋 **What:** Complete setup from scratch to playing first sound

#### Step 1: Create AudioManager (2 min)

1. Create empty GameObject in scene: "AudioManager"
2. Add component: `AudioManager` (Component → Audio System → Audio Manager)
3. Configure Inspector:
   - Master Volume: 1.0
   - Max Real Voices: 32
   - Max Virtual Voices: 64
   - Enable Occlusion: ✓
   - Enable LOD: ✓

#### Step 2: Create Folder Structure (2 min)

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

#### Step 3: Create Master Bus (1 min)

1. Right-click `Buses/` → Create → Audio System → Audio Bus
2. Name: "Master_Bus"
3. Inspector:
   - Bus Name: "Master"
   - Volume Db: -3
   - Parent Bus: (none)

#### Step 4: Create SFX Bus (1 min)

1. Right-click `Buses/` → Create → Audio System → Audio Bus
2. Name: "SFX_Bus"
3. Inspector:
   - Bus Name: "SFX"
   - Volume Db: 0
   - Parent Bus: Master_Bus

#### Step 5: Create First Container (2 min)

1. Right-click `Containers/` → Create → Audio System → Routing Container
2. Name: "Test_Click_RC"
3. Inspector:
   - Container Name: "TestClick"
   - Audio Clips: Drag in a sound
   - Volume: 1.0
   - Enable Volume Randomization: ✓ (Min: -1, Max: 1)
   - Enable Pitch Randomization: ✓ (Min: -50, Max: 50)

#### Step 6: Create First Event (2 min)

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

### Recipe: Recommended Bus Hierarchy

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

## UI Sounds

### Recipe: Button Click Sound

📋 **What:** Button with click sound and subtle variations

🎯 **Use Case:** UI buttons, menu navigation

#### Asset Setup

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

#### Code

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

### Recipe: Settings Menu with Volume Sliders

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

## Footsteps & Movement

### Recipe: Basic Footsteps with Variations

📋 **What:** Player footsteps with natural variation

🎯 **Use Case:** Character locomotion

#### Asset Setup

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

#### Code (Animation Events)

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

### Recipe: Surface-Dependent Footsteps

📋 **What:** Different footstep sounds per surface type

🎯 **Use Case:** Realistic environmental feedback

#### Asset Setup

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

#### Code

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

### Recipe: Footstep Timer (No Animation Events)

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

## Weapons & Combat

### Recipe: Weapon Fire Sound

📋 **What:** Weapon shot with variations and instance limiting

🎯 **Use Case:** Guns, projectile weapons

#### Asset Setup

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

#### Code

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

### Recipe: Impact Sounds by Material

📋 **What:** Different impact sounds based on hit material

🎯 **Use Case:** Bullet impacts, melee hits, projectile collisions

#### Asset Setup

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

#### Code

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

## Music Systems

### Recipe: Background Music (Intro + Loop)

📋 **What:** Music with intro that transitions to seamless loop

🎯 **Use Case:** Level music, menu music

#### Asset Setup

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

#### Code

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

### Recipe: Dynamic Combat Music (3 Layers)

📋 **What:** Music that responds to combat intensity with layered crossfading

🎯 **Use Case:** Action games, dynamic gameplay music

#### Asset Setup

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

#### Code

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

## 3D Spatial Audio

### Recipe: Looping Campfire Sound

📋 **What:** 3D looping ambient sound attached to object

🎯 **Use Case:** Fire, machines, environmental loops

#### Asset Setup

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

#### Code

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

## States & RTPCs

### Recipe: Underwater State

📋 **What:** Complete underwater audio effect

🎯 **Use Case:** Swimming, diving, underwater sections

#### Asset Setup

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

#### Code

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

### Recipe: Vehicle Engine RPM

📋 **What:** Engine sound that changes with speed/RPM

🎯 **Use Case:** Cars, motorcycles, aircraft

#### Asset Setup

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

#### Code

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

## Multi-Position Audio

### Recipe: Nightclub Speaker Setup

📋 **What:** 8 speakers playing synchronized music

🎯 **Use Case:** Nightclubs, concerts, speaker arrays

#### Asset Setup

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

#### Scene Setup

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

#### Code

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
            musicHandle = musicEvent.PostMulti(emitterParent);

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

## Performance & Debugging

### Recipe: Voice Count Display

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
            $"Real Voices: {stats.realVoices} / {stats.maxRealVoices}",
            style);
        y += lineHeight;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Virtual Voices: {stats.virtualVoices}",
            style);
        y += lineHeight;

        GUI.Label(new Rect(10, y, 500, lineHeight),
            $"Total Active: {stats.realVoices + stats.virtualVoices}",
            style);
        y += lineHeight;

        // Voice usage bar
        float usagePercent = (float)stats.realVoices / stats.maxRealVoices;
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

### Recipe: Event Callbacks for Debugging

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

## Appendix: Quick Reference

### Common Tasks Cheat Sheet

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
AudioMultiHandle mh = event.PostMulti(emitterParent);
mh.SetVolume(0.8f);
mh.Stop(2f);
```

---

**End of Cookbook**

---

*For deep system knowledge, see MANUAL.md*
*For API documentation, see API_REFERENCE.md*
*For quick start, see QUICK_START.md*

*SFX System v1.2.0 - Professional Audio Middleware for Unity*
