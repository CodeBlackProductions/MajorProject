using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BoidFlockingWeightManager : MonoBehaviour
{
    [SerializeField] private SO_FlockingWeights m_BaseWeights;

    private Dictionary<Weight,float> m_Weights = new Dictionary<Weight,float>();

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        foreach ( var weight in m_BaseWeights.Weights)
        {
            m_Weights.Add(weight.Key,weight.Value);
        }
    }

    public void SetWeight(Weight _Weight, float _Value) 
    {
        m_Weights[_Weight] = _Value;
    }

    public void ResetToBaseWeight(Weight _Weight) 
    {
        m_Weights[_Weight] = m_BaseWeights.Weights[_Weight];
    }

    public float QueryWeight(Weight _Weight) 
    {
        return m_Weights[_Weight];
    }


}