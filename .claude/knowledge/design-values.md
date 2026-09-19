# Design Values — Operation Cross-Fire

Every numeric value the GDD specifies, laid out the way it should end up in ScriptableObjects, plus every value it does **not** specify. Per `CLAUDE.md`: *"Balance numbers come from ScriptableObjects. Never invent a constant — if a value is missing, ask."* Nothing marked "not specified" below should be filled in with a guessed number — propose the field and ask the user for the value, as `CLAUDE.md`'s "Working with Unity assets" section describes.

## Ship / Hull

| Value | Default | Source |
|---|---|---|
| Starting hull points | 3 | GDD, "Game layout and rules" |
| Hull damage per unshielded hit | 1 (flat, regardless of attacker) | GDD |
| Post-hit invulnerability duration | **not specified** — "a short configurable invulnerability period" only | GDD |
| Ship base movement speed | **not specified** | — |
| Playfield horizontal bounds | **not specified** (implementation-dependent on camera/screen setup) | — |
| Round length | 60s | GDD |

## Boost (Pilot only)

| Value | Default |
|---|---|
| Duration | 1s |
| Movement speed multiplier | 1.75× |
| Cooldown | 4s |
| Grants invulnerability? | No |

## Shield (Gunner only)

| Value | Default |
|---|---|
| Duration | 1.5s |
| Cooldown | 5s |
| Effect | Prevents all hull damage while active |

## Weapon (player laser)

| Value | Default |
|---|---|
| Fire cooldown | 0.25s (must not be bypassable by rapid input) |
| Aim behaviour | Toward the reticle |
| Laser travel speed | **not specified** |

## Round / phase timing (single round timer drives all of this)

| Phase | Window | Player 1 role | Player 2 role |
|---|---|---|---|
| Patrol | 0–20s | Pilot | Gunner |
| Alert | 20–40s | Gunner | Pilot |
| Critical | 40–60s | Pilot | Gunner |

| Difficulty value | Patrol (base) | Alert | Critical |
|---|---|---|---|
| Enemy move speed | base (**value not specified**) | base × 1.25 | base × 1.25 (carried forward — not restated, see assumption below) |
| Spawn interval | base (**value not specified**) | base (unchanged) | base × 0.70 |
| Enemy projectile speed | base (**value not specified**) | base (unchanged) | base × 1.50 |
| Breach hazards active? | No | Yes, starts appearing | Yes |

**Assumption to confirm with the user**: the table states each change as "base × N", not "previous × N". Adopted reading — a phase that doesn't restate a multiplier keeps whatever the previous phase set (multipliers don't silently reset), and each multiplier is computed against the *original* base value, not chained. Confirm before the round/phase Systems code locks this in — see `knowledge.md` known issue #4.

Countdown: "QUANTUM FLUX IN 3… 2… 1…" shown for the 3 seconds immediately before each transition (20s and 40s marks). Input stays live during the countdown; cancellation happens exactly at the transition instant.

## Gameplay objects

| Object | Health (laser hits) | Hull damage on contact | Score value | Appears from |
|---|---|---|---|---|
| Enemy | 1 | 1 | 10 | Patrol |
| Debris | 2 | 1 | 15 | Patrol |
| Breach hazard | 3 | 1 (also: reaching bottom = instant loss) | 25 | Alert |
| Enemy projectile | n/a (despawns on hit/cleanup) | 1 | — | Patrol (fired by enemies) |
| Player laser | n/a | n/a | — | n/a |

| Object motion value | Status |
|---|---|
| Enemy fall speed | Uses "Enemy move speed" above — base not specified |
| Debris fall speed | **not specified** (GDD never gives debris its own speed, only "moves downward") |
| Breach hazard fall speed | **not specified** |
| Enemy fire rate / pattern | **not specified** ("may fire projectiles" only) |

## Pooling

| Value | Status |
|---|---|
| Pool sizes (per type) | **Not numerically specified.** GDD says "size pools for the Critical phase; safe expansion is acceptable if documented." Actual counts depend on the missing spawn-interval and object-lifetime values above — compute once those are set (roughly: concurrent-on-screen count ≈ spawn rate × average lifetime, with headroom). |
| Prewarm timing | Before round start (explicit requirement) |

## Summary — values that must be asked for before the Systems layer can be fully implemented

1. Ship base movement speed.
2. Base enemy move speed (Patrol phase).
3. Base spawn interval (Patrol phase).
4. Base enemy projectile speed.
5. Debris fall speed.
6. Breach hazard fall speed.
7. Enemy fire rate/pattern (frequency, aimed vs. straight-down, projectile count).
8. Post-hit invulnerability duration.
9. Player laser travel speed.
10. Playfield/camera bounds the ship clamps to.

None of these block building the architecture (services, interfaces, ScriptableObject schemas, pooling scaffolding) — they only block final tuning. Expose all of them as `[SerializeField]` fields on the relevant Data ScriptableObjects with obvious placeholder-free names, and flag to the user that these specific fields need real numbers before the game is genuinely playable/balanced.
