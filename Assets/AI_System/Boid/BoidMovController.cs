using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoidFlockingManager))]
public class BoidMovController : MonoBehaviour
{
    private Vector3 m_Velocity = Vector3.zero;
    private Rigidbody m_rigidbody;
    private float m_MaxSteering = 0.2f;
    public Vector3 Velocity { get => m_Velocity; set => m_Velocity = value; }

    private void Awake()
    {
        m_rigidbody = GetComponent<Rigidbody>();
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

        if (steeringVelocity.magnitude > m_MaxSteering)
        {
            steeringVelocity.Normalize();
            steeringVelocity *= m_MaxSteering;
        }
        steeringVelocity /= _Mass;

        m_Velocity += steeringVelocity;
        if (m_Velocity.magnitude > _Speed)
        {
            m_Velocity.Normalize();
            m_Velocity *= _Speed;
        }
    }

}