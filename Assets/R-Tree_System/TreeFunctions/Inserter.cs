using System;
using System.Numerics;

/// <summary>
/// Handles all object insertion related functionality.
/// </summary>
public class Inserter
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
    public static void InsertData(Node _Root, int _ObjIDX, float _PosX, float _PosY, float _PosZ)
    {
        LeafData insertData = new LeafData(_ObjIDX, _PosX, _PosY, _PosZ);

        Node targetNode = ChooseTargetNode(_Root, new Vector3(_PosX, _PosY, _PosZ));

        if (targetNode == null)
        {
            throw new Exception("targetNode should not be Null when Inserting!");
        }

        if (targetNode.Entry is Leaf leaf)
        {
            InsertIntoLeaf(targetNode, leaf, insertData);
        }
        else
        {
            throw new InvalidOperationException("Invalid node while inserting into tree: " + targetNode);
        }
    }

    /// <summary>
    /// Handles data insertion inside the actual leaf node.
    /// </summary>
    /// <param name="_LeafNode">The leaf node that either encapsulates the position of object or is closest by.</param>
    /// <param name="_Leaf">The entry inside the leaf node that holds the actual data.</param>
    /// <param name="_InsertData">The prepared data to insert.</param>
    private static void InsertIntoLeaf(Node _LeafNode, Leaf _Leaf, LeafData _InsertData)
    {
        LeafData[] oldData = _Leaf.Data;
        LeafData[] newData = new LeafData[oldData.Length + 1];

        Array.Copy(oldData, newData, oldData.Length);
        newData[oldData.Length] = _InsertData;

        _Leaf.Data = newData;

        _Leaf.UpdateRect();

        if (_LeafNode.Parent != null)
        {
            _LeafNode.Parent.Entry.UpdateRect();
        }

        if (_LeafNode.IsOverflowing())
        {
            NodeRebalancer.RebalanceOverflowNodes(_LeafNode);
        }
    }

    /// <summary>
    /// Searches for the optimal node to insert into.
    /// </summary>
    /// <param name="_Root">The root node of the tree.</param>
    /// <param name="_ObjPos">The position of the object to insert.</param>
    /// <returns>The leaf node that either encapsulates the position of object or is closest by.</returns>
    private static Node ChooseTargetNode(Node _Root, Vector3 _ObjPos)
    {
        Leaf result = TreeScanner.SearchLeaf(_Root, new Rect(_ObjPos, _ObjPos));
        if (result == null)
        {
            return null;
        }
        return result.EncapsulatingNode;
    }
}