# Operation Cross-Fire: Protecting the Orbital Corridor

A local, same-device 2-player co-op arcade shooter built in Unity. Two players share one interceptor ship for a 60-second round — a Pilot (movement + Boost) and a Gunner (aim, fire, Shield) — with a "Quantum Flux" event at 20s and 40s that swaps their roles and ramps up difficulty.

Full design brief: [docs/game-design-doc.md](docs/game-design-doc.md). Condensed digest, balance values, and open questions: [knowledge.md](knowledge.md). Architecture rules this project follows: [CLAUDE.md](CLAUDE.md) and [.claude/knowledge/architecture.md](.claude/knowledge/architecture.md).

> This README follows the sections the design brief's submission checklist asks for. Sections marked **TODO** describe what will be true once that part of the game exists — they'll be filled in as the corresponding work lands, not before.

## Architecture

Four layers, each only calling the one below it: **Input** (reads input only) → **Gameplay** (decides actions, calls services) → **Systems** (all core game logic, no scene-object references) → **UI** (displays state, listens to events). Cross-system communication goes through a static `EventBus`; services are resolved through a `ServiceLocator` by interface (`IFooService`, never the concrete type); all balance numbers live on ScriptableObjects, never as hardcoded constants.

Code layout: `Assets/Project/{Core, Systems, Gameplay, UI, Data, Infrastructure, Bootstrap, Tests}`. See [.claude/knowledge/architecture.md](.claude/knowledge/architecture.md) for the full rule set, reference code, and how each of this game's systems (round/phase timing, hull, Boost, Shield, weapon, spawning, pooling) maps onto it.

**TODO** — once the Systems/Gameplay/UI code exists: name the concrete services and their interfaces here, and note any place the implementation deliberately diverges from the plan above.

## Input ownership and role switching

Player 1 always owns the left half of the screen/controls, Player 2 always owns the right half — **by screen side, not by role**. Whichever role (Pilot/Gunner) a player currently holds determines which control set is active on their fixed side. A single round timer drives one phase-transition event (not separate role-swap and difficulty timers): at 20s and 40s it cancels all active input (movement, firing, Boost, Shield, in-progress touches), swaps which player is Pilot vs. Gunner, applies the new phase's difficulty values, and shows a "ROLES REVERSED" banner after a 3-2-1 countdown warning.

Touch input tracks each touch by its ID and keeps it bound to whichever control it started on until release/cancel, even if the finger crosses the centre line — ownership never transfers mid-touch. Shield input is exclusive of aim/fire input.

**TODO** — once the Input layer exists: confirm here whether it's built on the new Input System (the project currently only has Unity's unrelated stock `InputSystem_Actions` template asset — see `knowledge.md` known issue #3) or on `Input`/touch APIs directly, and how the two schemes (editor vs. device) are unified behind one Input-layer interface.

## Pooling and performance

Player lasers, enemy projectiles, enemies, debris, and breach hazards are all pooled — prewarmed before the round starts, sized for Critical-phase load, reused rather than `Instantiate`/`Destroy`d. Reuse resets position, rotation, velocity, health, timers, collision state, and visuals. Gameplay code avoids per-frame allocations (no LINQ, no per-frame `GetComponent`/`Find`/`Camera.main`, no boxing), per `CLAUDE.md`'s allocation rules.

**TODO**: pool sizes chosen, and why; Profiler screenshot(s) captured during the Critical phase after pool warm-up, on the target device.

## Progression

Three phases driven by one 60-second round timer:

| Time | Phase | Player 1 | Player 2 | Difficulty |
|---|---|---|---|---|
| 0–20s | Patrol | Pilot | Gunner | Base enemy speed & spawn interval |
| 20–40s | Alert | Gunner | Pilot | Enemy speed × 1.25; breach hazards begin |
| 40–60s | Critical | Pilot | Gunner | Spawn interval × 0.70; enemy projectile speed × 1.50 |

Full numeric defaults (Boost/Shield/weapon values, and which base numbers are still undecided) are in [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md).

## Setup / device details

**TODO**: Unity Editor version used, target platform(s) tested (Android/iOS), physical device model(s), and any project setup steps beyond cloning the repo and opening it in Unity.

## AI usage

This project uses Claude Code under the team's architecture guidelines (`CLAUDE.md`), which require planning before implementation, user approval before code changes, and explicit flags on unverified/untested work. **TODO**: as implementation proceeds, log here which parts of the code were AI-generated, AI-assisted, or hand-written, per the brief's request to be able to explain every line.

## Assumptions

- Difficulty multipliers in the phase table are read as relative to each stat's *original base value*, and as **not resetting** between phases — a phase that doesn't restate a multiplier (e.g. Critical doesn't restate enemy speed) keeps the previous phase's value rather than reverting to base. See `knowledge.md` known issue #4.
- The reticle/aim is free-angle within the playfield (the ship's own movement is horizontal-only, but aiming is independent of that axis) — the brief doesn't fully spell this out. See `knowledge.md` known issue #5.
- Several base numeric values (ship speed, base enemy speed, base spawn interval, base projectile speed, debris/breach-hazard fall speed, invulnerability duration, enemy fire rate) are not specified in the design brief and are treated as pending — not guessed — per `CLAUDE.md`'s "never invent a constant" rule. Full list in [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md).

**TODO**: add any further assumptions made during actual implementation.

## Incomplete work

Nothing has been implemented yet — this repo currently contains only project scaffolding and the documentation set above (this README, `knowledge.md`, `.claude/knowledge/*`, `docs/game-design-doc.md`). **TODO**: keep this section current as features land, and call out anything left unfinished at submission time.
