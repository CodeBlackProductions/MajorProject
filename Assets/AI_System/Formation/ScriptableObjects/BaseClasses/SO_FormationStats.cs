using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FormationStat
{
    MaxUnitCount, WidthScale, DepthScale, UnitSize, UnitSpacing
}

[CreateAssetMenu(fileName = "new_FormationStats", menuName = "Formation/SO_FormationStats")]
public class SO_FormationStats : ScriptableObject
{
    [SerializeField] private int m_MaxUnitCount = 0;
    [SerializeField] private float m_UnitSize = 0;
    [SerializeField][Range(0, 2)] private float m_UnitSpacing = 0;
    [SerializeField] private int m_WidthScale = 0;
    [SerializeField] private int m_DepthScale = 0;

    private Dictionary<FormationStat, float> m_Stats = new Dictionary<FormationStat, float>();

    public Dictionary<FormationStat, float> Stats { get => m_Stats; }

    private void OnEnable()
    {
        m_Stats.Clear();
        m_Stats.Add(FormationStat.MaxUnitCount, m_MaxUnitCount);
        m_Stats.Add(FormationStat.WidthScale, m_WidthScale);
        m_Stats.Add(FormationStat.DepthScale, m_DepthScale);
        m_Stats.Add(FormationStat.UnitSize, m_UnitSize);
        m_Stats.Add(FormationStat.UnitSpacing, m_UnitSpacing);
    }

    private void OnValidate()
    {
        m_Stats[FormationStat.MaxUnitCount] = m_MaxUnitCount;
        m_Stats[FormationStat.WidthScale] = m_WidthScale;
        m_Stats[FormationStat.DepthScale] = m_DepthScale;
        m_Stats[FormationStat.UnitSize] = m_UnitSize;
        m_Stats[FormationStat.UnitSpacing] = m_UnitSpacing;
    }
}