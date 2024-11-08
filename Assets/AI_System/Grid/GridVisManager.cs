using System;
using System.Collections.Generic;
using UnityEngine;

public class GridVisManager : MonoBehaviour
{
    public static GridVisManager Instance;

    private GridDataManager m_DataManager;

    private HashSet<Vector2Int> m_VisionGained = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> m_VisionLost = new HashSet<Vector2Int>();

    private Vector3 m_TempVec = new Vector3();

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
    }

    private void Start()
    {
        if (GridDataManager.Instance != null)
        {
            m_DataManager = GridDataManager.Instance;
        }
    }

    /// <summary>
    /// Updates visibility lists of the grid.
    /// </summary>
    /// <param name="_OldGridPos">Previous boid position</param>
    /// <param name="_GridPos">New boid position</param>
    /// <param name="_VisionRange">Boid vision</param>
    /// <param name="_BoidGuid">Boid ID</param>
    public void UpdateVisionEdges(Vector2Int _OldGridPos, Vector2Int _GridPos, int _VisionRange, Guid _BoidGuid)
    {
        Vector2Int direction = _GridPos - _OldGridPos;

        if (direction != Vector2Int.zero)
        {
            m_VisionGained.Clear();
            m_VisionLost.Clear();

            // Moving Right or Left
            if (direction.x != 0)
            {
                int edgeX = _GridPos.x + direction.x * _VisionRange;
                for (int y = -_VisionRange; y <= _VisionRange; y++)
                {
                    Vector2Int addTile = new Vector2Int(edgeX, _GridPos.y + y);
                    Vector2Int removeTile = new Vector2Int(_OldGridPos.x - direction.x * _VisionRange, _OldGridPos.y + y);

                    if (m_DataManager.IsInBounds(addTile)) m_VisionGained.Add(addTile);
                    if (m_DataManager.IsInBounds(removeTile)) m_VisionLost.Add(removeTile);
                }
            }

            // Moving Up or Down
            if (direction.y != 0)
            {
                int edgeY = _GridPos.y + direction.y * _VisionRange;
                for (int x = -_VisionRange; x <= _VisionRange; x++)
                {
                    Vector2Int addTile = new Vector2Int(_GridPos.x + x, edgeY);
                    Vector2Int removeTile = new Vector2Int(_OldGridPos.x + x, _OldGridPos.y - direction.y * _VisionRange);

                    if (m_DataManager.IsInBounds(addTile)) m_VisionGained.Add(addTile);
                    if (m_DataManager.IsInBounds(removeTile)) m_VisionLost.Add(removeTile);
                }
            }

            // Process vision changes
            foreach (Vector2Int vec in m_VisionGained)
            {
                AddVision(vec, _BoidGuid);
            }

            foreach (Vector2Int vec in m_VisionLost)
            {
                RemoveVision(vec, _BoidGuid);
            }
        }
    }

    /// <summary>
    /// Removes an entry from visibility hashset of a tile.
    /// </summary>
    /// <param name="_GridPos">Tile position</param>
    /// <param name="_boidIndex">Index of the boid to remove</param>
    private void RemoveVision(Vector2Int _GridPos, Guid _BoidGuid)
    {
        GridTile tile = m_DataManager.QueryGridTile(_GridPos.x, _GridPos.y);

        if (tile.visionList == null)
        {
            return;
        }

        if (tile.visionList.Contains(_BoidGuid))
        {
            tile.visionList.Remove(_BoidGuid);
        }

        if (tile.cellType == CellType.Obstacle)
        {
            m_TempVec.x = _GridPos.x * m_DataManager.CellSize;
            m_TempVec.y = 0;
            m_TempVec.z = _GridPos.y * m_DataManager.CellSize;
            GridBoidManager.Instance.OnRemoveVision?.Invoke(_BoidGuid, m_TempVec);
        }

        m_DataManager.UpdateGridTile(tile, _GridPos.x, _GridPos.y);
    }

    /// <summary>
    /// Add an entry to visibility hashset of a tile.
    /// </summary>
    /// <param name="_GridPos">Tile position</param>
    /// <param name="_boidIndex">Index of the boid to add</param>
    private void AddVision(Vector2Int _GridPos, Guid _BoidGuid)
    {
        GridTile tile = m_DataManager.QueryGridTile(_GridPos.x, _GridPos.y);

        if (tile.visionList == null)
        {
            tile.visionList = new HashSet<Guid>();
        }

        tile.visionList.Add(_BoidGuid);

        if (tile.cellType == CellType.Obstacle)
        {
            m_TempVec.x = _GridPos.x * m_DataManager.CellSize;
            m_TempVec.y = 0;
            m_TempVec.z = _GridPos.y * m_DataManager.CellSize;
            GridBoidManager.Instance.OnAddVision?.Invoke(_BoidGuid, m_TempVec);
        }

        m_DataManager.UpdateGridTile(tile, _GridPos.x, _GridPos.y);
    }

    /// <summary>
    /// Gets tiles within boid vision range.
    /// </summary>
    /// <param name="_BoidPos">Position of the boid</param>
    /// <param name="_VisionRange">Vision range of the boid transformed to gridspace</param>
    /// <returns>Tiles in vision of boid</returns>
    private HashSet<Vector2Int> GetTilesInVision(Vector2Int _BoidPos, int _VisionRange)
    {
        HashSet<Vector2Int> tilesInRange = new HashSet<Vector2Int>();

        int sqrVisionRange = _VisionRange * _VisionRange;

        for (int x = -_VisionRange; x <= _VisionRange; x++)
        {
            for (int y = -_VisionRange; y <= _VisionRange; y++)
            {
                Vector2Int tilePos = new Vector2Int(_BoidPos.x + x, _BoidPos.y + y);

                int sqrDistance = x * x + y * y;

                if (sqrDistance <= sqrVisionRange && m_DataManager.IsInBounds(tilePos))
                {
                    tilesInRange.Add(tilePos);
                }
            }
        }

        return tilesInRange;
    }

    /// <summary>
    /// Checks if a singular tile is actually visible to the boid
    /// </summary>
    /// <param name="_BoidPos">Position of the boid</param>
    /// <param name="_TilePos">Position of the tile</param>
    /// <param name="_VisionRange">Vision range of the boid in gridspace</param>
    /// <returns>true if at least 50% visible to boid, false if not</returns>
    private bool CheckTileVisibility(Vector2Int _BoidPos, Vector2Int _TilePos, int _VisionRange)
    {
        float sqrDistance = (_BoidPos - _TilePos).sqrMagnitude;
        float sqrVision = _VisionRange * _VisionRange;

        return sqrDistance <= sqrVision;
    }

    /// <summary>
    /// Converts worldsize visionrange to gridsize.
    /// </summary>
    /// <param name="_Vis">Visionrange to convert</param>
    /// <returns>grid scale version of _Vis</returns>
    private int CalculateGridVision(float _Vis)
    {
        return Mathf.RoundToInt(_Vis / m_DataManager.CellSize);
    }
}