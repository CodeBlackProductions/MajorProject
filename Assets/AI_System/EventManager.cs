using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public Action<float, Guid> BoidAttack;
    public Action<KeyValuePair<Guid, BoidDataManager>> BoidDeath;
    public Action<BoidData> SendBoidDataToGrid;
    public Dictionary<Guid, Action<Guid, Team>> OnRemoveBoidFromGridCallbacks = new Dictionary<Guid, Action<Guid, Team>>();
    public Dictionary<Guid, Action<Guid, Team>> OnAddBoidToGridCallbacks = new Dictionary<Guid, Action<Guid, Team>>();
    public Dictionary<Guid, Action<Vector3>> OnRemoveBoidVisionFromGridCallbacks = new Dictionary<Guid, Action<Vector3>>();
    public Dictionary<Guid, Action<Vector3>> OnAddBoidVisionToGridCallbacks = new Dictionary<Guid, Action<Vector3>>();

    public Action<Vector2> PlayerMove;
    public Action<float> PlayerScroll;
    public Action<bool,bool> PlayerLeftMouseDown;
    public Action<bool,bool> PlayerLeftMouseUp;
    public Action<bool, bool> PlayerRightMouseDown;
    public Action<bool, bool> PlayerRightMouseUp;

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