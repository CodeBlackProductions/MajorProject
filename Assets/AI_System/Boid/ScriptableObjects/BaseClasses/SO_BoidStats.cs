using System.Collections.Generic;
using UnityEngine;

public enum BoidStat
{
    Health, MovSpeed, VisRange, Mass, AtkRange, StopRange, TurnRate, AtkDamage, AtkSpeed, FormationRadius
}

[CreateAssetMenu(fileName = "SO_NewBoidStats", menuName = "Boids/SO_BoidStats")]
public class SO_BoidStats : ScriptableObject
{
    [SerializeField] private float m_Health = 0;
    [SerializeField] private float m_MovSpeed = 0;
    [SerializeField] private float m_VisRange = 0;
    [SerializeField] private float m_Mass = 0;
    [SerializeField] private float m_AtkRange = 0;
    [SerializeField] private float m_StopRange = 0;
    [SerializeField] private float m_AtkDamage = 0;
    [SerializeField] private float m_AtkSpeed = 0;
    [SerializeField] private float m_TurnRate = 0;
    [SerializeField] private float m_FormationRadius = 0;

    private Dictionary<BoidStat, float> m_Stats = new Dictionary<BoidStat, float>();

    public Dictionary<BoidStat, float> Stats { get => m_Stats; }

    private void OnEnable()
    {
        m_Stats.Clear();
        m_Stats.Add(BoidStat.Health, m_Health);
        m_Stats.Add(BoidStat.MovSpeed, m_MovSpeed);
        m_Stats.Add(BoidStat.VisRange, m_VisRange);
        m_Stats.Add(BoidStat.Mass, m_Mass);
        m_Stats.Add(BoidStat.AtkRange, m_AtkRange);
        m_Stats.Add(BoidStat.StopRange, m_StopRange);
        m_Stats.Add(BoidStat.TurnRate, m_TurnRate);
        m_Stats.Add(BoidStat.AtkDamage, m_AtkDamage);
        m_Stats.Add(BoidStat.AtkSpeed, m_AtkSpeed);
        m_Stats.Add(BoidStat.FormationRadius, m_FormationRadius);
    }

    private void OnValidate()
    {
        m_Stats[BoidStat.Health] = m_Health;
        m_Stats[BoidStat.MovSpeed] = m_MovSpeed;
        m_Stats[BoidStat.VisRange] = m_VisRange;
        m_Stats[BoidStat.Mass] = m_Mass;
        m_Stats[BoidStat.TurnRate] = m_TurnRate;
        m_Stats[BoidStat.AtkRange] = m_AtkRange;
        m_Stats[BoidStat.StopRange] = m_StopRange;
        m_Stats[BoidStat.AtkDamage] = m_AtkDamage;
        m_Stats[BoidStat.AtkSpeed] = m_AtkSpeed;
        m_Stats[BoidStat.FormationRadius] = m_FormationRadius;
    }
}