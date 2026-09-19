// Published by anything that awards score on death (Enemy now; Debris/BreachHazard later).
public readonly struct ScoreAwardedSignal
{
    public readonly int Amount;

    public ScoreAwardedSignal(int amount)
    {
        Amount = amount;
    }
}
