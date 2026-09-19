# Operation Cross-Fire: Protecting the Orbital Corridor

A local, same-device 2-player co-op arcade shooter built in Unity. Two players share one interceptor ship for a 60-second round — a Pilot (movement + Boost) and a Gunner (aim, fire, Shield) — with a "Quantum Flux" event at 20s and 40s that swaps their roles and ramps up difficulty.

Full design brief: [docs/game-design-doc.md](docs/game-design-doc.md). Condensed digest, known issues, and design decisions: [knowledge.md](knowledge.md). Architecture rules this project follows: [CLAUDE.md](CLAUDE.md) and [.claude/knowledge/architecture.md](.claude/knowledge/architecture.md).

## Architecture

Four layers, each only calling the one below it: **Input** → **Gameplay** (decides actions, calls services) → **Systems** (all core game logic, no scene-object references) → **UI** (displays state, listens to events). Cross-system communication goes through a static `EventBus`; services are resolved through a `ServiceLocator` by interface, registered once in `GameBootstrapper`; all balance numbers live on ScriptableObjects.

Code layout: `Assets/Project/{Core, Systems, Gameplay, UI, Data, Infrastructure, Bootstrap}`.

| Layer | What lives there |
|---|---|
| **Core** | `ServiceLocator`, `EventBus`, `DataBus`, generic `ObjectPool<T>`/`IObjectPool<T>`, and the whole Input layer (`Core/Input/`): `ShipInputRouter`, `IInputSource` (`KeyboardMouseInputSource`, `TouchInputSource`), and the shared `PilotIntent`/`GunnerIntent` structs |
| **Systems** | `HullService`, `ShieldService`, `ShipMovementService`, `WeaponService`, `EnemySpawnService`, `DebrisSpawnService`, `BreachHazardSpawnService`, `ScoreService`, `RoundService`, `RoleAssignment` — each behind an interface, plus every `EventBus` signal struct (`HullChangedSignal`, `PhaseChangedSignal`, `RoundEndedSignal`, etc.) |
| **Gameplay** | `PilotController`, `GunnerController` (read input, call Systems), `Enemy`/`Debris`/`BreachHazard`/`PlayerLaser`/`EnemyProjectile` (pooled per-instance behaviour), `ShipHullCollider`, `ShieldVisual`, the `IDamageable`/`IShipContactHazard` interfaces, and the small `*SpawnDriver`/`RoundDriver` classes that just call `Tick()` each frame |
| **UI** | `TouchPanel`/`TouchControl` (touch controls) and the HUD: `HullDisplay`, `ScoreDisplay`, `PhaseDisplay`, `RoundTimerDisplay`, `RoleAssignmentDisplay`, `BoostCooldownDisplay`, `ShieldCooldownDisplay`, `QuantumFluxBanner`, `RoundEndScreen` |
| **Data** | One ScriptableObject per tunable system: `ShipMovementData`, `WeaponData`, `HullData`, `ShieldData`, `EnemyData`/`EnemySpawnerData`, `DebrisData`/`DebrisSpawnerData`, `BreachHazardData`/`BreachHazardSpawnerData`, `RoundData` |
| **Bootstrap** | `GameBootstrapper` — the only place any service gets constructed and registered |

**Where the implementation deliberately diverges from the original plan:**
- There's no folder literally named "Input" in the mandated structure, so the whole Input layer lives under `Core/Input/` — Core being the closest fit ("reusable logic, no gameplay").
- `IDamageable` (laser can hit it) and `IShipContactHazard` (deals hull damage + is consumed on ship contact) were introduced when Debris/Breach Hazard needed the same interactions `Enemy` already had. `PlayerLaser` and `ShipHullCollider` check for the interface, not a concrete type — extending to a new hazard type has needed zero changes to either since.
- `HullService` and `ShieldService` never reference each other. Whatever detects a hit (`ShipHullCollider`, or `DebugDamageTrigger` for pre-collision testing) checks `IShieldService.IsActive` itself before calling `IHullService.ApplyDamage` — keeping "no system references another system" literal.
- `RoundService` never calls into `ShipInputRouter`, `RoleAssignment`, or any spawn service directly. Each of those subscribes to `RoundService`'s own signals (`QuantumFluxTriggeredSignal`, `PhaseChangedSignal`, `RoundEndedSignal`) and reacts independently, so the round timer stays a pure publisher with zero outgoing calls.

## Input ownership and role switching

Built on Unity's new Input System (`Co-opMovement.inputactions` → generated `CoopMovement` class) for keyboard/mouse, and the Enhanced Touch API for device touch — unified behind one `IInputSource` interface and merged every frame by `ShipInputRouter` into a single `PilotIntent`/`GunnerIntent` pair that `PilotController`/`GunnerController` read.

- **Keyboard/mouse** (`KeyboardMouseInputSource`): keyboard always drives Pilot actions, mouse always drives Gunner actions — fixed by which `CoopMovement` action map they're bound to, independent of which physical player currently holds that role.
- **Touch** (`TouchInputSource` + `TouchPanel`/`TouchControl`): Player 1 owns the left half of the screen, Player 2 the right — by screen side, never by role. Each `TouchPanel` shows only the control set matching whichever role its player currently holds (`RoleAssignment.RolesChanged` → `Refresh()`); the touch reader itself only ever sees generic `TouchControlId`s (`Left`/`Right`/`Boost`/`AimArea`/`Fire`/`Shield`) and has no concept of "role" at all — the role-swap problem is solved entirely by which buttons a panel displays, not by rerouting input. Each touch is bound by ID to whatever control it began on and re-checked every frame without re-hit-testing, so a finger crossing the centre line never transfers ownership, and a touch can't double as both Shield and Fire/Aim since each zone is exclusive.
- **Quantum Flux** (`RoundService` → `QuantumFluxTriggeredSignal`): `ShipInputRouter` and `RoleAssignment` each subscribe independently — the router calls its own `CancelAll()` (drops held input per-source until released, doesn't just zero it once), `RoleAssignment` calls its own `Swap()` (which fires `RolesChanged`, refreshing every `TouchPanel` and the HUD's `RoleAssignmentDisplay` for free). A 3s warning (`QuantumFluxWarningSignal`) and the "roles reversed" banner (`QuantumFluxBanner`) are separate, UI-only reactions to the same signals.
- **Aim model**: the reticle is tracked as an **offset from the ship**, not an absolute world point. Mouse movement recomputes that offset from wherever the ship currently is; touch drag nudges it directly. This was a deliberate fix — tracking an absolute point meant Pilot movement silently changed the Gunner's firing direction, which felt (and was) wrong for a mechanic where two different people own movement and aim independently.

## Pooling and performance

Every spawned type is pooled through one generic `ObjectPool<T>`/`IObjectPool<T>` (Core layer): player lasers, enemy projectiles, enemies, debris, and breach hazards. Pools are prewarmed in `GameBootstrapper.Awake`, before the round starts; `Launch(...)` on each pooled type resets position, rotation, health, timers, and pool reference on reuse.

A static-analysis PERF AUDIT pass (per `CLAUDE.md`'s routine) found: no LINQ anywhere, no `FindObjectOfType`/`GameObject.Find`, `Camera.main` cached exactly once, `GetComponent` used only twice and both are event-driven `TryGetComponent` calls inside collision callbacks (not per-frame polling), every `EventBus` signal is a `readonly struct` published only on discrete events (never per-frame), and pooled objects stop ticking entirely while inactive (`SetActive(false)` — Unity just doesn't call `Update` on them).

One real risk, not a code bug: `ObjectPool<T>.Get()` falls back to `Object.Instantiate` if a pool is exhausted — allowed by the GDD ("safe expansion is acceptable if documented") but would show up as a live allocation during the Profiler capture if any pool is undersized for Critical-phase spawn rates. Current prewarm counts (enemies 8, debris 8, breach hazards 6, lasers 8, enemy projectiles 6) were sized after computing steady-state on-screen counts from each type's fall speed vs. spawn interval — see [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md) for the arithmetic.

## Progression

One round timer (`RoundService`) drives everything — no separate role-swap/difficulty timers:

| Time | Phase | Player 1 | Player 2 | Difficulty |
|---|---|---|---|---|
| 0–20s | Patrol | Pilot | Gunner | Base enemy speed & spawn interval |
| 20–40s | Alert | Gunner | Pilot | Enemy speed × 1.25; breach hazards begin |
| 40–60s | Critical | Pilot | Gunner | Spawn interval × 0.70 (all three spawners); enemy projectile speed × 1.50 |

Win: timer reaches 60s with hull ≥ 1 (`RoundService` publishes `RoundEndedSignal(won: true)`). Lose: hull hits 0 or a breach hazard reaches the bottom boundary — both are `EventBus` signals `RoundService` subscribes to itself, ending the round immediately regardless of remaining time.

## Setup / device details

- **Unity version**: 6000.0.59f2 (matches `ProjectSettings/ProjectVersion.txt`).
- **Input System**: `com.unity.inputsystem` 1.14.2, Active Input Handling set to "Both."
- Clone the repo, open in Unity 6000.0.59f2, open `Assets/Scenes/SampleScene.unity`, press Play for keyboard/mouse testing.

**TODO**: target device model(s) actually used for the touch/Profiler evidence (Android or iOS, which device) — fill in once that testing pass happens.

## AI usage

Built with Claude Code (Claude Opus 5 / Sonnet 5) under this repo's `CLAUDE.md` working agreement: plan before implementing, one reviewable increment at a time, no code written without an explicit go-ahead on the plan for that step. In practice this meant each system (Pilot movement, Gunner/weapon, Hull/Shield, enemy spawning+pooling, the round/Quantum Flux timer, debris/breach hazard, enemy projectiles, the HUD) landed as its own plan → review → implement cycle, each verified in the Editor before the next started.

Claude-generated: the great majority of the C# under `Assets/Project/` — services, interfaces, signals, pooled-object behaviours, the HUD scripts, and the documentation set (`knowledge.md`, `.claude/knowledge/*`, this README's scaffolding). Claude also diagnosed and fixed two real bugs from Play-mode testing (the aim-direction-changes-with-ship-movement bug, and an `EventBus` design flaw where one throwing subscriber could silently block every other subscriber for that signal) and ran a static PERF AUDIT pass ahead of the Profiler capture.

Hand-built by the developer: the original Input System actions asset and its keyboard/mouse/touch source scripts (`Co-opMovement.inputactions`, `KeyboardMouseInputSource`, `TouchInputSource`, `RoleAssignment`, `ShipInputRouter`, `TouchPanel`/`TouchControl`) — Claude reviewed, relocated to the correct architecture layer, and extended these rather than authoring them from scratch. All scene/prefab wiring, ScriptableObject asset values, and balance tuning (spawn rates, fall speeds) were done directly by the developer, with Claude suggesting starting numbers derived from camera geometry and round timing where the GDD gave none.

## Assumptions

The GDD leaves several things genuinely unspecified. Where an assumption was needed to keep moving, it's recorded here rather than silently baked in:

- **Difficulty multipliers don't reset between phases.** The phase table states each change as "base × N," not "previous × N" — read as: Alert's enemy-speed multiplier (×1.25) carries forward into Critical (which doesn't restate it), and each multiplier is computed against the original base value, not chained.
- **Critical's spawn-interval multiplier (×0.70) applies to all three spawners** (enemy, debris, breach hazard), not just enemies — the GDD names "enemy speed" specifically for the Alert multiplier but says only "spawn interval" (unqualified) for Critical's.
- **Enemy fire pattern**: each enemy fires independently on a fixed interval, straight down, one projectile per shot — the GDD says only "may fire projectiles" with no rate, pattern, or aim behaviour specified.
- **Aim is ship-relative, not an absolute world point** — for both mouse and touch. See the Input section above for why.
- **Breach hazards do not damage the ship on contact.** Re-reading the GDD's own collision list ("enemies, debris, and enemy projectiles against the ship") — breach hazards aren't in it. Their only listed collision is against the bottom boundary. (`knowledge.md` briefly stated this wrong before the correction.)
- **Base numeric values not given by the GDD** (ship speed, playfield width, per-type fall speeds, spawn intervals, invulnerability duration, laser/projectile speed) were derived from camera/screen geometry, then tuned against actual playtesting once the game was running end-to-end. Final values live in each `Data/*.asset`; the derivation is in [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md).

## Incomplete work

- **Profiler evidence** — not yet captured. A static PERF AUDIT pass is done and clean (see Pooling and performance above); the actual on-device Profiler capture during Critical phase, after pool warm-up, still needs to happen.
- **Device evidence** (screenshots + video of simultaneous controls, Boost, Shield, Quantum Flux, difficulty ramp, win/loss) — not yet captured.
- **Physics2D collision layers/matrix** — the GDD asks for the layer matrix to exclude impossible pairs; current collision code is defensive (checks components, not layers) so nothing breaks without it, but the matrix itself hasn't been confirmed set up in Project Settings.
- **`DebugDamageTrigger`** is now redundant — it existed to test Hull/Shield before real collisions (Enemy/Debris/EnemyProjectile) landed. Harmless to leave, safe to delete.
- **Minor, non-blocking**: `RoundService.AdvancePhase()` publishes `PhaseChangedSignal` before `QuantumFluxTriggeredSignal`, meaning "apply new phase values" currently fires a moment before "cancel input / swap roles" — reversed from the GDD's literal step order (1–2 before 3). No observable effect (the phase-value subscribers only update internal multiplier fields), left as-is.
- **Balance values were just re-derived** (see Assumptions) after initial playtesting showed the round was unsurvivable to Critical phase — not yet reconfirmed by a full playtest with the new numbers.
- No restart/replay flow — the GDD doesn't ask for one; a round is currently one-shot per Play session.
