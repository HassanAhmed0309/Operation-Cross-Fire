# Domain Glossary — Operation Cross-Fire

Game-domain terms from the GDD. For architecture/code-pattern vocabulary (Service, EventBus, Layer names) see [architecture.md](architecture.md).

| Term | Meaning |
|---|---|
| **Interceptor / the ship** | The single shared player ship. Exactly one exists per round; starts near bottom-centre; moves horizontally only, clamped inside the playfield. |
| **Pilot** | The role controlling ship movement (left/right) and Boost. Not a fixed player — Player 1 and Player 2 alternate holding this role. |
| **Gunner** | The role controlling aim (reticle), fire, and Shield. Alternates with Pilot at each Quantum Flux. |
| **Player 1 / Player 2** | The two physical people. Fixed identities that keep the same screen half (P1 = left, P2 = right) regardless of which role they currently hold. |
| **Role** | Which of Pilot/Gunner a player currently controls. Distinct from "Player" — the GDD is explicit that these are two different axes. |
| **Hull** | The ship's health, starts at 3 points. Reaching 0 is a loss condition. |
| **Hull point / hit** | One unit of hull damage. Any unshielded hit from an enemy, debris, or enemy projectile removes exactly one, regardless of the object's own health/score value. |
| **Invulnerability window** | A short timer that starts after the ship takes damage, during which further hits don't remove additional hull points. Duration not yet specified (see `design-values.md`). |
| **Boost** | Pilot-only ability: temporarily multiplies movement speed. Does not grant invulnerability. |
| **Shield** | Gunner-only ability: temporarily prevents all hull damage while active. |
| **Reticle** | The Gunner's aim point, moved by mouse (editor) or drag (touch). The player laser travels from the ship toward it. |
| **Weapon / fire** | The Gunner's shooting action; gated by a fixed cooldown that rapid input cannot bypass. |
| **Quantum Flux** | The single event that, at 20s and 40s, cancels input, swaps Pilot/Gunner between the two players, advances the difficulty phase, and shows a banner. It is one mechanic, not "role swap" + "difficulty" as two separate systems. |
| **Round timer** | The single 60-second timer that drives the whole round — phase changes and Quantum Flux transitions are derived from it, not tracked separately. |
| **Phase** | One of three named time windows: Patrol (0–20s), Alert (20–40s), Critical (40–60s). Each has its own difficulty multipliers. |
| **Patrol** | Phase 1 (0–20s). Base enemy speed and spawn interval; enemies and debris begin appearing. |
| **Alert** | Phase 2 (20–40s). Enemy speed increases; breach hazards start appearing. |
| **Critical** | Phase 3 (40–60s). Spawn interval shortens; enemy projectile speed increases. |
| **Enemy** | Red-square hazard, 1 hit to destroy, may fire projectiles, worth 10 points, deals 1 hull point on unshielded contact. |
| **Debris** | Grey-circle hazard, 2 hits to destroy, worth 15 points, deals 1 hull point on unshielded contact (same as Enemy — its higher health is not extra damage). |
| **Breach hazard** | Orange-diamond hazard that only appears from Alert onward; 3 hits to destroy, worth 25 points, deals 1 hull point on unshielded contact, **and** reaching the bottom boundary is an instant round loss independent of hull. |
| **Enemy projectile** | Red-rectangle shot fired by an Enemy; damages an unshielded ship on contact; no health/score of its own. |
| **Player laser** | The Gunner's shot, travels from the ship toward the reticle; the thing that damages Enemy/Debris/Breach hazard. |
| **Cleanup boundary** | An off-screen boundary that despawned/missed projectiles and hazards hit, returning them to their pool instead of running forever. |
| **Pool / pooling** | Pre-instantiated, reused objects (player laser, enemy projectile, enemy, debris, breach hazard) to avoid runtime `Instantiate`/`Destroy` churn. Prewarmed before the round starts; resized safely if needed, but sized for the Critical phase's load by default. |
| **Prewarm** | Populating a pool with inactive instances before the round begins, so gameplay never pays an `Instantiate` cost mid-round. |
| **HUD** | The always-visible UI showing time remaining, hull, score, current phase, current role assignments, and Boost/Shield cooldowns. |
| **Banner** | A brief on-screen text callout — "QUANTUM FLUX IN 3… 2… 1…" (warning) and "QUANTUM FLUX — ROLES REVERSED" (transition) — using default/simple UI, no production polish required. |
