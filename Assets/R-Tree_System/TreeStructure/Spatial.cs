/// <summary>
/// Base class for branch and leaf.
/// </summary>
public abstract class Spatial
{
    public abstract Node EncapsulatingNode { get; set; }

    public abstract Rect Rect { get; set; }

    public abstract int EntryCount { get; }

    public abstract int NodeCapacity { get; }

    public abstract int MinNodeCapacity { get; }

    public abstract void UpdateRect();
}