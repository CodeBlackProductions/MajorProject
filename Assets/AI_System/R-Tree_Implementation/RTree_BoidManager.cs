using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RTree_DataManager))]
public class RTree_BoidManager : MonoBehaviour
{
    public static RTree_BoidManager Instance;

    private RTree_DataManager m_DataManager;

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

    /// <summary>
    /// Updates the grid after receiving new data.
    /// </summary>
    /// <param name="_Data">Data received</param>
    /// <param name="_oldPos">Previous grid position of the boid</param>
    /// <param name="_newPos">New grid position of the boid</param>
    public void UpdateTree(GameObject _Obj)
    {
        m_DataManager.UpdateObjectInTree(_Obj);
    }

    public void RegisterObject(GameObject _Obj) 
    {
        m_DataManager.AddObjectToTree(_Obj);
    }

    public void RemoveObject(GameObject _Obj) 
    {
        m_DataManager.RemoveObjectFromTree(_Obj);
    }
}