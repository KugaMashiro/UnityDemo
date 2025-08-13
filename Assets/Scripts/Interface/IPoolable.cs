public interface IPoolable
{
    bool IsInUse { get; set; }
    void Reset();
}