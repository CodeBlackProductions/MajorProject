using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoidFlockingWeightManager))]
[RequireComponent(typeof(BoidDataManager))]
public class BoidFlockingManager : MonoBehaviour
{
    private BoidFlockingWeightManager m_WeightManager;
    private BoidDataManager m_DataManager;
    private FlowfieldManager m_FlowfieldManager;
    public Action<Vector3, Vector3> OnBehaviourUpdate;

    private Rigidbody m_Rigidbody;
    private bool m_Incombat = false;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_CurrentFacing;

    private void Awake()
    {
        m_WeightManager = GetComponent<BoidFlockingWeightManager>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_FlowfieldManager = FlowfieldManager.Instance;

        m_CurrentVelocity = m_Rigidbody.velocity;
        m_CurrentFacing = m_Rigidbody.transform.forward;
    }

    private void Update()
    {
        Vector3 desiredVelocity = Vector3.zero;

        float movSpeed = m_DataManager.QueryStat(BoidStat.MovSpeed);
        float stopRange = m_DataManager.QueryStat(BoidStat.AtkRange);
        float visRange = m_DataManager.QueryStat(BoidStat.VisRange);
        float slowRadius = visRange * 0.5f;
        float obstacleAvoidanceWeight = m_WeightManager.QueryWeight(Weight.ObstacleAvoidance);

        Dictionary<Guid, Rigidbody> nearbyEnemies = m_DataManager.QueryNeighbours(Team.Enemy);
        KeyValuePair<Guid, Rigidbody> targetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);
        Dictionary<Guid, Rigidbody> nearbyAllies = m_DataManager.QueryNeighbours(Team.Ally);
        Vector3[] nearbyObstacles = m_DataManager.QueryObstaclePositions();
        float[] obstacleSizes = m_DataManager.QueryObstacleSizes();
        Vector3 movTarget = m_DataManager.QueryNextMovTarget();

        Vector3 formationPos = m_DataManager.FormationPosition;
        Vector3 avoidance = Vector3.zero;

        Vector2[,] flowfield = null;

        if (movTarget != Vector3.zero)
        {
            Vector2Int targetPos = new Vector2Int((int)(movTarget.x / GridDataManager.Instance.CellSize), (int)(movTarget.z / GridDataManager.Instance.CellSize));
            if (m_FlowfieldManager != null)
            {
                flowfield = m_DataManager.QueryFlowfield();
            }
        }

        avoidance = SteeringBehaviours.ObstacleAvoidance(nearbyObstacles, obstacleSizes, m_Rigidbody.position, m_Rigidbody.velocity, visRange, movSpeed, 1, 30) * obstacleAvoidanceWeight;

        if (avoidance != Vector3.zero)
        {
            desiredVelocity += avoidance;

            if (flowfield != null)
            {
                int x = (int)Mathf.Floor(m_Rigidbody.position.x / GridDataManager.Instance.CellSize);
                int y = (int)Mathf.Floor(m_Rigidbody.position.z / GridDataManager.Instance.CellSize);
                Vector2 dir = flowfield[x, y];
                desiredVelocity += new Vector3(dir.x, transform.position.y, dir.y) * movSpeed * m_WeightManager.QueryWeight(Weight.MovTarget) * 2;
            }
        }

        if (m_Incombat)
        {
            desiredVelocity = InCombat(nearbyAllies, nearbyEnemies, targetEnemy, formationPos, movSpeed, slowRadius, stopRange, visRange);
        }
        else
        {
            desiredVelocity = OutOfCombat(nearbyAllies, nearbyEnemies, targetEnemy, movTarget, flowfield, formationPos, movSpeed, slowRadius, stopRange, visRange);
        }

        UpdateBoid(desiredVelocity, targetEnemy);
    }

    private Vector3 OutOfCombat(Dictionary<Guid, Rigidbody> _NearbyAllies, Dictionary<Guid, Rigidbody> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, Vector3 _MovTarget, Vector2[,] _Flowfield, Vector3 _FormationPos, float _MovSpeed, float _SlowRadius, float _StopRange, float _VisRange)
    {
        Vector3 movementVelocity = Vector3.zero;
        Vector3 pos = m_Rigidbody.position;

        if (_TargetEnemy.Value != null)
        {
            if (Vector3.Distance(_TargetEnemy.Value.position, pos) < _StopRange * 2f)
            {
                m_Incombat = true;
            }
            movementVelocity += SteeringBehaviours.Pursue(_TargetEnemy.Value, pos, _MovSpeed, _SlowRadius, _StopRange) * m_WeightManager.QueryWeight(Weight.EnemyPursue);
        }

        if (_NearbyEnemies != null && _NearbyEnemies.Count > 0)
        {
            Vector3 enemyAvoidance = Vector3.zero;
            foreach (var enemy in _NearbyEnemies)
            {
                if (enemy.Key != _TargetEnemy.Key)
                {
                    if (Vector3.Distance(enemy.Value.position, pos) > _StopRange)
                    {
                        enemyAvoidance += SteeringBehaviours.Evade(enemy.Value, pos, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.EnemyAvoidance) * 0.5f;
                    }
                    else
                    {
                        enemyAvoidance += SteeringBehaviours.Evade(enemy.Value, pos, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.EnemyAvoidance);
                    }
                }
            }

            enemyAvoidance /= _NearbyEnemies.Count - 1;
            movementVelocity += enemyAvoidance;
        }

        if (_NearbyAllies != null && _NearbyAllies.Count > 0)
        {
            foreach (var ally in _NearbyAllies)
            {
                if (Vector3.Distance(_FormationPos, pos) > _SlowRadius && Vector3.Distance(ally.Value.position, pos) > _SlowRadius)
                {
                    movementVelocity += SteeringBehaviours.Evade(ally.Value, pos, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.FAllySeparation);
                }
                else if (Vector3.Distance(ally.Value.position, pos) < _SlowRadius * 0.25f)
                {
                    movementVelocity += SteeringBehaviours.Evade(ally.Value, pos, _MovSpeed, _VisRange) * m_WeightManager.QueryWeight(Weight.FAllySeparation);
                }
            }
        }

        if (_Flowfield != null)
        {
            int x = (int)Mathf.Floor(pos.x / GridDataManager.Instance.CellSize);
            int y = (int)Mathf.Floor(pos.z / GridDataManager.Instance.CellSize);
            Vector2 dir = _Flowfield[x, y];
            movementVelocity += new Vector3(dir.x, transform.position.y, dir.y) * _MovSpeed * m_WeightManager.QueryWeight(Weight.MovTarget) * 0.5f;
        }
        else if (_MovTarget != Vector3.zero)
        {
            movementVelocity += SteeringBehaviours.Arrive(_MovTarget, pos, _MovSpeed, _SlowRadius, _StopRange) * m_WeightManager.QueryWeight(Weight.MovTarget) * 0.5f;
        }

        if (_FormationPos != Vector3.zero && !float.IsNaN(_FormationPos.x) && !float.IsNaN(_FormationPos.y) && !float.IsNaN(_FormationPos.z))
        {
            movementVelocity += SteeringBehaviours.FormationCohesion(_FormationPos, pos, _VisRange, _MovSpeed) * m_WeightManager.QueryWeight(Weight.FormationCohesion);
        }

        if (float.IsNaN(movementVelocity.x) || float.IsNaN(movementVelocity.y) || float.IsNaN(movementVelocity.z))
        {
            movementVelocity = Vector3.zero;
        }

        return movementVelocity;
    }

    private Vector3 InCombat(Dictionary<Guid, Rigidbody> _NearbyAllies, Dictionary<Guid, Rigidbody> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, Vector3 _FormationPos, float _MovSpeed, float _SlowRadius, float _StopRange, float _VisRange)
    {
        Vector3 combatVelocity = Vector3.zero;
        Vector3 pos = m_Rigidbody.position;

        if (_TargetEnemy.Value != null)
        {
            if (Vector3.Distance(_TargetEnemy.Value.position, pos) > _StopRange * 2f)
            {
                m_Incombat = false;
            }
            combatVelocity += SteeringBehaviours.Arrive(_TargetEnemy.Value.position, pos, _MovSpeed, _SlowRadius, _StopRange);
        }

        if (_NearbyAllies != null && _NearbyAllies.Count > 0)
        {
            combatVelocity += SteeringBehaviours.Flock(
                _NearbyAllies,
                pos,
                _VisRange,
                _MovSpeed,
                m_WeightManager.QueryWeight(Weight.FAllyCohesion),
                m_WeightManager.QueryWeight(Weight.FAllySeparation),
                m_WeightManager.QueryWeight(Weight.FAllyAlignment));
        }

        if (_NearbyEnemies != null && _NearbyEnemies.Count > 0)
        {
            combatVelocity += SteeringBehaviours.Flock(
                _NearbyAllies,
                pos,
                _VisRange,
                _MovSpeed,
                m_WeightManager.QueryWeight(Weight.FEnemyCohesion),
                m_WeightManager.QueryWeight(Weight.FEnemySeparation),
                m_WeightManager.QueryWeight(Weight.FEnemyAlignment));
        }

        if (_FormationPos != Vector3.zero && !float.IsNaN(_FormationPos.x) && !float.IsNaN(_FormationPos.y) && !float.IsNaN(_FormationPos.z))
        {
            combatVelocity += SteeringBehaviours.FormationCohesion(_FormationPos, pos, _VisRange, _MovSpeed) * m_WeightManager.QueryWeight(Weight.FormationCohesion) * 0.5f;
        }

        combatVelocity *= 0.3f;

        if (float.IsNaN(combatVelocity.x) || float.IsNaN(combatVelocity.y) || float.IsNaN(combatVelocity.z))
        {
            combatVelocity = Vector3.zero;
        }

        return combatVelocity;
    }

    private void UpdateBoid(Vector3 _DesiredVelocity, KeyValuePair<Guid, Rigidbody> _TargetEnemy)
    {
        Vector3 desiredFacing = Vector3.zero;
        if (m_Incombat && _TargetEnemy.Value != null)
        {
            desiredFacing = (_TargetEnemy.Value.position - m_Rigidbody.position).normalized;
        }
        else
        {
            desiredFacing = new Vector3(m_Rigidbody.velocity.normalized.x, m_Rigidbody.transform.forward.y, m_Rigidbody.velocity.normalized.z);
        }

        Vector3 smoothedVelocity = Vector3.SmoothDamp(m_Rigidbody.velocity, _DesiredVelocity, ref m_CurrentVelocity, 0.1f);
        Vector3 smoothedFacing = Vector3.SmoothDamp(m_Rigidbody.transform.forward, desiredFacing, ref m_CurrentFacing, 0.1f);
        OnBehaviourUpdate.Invoke(smoothedVelocity, desiredFacing);
    }
}