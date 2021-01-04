public sealed class AudioSnapshot : ISnapshot
{
    public ITimelined Owner { get; }
    public bool Started { get; }
    public float Time { get; }

    public AudioSnapshot(ITimelined owner, bool started, float time)
    {
        Owner = owner;
        Started = started;
        Time = time;
    }
}