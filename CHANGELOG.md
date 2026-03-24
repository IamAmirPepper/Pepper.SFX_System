# Changelog
## [2.1.9] - 2026-03-24
1. AudioManager.GetEvent(string) — look up any registered event by name
2. AudioManager.PostEvent(string, ...) — fire an event by name, returns handle
3. AudioManager.GetEventContainer(string) — get the container behind an event
4. AudioEvent.GetContainer() — returns first Play action's container
5. AudioContainer.ContainerOverrides — per-instance runtime overrides (is3D, minDistance, maxDistance, rolloffMode) that never touch the asset
6. SetOverride3D(), SetOverride(), ClearOverrides() on any container
###