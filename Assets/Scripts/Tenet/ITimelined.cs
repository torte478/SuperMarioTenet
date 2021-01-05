public interface ITimelined
{
    bool Replaying { get; }

    void Play(ISnapshot snapshot);
}
