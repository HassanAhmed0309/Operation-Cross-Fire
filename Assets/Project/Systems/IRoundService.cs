// What RoundDriver calls each frame to advance the round timer.
public interface IRoundService
{
    float ElapsedTime { get; }
    float TimeRemaining { get; }
    Phase CurrentPhase { get; }
    bool HasEnded { get; }

    void Tick(float deltaTime);
}
