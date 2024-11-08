using System;
using System.Collections.Generic;
using UnityEngine;

public class FormationDataManager : MonoBehaviour
{
    [SerializeField] private SO_FormationStats m_BaseStats;

    private Dictionary<FormationStat, float> m_Stats = new Dictionary<FormationStat, float>();

    private Dictionary<int, Vector3> m_BoidOffsets = new Dictionary<int, Vector3>();

    private Vector2 m_FlowfieldDir = Vector2.zero;

    private Queue<Vector3> m_MovTargets = new Queue<Vector3>();
    private Vector3 m_CurrentMovTarget = Vector3.zero;
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

        CalculateBoidOffsets(
            (int)m_Stats[FormationStat.MaxUnitCount],
            m_Stats[FormationStat.WidthScale],
            m_Stats[FormationStat.DepthScale],
            m_Stats[FormationStat.UnitSize],
            m_Stats[FormationStat.UnitSpacing]);
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

    private void CalculateBoidOffsets(int _UnitCount, float _WidthScale, float _DepthScale, float _UnitSize, float _UnitSpacing)
    {
        Vector3[] gridOffsets = new Vector3[_UnitCount];

        int[] sortedIndices = new int[_UnitCount];

        for (int i = 0; i < _UnitCount; i++)
        {
            sortedIndices[i] = i;
        }

        int numberOfRows = 0;
        int numberOfColumns = 0;

        numberOfRows = (int)Mathf.Sqrt(_UnitCount / (_DepthScale / _WidthScale));
        numberOfColumns = _UnitCount / numberOfRows;

        float centerX = (numberOfColumns - 1) * 0.5f;
        float centerY = (numberOfRows - 1) * 0.5f;

        for (int i = 0; i < _UnitCount; i++)
        {
            float col = (i % numberOfColumns) - (numberOfColumns - 1) * 0.5f;
            float row = (i / numberOfColumns) - (numberOfRows - 1) * 0.5f;
            gridOffsets[i] = new Vector3(col, 0, row);
        }

        sortedIndices = SortOffsets(sortedIndices, numberOfColumns, centerX, centerY);

        for (int i = 0; i < gridOffsets.Length; i++)
        {
            int index = sortedIndices[i];

            m_BoidOffsets[index] = gridOffsets[i] * (_UnitSize + _UnitSpacing);
        }
    }

    private int[] SortOffsets(int[] _IndicesToSort, int _NumberOfColumns, float _CenterX, float _CenterY)
    {
        int[] sortedIndices = _IndicesToSort;

        Array.Sort(sortedIndices, (a, b) =>
        {
            float distA = Mathf.Sqrt(Mathf.Pow((a % _NumberOfColumns) - _CenterX, 2) + Mathf.Pow((a / _NumberOfColumns) - _CenterY, 2));
            float distB = Mathf.Sqrt(Mathf.Pow((b % _NumberOfColumns) - _CenterX, 2) + Mathf.Pow((b / _NumberOfColumns) - _CenterY, 2));
            int distanceComparison = distA.CompareTo(distB);

            if (distanceComparison != 0)
            {
                return distanceComparison;
            }
            else
            {
                return a.CompareTo(b);
            }
        });

        return sortedIndices;
    }

    public Vector3 QueryBoidPosition(int _BoidIndex)
    {
        Quaternion rotation = Quaternion.LookRotation(transform.forward);

        Vector3 rotatedOffset = rotation * m_BoidOffsets[_BoidIndex];

        return transform.position + rotatedOffset;
    }
}