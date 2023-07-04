# Animation Recording & Playback Design Notes

---

## Design Goals

- Efficiently store player gameplay data in a format that doesn't eat memory space and *possibly* can be serialized to disk.

- Correction for dropped ticks / lag

- Easily scrubbable for use in the gameplay inspector
  
  - Random access of the frame at a given timestamp

- Store discrete "events" so they can be located without scanning through frames

## Notes

- Although metadata at the beginning may store the tickrate, it cannot be assumed that it will play back properly at a different tickrate than that at which it was recorded.

- While player position is recorded and may be easily scrubbed through, the state of other entities on the map may be determined procedurally (physics, etc) and therefore may be hard to scrub through.
  
  - Using the demo system is easy but hard to control
  
  - Potentially a separate "map" recording with the position / state of every model (without any gameplay data) which can be easily played back. This would also fix issues with rewinding.

- Likely want to animate velocity as well as position (for prediction)

- Ammo count should not be recorded as this could lead to unwanted unlinks

- Store clothing container with animation because it's serializable 🙂

### Events

- Putting an event table at the beginning of the file makes them easily accessible globally, but is less efficient on a per-frame basis, as each event would have to be checked on each frame.

- Events cannot be *purely* for unlinking purposes as they might impact the animation playback (pressing buttons, etc)

- Do we store the frame they land on or the timestamp? Storing the frame makes it easier during playback, but it makes it harder to determine *when* they occur looking at the file overall.
  
  - Probably timestamp

- On a given frame, how to determine *which* timestamps count as part of that frame?

- Some events impact frames going forward (changing weapon, etc). How to read while scrubbing?


