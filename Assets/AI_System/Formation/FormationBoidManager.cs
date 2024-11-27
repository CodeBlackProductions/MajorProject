using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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

    private void Start()
    {
        if (EventManager.Instance) 
        {
            EventManager.Instance.BoidDeath += RemoveBoid;
        }
    }

    public void AddBoid(KeyValuePair<Guid, GameObject> _Boid)
    {
        if (m_Boids.Count < m_DataManager.QueryStat(FormationStat.MaxUnitCount) && !m_Boids.Contains(_Boid))
        {
            m_Boids.Add(_Boid);

            if (m_Boids.Count > m_DataManager.QueryStat(FormationStat.MaxUnitCount))
            {
                m_DataManager.UpdateBoidOffsets(m_Boids.Count);
            }
        }
    }

    public void RemoveBoid(KeyValuePair<Guid, GameObject> _Boid)
    {
        if (m_Boids.Contains(_Boid))
        {
            BoidDataManager boid = _Boid.Value.GetComponent<BoidDataManager>();
            boid.FormationPosition = Vector3.zero;
            boid.FormationCenter = Vector3.zero;

            m_Boids.Remove(_Boid);
        }

        if (m_Boids.Count == 1)
        {
            BoidDataManager boid = m_Boids[0].Value.GetComponent<BoidDataManager>();
            boid.FormationPosition = Vector3.zero;
            boid.FormationCenter = Vector3.zero;

            m_Boids.Clear();
            this.gameObject.SetActive(false);
        }
        else if (m_Boids.Count <= 0)
        {
            this.gameObject.SetActive(false);
        }
        else if (m_Boids.Count <= m_DataManager.QueryStat(FormationStat.MaxUnitCount) - (m_DataManager.QueryStat(FormationStat.MaxUnitCount) *0.1f)) 
        {
            m_DataManager.UpdateBoidOffsets(m_Boids.Count);
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

    public void DisbandFormation() 
    {
        foreach (var boid in m_Boids)
        {
            RemoveBoid(boid);
        }
    }

}