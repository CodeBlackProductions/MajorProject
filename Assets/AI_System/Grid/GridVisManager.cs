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
        if (_GridPos == Vector2Int.zero)
        {
            for (int x = -_VisionRange; x < _VisionRange; x++)
            {
                for (int y = -_VisionRange; y < _VisionRange; y++)
                {
                    if (m_DataManager.IsInBounds(_OldGridPos.x + x, _OldGridPos.y + y))
                    {
                        RemoveVision(new Vector2Int(_OldGridPos.x + x, _OldGridPos.y + y), _BoidGuid);
                    }
                }
            }
        }
        else if (_GridPos != _OldGridPos)
        {
            m_VisionGained.Clear();
            m_VisionLost.Clear();

            for (int x = -_VisionRange; x < _VisionRange; x++)
            {
                for (int y = -_VisionRange; y < _VisionRange; y++)
                {
                    if (m_DataManager.IsInBounds(_GridPos.x + x, _GridPos.y + y))
                    {
                        m_VisionGained.Add(new Vector2Int(_GridPos.x + x, _GridPos.y + y));
                    }
                    if (m_DataManager.IsInBounds(_OldGridPos.x + x, _OldGridPos.y + y))
                    {
                        m_VisionLost.Add(new Vector2Int(_OldGridPos.x + x, _OldGridPos.y + y));
                    }
                }
            }

            Vector2Int[] temp = new Vector2Int[m_VisionGained.Count];
            m_VisionGained.CopyTo(temp);
            m_VisionGained.ExceptWith(m_VisionLost);
            m_VisionLost.ExceptWith(temp);

            foreach (Vector2Int vec in m_VisionGained)
            {
                if (m_DataManager.HasLoS(_GridPos, vec))
                {
                    AddVision(vec, _BoidGuid, true);
                }
                else
                {
                    AddVision(vec, _BoidGuid, false);
                }
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
            EventManager.Instance.OnRemoveBoidVisionFromGridCallbacks[_BoidGuid]?.Invoke(m_TempVec3);
        }

        m_DataManager.UpdateGridTile(tile, _GridPos.x, _GridPos.y);
    }

    /// <summary>
    /// Add an entry to visibility hashset of a tile.
    /// </summary>
    /// <param name="_GridPos">Tile position</param>
    /// <param name="_boidIndex">Index of the boid to add</param>
    private void AddVision(Vector2Int _GridPos, Guid _BoidGuid, bool _HasLoS)
    {
        GridTile tile = m_DataManager.QueryGridTile(_GridPos.x, _GridPos.y);

        if (tile.cellType == CellType.Obstacle)
        {
            m_TempVec3.x = _GridPos.x * m_DataManager.CellSize;
            m_TempVec3.y = 0;
            m_TempVec3.z = _GridPos.y * m_DataManager.CellSize;
            EventManager.Instance.OnAddBoidVisionToGridCallbacks[_BoidGuid]?.Invoke(m_TempVec3);
        }
        if (_HasLoS)
        {
            if (tile.visionList == null)
            {
                tile.visionList = new HashSet<Guid>();
            }

            tile.visionList.Add(_BoidGuid);

            m_DataManager.UpdateGridTile(tile, _GridPos.x, _GridPos.y);
        }
    }
}