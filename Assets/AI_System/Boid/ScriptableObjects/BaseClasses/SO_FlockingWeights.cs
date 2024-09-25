using System.Collections.Generic;
using UnityEngine;

public enum Weight
{
    AllyCohesion, AllySeparation, AllyAlignment, EnemyCohesion, EnemySeparation, EnemyAlignment
}

[CreateAssetMenu(fileName = "SO_NewFlockingWeights", menuName = "Boids/SO_FlockingWeights")]
public class SO_FlockingWeights : ScriptableObject
{
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllyCohesionWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllySeparationWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllyAlignmentWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemyCohesionWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemySeparationWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemyAlignmentWeight = 0;


    private Dictionary<Weight, float> m_Weights = new Dictionary<Weight, float>();

    public Dictionary<Weight, float> Weights { get => m_Weights; }

    private void OnEnable()
    {
        m_Weights.Clear();
        m_Weights.Add(Weight.AllyCohesion, m_AllyCohesionWeight);
        m_Weights.Add(Weight.AllySeparation, m_AllySeparationWeight);
        m_Weights.Add(Weight.AllyAlignment, m_AllyAlignmentWeight);

        m_Weights.Add(Weight.EnemyCohesion, m_EnemyCohesionWeight);
        m_Weights.Add(Weight.EnemySeparation, m_EnemySeparationWeight);
        m_Weights.Add(Weight.EnemyAlignment, m_EnemyAlignmentWeight);
    }

    private void OnValidate()
    {
        m_Weights[Weight.AllyCohesion] = m_AllyCohesionWeight;
        m_Weights[Weight.AllySeparation] = m_AllySeparationWeight;
        m_Weights[Weight.AllyAlignment] = m_AllyAlignmentWeight;

        m_Weights[Weight.EnemyCohesion] = m_EnemyCohesionWeight;
        m_Weights[Weight.EnemySeparation] = m_EnemySeparationWeight;
        m_Weights[Weight.EnemyAlignment] = m_EnemyAlignmentWeight;
    }
}