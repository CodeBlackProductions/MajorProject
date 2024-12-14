using System.Collections.Generic;
using System.Numerics;

/// <summary>
/// R-Tree core class. Create an instance of this to create a new R-Tree.
/// </summary>
public class RTree
{
    private Node m_Root;
    private int m_NodeCapacity;
    private int m_MinNodeCapacity;

    private int m_IndexCounter = 0;
    private Dictionary<int, UnityEngine.GameObject> m_GameObjects;
    private Dictionary<UnityEngine.GameObject, int> m_Indices;
    private Dictionary<UnityEngine.GameObject, Vector3> m_TreePositions;

    public Node Root { get => m_Root; set => m_Root = value; }

    public RTree(int _NodeCapacity, int nodeMinCapacity)
    {
        m_NodeCapacity = _NodeCapacity;
        m_GameObjects = new Dictionary<int, UnityEngine.GameObject>();
        m_Indices = new Dictionary<UnityEngine.GameObject, int>();
        m_TreePositions = new Dictionary<UnityEngine.GameObject, Vector3>();
        m_MinNodeCapacity = nodeMinCapacity;

        m_Root = new Node
                     (0,
                     new Leaf(null, new Rect(new Vector3(0, 0, 0), new Vector3(1, 1, 1)), new LeafData[0], _NodeCapacity, nodeMinCapacity),
                     null, this);
        m_Root.Entry.EncapsulatingNode = m_Root;
    }

    #region External Access

    /// <summary>
    /// Inserts a unity gameobject into the R-Tree.
    /// </summary>
    /// <param name="_Obj">The unity gameobject to insert into R-Tree</param>
    public void Insert(UnityEngine.GameObject _Obj)
    {
        m_GameObjects.Add(m_IndexCounter, _Obj);
        m_Indices.Add(_Obj, m_IndexCounter);

        Vector3 pos = new Vector3(_Obj.transform.position.x, _Obj.transform.position.y, _Obj.transform.position.z);
        m_TreePositions.Add(_Obj, pos);

        if (m_Root.Entry == null)
        {
            Vector3 lowerLeft = new Vector3(pos.X - 10, pos.Y, pos.Z - 10);
            Vector3 upperRight = new Vector3(pos.X + 10, pos.Y, pos.Z + 10);

            Rect rect = new Rect(lowerLeft, upperRight);
            LeafData[] leafData = new LeafData[] { new LeafData(m_IndexCounter, pos.X, pos.Y, pos.Z) };
            m_Root.Entry = new Leaf(m_Root, rect, leafData, m_NodeCapacity, m_MinNodeCapacity);
            return;
        }

        Inserter.InsertData(m_Root, m_IndexCounter, pos.X, pos.Y, pos.Z);

        m_IndexCounter++;
    }

    /// <summary>
    /// Removes a unity gameobject from the R-Tree.
    /// </summary>
    /// <param name="_Obj">The unity gameobject to remove from R-Tree</param>
    public void Remove(UnityEngine.GameObject _Obj)
    {
        if (m_GameObjects.ContainsValue(_Obj))
        {
            int index = m_Indices[_Obj];

            Remover.RemoveEntry(m_Root, index, m_TreePositions[_Obj]);

            m_GameObjects.Remove(m_Indices[_Obj]);
            m_Indices.Remove(_Obj);
            m_TreePositions.Remove(_Obj);
        }
    }

    /// <summary>
    /// Removes an object from the R-Tree based on its index.
    /// </summary>
    /// <param name="_Idx">The index of the object you want to remove</param>
    public void Remove(int _Idx)
    {
        if (m_GameObjects[_Idx])
        {
            Remover.RemoveEntry(m_Root, _Idx, m_TreePositions[m_GameObjects[_Idx]]);

            m_TreePositions.Remove(m_GameObjects[_Idx]);
            m_Indices.Remove(m_GameObjects[_Idx]);
            m_GameObjects.Remove(_Idx);
        }
    }

    /// <summary>
    /// Updates the objects position inside the R-Tree.
    /// </summary>
    /// <param name="_Obj">The object you want to update.</param>
    public void UpdateObjectPosition(UnityEngine.GameObject _Obj)
    {
        Remove(_Obj);
        Insert(_Obj);
    }

    /// <summary>
    /// Finds all objects within a specified area.
    /// </summary>
    /// <param name="_Range">The area to search for objects</param>
    /// <returns>An array of unity gameobjects inside the specified area</returns>
    public UnityEngine.GameObject[] FindRange(Rect _Range)
    {
        LeafData[] searchData = TreeScanner.SearchLeafData(m_Root, _Range);

        if (searchData != null && searchData.Length > 0)
        {
            UnityEngine.GameObject[] result = new UnityEngine.GameObject[searchData.Length];
            for (int i = 0; i < searchData.Length; i++)
            {
                result[i] = m_GameObjects[searchData[i].ObjIDX];
            }
            return result;
        }
        return null;
    }

    #endregion External Access
}