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
    [SerializeField] private Transform m_AllyTarget;
    [SerializeField] private Transform m_EnemyTarget;

    private List<KeyValuePair<Guid, BoidDataManager>> TeamA = new List<KeyValuePair<Guid, BoidDataManager>>();
    private List<KeyValuePair<Guid, BoidDataManager>> TeamB = new List<KeyValuePair<Guid, BoidDataManager>>();
    private GameObject FormationTeamA = null;
    private GameObject FormationTeamB = null;

    private void Start()
    {
        if (m_SpawnFormations)
        {
            FormationTeamA = GameObject.Instantiate(m_formationPrefab);
            FormationTeamA.transform.position = new Vector3(m_EnemyTarget.transform.position.x, 1, m_EnemyTarget.transform.position.z);

            FormationTeamB = GameObject.Instantiate(m_formationPrefab);
            FormationTeamB.transform.position = new Vector3(m_AllyTarget.transform.position.x, 1, m_AllyTarget.transform.position.z);
        }

        for (int i = 0; i < m_spawnAmountTeamA; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialA;
            temp.Value.transform.position = transform.position + transform.right * 10 * i;
            temp.Value.GetComponent<BoidDataManager>().Team = Team.Ally;
            temp.Value.GetComponent<BoidDataManager>().SetMovTarget(m_AllyTarget.position);

            TeamA.Add(new KeyValuePair<Guid, BoidDataManager>(temp.Key, temp.Value.GetComponent<BoidDataManager>()));
            if (m_SpawnFormations)
            {
                FormationTeamA.GetComponent<FormationBoidManager>().AddBoid(TeamA[i]);
                temp.Value.transform.position = FormationTeamA.GetComponent<FormationDataManager>().QueryBoidPosition(i);
            }
        }

        for (int i = 0; i < m_spawnAmountTeamB; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialB;
            temp.Value.transform.position = transform.position + transform.forward * 200 + transform.right * 10 * i;
            temp.Value.GetComponent<BoidDataManager>().Team = Team.Enemy;
            temp.Value.GetComponent<BoidDataManager>().SetMovTarget(m_EnemyTarget.position);

            TeamB.Add(new KeyValuePair<Guid, BoidDataManager>(temp.Key, temp.Value.GetComponent<BoidDataManager>()));
            if (m_SpawnFormations)
            {
                FormationTeamB.GetComponent<FormationBoidManager>().AddBoid(TeamB[i]);
                temp.Value.transform.position = FormationTeamB.GetComponent<FormationDataManager>().QueryBoidPosition(i);
            }
        }

        if (m_SpawnFormations)
        {
            FormationDataManager dataManager = FormationTeamA.GetComponent<FormationDataManager>();
            FormationBoidManager boidManager = FormationTeamA.GetComponent<FormationBoidManager>();
            dataManager.UpdateBoidOffsets(boidManager.Boids.Count);

            dataManager = FormationTeamB.GetComponent<FormationDataManager>();
            boidManager = FormationTeamB.GetComponent<FormationBoidManager>();
            dataManager.UpdateBoidOffsets(boidManager.Boids.Count);
        }
    }
}