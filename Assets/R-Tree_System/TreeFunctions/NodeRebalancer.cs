using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Rebalances the content of nodes when they are over- or underflowing.
/// </summary>
public class NodeRebalancer
{
    /// <summary>
    /// Rebalances targeted overflowing node.
    /// </summary>
    /// <param name="_TargetNode">Node to rebalance</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RebalanceOverflowNodes(Node _TargetNode)
    {
        if (_TargetNode == null)
        {
            throw new ArgumentNullException("Node should not be Null when Rebalancing");
        }

        if (_TargetNode.Parent == null)
        {
            if (_TargetNode.IsOverflowing())
            {
                NodeSplitter.SplitNode(_TargetNode);
            }
            return;
        }

        Branch targetParent = _TargetNode.Parent.Entry as Branch;

        if (_TargetNode.Entry is Leaf leaf)
        {
            if (targetParent.EntryCount > 1)
            {
                Leaf redistributeLeaf = null;
                int amountToRedistribute = leaf.EntryCount - leaf.NodeCapacity;

                Array.Sort(targetParent.Children, (a, b) => CompareProximity(leaf.Rect.GetCenter(), a, b));

                Leaf sibling;

                for (int i = 0; i < targetParent.EntryCount; i++)
                {
                    if (targetParent.Children[i].ID == _TargetNode.ID)
                    {
                        continue;
                    }

                    sibling = targetParent.Children[i].Entry as Leaf;

                    if ((sibling.EntryCount + amountToRedistribute) <= sibling.NodeCapacity)
                    {
                        redistributeLeaf = sibling;
                        break;
                    }
                }

                if (redistributeLeaf != null)
                {
                    RedistributeOverflowEntries(leaf, redistributeLeaf, amountToRedistribute);
                    return;
                }
                else
                {
                    NodeSplitter.SplitNode(_TargetNode);

                    return;
                }
            }
        }
        else if (_TargetNode.Entry is Branch branch)
        {
            if (targetParent.EntryCount > 1)
            {
                Branch redistributeBranch = null;
                int amountToRedistribute = branch.EntryCount - branch.NodeCapacity;

                Array.Sort(targetParent.Children, (a, b) => CompareProximity(branch.Rect.GetCenter(), a, b));

                for (int i = 0; i < targetParent.EntryCount; i++)
                {
                    if (targetParent.Children[i].ID == _TargetNode.ID)
                    {
                        continue;
                    }

                    Branch sibling = targetParent.Children[i].Entry as Branch;

                    if ((sibling.EntryCount + amountToRedistribute) <= sibling.NodeCapacity)
                    {
                        redistributeBranch = sibling;
                        break;
                    }
                }

                if (redistributeBranch != null)
                {
                    RedistributeOverflowEntries(branch, redistributeBranch, amountToRedistribute);
                    return;
                }
                else
                {
                    NodeSplitter.SplitNode(_TargetNode);

                    return;
                }
            }
        }
    }

    /// <summary>
    /// Rebalances targeted underflowing node.
    /// </summary>
    /// <param name="_TargetNode">Node to rebalance</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void RebalanceUnderflowNodes(Node _TargetNode)
    {
        if (_TargetNode == null)
        {
            throw new ArgumentNullException("Node should not be Null when Rebalancing");
        }

        if (_TargetNode.Parent == null)
        {
            return;
        }

        Branch targetParent = _TargetNode.Parent.Entry as Branch;

        if (_TargetNode.Entry is Leaf leaf)
        {
            if (targetParent.EntryCount > 1)
            {
                Leaf redistributeLeaf = null;
                Leaf mergeLeaf = null;
                int amountToRedistribute = leaf.MinNodeCapacity - leaf.EntryCount;

                Array.Sort(targetParent.Children, (a, b) => CompareProximity(leaf.Rect.GetCenter(), a, b));

                Leaf sibling;

                for (int i = 0; i < targetParent.EntryCount; i++)
                {
                    if (targetParent.Children[i].ID == _TargetNode.ID)
                    {
                        continue;
                    }

                    sibling = targetParent.Children[i].Entry as Leaf;

                    if ((sibling.EntryCount - amountToRedistribute) >= sibling.MinNodeCapacity)
                    {
                        redistributeLeaf = sibling;
                        break;
                    }
                    else if ((sibling.EntryCount + leaf.EntryCount) <= leaf.NodeCapacity)
                    {
                        mergeLeaf = sibling;
                    }
                }

                if (redistributeLeaf != null)
                {
                    RedistributeUnderflowEntries(leaf, redistributeLeaf, amountToRedistribute);
                    return;
                }
                else if (mergeLeaf != null)
                {
                    MergeNode(leaf, mergeLeaf);
                    return;
                }
                else
                {
                    return;
                }
            }
        }
        else if (_TargetNode.Entry is Branch branch)
        {
            if (targetParent.EntryCount > 1)
            {
                Branch redistributeBranch = null;
                Branch mergeBranch = null;
                int amountToRedistribute = branch.MinNodeCapacity - branch.EntryCount;

                Array.Sort(targetParent.Children, (a, b) => CompareProximity(branch.Rect.GetCenter(), a, b));

                for (int i = 0; i < targetParent.EntryCount; i++)
                {
                    if (targetParent.Children[i].ID == _TargetNode.ID)
                    {
                        continue;
                    }

                    Branch sibling = targetParent.Children[i].Entry as Branch;

                    if ((sibling.EntryCount - amountToRedistribute) >= sibling.MinNodeCapacity)
                    {
                        redistributeBranch = sibling;
                        break;
                    }
                    else if ((sibling.EntryCount + branch.EntryCount) <= branch.NodeCapacity)
                    {
                        mergeBranch = sibling;
                    }
                }

                if (redistributeBranch != null)
                {
                    RedistributeUnderflowEntries(branch, redistributeBranch, amountToRedistribute);
                    return;
                }
                else if (mergeBranch != null)
                {
                    MergeNode(branch, mergeBranch);
                    return;
                }
                else
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// Redistributes entries from the partner node to the underflowing target node.
    /// </summary>
    /// <param name="_TargetLeaf">Node that is underflowing</param>
    /// <param name="_PartnerLeaf">Node that has enough entries to balance out target node without underflowing itself</param>
    /// <param name="_AmountToRedistribute">Amount of objects to redistribute</param>
    private static void RedistributeUnderflowEntries(Leaf _TargetLeaf, Leaf _PartnerLeaf, int _AmountToRedistribute)
    {
        LeafData[] newTargetData = new LeafData[_TargetLeaf.EntryCount + _AmountToRedistribute];
        LeafData[] newPartnerData = new LeafData[_PartnerLeaf.EntryCount - _AmountToRedistribute];

        Array.Sort(_PartnerLeaf.Data, (a, b) => CompareProximity(_TargetLeaf.Rect.GetCenter(), a, b));

        Array.Copy(_TargetLeaf.Data, 0, newTargetData, 0, _TargetLeaf.EntryCount);
        Array.Copy(_PartnerLeaf.Data, 0, newTargetData, _TargetLeaf.EntryCount, _AmountToRedistribute);

        _TargetLeaf.Data = newTargetData;
        _TargetLeaf.UpdateRect();

        Array.Copy(_PartnerLeaf.Data, _AmountToRedistribute, newPartnerData, 0, _PartnerLeaf.EntryCount - _AmountToRedistribute);

        _PartnerLeaf.Data = newPartnerData;
        _PartnerLeaf.UpdateRect();

        _TargetLeaf.EncapsulatingNode.Parent.Entry.UpdateRect();
    }

    /// <summary>
    /// Redistributes entries from the partner node to the underflowing target node.
    /// </summary>
    /// <param name="_TargetBranch">Node that is underflowing</param>
    /// <param name="_PartnerBranch">Node that has enough entries to balance out target node without underflowing itself</param>
    /// <param name="_AmountToRedistribute">Amount of objects to redistribute</param>
    private static void RedistributeUnderflowEntries(Branch _TargetBranch, Branch _PartnerBranch, int _AmountToRedistribute)
    {
        Node[] newTargetData = new Node[_TargetBranch.EntryCount + _AmountToRedistribute];
        Node[] newPartnerData = new Node[_PartnerBranch.EntryCount - _AmountToRedistribute];

        Array.Sort(_PartnerBranch.Children, (a, b) => CompareProximity(_TargetBranch.Rect.GetCenter(), a, b));

        Array.Copy(_TargetBranch.Children, 0, newTargetData, 0, _TargetBranch.EntryCount);
        Array.Copy(_PartnerBranch.Children, 0, newTargetData, _TargetBranch.EntryCount, _AmountToRedistribute);

        _TargetBranch.Children = newTargetData;

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        _TargetBranch.Children.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            node.Parent = _TargetBranch.EncapsulatingNode;
        });

        _TargetBranch.UpdateRect();

        Array.Copy(_PartnerBranch.Children, _AmountToRedistribute, newPartnerData, 0, _PartnerBranch.EntryCount - _AmountToRedistribute);

        _PartnerBranch.Children = newPartnerData;

        _PartnerBranch.Children.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            node.Parent = _PartnerBranch.EncapsulatingNode;
        });

        _PartnerBranch.UpdateRect();

        _TargetBranch.EncapsulatingNode.Parent.Entry.UpdateRect();
    }

    /// <summary>
    /// Redistributes entries from the overflowing target node to the partner node.
    /// </summary>
    /// <param name="_TargetLeaf">Node that is overflowing</param>
    /// <param name="_PartnerLeaf">Node that has enough space to balance out target node without overflowing itself</param>
    /// <param name="_AmountToRedistribute">Amount of objects to redistribute</param>
    private static void RedistributeOverflowEntries(Leaf _TargetLeaf, Leaf _PartnerLeaf, int _AmountToRedistribute)
    {
        LeafData[] newTargetData = new LeafData[_TargetLeaf.EntryCount - _AmountToRedistribute];
        LeafData[] newPartnerData = new LeafData[_PartnerLeaf.EntryCount + _AmountToRedistribute];

        Array.Sort(_TargetLeaf.Data, (a, b) => CompareProximity(_PartnerLeaf.Rect.GetCenter(), a, b));

        Array.Copy(_PartnerLeaf.Data, 0, newPartnerData, 0, _PartnerLeaf.EntryCount);
        Array.Copy(_TargetLeaf.Data, 0, newPartnerData, _PartnerLeaf.EntryCount, _AmountToRedistribute);

        _PartnerLeaf.Data = newPartnerData;
        _PartnerLeaf.UpdateRect();

        Array.Copy(_TargetLeaf.Data, _AmountToRedistribute, newTargetData, 0, _TargetLeaf.EntryCount - _AmountToRedistribute);

        _TargetLeaf.Data = newTargetData;
        _TargetLeaf.UpdateRect();

        _TargetLeaf.EncapsulatingNode.Parent.Entry.UpdateRect();
    }

    /// <summary>
    /// Redistributes entries from the overflowing target node to the partner node.
    /// </summary>
    /// <param name="_TargetBranch">Node that is overflowing</param>
    /// <param name="_PartnerBranch">Node that has enough space to balance out target node without overflowing itself</param>
    /// <param name="_AmountToRedistribute">Amount of objects to redistribute</param>
    private static void RedistributeOverflowEntries(Branch _TargetBranch, Branch _PartnerBranch, int _AmountToRedistribute)
    {
        Node[] newTargetData = new Node[_TargetBranch.EntryCount - _AmountToRedistribute];
        Node[] newPartnerData = new Node[_PartnerBranch.EntryCount + _AmountToRedistribute];

        Array.Sort(_TargetBranch.Children, (a, b) => CompareProximity(_PartnerBranch.Rect.GetCenter(), a, b));

        Array.Copy(_PartnerBranch.Children, 0, newPartnerData, 0, _PartnerBranch.EntryCount);
        Array.Copy(_TargetBranch.Children, 0, newPartnerData, _PartnerBranch.EntryCount, _AmountToRedistribute);

        _PartnerBranch.Children = newPartnerData;

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        _PartnerBranch.Children.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            node.Parent = _PartnerBranch.EncapsulatingNode;
        });

        _PartnerBranch.UpdateRect();

        Array.Copy(_TargetBranch.Children, _AmountToRedistribute, newTargetData, 0, _TargetBranch.EntryCount - _AmountToRedistribute);

        _TargetBranch.Children = newTargetData;

        _TargetBranch.Children.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            node.Parent = _TargetBranch.EncapsulatingNode;
        });

        _TargetBranch.UpdateRect();

        _TargetBranch.EncapsulatingNode.Parent.Entry.UpdateRect();
    }

    /// <summary>
    /// Merges two nodes into one.
    /// </summary>
    /// <param name="_TargetLeaf">Node that is underflowing</param>
    /// <param name="_PartnerLeaf">Node that is close to underflowing</param>
    private static void MergeNode(Leaf _TargetLeaf, Leaf _PartnerLeaf)
    {
        LeafData[] mergedData = new LeafData[_TargetLeaf.EntryCount + _PartnerLeaf.EntryCount];

        Array.Copy(_TargetLeaf.Data, 0, mergedData, 0, _TargetLeaf.EntryCount);
        Array.Copy(_PartnerLeaf.Data, 0, mergedData, _TargetLeaf.EntryCount, _PartnerLeaf.EntryCount);

        _TargetLeaf.Data = mergedData;

        _PartnerLeaf.Data = null;

        _TargetLeaf.EncapsulatingNode.Parent.Entry.UpdateRect();

        Remover.RemoveNode(_PartnerLeaf.EncapsulatingNode);
    }

    /// <summary>
    /// Merges two nodes into one.
    /// </summary>
    /// <param name="_TargetBranch">Node that is underflowing</param>
    /// <param name="_PartnerBranch">Node that is close to underflowing</param>
    private static void MergeNode(Branch _TargetBranch, Branch _PartnerBranch)
    {
        Node[] mergedData = new Node[_TargetBranch.EntryCount + _PartnerBranch.EntryCount];

        Array.Copy(_TargetBranch.Children, 0, mergedData, 0, _TargetBranch.EntryCount);
        Array.Copy(_PartnerBranch.Children, 0, mergedData, _TargetBranch.EntryCount, _PartnerBranch.EntryCount);

        _TargetBranch.Children = mergedData;

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        _TargetBranch.Children.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            node.Parent = _TargetBranch.EncapsulatingNode;
        });

        _PartnerBranch.Children = null;

        _TargetBranch.EncapsulatingNode.Parent.Entry.UpdateRect();

        Remover.RemoveNode(_PartnerBranch.EncapsulatingNode);
    }

    /// <summary>
    /// Compares proximity between two objects and a position. Used for distance based sorting.
    /// </summary>
    /// <param name="_TargetCenter">The position to compare to</param>
    /// <param name="_A">Object A</param>
    /// <param name="_B">Object B</param>
    /// <returns>Returns an integer that determines if A or B is closer to the target point</returns>
    private static int CompareProximity(Vector3 _TargetCenter, LeafData _A, LeafData _B)
    {
        float distanceA = Vector3.DistanceSquared(_TargetCenter, new Vector3(_A.PosX, _A.PosY, _A.PosZ));
        float distanceB = Vector3.DistanceSquared(_TargetCenter, new Vector3(_B.PosX, _B.PosY, _B.PosZ));

        if (distanceA < distanceB)
        {
            return -1;
        }
        else if (distanceA > distanceB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Compares proximity between two Nodes and a position. Used for distance based sorting.
    /// </summary>
    /// <param name="_TargetCenter">The position to compare to</param>
    /// <param name="_A">Node A</param>
    /// <param name="_B">Node B</param>
    /// <returns>Returns an integer that determines if A or B is closer to the target point</returns>
    private static int CompareProximity(Vector3 _TargetCenter, Node _A, Node _B)
    {
        float distanceA = Vector3.DistanceSquared(_TargetCenter, _A.Entry.Rect.GetCenter());
        float distanceB = Vector3.DistanceSquared(_TargetCenter, _B.Entry.Rect.GetCenter());

        if (distanceA < distanceB)
        {
            return -1;
        }
        else if (distanceA > distanceB)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}