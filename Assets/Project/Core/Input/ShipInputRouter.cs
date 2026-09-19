using UnityEngine;

// The only thing gameplay reads input from. Runs before gameplay scripts and merges every source into role intents.
[DefaultExecutionOrder(-100)]
public class ShipInputRouter : MonoBehaviour, IShipInputRouter
{
    [SerializeField] KeyboardMouseInputSource keyboardMouseSource;
    [SerializeField] TouchInputSource touchSource;
    [SerializeField] bool enableOnStart = true;

    IInputSource[] sources;
    bool gameplayEnabled;

    public PilotIntent Pilot { get; private set; }
    public GunnerIntent Gunner { get; private set; }
    public bool GameplayEnabled => gameplayEnabled;

    void Awake()
    {
        int count = (keyboardMouseSource != null ? 1 : 0) + (touchSource != null ? 1 : 0);
        sources = new IInputSource[count];

        int index = 0;
        if (keyboardMouseSource != null)
            sources[index++] = keyboardMouseSource;
        if (touchSource != null)
            sources[index++] = touchSource;
    }

    void Start()
    {
        if (enableOnStart)
            SetGameplayEnabled(true);
    }

    void Update()
    {
        PilotIntent pilot = default;
        GunnerIntent gunner = default;

        if (gameplayEnabled)
        {
            for (int i = 0; i < sources.Length; i++)
                sources[i].Tick(ref pilot, ref gunner);
        }

        Pilot = pilot;
        Gunner = gunner;

        InputDebugLog.ShowState(in pilot, in gunner);
    }

    // Quantum Flux: drop all held input across every source.
    public void CancelAll()
    {
        if (InputDebugLog.Enabled)
            InputDebugLog.Write("--- ROUTER cancel all input ---");

        for (int i = 0; i < sources.Length; i++)
            sources[i].CancelAll();

        Pilot = default;
        Gunner = default;
    }

    // Round end: stop all gameplay input.
    public void SetGameplayEnabled(bool isEnabled)
    {
        if (InputDebugLog.Enabled)
            InputDebugLog.Write(isEnabled ? "--- ROUTER input ON ---" : "--- ROUTER input OFF ---");

        gameplayEnabled = isEnabled;
        for (int i = 0; i < sources.Length; i++)
            sources[i].SetEnabled(isEnabled);

        if (!isEnabled)
        {
            Pilot = default;
            Gunner = default;
        }
    }
}
