using System.Collections.Generic;
using UnityEngine;

public class BaseFlowfield : MonoBehaviour
{
    private float[,] m_CostField = new float[80, 80];
    private float[,] m_IntegrationField = new float[80, 80];
    private Vector2[,] m_Flowfield = new Vector2[80, 80];

    [SerializeField] private Transform m_TargetObject;

    private Vector2Int m_Target;

    private float timer = 1;

    private void Awake()
    {
        m_Target = new Vector2Int((int)m_TargetObject.position.x / 4, (int)m_TargetObject.position.z / 4);
    }

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            CalculateCostField(GridDataManager.Instance.BoidGrid, m_Target);
            CalculateIntegrationField(m_Target);
            CalculateFlowField(m_Target);
            timer = 5;
        }
    }

    public void CalculateCostField(GridTile[,] _BoidGrid, Vector2Int _TargetPos)
    {
        for (int x = 0; x < 80; x++)
        {
            for (int y = 0; y < 80; y++)
            {
                switch (_BoidGrid[x, y].cellType)
                {
                    case CellType.Enemy:
                        m_CostField[x, y] = 4 * _BoidGrid[x, y].numberOfEnemies;
                        break;

                    case CellType.Empty:
                        m_CostField[x, y] = 1;
                        break;

                    case CellType.Ally:
                        m_CostField[x, y] = 2 * _BoidGrid[x, y].numberOfAllies;
                        break;

                    case CellType.Combat:
                        m_CostField[x, y] = 6 * (_BoidGrid[x, y].numberOfAllies + _BoidGrid[x, y].numberOfEnemies);
                        break;

                    case CellType.Obstacle:
                        m_CostField[x, y] = 255;
                        break;

                    default:
                        m_CostField[x, y] = 1;
                        break;
                }

                if (new Vector2Int(x, y) == _TargetPos)
                {
                    m_CostField[x, y] = 0;
                }
            }
        }
    }

    public void CalculateIntegrationField(Vector2Int _TargetPos)
    {
        List<Vector2Int> openCells = new List<Vector2Int>();

        for (int x = 0; x < 80; x++)
        {
            for (int y = 0; y < 80; y++)
            {
                m_IntegrationField[x, y] = float.MaxValue;
            }
        }

        m_IntegrationField[_TargetPos.x, _TargetPos.y] = 0;
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
                    if (IsInBounds(x + i, y + o) && m_CostField[x + i, y + o] != 255)
                    {
                        integratedCost = m_IntegrationField[x, y] + m_CostField[x + i, y + o];
                        if (integratedCost < m_IntegrationField[x + i, y + o])
                        {
                            m_IntegrationField[x + i, y + o] = integratedCost;
                            openCells.Add(new Vector2Int(x + i, y + o));
                        }
                    }
                }
            }

            openCells.RemoveAt(0);
        }
    }

    private bool IsInBounds(int _X, int _Y)
    {
        return _X >= 0 && _X < 80 && _Y >= 0 && _Y < 80;
    }

    public void CalculateFlowField(Vector2 _TargetPos)
    {
        for (int x = 0; x < 80; x++)
        {
            for (int y = 0; y < 80; y++)
            {
                if (m_IntegrationField[x, y] == float.MaxValue)
                {
                    m_Flowfield[x, y] = Vector2.zero;
                    continue;
                }
                Vector2 neighbour = FindCheapestNeighbour(x, y);
                m_Flowfield[x, y] = (new Vector2(x, y) - neighbour).normalized;
            }
        }
    }

    private Vector2Int FindCheapestNeighbour(int _X, int _Y)
    {
        Vector2Int cheapestNeighbour = new Vector2Int(_X, _Y);

        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (IsInBounds(_X + x, _Y + y) && m_IntegrationField[_X + x, _Y + y] != float.MaxValue)
                {
                    if (m_IntegrationField[cheapestNeighbour.x, cheapestNeighbour.y] > m_IntegrationField[_X + x, _Y + y])
                    {
                        cheapestNeighbour = new Vector2Int(_X + x, _Y + y);
                    }
                }
            }
        }

        return cheapestNeighbour;
    }

    private void OnDrawGizmos()
    {
        for (int x = 0; x < 80; x++)
        {
            for (int y = 0; y < 80; y++)
            {
                Vector3 vec = new Vector3(this.transform.position.x + x * 4, 0, this.transform.position.z + y * 4);
                Vector3 vec2 = new Vector3(m_Flowfield[x, y].x * 2, 0, m_Flowfield[x, y].y * 2);

                //Handles.Label(vec, m_IntegrationField[x,y].ToString());
                Gizmos.DrawLine(vec, vec + vec2);
                //Gizmos.DrawWireCube(vec, new Vector3(4 - 0.1f, 1, 4 - 0.1f));
            }
        }
    }
}