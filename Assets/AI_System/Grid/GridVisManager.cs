using System;
using System.Collections.Generic;
using UnityEngine;

public class GridVisManager : MonoBehaviour
{
    public static GridVisManager Instance;

    private GridDataManager m_DataManager;

    private HashSet<Vector2Int> m_VisionGained = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> m_VisionLost = new HashSet<Vector2Int>();

    private Vector3 m_TempVec3 = new Vector3();

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
            direction.x = (int)(direction.x / direction.magnitude);
            direction.y = (int)(direction.y / direction.magnitude);

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

                    if (m_DataManager.IsInBounds(addTile.x, addTile.y))
                    {
                        m_VisionGained.Add(addTile);
                    }
                    if (m_DataManager.IsInBounds(removeTile.x, removeTile.y))
                    {
                        m_VisionLost.Add(removeTile);
                    }
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

                    if (m_DataManager.IsInBounds(addTile.x, addTile.y))
                    {
                        m_VisionGained.Add(addTile);
                    }
                    if (m_DataManager.IsInBounds(removeTile.x, removeTile.y))
                    {
                        m_VisionLost.Add(removeTile);
                    }
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
            m_TempVec3.x = _GridPos.x * m_DataManager.CellSize;
            m_TempVec3.y = 0;
            m_TempVec3.z = _GridPos.y * m_DataManager.CellSize;
            GridBoidManager.Instance.OnRemoveVision[_BoidGuid]?.Invoke(m_TempVec3);
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
            m_TempVec3.x = _GridPos.x * m_DataManager.CellSize;
            m_TempVec3.y = 0;
            m_TempVec3.z = _GridPos.y * m_DataManager.CellSize;
            GridBoidManager.Instance.OnAddVision[_BoidGuid]?.Invoke(m_TempVec3);
        }

        m_DataManager.UpdateGridTile(tile, _GridPos.x, _GridPos.y);
    }
}