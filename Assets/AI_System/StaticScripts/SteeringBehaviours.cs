using System;
using System.Collections.Generic;
using UnityEngine;

public static class SteeringBehaviours
{
    public static Vector3 Seek(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity)
    {
        return (_TargetPos - _Pos).normalized * _MaxVelocity;
    }

    public static Vector3 Flee(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity)
    {
        return (_Pos - _TargetPos).normalized * _MaxVelocity;
    }

    public static Vector3 Arrive(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity, float _SlowRadius, float _MinimumTargetDist)
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

    public static Vector3 Avoid(Vector3 _TargetPos, Vector3 _Pos, float _MaxVelocity, float _VisionRadius)
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

    public static Vector3 Pursue(Rigidbody _Target, Vector3 _Pos, float _MaxVelocity, float _SlowRadius, float _MinimumTargetDist)
    {
        Vector3 futurePos = Vector3.zero;
        Vector3 distance = Vector3.zero;
        float time = 0;

        distance = _Target.position - _Pos;
        time = distance.magnitude / _MaxVelocity;

        futurePos = _Target.position + _Target.velocity * time;

        return Arrive(futurePos, _Pos, _MaxVelocity, _SlowRadius, _MinimumTargetDist);
    }

    public static Vector3 Evade(Rigidbody _Target, Vector3 _Pos, float _MaxVelocity, float _VisionRadius)
    {
        Vector3 futurePos = Vector3.zero;
        Vector3 distance = Vector3.zero;
        float time = 0;

        distance = _Target.position - _Pos;
        time = distance.magnitude / _MaxVelocity;
        futurePos = _Target.position + _Target.velocity * time;

        return Avoid(futurePos, _Pos, _MaxVelocity, _VisionRadius);
    }

    public static Vector3 Flock(Dictionary<Guid, Rigidbody> _Neighbours, Vector3 _Pos, float _CohesionWeight, float _SeparationWeight, float _AlignmentWeight)
    {
        Vector3 flocking = Vector3.zero;
        flocking += FlockCohesion(_Neighbours, _Pos) * _CohesionWeight;
        flocking += FlockSeparation(_Neighbours, _Pos) * _SeparationWeight;
        flocking += FlockAlignment(_Neighbours) * _AlignmentWeight;

        return flocking;
    }

    private static Vector3 FlockCohesion(Dictionary<Guid, Rigidbody> _Neighbours, Vector3 _Pos)
    {
        Vector3 cohesion = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            cohesion += neighbour.Value.position - _Pos;
        }

        cohesion /= _Neighbours.Count;

        return cohesion;
    }

    private static Vector3 FlockSeparation(Dictionary<Guid, Rigidbody> _Neighbours, Vector3 _Pos)
    {
        Vector3 separation = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            separation += _Pos - neighbour.Value.position;
        }

        separation /= _Neighbours.Count;

        return separation;
    }

    private static Vector3 FlockAlignment(Dictionary<Guid, Rigidbody> _Neighbours)
    {
        Vector3 alignment = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            alignment += neighbour.Value.velocity;
        }

        alignment /= _Neighbours.Count;

        return alignment;
    }

    public static Vector3 ObstacleAvoidance(Vector3[] _Obstacles, float[] _ObstacleSizes, Vector3 _Pos, Vector3 _Velocity, float _VisionRange, float _MaxVelocity, float _AvoidanceRangeFactor, float _AvoidanceForce)
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

    public static Vector3 Queue(Dictionary<Guid, Rigidbody> _Neighbours, Vector3 _Pos, Vector3 _Velocity, Vector3 _Steering, float _VisionRange, float _MaxVelocity, float _QueueRangeFactor, float _StopFactor)
    {
        float speedFactor = _Velocity.magnitude / _MaxVelocity;
        Vector3 vision = _Pos + _Velocity.normalized * (_VisionRange * _QueueRangeFactor * speedFactor);
        Vector3 halfVision = vision * 0.5f;
        float visionRadius = _VisionRange * _QueueRangeFactor * speedFactor;
        bool Colliding = false;
        bool tooClose = false;

        foreach (var neighbour in _Neighbours)
        {
            Vector3 neighbourPos = neighbour.Value.position;
            if (Collides(neighbourPos, 1, vision) || Collides(neighbourPos, 1, halfVision) || Collides(neighbourPos, 1, _Pos))
            {
                Colliding = true;
            }
            if (Vector3.Distance(_Pos, neighbourPos) <= visionRadius)
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
        desiredVelo += FlockSeparation(_Neighbours, _Pos);

        if (tooClose)
        {
            desiredVelo += HardStop(_Velocity, _StopFactor);
        }

        return desiredVelo;
    }

    private static Vector3 HardStop(Vector3 _Velocity, float _StopFactor)
    {
        return (_Velocity * (1 - _StopFactor)) * -1;
    }

    private static Vector3 BrakeForce(Vector3 _Velocity, Vector3 _Steering, float _StopFactor)
    {
        Vector3 brake = _Steering * -1 * (1 - _StopFactor);

        return _Velocity * -1 + brake;
    }

    private static bool Collides(Vector3 _ObstaclePos, float _ObstacleRadius, Vector3 _CollidingPos)
    {
        float dist = Vector3.Distance(_ObstaclePos, _CollidingPos);

        if (dist < _ObstacleRadius)
        {
            return true;
        }
        return false;
    }

    private static KeyValuePair<Vector3, float> FindClosestObstacle(List<KeyValuePair<Vector3, float>> _Obstacles, Vector3 _Pos)
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
}