using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private int spawnAmountTeamA;
    [SerializeField] private Material spawnMaterialA;
    [SerializeField] private int spawnAmountTeamB;
    [SerializeField] private Material spawnMaterialB;

    private List<KeyValuePair<Guid, GameObject>> TeamA = new List<KeyValuePair<Guid, GameObject>>();
    private List<KeyValuePair<Guid, GameObject>> TeamB = new List<KeyValuePair<Guid, GameObject>>();

    private void Start()
    {
        for (int i = 0; i < spawnAmountTeamA; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = spawnMaterialA;
            temp.Value.transform.position = transform.position + transform.right * 10 * i;
            TeamA.Add(temp);
        }

        for (int i = 0; i < spawnAmountTeamB; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.GetComponent<MeshRenderer>().material = spawnMaterialB;
            temp.Value.transform.position = transform.position + transform.forward * 100 + transform.right * 10 * i;
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
    }
}