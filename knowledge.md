# Knowledge Digest — Operation Cross-Fire

This is the digest `CLAUDE.md` points to. The source of truth is the client-provided spec:

- `Junior Unity Dev Exercise_ _Operation Cross-Fire_ Protecting the Orbital Corridor_.pdf` — the GDD (treated here as a real game brief, not a throwaway exercise).
- `Dev TEAM ARCHITECTURE GUIDELINES - Overview.pdf` — the mandatory team architecture, already folded into `CLAUDE.md`.

When this digest and a PDF disagree, the PDF wins — update this file rather than trusting memory of it. Full companion docs: [.claude/knowledge/architecture.md](.claude/knowledge/architecture.md) (architecture reference), [.claude/knowledge/domain-glossary.md](.claude/knowledge/domain-glossary.md) (terminology), [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md) (every serialized/balance number, and which ones are still missing), [docs/game-design-doc.md](docs/game-design-doc.md) (full GDD transcription).

## One-line pitch

Two players share one interceptor ship for a 60-second round: a Pilot (move + Boost) and a Gunner (aim + fire + Shield). A "Quantum Flux" event at 20s and 40s swaps who plays which role and ramps difficulty. Win by surviving 60s with hull > 0; lose at 0 hull or if a breach hazard reaches the bottom.

## Initial scope (in)

- Local, same-device 2-player co-op. No networking.
- One shared ship, 3 hull points, horizontal movement only, clamped to the playfield.
- Pilot: left/right movement, Boost.
- Gunner: aim (reticle), fire, Shield.
- One round timer drives everything: a single phase transition at 20s and 40s (not separate role-swap/difficulty timers).
- Quantum Flux transition: 3‑2‑1 warning banner → cancel input → swap roles → apply new phase's difficulty values → update HUD/control panels → "ROLES REVERSED" banner.
- Enemies, debris, breach hazards, enemy projectiles, player laser — all pooled.
- Editor keyboard/mouse controls AND physical-device touch controls, working simultaneously, per player side (P1 = left half, P2 = right half).
- HUD: time, hull, score, phase, current role assignments, Boost/Shield cooldowns.
- Runs on one physical Android or iOS device; profiled during Critical phase.

## Out of scope (explicit)

Networking, accounts, multiple levels, upgrades, alternate ammo, shield polarity, adaptive difficulty, detailed art/audio/VFX, production menus, tutorials, save data. Also not required: assembly definitions / edit-mode unit tests (per `CLAUDE.md`, unless the team approves the split proposed by `WRITE TESTS`).

## Core rules

- Ship starts at 3 hull points, positioned near bottom-centre, moves horizontally only, stays inside the playfield.
- An **unshielded** hit from an enemy, debris, or enemy projectile removes exactly **1** hull point, regardless of which object it was.
- After any hit, a short, configurable invulnerability window prevents a second hit from double-counting. **Duration is not specified in the GDD — must be authored as a serialized field, not invented.**
- Win: timer reaches 60s with hull ≥ 1.
- Lose: hull reaches 0, **or** any breach hazard reaches the bottom boundary (instant loss, independent of hull).
- All spawning and gameplay input stop the instant the round ends (win or lose).

## Roles, controls, and the swap

| Time | Phase | Player 1 | Player 2 | Difficulty change |
|---|---|---|---|---|
| 0–20s | Patrol | Pilot | Gunner | Base enemy speed & spawn interval; enemies + debris appear |
| 20–40s | Alert | Gunner | Pilot | Enemy speed → base × 1.25; breach hazards begin appearing |
| 40–60s | Critical | Pilot | Gunner | Spawn interval → base × 0.70; enemy projectile speed → base × 1.50 |

Reading the table literally: each row's multiplier is stated as "base ×", not "previous ×". Adopted interpretation (flagged below as an assumption to confirm): multipliers **do not reset** between phases — Alert's ×1.25 enemy speed carries into Critical since Critical doesn't restate enemy speed, and Critical's own changes (spawn interval, projectile speed) stack on top of that carried-forward value, each still measured against the original *base* number, not against each other.

Editor controls: Pilot = A/D or arrows + Space (Boost). Gunner = mouse movement (aim), left click (fire), right click (Shield).

Touch controls: P1 owns the left half of the screen, P2 the right half — by screen side, not by role. Whichever role a player currently holds determines which control set appears on their (fixed) side. Rules that matter for input-layer design:
- Track each touch by its touch ID; keep it bound to whatever control it started on until it ends/cancels, even if the finger crosses the centre line.
- Holding Left+Right simultaneously (Pilot) cancels movement rather than picking one.
- Dragging in the aim area moves the reticle; holding Fire shoots at the fixed weapon cadence.
- Shield's touch must not double as aim/fire input.
- Both sides must be read every frame, independently — this is the concrete reason `InputHandler` must not do any decision-making itself (see architecture doc).

At every Quantum Flux transition (20s, 40s): cancel active movement/firing/Boost/Shield/touches → swap Pilot/Gunner assignment → apply new phase's values → refresh control panels + role labels → show "QUANTUM FLUX — ROLES REVERSED" briefly. A "QUANTUM FLUX IN 3… 2… 1…" banner shows for the 3 seconds *before* the transition — input is still live during that countdown; only the transition instant itself cancels input.

## Gameplay objects & scoring

| Object | Behaviour | Health (laser hits) | Contact hull damage | Score |
|---|---|---|---|---|
| Enemy | red square, moves down, may fire projectiles | 1 | 1 hull point | 10 |
| Debris | grey circle, moves down | 2 | 1 hull point | 15 |
| Breach hazard | orange diamond, appears from Alert onward; reaching the bottom is an instant loss | 3 | 1 hull point | 25 |
| Enemy projectile | red rectangle, fired by enemies | — (despawns on hit/cleanup) | 1 hull point | — |
| Player laser | fired by the ship toward the reticle | — | n/a | — |

Note the two different "damage" numbers per object are independent: an object's *health* (how many player-laser hits destroy it) is unrelated to the *1 hull point* it deals to the ship on contact — don't conflate a debris's 2-hit health with dealing 2 damage; it still only takes 1 hull point.

## Ability & weapon defaults (see design-values.md for the full authoritative table)

- Boost — Pilot only, 1s duration, 1.75× move speed, 4s cooldown, **no invulnerability**.
- Shield — Gunner only, 1.5s duration, 5s cooldown, blocks hull damage while active.
- Weapon — fires toward the reticle, 0.25s cooldown, cannot be bypassed by rapid clicking.

## Technical requirements summary

- Unity 2D colliders/triggers; configure the Physics2D collision matrix to exclude impossible pairs (e.g. player laser vs. player laser).
- Minimum collision pairs: player laser ↔ {enemy, debris, breach hazard}; {enemy, debris, enemy projectile} ↔ {ship, active Shield}; breach hazard ↔ bottom boundary; all spawned objects ↔ an off-screen cleanup boundary.
- Pool every spawned type (player laser, enemy projectile, enemy, debris, breach hazard). Prewarm before round start. Reuse must fully reset position, rotation, velocity, health, timers, collision state, and visuals. Size for Critical-phase load; documented safe growth is acceptable.
- No recurring managed allocations in gameplay code after warm-up; no scene-wide searches in `Update`/physics callbacks; cache component references (matches `CLAUDE.md`'s allocation rules directly).
- Must run on one physical Android or iOS device; Profiler evidence required from the Critical phase.

## Known issues / open questions

These are genuine gaps in the GDD — don't invent numbers for them; ask the user (per `CLAUDE.md`'s "never invent a constant" rule). Full list with context in [.claude/knowledge/design-values.md](.claude/knowledge/design-values.md):

1. **No base numeric values given** for: ship movement speed, base enemy move speed, base spawn interval, enemy projectile base speed, debris/breach-hazard fall speed, enemy fire rate/pattern, laser travel speed, hit-invulnerability duration. All multipliers in the phase table are relative to these undefined bases.
2. **Folder naming discrepancy**: the wiki's example tree uses `Assets/_Project/…`; `CLAUDE.md` (authoritative for this repo) says `Assets/Project/…` (no underscore). This project follows `CLAUDE.md`.
3. **Default Input System asset mismatch**: `Assets/InputSystem_Actions.inputactions` is Unity's stock template (Move/Look/Attack/Interact/Crouch/Jump/…) generated by the project wizard — it does not model this game's Pilot/Gunner control scheme at all and will need to be replaced or heavily edited once the Input layer is built. Flagging so nobody assumes it's already wired up.
4. **Cumulative vs. reset difficulty multipliers** across phases — see the assumption stated under "Roles, controls, and the swap" above; confirm with the user before the Systems layer locks in the math.
5. **Aim/reticle model** isn't fully explicit for keyboard/mouse — "mouse movement moves the reticle" and "laser travels toward the reticle" is being read as free-angle aim within the playfield (not a fixed vertical lane), since the ship itself is constrained to horizontal-only movement but the reticle/aim is described independently of the ship's axis.
6. **Enemy firing pattern** ("may fire projectiles") has no stated frequency, targeting rule, or projectile count — needs a value/ScriptableObject field once decided.
7. Pool sizes have no numeric floor from the spec beyond "size for Critical phase" — actual counts depend on the still-missing spawn interval/lifetime numbers in (1).
