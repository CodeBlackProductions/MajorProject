using System.Collections.Generic;
using UnityEngine;

public enum Weight
{
    FAllyCohesion, FAllySeparation, FAllyAlignment, FEnemyCohesion, FEnemySeparation, FEnemyAlignment, MovTarget, FormationCohesion, EnemyPursue, EnemyAvoidance, ObstacleAvoidance
}

[CreateAssetMenu(fileName = "SO_NewFlockingWeights", menuName = "Boids/SO_FlockingWeights")]
public class SO_FlockingWeights : ScriptableObject
{
    [Header("Flocking")]
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllyCohesionWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllySeparationWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_AllyAlignmentWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemyCohesionWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemySeparationWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemyAlignmentWeight = 0;

    [Header("Other Behaviours")]
    [SerializeField][Range(0.0f, 1.0f)] private float m_MovTargetWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_TargetEnemyPursueWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_EnemyAvoidanceWeight = 0;
    [SerializeField][Range(0.0f, 1.0f)] private float m_FormationCohesion = 0;


    private Dictionary<Weight, float> m_Weights = new Dictionary<Weight, float>();

    public Dictionary<Weight, float> Weights { get => m_Weights; }

    private void OnEnable()
    {
        m_Weights.Clear();
        m_Weights.Add(Weight.FAllyCohesion, m_AllyCohesionWeight);
        m_Weights.Add(Weight.FAllySeparation, m_AllySeparationWeight);
        m_Weights.Add(Weight.FAllyAlignment, m_AllyAlignmentWeight);

        m_Weights.Add(Weight.FEnemyCohesion, m_EnemyCohesionWeight);
        m_Weights.Add(Weight.FEnemySeparation, m_EnemySeparationWeight);
        m_Weights.Add(Weight.FEnemyAlignment, m_EnemyAlignmentWeight);

        m_Weights.Add(Weight.MovTarget, m_MovTargetWeight);
        m_Weights.Add(Weight.FormationCohesion, m_FormationCohesion);
        m_Weights.Add(Weight.EnemyPursue, m_TargetEnemyPursueWeight);
        m_Weights.Add(Weight.EnemyAvoidance, m_EnemyAvoidanceWeight);
    }

    private void OnValidate()
    {
        m_Weights[Weight.FAllyCohesion] = m_AllyCohesionWeight;
        m_Weights[Weight.FAllySeparation] = m_AllySeparationWeight;
        m_Weights[Weight.FAllyAlignment] = m_AllyAlignmentWeight;

        m_Weights[Weight.FEnemyCohesion] = m_EnemyCohesionWeight;
        m_Weights[Weight.FEnemySeparation] = m_EnemySeparationWeight;
        m_Weights[Weight.FEnemyAlignment] = m_EnemyAlignmentWeight;

        m_Weights[Weight.MovTarget] = m_MovTargetWeight;
        m_Weights[Weight.FormationCohesion] = m_FormationCohesion;
        m_Weights[Weight.EnemyPursue] = m_TargetEnemyPursueWeight;
        m_Weights[Weight.EnemyAvoidance] = m_EnemyAvoidanceWeight;
    }
}