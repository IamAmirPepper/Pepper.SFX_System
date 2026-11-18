# SFX System Beta: Known Issues

This is a heads-up for beta testers: things that may not work as intended in this build yet, and places where the guides are out of date. If you hit one of these, it's already known. If you hit something that isn't listed, please report it.

**Version:** 3.1.2
Last updated: November 2026

---

## Where the guides are wrong

- None known. From 3.1.0 the free version's Documentation~ folder carries the same guides as the full version.

## Things that might not work

- **Convolution reverb** ("Use Convolution" on a Reverb Send Bus) only works on 64-bit Windows, not Windows on ARM. On any other platform the bus stays silent, and you may get an error when the game starts. The normal reverb works everywhere except Web builds. See "Build Compatibility" in the User Guide.
- **Web builds** play sound, but occlusion doesn't muffle and there's no reverb, because Unity's Web audio has no mixer effects. See "Build Compatibility" in the User Guide.
- **Mac and Linux editor.** The console error about SFXConvolutionReverb.dll when the editor opens should be gone in this build. If you still see it, please report it.
- **Automatic mixer setup** in the Occlusion Layout and Reverb Send Buses windows has only been tested on Unity 6000.0.48f1 and may fail on newer versions. If it does, click "Diagnose Reflection" in the Occlusion Layout window and send us the console output.
- **Sound Builder (Simple)** (formerly the Quick Sound Setup Wizard) always warns that events must be in a Resources folder. You can ignore that if you use an Audio Event Registry. Its preview play button may do nothing.
- **Icons.** Custom icons on SFX assets and components may not show. This is cosmetic only.
- **Upgrade From Free** (moving a project from free to full without losing anything) is its own package from GitHub, installed next to free; neither version contains it. Its README gives the steps, and peppersfx.com/get links to it. It has been through one real upgrade so far, and the rest of its tests are still to do: back up your project before you run it, even though it zips every file it changes first.
- **Mirror adapter.** New in 3.1.1 and only tested with a host so far (its automated tests run the server and the host's own player in one process). Sessions with several players haven't been checked yet: please report anything that sounds wrong, such as a sound heard twice, a missed late-join loop, or a departed player's loop that keeps playing.
- **Unity version.** You need Unity 6000.0.48f1 or newer. Package Manager will let you install on older 6000.0 versions, but those aren't supported. The package's automated tests pass on 6000.0.48f1 and 6000.3.19f1.
- **Unity 6.3: `AudioExtensions` in your own scripts.** Unity 6.3 added its own `UnityEngine.AudioExtensions`. A script with both `using UnityEngine;` and `using AudioSystem;` that calls `AudioExtensions.LinearToDb` (or `DbToLinear`, `CentsToPitch`) stops compiling with error CS0104. Write `AudioSystem.AudioExtensions.LinearToDb`, or add `using AudioExtensions = AudioSystem.AudioExtensions;` to that file.

## Known limitations

- **Stop actions.** A Stop action in an event only stops sounds that same event started. It can't stop another event's sound, such as music started elsewhere. To stop a sound, keep the handle you got from Post and call Stop on it. Its Scope (All or This Emitter) only narrows which of that event's sounds it reaches.
- **Removing many cars at once.** An Audio Emitter set to Stop keeps its persistent sounds' voices until their fade ends, so removing dozens of such objects in the same frame can fill the voice pool and skip a sound (with a warning). Shorten their Stop Fade Seconds if your game does that.
- **Sequence Containers.** Each Sequence Container asset keeps a single position, so two objects playing the same one share it. If you need two independent sequences, duplicate the asset.
- **Stopping a container.** Calling Stop on a container stops every sound playing from that container asset. To stop just one of them, stop its handle.
- **Multi-position emitter cones.** Use Directionality and Cone Angle on an Audio Multi-Position Emitter Child only draw a cone in the Scene view. They don't change the sound: every emitter is heard the same in every direction. Use Volume Multiplier to make one quieter.
- **Big bursts of sounds.** If a very large number of sounds start in the same frame, some are skipped (with a warning) when the voice pool can't free voices fast enough. Nothing is cut for a sound that is then skipped. Raise Steal Headroom on the Audio Manager if it happens often.
- **Decorrelated multi-position playback** may click on sharp sounds like gunshots.
- **Spatial Mode = Pepper** has no spatializer plugins (HRTF), doesn't read the project's Doppler Factor, and balances a stereo clip rather than placing it (give 3D sounds mono clips). Unity's own AudioSources (the ambient system's) are still heard from the AudioListener: keep it on the primary point with `DriveUnityListener`. Changing the mode affects sounds started after the change.

## Changes since 3.1.0

- **Menus.** Everything is under **PepperSFX** now, in categories, instead of one long "Audio System" list:
  - **Window > PepperSFX**: Authoring, Mixing, Engines, Loading, Multiplayer, Debug and Setup.
  - **Create > PepperSFX**: the Sound Builders and Audio Event at the top, then Containers, States and Parameters, Buses and Settings.
  - **Add Component > PepperSFX**: the Audio Manager (new to the menu), the emitters and the Beat Scheduler, then Propagation, Loading and Multiplayer. They used to sit under "Audio", "Audio System" and "SFX System".
  - **Tools > PepperSFX** for the diagnostics.

  The windows' tabs carry the same names as the menu items. The Mixer's tab is "PepperSFX Mixer", so it isn't mistaken for Unity's own Audio Mixer. A window docked before you updated keeps its old tab name until you close and reopen it.
- **Welcome window.** It opens by itself the first time you open a project with Pepper.SFX, and once after each update to a new release, with the first steps (add an Audio Manager, open Sound Builder (Simple), the code to post an event) and links to the guides. Untick "Show this window when Pepper.SFX is installed or updated" if you don't want that; Window > PepperSFX > Setup > Welcome opens it any time.
- **Mirror adapter (full version).** New: `MirrorAudioTransport`, for Mirror 96 or newer. On Mirror, your listener provider and validator receive Pepper's client ids (host 0, then 1, 2, 3 …), not Mirror's connection ids, which clients never learn. See Manual §17.7.

## Changes since 3.0.0

- **Volume on handles.** SetVolume and SetVoiceVolume still work but show "obsolete" warnings. Use SetVolumeScale (relative to the authored volume) or SetVolumeAbsolute. Per emitter, use SetVoiceVolumeScale.
- **Blend Containers.** Posting a Blend Container event again now plays a second copy alongside the first. If you want it to restart instead, set the event's Max Instances to 1.
- **Reverse sequences.** Sequence Containers in Reverse mode now start from the last clip.
- **Multi-position handles.** The per-emitter methods (SetVoiceVolumeScale, StopVoice and the others) now take the emitter number, not a voice number.
- **Multi-position posts with a source.** If you pass a source GameObject to PostMultiPosition, each 3D voice now plays at its own position, attached to the source so it moves with it. Before, every voice played from the source itself and the positions were ignored.
- **Equal Power crossfades.** The outgoing sound now fades out along a true equal-power curve, so loudness stays steady through the crossfade. It used to drop away early, so the old sound is now noticeably louder through the middle of the fade. Linear crossfades are unchanged.
- **CrossFade actions and buses.** A CrossFade action with a Target Bus now routes the incoming sound through that bus, so it follows the bus's volume slider, mute and ducking. Before, it ignored the bus. With no Target Bus nothing changes.
- **Fade Duration on Play actions.** Play actions never used it. Sound Builder (Simple) no longer shows it for Play.
- **Delayed actions.** An action with a Delay that isn't a Play (CrossFade, Stop, SetState, SetRTPC, SetSwitch, ducking, Pause, Resume) used to be skipped whenever nothing from that post was still playing when the delay ended. They now run. Only stopping the post's handle cancels them. If an event of yours has such an action that "never did anything", it will now.
- **CrossFade actions** place a 3D sound at the position or GameObject passed to Post, instead of the world origin. A crossfaded-in Sequence's later clips now get the Target Bus too. Crossfading back while a sound is still fading in no longer makes it jump up first.
- **Handle crossfade.** New `AudioManager.CrossFade(handle, event, duration)`: crossfades from a sound you hold the handle of, and also cancels what it was still going to start, such as the loop of an intro + loop event.
- **Sound Builder (Simple)** now asks for the Fade From Container when you make a Crossfade event. Without one, the crossfade did nothing.
- **LOD Distances** on the Audio Manager are hidden. They never did anything: voices are only virtualized once there are more than Max Real Voices.
- **Pitch on handles.** SetPitch and SetVoicePitch still work but show "obsolete" warnings. They replace the container's pitch randomization, as before; that is now called SetPitchAbsolute. The new SetPitchOffset (and SetVoicePitchOffset) shifts the pitch on top of the randomization instead. Containers also have a new **Pitch Limit**, off by default.
- **ConvolutionReverbDevTester** was removed. It was a temporary development harness for the convolution reverb. If you added it to a scene, that scene now shows a "missing script" on the object; remove the component.
- **Parameters per object and per sound.** New Parameter assets, the Audio Emitter component and per-handle values let two objects play the same Blend with different values (two cars, two RPMs). `SetRTPC` and the other string calls keep working: they set the global value, which every sound without its own value follows. See "Parameters & Scopes" (Manual §3.4).
- **Blend Containers follow their parameter once a frame.** A `SetRTPC` used to move the Blend's layers inside the call. They now move in Pepper's per-frame update, after every LateUpdate and before the frame's audio, so it is still the same frame. Code that reads a layer's gain straight after `SetRTPC` sees the old value until then. RTPC listeners are still told inside the call. `BlendContainer.UpdateBlend` is obsolete: it sets the parameter's global value.
- **Nested Blend Containers multiply.** A layer under two Blends now gets the product of both curves. Before, whichever parameter had changed last decided its level.
- **Later clips in a Blend or Switch.** A Sequence inside a Blend or Switch now plays its later clips at the Blend's curve and the Blend's and Switch's Volume, like its first clip. Before, they came in at full level, so such a sound may now be quieter after its first clip.
- **Curves, regions and pitch follow.** Containers can have Parameter Curves (volume, pitch, low- and high-pass), and Blends can use a Regions layout and Pitch Follow for engines. All of it is off until you add it, so existing sounds don't change.
- **Parent Settings.** Containers have two new options, Uses Parent Positioning and Uses Parent Output, for taking a Blend's or Switch's 3D settings or Mixer Group. They are off, so nothing changes unless you tick them.
- **TransitionRTPC replaces.** A second `TransitionRTPC` on the same name (two quick state changes, for example) now replaces the first, and a `SetRTPC` ends a running transition. Before, they ran together and fought.
- **Parameter smoothing.** Parameter assets can ease their values (Smoothing on the asset). It only applies to values set through the asset, so `SetRTPC` code sounds exactly as before.
- **Example 06 (vehicle engine)** now takes a Parameter asset and uses its Smoothing instead of its own "Rpm Transition Speed" field, which is gone. An Audio Emitter is added to the car with it. Without a Parameter asset it falls back to the old name, set globally and not smoothed.
- **Listening points.** You can now tell Pepper where you listen from: `AudioManager.Instance.AddListeningPoint(cameraTransform)`. If you don't, everything works as before, but a warning appears once per play session: "No listening point is registered". Code that calls `ListenerUtil.Set` counts as registering one.
- **`Post(null)`.** Calling `myEvent.Post(null)` with a bare `null` no longer compiles, because the new `Post(AudioEmitter)` matches it too. Write `myEvent.Post()`.
- **Virtual voices.** A sound that went virtual (more sounds than Max Real Voices) now comes back exactly where it would have been at any pitch. It used to come back off its place when pitched (randomization, pitch offsets, engine RPM). It also fades out over a few frames before going silent and fades back in, instead of cutting, so for those frames one more sound than Max Real Voices can be heard. A virtual one-shot at a raised pitch ends sooner than before, when its clip would have.
- **Stealing ends a whole sound.** When the voice pool is full, Pepper now ends whole sounds to make room (all of a Blend's layers, all of a Routing container's clips), never one layer of a sound while the rest plays on. A sound that can't get all its voices is skipped before anything is cut for it.
- **Starting while the real voices are full.** A new sound that matters less than everything already playing now starts virtual (silent, keeping its place) instead of pushing a more important sound out. Before, a new sound always started real. And an important virtual sound now comes back while the real voices are full, taking the place of one that clearly matters less.
- **Blend layers start on one exact time**, one audio buffer (a few milliseconds) later than a single sound starts, so they are sample-aligned.
- **Blend Loop Mode and Silent Layers.** The Blend's Loop checkbox, which was never read, is replaced by **Loop Mode** (From Layers, as before; Force Loop; Force One Shot). Sound Builder (Simple) sets Force Loop when its Loop is ticked. **Silent Layers = Virtualize** frees the real voices of layers that are near silence (engines); the default, Keep Playing, is as before.
- **Persistent events.** Tick Persistent on an event (an engine) so its sounds are never stolen. Off by default.
- **Unity's voice limits.** Pepper warns at startup if its Max Real Voices, or its total voices, are above Unity's own (Project Settings > Audio), and sets each voice's `AudioSource.priority` from its event.
- **MakeVirtual from your code.** `voice.MakeVirtual()` now keeps the voice virtual until you call `MakeReal()`; the voice budget no longer brings it back by itself.
- **Priority of reused voices.** A container played directly from code (not through an event) now always starts at High priority, as on a fresh voice, instead of whatever the reused voice's last sound had.
- **Posting with no Audio Manager** now logs a warning once per event and no longer uses up the event's cooldown.
- **Ambient system listener.** Zones, portals and ambient sources now listen from the listening point you registered with `AudioManager.AddListeningPoint`, not from the first AudioListener in the scene. If you register a point somewhere other than your AudioListener, the ambience follows the point. Its Listener Override field still wins.
- **Leaving out multiplayer.** New define `SFXSYSTEM_DISABLE_NETCODE` (Project Settings > Player > Scripting Define Symbols) leaves the multiplayer code out of your project. Nothing changes unless you set it.
- **Named points on an Audio Emitter.** An Audio Emitter can list named points (a car's Exhaust, Intake, axles): `car.Post(exhaustEvent, "Exhaust")` plays the sound attached at that spot. Drag the points in the Scene view. An Audio Emitter without points works as before. See "Named points" (Manual §3.4).
- **Switches per car, and while playing.** A switch (a surface) can now be set on an Audio Emitter or a handle, not only globally, and a Switch Container can follow a change while it plays: **When Switch Changes** = Crossfade or Restart. The default, Next Play Only, is how Switches always worked. New **Switch Group** assets list a group's values; `SetSwitch("Surface", ...)` keeps working and is the same switch. A group can also be driven by a parameter. See "When Switch Changes" and "Switch Group assets" (Manual §4.5).
- **`SwitchContainer.SetSwitch`** used to do nothing until that container had played once. It now works straight away.
- **A Sequence in a far-away sound.** When a sound has gone virtual (silent because it's far away or less important), its Sequence's next clip now starts virtual too. Before, it started audible and could push a more important sound out for half a second.
- **Distance assets and Distance/Approach Speed/Angle.** New **Attenuation** assets describe how 3D sounds fade, dull, thin and narrow with distance, shared between containers; a sound past its Max Distance goes silent (virtual) until it comes back in range. Parameter assets can take their value from Distance, Approach Speed or Angle. Containers without an Attenuation asset work as before. See "Attenuation assets" (Manual §4.1) and "Built-in values" (§3.4).
- **Audition.** New Window > PepperSFX > Authoring > Audition plays a container while you edit it, with sliders for its parameters, a sweep and a live layer table, outside Play mode. Please report anything it leaves behind or any sound that differs from Play mode beyond the known limits (heard 2D; Sequences play their first clip). See Manual §10.5.
- **Audio's random numbers are its own.** Which variation plays, and volume and pitch randomization, no longer use Unity's random generator: each Audio Emitter has its own sequence (with an optional Random Seed), other sounds share one. Your gameplay's random sequence is no longer changed by sounds playing. If you relied on `Random.InitState` to get the same audio picks, set the Audio Emitter's Random Seed instead, or set the Audio Manager's Random Numbers to Unity. Multiplayer posts work as before.
- **No-repeat per car.** A Random container's Avoid Repeat Last now applies per Audio Emitter: each car avoids its own recent clips.
- **Per-car limits and values at post.** Events have Max Instances Per Emitter and Cooldown Per Emitter (off by default), and a post can carry values for its sound (`PostValues`). See "Per Audio Emitter" (Manual §5.3) and "Values at post" (§3.4).
- **Bus curves.** A bus can now follow a parameter: its volume, and a low- or high-pass on its Mixer Group (the bus inspector adds the filter for you). Buses without curves are unchanged. See "Parameter Curves on a Bus" (Manual §6.6).
- **`container.MaxDistance = x`** in code now changes only the running game. Before, in the editor it changed the asset itself and stayed after Play.
- **`SetSwitch(null, ...)`** with a bare `null` group no longer compiles, because the new asset overload matches it too. Pass a group name or asset.
- **Doppler Level** on containers (3D Settings), default 1 as before. Set it to 0 on engine layers so they stay in step when they go virtual and come back. Sound Builder (Simple)'s Doppler Level slider now sets it; before, it did nothing.
- **Switching the Audio Manager off.** The pooled voices now live under "AudioVoices", a root object of Pepper's own, which also runs Pepper's background work. Switching the Audio Manager's GameObject off no longer stops anything; before, it stopped Pepper for good. Use `PauseAll`, a bus `Pause` or `StopAllSounds` to silence the game.
- **Persist Across Scenes** on the Audio Manager (on by default, as before). Turn it off to keep the manager under another object; a second manager then removes only its own component instead of its whole GameObject. See "Memory & Lifecycle" (Manual §2.4).
- **Dedicated servers.** `AudioManager.Inert = true`, set at boot, makes every post silent and free, with or without an Audio Manager. See Manual §17.11.
- **Cars that come and go.** An Audio Emitter's **On Disable** (Keep, the default and as before; Stop; Pause) decides what its sounds do when the object is switched off or destroyed: fade out where they are, or pause and come back in phase. See "Switched off or destroyed" (Manual §3.4).
- **Stop, Pause and Resume actions** have a **Scope**: All (the default, as before) or This Emitter, so one car's post doesn't reach every car's sounds.
- **Pausing a bus.** `bus.Pause()` / `Resume()` hold a bus's sounds (and its child buses'), faded, without a real voice, while other buses (a pause menu) play on. See "Pausing a Bus" (Manual §6.7).
- **Time on events, Ducking Time on buses.** Scaled by default, as before; Unscaled lets a UI sound's cooldown, a delay, a Sequence gap, a crossfade or a duck run while the time scale is 0.
- **The "was skipped" warning** shows at most every 5 s, with a count.
- **Finished handles** ignore new `OnStarted`, `OnStopping` and `OnLoop` subscriptions; they could never fire.
- **Tools for authoring engines and mixes.** The Audition plays events too, runs several sweeps at once, and replays a drive recorded in the new **Live View** (Play mode: every Audio Emitter's values and sounds, and why a voice is virtual). New **Loop Analysis** (a loop's RPM, seam and loudness) and **Engine Builder** (an engine's whole tree from its loops). See Manual §10.5–§10.9. Please report anything they get wrong.
- **Container Validator** checks events too, and Blends' regions, pitch and Doppler. It no longer reports "No mixer group" on every container: a Play action with neither a Target Bus nor a Mixer Group is what it warns about now.
- **Events outside the registry.** The event Inspector no longer says an event "will NOT load at runtime", and no warning is logged on import: an event held in a field plays wherever it is. Only posting by name or id needs the registry.
- **Import settings.** The package no longer ships an Audio Import Settings asset (it couldn't be saved once installed). Make one in your project (Create > PepperSFX > Settings > Import Settings) to change the presets; without one the defaults apply, the same values the shipped asset had. New **Folder Rules** apply settings to clips first imported into a folder.
- **Bus faders, mute and solo** in the Mixer and Audio Profiler windows can be undone, and are saved with the project, also when moved in Play mode.
- **Audio Profiler and Voice Pool Debug.** Voice Usage is now real voices against Max Real Voices (it read 50% at the cap); the pool window compares its objects with the pool's whole size.
- **Handle volume and pitch reach later voices.** `SetVolumeScale` and `SetPitchOffset` on a handle now also reach the voices the sound starts later: a Switch's new choice, a Sequence's next clip, a Play action with a Delay. Before, those started at the authored level and pitch. `SetVolumeAbsolute` and `SetPitchAbsolute` still reach only the voices playing now. New: `SetVolumeOffset` (dB), and both offsets can be given at the post (`PostValues.WithVolumeOffset`, `WithPitchOffset`).
- **Overlapping ducks: the deepest wins.** Each trigger is now a duck of its own and the bus follows the deepest, so a short or shallow duck no longer cuts a long or deep one short (a UI blip during a dialogue duck used to bring the music back up under the dialogue). Re-triggering the same duck sounds as it did.
- **`SetBusVolume` with a name no bus has** now changes nothing and warns once. Before, it set the master bus. `GetBus` with such a name still returns the master bus, now with a warning.
- **Level and pitch per Audio Emitter.** New **Volume Offset** (dB) and **Pitch Offset** (cents) on an Audio Emitter reach every sound on it. 0 by default, so nothing changes.
- **Posting anywhere on an Audio Emitter, and moving a sound.** `car.Post(impactEvent, localOffset, values)` plays at that spot with the car's values and limits; `handle.SetLocalOffset` / `SetWorldPosition` move a playing sound. See "Posting at an offset" (Manual §3.4).
- **Timed posts and lengths.** `Post(emitter, startDsp, startOffsetSeconds, values)` makes a moment of the sound land on a DSP time (a pass-by's peak on the pass); `EstimateDuration` tells how long an event will play. See Manual §5.1.
- **Pepper's Doppler.** A container's new **Doppler** setting can be Pepper (exaggeration, maximum shift) instead of Unity's (the default, as before). With it, sounds that go virtual come back in step. `AudioEmitter.Teleport()` and `AudioListeningPoint.ClearVelocity()` keep a reset or a camera cut from reading as speed. See "Doppler" (Manual §4.1).
- **Offset states and the player's volume.** A state's **Bus Volume Mode** can be Offset: its buses move by that much and go back when the state ends (`ClearState` leaves a group). Set Level, the default, works as before. A bus's **Player Volume** (`PlayerVolumeDb`) is a Settings page's volume, never saved and never undone by a state. Also `MasterVolumeDb`, `IsAllMuted` and `AudioBus.IsMuted`. See Manual §6.8 and §7.2. Example 08 now sets player volumes.
- **Low priority counts.** A Low sound weighs 1/8 in how important it is (Medium 1/4, High 1/2, Critical 1, as before). It used to weigh 0, so a Low sound right beside you went virtual, or was stolen, before a Medium one you could hardly hear. See Manual §3.2.
- **Steals fade.** A sound stolen to make room, a play an event's Max Instances ends, and a Switch set to Restart's old choice now fade out over the Audio Manager's new **Steal Fade Seconds** (0.025 s, at least three frames) instead of cutting, which could click. A stolen sound keeps its voice a few frames longer while it fades; if bursts of sounds start getting skipped, raise Steal Headroom. Steal Fade Seconds 0 brings back the cut. A sound stolen before it has started now frees its voice at once.
- **A listening point whose camera is destroyed is removed,** with a warning once per session, instead of staying where the camera was. Remove your points when their camera goes. See "Listening Points" (Manual §9.3).
- **Split screen.** The Audio Manager's new **Spatial Mode** can be Pepper: Pepper then pans and attenuates 3D sounds itself, from every listening point by its Weight and Pan Width, and sounds exactly as Unity's 3D at one point. Unity, the default, works as before. You can pick the primary listening point (`SetPrimaryListeningPoint`), and a camera change no longer reads as speed. See "Listening Points" (Manual §9.3).
- **A bus can cap its real voices** (**Max Real Voices**, 0 = none by default), so a group of sounds can't crowd out the rest. See Manual §6.9.
- **Voice filters.** Every pooled voice now carries its low- and high-pass from the start, switched off (a switched-off filter isn't heard), instead of adding them mid-game the first time a filter curve needed them.
- **A parameter value that isn't a number** (NaN, or an infinity, say from a division by zero in game code) is now refused with a warning once per parameter, and sounds keep the last value. Before, it stayed until the value was snapped, and a Blend played every band at once. If you see the warning, fix the code that computes the value.
- **Engine Builder.** Clips it folds to mono keep their level: it turns Unity's Normalize off on them, which used to level every loop. The engine event it makes is Priority High. Trees built before keep their settings.
- **Engine loops and Cylinders.** The Engine Builder makes its loop containers with Avoid Repeat Last 0, and a one-clip Random container no longer warns about Avoid Repeat Last (it never stopped a clip playing). Cylinders is a scale, not always the engine's count: when most loops read 2 semitones or more from the RPMs in their names (set it per library), Loop Analysis, the Engine Builder and Detect Root Values name the count that fits, and flag a single loop that reads an octave off. See Manual §10.7 and §10.8.
- **Sounds on a bus with no slots.** The Occlusion Layout window now makes slots for every bus whose sounds use Use Occlusion, Allow Reverb Send or Use Propagation (it used to count Use Occlusion only, so the other two could play with no reverb send and no low-pass). Click its Scan, Auto-Create, Refresh again to add them. When an Occlusion Layout is assigned and a sound still plays through a bus it has no slots for, the console warns once per bus.

## Full version only

- **Multiplayer** is tested with FishNet v4. Netcode for GameObjects 2.x has had only light testing so far, so expect rough edges. NGO 1.x and FishNet v3 cause compile errors.
- **Addressables: one group for every zone saves no memory.** Unity can only unload a bundle as a whole, so with every zone's clips in one Pack Together group (like the Default Group), leaving a zone frees nothing. The Addressable Audio Authoring window and Sound Builder now put each bank's clips in its own group by default. If you converted zones into the Default Group with an earlier build, the window's Groups tab flags it and can switch it to one bundle per clip. See "Authoring window" in the User Guide.
- **Phase-locked loops (Loop Sync Tier 2)** are experimental. A player who joins late may hear the loop out of sync for one or two cycles before it locks in.
- **Proximity culling** can use the wrong range for events that play a Switch or Blend container, so a nearby player might not get the sound.
- **Sound Builder (Advanced)** is brand new, so expect rough edges. The free version has Sound Builder (Simple).
- **Engine Builder** is in the full version only. The free version keeps Loop Analysis and the Blend inspector's Detect Root Values, so you can still build an engine's tree by hand (User Guide §10.8).
- **Generated engines** (Loop, Granular and Additive Engines, Manual §4.7, the Engine Grains window and Loop Sets) are in the full version only, and need Unity 6000.3 or later. They haven't been measured on a Steam Deck yet (on a desktop PC about 0.4% of a core each for the granular and additive engines, and up to 1.3% for a Loop Engine with both sets in a crossfade). Their grains are resampled with the simplest interpolation, which may dull very high harmonics. Grain tables cut by Engine Grains in 3.1.0-dev.10 or earlier from a sweep that rises or falls evenly in pitch (most synthesisers) were cut along a straight line, so their grains aren't whole engine cycles: analyse them again with **Curve: Even Pitch** (Manual §10.10). In 3.1.0-dev.11 and earlier a held RPM warbled once an engine cycle, worst at low RPM, and a rev climbed in uneven steps: both fixed after dev.11, with no need to cut the tables again. Since then a generated engine moves to each frame's RPM over the time that frame took, so it hears the RPM about a frame and two audio buffers late (25–75 ms).
- **Audio Profiler.** "Est. CPU" is worked out from the voice count, not measured, and "Managed heap" is all of your game's managed memory, not just audio. Use Unity's Profiler for real audio figures.
- **Example scripts** work with either input setting (old Input Manager, new Input System, or Both). The audio settings UI example (Example 08) needs the Unity UI package; without it, that one example is left out.
- **Import Manager settings** live in an Audio Import Settings asset in your project, so an update keeps them. Without one, the code defaults apply.
- **Mixer window.** It creates Assets/Settings/Audio/MixerLayout.asset by itself to save its layout.

---

## Reporting a problem

Please include:

- your Unity version
- your platform (Windows, Mac or Linux)
- whether you're on the free or full version
- the steps to reproduce the problem
- any console errors
