public sealed class AudioSnapshot : BaseSnapshot
{
    public bool Started { get; }
    public float Time { get; }

    public AudioSnapshot(ITimelined owner, bool started, float time) : base(owner)
    {
        Started = started;
        Time = time;
    }
}
