using System;
using System.Collections.Generic;
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
    public GridTile[,] BoidGrid { get => m_BoidGrid;}

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

        InitializeObstacles();
    }

    private void InitializeObstacles()
    {
        for (int x = 0; x < m_GridWidth; x++)
        {
            for (int y = 0; y < m_GridHeight; y++)
            {
                Vector3 pos = new Vector3(x * m_CellSize, 0, y * m_CellSize);
                bool blocked = Physics.CheckBox(pos, Vector3.one * m_CellSize * 0.5f, Quaternion.identity, LayerMask.GetMask("Obstacle"));
                if (blocked)
                {
                    m_BoidGrid[x, y].cellType = CellType.Obstacle;
                }
            }
        }
    }

    public GridTile QueryGridTile(int _PosX, int _PosY)
    {
        return m_BoidGrid[_PosX, _PosY];
    }

    public void UpdateGridTile(GridTile _NewTile, int _PosX, int _PosY)
    {
        m_BoidGrid[_PosX, _PosY] = _NewTile;
        UpdateCellType(_PosX, _PosY);
    }

    /// <summary>
    /// Updates the "Owner" of the current grid tile.
    /// </summary>
    /// <param name="_posX">X position in grid to update</param>
    /// <param name="_posY">Y position in grid to update</param>
    private void UpdateCellType(int _PosX, int _PosY)
    {
        if (!IsInBounds(_PosX, _PosY) || m_BoidGrid[_PosX, _PosY].cellType == CellType.Obstacle)
        {
            return;
        }

        GridTile targetTile = m_BoidGrid[_PosX, _PosY];
        int tileNumberOfAllies = targetTile.numberOfAllies;
        int tileNumberOfEnemies = targetTile.numberOfEnemies;

        if (tileNumberOfAllies <= 0 && tileNumberOfEnemies <= 0)
        {
            targetTile.cellType = CellType.Empty;
        }
        else if (tileNumberOfAllies > 0 && tileNumberOfEnemies <= 0)
        {
            targetTile.cellType = CellType.Ally;
        }
        else if (tileNumberOfAllies <= 0 && tileNumberOfEnemies > 0)
        {
            targetTile.cellType = CellType.Enemy;
        }
        else if (tileNumberOfAllies > 0 && tileNumberOfEnemies > 0)
        {
            targetTile.cellType = CellType.Combat;
        }

        m_BoidGrid[_PosX, _PosY] = targetTile;
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

    /// <summary>
    /// Checks if coordinate is within Bounds of Grid.
    /// </summary>
    /// <param name="_TilePos">Coordinate to check</param>
    /// <returns>true if in bounds</returns>
    public bool IsInBounds(int _X, int _Y)
    {
        return _X >= 0 && _X < m_BoidGrid.GetLength(0) && _Y >= 0 && _Y < m_BoidGrid.GetLength(1);
    }

    //DEBUG VISUALS; REMOVE BEFORE FINISHING SYSTEM
    //private void OnDrawGizmos()
    //{
    //    for (int x = 0; x < m_GridWidth; x++)
    //    {
    //        for (int y = 0; y < m_GridHeight; y++)
    //        {
    //            if (m_BoidGrid != null)
    //            {
    //                switch (m_BoidGrid[x, y].cellType)
    //                {
    //                    case CellType.Enemy:
    //                        Gizmos.color = Color.red;
    //                        break;

    //                    case CellType.Combat:
    //                        Gizmos.color = Color.yellow;
    //                        break;

    //                    case CellType.Ally:
    //                        Gizmos.color = Color.blue;
    //                        break;

    //                    case CellType.Empty:
    //                        Gizmos.color = Color.white;
    //                        break;

    //                    case CellType.Obstacle:
    //                        Gizmos.color = Color.black;
    //                        break;
    //                }

    //                Gizmos.DrawWireCube(new Vector3(this.transform.position.x + x * CellSize, 0, this.transform.position.z + y * CellSize), new Vector3(CellSize - 0.1f, 1, CellSize - 0.1f));
    //            }
    //        }
    //    }
    //    Gizmos.color = Color.white;
    //}
}

public class GridTile
{
    public int numberOfAllies;
    public int numberOfEnemies;
    public CellType cellType;
    public HashSet<BoidData> boids;
    public HashSet<Guid> visionList;
};