using System;

/// <summary>
/// Handles all object insertion related functionality.
/// </summary>
public class BulkInserter
{
    /// <summary>
    /// Inserts data into the tree.
    /// </summary>
    /// <param name="_Root">Root of the tree</param>
    /// <param name="_ObjIDX">Index of object to insert</param>
    /// <param name="_PosX">X position of object to insert</param>
    /// <param name="_PosY">Y position of object to insert</param>
    /// <param name="_PosZ">Z position of object to insert</param>
    /// <exception cref="Exception"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void InsertData(Node _Root, int[] _ObjIndices, float[] _XPositions, float[] _YPositions, float[] _ZPositions)
    {
        Node _BulkDataRoot = CreateBulk(_Root.Entry.NodeCapacity, _Root.Entry.MinNodeCapacity, _ObjIndices, _XPositions, _YPositions, _ZPositions);

        Node targetNode = ChooseTargetNode(_Root, _BulkDataRoot.Entry.Rect);

        if (targetNode == null)
        {
            throw new Exception("targetNode should not be Null when Inserting!");
        }

        if (targetNode.Entry is Leaf leaf)
        {
            InsertPartialTree(targetNode.Parent, (Branch)targetNode.Parent.Entry, _BulkDataRoot);
        }
        else
        {
            InsertPartialTree(targetNode, (Branch)targetNode.Entry, _BulkDataRoot);
        }
    }

    /// <summary>
    /// Handles insertion of partial Trees into Branch nodes.
    /// </summary>
    /// <param name="_Node">The leaf node that either encapsulates the position of Bulk or is closest by.</param>
    /// <param name="_Branch">The entry inside the node that holds the actual data.</param>
    /// <param name="_InsertData">The partial tree to insert.</param>
    private static void InsertPartialTree(Node _Node, Branch _Branch, Node _InsertData)
    {
        Node[] newData = new Node[_Branch.Children.Length + 1];
        _Branch.Children.CopyTo(newData, 0);
        newData[_Branch.Children.Length] = _InsertData;

        _Branch.UpdateRect();

        if (_Node.Parent != null)
        {
            _Node.Parent.Entry.UpdateRect();
        }

        if (_Node.IsOverflowing())
        {
            NodeRebalancer.RebalanceOverflowNodes(_Node);
        }
    }

    /// <summary>
    /// Searches for the optimal node to insert into.
    /// </summary>
    /// <param name="_Root">The root node of the tree.</param>
    /// <param name="_ObjPos">The position of the object to insert.</param>
    /// <returns>The leaf node that either encapsulates the position of object or is closest by.</returns>
    private static Node ChooseTargetNode(Node _Root, Rect _Bounds)
    {
        Leaf result = TreeScanner.SearchLeaf(_Root, _Bounds);
        if (result == null)
        {
            return null;
        }
        return result.EncapsulatingNode;
    }

    /// <summary>
    /// Creates a Sub-Tree from a preset Data Packet.
    /// </summary>
    /// <param name="_NodeCapacity">Max Capacity of Nodes.</param>
    /// <param name="_MinNodeCapacity">Min Capacity of Nodes.</param>
    /// <param name="_ObjIndices">Array of Object indices.</param>
    /// <param name="_XPositions"></param>
    /// <param name="_YPositions"></param>
    /// <param name="_ZPositions"></param>
    /// <returns></returns>
    private static Node CreateBulk(int _NodeCapacity, int _MinNodeCapacity, int[] _ObjIndices, float[] _XPositions, float[] _YPositions, float[] _ZPositions)
    {
        RTree partialTree = new RTree(_NodeCapacity, _MinNodeCapacity);

        for (int i = 0; i < _ObjIndices.Length; i++)
        {
            partialTree.Insert(new LeafData(_ObjIndices[i], _XPositions[i], _YPositions[i], _ZPositions[i]));
        }

        return partialTree.Root;
    }
}