// Published whenever hull changes - HUD (hull pips) subscribes to this.
public readonly struct HullChangedSignal
{
    public readonly int CurrentHull;
    public readonly int MaxHull;

    public HullChangedSignal(int currentHull, int maxHull)
    {
        CurrentHull = currentHull;
        MaxHull = maxHull;
    }
}
