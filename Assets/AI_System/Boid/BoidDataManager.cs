using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Team
{ Ally = 1, Neutral = 0, Enemy = -1 }

public class BoidDataManager : MonoBehaviour
{
    [SerializeField] private SO_BoidStats m_BaseStats;

    private Dictionary<BoidStat, float> m_Stats = new Dictionary<BoidStat, float>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringEnemies = new Dictionary<Guid, Rigidbody>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringAllies = new Dictionary<Guid, Rigidbody>();
    private List<Vector3> m_NearbyObstacles = new List<Vector3>();
    private Queue<Vector3> m_MovTargets = new Queue<Vector3>();
    private Vector3 m_CurrentMovTarget = Vector3.zero;
    private Vector3 m_FormationPosition = Vector3.zero;
    private Guid m_Guid;
    private Team m_Team;

    public Vector3 FormationPosition { get => m_FormationPosition; set => m_FormationPosition = value; }
    public Guid Guid { get => m_Guid; set => m_Guid = value; }
    public Team Team { get => m_Team; set => m_Team = value; }

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

    public void AddNeighbour(Team _Team, Guid _ID, Rigidbody _RB)
    {
        if (_Team == m_Team)
        {
            if (!m_NeighbouringAllies.ContainsKey(_ID))
            {
                m_NeighbouringAllies.Add(_ID, _RB);
            }
        }
        else if (_Team == Team.Neutral)
        {
            return;
        }
        else
        {
            if (!m_NeighbouringEnemies.ContainsKey(_ID))
            {
                m_NeighbouringEnemies.Add(_ID, _RB);
            }
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

    public Dictionary<Guid, Rigidbody> QueryNeighbours(Team _Team)
    {
        switch (_Team)
        {
            case Team.Ally:
                return m_NeighbouringAllies;

            case Team.Neutral:
                break;

            case Team.Enemy:
                return m_NeighbouringEnemies;

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

        return closestNeighbour;
    }

    public Vector3 QueryNextMovTarget()
    {
        if (m_MovTargets.Count > 0 && (Vector3.Distance(m_CurrentMovTarget, transform.position) <= m_Stats[BoidStat.AtkRange] || m_CurrentMovTarget == Vector3.zero))
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
    }

    public void AddObstacle(Vector3 _Obstacle)
    {
        if (!m_NearbyObstacles.Contains(_Obstacle))
        {
            m_NearbyObstacles.Add(_Obstacle);
        }
    }

    public void RemoveObstacle(Vector3 _Obstacle)
    {
        if (m_NearbyObstacles.Contains(_Obstacle))
        {
            m_NearbyObstacles.Remove(_Obstacle);
        }
    }

    public Vector3[] QueryObstaclePositions()
    {
        Vector3[] positions = new Vector3[m_NearbyObstacles.Count];

        for (int i = 0; i < m_NearbyObstacles.Count; i++)
        {
            positions[i] = m_NearbyObstacles[i];
        }

        return positions;
    }

    //To DO: Determine Obstacle sizes? CHange to Grid based approach?
    public float[] QueryObstacleSizes()
    {
        float[] sizes = new float[m_NearbyObstacles.Count];

        for (int i = 0; i < m_NearbyObstacles.Count; i++)
        {
            sizes[i] = 4;
        }

        return sizes;
    }
}