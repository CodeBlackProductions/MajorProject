using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidGridDataManager : MonoBehaviour
{
    [SerializeField] private float m_GridUpdateInterval = 0.5f;

    private BoidDataManager m_DataManager;
    private Rigidbody m_Rb;
    private Vector3 m_OldPos;
    private float m_Timer;

    private BoidData m_BoidData = new BoidData();
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

            if (!m_Eventmanager.OnAddBoidToGridCallbacks.ContainsKey(guid))
            {
                m_Eventmanager.OnAddBoidToGridCallbacks[guid] = null;
                m_Eventmanager.OnAddBoidToGridCallbacks[guid] += AddNeighbour;
            }

            if (!m_Eventmanager.OnRemoveBoidFromGridCallbacks.ContainsKey(guid))
            {
                m_Eventmanager.OnRemoveBoidFromGridCallbacks[guid] = null;
                m_Eventmanager.OnRemoveBoidFromGridCallbacks[guid] += RemoveNeighbour;
            }

            if (!m_Eventmanager.OnRemoveBoidVisionFromGridCallbacks.ContainsKey(guid))
            {
                m_Eventmanager.OnRemoveBoidVisionFromGridCallbacks[guid] = null;
                m_Eventmanager.OnRemoveBoidVisionFromGridCallbacks[guid] += RemoveObstacle;
            }

            if (!m_Eventmanager.OnAddBoidVisionToGridCallbacks.ContainsKey(guid))
            {
                m_Eventmanager.OnAddBoidVisionToGridCallbacks[guid] = null;
                m_Eventmanager.OnAddBoidVisionToGridCallbacks[guid] += AddObstacle;
            }
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
            PrepareData();
            m_Eventmanager.SendBoidDataToGrid?.Invoke(m_BoidData);
            m_OldPos = m_Rb.position;
            m_Timer = m_GridUpdateInterval;
        }
        else
        {
            m_Timer -= Time.deltaTime;
        }
    }

    private void PrepareData()
    {
        m_BoidData.boidGuid = m_DataManager.Guid;
        m_BoidData.boidTeam = m_DataManager.Team;
        m_BoidData.boidVis = m_DataManager.QueryStat(BoidStat.VisRange);
        m_BoidData.boidPos = m_Rb.position;
        m_BoidData.oldPos = m_OldPos;
    }

    private void AddNeighbour(Guid _ToAdd, Team _TeamToAddTo)
    {
        Rigidbody neighbour = BoidPool.Instance.GetActiveBoid(_ToAdd).GetComponent<Rigidbody>();
        m_DataManager.AddNeighbour(_TeamToAddTo, _ToAdd, neighbour);
    }

    private void RemoveNeighbour(Guid _ToRemove, Team _TeamToRemoveFrom)
    {
        m_DataManager.RemoveNeighbour(_TeamToRemoveFrom, _ToRemove);
    }

    private void AddObstacle(Vector3 _Pos)
    {
        m_DataManager.AddObstacle(_Pos);
    }

    private void RemoveObstacle(Vector3 _Pos)
    {
        m_DataManager.RemoveObstacle(_Pos);
    }

    public void OnBoidDeath(KeyValuePair<Guid, BoidDataManager> _Boid)
    {
        if (m_DataManager.Guid == _Boid.Key)
        {
            PrepareData();
            m_BoidData.boidPos = Vector3.zero;
            m_Eventmanager.SendBoidDataToGrid?.Invoke(m_BoidData);
        }
    }
}

public struct BoidData
{
    public Guid boidGuid;
    public Team boidTeam;
    public float boidVis;
    public Vector3 boidPos;
    public Vector3 oldPos;
}