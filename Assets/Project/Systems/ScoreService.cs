using UnityEngine;

// Plain C# service: accumulates score from ScoreAwardedSignal. Subscribes once at construction -
// this service lives for the whole session, same as Hull/Shield/Weapon.
public class ScoreService : IScoreService
{
    int totalScore;

    public ScoreService()
    {
        EventBus.Subscribe<ScoreAwardedSignal>(OnScoreAwarded);
    }

    public int TotalScore => totalScore;

    void OnScoreAwarded(ScoreAwardedSignal signal)
    {
        totalScore += signal.Amount;
        EventBus.Publish(new ScoreChangedSignal(totalScore));

#if UNITY_EDITOR
        Debug.Log("Score +" + signal.Amount + " -> " + totalScore);
#endif
    }
}
