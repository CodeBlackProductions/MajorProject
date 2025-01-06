using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Team
{ Ally = 1, Neutral = 0, Enemy = -1 }

public class BoidDataManager : MonoBehaviour
{
    [SerializeField] private SO_BoidStats m_BaseStats;
    [SerializeField] private int m_MaxNeighboursToCalculate = 50;
    [SerializeField] private GameObject m_SelectionIndicator;

    private Dictionary<BoidStat, float> m_Stats = new Dictionary<BoidStat, float>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringEnemies = new Dictionary<Guid, Rigidbody>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringAllies = new Dictionary<Guid, Rigidbody>();

    private List<KeyValuePair<Guid, Rigidbody>> m_NeighbourAllyList = new List<KeyValuePair<Guid, Rigidbody>>();
    private List<KeyValuePair<Guid, Rigidbody>> m_NeighbourEnemyList = new List<KeyValuePair<Guid, Rigidbody>>();

    private List<Vector3> m_NearbyObstacles = new List<Vector3>();
    private Queue<Vector3> m_MovTargets = new Queue<Vector3>();
    private Vector3 m_CurrentMovTarget = Vector3.zero;
    private FormationBoidManager m_FormationBoidManager;
    private Vector3 m_FormationPosition = Vector3.zero;
    private Vector3 m_FormationOffset = Vector3.zero;
    private Vector3 m_FormationCenter = Vector3.zero;
    private Guid m_Guid;
    private Team m_Team;
    private Vector2[,] m_CurrentFlowfield;
    private Vector2Int m_CurrentFlowfieldTarget = Vector2Int.zero;
    private bool m_IsSelectedByPlayer = false;

    public FormationBoidManager FormationBoidManager { get => m_FormationBoidManager; set => m_FormationBoidManager = value; }
    public Vector3 FormationPosition { get => m_FormationPosition; set => m_FormationPosition = value; }
    public Vector3 FormationOffset { get => m_FormationOffset; set => m_FormationOffset = value; }
    public Vector3 FormationCenter { get => m_FormationCenter; set => m_FormationCenter = value; }
    public Guid Guid { get => m_Guid; set => m_Guid = value; }
    public Team Team { get => m_Team; set => m_Team = value; }

    public bool IsSelectedByPlayer
    {
        get => m_IsSelectedByPlayer;
        set
        {
            m_IsSelectedByPlayer = value;
            if (m_SelectionIndicator != null)
            {
                m_SelectionIndicator.SetActive(m_IsSelectedByPlayer);
            }
        }
    }

    private List<KeyValuePair<Team, Guid>> m_RemoveBuffer = new List<KeyValuePair<Team, Guid>>();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach (var stat in m_BaseStats.Stats)
        {
            m_Stats.Add(stat.Key, stat.Value);
        }

        m_SelectionIndicator.SetActive(false);
    }

    public void SetStat(BoidStat _Stat, float _Value)
    {
        m_Stats[_Stat] = _Value;
    }

    public void ResetToBaseStat(BoidStat _Stat)
    {
        m_Stats[_Stat] = m_BaseStats.Stats[_Stat];
    }

    public float QueryStat(BoidStat _Stat)
    {
        return m_Stats[_Stat];
    }

    public void SetNeighbours(Team _Team, Dictionary<Guid, Rigidbody> _Boids)
    {
        if (_Team == Team.Ally)
        {
            List<KeyValuePair<Guid, Rigidbody>> sortedAllies = _Boids.ToList();
            sortedAllies.RemoveAll(neighbour => neighbour.Value.gameObject.activeSelf == false);
            if (sortedAllies.Count > m_MaxNeighboursToCalculate)
            {
                if (sortedAllies.Count > m_MaxNeighboursToCalculate)
                {
                    sortedAllies.OrderBy(neighbour => Vector3.Distance(neighbour.Value.position, transform.position));
                    sortedAllies.RemoveRange(m_MaxNeighboursToCalculate, sortedAllies.Count - m_MaxNeighboursToCalculate);
                }
            }

            m_NeighbouringAllies.Clear();

            for (int i = 0; i < sortedAllies.Count; i++)
            {
                m_NeighbouringAllies.Add(sortedAllies[i].Key, sortedAllies[i].Value);
            }

            m_NeighbourAllyList = sortedAllies;
        }
        else if (_Team == Team.Neutral)
        {
            return;
        }
        else if (_Team == Team.Enemy)
        {
            List<KeyValuePair<Guid, Rigidbody>> sortedEnemies = _Boids.ToList();
            sortedEnemies.RemoveAll(neighbour => neighbour.Value.gameObject.activeSelf == false);
            if (sortedEnemies.Count > m_MaxNeighboursToCalculate)
            {
                if (sortedEnemies.Count > m_MaxNeighboursToCalculate)
                {
                    sortedEnemies.OrderBy(neighbour => Vector3.Distance(neighbour.Value.position, transform.position));
                    sortedEnemies.RemoveRange(m_MaxNeighboursToCalculate, sortedEnemies.Count - m_MaxNeighboursToCalculate);
                }
            }

            m_NeighbouringEnemies.Clear();

            for (int i = 0; i < sortedEnemies.Count; i++)
            {
                m_NeighbouringEnemies.Add(sortedEnemies[i].Key, sortedEnemies[i].Value);
            }

            m_NeighbourEnemyList = sortedEnemies;
        }
    }

    public void RemoveNeighbour(Team _Team, Guid _ID)
    {
        if (_Team == m_Team)
        {
            if (m_NeighbouringAllies.ContainsKey(_ID))
            {
                m_NeighbouringAllies.Remove(_ID);
            }
        }
        else if (_Team == Team.Neutral)
        {
            return;
        }
        else
        {
            if (m_NeighbouringEnemies.ContainsKey(_ID))
            {
                m_NeighbouringEnemies.Remove(_ID);
            }
        }
    }

    public List<KeyValuePair<Guid, Rigidbody>> QueryNeighbours(Team _Team)
    {
        switch (_Team)
        {
            case Team.Ally:

                return m_NeighbourAllyList;

            case Team.Neutral:
                break;

            case Team.Enemy:

                return m_NeighbourEnemyList;

            default:
                break;
        }

        return null;
    }

    public KeyValuePair<Guid, Rigidbody> QueryClosestNeighbour(Team _Team)
    {
        switch (_Team)
        {
            case Team.Ally:
                return FindClosestNeighbour(m_NeighbouringAllies);

            case Team.Neutral:
                return new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);

            case Team.Enemy:
                return FindClosestNeighbour(m_NeighbouringEnemies);

            default:
                return new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);
        }
    }

    private KeyValuePair<Guid, Rigidbody> FindClosestNeighbour(Dictionary<Guid, Rigidbody> keyValuePairs)
    {
        float dist = float.MaxValue;
        KeyValuePair<Guid, Rigidbody> closestNeighbour = new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);
        if (keyValuePairs.Count > 1)
        {
            foreach (var neighbour in keyValuePairs)
            {
                if (!neighbour.Value.gameObject.activeSelf)
                {
                    m_RemoveBuffer.Add(new KeyValuePair<Team, Guid>(neighbour.Value.GetComponent<BoidDataManager>().Team, neighbour.Key));
                    continue;
                }

                float currentDist = Vector3.Distance(transform.position, neighbour.Value.position);
                if (dist > currentDist)
                {
                    dist = currentDist;
                    closestNeighbour = neighbour;
                }
            }
        }
        else if (keyValuePairs.Count > 0)
        {
            closestNeighbour = keyValuePairs.First();
        }

        if (m_RemoveBuffer.Count > 0)
        {
            for (int i = 0; i < m_RemoveBuffer.Count; i++)
            {
                RemoveNeighbour(m_RemoveBuffer[i].Key, m_RemoveBuffer[i].Value);
            }
        }

        if (closestNeighbour.Value != null && closestNeighbour.Value.gameObject.activeSelf)
        {
            return closestNeighbour;
        }
        else
        {
            return new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);
        }
    }

    public Vector3 QueryNextMovTarget()
    {
        if (m_MovTargets.Count > 0 && (Vector3.Distance(m_CurrentMovTarget, transform.position) <= m_Stats[BoidStat.VisRange] * 0.5f || m_CurrentMovTarget == Vector3.zero))
        {
            m_CurrentMovTarget = m_MovTargets.Dequeue();
        }

        return m_CurrentMovTarget;
    }

    public void AddMovTarget(Vector3 _Pos)
    {
        m_MovTargets.Enqueue(_Pos);
    }

    public void SetMovTarget(Vector3 _Pos)
    {
        m_MovTargets.Clear();
        m_MovTargets.Enqueue(_Pos);
        m_CurrentMovTarget = Vector3.zero;
    }

    public void SetObstacles(List<Vector3> _Obstacles)
    {
        m_NearbyObstacles = _Obstacles;
    }

    public Vector3[] QueryObstaclePositions()
    {
        return m_NearbyObstacles.ToArray();
    }

    public Vector2[,] QueryFlowfield(Vector3 _TargetPos)
    {
        if (m_CurrentMovTarget == Vector3.zero)
        {
            return null;
        }

        if (m_CurrentFlowfield != null)
        {
            Vector2Int tempVec = new Vector2Int((int)(_TargetPos.x / GridDataManager.Instance.CellSize), (int)(_TargetPos.z / GridDataManager.Instance.CellSize));
            if (tempVec != m_CurrentFlowfieldTarget)
            {
                m_CurrentFlowfieldTarget = tempVec;
                m_CurrentFlowfield = FlowfieldManager.Instance.QueryFlowfield(m_CurrentFlowfieldTarget);
            }
        }
        else
        {
            Vector2Int tempVec = new Vector2Int((int)(_TargetPos.x / GridDataManager.Instance.CellSize), (int)(_TargetPos.z / GridDataManager.Instance.CellSize));
            m_CurrentFlowfieldTarget = tempVec;
            m_CurrentFlowfield = FlowfieldManager.Instance.QueryFlowfield(m_CurrentFlowfieldTarget);
        }

        return m_CurrentFlowfield;
    }
}