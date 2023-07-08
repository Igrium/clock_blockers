# Design Goals

- Efficiently store player gameplay data in a format that doesn't eat memory space and *possibly* can be serialized to disk.

- Correction for dropped ticks / lag

- Easily scrubbable for use in the gameplay inspector
  
  - Random access of the frame at a given timestamp

- Store discrete "events" so they can be located without scanning through frames

# Notes

- Although metadata at the beginning may store the tickrate, it cannot be assumed that it will play back properly at a different tickrate than that at which it was recorded.

- While player position is recorded and may be easily scrubbed through, the state of other entities on the map may be determined procedurally (physics, etc) and therefore may be hard to scrub through.
  
  - Using the demo system is easy but hard to control
  
  - Potentially a separate "map" recording with the position / state of every model (without any gameplay data) which can be easily played back. This would also fix issues with rewinding.

- Likely want to animate velocity as well as position (for prediction)

- Ammo count should not be recorded as this could lead to unwanted unlinks

- Store clothing container with animation because it's serializable 🙂

- How to keep track of non-player entities that may impact animation for unlinking purposes? (grenades, etc.)
  
  - Entity component storing some kind of ID, which gets added to the animation?

- Do we keep track of remnant health or just when they're killed?
  
  - We need to in order to allow free agents to kill them. The question is whether remnants damaging remnants should be tracked.

## Events

- Putting an event table at the beginning of the file makes them easily accessible globally, but is less efficient on a per-frame basis, as each event would have to be checked on each frame.

- Events cannot be *purely* for unlinking purposes as they might impact the animation playback (pressing buttons, etc)

- Do we store the frame they land on or the timestamp? Storing the frame makes it easier during playback, but it makes it harder to determine *when* they occur looking at the file overall.
  
  - Probably timestamp

- On a given frame, how to determine *which* timestamps count as part of that frame?

- Some events impact frames going forward (changing weapon, etc). How to read while scrubbing?

**Solution:** Two types of events:

- *Events*
  
  - Indexed at the beginning of file with timecode
  
  - Not stored with frame
  
  - Must have a corresponding *action* in order to be seen during playback
  
  - Action failed: unlink
  
  - *ex: player killed, grenade thrown, button pressed, etc*

- *Actions*
  
  - Stored as part of individual frames
  
  - Represent discrete animation data (rather than continuous such as position)
  
  - Only used in animation playback
  
  - Not indexed
  
  - *ex: jump, shoot, button press (animation)*

## Damage Tracking & Unlinks

Damage & health pose a unique challenge when it comes to handling remnants. While a remnant's health must be tracked so it can interact with free agents, its time of death also needs to remain predictable (unless interfered with).

- Store which entities were damaged by how much in `Shoot` action. That way, even if the weapon itself acts non-deterministically (random bullet spread, etc.) it can be ensured that the damage done to other remnants remains consistent.

- Do we use auto health regen so accidental damage doesn't cause unpredictability down the line?

- How much non-canon damage causes an unlink (if any)?

It's also important to decide how deviations from the canon are handled when it comes to *infliction.* For instance, if a remnant is supposed to kill another remnant, but the other remnant has slightly more health this time around, what happens? If we consider the amount of damage inflicted canon, then it will cause an unlink even if the other remnant still died (bad). However, only considering kills canon can fail to account for other scenarios where there was a scrimmage but no kill.

- Fights that end slightly early should not cause an unlink, but fights that end *too* early should.

- Auto health regen could help contain unknown variables within a single encounter.

Ultimately, it makes sense to consider a "attack" and its outcome canon. However, it could be hard to determine where an attack starts and ends. For instance, if someone starts spraying a minigun at their opponent, and only hit some of their shots but still manage to kill them, what happens? 

- Possible solution: consider the beginning of the attack and the final kill canon, and give a small time buffer for when the kill happens.

### Player intent

It will be important to come up with a system to distinguish player intent. For instance, a player firing randomly and accidentally hitting an explosive barrel needs to be treated differently than if they intended to shoot that barrel.

- Potentially based on the outcome of the explosion. Did it kill someone?

## What to store in a frame

- Frame actions

- Position

- Rotation

- Eye rotation

- Velocity

## Map / gameplay ideas

- Mounted minigun that can be destroyed.

- Elevator / moving walkway with two possible positions. Wrong position -> unlink

- Can only carry one weapon at a time, but can swap with weapons on the ground (from dead players or placed in map).
  
  - Missing weapon -> unlink

- Doors that only open if another agent is standing on or holding a button

- Explosive barrels that can only be triggered once per round.
