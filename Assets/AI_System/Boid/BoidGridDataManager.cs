using System;
using UnityEngine;

public class BoidGridDataManager : MonoBehaviour
{
    [SerializeField] private float m_GridUpdateInterval = 0.5f;

    private BoidDataManager m_DataManager;
    private Rigidbody m_Rb;
    private Vector3 m_OldPos;
    private float m_Timer;

    private Action<BoidData> SendDataToGrid;

    private BoidData m_BoidData = new BoidData();

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_OldPos = Vector3.zero;

        if (GridBoidManager.Instance != null)
        {
            SendDataToGrid += GridBoidManager.Instance.OnReceiveBoidPos;
            GridBoidManager.Instance.OnAddBoid += AddNeighbour;
            GridBoidManager.Instance.OnRemoveBoid += RemoveNeighbour;
            GridBoidManager.Instance.OnRemoveVision += RemoveObstacle;
            GridBoidManager.Instance.OnAddVision += AddObstacle;
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
            SendDataToGrid?.Invoke(m_BoidData);
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

    private void AddNeighbour(Guid _Guid, Guid _ToAdd, Team _TeamToAddTo)
    {
        if (_Guid == m_DataManager.Guid)
        {
            Rigidbody neighbour = BoidPool.Instance.GetActiveBoid(_ToAdd).GetComponent<Rigidbody>();
            m_DataManager.AddNeighbour(_TeamToAddTo, _ToAdd, neighbour);
        }
    }

    private void RemoveNeighbour(Guid _Guid, Guid _ToRemove, Team _TeamToRemoveFrom)
    {
        if (_Guid == m_DataManager.Guid)
        {
            m_DataManager.RemoveNeighbour(_TeamToRemoveFrom, _ToRemove);
        }
    }

    private void AddObstacle(Guid _Guid, Vector3 _Pos)
    {
        if (_Guid == m_DataManager.Guid)
        {
            m_DataManager.AddObstacle(_Pos);
        }
    }

    private void RemoveObstacle(Guid _Guid, Vector3 _Pos)
    {
        if (_Guid == m_DataManager.Guid)
        {
            m_DataManager.RemoveObstacle(_Pos);
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