using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidDataManager : MonoBehaviour
{
    [SerializeField] private SO_BoidStats m_BaseStats;

    private Dictionary<Stat, float> m_Stats = new Dictionary<Stat, float>();

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

}
