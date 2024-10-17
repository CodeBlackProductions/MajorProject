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
    [SerializeField] private Transform[] m_Obstacles;
    [SerializeField] private Transform m_AllyTarget;
    [SerializeField] private Transform m_EnemyTarget;

    private List<KeyValuePair<Guid, GameObject>> TeamA = new List<KeyValuePair<Guid, GameObject>>();
    private List<KeyValuePair<Guid, GameObject>> TeamB = new List<KeyValuePair<Guid, GameObject>>();

    private void Start()
    {
        for (int i = 0; i < m_spawnAmountTeamA; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialA;
            temp.Value.transform.position = transform.position + transform.right * 10 * i;
            temp.Value.GetComponent<BoidDataManager>().Team = Team.Ally;
            temp.Value.GetComponent<BoidDataManager>().SetMovTarget(m_AllyTarget.position);

            TeamA.Add(temp);
        }

        for (int i = 0; i < m_spawnAmountTeamB; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialB;
            temp.Value.transform.position = transform.position + transform.forward * 200 + transform.right * 10 * i;
            temp.Value.GetComponent<BoidDataManager>().Team = Team.Enemy;
            temp.Value.GetComponent<BoidDataManager>().SetMovTarget(m_EnemyTarget.position);

            TeamB.Add(temp);
        }

        if (m_SpawnFormations)
        {
            GameObject TestFormation = GameObject.Instantiate(m_formationPrefab);
            for (int i = 0; i < TeamA.Count; i++)
            {
                TestFormation.GetComponent<FormationBoidManager>().AddBoid(TeamA[i]);
            }

            TestFormation = GameObject.Instantiate(m_formationPrefab);
            for (int i = 0; i < TeamB.Count; i++)
            {
                TestFormation.GetComponent<FormationBoidManager>().AddBoid(TeamB[i]);
            }
        }
    }
}