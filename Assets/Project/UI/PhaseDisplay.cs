using TMPro;
using UnityEngine;

// Displays the current phase name. Reads the live value once, then only updates on PhaseChangedSignal.
public class PhaseDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text phaseText;

    void Start()
    {
        IRoundService roundService = ServiceLocator.Get<IRoundService>();
        SetText(roundService.CurrentPhase);
    }

    void OnEnable() => EventBus.Subscribe<PhaseChangedSignal>(OnPhaseChanged);

    void OnDisable() => EventBus.Unsubscribe<PhaseChangedSignal>(OnPhaseChanged);

    void OnPhaseChanged(PhaseChangedSignal signal) => SetText(signal.NewPhase);

    void SetText(Phase phase) => phaseText.text = phase.ToString().ToUpperInvariant();
}
