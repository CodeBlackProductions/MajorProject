using RBush;
using UnityEngine;

public enum CellType
{
    Enemy = -1, Empty, Ally, Combat, Obstacle
}

public class GridDataManager : MonoBehaviour
{
    public static GridDataManager Instance;

    [SerializeField][Delayed][Tooltip("Has to be multiple of 10")] private int m_GridHeight = 10;
    [SerializeField][Delayed][Tooltip("Has to be multiple of 10")] private int m_GridWidth = 10;
    [SerializeField] private int m_CellSize = 4;

    private GridTile[,] m_BoidGrid;
    public int CellSize { get => m_CellSize; }
    public GridTile[,] BoidGrid { get => m_BoidGrid; }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ValidateGridSize();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        m_BoidGrid = new GridTile[m_GridWidth, m_GridHeight];

        for (int x = 0; x < m_GridWidth; x++)
        {
            for (int y = 0; y < m_GridHeight; y++)
            {
                m_BoidGrid[x, y] = new GridTile();
            }
        }
    }

    private void Start()
    {
        InitializeObstacles();
    }

    private void InitializeObstacles()
    {
        GameObject obstacleParent = new GameObject("ObstacleParent");
        for (int x = 0; x < m_GridWidth; x++)
        {
            for (int y = 0; y < m_GridHeight; y++)
            {
                Vector3 pos = new Vector3(x * m_CellSize, 0, y * m_CellSize);
                bool blocked = Physics.CheckBox(pos, Vector3.one * m_CellSize * 0.5f, Quaternion.identity, LayerMask.GetMask("Obstacle"));
                if (blocked)
                {
                    m_BoidGrid[x, y].cellType = CellType.Obstacle;
                    GameObject tempObj = new GameObject("Obstacle_" + x + "_" + y);
                    tempObj.transform.localScale = new Vector3(m_CellSize, m_CellSize, m_CellSize);
                    tempObj.transform.position = pos;
                    tempObj.layer = LayerMask.NameToLayer("Obstacle");
                    RTree_BoidManager.Instance?.RegisterObject(tempObj, CreateTreeEntry(tempObj));
                    tempObj.transform.parent = obstacleParent.transform;
                }
            }
        }
    }

    public GridTile QueryGridTile(int _PosX, int _PosY)
    {
        if (IsInBounds(_PosX, _PosY))
        {
            return m_BoidGrid[_PosX, _PosY];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets gridsize to the next smaller multiple of 10, if not already a multiple.
    /// </summary>
    private void ValidateGridSize()
    {
        while (m_GridHeight % 10 != 0)
        {
            m_GridHeight--;
        };
        while (m_GridWidth % 10 != 0)
        {
            m_GridWidth--;
        };
    }

    private RTree_Object CreateTreeEntry(GameObject _Obj)
    {
        RTree_Object obj = new RTree_Object(new Envelope(_Obj.transform.position.x, _Obj.transform.position.z, _Obj.transform.position.x, _Obj.transform.position.z));

        obj.Object = _Obj;

        return obj;
    }

    /// <summary>
    /// Checks if coordinate is within Bounds of Grid.
    /// </summary>
    /// <param name="_TilePos">Coordinate to check</param>
    /// <returns>true if in bounds</returns>
    public bool IsInBounds(int _X, int _Y)
    {
        return _X >= 0 && _X < m_BoidGrid.GetLength(0) && _Y >= 0 && _Y < m_BoidGrid.GetLength(1);
    }

    public bool HasLoS(Vector2Int _From, Vector2Int _To)
    {
        if (Mathf.Abs(_To.x - _From.x) > Mathf.Abs(_To.y - _From.y))
        {
            return CheckLoSHorizontal(_From, _To);
        }
        else
        {
            return CheckLoSVertical(_From, _To);
        }
    }

    private bool CheckLoSHorizontal(Vector2Int _From, Vector2Int _To)
    {
        int x0;
        int x1;
        int y0;
        int y1;

        if (_From.x > _To.x)
        {
            x0 = _To.x;
            x1 = _From.x;
            y0 = _To.y;
            y1 = _From.y;
        }
        else
        {
            x0 = _From.x;
            x1 = _To.x;
            y0 = _From.y;
            y1 = _To.y;
        }

        int dX = x1 - x0;
        int dY = y1 - y0;

        int dir = dY < 0 ? -1 : 1;
        dY *= dir;

        if (dX == 0)
        {
            return true;
        }

        int y = y0;
        int step = 2 * dY - dX;

        for (int i = 0; i < dX + 1; i++)
        {
            GridTile tempTile = QueryGridTile(x0 + i, y);
            if (tempTile == null || tempTile.cellType == CellType.Obstacle)
            {
                return false;
            }

            if (step >= 0)
            {
                y += dir;
                step = step - 2 * dX;
                step = step + 2 * dY;
            }
        }

        return true;
    }

    private bool CheckLoSVertical(Vector2Int _From, Vector2Int _To)
    {
        int x0;
        int x1;
        int y0;
        int y1;

        if (_From.y > _To.y)
        {
            x0 = _To.x;
            x1 = _From.x;
            y0 = _To.y;
            y1 = _From.y;
        }
        else
        {
            x0 = _From.x;
            x1 = _To.x;
            y0 = _From.y;
            y1 = _To.y;
        }

        int dX = x1 - x0;
        int dY = y1 - y0;

        int dir = dX < 0 ? -1 : 1;
        dX *= dir;

        if (dY == 0)
        {
            return true;
        }

        int x = x0;
        int step = 2 * dX - dY;

        for (int i = 0; i < dY + 1; i++)
        {
            GridTile tempTile = QueryGridTile(x, y0 + i);
            if (tempTile == null || tempTile.cellType == CellType.Obstacle)
            {
                return false;
            }

            if (step >= 0)
            {
                x += dir;
                step = step - 2 * dY;
                step = step + 2 * dX;
            }
        }

        return true;
    }
}

public class GridTile
{
    public int numberOfAllies;
    public int numberOfEnemies;
    public CellType cellType;
};