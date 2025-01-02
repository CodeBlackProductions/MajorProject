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

    private Queue<Tuple<GameObject, RTree_Object>> m_UpdateQueue = new Queue<Tuple<GameObject, RTree_Object>>();
    private List<GameObject> m_UpdateListObj = new List<GameObject>();
    private List<RTree_Object> m_UpdateListRObj = new List<RTree_Object>();
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

            //if (m_UpdateListObj.Count > 5)
            //{
            //    for (int i = 0; i < m_UpdateListObj.Count; i++) 
            //    {
            //        m_DataManager.RemoveObjectFromTree(m_UpdateListObj[i]);
            //    }
            //    m_DataManager.BulkAddToTree(m_UpdateListObj, m_UpdateListRObj);
            //}
            //else if (m_UpdateListObj.Count > 0) 
            //{
            //    for(int i = 0;i < m_UpdateListObj.Count; i++) 
            //    {
            //        m_DataManager.UpdateObjectInTree(m_UpdateListObj[i], m_UpdateListRObj[i]);
            //    }
            //}

            //m_UpdateListObj.Clear();
            //m_UpdateListRObj.Clear();

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
    public void UpdateTree(GameObject _Obj, RTree_Object _RObj)
    {
        m_UpdateQueue.Enqueue(new Tuple<GameObject, RTree_Object>(_Obj, _RObj));
        //m_UpdateListObj.Add(_Obj);
        //m_UpdateListRObj.Add(_RObj);

    }

    public void RegisterObject(GameObject _Obj, RTree_Object _RObj)
    {
        m_DataManager.AddObjectToTree(_Obj, _RObj);
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
                Tuple<GameObject, RTree_Object> item = m_UpdateQueue.Dequeue();
                m_DataManager.UpdateObjectInTree(item.Item1, item.Item2);
            }

            yield return null;
        }
        m_UpdateRunning = false;
    }
}