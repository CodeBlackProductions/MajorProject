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

        if (m_Incombat)
        {
            desiredVelocity = InCombat(nearbyAllies, nearbyEnemies, targetEnemy, movSpeed, slowRadius, stopRange);
        }
        else
        {
            desiredVelocity = OutOfCombat(nearbyAllies, nearbyEnemies, targetEnemy, movTarget, movSpeed, slowRadius, stopRange, visRange);
        }

        UpdateBoid(desiredVelocity);
    }

    private Vector3 OutOfCombat(Dictionary<Guid, Rigidbody> _NearbyAllies, Dictionary<Guid, Rigidbody> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, Vector3 _MovTarget, float _MovSpeed, float _SlowRadius, float _StopRange, float _VisRange)
    {
        Vector3 movementVelocity = Vector3.zero;

        if (_TargetEnemy.Value != null)
        {
            if (Vector3.Distance(_TargetEnemy.Value.position, m_Rigidbody.position) > _VisRange)
            {
                movementVelocity += SteeringBehaviours.Pursue(_TargetEnemy.Value, m_Rigidbody.position, _MovSpeed, _SlowRadius, _StopRange) * m_WeightManager.QueryWeight(Weight.EnemyPursue);
            }
            else if (Vector3.Distance(_TargetEnemy.Value.position, m_Rigidbody.position) > _StopRange)
            {
                movementVelocity += SteeringBehaviours.Arrive(_TargetEnemy.Value.position, m_Rigidbody.position, _MovSpeed, _SlowRadius, _StopRange);
            }
            else
            {
                m_Incombat = true;
                return movementVelocity;
            }
        }

        if (_NearbyAllies != null)
        {
            movementVelocity += SteeringBehaviours.Flock(
                _NearbyAllies,
                m_Rigidbody.position,
                m_WeightManager.QueryWeight(Weight.FAllyCohesion),
                m_WeightManager.QueryWeight(Weight.FAllySeparation),
                m_WeightManager.QueryWeight(Weight.FAllyAlignment));
        }

        if (_NearbyEnemies != null)
        {
            foreach (var enemy in _NearbyEnemies)
            {
                if (enemy.Key != _TargetEnemy.Key)
                {
                    if (Vector3.Distance(enemy.Value.position, m_Rigidbody.position) > _VisRange)
                    {
                        movementVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.EnemyAvoidance) * 0.5f;
                    }
                    else if (Vector3.Distance(enemy.Value.position, m_Rigidbody.position) > _SlowRadius)
                    {
                        movementVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.EnemyAvoidance);
                    }
                    else
                    {
                        movementVelocity += SteeringBehaviours.Evade(enemy.Value, m_Rigidbody.position, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.EnemyAvoidance) * 0.25f;
                    }
                }
            }
        }

        if (_MovTarget != Vector3.zero)
        {
            movementVelocity += SteeringBehaviours.Arrive(_MovTarget, m_Rigidbody.position, _MovSpeed, _SlowRadius, _StopRange) * m_WeightManager.QueryWeight(Weight.MovTarget);
        }

        return movementVelocity;
    }

    private Vector3 InCombat(Dictionary<Guid, Rigidbody> _NearbyAllies, Dictionary<Guid, Rigidbody> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, float _MovSpeed, float _SlowRadius, float _StopRange)
    {
        Vector3 combatVelocity = Vector3.zero;

        if (_TargetEnemy.Value != null)
        {
            if (Vector3.Distance(_TargetEnemy.Value.position, m_Rigidbody.position) > _StopRange)
            {
                m_Incombat = false;
                return combatVelocity;
            }
            combatVelocity += SteeringBehaviours.Arrive(_TargetEnemy.Value.position, m_Rigidbody.position, _MovSpeed, _SlowRadius, _StopRange);
        }

        if (_NearbyAllies != null)
        {
            combatVelocity += SteeringBehaviours.Flock(
                _NearbyAllies,
                m_Rigidbody.position,
                m_WeightManager.QueryWeight(Weight.FAllyCohesion),
                m_WeightManager.QueryWeight(Weight.FAllySeparation),
                m_WeightManager.QueryWeight(Weight.FAllyAlignment));
        }

        if (_NearbyEnemies != null)
        {
            combatVelocity += SteeringBehaviours.Flock(
                _NearbyAllies,
                m_Rigidbody.position,
                m_WeightManager.QueryWeight(Weight.FEnemyCohesion),
                m_WeightManager.QueryWeight(Weight.FEnemySeparation),
                m_WeightManager.QueryWeight(Weight.FEnemyAlignment));
        }

        combatVelocity *= 0.2f;

        return combatVelocity;
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