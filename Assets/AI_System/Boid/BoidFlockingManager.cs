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

        float movSpeed = m_DataManager.QueryStat(Stat.MovSpeed);
        float slowRadius = movSpeed * 0.5f;
        float stopRange = m_DataManager.QueryStat(Stat.AtkRange);
        float visRange = m_DataManager.QueryStat(Stat.VisRange);

        Dictionary<Guid, Rigidbody> nearbyEnemies = m_DataManager.QueryNeighbours(Team.Enemy);
        KeyValuePair<Guid, Rigidbody> targetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);
        Dictionary<Guid, Rigidbody> nearbyAllies = m_DataManager.QueryNeighbours(Team.Ally);
        Vector3 movTarget = m_DataManager.QueryNextMovTarget();

        if (nearbyAllies != null)
        {
            desiredVelocity += SteeringBehaviours.Flock(
                nearbyAllies,
                m_Rigidbody.position,
                m_WeightManager.QueryWeight(Weight.AllyCohesion),
                m_WeightManager.QueryWeight(Weight.AllySeparation),
                m_WeightManager.QueryWeight(Weight.AllyAlignment));
        }

        if (nearbyEnemies != null && !m_Incombat)
        {
            foreach (var enemy in nearbyEnemies)
            {
                if (enemy.Key != targetEnemy.Key)
                {
                    if (Vector3.Distance(enemy.Value.position, m_Rigidbody.position) > visRange)
                    {
                        desiredVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, movSpeed, visRange) * m_WeightManager.QueryWeight(Weight.EnemySeparation) * 0.5f;
                    }
                    else if (Vector3.Distance(enemy.Value.position, m_Rigidbody.position) > slowRadius)
                    {
                        desiredVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, movSpeed, visRange) * m_WeightManager.QueryWeight(Weight.EnemySeparation);
                    }
                    else
                    {
                        desiredVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, movSpeed, visRange) * m_WeightManager.QueryWeight(Weight.EnemySeparation) * 0.25f;
                    }
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
            desiredVelocity += SteeringBehaviours.Arrive(movTarget, m_Rigidbody.position, movSpeed, slowRadius, stopRange) * m_WeightManager.QueryWeight(Weight.TargetCohesion);
        }

        if (m_Incombat) 
        {
            desiredVelocity *= 0.1f;
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