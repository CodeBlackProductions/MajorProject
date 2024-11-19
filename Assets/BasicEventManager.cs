using System;
using UnityEngine;

public class BasicEventManager : MonoBehaviour
{
    public static BasicEventManager Instance;

    public Action<float, Guid> Attack;

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