using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BoidPool : MonoBehaviour
{
    public static BoidPool Instance;

    [SerializeField] private GameObject m_BoidPrefab;

    private Dictionary<Guid, GameObject> m_InUsePool;
    private Dictionary<Guid, GameObject> m_InActivePool;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        m_InUsePool = new Dictionary<Guid, GameObject>();
        m_InActivePool = new Dictionary<Guid, GameObject>();
    }

    private bool IsPoolEmpty()
    {
        return m_InActivePool.Count == 0;
    }

    public KeyValuePair<Guid, GameObject> GetNewBoid()
    {
        if (IsPoolEmpty())
        {
            GameObject temp = GameObject.Instantiate(m_BoidPrefab);
            Guid tempGUID =  Guid.NewGuid();

            m_InUsePool.Add(tempGUID, temp);

            return new KeyValuePair<Guid, GameObject>(tempGUID, temp);
        }
        else
        {
            GameObject temp = m_InActivePool.First().Value;
            Guid tempGUID = m_InActivePool.First().Key;

            m_InUsePool.Add(tempGUID, temp);
            m_InActivePool.Remove(tempGUID);

            temp.SetActive(true);

            return new KeyValuePair<Guid, GameObject>(tempGUID, temp);
        }
    }

    public GameObject GetActiveBoid(Guid _GUID)
    {
        if (_GUID == null || !m_InUsePool.ContainsKey(_GUID))
        {
            return null;
        }
        return m_InUsePool[_GUID];
    }

    public void ReturnActiveBoid(Guid _GUID) 
    {
        if (_GUID == null || !m_InUsePool.ContainsKey(_GUID))
        {
            return;
        }

        GameObject temp = m_InUsePool[_GUID];
        temp.SetActive(false);

        m_InUsePool.Remove(_GUID);
        m_InActivePool.Add(_GUID,temp);
    }
}