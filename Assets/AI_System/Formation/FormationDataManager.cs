using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FormationDataManager : MonoBehaviour
{
    [SerializeField] private SO_FormationStats m_BaseStats;
    [SerializeField] private SO_FormationOffsetCalculation m_FormationType;

    private Dictionary<FormationStat, float> m_Stats = new Dictionary<FormationStat, float>();

    private Dictionary<int, Vector3> m_BoidOffsets = new Dictionary<int, Vector3>();

    private FlowfieldManager m_FlowfieldManager = FlowfieldManager.Instance;

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

        m_BoidOffsets = m_FormationType.CalculateOffsets(
                (int)m_Stats[FormationStat.MaxUnitCount],
                m_Stats[FormationStat.WidthScale],
                m_Stats[FormationStat.DepthScale],
                m_Stats[FormationStat.UnitSize],
                m_Stats[FormationStat.UnitSpacing]
                );
    }

    public void SetStat(FormationStat _Stat, float _Value)
    {
        m_Stats[_Stat] = _Value;
    }

    public void ResetToBaseStat(FormationStat _Stat)
    {
        m_Stats[_Stat] = m_BaseStats.Stats[_Stat];
    }

    public float QueryStat(FormationStat _Stat)
    {
        return m_Stats[_Stat];
    }

    public void UpdateBoidOffsets(int _NewBoidCount)
    {
        m_Stats[FormationStat.MaxUnitCount] = _NewBoidCount;

        m_BoidOffsets = m_FormationType.CalculateOffsets(
            _NewBoidCount,
            m_Stats[FormationStat.WidthScale],
            m_Stats[FormationStat.DepthScale],
            m_Stats[FormationStat.UnitSize],
            m_Stats[FormationStat.UnitSpacing]
            );
    }

    public void UpdateBoidOffsets()
    {
        m_BoidOffsets = m_FormationType.CalculateOffsets(
            (int)m_Stats[FormationStat.MaxUnitCount],
            m_Stats[FormationStat.WidthScale],
            m_Stats[FormationStat.DepthScale],
            m_Stats[FormationStat.UnitSize],
            m_Stats[FormationStat.UnitSpacing]
            );
    }

    public Vector3 QueryBoidPosition(int _BoidIndex)
    {
        Quaternion rotation = Quaternion.LookRotation(transform.forward);

        Vector3 rotatedOffset = rotation * m_BoidOffsets[_BoidIndex];

        return transform.position + rotatedOffset;
    }

    public Vector3 QueryBoidOffset(int _BoidIndex)
    {
        Quaternion rotation = Quaternion.LookRotation(transform.forward);

        Vector3 rotatedOffset = rotation * m_BoidOffsets[_BoidIndex];

        return rotatedOffset;
    }

    public Vector2 QueryFlowfieldDir(Vector3 _MovTarget)
    {
        Vector2[,] flowfield;
        Vector2 dir = Vector2.zero;
        Vector2Int targetpos = new Vector2Int((int)(_MovTarget.x / GridDataManager.Instance.CellSize), (int)(_MovTarget.z / GridDataManager.Instance.CellSize));
        Vector2Int pos = new Vector2Int((int)(transform.position.x / GridDataManager.Instance.CellSize), (int)(transform.position.z / GridDataManager.Instance.CellSize));
        flowfield = m_FlowfieldManager.QueryFlowfield(targetpos);
        dir = flowfield[pos.x, pos.y];

        return dir;
    }
}