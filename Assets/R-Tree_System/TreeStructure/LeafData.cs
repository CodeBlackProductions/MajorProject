/// <summary>
/// Contains the position and index of one single object inside the tree.
/// </summary>
public struct LeafData
{
    private readonly int objIDX;
    private readonly float posX;
    private readonly float posY;
    private readonly float posZ;

    public int ObjIDX { get => objIDX; }
    public float PosX { get => posX; }
    public float PosY { get => posY; }
    public float PosZ { get => posZ; }

    public LeafData(int _ObjIDX, float _PosX, float _PosY, float _PosZ)
    {
        objIDX = _ObjIDX;
        posX = _PosX;
        posY = _PosY;
        posZ = _PosZ;
    }
}