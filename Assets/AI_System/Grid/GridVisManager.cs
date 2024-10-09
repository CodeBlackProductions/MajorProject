using System;
using System.Collections.Generic;
using UnityEngine;

public class GridVisManager : MonoBehaviour
{
    public static GridVisManager Instance;

    private GridDataManager m_DataManager;

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
    /// <param name="_Data">Boid data</param>
    public void UpdateVision(BoidData _Data, Vector2Int _OldGridPos, Vector2Int _GridPos)
    {
        int visionRange = CalculateGridVision(_Data.boidVis);
        Guid boidGuid = _Data.boidGuid;

        HashSet<Vector2Int> oldVision = GetTilesInVision(_OldGridPos, visionRange);
        HashSet<Vector2Int> newVision = GetTilesInVision(_GridPos, visionRange);

        HashSet<Vector2Int> visionLost = new HashSet<Vector2Int>(oldVision);
        visionLost.ExceptWith(newVision);

        HashSet<Vector2Int> visionGained = new HashSet<Vector2Int>(newVision);
        visionGained.ExceptWith(oldVision);

        // Process vision lost
        foreach (Vector2Int vec in visionLost)
        {
            RemoveVision(vec, boidGuid);
        }

        // Process vision gained
        foreach (Vector2Int vec in visionGained)
        {
            AddVision(vec, boidGuid);
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

        int startX = _BoidPos.x - _VisionRange;
        int endX = _BoidPos.x + _VisionRange;
        int startY = _BoidPos.y - _VisionRange;
        int endY = _BoidPos.y + _VisionRange;

        for (int x = startX; x <= endX; x++)
        {
            for (int y = startY; y <= endY; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);
                if (CheckTileVisibility(_BoidPos, tilePos, _VisionRange) && m_DataManager.IsInBounds(tilePos))
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
        return Mathf.RoundToInt(_Vis / m_DataManager.CellSize); ;
    }
}