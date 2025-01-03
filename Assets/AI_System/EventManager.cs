using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    public Action<float, Guid> BoidAttack;
    public Action<KeyValuePair<Guid, BoidDataManager>> BoidDeath;
    public Action<GameObject, RTree_Object> SendTreeBoidUpdate;
    public Action<GameObject, RTree_Object> SendTreeBoidRegister;
    public Action<GameObject> SendTreeBoidRemove;

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