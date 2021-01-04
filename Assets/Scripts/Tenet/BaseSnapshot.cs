public abstract class BaseSnapshot : ISnapshot
{
    public ITimelined Owner { get; }

    protected BaseSnapshot(ITimelined owner)
    {
        Owner = owner;
    }

    public void Play()
    {
        Owner.Play(this);
    }
}