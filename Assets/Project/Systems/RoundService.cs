using UnityEngine;

// Plain C# service: the single round timer. Publishes every phase/flux/end signal; never touches
// input, roles, or spawning directly - those react to the signals themselves.
public class RoundService : IRoundService
{
    readonly RoundData data;

    float elapsedTime;
    Phase currentPhase = Phase.Patrol;
    bool hasEnded;
    bool warnedPatrolToAlert;
    bool warnedAlertToCritical;

    public RoundService(RoundData data)
    {
        this.data = data;
        EventBus.Subscribe<ShipDestroyedSignal>(OnShipDestroyed);
    }

    public float ElapsedTime => elapsedTime;
    public Phase CurrentPhase => currentPhase;
    public bool HasEnded => hasEnded;

    public void Tick(float deltaTime)
    {
        if (hasEnded)
            return;

        elapsedTime += deltaTime;

        CheckWarning(ref warnedPatrolToAlert, data.patrolEndTime);
        CheckWarning(ref warnedAlertToCritical, data.alertEndTime);

        if (currentPhase == Phase.Patrol && elapsedTime >= data.patrolEndTime)
            AdvancePhase(Phase.Alert);
        else if (currentPhase == Phase.Alert && elapsedTime >= data.alertEndTime)
            AdvancePhase(Phase.Critical);

        if (elapsedTime >= data.roundDuration)
            EndRound(won: true);
    }

    void CheckWarning(ref bool warned, float transitionTime)
    {
        if (warned || elapsedTime < transitionTime - data.warningLeadTime)
            return;

        warned = true;
        EventBus.Publish(new QuantumFluxWarningSignal());

#if UNITY_EDITOR
        Debug.Log("Quantum Flux warning - transition in " + data.warningLeadTime + "s");
#endif
    }

    void AdvancePhase(Phase newPhase)
    {
        currentPhase = newPhase;

        // Alert's enemy-speed bump carries forward into Critical (not restated there, not reset).
        float speedMultiplier = newPhase == Phase.Patrol ? 1f : data.alertEnemySpeedMultiplier;
        float spawnIntervalMultiplier = newPhase == Phase.Critical ? data.criticalSpawnIntervalMultiplier : 1f;

        EventBus.Publish(new PhaseChangedSignal(newPhase, speedMultiplier, spawnIntervalMultiplier));
        EventBus.Publish(new QuantumFluxTriggeredSignal());

#if UNITY_EDITOR
        Debug.Log("Quantum Flux triggered - phase now " + newPhase);
#endif
    }

    void OnShipDestroyed(ShipDestroyedSignal signal) => EndRound(won: false);

    void EndRound(bool won)
    {
        if (hasEnded)
            return;

        hasEnded = true;
        EventBus.Publish(new RoundEndedSignal(won));

#if UNITY_EDITOR
        Debug.Log("Round ended - " + (won ? "WON" : "LOST"));
#endif
    }
}
