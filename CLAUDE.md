# Working Agreement

## Workflow

Always plan before implementing new features.
Don't overcomplicate or overenginner code while planning.
Only implement code once the change has been approved by the user.
You first instinct should always be to plan the fix or suggest changes instead of directly implementing them.
If you are unsure about something, immediately ask the user, don't assume things by your self.
Always give updated information from the web if the user asks something related to a specific software or service.

## Project facts

- **Version control:** the project is expected to live on a GitHub repo. Until it is initialised there is no
  undo, so never delete or overwrite assets casually — check whether the working tree is under git before
  assuming either way. Once it is: commit every `.meta` file alongside its asset, use Unity's `.gitignore`
  (`Library/`, `Temp/`, `Logs/`, `obj/`, `UserSettings/`, the generated `.csproj`/`.sln`), set Asset
  Serialization to Force Text with the Unity YAML merge tool configured, and put large binaries (FBX, textures,
  audio) behind Git LFS.
- The project's design doc / spec is the source of truth. See `knowledge.md` for the digest, the initial
  scope, and the list of known issues, if present.

## Architecture (from the team wiki — mandatory)

Layers, in dependency order:

```
Input    → reads input ONLY, no gameplay logic
Gameplay → receives input, decides actions, calls services
Systems  → all core logic; no reference to Player, UI, or specific scene objects
UI       → displays data and listens to events; NO game logic
```

- ❌ No system directly references another system · ❌ No logic inside UI ·
  ❌ No heavy logic inside MonoBehaviours · ❌ No hardcoded values
- ✅ `EventBus` for cross-system communication · ✅ ScriptableObjects for data · ✅ Services behind interfaces
- Depend on `IFooService`, never `FooService`. Resolve with `ServiceLocator.Get<IFooService>()` in `Start`,
  never `new` a service outside `GameBootstrapper`.
- Code lives under `Assets/Project/<Layer>/`: `Core`, `Systems`, `Gameplay`, `UI`, `Data`, `Infrastructure`,
  `Bootstrap`, `Tests`. Put new files in the layer that matches their job, not wherever is convenient.
- Balance numbers come from ScriptableObjects. Never invent a constant — if a value is missing, ask.

## C# and Unity conventions

- Match the existing style: `[SerializeField] private _camelCase` backing field + `public Type Name => _field;`.
  No public fields. Group fields with `[Header("…")]`.
- One public type per file; the file name matches the class name.
- `[CreateAssetMenu]` menu paths use the `Game Data/<Category>` prefix.
- Prefer `readonly struct` for event/signal payloads.
- Keep `Update` bodies thin. Cache component references in `Awake`; never call `GameObject.Find`,
  `FindObjectOfType`, `GetComponent` or `Camera.main` per frame.
- Avoid per-frame allocations: no LINQ, no string concatenation, no boxing in `Update`/`LateUpdate`.
  Pool spawned objects wherever possible.
- Wrap diagnostic logging in `#if UNITY_EDITOR` or a debug flag. No `Debug.Log` in hot paths.

## Serialization — this repo has already lost data twice

- **Renaming a serialized field silently orphans authored asset data.** Always add
  `[FormerlySerializedAs("_oldName")]`, or migrate the assets deliberately.
- **Enums serialize as ints.** Any enum backing authored asset data (e.g. `ItemType`, `StateType`) is
  **append-only** — never insert or reorder values, or every authored asset silently remaps.
- Changing a field's type or wrapping it in a struct/list also orphans the old data. Check the `.asset` files
  before and after.
- `JsonUtility` only handles public fields on `[Serializable]` types. It cannot serialize dictionaries,
  properties, or polymorphic types.
- **Do not mutate ScriptableObjects at runtime for save state.** Edits persist in the editor between Play
  sessions and do not survive a rebuilt player. Runtime state belongs in the save system.

## Lifecycle and statics

- Subscribe in `OnEnable`, unsubscribe in `OnDisable`; subscribe in `Awake`, unsubscribe in `OnDestroy`.
  An un-unsubscribed listener becomes a `MissingReferenceException` after a scene reload.
- **`Awake`/`OnEnable` never run on inactive GameObjects.** Anything that must self-register (focus anchors,
  spawn points) belongs on an always-active object.
- Static state (`EventBus`, `ServiceLocator`) survives Play mode when Enter Play Mode Options disables domain
  reload. Reset it via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
- Kill DOTween tweens in `OnDisable`/`OnDestroy`, and make sure any state a tween's `OnComplete` restores
  (input locks, camera overrides) is also restored on kill.
- Use `[DefaultExecutionOrder]` only for bootstrapping; don't scatter it.

## Working with Unity assets (important for Claude)

- **Never hand-edit `.meta`, `.asset`, `.prefab`, `.unity` or `.controller` files.** They are GUID-linked YAML;
  a text edit can break references across the whole project. Read them for inspection only.
- **Never create or delete `.meta` files.** Unity generates them. Deleting an asset from the filesystem orphans
  its meta and breaks every reference — ask the user to do it in the Editor.
- Scene and prefab changes (adding components, wiring references, creating GameObjects) must be done by the user
  in the Editor. Describe exactly what to add and where; don't try to author it as text.
- ScriptableObject **asset values** are the user's to fill in. Claude proposes the fields and a table of values;
  the user enters them.
- Asset naming follows the general coding pattern.

## Verification

- **Claude cannot compile the project or enter Play mode.** After any script change, say plainly that it is
  unverified and ask the user to return to Unity and report the Console output.
- There are no assembly definitions, so everything lands in `Assembly-CSharp` and edit-mode unit tests are not
  currently possible. Prefer designs that can be exercised from a debug MonoBehaviour with `[ContextMenu]` entries.
- When proposing a change, include how the user can verify it in the Editor: what to click, what to watch, and
  what "working" looks like.

---

# Triggered routines

These run only when the user writes the trigger phrase. They are reports, not edits — the Workflow rule above
still applies, so **never apply fixes unless the user approves them**.

## `DO CODE REVIEW` — Unity code review

Also triggered by: `REVIEW THIS`, `CODE REVIEW`.

**Scope — establish it first:**
- If the working tree is under git, review the actual diff: uncommitted changes by default, or the current
  branch against its base when the user names a branch or PR.
- If it isn't under version control yet, review the files created or edited in this same prompt or session,
  and list them explicitly before starting.
- If neither applies, ask what to review rather than reviewing the whole project.

Work through, in this order — architecture first, because a layering mistake invalidates line-level notes:

1. **Architecture** — a system referencing another system · logic inside UI · heavy logic in a MonoBehaviour ·
   hardcoded values that belong in a ScriptableObject · a concrete type used where an interface exists ·
   a service constructed outside `GameBootstrapper` · a file in the wrong layer folder.
2. **Serialization safety** — a renamed serialized field without `[FormerlySerializedAs]` · an enum value
   inserted or reordered · a changed field type that orphans authored assets · runtime mutation of a
   ScriptableObject for state that must persist.
3. **Lifecycle** — every `Subscribe` has a matching `Unsubscribe` · `Awake`/`OnEnable` relied on for an object
   that may be inactive · static state without a reset hook · a DOTween/coroutine whose completion handler
   restores state (input locks, camera overrides) but whose *kill* path does not.
4. **Null and ordering** — `[SerializeField]` references assumed non-null · `ServiceLocator.Get` called in
   `Awake` for a service registered in another `Awake` · execution-order assumptions.
5. **Allocation** — anything in `Update`/`LateUpdate`/`FixedUpdate` that allocates (see the audit below).
6. **Conventions and leftovers** — `public` fields · file name not matching the class · `Debug.Log` outside an
   editor guard · `NotImplementedException`, dead code or a stub left behind · commented-out code.

**Output:** findings ranked most severe first, each with `file.cs:line`, a one-sentence defect statement, a
**concrete failure scenario** (inputs or state → wrong result), and the smallest fix. Say plainly when a finding
is a judgment call rather than a defect. Note anything that cannot be confirmed without compiling or entering
Play mode, and end with what the user should check in the Editor.

## `PERF AUDIT` — GC and performance audit

Also triggered by: `GC AUDIT`, `DO PERF AUDIT`, `CHECK ALLOCATIONS`.

A GC spike stalls the frame outright and is visible to the player. The goal is
**zero steady-state allocation** once gameplay is running.

**Allocation sources to hunt, in rough order of impact:**
- Anything allocating **per frame**: `new` in `Update`, LINQ, `ToList()`/`ToArray()`, lambdas or closures
  capturing locals, `params` arrays, string concatenation or interpolation (including inside `Debug.Log`).
- **Boxing**: a struct passed as `object`, a struct signal published through a non-generic delegate, a struct
  stored in a non-generic collection, an enum used as a dictionary key without a comparer.
- `GetComponent`, `GameObject.Find`, `FindObjectsByType`, `Camera.main` called per frame instead of cached in `Awake`.
- `new WaitForSeconds(...)` allocated each loop of a coroutine instead of cached in a field.
- Physics without the non-allocating variants (`RaycastNonAlloc`, `OverlapSphereNonAlloc`), or per-NPC raycasts each frame.
- `renderer.material` (instantiates a material) where `sharedMaterial` would do; shader and animator strings not
  cached via `Shader.PropertyToID` / `Animator.StringToHash`.
- UI: per-frame text updates, layout rebuilds, one giant canvas that redraws whenever any element changes.

**General Unity considerations:**
- `Instantiate`/`Destroy` churn for frequently spawned objects (projectiles, enemies, pickups, VFX) — these
  should be **pooled**.
- Hundreds of MonoBehaviours each running `Update` costs more than one manager iterating a list.
- NavMeshAgent count and repath frequency are a real budget if using AI navigation; check before adding more agents.
- Prefer disabling offscreen or idle behaviours over letting them tick.

**Method:** profile a **build**, not the editor — editor numbers include editor overhead and mislead badly.
Use the Profiler's **GC Alloc** column to find the callers, the Memory Profiler package for retained memory, and
the Frame Debugger for draw calls and batching.

**Output:** ranked findings with `file.cs:line`, what allocates and how often (per frame / per spawn / per event),
and the fix. Don't micro-optimize cold paths — a one-time allocation at startup is not a finding. Say which items
need a profiler run to confirm rather than asserting a number that wasn't measured.

## `WRITE TESTS` — unit test writer

Also triggered by: `ADD TESTS`, `DO UNIT TESTS`, `TEST THIS`.

**Blocker to state first:** there are no assembly definitions, and **a test asmdef cannot reference the predefined
`Assembly-CSharp`**. So testable code has to move into its own assembly before any unit test can exist. The first
run of this routine should propose that split — `Project.Core`, `Project.Systems`, `Project.Data`, plus a
`Project.Tests` asmdef (Editor platform, referencing those and the NUnit/TestRunner assemblies) — and wait for
approval before writing tests. It is a structural change, not a test.

**What is worth testing here** — the plain C# services, which is most of the logic:
- Any state-machine or step-based service: a step advances only when its conditions are actually met · an
  action for a non-current step is ignored · it self-heals from an unexpected or mid-step state · the final
  step raises completion.
- Save/load logic: conflicting data sources resolve deterministically (e.g. higher version/counter wins) ·
  corrupt payloads fall back safely without throwing · distinct result states (no data vs. error) are handled
  differently.
- `EventBus`: subscribe/publish/unsubscribe, and that an unsubscribed handler stops receiving.
- Any ref-counted gate or lock (e.g. an input lock): two acquires require two releases before it opens.

**Rules for the tests themselves:**
- **Reset static state in `[SetUp]`** (`EventBus.Clear()`, `ServiceLocator.Clear()`). Statics leak between tests
  and produce passes and failures that depend on test order.
- Use hand-written fakes (`FakeSaveStorage`, `FakeTaskSystem`), not a mocking framework.
- Name tests `Method_Condition_ExpectedResult`. One behaviour per test, Arrange/Act/Assert.
- Keep tests deterministic: no real time, no frame waits, no random. Inject anything time-based.
- Build ScriptableObject fixtures with `ScriptableObject.CreateInstance` in the test.
- EditMode by default under `Assets/Project/Tests/EditMode`. Use PlayMode (`Tests/PlayMode`) only for things that
  genuinely need frames — coroutines, tweens, camera or animation blends — and say why.
- Don't test Unity itself, trivial property getters, or authored asset values.

**Output:** the proposed test list first, then the code once approved. Tests are unverified until the user runs
the Test Runner and reports the result.
