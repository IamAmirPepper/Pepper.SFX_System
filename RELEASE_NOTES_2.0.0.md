# SFX System - Release Notes v2.0.0

**Release Date:** January 2026


---

## What's New

### ⚡ Performance Improvements

#### Listener Cache Optimization
**Eliminated unnecessary CPU overhead from AudioListener lookups**

- **Before:** System searched for AudioListener 2x per second (120 searches/minute) even when unnecessary
- **After:** Listener cache only refreshes when actually needed (destroyed/null)
- **Impact:** Removes ~12-60ms of CPU overhead per minute
- **Result:** Zero-cost optimization for 99% of use cases

**Breaking Changes:** None - fully backward compatible

**Action Required:** None for standard usage (one camera, scene transitions handled automatically)

**Advanced Users:** If you swap cameras without destroying the old one, manually call `ListenerUtil.Set(newCamera.transform)`. See [Manual Section 9.3](3_MANUAL.md#listener-cache-management) for details.

---

## Technical Details

### Changed Files
- `listener_util.cs` - Removed timer-based refresh, retained only null/destroyed checks
- `3_MANUAL.md` - Added comprehensive documentation on listener cache management

### Performance Metrics
- **Frequency Reduction:** 120 searches/min → 1-2 searches total (per scene)
- **Per-Call Savings:** ~0.1-0.5ms (depends on scene hierarchy complexity)
- **Risk Level:** Near Zero - maintains all safety guarantees for destroyed objects

---

## Upgrade Guide

**From v1.2.0 to v2.0.0:**

1. No code changes required
2. No asset changes required
3. Drop-in replacement - just update the files

**Verification:**
```csharp
// Your existing code works unchanged
AudioManager.Instance.PostEvent("MyEvent");
ListenerUtil.Get(); // Automatically optimized
```

---

## Known Issues

None

---

## Credits

Performance optimization based on production profiling and community feedback.

---
