**Try Weapon Fire** (free agent)

(called by arbitrary gameplay code)

1. Checks to make sure weapon *can* fire

2. Create trace configuration

3. Send to base firearm to perform and calc results

**Fire**
(Base firearm, called by Try Weapon Fire)

**On Weapon Fire** (free agent & remnant)

(Called to create weapon visual effects, etc.)

---

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


