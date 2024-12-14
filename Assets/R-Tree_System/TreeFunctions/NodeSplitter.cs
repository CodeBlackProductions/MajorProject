using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

internal enum Axis
{
    X, Y, Z
}

/// <summary>
/// Used for splitting overflowing nodes into two nodes.
/// </summary>
public class NodeSplitter
{
    /// <summary>
    /// Splits an overflowing node into two, non overflowing nodes.
    /// </summary>
    /// <param name="_NodeToSplit">Overflowing node that should be split up</param>
    public static void SplitNode(Node _NodeToSplit)
    {
        Stack<Node> splitStack = new Stack<Node>();
        splitStack.Push(_NodeToSplit);

        while (splitStack.Count > 0)
        {
            Axis splitAxis;

            Node localNodeToSplit = splitStack.Pop();

            if (localNodeToSplit.Parent == null || localNodeToSplit.Parent == localNodeToSplit || localNodeToSplit.Parent.Entry is Leaf)
            {
                PrepareRootSplit(localNodeToSplit);
            }

            if (localNodeToSplit.Entry is Branch branch)
            {
                if (localNodeToSplit.Parent.Entry is not Branch parentBranch)
                {
                    return;
                }
              
                splitAxis = CalculateSplitAxis(branch);

                Node[] sortArray;
                SortAlongAxis(branch, splitAxis, out sortArray);

                int pivot = FindPivotEntry(sortArray);

                Node[][] splitChildren = SplitChildren(sortArray, pivot);

                Node[] newNodes = CreateNewNodes(localNodeToSplit.Parent, localNodeToSplit.Level, splitChildren, branch.NodeCapacity, branch.MinNodeCapacity);

                UpdateParentChildren(parentBranch, localNodeToSplit, newNodes);

                if (localNodeToSplit.Parent.IsOverflowing())
                {
                    splitStack.Push(localNodeToSplit.Parent);
                }
            }
            else if (localNodeToSplit.Entry is Leaf leaf)
            {
                if (localNodeToSplit.Parent.Entry is not Branch parentBranch)
                {
                    return;
                }

                splitAxis = CalculateSplitAxis(leaf);

                LeafData[] sortArray;
                SortAlongAxis(leaf, splitAxis, out sortArray);

                int pivot = FindPivotEntry(sortArray);

                LeafData[][] splitChildren = SplitChildren(sortArray, pivot);

                Node[] newNodes = CreateNewNodes(localNodeToSplit.Parent, _NodeToSplit.Level, splitChildren, leaf.NodeCapacity, leaf.MinNodeCapacity);

                UpdateParentChildren(parentBranch, localNodeToSplit, newNodes);

                if (localNodeToSplit.Parent.IsOverflowing())
                {
                    splitStack.Push(localNodeToSplit.Parent);
                }
            }
        }
    }

    /// <summary>
    /// Creates a new Root and adds a new layer to the tree before splitting the old one.
    /// </summary>
    /// <param name="_NodeToSplit">The old Root node</param>
    public static void PrepareRootSplit(Node _NodeToSplit)
    {
        Node newRoot = new Node(_NodeToSplit.Level + 1,
                                new Branch(null, _NodeToSplit.Entry.Rect, new Node[] { _NodeToSplit }, _NodeToSplit.Entry.NodeCapacity, _NodeToSplit.Entry.MinNodeCapacity),
                                null, _NodeToSplit.ParentTree);

        newRoot.Entry.EncapsulatingNode = newRoot;

        _NodeToSplit.ParentTree.Root = newRoot;
        _NodeToSplit.Parent = newRoot;
    }

    #region Methods for the actual split

    /// <summary>
    /// Updates the parent of the node that got split, so it contains the new children.
    /// </summary>
    /// <param name="_Parent">Parent of the node that got split</param>
    /// <param name="_OldNode">The node that got split</param>
    /// <param name="_NewNodes">The new nodes the old one got split into</param>
    private static void UpdateParentChildren(Branch _Parent, Node _OldNode, Node[] _NewNodes)
    {
        Node[] newChildren = new Node[_Parent.Children.Length + 1];

        for (int i = 0; i < _Parent.Children.Length; i++)
        {
            if (_Parent.Children[i].ID == _OldNode.ID)
            {
                newChildren[i] = _NewNodes[0];
            }
            else
            {
                newChildren[i] = _Parent.Children[i];
            }
        }
        newChildren[_Parent.Children.Length] = _NewNodes[1];

        _Parent.Children = newChildren;
        _Parent.UpdateRect();
    }

    /// <summary>
    /// Creates two new nodes and adds the already split content to them
    /// </summary>
    /// <param name="_Parent">Parent of the split node</param>
    /// <param name="_Level">Tree level the new nodes are on</param>
    /// <param name="_SplitChildren">The new nodes</param>
    /// <param name="_NodeCapacity">Max node capacity</param>
    /// <param name="_MinNodeCapacity">Min node capacity</param>
    /// <returns>Returns the two new nodes, containing the split content of the old node</returns>
    private static Node[] CreateNewNodes(Node _Parent, int _Level, Node[][] _SplitChildren, int _NodeCapacity, int _MinNodeCapacity)
    {
        Node nodeA = new Node(_Level, new Branch(_Parent, CreateNewNodeRect(_SplitChildren[0]), _SplitChildren[0],
                                                _NodeCapacity, _MinNodeCapacity), _Parent, _Parent.ParentTree);

        nodeA.Entry.EncapsulatingNode = nodeA;
        for (int i = 0; i < _SplitChildren[0].Length; i++)
        {
            _SplitChildren[0][i].Parent = nodeA;
        }

        Node nodeB = new Node(_Level, new Branch(_Parent, CreateNewNodeRect(_SplitChildren[1]), _SplitChildren[1], _NodeCapacity, _MinNodeCapacity),
                                                _Parent, _Parent.ParentTree);

        nodeB.Entry.EncapsulatingNode = nodeB;
        for (int i = 0; i < _SplitChildren[1].Length; i++)
        {
            _SplitChildren[1][i].Parent = nodeB;
        }

        return new Node[] { nodeA, nodeB };
    }

    /// <summary>
    /// Creates two new nodes and adds the already split content to them
    /// </summary>
    /// <param name="_Parent">Parent of the split node</param>
    /// <param name="_Level">Tree level the new nodes are on</param>
    /// <param name="_SplitChildren">The new nodes</param>
    /// <param name="_NodeCapacity">Max node capacity</param>
    /// <param name="_MinNodeCapacity">Min node capacity</param>
    /// <returns>Returns the two new nodes, containing the split content of the old node</returns>
    private static Node[] CreateNewNodes(Node _Parent, int _Level, LeafData[][] _SplitChildren, int _NodeCapacity, int _MinNodeCapacity)
    {
        Node nodeA = new Node(_Level, new Leaf(_Parent, CreateNewNodeRect(_SplitChildren[0]), _SplitChildren[0], _NodeCapacity, _MinNodeCapacity),
                                                                         _Parent, _Parent.ParentTree);
        nodeA.Entry.EncapsulatingNode = nodeA;

        Node nodeB = new Node(_Level, new Leaf(_Parent, CreateNewNodeRect(_SplitChildren[1]), _SplitChildren[1], _NodeCapacity, _MinNodeCapacity), 
                                                                         _Parent, _Parent.ParentTree);
        nodeB.Entry.EncapsulatingNode = nodeB;

        return new Node[] { nodeA, nodeB };
    }

    /// <summary>
    /// Creates a rect that encapsulates the input nodes.
    /// </summary>
    /// <param name="_Nodes">The nodes to build the rect around</param>
    /// <returns>a rect that encapsulates the input nodes</returns>
    /// <exception cref="ArgumentException"></exception>
    private static Rect CreateNewNodeRect(Node[] _Nodes)
    {
        if (_Nodes == null || _Nodes.Length == 0)
        {
            throw new ArgumentException("Nodes array cannot be null or empty when creating new Rect", nameof(_Nodes));
        }

        Vector3 lowerLeft = _Nodes[0].Entry.Rect.LowerLeft;
        Vector3 upperRight = _Nodes[0].Entry.Rect.UpperRight;

        for (int i = 1; i < _Nodes.Length; i++)
        {
            Rect currentRect = _Nodes[i].Entry.Rect;

            lowerLeft.X = Math.Min(currentRect.LowerLeft.X, lowerLeft.X);
            lowerLeft.Y = Math.Min(currentRect.LowerLeft.Y, lowerLeft.Y);
            lowerLeft.Z = Math.Min(currentRect.LowerLeft.Z, lowerLeft.Z);

            upperRight.X = Math.Max(currentRect.UpperRight.X, upperRight.X);
            upperRight.Y = Math.Max(currentRect.UpperRight.Y, upperRight.Y);
            upperRight.Z = Math.Max(currentRect.UpperRight.Z, upperRight.Z);
        }

        return new Rect(lowerLeft, upperRight);
    }

    /// <summary>
    /// Creates a rect that encapsulates the input objects.
    /// </summary>
    /// <param name="_Nodes">The objects to build the rect around</param>
    /// <returns>a rect that encapsulates the input objects</returns>
    /// <exception cref="ArgumentException"></exception>
    private static Rect CreateNewNodeRect(LeafData[] _Nodes)
    {
        if (_Nodes == null || _Nodes.Length == 0)
        {
            throw new ArgumentException("LeafData array cannot be null or empty when creating new Rect", nameof(_Nodes));
        }

        float X = _Nodes[0].PosX;
        float Y = _Nodes[0].PosY;
        float Z = _Nodes[0].PosZ;

        Vector3 lowerLeft = new Vector3(X, Y, Z);
        Vector3 upperRight = new Vector3(X, Y, Z);

        for (int i = 1; i < _Nodes.Length; i++)
        {
            X = _Nodes[i].PosX;
            Y = _Nodes[i].PosY;
            Z = _Nodes[i].PosZ;

            lowerLeft.X = Math.Min(X, lowerLeft.X);
            lowerLeft.Y = Math.Min(Y, lowerLeft.Y);
            lowerLeft.Z = Math.Min(Z, lowerLeft.Z);

            upperRight.X = Math.Max(X, upperRight.X);
            upperRight.Y = Math.Max(Y, upperRight.Y);
            upperRight.Z = Math.Max(Z, upperRight.Z);
        }

        return new Rect(lowerLeft, upperRight);
    }

    /// <summary>
    /// Returns the pivot point (middle) of a data set.
    /// </summary>
    /// <param name="_Data">The data to find the pivot of</param>
    /// <returns></returns>
    private static int FindPivotEntry(Node[] _Data)
    {
        int pivotIndex = 0;

        pivotIndex = (int)Math.Round(_Data.Length * 0.5f);

        return pivotIndex;
    }

    /// <summary>
    /// Returns the pivot point (middle) of a data set.
    /// </summary>
    /// <param name="_Data">The data to find the pivot of</param>
    /// <returns></returns>
    private static int FindPivotEntry(LeafData[] _Data)
    {
        int pivotIndex = 0;

        pivotIndex = (int)Math.Round(_Data.Length * 0.5f) - 1;

        return pivotIndex;
    }

    /// <summary>
    /// Splits an already sorted array at the pivot, to create two sets of data.
    /// </summary>
    /// <param name="_SortedArray">The array to split</param>
    /// <param name="_PivotIndex">The index to split the array at</param>
    /// <returns>Two arrays of data</returns>
    private static Node[][] SplitChildren(Node[] _SortedArray, int _PivotIndex)
    {
        Node[] nodesA = new Node[_PivotIndex + 1];
        Node[] nodesB = new Node[_SortedArray.Length - (_PivotIndex + 1)];

        Array.Copy(_SortedArray, 0, nodesA, 0, _PivotIndex + 1);
        Array.Copy(_SortedArray, _PivotIndex + 1, nodesB, 0, nodesB.Length);

        return new Node[][] { nodesA, nodesB };
    }

    /// <summary>
    /// Splits an already sorted array at the pivot, to create two sets of data.
    /// </summary>
    /// <param name="_SortedArray">The array to split</param>
    /// <param name="_PivotIndex">The index to split the array at</param>
    /// <returns>Two arrays of data</returns>
    private static LeafData[][] SplitChildren(LeafData[] _SortedArray, int _PivotIndex)
    {
        LeafData[] dataA = new LeafData[_PivotIndex + 1];
        LeafData[] dataB = new LeafData[_SortedArray.Length - (_PivotIndex + 1)];

        Array.Copy(_SortedArray, 0, dataA, 0, _PivotIndex + 1);
        Array.Copy(_SortedArray, _PivotIndex + 1, dataB, 0, dataB.Length);

        return new LeafData[][] { dataA, dataB };
    }

    #endregion Methods for the actual split

    #region Methods to sort along axis

    /// <summary>
    /// Sorts the children of a node along the selected axis.
    /// </summary>
    /// <param name="_NodeEntry">The node to sort</param>
    /// <param name="_SplitAxis">The axis to sort along</param>
    /// <param name="_SortedArray">An array of the children, sorted along the axis</param>
    private static void SortAlongAxis(Branch _NodeEntry, Axis _SplitAxis, out Node[] _SortedArray)
    {
        _SortedArray = _NodeEntry.Children
                                 .OrderBy(item => GetCoordinate(item.Entry.Rect.GetCenter(), _SplitAxis))
                                 .ToArray();
    }

    /// <summary>
    /// Sorts the children of a node along the selected axis.
    /// </summary>
    /// <param name="_NodeEntry">The node to sort</param>
    /// <param name="_SplitAxis">The axis to sort along</param>
    /// <param name="_SortedArray">An array of the children, sorted along the axis</param>
    private static void SortAlongAxis(Leaf _NodeEntry, Axis _SplitAxis, out LeafData[] _SortedArray)
    {
        _SortedArray = _NodeEntry.Data
                                 .OrderBy(item => GetCoordinate(new Vector3(item.PosX, item.PosY, item.PosZ), _SplitAxis))
                                 .ToArray();
    }

    #endregion Methods to sort along axis

    #region Methods for finding target axis

    /// <summary>
    /// Calculates the maximum distance between children along a specific axis.
    /// </summary>
    /// <param name="_Axis">The axis to calculate the spread on</param>
    /// <param name="_Branch">The node that contains the children to check</param>
    /// <returns>The maximum distance along the defined axis as float value.</returns>
    private static float CalculateSpread(Axis _Axis, Branch _Branch)
    {
        float lowMin;
        float lowMax;
        float highMin;
        float highMax;

        lowMin = GetCoordinate(_Branch.Children[0].Entry.Rect.LowerLeft, _Axis);
        lowMax = lowMin;
        highMin = GetCoordinate(_Branch.Children[0].Entry.Rect.UpperRight, _Axis);
        highMax = highMin;

        for (int i = 1; i < _Branch.Children.Length; i++)
        {
            Node child = _Branch.Children[i];
            
            lowMin = Math.Min(GetCoordinate(child.Entry.Rect.LowerLeft, _Axis), lowMin);
            lowMax = Math.Max(GetCoordinate(child.Entry.Rect.LowerLeft, _Axis), lowMax);
            highMin = Math.Min(GetCoordinate(child.Entry.Rect.UpperRight, _Axis), highMin);
            highMax = Math.Max(GetCoordinate(child.Entry.Rect.UpperRight, _Axis), highMax);
        }

        return (lowMax - lowMin) + (highMax - highMin);
    }

    /// <summary>
    /// Calculates the maximum distance between children along a specific axis.
    /// </summary>
    /// <param name="_Axis">The axis to calculate the spread on</param>
    /// <param name="_Leaf">The node that contains the children to check</param>
    /// <returns>The maximum distance along the defined axis as float value.</returns>
    private static float CalculateSpread(Axis _Axis, Leaf _Leaf)
    {
        float min;
        float max;

        min = GetCoordinate(_Leaf.Data[0], _Axis);
        max = min;

        for (int i = 1; i < _Leaf.Data.Length; i++)
        {
            LeafData obj = _Leaf.Data[i];
            min = Math.Min(GetCoordinate(obj, _Axis), min);
            max = Math.Max(GetCoordinate(obj, _Axis), max);
        }

        return max - min;
    }

    /// <summary>
    /// Retrieves the axis related coordinate of a vector.
    /// </summary>
    /// <param name="_Pos">The vector to retrieve from</param>
    /// <param name="_Axis">The axis to retrieve</param>
    /// <returns>The float value of one of the vector coordinates</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static float GetCoordinate(Vector3 _Vector, Axis _Axis)
    {
        switch (_Axis)
        {
            case Axis.X:
                return _Vector.X;

            case Axis.Y:
                return _Vector.Y;

            case Axis.Z:
                return _Vector.Z;

            default:
                throw new ArgumentOutOfRangeException("Invalid Axis while trying to split Node!");
        }
    }

    /// <summary>
    /// Retrieves the axis related coordinate of an object.
    /// </summary>
    /// <param name="_Pos">The object to retrieve from</param>
    /// <param name="_Axis">The axis to retrieve</param>
    /// <returns>The float value of one of the object coordinates</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private static float GetCoordinate(LeafData _Object, Axis _Axis)
    {
        switch (_Axis)
        {
            case Axis.X:
                return _Object.PosX;

            case Axis.Y:
                return _Object.PosY;

            case Axis.Z:
                return _Object.PosZ;

            default:
                throw new ArgumentOutOfRangeException("Invalid Axis while trying to split Node!");
        }
    }

    /// <summary>
    /// Compares spread values.
    /// </summary>
    /// <param name="_X">Spread along x axis</param>
    /// <param name="_Y">Spread along y axis</param>
    /// <param name="_Z">Spread along z axis</param>
    /// <returns>The axis with the largest spread</returns>
    private static Axis FindLargestSpread(float _X, float _Y, float _Z)
    {
        float[] val = new float[] { _X, _Y, _Z };

        int maxIndex = Array.IndexOf(val, val.Max());

        return (Axis)maxIndex;
    }

    /// <summary>
    /// Calculates the axis along wich a node should be split
    /// </summary>
    /// <param name="_Branch">The node to split</param>
    /// <returns>The axis along wich the node should be split</returns>
    private static Axis CalculateSplitAxis(Branch _Branch)
    {
        float spreadX = CalculateSpread(Axis.X, _Branch);
        float spreadY = CalculateSpread(Axis.Y, _Branch);
        float spreadZ = CalculateSpread(Axis.Z, _Branch);
        return FindLargestSpread(spreadX, spreadY, spreadZ);
    }

    /// <summary>
    /// Calculates the axis along wich a node should be split
    /// </summary>
    /// <param name="_Leaf">The node to split</param>
    /// <returns>The axis along wich the node should be split</returns>
    private static Axis CalculateSplitAxis(Leaf _Leaf)
    {
        float spreadX = CalculateSpread(Axis.X, _Leaf);
        float spreadY = CalculateSpread(Axis.Y, _Leaf);
        float spreadZ = CalculateSpread(Axis.Z, _Leaf);
        return FindLargestSpread(spreadX, spreadY, spreadZ);
    }

    #endregion Methods for finding target axis
}