public interface IPool
{
    int MaxSize { get; }
    int CurrentCount { get; }
    void Shrink();
}