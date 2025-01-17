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

    private float m_maxSteering = 0;
    private float m_MaxVelocity = 0;
    private float m_Mass = 0;

    public Vector3 Velocity { get => m_Velocity; set => m_Velocity = value; }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_DataManager = GetComponent<BoidDataManager>();
        GetComponent<BoidFlockingManager>().OnBehaviourUpdate += UpdateVelocity;
    }

    private void Start()
    {
        m_maxSteering = m_DataManager.QueryStat(BoidStat.TurnRate);
        m_MaxVelocity = m_DataManager.QueryStat(BoidStat.MovSpeed);
        m_Mass = m_DataManager.QueryStat(BoidStat.Mass);
    }

    private void OnDisable()
    {
        GetComponent<BoidFlockingManager>().OnBehaviourUpdate -= UpdateVelocity;
    }

    private void FixedUpdate()
    {
        if (!float.IsNaN(m_Velocity.x) && !float.IsNaN(m_Velocity.y) && !float.IsNaN(m_Velocity.z))
        {
            m_rigidbody.velocity = m_Velocity;
        }
        else
        {
            m_Velocity = Vector3.zero;
        }

        if (!float.IsNaN(m_Facing.x) && !float.IsNaN(m_Facing.y) && !float.IsNaN(m_Facing.z))
        {
            m_rigidbody.transform.forward = m_Facing;
        }
        else
        {
            m_Facing = Vector3.zero;
        }
    }

    private void UpdateVelocity(Vector3 _DesiredVelocity, Vector3 _DesiredFacing)
    {
        Vector3 steeringVelocity = _DesiredVelocity - m_Velocity;

        if (steeringVelocity.magnitude > m_maxSteering)
        {
            steeringVelocity.Normalize();
            steeringVelocity *= m_maxSteering;
        }
        steeringVelocity /= m_Mass;
        m_Velocity += steeringVelocity;

        if (m_Velocity.magnitude > m_MaxVelocity)
        {
            m_Velocity.Normalize();
            m_Velocity *= m_MaxVelocity;
        }

        if (_DesiredFacing != Vector3.zero)
        {
            m_Facing = Vector3.Slerp(m_Facing, _DesiredFacing, 0.5f);
            m_Facing.y = Vector3.forward.y; ;
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