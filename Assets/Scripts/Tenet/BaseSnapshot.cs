public abstract class BaseSnapshot : ISnapshot
{
    public ITimelined Owner { get; }
    public int Direction { get; protected set; }

    protected BaseSnapshot(ITimelined owner)
    {
        Owner = owner;
        Direction = 0;
    }

    public void Play()
    {
        Owner.Play(this);
    }
}