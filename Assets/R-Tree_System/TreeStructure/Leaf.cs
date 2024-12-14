using System;
using System.Numerics;

/// <summary>
/// A type of node entry. Contains the actual object data.
/// </summary>
public class Leaf : Spatial
{
    private Node m_EncapsulatingNode;
    private Rect m_Rect;
    private LeafData[] m_Data;
    private int m_NodeCapacity;
    private int m_MinNodeCapacity;

    public LeafData[] Data { get => m_Data; set => m_Data = value; }
    public override Node EncapsulatingNode { get => m_EncapsulatingNode; set => m_EncapsulatingNode = value; }
    public override Rect Rect { get => m_Rect; set => m_Rect = value; }
    public override int EntryCount { get => m_Data.Length; }
    public override int NodeCapacity { get => m_NodeCapacity; }
    public override int MinNodeCapacity { get => m_MinNodeCapacity; }

    public Leaf(Node _EncapsulatingNode, Rect _Rect, LeafData[] _Data, int _MaxCapacity, int _MinCapacity)
    {
        m_EncapsulatingNode = _EncapsulatingNode;
        m_Rect = _Rect;
        m_Data = _Data;
        m_NodeCapacity = _MaxCapacity;
        m_MinNodeCapacity = _MinCapacity;
    }

    /// <summary>
    /// Updates the rectangle of the node based on the contained children.
    /// </summary>
    public override void UpdateRect()
    {
        float x = this.Data[0].PosX;
        float y = this.Data[0].PosY;
        float z = this.Data[0].PosZ;

        Vector3 lowerLeft = new Vector3(x, y, z);

        Vector3 upperRight = new Vector3(x, y, z);

        for (int i = 0; i < this.Data.Length; i++)
        {
            lowerLeft.X = Math.Min(lowerLeft.X, this.Data[i].PosX);
            lowerLeft.Y = Math.Min(lowerLeft.Y, this.Data[i].PosY);
            lowerLeft.Z = Math.Min(lowerLeft.Z, this.Data[i].PosZ);

            upperRight.X = Math.Max(upperRight.X, this.Data[i].PosX);
            upperRight.Y = Math.Max(upperRight.Y, this.Data[i].PosY);
            upperRight.Z = Math.Max(upperRight.Z, this.Data[i].PosZ);
        }

        this.Rect.LowerLeft = lowerLeft;
        this.Rect.UpperRight = upperRight;
    }
}