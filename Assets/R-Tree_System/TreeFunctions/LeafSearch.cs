using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

/// <summary>
/// Searches for leaf nodes within the specified area. Gets the closest one if no intersecting nodes are found.
/// </summary>
public class LeafSearch
{
    /// <summary>
    /// Searches for leaf nodes within the specified area. Gets the closest one found.
    /// If no intersecting nodes are found returns the closest non intersecting node.
    /// </summary>
    /// <param name="_Root">The root node of the tree.</param>
    /// <param name="_Range">The area to search for leaf nodes</param>
    /// <param name="_Result">The closest leaf node found within the specified area or around</param>
    public void StartSearch(Node _Root, Rect _Range, out Leaf _Result)
    {
        IEnumerable<Node> nodes;

        if (_Root.Entry is Branch branch)
        {
            nodes = branch.Children;
        }
        else
        {
            _Result = (Leaf)_Root.Entry;
            return;
        }

        List<Leaf> intersectingLeaves = new List<Leaf>();
        List<Leaf> nonIntersectingLeaves = new List<Leaf>();

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        nodes.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            List<Leaf> leaves = ScanRange(_Range, node, true);
            lock (intersectingLeaves)
            {
                intersectingLeaves.AddRange(leaves);
            }

            leaves = ScanRange(_Range, node, false);
            lock (nonIntersectingLeaves)
            {
                nonIntersectingLeaves.AddRange(leaves);
            }
        });

        Vector3 rangeCenter = _Range.GetCenter();

        if (intersectingLeaves.Count > 0)
        {
            _Result = intersectingLeaves.OrderBy(leaf =>
            Vector3.DistanceSquared(rangeCenter, leaf.Rect.GetCenter())).First();
        }
        else if (nonIntersectingLeaves.Count > 0)
        {
            _Result = nonIntersectingLeaves.OrderBy(leaf =>
             Vector3.DistanceSquared(rangeCenter, leaf.Rect.GetCenter())).First();
        }
        else
        {
            _Result = null;
        }
    }

    /// <summary>
    /// Searches for leaf nodes within the specified area, containing a certain object.
    /// </summary>
    /// <param name="_Root">The root node of the tree.</param>
    /// <param name="_EntryIndex">The index of the object that has to be contained</param>
    /// <param name="_Range">The area to search for leaf nodes</param>
    /// <param name="_Result">The leaf nodes found within the specified area</param>
    public void StartSearch(Node _Root, int _EntryIndex, Rect _Range, out Leaf _Result)
    {
        IEnumerable<Node> nodes;

        if (_Root.Entry is Branch branch)
        {
            nodes = branch.Children;
        }
        else
        {
            _Result = (Leaf)_Root.Entry;
            return;
        }

        List<Leaf> intersectingLeaves = new List<Leaf>();

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };
        nodes.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(node =>
        {
            List<Leaf> leaves = ScanRange(_Range, node, true, _EntryIndex);
            lock (intersectingLeaves)
            {
                intersectingLeaves.AddRange(leaves);
            }
        });

        if (intersectingLeaves.Count > 0)
        {
            Leaf result = null;

            intersectingLeaves.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism).ForAll(leaf =>
            {
                if (leaf == null)
                {
                    return;
                }
                for (int i = 0; i < leaf.EntryCount; i++)
                {
                    if (leaf.Data[i].ObjIDX == _EntryIndex)
                    {
                        result = leaf;
                    }
                }
            });

            _Result = result;
            return;
        }

        _Result = null;
    }

    /// <summary>
    /// Used for recursively scanning the entire tree.
    /// </summary>
    /// <param name="_Range">The area to search for leaf nodes</param>
    /// <param name="_Start">The node to start searching from</param>
    /// <param name="_Intersecting">Wether this method should check for intersecting nodes only</param>
    /// <returns>The leaf nodes found within the specified area</returns>
    private List<Leaf> ScanRange(Rect _Range, Node _Start, bool _Intersecting)
    {
        List<Leaf> resultData = new List<Leaf>();

        if (_Start.Entry is Branch branch)
        {
            Leaf result;
            foreach (Node child in branch.Children)
            {
                if (_Intersecting)
                {
                    if (child.Entry is Branch)
                    {
                        StartSearch(child, _Range, out result);
                        resultData.Add(result);
                    }
                    else if (child.Entry is Leaf leaf && TreeScanner.Contains(child.Entry.Rect, _Range))
                    {
                        resultData.Add(leaf);
                    }
                }
                else
                {
                    if (child.Entry is Branch)
                    {
                        StartSearch(child, _Range, out result);
                        resultData.Add(result);
                    }
                    else if (child.Entry is Leaf leaf)
                    {
                        resultData.Add(leaf);
                    }
                }
            }
        }
        else if (_Start.Entry is Leaf leaf)
        {
            resultData.Add(leaf);
        }

        return resultData;
    }

    /// <summary>
    /// Used for recursively scanning the entire tree.
    /// </summary>
    /// <param name="_Range">The area to search for leaf nodes</param>
    /// <param name="_Start">The node to start searching from</param>
    /// <param name="_Intersecting">Wether this method should check for intersecting nodes only</param>
    /// <param name="_EntryIndex">The index of the object that has to be contained</param>
    /// <returns>The leaf nodes found within the specified area</returns>
    private List<Leaf> ScanRange(Rect _Range, Node _Start, bool _Intersecting, int _EntryIndex)
    {
        List<Leaf> resultData = new List<Leaf>();

        if (_Start.Entry is Branch branch)
        {
            Leaf result;
            foreach (Node child in branch.Children)
            {
                if (_Intersecting)
                {
                    if (child.Entry is Branch)
                    {
                        StartSearch(child, _EntryIndex, _Range, out result);
                        resultData.Add(result);
                    }
                    else if (child.Entry is Leaf leaf && TreeScanner.Contains(child.Entry.Rect, _Range))
                    {
                        resultData.Add(leaf);
                    }
                }
                else
                {
                    if (child.Entry is Branch)
                    {
                        StartSearch(child, _EntryIndex, _Range, out result);
                        resultData.Add(result);
                    }
                    else if (child.Entry is Leaf leaf)
                    {
                        resultData.Add(leaf);
                    }
                }
            }
        }
        else if (_Start.Entry is Leaf leaf)
        {
            resultData.Add(leaf);
        }

        return resultData;
    }
}