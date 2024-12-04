using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
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
    private bool m_AvoidingObstacle = false;
    private Vector3 m_CurrentVelocity;
    private Vector3 m_CurrentFacing;

    private KeyValuePair<Guid, Rigidbody> m_CurrentTargetEnemy;

    private Color m_DebugBaseColor;

    private void Awake()
    {
        m_WeightManager = GetComponent<BoidFlockingWeightManager>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_FlowfieldManager = FlowfieldManager.Instance;

        m_CurrentVelocity = m_Rigidbody.velocity;
        m_CurrentFacing = m_Rigidbody.transform.forward;
    }

    //private void Start()
    //{
    //    m_DebugBaseColor = GetComponent<MeshRenderer>().material.color;
    //}

    private void Update()
    {
        Vector3 desiredVelocity = Vector3.zero;

        float movSpeed = m_DataManager.QueryStat(BoidStat.MovSpeed);
        float stopRange = m_DataManager.QueryStat(BoidStat.AtkRange);
        float visRange = m_DataManager.QueryStat(BoidStat.VisRange);
        float slowRadius = visRange * 0.5f;

        List<KeyValuePair<Guid, Rigidbody>> nearbyEnemies = m_DataManager.QueryNeighbours(Team.Enemy);

        if (m_CurrentTargetEnemy.Value == null || !m_CurrentTargetEnemy.Value.gameObject.activeSelf || Vector3.Distance(m_CurrentTargetEnemy.Value.position,transform.position) > stopRange)
        {
            m_CurrentTargetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);
        }

        List<KeyValuePair<Guid, Rigidbody>> nearbyAllies = m_DataManager.QueryNeighbours(Team.Ally);
        Vector3[] nearbyObstacles = m_DataManager.QueryObstaclePositions();
        Vector3 movTarget = m_DataManager.QueryNextMovTarget();

        Vector3 formationPos = m_DataManager.FormationPosition;

        Vector2[,] flowfield = null;

        if (movTarget != Vector3.zero)
        {
            Vector2Int targetPos = new Vector2Int((int)(movTarget.x / GridDataManager.Instance.CellSize), (int)(movTarget.z / GridDataManager.Instance.CellSize));
            if (m_FlowfieldManager != null)
            {
                flowfield = m_DataManager.QueryFlowfield();
            }
        }

        m_AvoidingObstacle = false;
        for (int i = 0; i < nearbyObstacles.Length; i++)
        {
            if (Vector3.Distance(nearbyObstacles[i], m_Rigidbody.position) <= slowRadius)
            {
                m_AvoidingObstacle = true;
                break;
            }
        }

        //if (m_AvoidingObstacle)
        //{
        //    GetComponent<MeshRenderer>().material.color = Color.yellow;
        //}
        //else
        //{
        //    GetComponent<MeshRenderer>().material.color = m_DebugBaseColor;
        //}

        if (m_Incombat)
        {
            desiredVelocity = InCombatBehaviour(nearbyAllies, nearbyEnemies, m_CurrentTargetEnemy, formationPos, movSpeed, slowRadius, stopRange, visRange);
        }
        else
        {
            desiredVelocity = OutOfCombatBehaviour(nearbyAllies, nearbyEnemies, m_CurrentTargetEnemy, movTarget, flowfield, formationPos, movSpeed, slowRadius, stopRange, visRange);
        }

        desiredVelocity.y = Vector3.forward.y;

        UpdateBoid(desiredVelocity, m_CurrentTargetEnemy);
    }

    private Vector3 OutOfCombatBehaviour(List<KeyValuePair<Guid, Rigidbody>> _NearbyAllies, List<KeyValuePair<Guid, Rigidbody>> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, Vector3 _MovTarget, Vector2[,] _Flowfield, Vector3 _FormationPos, float _MovSpeed, float _SlowRadius, float _StopRange, float _VisRange)
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

        if (_Flowfield != null)
        {
            int x = (int)Mathf.Floor(pos.x / GridDataManager.Instance.CellSize);
            int y = (int)Mathf.Floor(pos.z / GridDataManager.Instance.CellSize);
            Vector2 dir = _Flowfield[x, y];

            Vector3 flowDir = new Vector3(dir.x, transform.position.y, dir.y) * _MovSpeed * m_WeightManager.QueryWeight(Weight.MovTarget) * 0.5f;
            flowDir.y = transform.position.y;

            movementVelocity += flowDir;
        }
        else if (_MovTarget != Vector3.zero)
        {
            movementVelocity += SteeringBehaviours.Arrive(_MovTarget, pos, _MovSpeed, _SlowRadius, _StopRange) * m_WeightManager.QueryWeight(Weight.MovTarget) * 0.5f;
        }

        if (_FormationPos != Vector3.zero && !float.IsNaN(_FormationPos.x) && !float.IsNaN(_FormationPos.y) && !float.IsNaN(_FormationPos.z) && !m_AvoidingObstacle)
        {
            movementVelocity += SteeringBehaviours.FormationCohesion(_FormationPos, pos, _VisRange, _MovSpeed) * m_WeightManager.QueryWeight(Weight.FormationCohesion);
        }

        if (float.IsNaN(movementVelocity.x) || float.IsNaN(movementVelocity.y) || float.IsNaN(movementVelocity.z))
        {
            movementVelocity = Vector3.zero;
        }

        return movementVelocity;
    }

    private Vector3 InCombatBehaviour(List<KeyValuePair<Guid, Rigidbody>> _NearbyAllies, List<KeyValuePair<Guid, Rigidbody>> _NearbyEnemies, KeyValuePair<Guid, Rigidbody> _TargetEnemy, Vector3 _FormationPos, float _MovSpeed, float _SlowRadius, float _StopRange, float _VisRange)
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
        else
        {
            m_Incombat = false;
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
            _NearbyEnemies,
            pos,
            _VisRange,
            _MovSpeed,
            m_WeightManager.QueryWeight(Weight.FEnemyCohesion),
            m_WeightManager.QueryWeight(Weight.FEnemySeparation),
            m_WeightManager.QueryWeight(Weight.FEnemyAlignment));
        }

        if (_FormationPos != Vector3.zero && !float.IsNaN(_FormationPos.x) && !float.IsNaN(_FormationPos.y) && !float.IsNaN(_FormationPos.z) && !m_AvoidingObstacle)
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