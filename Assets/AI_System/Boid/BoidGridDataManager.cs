using System;
using UnityEditor;
using UnityEngine;

public class BoidGridDataManager : MonoBehaviour
{
    [SerializeField] private float m_GridUpdateInterval = 0.5f;

    private BoidDataManager m_DataManager;
    private Rigidbody m_Rb;
    private Vector3 m_OldPos;
    private float m_Timer;

    private Action<BoidData> SendDataToGrid;

    private void Awake()
    {
        m_Rb = GetComponent<Rigidbody>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_OldPos = Vector3.zero;

        if (GridBoidManager.Instance != null)
        {
            SendDataToGrid += GridBoidManager.Instance.OnReceiveBoidPos;
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
            SendDataToGrid?.Invoke(PrepareData());
            m_OldPos = m_Rb.position;
            m_Timer = m_GridUpdateInterval;
        }
        else 
        {
            m_Timer -= Time.deltaTime;
        }
    }

    private BoidData PrepareData() 
    {
        BoidData data = new BoidData();
        data.boidGuid = m_DataManager.Guid;
        data.boidTeam = m_DataManager.Team;
        data.boidVis = m_DataManager.QueryStat(BoidStat.VisRange);
        data.boidPos = m_Rb.position;
        data.oldPos = m_OldPos;

        return data;
    }

    private void AddNeighbour(Guid _Guid, Guid _ToAdd, Team _TeamToAddTo) 
    {
        if (_Guid == m_DataManager.Guid)
        {

        }
    }

    private void RemoveNeighbour(Guid _Guid, Guid _ToRemove, Team _TeamToRemoveFrom) 
    {
        if (_Guid == m_DataManager.Guid) 
        {

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