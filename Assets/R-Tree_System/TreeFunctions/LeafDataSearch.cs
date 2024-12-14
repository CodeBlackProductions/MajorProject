using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Searches for object data packages inside the tree.
/// </summary>
public class LeafDataSearch
{
    /// <summary>
    /// Searches for objects within the specified area.
    /// </summary>
    /// <param name="_Root">Root node of the tree</param>
    /// <param name="_Range">The area to search for objects in</param>
    /// <param name="_Result">The Objects found within the specified area</param>
    public void StartSearch(Node _Root, Rect _Range, out LeafData[] _Result)
    {
        IEnumerable<Node> nodes;
        if (_Root.Entry is Branch)
        {
            nodes = ((Branch)_Root.Entry).Children;
        }
        else
        {
            if (!TreeScanner.Intersects(_Root.Entry.Rect, _Range))
            {
                _Result = new LeafData[0];
                return;
            }
            else
            {
                _Result = ((Leaf)_Root.Entry).Data;
                return;
            }
        }

        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        List<List<LeafData>> results = nodes.AsParallel().WithDegreeOfParallelism(parallelOptions.MaxDegreeOfParallelism)
            .Where(node => TreeScanner.Intersects(node.Entry.Rect, _Range))
            .Select(node => ScanRange(_Range, node))
            .ToList();

        IEnumerable<LeafData> combinedResults = results.SelectMany(list => list);

        _Result = combinedResults.ToArray();
    }

    /// <summary>
    /// Used for recursively scanning the entire tree.
    /// </summary>
    /// <param name="_Range">The area to search for objects in</param>
    /// <param name="_Start">The node to start searching from</param>
    /// <returns>The Objects found within the specified area</returns>
    private List<LeafData> ScanRange(Rect _Range, Node _Start)
    {
        List<LeafData> resultData = new List<LeafData>();

        if (_Start.Entry is Branch)
        {
            Branch branch = (Branch)_Start.Entry;
            foreach (Node child in branch.Children)
            {
                if (TreeScanner.Intersects(child.Entry.Rect, _Range))
                {
                    if (child.Entry is Branch)
                    {
                        LeafData[] result;
                        StartSearch(child, _Range, out result);
                        resultData.AddRange(result);
                    }
                    else if (child.Entry is Leaf)
                    {
                        resultData.AddRange(((Leaf)child.Entry).Data);
                    }
                }
            }
        }
        else if (_Start.Entry is Leaf)
        {
            resultData.AddRange(((Leaf)_Start.Entry).Data);
        }

        return resultData;
    }
}