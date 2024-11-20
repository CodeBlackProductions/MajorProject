using System;
using System.Collections.Generic;
using UnityEngine;

public class GridPosManager : MonoBehaviour
{
    public static GridPosManager Instance;

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
    /// Updates boid lists inside the grid.
    /// </summary>
    /// <param name="_BoidData">Boid to update.</param>
    /// <param name="_OldGridPos">Previous boid position</param>
    /// <param name="_GridPos">New boid position</param>
    public void UpdateBoidPosition(BoidData _BoidData, Vector2Int _OldGridPos, Vector2Int _GridPos)
    {
        RemoveBoid(_OldGridPos.x, _OldGridPos.y, _BoidData);
        AddBoid(_GridPos.x, _GridPos.y, _BoidData);
    }

    /// <summary>
    /// Removes a boid from the hashset of a grid tile
    /// </summary>
    /// <param name="_PosX">Grid X to remove from</param>
    /// <param name="_PosY">Grid Y to remove from</param>
    /// <param name="_boidID">ID of boid to be removed</param>
    private void RemoveBoid(int _PosX, int _PosY, BoidData _Data)
    {
        GridTile tile = m_DataManager.QueryGridTile(_PosX, _PosY);

        if (tile.boids == null || tile.boids.Count == 0)
        {
            return;
        }

        tile.boids.RemoveWhere(boid => boid.boidGuid == _Data.boidGuid);

        if (_Data.boidTeam == Team.Ally)
        {
            tile.numberOfAllies--;
        }
        else
        {
            tile.numberOfEnemies--;
        }

        if (tile.visionList != null)
        {
            foreach (Guid guid in tile.visionList)
            {
                var onRemoveBoid = GridBoidManager.Instance.OnRemoveBoidCallbacks[guid];

                if (guid != _Data.boidGuid)
                {
                    onRemoveBoid?.Invoke(_Data.boidGuid, _Data.boidTeam);
                }
            }
        }

        m_DataManager.UpdateGridTile(tile, _PosX, _PosY);
    }

    /// <summary>
    /// Adds a boid to the hashset of a grid tile.
    /// </summary>
    /// <param name="_PosX">Grid X to add to</param>
    /// <param name="_PosY">Grid Y to add to</param>
    /// <param name="_Data">Boid data to add</param>
    private void AddBoid(int _PosX, int _PosY, BoidData _Data)
    {
        GridTile tile = m_DataManager.QueryGridTile(_PosX, _PosY);

        if (tile.boids == null)
        {
            tile.boids = new HashSet<BoidData>();
        }

        tile.boids.Add(_Data);

        if (_Data.boidTeam == Team.Ally)
        {
            tile.numberOfAllies++;
        }
        else
        {
            tile.numberOfEnemies++;
        }

        if (tile.visionList != null)
        {
            foreach (Guid guid in tile.visionList)
            {
                var onAddBoid = GridBoidManager.Instance.OnAddBoidCallbacks[guid];
                if (guid != _Data.boidGuid)
                {
                    onAddBoid?.Invoke(_Data.boidGuid, _Data.boidTeam);
                }
            }
        }

        m_DataManager.UpdateGridTile(tile, _PosX, _PosY);
    }
}