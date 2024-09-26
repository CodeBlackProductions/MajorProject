using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public enum Team
{ Ally = 1, Neutral = 0, Enemy = -1 }

public class BoidDataManager : MonoBehaviour
{
    [SerializeField] private SO_BoidStats m_BaseStats;

    private Dictionary<Stat, float> m_Stats = new Dictionary<Stat, float>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringEnemies = new Dictionary<Guid, Rigidbody>();
    private Dictionary<Guid, Rigidbody> m_NeighbouringAllies = new Dictionary<Guid, Rigidbody>();
    private Queue<Vector3> m_MovTargets = new Queue<Vector3>();
    private Vector3 m_CurrentMovTarget = Vector3.zero;

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

    public void SetStat(Stat _Stat, float _Value)
    {
        m_Stats[_Stat] = _Value;
    }

    public void ResetToBaseStat(Stat _Stat)
    {
        m_Stats[_Stat] = m_BaseStats.Stats[_Stat];
    }

    public float QueryStat(Stat _Stat)
    {
        return m_Stats[_Stat];
    }

    public void AddNeighbour(Team _Team, Guid _ID, Rigidbody _RB)
    {
        switch (_Team)
        {
            case Team.Ally:
                m_NeighbouringAllies.Add(_ID, _RB);
                return;

            case Team.Neutral:
                break;

            case Team.Enemy:
                m_NeighbouringEnemies.Add(_ID, _RB);
                return;

            default:
                break;
        }
    }

    public void RemoveNeighbour(Team _Team, Guid _ID)
    {
        switch (_Team)
        {
            case Team.Ally:
                m_NeighbouringAllies.Remove(_ID);
                return;

            case Team.Neutral:
                break;

            case Team.Enemy:
                m_NeighbouringEnemies.Remove(_ID);
                return;

            default:
                break;
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
        if (m_MovTargets.Count > 0 && Vector3.Distance(m_CurrentMovTarget, transform.position) <= m_Stats[Stat.AtkRange])
        {
            m_CurrentMovTarget = m_MovTargets.Dequeue();
        }

        return m_CurrentMovTarget;
    }
}