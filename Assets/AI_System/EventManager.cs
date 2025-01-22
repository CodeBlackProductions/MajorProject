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
    public Action<bool,bool, bool> PlayerLeftMouseDown;
    public Action<bool,bool, bool> PlayerLeftMouseUp;
    public Action<bool, bool, bool> PlayerRightMouseDown;
    public Action<bool, bool, bool> PlayerRightMouseUp;
    public Action PlayerFormationDown;
    public Action PlayerFormationUp;
    public Action<bool> PlayerUnitsSelected;
    public Action PlayerSpaceDown;
    public Action PlayerSpaceUp;
    public Action Player1Down;
    public Action Player1Up;
    public Action Player2Down;
    public Action Player2Up;

    public Action<Team, Vector3> SpawnFormationAtPosition;
    public Action<Team> SpawnNewWave;
    public Action EnableSpawningUI;

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