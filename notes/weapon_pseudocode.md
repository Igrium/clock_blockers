# Intro

The main concern with weapon fire is that the original traces will have been performed on a lag-corrected version of the world. We need to make sure that any inconsistencies that arise between the original world and the current world due to this are dealt with.

We don't need to actually check that the held weapon is the same as in the recording because it will have already unlinked if there's an inconsistency. Additionally, the weapon behavior and damage is baked into the recording, so all the child weapon class implements for remnants is visual effects.

# Pseudocode

**Try Weapon Fire** (free agent)

(called by arbitrary gameplay code)

1. Checks to make sure weapon *can* fire

2. Create trace configuration

3. Send to base firearm to perform and calc results

**Fire**
(Base firearm, called by Try Weapon Fire)

- Trace bullet & calculate damage amount

- If recording: Save traces info (see below) into recording

- Do Damage

- Call On Weapon Fire

**Remnant Fire**

(Base firearm, called when remnants fire their weapon)

- Passed list of trace objects

- For each trace:
  
  - Re-execute trace and determine end position
  
  - Identify marked entities that are still present and haven't moved from their recorded position.
    
    - Filter entities that are significantly farther from new end position (shot was probably obstructed)
    
    - Inflict recorded damage
  
  - Clip end position to original end position of ending entity is still consistent with original timeline.
  
  - Apply damage to hit entities *not* in the recording.

- Call On Weapon Fire

**On Weapon Fire** (free agent & remnant)

(Called to create weapon visual effects, etc.)

- Do whatever the fuck the implementing weapon wants in terms of visual effects.

---

**Fire Action (Object)**

*The actual fire action*

- All traces performed in this fire (shotguns, etc)

**Trace (Object)**

*Everything required to recreate the trace **and** its original results*

- Start & end points (`Vector3`)

- Damaged entity(s) (list)

- Damage amount (for non-persistent entities)

- Bullet size

**Entity Damage (struct)**

- Amount

- Entity Persistent ID

- Damage type

- Location

- Force

- True position
  
  > The true (not lag corrected) position the target was at when hit. Used to check for deviation during replay.

**Reload Action** (Object)

- Purely visual; plays the reload animation.

- One for each instance of the reload (shotguns will have multiple in a row)
