using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoidFlockingManager))]
public class BoidMovController : MonoBehaviour
{
    private Vector3 m_Velocity = Vector3.zero;
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
        if (m_Velocity.normalized != Vector3.zero)
        {
            m_rigidbody.transform.forward = m_Velocity.normalized;
        }

        m_rigidbody.velocity = m_Velocity;
    }

    private void UpdateVelocity(Vector3 _DesiredVelocity)
    {
       Vector3 steeringVelocity = _DesiredVelocity - m_Velocity;
        float maxSteering = m_DataManager.QueryStat(Stat.TurnRate);
        float maxVelocity = m_DataManager.QueryStat(Stat.MovSpeed);

        if (steeringVelocity.magnitude > maxSteering)
        {
            steeringVelocity.Normalize();
            steeringVelocity *= maxSteering;
        }
        steeringVelocity /= m_DataManager.QueryStat(Stat.Mass);

        m_Velocity += steeringVelocity;
        if (m_Velocity.magnitude > maxVelocity)
        {
            m_Velocity.Normalize();
            m_Velocity *= maxVelocity;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(Stat.VisRange));
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(Stat.AtkRange));
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, m_DataManager.QueryStat(Stat.MovSpeed) * 0.5f);
        Gizmos.color = Color.white;
    }
}