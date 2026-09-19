// Published once when the round ends, win or lose.
public readonly struct RoundEndedSignal
{
    public readonly bool Won;

    public RoundEndedSignal(bool won)
    {
        Won = won;
    }
}
