using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RTree_DataManager))]
public class RTree_BoidManager : MonoBehaviour
{
    public static RTree_BoidManager Instance;

    private RTree_DataManager m_DataManager;

    private Queue<GameObject> m_UpdateQueue = new Queue<GameObject>();
    private List<GameObject> m_UpdateList = new List<GameObject>();
    private bool m_UpdateRunning = false;

    private float timer = 0;
    private float time = 0.5f;

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

    private void Start()
    {
        if (RTree_DataManager.Instance != null)
        {
            m_DataManager = RTree_DataManager.Instance;
        }

        if (EventManager.Instance)
        {
            EventManager.Instance.SendTreeBoidUpdate += UpdateTree;
            EventManager.Instance.SendTreeBoidRegister += RegisterObject;
            EventManager.Instance.SendTreeBoidRemove += RemoveObject;
        }
    }

    private void Update()
    {
        if (timer <= 0)
        {
            if (m_UpdateQueue.Count > 0 && !m_UpdateRunning)
            {
                m_UpdateRunning = true;
                StartCoroutine(UpdateCall());
            }

            timer = time;
        }
        else
        {
            timer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Updates the grid after receiving new data.
    /// </summary>
    /// <param name="_Data">Data received</param>
    /// <param name="_oldPos">Previous grid position of the boid</param>
    /// <param name="_newPos">New grid position of the boid</param>
    public void UpdateTree(GameObject _Obj)
    {
        m_UpdateQueue.Enqueue(_Obj);
    }

    public void RegisterObject(GameObject _Obj)
    {
        m_DataManager.AddObjectToTree(_Obj);
    }

    public void RemoveObject(GameObject _Obj)
    {
        m_DataManager.RemoveObjectFromTree(_Obj);
    }

    private IEnumerator UpdateCall()
    {
        while (m_UpdateQueue.Count > 0)
        {
            int callsThisFrame = Mathf.CeilToInt(m_UpdateQueue.Count * (Time.deltaTime / timer));
            callsThisFrame = Mathf.Min(callsThisFrame, m_UpdateQueue.Count);
            for (int i = 0; i < callsThisFrame; i++)
            {
                m_DataManager.UpdateObjectInTree(m_UpdateQueue.Dequeue());
            }

            yield return null;
        }
        m_UpdateRunning = false;
    }
}