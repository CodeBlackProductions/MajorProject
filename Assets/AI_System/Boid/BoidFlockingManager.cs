using System;
using UnityEngine;

[RequireComponent(typeof(BoidFlockingWeightManager))]
[RequireComponent(typeof(BoidDataManager))]
public class BoidFlockingManager : MonoBehaviour
{
    private BoidFlockingWeightManager m_WeightManager;
    private BoidDataManager m_DataManager;

    public Action<Vector3> OnBehaviourUpdate;

    private float m_FlockingTimer;
    private float m_FlockingTime = 0.1f;

    private Rigidbody m_Rigidbody;

    private void Awake()
    {
        m_WeightManager = GetComponent<BoidFlockingWeightManager>();
        m_DataManager = GetComponent<BoidDataManager>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_FlockingTimer = m_FlockingTime;
        
    }

    private void Update()
    {
        if (m_FlockingTimer <= 0)
        {
            Vector3 desiredVelocity = Vector3.zero;
            desiredVelocity += SteeringBehaviours.Pursue();
            desiredVelocity += SteeringBehaviours.Evade();

            UpdateBoid(desiredVelocity);
            m_FlockingTimer = m_FlockingTime;
        }
        else
        {
            m_FlockingTimer -= Time.deltaTime;
        }
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

//Vector3[] NeighbourPositions = _NeighbourIDs.AsParallel().Select(n => BoidPool.Instance.GetActiveBoid(n).transform.position).ToArray();

//For Pursue:
//time = targetDist / targetSpeed;