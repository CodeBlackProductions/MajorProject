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

    public static Vector3 Flock(List<KeyValuePair<Guid, Rigidbody>> _Neighbours, Vector3 _Pos, float _VisRange, float _MaxVelocity, float _CohesionWeight, float _SeparationWeight, float _AlignmentWeight)
    {
        Vector3 flocking = Vector3.zero;
        flocking += FlockCohesion(_Neighbours, _Pos, _VisRange, _MaxVelocity) * _CohesionWeight;
        flocking += FlockSeparation(_Neighbours, _Pos, _VisRange, _MaxVelocity) * _SeparationWeight;
        flocking += FlockAlignment(_Neighbours) * _AlignmentWeight;

        return flocking;
    }

    private static Vector3 FlockCohesion(List<KeyValuePair<Guid, Rigidbody>> _Neighbours, Vector3 _Pos, float _VisRange, float _MaxVelocity)
    {
        Vector3 cohesion = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            float dist = (neighbour.Value.position - _Pos).magnitude;
            float distFactor = CalculateDistanceFactor(dist, _VisRange);
            cohesion += (neighbour.Value.position - _Pos).normalized * distFactor * _MaxVelocity;
        }

        cohesion /= _Neighbours.Count;

        return cohesion;
    }

    private static Vector3 FlockSeparation(List<KeyValuePair<Guid, Rigidbody>> _Neighbours, Vector3 _Pos, float _VisRange, float _MaxVelocity)
    {
        Vector3 separation = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            float dist = (neighbour.Value.position - _Pos).magnitude;
            float distFactor = CalculateDistanceFactor(dist, _VisRange);
            float closeScaling = Mathf.Pow(1 + (1 / (dist + 0.1f)), 3);
            separation += (neighbour.Value.position - _Pos).normalized * (1 - distFactor) * closeScaling * -1 * _MaxVelocity;
        }

        separation /= _Neighbours.Count;

        return separation;
    }

    private static Vector3 FlockAlignment(List<KeyValuePair<Guid, Rigidbody>> _Neighbours)
    {
        Vector3 alignment = Vector3.zero;

        foreach (var neighbour in _Neighbours)
        {
            alignment += neighbour.Value.velocity;
        }

        alignment /= _Neighbours.Count;

        return alignment;
    }

    public static bool CheckForObstacleAvoidance(Vector3[] _Obstacles, float[] _ObstacleSizes, Vector3 _Pos, Vector3 _Velocity, float _VisionRange, float _MaxVelocity, float _AvoidanceRangeFactor, float _AvoidanceForce)
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
            return false;
        }

        return true;
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

    private static float CalculateDistanceFactor(float _dist, float _RefDist)
    {
        float result = Mathf.InverseLerp(0, _RefDist, _dist);

        return result;
    }

    public static Vector3 FormationCohesion(Vector3 _TargetPos, Vector3 _Pos, float _VisRange, float _MaxVelocity)
    {
        Vector3 cohesion = Vector3.zero;

        float dist = (_TargetPos - _Pos).magnitude;
        float distFactor = CalculateDistanceFactor(dist, _VisRange);
        distFactor = distFactor <= 0.1f ? 0 : distFactor * 2;
        if (distFactor >= 1)
        {
            distFactor = 2f;
        }
        cohesion = (_TargetPos - _Pos).normalized * distFactor * _MaxVelocity;

        return cohesion;
    }
}