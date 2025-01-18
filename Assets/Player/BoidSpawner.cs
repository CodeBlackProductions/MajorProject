using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private int m_spawnAmountTeamA;
    [SerializeField] private Material m_spawnMaterialA;
    [SerializeField] private int m_spawnAmountTeamB;
    [SerializeField] private Material m_spawnMaterialB;

    [SerializeField] private bool m_SpawnFormations;

    [SerializeField] private GameObject m_formationPrefab;
    [SerializeField] private List<Transform> m_SpawnsTeamA;
    [SerializeField] private List<Transform> m_SpawnsTeamB;

    private EventManager m_EventManager = null;

    private void Start()
    {
        m_EventManager = EventManager.Instance;
        m_EventManager.SpawnNewWave += SpawnNewWave;
        m_EventManager.SpawnFormationAtPosition += SpawnNewFormation;

        SpawnNewWave(Team.Ally);
        SpawnNewWave(Team.Enemy);
    }

    public void SpawnNewWave(Team _Team)
    {
        if (_Team == Team.Ally)
        {
            foreach (var spawn in m_SpawnsTeamA)
            {
                SpawnFormation(Team.Ally, m_spawnAmountTeamA, m_spawnMaterialA, m_SpawnFormations, spawn.position);
            }
        }
        else
        {
            foreach (var spawn in m_SpawnsTeamB)
            {
                SpawnFormation(Team.Enemy, m_spawnAmountTeamB, m_spawnMaterialB, m_SpawnFormations, spawn.position);
            }
        }
    }

    public void SpawnNewFormation(Team _Team, Vector3 _Pos)
    {
        if (_Team == Team.Ally)
        {
            SpawnFormation(Team.Ally, m_spawnAmountTeamA, m_spawnMaterialA, m_SpawnFormations, _Pos);
        }
        else
        {
            SpawnFormation(Team.Enemy, m_spawnAmountTeamB, m_spawnMaterialB, m_SpawnFormations, _Pos);
        }
    }

    private void SpawnFormation(Team _Team, int _SpawnAmount, Material _BoidMat, bool _SpawnFormations, Vector3 _SpawnPos)
    {
        List<KeyValuePair<Guid, BoidDataManager>> boids = new List<KeyValuePair<Guid, BoidDataManager>>();
        GameObject formation = null;

        if (m_SpawnFormations)
        {
            formation = GameObject.Instantiate(m_formationPrefab);
            formation.transform.position = new Vector3(_SpawnPos.x, 1, _SpawnPos.z);
        }

        for (int i = 0; i < _SpawnAmount; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = _BoidMat;
            temp.Value.transform.position = transform.position + transform.right * 10 * i;
            temp.Value.GetComponent<BoidDataManager>().Team = _Team;

            boids.Add(new KeyValuePair<Guid, BoidDataManager>(temp.Key, temp.Value.GetComponent<BoidDataManager>()));
            if (m_SpawnFormations)
            {
                formation.GetComponent<FormationBoidManager>().AddBoid(boids[i]);
                temp.Value.transform.position = formation.GetComponent<FormationDataManager>().QueryBoidPosition(i);
            }
        }

        if (m_SpawnFormations)
        {
            FormationDataManager dataManager = formation.GetComponent<FormationDataManager>();
            FormationBoidManager boidManager = formation.GetComponent<FormationBoidManager>();
            dataManager.UpdateBoidOffsets(boidManager.Boids.Count);
        }
    }
}