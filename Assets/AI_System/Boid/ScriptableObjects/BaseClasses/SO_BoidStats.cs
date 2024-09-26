using System.Collections.Generic;
using UnityEngine;

public enum Stat
{
    Health, MovSpeed, VisRange, Mass, AtkRange, TurnRate
}

[CreateAssetMenu(fileName = "SO_NewBoidStats", menuName = "Boids/SO_BoidStats")]
public class SO_BoidStats : ScriptableObject
{
    [SerializeField] private float m_Health = 0;
    [SerializeField] private float m_MovSpeed = 0;
    [SerializeField] private float m_VisRange = 0;
    [SerializeField] private float m_Mass = 0;
    [SerializeField] private float m_AtkRange = 0;
    [SerializeField] private float m_TurnRate = 0;

    private Dictionary<Stat, float> m_Stats = new Dictionary<Stat, float>();

    public Dictionary<Stat, float> Stats { get => m_Stats; }

    private void OnEnable()
    {
        m_Stats.Clear();
        m_Stats.Add(Stat.Health, m_Health);
        m_Stats.Add(Stat.MovSpeed, m_MovSpeed);
        m_Stats.Add(Stat.VisRange, m_VisRange);
        m_Stats.Add(Stat.Mass, m_Mass);
        m_Stats.Add(Stat.AtkRange, m_AtkRange);
        m_Stats.Add(Stat.TurnRate, m_TurnRate);
    }

    private void OnValidate()
    {
        m_Stats[Stat.Health] = m_Health;
        m_Stats[Stat.MovSpeed] = m_MovSpeed;
        m_Stats[Stat.VisRange] = m_VisRange;
        m_Stats[Stat.Mass] = m_Mass;
        m_Stats[Stat.TurnRate] = m_TurnRate;
    }
}