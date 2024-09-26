using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidFlockingWeightManager))]
[RequireComponent(typeof(BoidDataManager))]
public class BoidFlockingManager : MonoBehaviour
{
    private BoidFlockingWeightManager m_WeightManager;
    private BoidDataManager m_DataManager;

    public Action<Vector3> OnBehaviourUpdate;

    private Rigidbody m_Rigidbody;
    private bool m_Incombat = false;

    private void Awake()
    {
        m_WeightManager = GetComponent<BoidFlockingWeightManager>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Vector3 desiredVelocity = Vector3.zero;

        float movSpeed = 0;
        if (!m_Incombat)
        {
            movSpeed = m_DataManager.QueryStat(Stat.MovSpeed);
        }

        float slowRadius = movSpeed * 0.5f;
        float stopRange = m_DataManager.QueryStat(Stat.AtkRange);
        float visRange = m_DataManager.QueryStat(Stat.VisRange);

        Dictionary<Guid, Rigidbody> nearbyEnemies = m_DataManager.QueryNeighbours(Team.Enemy);
        KeyValuePair<Guid, Rigidbody> targetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);
        Vector3 movTarget = m_DataManager.QueryNextMovTarget();

        if (nearbyEnemies != null && !m_Incombat)
        {
            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.Key != targetEnemy.Key)
                {
                    desiredVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, movSpeed, visRange) * m_WeightManager.QueryWeight(Weight.EnemySeparation);
                }
            }
        }

        if (targetEnemy.Value != null)
        {
            if (Vector3.Distance(targetEnemy.Value.position, m_Rigidbody.position) > visRange)
            {
                desiredVelocity += SteeringBehaviours.Pursue(targetEnemy.Value, m_Rigidbody.position, movSpeed, slowRadius, stopRange) * m_WeightManager.QueryWeight(Weight.EnemyCohesion);
                m_Incombat = false;
            }
            else if (Vector3.Distance(targetEnemy.Value.position, m_Rigidbody.position) > stopRange)
            {
                desiredVelocity += SteeringBehaviours.Arrive(targetEnemy.Value.position, m_Rigidbody.position, movSpeed, slowRadius, stopRange);
                m_Incombat = false;
            }
            else 
            {
                m_Incombat = true;
            }
        }

        if (movTarget != Vector3.zero)
        {
            desiredVelocity += SteeringBehaviours.Arrive(movTarget, m_Rigidbody.position, movSpeed, slowRadius, stopRange);
        }

        UpdateBoid(desiredVelocity);
    }

    private void UpdateBoid(Vector3 _DesiredVelocity)
    {
        OnBehaviourUpdate.Invoke(_DesiredVelocity);
    }
}

//GameObject targetObject = BoidPool.Instance.GetActiveBoid(_TargetID);
//if (targetObject != null)
//{
//    targetPos = targetObject.transform.position;
//}