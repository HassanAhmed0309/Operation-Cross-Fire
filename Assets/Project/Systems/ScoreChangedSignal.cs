// Published by ScoreService whenever the total changes - HUD (later step) subscribes to this.
public readonly struct ScoreChangedSignal
{
    public readonly int TotalScore;

    public ScoreChangedSignal(int totalScore)
    {
        TotalScore = totalScore;
    }
}
