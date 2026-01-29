# Advanced Migration Guide - Eliminate Resources Folder Requirement

**Version:** 2.1.1
**Unity Compatibility:** 6000.0.48f1 and above
**Last Updated:** January 2026

**Level:** Advanced • **Time:** 10-15 minutes • **Optional but Recommended**

This guide helps you migrate from the old Resources-based workflow to the new Registry system.

---

## Why Migrate?

### Current Pain Points (Resources-based)
❌ Events MUST be in `Assets/Audio/Resources/Audio/Events/` (confusing, easy to mess up)
❌ States MUST be in `Assets/Audio/Resources/Audio/States/`
❌ Resources folder increases build size
❌ No validation when placed incorrectly (silent failures)
❌ Tedious folder management

### Benefits After Migration (Registry-based)
✅ Place Events/States ANYWHERE in your project
✅ Better organization
✅ Smaller build size
✅ Clear validation and errors
✅ Version control friendly
✅ Backwards compatible (no breaking changes)

---

## Migration Process (5-10 minutes)

### Step 1: Create AudioEventRegistry

1. Right-click in Project → `Create > Audio System > Audio Event Registry`
2. Name it exactly: **"AudioEventRegistry"**
3. Place in: `Assets/Audio/Resources/AudioEventRegistry.asset`

**Important:** The registry itself should be in a Resources folder so AudioManager can auto-find it at runtime.

### Step 2: Populate Registry from Existing Assets

1. Select the `AudioEventRegistry` asset
2. In Inspector, click **"Populate from Resources Folders"**
3. Verify: Registry should now show all your existing Events and States

Console output:
```
✓ Registry populated: 45 events, 12 states
```

### Step 3: Test That Everything Still Works

1. Enter Play Mode
2. Check Console for:
   ```
   Loading audio assets from AudioEventRegistry
   ✓ Loaded from Registry: 45 events, 12 states
   ```
3. Test your sounds - they should work exactly as before

**No code changes needed!** AudioManager automatically detects the registry.

### Step 4: Move Events/States Out of Resources (Optional)

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

### Step 5: Validate Registry

1. Select `AudioEventRegistry`
2. Click **"Validate Registry (Remove Nulls)"**
3. Check Console:
   ```
   ✓ Registry validated: No issues found
   ```

---

## Migration Checklist

- [ ] AudioEventRegistry created
- [ ] Registry placed in Resources folder
- [ ] Registry populated from Resources
- [ ] Tested in Play Mode (sounds work)
- [ ] (Optional) Moved Events/States out of Resources
- [ ] (Optional) Updated registry from entire project
- [ ] Registry validated

---

## Backwards Compatibility

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

## Adding New Sounds After Migration

### Option 1: Quick Wizard (Recommended)
1. `Window > Audio System > Quick Sound Setup Wizard`
2. Create sound assets
3. Open `AudioEventRegistry`
4. Click **"Populate from Entire Project"**

### Option 2: Manual
1. Create AudioEvent anywhere in project
2. Open `AudioEventRegistry`
3. Drag new Event into "Registered Events" array
4. OR click **"Populate from Entire Project"**

**Tip:** Set up a post-processing script to auto-update registry when Events are created.

---

## Common Migration Issues

### Issue: "No AudioEvents found in Resources/Audio/Events"
**Cause:** Registry not found, falling back to Resources.LoadAll
**Fix:** Ensure registry is named "AudioEventRegistry" and placed in `Assets/Audio/Resources/AudioEventRegistry.asset`

### Issue: "Loaded 0 events" after migration
**Cause:** Registry is empty
**Fix:** Select registry → Click "Populate from Entire Project"

### Issue: Some sounds don't play after migration
**Cause:** Events not registered
**Fix:** Open registry → Click "Validate Registry" → Re-populate

### Issue: Registry has null references
**Cause:** Events were deleted but registry not updated
**Fix:** Click "Validate Registry (Remove Nulls)"

---

## Performance Comparison

### Before (Resources-based)
- `Resources.LoadAll<AudioEvent>("Audio/Events")` at startup
- All Resources assets included in build
- Load time: ~50-200ms (project dependent)

### After (Registry-based)
- Direct reference lookup (no scanning)
- Only registered assets in build
- Load time: ~5-20ms (instant)

**Result:** Faster startup, smaller builds

---

## Team Workflow

### Before Migration
❌ "Where do Events go again? Assets/Audio/Resources/Audio/Events?"
❌ "Why isn't my sound playing?" (placed in wrong folder)
❌ "Do I need Resources nested inside Audio folder?"

### After Migration
✅ "Events can go anywhere, just update the registry"
✅ Inspector shows clear warnings if Event not registered
✅ Single registry asset, auto-detected

---

## Advanced: Custom Registry Locations

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

## Rollback Plan

If migration causes issues, you can instantly rollback:

1. Delete or move `AudioEventRegistry` out of Resources
2. AudioManager will fallback to old `Resources.LoadAll` method
3. Everything works as before

**No code changes needed for rollback!**

---

## Summary

### What Changes:
- AudioEvents/States can live anywhere (not just Resources)
- AudioEventRegistry tracks all assets
- AudioManager auto-detects registry

### What Stays the Same:
- All existing code works unchanged
- AudioEvent.Post() API identical
- Container/Bus workflow unchanged
- Old Resources-based projects still work

### Time Investment:
- Initial migration: **5-10 minutes**
- Per new sound after: **+10 seconds** (update registry)

### ROI:
- Eliminates #1 confusion point for new users
- Better project organization
- Faster builds and startup

---

## Next Steps

After migration:
1. ✅ Update team documentation
2. ✅ Use Quick Sound Setup Wizard for new sounds
3. ✅ Consider removing Resources folders entirely
4. ✅ Update registry when adding/removing Events

---

**Questions?** See [3_MANUAL.md](3_MANUAL.md) or examples in [Assets/Scripts/SFX_System/RunTime/Examples/](../Examples/)
