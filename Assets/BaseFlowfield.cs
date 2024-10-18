using System.Collections.Generic;
using UnityEngine;

public class BaseFlowfield : MonoBehaviour
{
    private Dictionary<Vector2Int, Vector2[,]> m_Flowfields = new Dictionary<Vector2Int, Vector2[,]>();

    public Vector2[,] GetFlowfield(GridTile[,] _BoidGrid, Vector2Int _TargetPos)
    {
        if (m_Flowfields.ContainsKey(_TargetPos))
        {
            return m_Flowfields[_TargetPos];
        }

        float[,] costField;
        float[,] integrationField;
        Vector2[,] flowfield;

        costField = CalculateCostField(_BoidGrid, _TargetPos);
        integrationField = CalculateIntegrationField(costField, _TargetPos);
        flowfield = CalculateFlowField(integrationField, _TargetPos);

        m_Flowfields.Add(_TargetPos, flowfield);

        return flowfield;
    }

    private float[,] CalculateCostField(GridTile[,] _BoidGrid, Vector2Int _TargetPos)
    {
        float[,] costField = new float[_BoidGrid.GetLength(0), _BoidGrid.GetLength(1)];

        for (int x = 0; x < _BoidGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _BoidGrid.GetLength(1); y++)
            {
                switch (_BoidGrid[x, y].cellType)
                {
                    case CellType.Enemy:
                        costField[x, y] = 4 * _BoidGrid[x, y].numberOfEnemies;
                        break;

                    case CellType.Empty:
                        costField[x, y] = 1;
                        break;

                    case CellType.Ally:
                        costField[x, y] = 2 * _BoidGrid[x, y].numberOfAllies;
                        break;

                    case CellType.Combat:
                        costField[x, y] = 6 * (_BoidGrid[x, y].numberOfAllies + _BoidGrid[x, y].numberOfEnemies);
                        break;

                    case CellType.Obstacle:
                        costField[x, y] = 255;
                        break;

                    default:
                        costField[x, y] = 1;
                        break;
                }

                if (new Vector2Int(x, y) == _TargetPos)
                {
                    costField[x, y] = 0;
                }
            }
        }

        return costField;
    }

    private float[,] CalculateIntegrationField(float[,] _Costfield, Vector2Int _TargetPos)
    {
        List<Vector2Int> openCells = new List<Vector2Int>();
        float[,] integrationField = new float[_Costfield.GetLength(0), _Costfield.GetLength(1)];

        for (int x = 0; x < _Costfield.GetLength(0); x++)
        {
            for (int y = 0; y < _Costfield.GetLength(1); y++)
            {
                integrationField[x, y] = float.MaxValue;
            }
        }

        integrationField[_TargetPos.x, _TargetPos.y] = 0;
        openCells.Add(_TargetPos);

        while (openCells.Count > 0)
        {
            int x = openCells[0].x;
            int y = openCells[0].y;

            float integratedCost = 0;

            for (int i = -1; i < 2; i++)
            {
                for (int o = -1; o < 2; o++)
                {
                    if (IsInBounds(_Costfield.GetLength(0), _Costfield.GetLength(1), x + i, y + o) && _Costfield[x + i, y + o] != 255)
                    {
                        integratedCost = integrationField[x, y] + _Costfield[x + i, y + o];
                        if (integratedCost < integrationField[x + i, y + o])
                        {
                            integrationField[x + i, y + o] = integratedCost;
                            openCells.Add(new Vector2Int(x + i, y + o));
                        }
                    }
                }
            }

            openCells.RemoveAt(0);
        }

        return integrationField;
    }

    private bool IsInBounds(int _Width, int _Height, int _X, int _Y)
    {
        return _X >= 0 && _X < _Width && _Y >= 0 && _Y < _Height;
    }

    private Vector2[,] CalculateFlowField(float[,] _Integrationfield, Vector2 _TargetPos)
    {
        Vector2[,] flowfield = new Vector2[_Integrationfield.GetLength(0), _Integrationfield.GetLength(1)];

        for (int x = 0; x < _Integrationfield.GetLength(0); x++)
        {
            for (int y = 0; y < _Integrationfield.GetLength(1); y++)
            {
                if (_Integrationfield[x, y] == float.MaxValue)
                {
                    flowfield[x, y] = Vector2.zero;
                    continue;
                }
                Vector2 neighbour = FindCheapestNeighbour(_Integrationfield, x, y);
                flowfield[x, y] = (neighbour - new Vector2(x, y)).normalized;
            }
        }

        return flowfield;
    }

    private Vector2Int FindCheapestNeighbour(float[,] _Integrationfield, int _X, int _Y)
    {
        Vector2Int cheapestNeighbour = new Vector2Int(_X, _Y);

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (IsInBounds(_Integrationfield.GetLength(0), _Integrationfield.GetLength(1), _X + x, _Y + y) && _Integrationfield[_X + x, _Y + y] != float.MaxValue)
                {
                    if (_Integrationfield[cheapestNeighbour.x, cheapestNeighbour.y] > _Integrationfield[_X + x, _Y + y])
                    {
                        cheapestNeighbour = new Vector2Int(_X + x, _Y + y);
                    }
                }
            }
        }

        return cheapestNeighbour;
    }

    //private void OnDrawGizmos()
    //{
    //    for (int x = 0; x < 80; x++)
    //    {
    //        for (int y = 0; y < 80; y++)
    //        {
    //            Vector3 vec = new Vector3(this.transform.position.x + x * 4, 0, this.transform.position.z + y * 4);
    //            Vector3 vec2 = new Vector3(m_Flowfield[x, y].x * 2, 0, m_Flowfield[x, y].y * 2);

    //            Handles.Label(vec, m_IntegrationField[x, y].ToString());
    //            Gizmos.DrawLine(vec, vec + vec2);
    //            Gizmos.DrawWireCube(vec, new Vector3(4 - 0.1f, 1, 4 - 0.1f));
    //        }
    //    }
    //}
}