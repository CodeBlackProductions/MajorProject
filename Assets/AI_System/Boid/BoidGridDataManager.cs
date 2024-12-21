using System;
using System.Collections.Generic;
using UnityEngine;
using RBush;

public class BoidGridDataManager : MonoBehaviour
{
    [SerializeField] private float m_TreeUpdateInterval = 0.5f;

    private BoidDataManager m_DataManager;
    private Rigidbody m_Rb;
    private Vector3 m_OldPos;
    private float m_Timer;

    private EventManager m_Eventmanager = null;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_OldPos = Vector3.zero;
    }

    private void Start()
    {
        if (EventManager.Instance)
        {
            Guid guid = m_DataManager.Guid;
            m_Eventmanager = EventManager.Instance;

            m_Eventmanager.BoidDeath += OnBoidDeath;

            m_Eventmanager.SendTreeBoidRegister?.Invoke(gameObject, CreateTreeEntry(transform.position));
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (m_Timer <= 0)
        {
            m_Eventmanager.SendTreeBoidUpdate?.Invoke(gameObject, CreateTreeEntry(transform.position));

            if (RTree_DataManager.Instance)
            {
                float visRange = m_DataManager.QueryStat(BoidStat.VisRange);

                Dictionary<GameObject, Team> neighbours = RTree_DataManager.Instance.QueryNeighboursInRange(m_Rb.position, visRange);
                if (neighbours != null)
                {
                    SetNeighbours(neighbours);
                }

                if (Vector3.Distance(m_Rb.position, m_OldPos) > GridDataManager.Instance.CellSize)
                {
                    List<GameObject> obstacles = RTree_DataManager.Instance.QueryObstaclesInRange(m_Rb.position, visRange);
                    if (obstacles != null)
                    {
                        SetObstacles(obstacles);
                    }
                }
            }

            m_OldPos = m_Rb.position;
            m_Timer = m_TreeUpdateInterval;
        }
        else
        {
            m_Timer -= Time.deltaTime;
        }
    }

    private void SetNeighbours(Dictionary<GameObject, Team> neighbours)
    {
        Dictionary<Guid, Rigidbody> neighbouringAllies = new Dictionary<Guid, Rigidbody>();
        Dictionary<Guid, Rigidbody> neighbouringEnemies = new Dictionary<Guid, Rigidbody>();

        foreach (var n in neighbours)
        {
            if (n.Value == m_DataManager.Team && n.Key != gameObject)
            {
                neighbouringAllies.Add(n.Key.GetComponent<BoidDataManager>().Guid, n.Key.GetComponent<Rigidbody>());
            }
            else if (n.Key != gameObject)
            {
                neighbouringEnemies.Add(n.Key.GetComponent<BoidDataManager>().Guid, n.Key.GetComponent<Rigidbody>());
            }
        }

        m_DataManager.SetNeighbours(Team.Ally, neighbouringAllies);
        m_DataManager.SetNeighbours(Team.Enemy, neighbouringEnemies);
    }

    private void SetObstacles(List<GameObject> _Obstacles)
    {
        List<Vector3> obstaclesPositions = new List<Vector3>();
        for (int i = 0; i < _Obstacles.Count; i++)
        {
            obstaclesPositions.Add(_Obstacles[i].transform.position);
        }

        m_DataManager.SetObstacles(obstaclesPositions);
    }

    public void OnBoidDeath(KeyValuePair<Guid, BoidDataManager> _Boid)
    {
        if (m_DataManager.Guid == _Boid.Key)
        {
            m_Eventmanager.SendTreeBoidRemove?.Invoke(gameObject, CreateTreeEntry(m_OldPos));
        }
    }

    private RTree_Object CreateTreeEntry(Vector3 _Pos)
    {
        RTree_Object obj = new RTree_Object(new Envelope(_Pos.x, _Pos.z, _Pos.x, _Pos.z));

        obj.Object = gameObject;

        return obj;
    }
}