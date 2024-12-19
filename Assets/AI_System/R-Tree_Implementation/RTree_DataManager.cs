using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RTree_DataManager : MonoBehaviour
{
    public static RTree_DataManager Instance;

    [SerializeField] private int m_NodeCapacity = 10;
    [SerializeField] private int m_NodeMinCapacity = 5;

    private RTree m_BoidTree;
    private RTree m_ObstacleTree;

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

        m_BoidTree = new RTree(m_NodeCapacity, m_NodeMinCapacity);
        m_ObstacleTree = new RTree(m_NodeCapacity, m_NodeMinCapacity);
    }

    public void AddObjectToTree(GameObject _Obj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.Insert(_Obj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.Insert(_Obj);
        }
    }

    public void RemoveObjectFromTree(GameObject _Obj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.Remove(_Obj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.Remove(_Obj);
        }
    }

    public void UpdateObjectInTree(GameObject _Obj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.UpdateObjectPosition(_Obj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.UpdateObjectPosition(_Obj);
        }
    }

    public void BulkUpdateObjects(GameObject[] _Objects) 
    {
        if (_Objects[0].layer == LayerMask.NameToLayer("Boid"))
        {
            for (int i = 0; i < _Objects.Length; i++)
            {
                m_BoidTree.Remove(_Objects[i]);
            }
            m_BoidTree.BulkInsert(_Objects);
        }
        else if (_Objects[0].layer == LayerMask.NameToLayer("Obstacle"))
        {
            for (int i = 0; i < _Objects.Length; i++)
            {
                m_ObstacleTree.Remove(_Objects[i]);
            }
            m_ObstacleTree.BulkInsert(_Objects);
        }
    }

    public Dictionary<GameObject, Team> QueryNeighboursInRange(Vector3 _Pos, float _Radius)
    {
        GameObject[] foundObjects = m_BoidTree.FindRange(CreateRect(_Pos, _Radius));

        if (foundObjects != null)
        {
            Dictionary<GameObject, Team> neighbours = new Dictionary<GameObject, Team>();

            for (int i = 0; i < foundObjects.Length; i++)
            {
                neighbours.Add(foundObjects[i], foundObjects[i].GetComponent<BoidDataManager>().Team);
            }

            return neighbours;
        }

        return null;
    }

    public List<GameObject> QueryObstaclesInRange(Vector3 _Pos, float _Radius)
    {
        GameObject[] foundObjects = m_ObstacleTree.FindRange(CreateRect(_Pos, _Radius));

        if (foundObjects != null)
        {
            List<GameObject> obstacles = new List<GameObject>();

            obstacles = foundObjects.ToList();

            return obstacles;
        }

        return null;
    }

    private Rect CreateRect(Vector3 _Pos, float _Radius)
    {
        System.Numerics.Vector3 ll = new System.Numerics.Vector3(_Pos.x - _Radius, _Pos.y - _Radius, _Pos.z - _Radius);
        System.Numerics.Vector3 ur = new System.Numerics.Vector3(_Pos.x + _Radius, _Pos.y + _Radius, _Pos.z + _Radius);

        return new Rect(ll, ur);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            TreeDebugger.Instance?.DrawDebug(m_BoidTree.Root);
            TreeDebugger.Instance?.DrawDebug(m_ObstacleTree.Root);
        }
    }
}