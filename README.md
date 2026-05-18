# Pepper's SFX System

**Audio middleware for Unity — events, containers, hierarchical buses, mixer-slot occlusion, per-zone reverb, and portal-driven propagation.**

[![Unity](https://img.shields.io/badge/Unity-6000.0+-black.svg?style=flat&logo=unity)](https://unity.com/)
[![Version](https://img.shields.io/github/package-json/v/IamAmirPepper/Pepper.SFX_System?label=version)](./package.json)
[![License](https://img.shields.io/badge/License-Proprietary-red.svg)](./LICENSE.txt)

<!-- Optional: hero screenshot or short demo gif. The Quick Sound Setup Wizard
     or a scene gizmo of zones/portals would both work well here. -->

---

## Why this exists

Unity's built-in audio is fine for a "play a sound at a position" project. Real games need more: variations that don't sound repetitive, hierarchical volume control that scales to dozens of buses, reverb that changes per room, ambient sounds that route through doors and windows, and a workflow that lets sound designers iterate without touching code.

**Pepper's SFX System** is the layer between your game logic and Unity's audio API that makes those things easy. Sounds are played through **events**, organized through **containers**, mixed through **buses**, and (optionally) routed through a **zone/portal graph** for physically plausible propagation through geometry.

### Before

```csharp
public AudioClip footstepClip;
public AudioSource audioSource;

void PlayFootstep()
{
    audioSource.clip = footstepClip;
    audioSource.Play();
}
```

❌ No variations · ❌ Manual pooling · ❌ No mix structure · ❌ No spatialisation logic

### After

```csharp
public AudioEvent footstepEvent;

void PlayFootstep()
{
    footstepEvent.Post(gameObject, transform.position);
}
```

✅ Built-in variations · ✅ Automatic pooling · ✅ Hierarchical mixing · ✅ Spatial routing comes free

---

## Highlights

### 🎵 Core SFX
- **Events + Containers** — Random (weighted, with repeat avoidance), Sequence (Forward / Reverse / PingPong / Random), Switch (state-driven selection), Blend (RTPC-driven crossfading), Routing (layered playback).
- **Hierarchical Buses** — tree of volume controls with auto-propagating parent volumes, mute/solo, ducking, and Unity AudioMixer integration.
- **States** — group-based state machines that drive bus volumes, RTPCs, switches, and effect parameters in one transition.
- **RTPCs** — real-time parameter controls with smooth transitions and `IRTPCListener` callbacks.
- **Multi-position audio** — one sound playing across N speakers, DSP-scheduled for sample-accurate sync.

### 🌍 Spatial audio
- **Ambient propagation** — zone/portal graph routes long-running ambient beds (rain, wind, machinery) through real geometry. Rain heard *from the window* when you're in another room. Doors that animate smoothly between muffled and clear.
- **Per-zone reverb** — every acoustic space drives its own reverb bus character; SFX automatically sends to the room it's fired in.
- **Mixer-slot occlusion** — voice-side occlusion via pre-built mixer slot pools (no per-source filter rebuild clicks). Multi-ray partial occlusion produces smooth gradients across wall edges instead of binary steps.
- **Soft zone boundaries** — `AudioZone.entryFadeMeters` smooths reverb sends, listener-side character, and `BaseVolumeDb` across the surface.

### 🛠️ Authoring tooling
- **Quick Sound Setup Wizard** — `Window > Audio System > Quick Sound Setup Wizard`. One-click creates Bus + Container + Event with all references pre-filled. 5 modes, 8 preset templates.
- **Occlusion Layout Builder** — `Window > Audio System > Occlusion Layout`. Scans your project, auto-authors the entire per-bus occlusion slot hierarchy on the mixer.
- **Reverb Send Buses Window** — `Window > Audio System > Reverb Send Buses`. Project-wide overview + per-asset Generate / Validate / Apply / Pull workflow.
- **Custom inspectors with live diagnostics** on `AudioZone`, `AudioPortal`, `PropagationManager`, and `ReverbSendBus`.

---

## Editor authoring

Designers don't need to touch code to get the system online. Three main UI surfaces:

<!-- Optional: 3 small screenshots here would be ideal —
     Quick Sound Setup Wizard / Occlusion Layout Builder / Reverb Send Buses window. -->

- **Quick Sound Setup Wizard** — go from 0 to playing-a-sound in under 2 minutes. Bus + Container + Event created in one click, with all references pre-filled.
- **Occlusion Layout Builder** — scans the project for occlusion-using containers, authors the per-bus slot hierarchy on your mixer, exposes the cutoff parameters automatically.
- **Reverb Send Buses Window** — per-room reverb assets with per-asset Generate / Validate / Apply / Pull buttons.

All three windows live under `Window > Audio System >`.

---

## Documentation

Full guides + API reference live in [`Documentation~/`](https://github.com/IamAmirPepper/Pepper.SFX_System/tree/main/Documentation~).

- **[USER_GUIDE.md](https://github.com/IamAmirPepper/Pepper.SFX_System/tree/main/Documentation~/USER_GUIDE.md)** — Introduction → Quick Start → Cookbook → Manual → Migration. One file for everything narrative.
  - **Quick Start** — first sound playing in 2–10 minutes
  - **Cookbook** — copy-paste recipes for footsteps, UI sounds, weapons, music systems, ambient propagation, per-zone reverb, multi-position audio, debugging HUDs
  - **Manual** — architecture, design rationale, every subsystem covered chapter-by-chapter
  - **Migration** — moving from Resources-based projects to the AudioEventRegistry workflow

- **[API_REFERENCE.md](https://github.com/IamAmirPepper/Pepper.SFX_System/tree/main/Documentation~/API_REFERENCE.md)** — every public type, method, property, inspector field. Search-first reference, separate from narrative docs.

---

## Install

**Requirements**: Unity 6000.0 or later. Git installed (Unity Package Manager fetches from Git).

1. **Window → Package Manager → + → Add package from git URL**
2. Paste:
   ```
   https://github.com/IamAmirPepper/Pepper.SFX_System.git
   ```
3. Done. The package appears under "Pepper's SFX System" in the manager.

To pin to a specific version, append `#v2.5.0` (or any tag/branch) to the URL.

---

## What's new in 2.5.0

- **Occlusion Mixer Slot Pool** — voice-side occlusion rewrite. No more per-source filter rebuild clicks.
- **Reverb Send Buses + Per-Zone Reverb** — full per-room reverb subsystem with auto-authoring.
- **BeatScheduler fixes** — tempo wobble and amplitude bumps both resolved; sample-accurate beat spacing.

Full release notes: [CHANGELOG.md](./CHANGELOG.md).

---

## License

**Proprietary.** All rights reserved. Do not distribute or modify without written permission. See [LICENSE.txt](./LICENSE.txt).

For licensing or commercial use, contact the author below.

---

## Author

**Amir Pepper**
[amirpepper96@gmail.com](mailto:amirpepper96@gmail.com)
[github.com/IamAmirPepper](https://github.com/IamAmirPepper)
