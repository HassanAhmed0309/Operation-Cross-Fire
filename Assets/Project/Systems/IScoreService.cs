// What HUD (later) reads for the score display. Nothing calls this to add score directly -
// ScoreService listens for ScoreAwardedSignal itself.
public interface IScoreService
{
    int TotalScore { get; }
}
