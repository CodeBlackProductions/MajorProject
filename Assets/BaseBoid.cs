using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BaseBoid : MonoBehaviour
{
    [SerializeField] private Rigidbody m_Target;
    [SerializeField] private Rigidbody m_Enemy;
    [SerializeField] private Transform[] m_ObstacleTransforms;
    [SerializeField] private Transform[] m_NeighbourTransforms;
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
    private Vector3[] m_ObstaclePositions;
    private float[] m_ObstacleSizes;
    private Vector3[] m_NeighbourPositions;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        m_ObstaclePositions = new Vector3[m_ObstacleTransforms.Length];
        m_ObstacleSizes = new float[m_ObstacleTransforms.Length];
        for (int i = 0; i < m_ObstacleTransforms.Length; i++)
        {
            m_ObstaclePositions[i] = m_ObstacleTransforms[i].position;
            m_ObstacleSizes[i] = 4.5f;
        }

        if (m_NeighbourTransforms.Length > 0)
        {
            m_NeighbourPositions = new Vector3[m_NeighbourTransforms.Length];
            for (int i = 0; i < m_NeighbourTransforms.Length; i++)
            {
                m_NeighbourPositions[i] = m_NeighbourTransforms[i].position;
            }
        }
    }

    private Vector3 Cohesion(Vector3[] _NeighbourPositions, Vector3 _Pos)
    {
        Vector3 cohesion = Vector3.zero;
        for (int i = 0; i < _NeighbourPositions.Length; i++)
        {
            cohesion += _NeighbourPositions[i] - _Pos;
        }
        cohesion /= _NeighbourPositions.Length;
        return cohesion;
    }

    private Vector3 Separation(Vector3[] _NeighbourPositions, Vector3 _Pos)
    {
        return Cohesion(_NeighbourPositions, _Pos) * -1;
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

    private Vector3 ObstacleAvoidance(Vector3[] _Obstacles, float[] _ObstacleSizes, Vector3 _Pos, Vector3 _Velocity, float _VisionRange, float _MaxVelocity, float _AvoidanceRangeFactor, float _AvoidanceForce)
    {
        float speedFactor = _Velocity.magnitude / _MaxVelocity;
        Vector3 vision = _Pos + _Velocity.normalized * (_VisionRange * _AvoidanceRangeFactor * speedFactor);
        Vector3 halfVision = vision * 0.5f;

        List<KeyValuePair<Vector3, float>> collidingObstacles = new List<KeyValuePair<Vector3, float>>();
        for (int i = 0; i < _Obstacles.Length; i++)
        {
            if (Collides(_Obstacles[i], _ObstacleSizes[i], vision) || Collides(_Obstacles[i], _ObstacleSizes[i], halfVision) || Collides(_Obstacles[i], _ObstacleSizes[i], _Pos))
            {
                KeyValuePair<Vector3, float> kvp = new KeyValuePair<Vector3, float>(_Obstacles[i], _ObstacleSizes[i]);
                collidingObstacles.Add(kvp);
            }
        }

        if (collidingObstacles.Count == 0)
        {
            return Vector3.zero;
        }

        KeyValuePair<Vector3, float> closest = FindClosestObstacle(collidingObstacles, _Pos);

        Vector3 avoidance = _Pos - closest.Key;
        avoidance = avoidance.normalized * _AvoidanceForce;

        return avoidance;
    }

    private Vector3 Queue(Vector3[] _NeighbourPositions, Vector3 _Pos, Vector3 _Velocity, Vector3 _Steering, float _VisionRange, float _MaxVelocity, float _QueueRangeFactor, float _StopFactor)
    {
        float speedFactor = _Velocity.magnitude / _MaxVelocity;
        Vector3 vision = _Pos + _Velocity.normalized * (_VisionRange * _QueueRangeFactor * speedFactor);
        Vector3 halfVision = vision * 0.5f;
        float visionRadius = _VisionRange * _QueueRangeFactor * speedFactor;
        bool Colliding = false;
        bool tooClose = false;

        for (int i = 0; i < _NeighbourPositions.Length; i++)
        {
            if (Collides(_NeighbourPositions[i], 1, vision) || Collides(_NeighbourPositions[i], 1, halfVision) || Collides(_NeighbourPositions[i], 1, _Pos))
            {
                Colliding = true;
            }
            if (Vector3.Distance(_Pos, _NeighbourPositions[i]) <= visionRadius)
            {
                tooClose = true;
            }
        }

        if (!Colliding)
        {
            return Vector3.zero;
        }

        Vector3 desiredVelo = Vector3.zero;

        desiredVelo += BrakeForce(_Velocity, _Steering, _StopFactor);
        desiredVelo += Separation(_NeighbourPositions, _Pos);

        if (tooClose)
        {
            desiredVelo += HardStop(_Velocity, _StopFactor);
        }

        return desiredVelo;
    }

    private Vector3 HardStop(Vector3 _Velocity, float _StopFactor)
    {
        return (_Velocity * (1 - _StopFactor)) * -1;
    }

    private Vector3 BrakeForce(Vector3 _Velocity, Vector3 _Steering, float _StopFactor)
    {
        Vector3 brake = _Steering * -1 * (1 - _StopFactor);

        return _Velocity * -1 + brake;
    }

    private bool Collides(Vector3 _ObstaclePos, float _ObstacleRadius, Vector3 _CollidingPos)
    {
        float dist = Vector3.Distance(_ObstaclePos, _CollidingPos);

        if (dist < _ObstacleRadius)
        {
            return true;
        }
        return false;
    }

    private KeyValuePair<Vector3, float> FindClosestObstacle(List<KeyValuePair<Vector3, float>> _Obstacles, Vector3 _Pos)
    {
        float dist = float.MaxValue;
        int index = -1;
        for (int i = 0; i < _Obstacles.Count; i++)
        {
            float temp = Vector3.Distance(_Obstacles[i].Key, _Pos);
            if (temp < dist)
            {
                dist = temp;
                index = i;
            }
        }

        if (index == -1)
        {
            return new KeyValuePair<Vector3, float>();
        }

        return _Obstacles[index];
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
        m_DesiredVelocity += ObstacleAvoidance(m_ObstaclePositions, m_ObstacleSizes, transform.position, m_Rigidbody.velocity, m_VisionRadius, m_MaxVelocity, 1, 25);
        if (m_NeighbourPositions != null)
        {
            m_DesiredVelocity += Queue(m_NeighbourPositions, transform.position, m_Rigidbody.velocity, m_DesiredVelocity - m_Velocity, m_VisionRadius, m_MaxVelocity, 0.5f, 0.3f);
        }
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