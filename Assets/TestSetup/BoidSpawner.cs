using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private int spawnAmount;

    private List<KeyValuePair<Guid, GameObject>> spawned = new List<KeyValuePair<Guid, GameObject>>();

    private void Start()
    {
        for (int i = 0; i < spawnAmount; i++)
        {
            KeyValuePair<Guid, GameObject> temp = BoidPool.Instance.GetNewBoid();
            temp.Value.transform.position = transform.position + transform.forward * 10 * i + transform.right * 10 * i;
            spawned.Add(temp);
        }

        foreach (var boid in spawned)
        {
            KeyValuePair<Guid, GameObject> temp = boid;
            foreach (var enemy in spawned)
            {
                if (temp.Key != enemy.Key)
                {
                    temp.Value.GetComponent<BoidDataManager>().AddNeighbour(Team.Enemy, enemy.Key, enemy.Value.GetComponent<Rigidbody>());
                }
            }
        }
    }
}