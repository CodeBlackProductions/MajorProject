using System;
using System.Numerics;

/// <summary>
/// A type of node entry. Contains other nodes as children.
/// </summary>
public class Branch : Spatial
{
    private Node m_EncapsulatingNode;
    private Rect m_Rect;
    private Node[] m_Children;
    private int m_NodeCapacity;
    private int m_MinNodeCapacity;

    public Node[] Children { get => m_Children; set => m_Children = value; }
    public override Node EncapsulatingNode { get => m_EncapsulatingNode; set => m_EncapsulatingNode = value; }
    public override Rect Rect { get => m_Rect; set => m_Rect = value; }
    public override int EntryCount { get => m_Children.Length; }
    public override int NodeCapacity { get => m_NodeCapacity; }
    public override int MinNodeCapacity { get => m_MinNodeCapacity; }

    public Branch(Node _EncapsulatingNode, Rect _Rect, Node[] _Children, int _MaxCapacity, int _MinCapacity)
    {
        m_EncapsulatingNode = _EncapsulatingNode;
        m_Rect = _Rect;
        m_Children = _Children;
        m_NodeCapacity = _MaxCapacity;
        m_MinNodeCapacity = _MinCapacity;
    }

    /// <summary>
    /// Updates the rectangle of the node based on the contained children.
    /// </summary>
    public override void UpdateRect()
    {
        if (this.Children.Length == 0 || this.Children[0] == null)
        {
            Vector3 val = this.Rect.GetCenter();
            this.Rect = new Rect(val, val);
            return;
        }

        Vector3 lowerLeft = this.Children[0].Entry.Rect.LowerLeft;
        Vector3 upperRight = this.Children[0].Entry.Rect.UpperRight;

        for (int i = 0; i < this.Children.Length; i++)
        {
            lowerLeft.X = Math.Min(lowerLeft.X, this.Children[i].Entry.Rect.LowerLeft.X);
            lowerLeft.Y = Math.Min(lowerLeft.Y, this.Children[i].Entry.Rect.LowerLeft.Y);
            lowerLeft.Z = Math.Min(lowerLeft.Z, this.Children[i].Entry.Rect.LowerLeft.Z);

            upperRight.X = Math.Max(upperRight.X, this.Children[i].Entry.Rect.UpperRight.X);
            upperRight.Y = Math.Max(upperRight.Y, this.Children[i].Entry.Rect.UpperRight.Y);
            upperRight.Z = Math.Max(upperRight.Z, this.Children[i].Entry.Rect.UpperRight.Z);
        }

        this.Rect.LowerLeft = lowerLeft;
        this.Rect.UpperRight = upperRight;

        if (m_EncapsulatingNode != null && m_EncapsulatingNode.Parent != null)
        {
            m_EncapsulatingNode.Parent.Entry.UpdateRect();
        }
    }
}