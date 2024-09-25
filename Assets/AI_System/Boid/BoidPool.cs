using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BoidPool : MonoBehaviour
{
    public static BoidPool Instance;

    [SerializeField] private GameObject m_BoidPrefab;

    private Dictionary<GUID, GameObject> m_InUsePool;
    private Dictionary<GUID, GameObject> m_InActivePool;

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

        m_InUsePool = new Dictionary<GUID, GameObject>();
        m_InActivePool = new Dictionary<GUID, GameObject>();
    }

    private bool IsPoolEmpty()
    {
        return m_InActivePool.Count == 0;
    }

    public GameObject GetNewBoid()
    {
        if (IsPoolEmpty())
        {
            GameObject temp = GameObject.Instantiate(m_BoidPrefab);
            GUID tempGUID = new GUID();

            m_InUsePool.Add(tempGUID, temp);

            return temp;
        }
        else
        {
            GameObject temp = m_InActivePool.First().Value;
            GUID tempGUID = m_InActivePool.First().Key;

            m_InUsePool.Add(tempGUID, temp);
            m_InActivePool.Remove(tempGUID);

            return temp;
        }
    }

    public GameObject GetActiveBoid(GUID _GUID) 
    {
        if (_GUID == null || !m_InUsePool.ContainsKey(_GUID)) 
        {
            return null;
        }
        return m_InUsePool[_GUID];
    }
}