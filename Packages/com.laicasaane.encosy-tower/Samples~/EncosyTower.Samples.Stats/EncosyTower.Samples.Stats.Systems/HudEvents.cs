namespace EncosyTower.Samples.Stats
{
    public delegate void SpawnStatAction(SpawnMessage msg);

    public delegate void UpdateAliveCounterAction(int totalAmount);

    public readonly record struct SpawnMessage(int StatAmount, float AffectedPercent);

    public static class HudEvents
    {
        public static event SpawnStatAction OnSpawnStat;

        public static event UpdateAliveCounterAction OnUpdateStatOwnerCounter;

        public static void Spawn(SpawnMessage msg)
        {
            OnSpawnStat?.Invoke(msg);
        }

        public static void UpdateCounter(int amount)
        {
            OnUpdateStatOwnerCounter?.Invoke(amount);
        }
    }
}
