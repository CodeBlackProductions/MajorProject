using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private int m_spawnAmountTeamA;
    [SerializeField] private Material m_spawnMaterialA;
    [SerializeField] private int m_spawnAmountTeamB;
    [SerializeField] private Material m_spawnMaterialB;

    [SerializeField] private GameObject m_formationPrefab;
    [SerializeField] private Transform[] m_Obstacles;

    private List<KeyValuePair<Guid, GameObject>> TeamA = new List<KeyValuePair<Guid, GameObject>>();
    private List<KeyValuePair<Guid, GameObject>> TeamB = new List<KeyValuePair<Guid, GameObject>>();

    private void Start()
    {
        for (int i = 0; i < m_spawnAmountTeamA; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialA;
            temp.Value.transform.position = transform.position + transform.right * 10 * i;

            for (int o = 0; o < m_Obstacles.Length; o++)
            {
                temp.Value.GetComponent<BoidDataManager>().AddObstacle(m_Obstacles[o]);
            }
            TeamA.Add(temp);
        }

        for (int i = 0; i < m_spawnAmountTeamB; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = m_spawnMaterialB;
            temp.Value.transform.position = transform.position + transform.forward * 100 + transform.right * 10 * i;

            for (int o = 0; o < m_Obstacles.Length; o++)
            {
                temp.Value.GetComponent<BoidDataManager>().AddObstacle(m_Obstacles[o]);
            }
            TeamB.Add(temp);
        }

        foreach (var boid in TeamA)
        {
            KeyValuePair<Guid, GameObject> temp = boid;
            foreach (var enemy in TeamB)
            {
                temp.Value.GetComponent<BoidDataManager>().AddNeighbour(Team.Enemy, enemy.Key, enemy.Value.GetComponent<Rigidbody>());
            }
            foreach (var ally in TeamA)
            {
                if (ally.Key != boid.Key)
                {
                    temp.Value.GetComponent<BoidDataManager>().AddNeighbour(Team.Ally, ally.Key, ally.Value.GetComponent<Rigidbody>());
                }
            }
        }

        foreach (var boid in TeamB)
        {
            KeyValuePair<Guid, GameObject> temp = boid;
            foreach (var enemy in TeamA)
            {
                temp.Value.GetComponent<BoidDataManager>().AddNeighbour(Team.Enemy, enemy.Key, enemy.Value.GetComponent<Rigidbody>());
            }
            foreach (var ally in TeamB)
            {
                if (ally.Key != boid.Key)
                {
                    temp.Value.GetComponent<BoidDataManager>().AddNeighbour(Team.Ally, ally.Key, ally.Value.GetComponent<Rigidbody>());
                }
            }
        }

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