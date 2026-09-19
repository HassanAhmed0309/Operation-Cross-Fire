# Architecture Reference — Operation Cross-Fire

Digest of `Dev TEAM ARCHITECTURE GUIDELINES - Overview.pdf`, adapted to this repo. `CLAUDE.md` is the authoritative, already-adapted copy of these rules for this project (folder names, layer list, checklist) — this file adds the wiki's reference code and rationale that `CLAUDE.md` doesn't repeat. If the two ever conflict, `CLAUDE.md` wins.

## Goal

Simple to read, easy to follow, safe to scale. Anyone should be able to add a feature without breaking another system.

## Layers (dependency order — a layer only calls the one below it)

```
Input    → reads input ONLY, no gameplay logic
Gameplay → receives input, decides actions, calls services
Systems  → all core logic; no reference to Player, UI, or specific scene objects
UI       → displays data and listens to events; NO game logic
```

- **Input**: reads raw input only (keyboard/mouse in editor, touch on device). Never decides what an action *means*.
- **Gameplay**: reads Input's output, decides what to do, calls a Systems-layer service through its interface. This is where `PlayerController`-style MonoBehaviours live.
- **Systems**: the actual game logic (spawning, scoring, hull/damage, ability cooldowns, round/phase timing). No knowledge of `Player`, UI, or any specific scene object — a system should be testable by itself.
- **UI**: displays state and reacts to events. Never computes gameplay outcomes.

Folders (per `CLAUDE.md`): `Assets/Project/Core`, `Systems`, `Gameplay`, `UI`, `Data`, `Infrastructure`, `Bootstrap`, `Tests`. (The wiki's own tree uses `Assets/_Project/…` with a leading underscore — this repo does not; see the open question in `knowledge.md`.)

## Core principles

1. **Single Responsibility** — one class, one job. `InputHandler` reads input; a service handles its own domain; a UI controller only updates UI. Don't let a single MonoBehaviour own input + logic + UI + persistence.
2. **Separation of Concerns** — Input → Gameplay → Systems → UI, each layer doing only its own job.
3. **Depend on interfaces, not concrete types** — inject/resolve `IFooService`, never `FooService` directly.
4. **Open for extension, closed for modification** — add new behaviour without editing existing, working code.

## Non-negotiable rules

❌ No system references another system directly · ❌ no logic inside UI · ❌ no heavy logic inside a MonoBehaviour · ❌ no hardcoded values.

✅ `EventBus` for cross-system communication · ✅ ScriptableObjects for data/config · ✅ services behind interfaces for logic.

## Core systems — reference skeletons

These are the wiki's canonical starting implementations. Treat them as a reference shape, not a final drop-in — once real `ServiceLocator`/`EventBus` classes exist in `Assets/Project/Core/`, that code is authoritative and this snippet is just for orientation.

```csharp
public static class ServiceLocator
{
    private static Dictionary<Type, object> services = new();

    public static void Register<T>(T service) => services[typeof(T)] = service;
    public static T Get<T>() => (T)services[typeof(T)];
}
```

```csharp
public static class EventBus
{
    private static Dictionary<Type, Action<object>> events = new();

    public static void Subscribe<T>(Action<T> callback)
    {
        Type type = typeof(T);
        if (!events.ContainsKey(type)) events[type] = delegate { };
        events[type] += (e) => callback((T)e);
    }

    public static void Publish<T>(T signal)
    {
        Type type = typeof(T);
        if (events.ContainsKey(type)) events[type].Invoke(signal);
    }
}
```

```csharp
public interface IInteractable
{
    void Interact(GameObject interactor);
}
```

Notes for this project specifically (per `CLAUDE.md`'s Lifecycle & statics section, which supersedes the wiki here — the wiki's snippets above are static and never reset):
- Both statics above need a `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` reset hook, since Enter Play Mode Options can disable domain reload and leave stale services/subscribers across Play sessions.
- `EventBus.Subscribe`'s `events[type] += (e) => callback((T)e)` allocates a new closure every subscribe call — fine for one-time `OnEnable` subscriptions, just don't call `Subscribe` from a hot path.
- This game's own signals (damage taken, hull changed, round won/lost, phase changed, role swapped, score changed, ability activated/cooled down) should be `readonly struct` payloads per `CLAUDE.md`'s convention, not the wiki's bare `public struct CropHarvestedSignal { }`.

## Worked example from the wiki (Crop system → pattern to follow)

Data (ScriptableObject) → Service behind an interface → MonoBehaviour that calls the service and reports outcomes via `EventBus`:

```csharp
[CreateAssetMenu(menuName = "Data/Crop")]
public class CropData : ScriptableObject
{
    public GameObject prefab;
    public float growTime;
}

public interface ICropService { void Plant(CropData data, Vector3 position); }

public class CropService : ICropService
{
    public void Plant(CropData data, Vector3 position)
    {
        var obj = GameObject.Instantiate(data.prefab, position, Quaternion.identity);
        obj.GetComponent<Crop>().Init(data);
    }
}
```

Map this onto Operation Cross-Fire's systems, e.g.:
- `EnemyData` / `DebrisData` / `BreachHazardData` (ScriptableObjects: prefab, speed, health, score value) → `ISpawnService` / `IHazardService`.
- `HullData` (max hull, invulnerability duration) → `IHullService` (damage, invulnerability, death signal).
- `AbilityData` for Boost and Shield (duration, cooldown, multiplier) → `IAbilityService` per ability, or one generic cooldown-gated service.
- `WeaponData` (fire cooldown) → `IWeaponService`.
- A single `IRoundService` owns the one round timer and fires the phase-transition event described in `knowledge.md` — this is the piece the GDD is explicit must be **one** timer, not separate role-swap/difficulty timers, and not owned by any MonoBehaviour directly (per `CLAUDE.md`'s "no heavy logic inside MonoBehaviours").
- Pooling belongs in Systems too (an `IPoolService`/per-type pool), never ad-hoc `Instantiate`/`Destroy` in Gameplay code, per the GDD's pooling requirement.

`GameBootstrapper` (in `Assets/Project/Bootstrap/`) is the only place any of these are `new`'d and registered:

```csharp
public class GameBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.Register<ICropService>(new CropService());
        // ... register every IXxxService this game needs, once, here.
    }
}
```

Consumers resolve in `Start` (per `CLAUDE.md`), not `Awake`, to guarantee `GameBootstrapper.Awake` has already run — mind `[DefaultExecutionOrder]` if that ordering ever needs to be explicit.

## Input/Gameplay example from the wiki

```csharp
public class InputHandler : MonoBehaviour
{
    public Vector2 Movement { get; private set; }
    public bool InteractPressed { get; private set; }

    private void Update()
    {
        Movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        InteractPressed = Input.GetKeyDown(KeyCode.E);
    }
}
```

This game needs **two** concurrent input sources (Pilot side + Gunner side) that swap meaning at each Quantum Flux, plus a touch variant that tracks touch IDs per screen half (see `knowledge.md`). The wiki's example uses the legacy `Input` class; this project already has the new Input System package installed with a (currently unrelated, stock-template) `InputSystem_Actions` asset — the Input layer design needs to pick one approach deliberately rather than default to whichever the template happened to generate. This is a decision to raise with the user before writing the Input layer, not something to assume.

`PlayerController` (Gameplay layer) reads `InputHandler`'s public state, decides an action, and calls a service — it never contains the logic itself:

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputHandler input;
    private ICropService cropService;

    private void Start() => cropService = ServiceLocator.Get<ICropService>();

    private void Update()
    {
        HandleMovement();
        HandleInteraction();
    }
    // ...
}
```

## Pre-commit checklist (from the wiki, matches `CLAUDE.md`'s review routine)

- Class has only one responsibility.
- Input is handled in an Input-layer class, never inside a Gameplay controller.
- No cross-system dependency (a Systems class never references another Systems class directly — go through `EventBus` or a shared Gameplay-layer orchestrator).
- `EventBus` used where cross-layer communication is needed.
- Data lives in a ScriptableObject, not a hardcoded constant.
- MonoBehaviours stay lightweight — logic lives in a service.
- Code reads in under 30 seconds.
