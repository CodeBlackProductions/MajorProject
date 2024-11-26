using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicEventManager : MonoBehaviour
{
    public static BasicEventManager Instance;

    public Action<float, Guid> Attack;
    public Action<KeyValuePair<Guid,GameObject>> BoidDeath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}