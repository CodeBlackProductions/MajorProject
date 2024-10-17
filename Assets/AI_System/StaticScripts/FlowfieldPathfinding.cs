using UnityEngine;

public static class FlowfieldPathfinding
{
    public static float[,] CalculateCostField(GridTile[,] _BoidGrid, Vector2Int _TargetPos)
    {
        float[,] costfield = new float[_BoidGrid.GetLength(0), _BoidGrid.GetLength(1)];
        for (int x = 0; x < _BoidGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _BoidGrid.GetLength(1); y++)
            {
                switch (_BoidGrid[x, y].cellType)
                {
                    case CellType.Enemy:
                        costfield[x, y] = 4 * _BoidGrid[x, y].numberOfEnemies;
                        break;

                    case CellType.Empty:
                        costfield[x, y] = 1;
                        break;

                    case CellType.Ally:
                        costfield[x, y] = 2 * _BoidGrid[x, y].numberOfAllies;
                        break;

                    case CellType.Combat:
                        costfield[x, y] = 6 * (_BoidGrid[x, y].numberOfAllies + _BoidGrid[x, y].numberOfEnemies);
                        break;

                    case CellType.Obstacle:
                        costfield[x, y] = 255;
                        break;

                    default:
                        costfield[x, y] = 1;
                        break;
                }
            }
        }

        return null;
    }

    private static float CalculateGridDistance(Vector2Int _Pos, Vector2Int _TargetPos)
    {
        return Mathf.Abs(_Pos.x - _TargetPos.x) + Mathf.Abs(_Pos.y - _TargetPos.y);
    }

    public static void CalculateIntegrationField(GridTile[,] _BoidGrid)
    {
    }

    public static void CalculateFlowField(GridTile[,] _BoidGrid)
    {
    }
}