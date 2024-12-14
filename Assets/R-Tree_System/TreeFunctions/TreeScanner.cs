using System;

/// <summary>
/// Contains search related functionality.
/// </summary>
public static class TreeScanner
{
    private static LeafDataSearch m_DataSearcher = new LeafDataSearch();
    private static LeafSearch m_LeafSearcher = new LeafSearch();

    /// <summary>
    /// Searches for objects within a certain area.
    /// </summary>
    /// <param name="_Root">The root of the tree</param>
    /// <param name="_Range">The area to search in</param>
    /// <returns>All objects within specified area</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static LeafData[] SearchLeafData(Node _Root, Rect _Range)
    {
        if (_Root.Entry == null)
        {
            throw new InvalidOperationException("Empty Node while scanning R-Tree: " + _Root);
        }
        if (_Range.LowerLeft.X > _Range.UpperRight.X || _Range.LowerLeft.Y > _Range.UpperRight.Y)
        {
            throw new InvalidOperationException("Invalid Range for scanning R-Tree: " + _Range.LowerLeft.X +
                                                " | " + _Range.LowerLeft.Y + " - " + _Range.UpperRight.X +
                                                " | " + _Range.UpperRight.Y);
        }

        LeafData[] result;
        m_DataSearcher.StartSearch(_Root, _Range, out result);
        return result;
    }

    /// <summary>
    /// Searches for leaf nodes within the specified area. Gets the closest one found.
    /// If no intersecting nodes are found returns the closest non intersecting node.
    /// </summary>
    /// <param name="_Root">The root of the tree</param>
    /// <param name="_Range">The area to search in</param>
    /// <returns>The closest leaf node found within the specified area or around</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Leaf SearchLeaf(Node _Root, Rect _Range)
    {
        if (_Root.Entry == null)
        {
            throw new InvalidOperationException("Empty Node while scanning R-Tree: " + _Root);
        }
        if (_Range.LowerLeft.X > _Range.UpperRight.X || _Range.LowerLeft.Y > _Range.UpperRight.Y)
        {
            throw new InvalidOperationException("Invalid Range for scanning R-Tree: " + _Range.LowerLeft.X +
                                                " | " + _Range.LowerLeft.Y + " - " + _Range.UpperRight.X +
                                                " | " + _Range.UpperRight.Y);
        }

        Leaf result;
        m_LeafSearcher.StartSearch(_Root, _Range, out result);
        return result;
    }

    /// <summary>
    /// Searches for a leaf node containing the defined object at the defined area.
    /// </summary>
    /// <param name="_Root">The root of the tree</param>
    /// <param name="_EntryIndex">The index of the object to look for</param>
    /// <param name="_Range">The area where to look</param>
    /// <returns>The leaf containing the object</returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Leaf SearchLeaf(Node _Root, int _EntryIndex, Rect _Range)
    {
        if (_Root.Entry == null)
        {
            throw new InvalidOperationException("Empty Node while scanning R-Tree: " + _Root);
        }
        if (_Range.LowerLeft.X > _Range.UpperRight.X || _Range.LowerLeft.Y > _Range.UpperRight.Y)
        {
            throw new InvalidOperationException("Invalid Range for scanning R-Tree: " + _Range.LowerLeft.X +
                                                " | " + _Range.LowerLeft.Y + " - " + _Range.UpperRight.X +
                                                " | " + _Range.UpperRight.Y);
        }

        Leaf result;
        m_LeafSearcher.StartSearch(_Root, _EntryIndex, _Range, out result);
        return result;
    }

    /// <summary>
    /// Checks if area A contains area B.
    /// </summary>
    /// <param name="A">The area to check</param>
    /// <param name="B">The area that should be contained</param>
    /// <returns>True if B is completely encapsulated by A</returns>
    public static bool Contains(Rect A, Rect B)
    {
        return
           A.LowerLeft.X <= B.LowerLeft.X &&
           A.UpperRight.X >= B.UpperRight.X &&
           A.LowerLeft.Y <= B.LowerLeft.Y &&
           A.UpperRight.Y >= B.UpperRight.Y &&
           A.LowerLeft.Z <= B.LowerLeft.Z &&
           A.UpperRight.Z >= B.UpperRight.Z;
    }

    public static bool Intersects(Rect A, Rect B)
    {
        return
            A.LowerLeft.X <= B.UpperRight.X &&
            A.UpperRight.X >= B.LowerLeft.X &&
            A.LowerLeft.Y <= B.UpperRight.Y &&
            A.UpperRight.Y >= B.LowerLeft.Y &&
            A.LowerLeft.Z <= B.UpperRight.Z &&
            A.UpperRight.Z >= B.LowerLeft.Z;
    }
}