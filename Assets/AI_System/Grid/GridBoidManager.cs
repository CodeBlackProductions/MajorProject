using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GridPosManager))]
[RequireComponent(typeof(GridVisManager))]
[RequireComponent(typeof(GridDataManager))]
public class GridBoidManager : MonoBehaviour
{
    public static GridBoidManager Instance;

    private GridDataManager m_DataManager;
    private GridPosManager m_GridPosManager;
    private GridVisManager m_GridVisManager;

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
        if (GridPosManager.Instance != null)
        {
            m_GridPosManager = GridPosManager.Instance;
        }
        if (GridVisManager.Instance != null)
        {
            m_GridVisManager = GridVisManager.Instance;
        }

        if (EventManager.Instance)
        {
            EventManager.Instance.SendBoidDataToGrid += OnReceiveBoidPos;
        }
    }

    /// <summary>
    /// Updates the grid after receiving new data.
    /// </summary>
    /// <param name="_Data">Data received</param>
    /// <param name="_oldPos">Previous grid position of the boid</param>
    /// <param name="_newPos">New grid position of the boid</param>
    private void UpdateGrid(BoidData _Data, Vector2Int _OldPos, Vector2Int _Pos)
    {
        m_GridPosManager.UpdateBoidPosition(_Data, _OldPos, _Pos);
        m_GridVisManager.UpdateVisionEdges(_OldPos, _Pos, (int)(_Data.boidVis / m_DataManager.CellSize), _Data.boidGuid);
    }

    /// <summary>
    /// Receives data from BoidPositionHandlers.
    /// </summary>
    /// <param name="_Data">Data of the boid received</param>
    /// <param name="_oldPos">Previous grid position of the boid</param>
    /// <param name="_newPos">New grid position of the boid</param>
    public void OnReceiveBoidPos(BoidData _Data)
    {
        Vector2Int oldGridPos;
        Vector2Int gridPos;

        if (_Data.oldPos == Vector3.zero)
        {
            oldGridPos = Vector2Int.zero;
        }
        else
        {
            oldGridPos = CalculateGridPos(_Data.oldPos);
        }

        if (_Data.boidPos == Vector3.zero)
        {
            gridPos = Vector2Int.zero;
        }
        else
        {
            gridPos = CalculateGridPos(_Data.boidPos);
        }

        UpdateGrid(_Data, oldGridPos, gridPos);
    }

    /// <summary>
    /// Converts world coordiantes to grid coordinates.
    /// </summary>
    /// <param name="_Pos">Coordinates to convert</param>
    /// <returns>Grid conversion of _Pos</returns>
    private Vector2Int CalculateGridPos(Vector3 _Pos)
    {
        Vector2Int gridCoord = new Vector2Int();
        gridCoord.x = Mathf.RoundToInt(_Pos.x / m_DataManager.CellSize);
        gridCoord.y = Mathf.RoundToInt(_Pos.z / m_DataManager.CellSize);

        return gridCoord;
    }
}