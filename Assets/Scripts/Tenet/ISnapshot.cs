public interface ISnapshot
{
    ITimelined Owner { get; }
    int Direction { get; }
    void Play();
}