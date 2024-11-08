using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FormationDataManager))]
public class FormationBoidManager : MonoBehaviour
{
    private List<KeyValuePair<Guid, GameObject>> m_Boids = new List<KeyValuePair<Guid, GameObject>>();

    private FormationDataManager m_DataManager;

    public List<KeyValuePair<Guid, GameObject>> Boids { get => m_Boids;}

    private void Awake()
    {
        m_DataManager = GetComponent<FormationDataManager>();
    }

    public void AddBoid(KeyValuePair<Guid, GameObject> _Boid)
    {
        if (m_Boids.Count < m_DataManager.QueryStat(FormationStat.MaxUnitCount) && !m_Boids.Contains(_Boid))
        {
            m_Boids.Add(_Boid);
        }
    }

    public void RemoveBoid(KeyValuePair<Guid, GameObject> _Boid)
    {
        if (m_Boids.Contains(_Boid))
        {
            m_Boids.Remove(_Boid);
        }

        if (m_Boids.Count <= 0)
        {
            Destroy(this.gameObject);
        }
    }

    public void UpdateFormationPos() 
    {
        for (int i = 0; i < m_Boids.Count; i++) 
        {
            BoidDataManager boid = m_Boids[i].Value.GetComponent<BoidDataManager>();
            boid.FormationPosition = m_DataManager.QueryBoidPosition(i);
            boid.FormationCenter = transform.position;
        }
    }
}