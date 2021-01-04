public interface ISnapshot
{
    ITimelined Owner { get; }
    void Play();
}
