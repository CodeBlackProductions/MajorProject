using System.Linq;
using UnityEditor;
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
}