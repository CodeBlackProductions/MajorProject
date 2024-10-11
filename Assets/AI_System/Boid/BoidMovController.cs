using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoidFlockingManager))]
public class BoidMovController : MonoBehaviour
{
    [SerializeField] private bool m_ShowDebug = false;

    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 m_Facing = Vector3.zero;
    private Rigidbody m_rigidbody;
    private BoidDataManager m_DataManager;

    public Vector3 Velocity { get => m_Velocity; set => m_Velocity = value; }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_DataManager = GetComponent<BoidDataManager>();
        GetComponent<BoidFlockingManager>().OnBehaviourUpdate += UpdateVelocity;
    }

    private void OnDisable()
    {
        GetComponent<BoidFlockingManager>().OnBehaviourUpdate -= UpdateVelocity;
    }

    private void FixedUpdate()
    {
        if (float.IsNaN(m_Velocity.x) || float.IsNaN(m_Velocity.y) || float.IsNaN(m_Velocity.z))
        {
            throw new Exception("Velocity should not be NaN when trying to move!");
        }

        m_rigidbody.velocity = m_Velocity;
     
        m_rigidbody.transform.forward = m_Facing;
    }

    private void UpdateVelocity(Vector3 _DesiredVelocity, Vector3 _DesiredFacing)
    {
        Vector3 steeringVelocity = _DesiredVelocity - m_Velocity;
        float maxSteering = m_DataManager.QueryStat(BoidStat.TurnRate);
        float maxVelocity = m_DataManager.QueryStat(BoidStat.MovSpeed);

        if (steeringVelocity.magnitude > maxSteering)
        {
            steeringVelocity.Normalize();
            steeringVelocity *= maxSteering;
        }
        steeringVelocity /= m_DataManager.QueryStat(BoidStat.Mass);
        m_Velocity += steeringVelocity;
        if (m_Velocity.magnitude > maxVelocity)
        {
            m_Velocity.Normalize();
            m_Velocity *= maxVelocity;
        }

        if (_DesiredFacing != Vector3.zero)
        {
            m_Facing = _DesiredFacing;
        }
    }

    private void OnDrawGizmos()
    {
        if (m_ShowDebug)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(BoidStat.VisRange));
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(BoidStat.AtkRange));
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(BoidStat.MovSpeed) * 0.5f);
            Gizmos.color = Color.white;
        }
    }
}