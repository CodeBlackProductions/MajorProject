using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseBoid : MonoBehaviour
{
    [SerializeField] private Rigidbody m_Target;
    [SerializeField] private Rigidbody m_Enemy;
    private Vector3 m_Velocity = Vector3.zero;
    private Vector3 m_SteeringVelocity = Vector3.zero;
    private Vector3 m_DesiredVelocity = Vector3.zero;
    private float m_MaxVelocity = 20;
    private float m_MaxSteering = 0.2f;
    private float m_mass = 1;
    private Rigidbody m_Rigidbody;
    private float m_SlowRadius = 10;
    private float m_minimumTargetDistance = 2;
    private float m_VisionRadius = 25;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private Vector3 Seek(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity)
    {
        return (_TargetPos - _Pos).normalized * _MaxVelocity;
    }

    private Vector3 Flee(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity)
    {
        return (_Pos - _TargetPos).normalized * _MaxVelocity;
    }

    private Vector3 Arrive(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity, float _SlowRadius, float _MinimumTargetDist)
    {
        Vector3 desiredVelocity = Vector3.zero;

        desiredVelocity += Seek(_TargetPos, _Pos, _MaxVelocity);
        float dist = (_TargetPos - _Pos).magnitude;

        if (dist < _SlowRadius)
        {
            desiredVelocity *= (dist - _MinimumTargetDist) / _SlowRadius;
        }

        return desiredVelocity;
    }

    private Vector3 Avoid(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity, float _VisionRadius)
    {
        Vector3 desiredVelocity = Vector3.zero;

        desiredVelocity += Flee(_TargetPos, _Pos, _MaxVelocity);
        float dist = (_TargetPos - _Pos).magnitude;

        if (dist < _VisionRadius)
        {
            desiredVelocity *= 1 - (dist / _VisionRadius);
        }
        else
        {
            desiredVelocity = Vector3.zero;
        }

        return desiredVelocity;
    }

    private Vector3 Pursue(Rigidbody _Target, Vector3 _Pos, float _MaxVelocity, float _SlowRadius, float _MinimumTargetDist)
    {
        Vector3 futurePos = Vector3.zero;
        Vector3 distance = Vector3.zero;
        float time = 0;

        distance = _Target.position - _Pos;
        time = distance.magnitude / _MaxVelocity;

        futurePos = _Target.position + _Target.velocity * time;

        return Arrive(futurePos, _Pos, _MaxVelocity, _SlowRadius, _MinimumTargetDist);
    }

    private Vector3 Evade(Rigidbody _Target, Vector3 _Pos, float _MaxVelocity, float _VisionRadius)
    {
        Vector3 futurePos = Vector3.zero;
        Vector3 distance = Vector3.zero;
        float time = 0;

        distance = _Target.position - _Pos;
        time = distance.magnitude / _MaxVelocity;
        futurePos = _Target.position + _Target.velocity * time;

        return Avoid(futurePos, _Pos, _MaxVelocity, _VisionRadius);
    }

    private void CalculateFinalVelocity()
    {
        m_SteeringVelocity = m_DesiredVelocity - m_Velocity;

        if (m_SteeringVelocity.magnitude > m_MaxSteering)
        {
            m_SteeringVelocity.Normalize();
            m_SteeringVelocity *= m_MaxSteering;
        }
        m_SteeringVelocity /= m_mass;

        m_Velocity += m_SteeringVelocity;
        if (m_Velocity.magnitude > m_MaxVelocity)
        {
            m_Velocity.Normalize();
            m_Velocity *= m_MaxVelocity;
        }
    }

    private void Update()
    {
        m_DesiredVelocity = Vector3.zero;

        m_DesiredVelocity += Pursue(m_Target, transform.position, m_MaxVelocity, m_SlowRadius, m_minimumTargetDistance);
        m_DesiredVelocity += Evade(m_Enemy, transform.position, m_MaxVelocity, m_VisionRadius);
        CalculateFinalVelocity();

        m_Rigidbody.velocity = m_Velocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, m_VisionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, m_SlowRadius);
        Gizmos.color = Color.white;
    }
}