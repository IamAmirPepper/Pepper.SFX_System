# Multi-Position Audio System

## Overview

The Multi-Position Audio System allows you to play a single sound from multiple positions simultaneously, with all sources perfectly synchronized. This is ideal for scenarios like:

- **Nightclub speakers** - Music playing from multiple speakers around a dance floor
- **Stadium announcements** - Voice coming from several speaker positions
- **Ambient sound sources** - Rain/wind effects emanating from multiple points
- **Surround sound setups** - Cinematic audio from positioned sources

Inspired by Wwise's multi-positioning feature, this system provides both component-based (designer-friendly) and code-based (runtime/procedural) approaches.

---

## Key Features

✅ **Synchronized playback** - All voices start at exactly the same time using DSP scheduling
✅ **Component-based workflow** - Visual placement of emitters in Unity Editor
✅ **Per-emitter control** - Individual volume, enable/disable, directionality per speaker
✅ **Dynamic position updates** - Move emitters at runtime
✅ **Unified control** - Control all voices together or individually
✅ **Gizmo visualization** - See emitters and connections in Scene view
✅ **Voice pooling integration** - Uses existing AudioManager voice pool
✅ **Automatic virtualization** - Less important positions can be virtualized

---

## Quick Start

### 1. Component-Based Approach (Recommended)

This is the easiest and most designer-friendly method.

#### Setup in Editor:

1. Create an empty GameObject (e.g., "Nightclub Speakers")
2. Add `AudioMultiPositionEmitterParent` component
3. Create child GameObjects for each speaker position
4. Add `AudioMultiPositionEmitterChild` component to each child
5. Position and rotate children in Scene view
6. Adjust per-child settings (volume, directionality, etc.)

**Tip:** Right-click the parent component and use "Create 4 Child Emitters (Square Pattern)" or "Create 8 Child Emitters (Circle Pattern)" for quick setup!

#### Usage in Code:

```csharp
using AudioSystem;

public class NightclubManager : MonoBehaviour
{
    [SerializeField] private AudioEvent musicEvent;
    [SerializeField] private AudioMultiPositionEmitterParent speakerSystem;

    private void Start()
    {
        // Use PostMultiPosition to get multi-position functionality
        AudioMultiHandle multiHandle = musicEvent.PostMultiPosition(speakerSystem);

        // Now you can control all speakers together or individually
        multiHandle.SetVolume(0.8f);
        multiHandle.SetVoiceVolume(0, 0.5f); // Adjust individual speaker
    }
}
```

---

### 2. Array-Based Approach (For Runtime/Procedural)

Use this when you need to create positions dynamically at runtime.

#### Using Vector3 Array:

```csharp
using AudioSystem;

public class ProceduralAudio : MonoBehaviour
{
    [SerializeField] private AudioEvent audioEvent;

    private void Start()
    {
        // Define positions programmatically
        Vector3[] speakerPositions = new Vector3[]
        {
            new Vector3(-5f, 0, -5f),  // Back Left
            new Vector3(5f, 0, -5f),   // Back Right
            new Vector3(-5f, 0, 5f),   // Front Left
            new Vector3(5f, 0, 5f)     // Front Right
        };

        // Play from all positions simultaneously
        AudioMultiHandle handle = audioEvent.PostMultiPosition(speakerPositions);
    }
}
```

#### Using Transform Array:

```csharp
[SerializeField] private Transform[] movingSpeakers;

private void PlayAudio()
{
    // Useful when you want to track moving transforms
    AudioMultiHandle handle = audioEvent.PostMultiPosition(movingSpeakers);

    // Positions will update automatically if you call RefreshPositions()
    StartCoroutine(UpdatePositionsRoutine(handle));
}

private IEnumerator UpdatePositionsRoutine(AudioMultiHandle handle)
{
    while (handle.isPlaying)
    {
        handle.RefreshPositions(); // Update from stored transforms
        yield return new WaitForSeconds(0.1f);
    }
}
```

---

## API Reference

### AudioEvent Methods

```csharp
// Multi-position with Vector3 array (max 16 positions)
AudioMultiHandle PostMultiPosition(Vector3[] positions, GameObject source = null)

// Multi-position with Transform array
AudioMultiHandle PostMultiPosition(Transform[] transforms, GameObject source = null)

// Multi-position with emitter parent component (recommended)
AudioMultiHandle PostMultiPosition(AudioMultiPositionEmitterParent emitterParent, GameObject source = null)
```

---

### AudioMultiHandle Control Methods

#### Global Control (All Voices)

```csharp
void SetVolume(float linear)              // Set volume for all voices (0-1)
void SetPitch(float semitones)            // Set pitch for all voices in semitones
void Stop(float fadeTime = 0.1f)          // Stop all voices with fade
void Pause()                               // Pause all voices
void Resume()                              // Resume all voices
```

#### Per-Voice Control

```csharp
void SetVoiceVolume(int voiceIndex, float volumeMultiplier)  // Control individual voice volume
void SetVoicePitch(int voiceIndex, float semitones)          // Control individual voice pitch
void StopVoice(int voiceIndex, float fadeTime = 0.1f)        // Stop specific voice
void PauseVoice(int voiceIndex)                              // Pause specific voice
void ResumeVoice(int voiceIndex)                             // Resume specific voice
```

#### Position Updates

```csharp
void UpdatePositions(Vector3[] newPositions)    // Update all positions with new array
void UpdatePositions(Transform[] newTransforms) // Update all positions with new transforms
void RefreshPositions()                         // Refresh from stored transforms
```

#### Properties

```csharp
bool isPlaying      // Whether any voice is playing
float time          // Current playback time
float duration      // Total duration of audio
int VoiceCount      // Number of active voices
```

---

### AudioMultiPositionEmitterParent Component

#### Inspector Settings

- **Max Emitters** - Maximum child emitters allowed (1-16)
- **Auto Find Children** - Automatically detect children on Start
- **Auto Update Children** - Update list when children change
- **Show Debug Info** - Log debug information to console
- **Show Gizmos** - Visualize emitters in Scene view

#### Public Methods

```csharp
void RefreshChildren()                          // Manually refresh child emitter list
void EnableAllEmitters()                        // Enable all child emitters
void DisableAllEmitters()                       // Disable all child emitters
void SetAllVolumes(float volume)                // Set volume for all children

Vector3[] GetActivePositions()                  // Get positions of active emitters
Transform[] GetActiveTransforms()               // Get transforms of active emitters
IReadOnlyList<AudioMultiPositionEmitterChild> ActiveEmitters  // Get active emitter list
```

---

### AudioMultiPositionEmitterChild Component

#### Inspector Settings

- **Is Active** - Enable/disable this emitter at runtime
- **Volume Multiplier** - Volume multiplier for this specific emitter (0-1)
- **Use Directionality** - Enable directional emission
- **Cone Angle** - Cone angle for directional emission (0-360°)
- **Gizmo Color** - Color for this emitter's gizmo visualization
- **Show Gizmos** - Show gizmos in Scene view

#### Properties

```csharp
bool IsActive                // Whether this emitter is active
float VolumeMultiplier       // Volume multiplier (0-1)
bool UseDirectionality       // Whether directional emission is enabled
float ConeAngle              // Cone angle for directionality
Vector3 Position             // World position
Quaternion Rotation          // World rotation
Vector3 Forward              // Forward direction
```

---

## Advanced Usage Examples

### Example 1: Nightclub with Individual Speaker Control

```csharp
public class NightclubController : MonoBehaviour
{
    [SerializeField] private AudioEvent musicEvent;
    [SerializeField] private AudioMultiPositionEmitterParent speakers;

    private AudioMultiHandle musicHandle;

    public void PlayMusic()
    {
        musicHandle = musicEvent.PostMultiPosition(speakers);
    }

    public void MuteBackSpeakers()
    {
        // Mute speakers 0 and 1 (back left and right)
        musicHandle.SetVoiceVolume(0, 0f);
        musicHandle.SetVoiceVolume(1, 0f);
    }

    public void AdjustVolumeByDistance(Vector3 listenerPos)
    {
        var emitters = speakers.ActiveEmitters;
        for (int i = 0; i < emitters.Count; i++)
        {
            float distance = Vector3.Distance(listenerPos, emitters[i].Position);
            float volume = Mathf.Clamp01(1f - (distance / 20f));
            musicHandle.SetVoiceVolume(i, volume);
        }
    }
}
```

---

### Example 2: Dynamic Moving Speakers

```csharp
public class MovingSpeakerSystem : MonoBehaviour
{
    [SerializeField] private AudioEvent ambientEvent;
    [SerializeField] private Transform[] drones; // Flying drones with speakers
    [SerializeField] private float moveSpeed = 2f;

    private AudioMultiHandle handle;

    private void Start()
    {
        handle = ambientEvent.PostMultiPosition(drones);
        StartCoroutine(MoveDrones());
    }

    private IEnumerator MoveDrones()
    {
        while (handle.isPlaying)
        {
            // Move drones
            foreach (var drone in drones)
            {
                drone.position += Vector3.up * Mathf.Sin(Time.time * moveSpeed) * Time.deltaTime;
            }

            // Update audio positions to follow drones
            handle.RefreshPositions();

            yield return null;
        }
    }
}
```

---

### Example 3: Stadium Announcements with Delay

```csharp
public class StadiumAnnouncementSystem : MonoBehaviour
{
    [SerializeField] private AudioEvent announcementEvent;
    [SerializeField] private AudioMultiPositionEmitterParent speakerArray;

    public void PlayAnnouncement()
    {
        var handle = announcementEvent.PostMultiPosition(speakerArray);

        // Add echo effect by delaying distant speakers
        StartCoroutine(AddEchoEffect(handle));
    }

    private IEnumerator AddEchoEffect(AudioMultiHandle handle)
    {
        var emitters = speakerArray.ActiveEmitters;
        Vector3 center = speakerArray.transform.position;

        for (int i = 0; i < emitters.Count; i++)
        {
            float distance = Vector3.Distance(center, emitters[i].Position);
            float delay = distance * 0.003f; // ~3ms per meter (speed of sound approximation)

            yield return new WaitForSeconds(delay);
            handle.ResumeVoice(i);
        }
    }
}
```

---

## Best Practices

### Performance

1. **Limit emitter count** - Keep to 16 or fewer for optimal performance
2. **Use voice virtualization** - Less important positions will automatically virtualize
3. **Reuse emitter components** - Create prefabs for common speaker setups
4. **Avoid excessive RefreshPositions()** - Only call when transforms actually move

### Design

1. **Use directionality** - Enable cone angles for realistic speaker behavior
2. **Adjust per-emitter volumes** - Match real-world speaker power differences
3. **Visual placement** - Use Scene view gizmos to position speakers accurately
4. **Test in-game** - Audio spatialization feels different than Scene view

### Synchronization

1. **DSP scheduling** - All voices start sample-accurately synchronized
2. **Same audio clip** - Multi-position works best with identical clips
3. **Loop alignment** - Looping audio remains synchronized across all positions

---

## Troubleshooting

### Problem: Audio not playing from all positions

**Solution:** Check that all `AudioMultiPositionEmitterChild` components have `IsActive = true` and the parent has refreshed its children.

### Problem: Voices are out of sync

**Solution:** This shouldn't happen with DSP scheduling. Verify that all voices are using the same AudioClip and the audio event is configured correctly.

### Problem: Some positions are silent

**Solution:** Check voice virtualization settings in AudioManager. Increase max real voices if needed, or increase emitter priority.

### Problem: Can't see emitters in Scene view

**Solution:** Enable "Show Gizmos" in both parent and child components, and ensure Gizmos are enabled in Scene view toolbar.

### Problem: Performance issues with many emitters

**Solution:** Reduce emitter count, increase voice virtualization threshold, or use fewer simultaneous multi-position events.

---

## Technical Details

### Synchronization Method

All voices in a multi-position playback are scheduled using Unity's `AudioSource.PlayScheduled(dspTime)` method, ensuring sample-accurate synchronization across all positions.

### Voice Management

Each position spawns a separate `AudioVoiceEnhanced` from the AudioManager's voice pool. These voices share:
- Same audio clip
- Same start time (DSP scheduled)
- Same container settings
- Individual spatial positioning

### Memory & CPU

- **Per-emitter overhead:** ~1KB + 1 AudioSource
- **CPU impact:** Similar to playing N separate sounds, but with synchronized scheduling
- **Memory sharing:** Audio clip data is shared across all voices (no duplication)

---

## Comparison with Wwise

This implementation is inspired by Wwise's multi-positioning feature with some differences:

| Feature | SFX System | Wwise |
|---------|------------|-------|
| Max positions | 16 | 16 |
| DSP sync | ✅ Yes | ✅ Yes |
| Component-based | ✅ Yes | ❌ No |
| Dynamic updates | ✅ Yes | ✅ Yes |
| Per-position volume | ✅ Yes | ✅ Yes |
| Directionality | ✅ Yes | ✅ Yes |
| Attenuation sharing | ✅ Automatic | ✅ Yes |

---

## Future Enhancements

Potential future additions to the system:

- [ ] Attenuation curve sharing across positions
- [ ] Multi-position for music containers with transitions
- [ ] Listener-relative positioning modes
- [ ] Position spread/randomization
- [ ] Time offset per position (for echo effects)
- [ ] Spatialization mode per emitter

---

## Support

For questions, issues, or feature requests related to Multi-Position Audio:

1. Check this documentation
2. Review [AudioMultiPositionExample.cs](../Examples/AudioMultiPositionExample.cs)
3. Examine the source code in `/RunTime/Utilities/AudioMultiPosition*.cs`

---

**Created:** December 2025
**Version:** 1.0
**Compatible with:** SFX System v1.0+
