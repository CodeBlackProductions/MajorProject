using System.Collections.Generic;
using UnityEngine;

public static class FlowfieldPathfinding
{
    public static Vector2[,] CreateNewFlowfield(GridTile[,] _BoidGrid, Vector2Int _TargetPos)
    {
        float[,] temp;

        temp = CalculateCostField(_BoidGrid);
        temp = CalculateIntegrationField(temp, _TargetPos);
        return CalculateFlowField(temp, _TargetPos);
    }

    public static float[,] CalculateCostField(GridTile[,] _BoidGrid)
    {
        float[,] costField = new float[_BoidGrid.GetLength(0), _BoidGrid.GetLength(1)];

        for (int x = 0; x < _BoidGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _BoidGrid.GetLength(1); y++)
            {
                if (_BoidGrid[x, y].cellType == CellType.Obstacle)
                {
                    costField[x, y] = 255;
                }
                else
                {
                    costField[x, y] = 1;
                }
            }
        }

        return costField;
    }

    public static float[,] CalculateIntegrationField(float[,] _Costfield, Vector2Int _TargetPos)
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

    private static bool IsInBounds(int _Width, int _Height, int _X, int _Y)
    {
        return _X >= 0 && _X < _Width && _Y >= 0 && _Y < _Height;
    }

    public static Vector2[,] CalculateFlowField(float[,] _Integrationfield, Vector2 _TargetPos)
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

    private static Vector2Int FindCheapestNeighbour(float[,] _Integrationfield, int _X, int _Y)
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
}