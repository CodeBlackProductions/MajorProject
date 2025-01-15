using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Team
{ Ally = 1, Neutral = 0, Enemy = -1 }

public class BoidDataManager : MonoBehaviour
{
    [SerializeField] private SO_BoidStats m_BaseStats;
    [SerializeField] private int m_MaxNeighboursToCalculate = 25;
    [SerializeField] private GameObject m_SelectionIndicator;

    private Dictionary<BoidStat, float> m_Stats = new Dictionary<BoidStat, float>();
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

    private void Update()
    {
        if (m_IsSelectedByPlayer)
        {
            for (int i = 0; i < m_NeighbourAllyList.Count; i++)
            {
                Debug.DrawLine(m_NeighbourAllyList[i].Value.transform.position, transform.position, Color.green);
            }

            for (int i = 0; i < m_NeighbourEnemyList.Count; i++)
            {
                Debug.DrawLine(m_NeighbourEnemyList[i].Value.transform.position, transform.position, Color.red);
            }
        }
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

    public void SetNeighbours(Team _Team, List<KeyValuePair<Guid, Rigidbody>> _Boids)
    {
        _Boids.RemoveAll(neighbour => neighbour.Value.gameObject.activeSelf == false);
        if (_Boids.Count > m_MaxNeighboursToCalculate)
        {
            _Boids.OrderBy(neighbour => Vector3.Distance(neighbour.Value.position, transform.position));
            _Boids.RemoveRange(m_MaxNeighboursToCalculate, _Boids.Count - m_MaxNeighboursToCalculate);
        }
        else
        {
            _Boids.OrderBy(neighbour => Vector3.Distance(neighbour.Value.position, transform.position));
        }

        if (_Team == Team.Ally)
        {
            m_NeighbourAllyList = _Boids;
        }
        else if (_Team == Team.Neutral)
        {
            return;
        }
        else if (_Team == Team.Enemy)
        {
            m_NeighbourEnemyList = _Boids;
        }
    }

    public void RemoveNeighbour(Team _Team, Guid _ID)
    {
        if (_Team == m_Team)
        {
            KeyValuePair<Guid, Rigidbody> toRemove = m_NeighbourAllyList.Find(c => c.Key == _ID);

            if (toRemove.Value != null)
            {
                m_NeighbourAllyList.Remove(toRemove);
            }
        }
        else if (_Team == Team.Neutral)
        {
            return;
        }
        else
        {
            KeyValuePair<Guid, Rigidbody> toRemove = m_NeighbourEnemyList.Find(c => c.Key == _ID);

            if (toRemove.Value != null)
            {
                m_NeighbourEnemyList.Remove(toRemove);
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
                return FindClosestNeighbour(m_NeighbourAllyList);

            case Team.Neutral:
                return new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);

            case Team.Enemy:
                return FindClosestNeighbour(m_NeighbourEnemyList);

            default:
                return new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);
        }
    }

    private KeyValuePair<Guid, Rigidbody> FindClosestNeighbour(List<KeyValuePair<Guid, Rigidbody>> _Neighbours)
    {
        float dist = float.MaxValue;
        KeyValuePair<Guid, Rigidbody> closestNeighbour = new KeyValuePair<Guid, Rigidbody>(Guid.NewGuid(), null);
        if (_Neighbours.Count > 1)
        {
            foreach (var neighbour in _Neighbours)
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
        else if (_Neighbours.Count > 0)
        {
            closestNeighbour = _Neighbours.First();
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