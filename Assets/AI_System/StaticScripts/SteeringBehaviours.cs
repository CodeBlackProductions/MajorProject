using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow;

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
}