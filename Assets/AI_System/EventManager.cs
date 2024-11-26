using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public Action<float, Guid> BoidAttack;
    public Action<KeyValuePair<Guid,GameObject>> BoidDeath;
    public Action<BoidData> SendBoidDataToGrid;
    public Dictionary<Guid, Action<Guid, Team>> OnRemoveBoidFromGridCallbacks = new Dictionary<Guid, Action<Guid, Team>>();
    public Dictionary<Guid, Action<Guid, Team>> OnAddBoidToGridCallbacks = new Dictionary<Guid, Action<Guid, Team>>();
    public Dictionary<Guid, Action<Vector3>> OnRemoveBoidVisionFromGridCallbacks = new Dictionary<Guid, Action<Vector3>>();
    public Dictionary<Guid, Action<Vector3>> OnAddBoidVisionToGridCallbacks = new Dictionary<Guid, Action<Vector3>>();

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