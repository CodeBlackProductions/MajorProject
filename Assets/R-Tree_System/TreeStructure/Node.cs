using System;

/// <summary>
/// Structural class. Contains either branches or leafs. Each node has a unique ID for identification.
/// </summary>
public class Node
{
    private Spatial m_Entry;
    private int m_Level;
    private Guid m_ID;
    private Node m_Parent;
    private RTree m_ParentTree;

    public Spatial Entry { get => m_Entry; set => m_Entry = value; }
    public int Level { get => m_Level; set => m_Level = value; }
    public Guid ID { get => m_ID; }

    public Node Parent
    {
        get => m_Parent; set
        {
            if (value != null && m_ID != value.ID)
            {
                m_Parent = value;
            }
            else
            {
                m_Parent = null;
            }
        }
    }

    public RTree ParentTree { get => m_ParentTree; set => m_ParentTree = value; }

    public Node(int _Level, Spatial _Entry, Node _Parent, RTree _ParentTree)
    {
        m_Level = _Level;
        m_Entry = _Entry;
        Parent = _Parent;
        m_ParentTree = _ParentTree;
        m_ID = Guid.NewGuid();
    }

    /// <summary>
    /// Checks if the node contains too many entries.
    /// </summary>
    /// <returns>is true, if node has too many entries.</returns>
    public bool IsOverflowing()
    {
        if (m_Entry.EntryCount > m_Entry.NodeCapacity)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the node contains not enough entries to be efficient.
    /// </summary>
    /// <returns>Returns true if there are not enough entries in the node.</returns>
    public bool IsUnderflowing()
    {
        if (m_Entry.EntryCount < m_Entry.MinNodeCapacity)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}