using System.Collections.Generic;
using UnityEngine;

public class RTree_DataManager : MonoBehaviour
{
    public static RTree_DataManager Instance;

    [SerializeField] private int m_NodeCapacity = 10;
    [SerializeField] private int m_NodeMinCapacity = 5;

    private RTree m_Tree;

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

        m_Tree = new RTree(m_NodeCapacity, m_NodeMinCapacity);
    }

    public void AddObjectToTree(GameObject _Obj)
    {
        m_Tree.Insert(_Obj);
    }

    public void RemoveObjectFromTree(GameObject _Obj)
    {
        m_Tree.Remove(_Obj);
    }

    public void UpdateObjectInTree(GameObject _Obj)
    {
        m_Tree.UpdateObjectPosition(_Obj);
    }

    public Dictionary<GameObject, Team> QueryNeighboursInRange(Vector3 _Pos, float _Radius)
    {
        GameObject[] foundObjects = m_Tree.FindRange(CreateRect(_Pos, _Radius));

        if (foundObjects != null)
        {
            Dictionary<GameObject, Team> neighbours = new Dictionary<GameObject, Team>();

            for (int i = 0; i < foundObjects.Length; i++)
            {
                if (foundObjects[i].layer == LayerMask.NameToLayer("Boid"))
                {
                    neighbours.Add(foundObjects[i], foundObjects[i].GetComponent<BoidDataManager>().Team);
                }
            }

            return neighbours;
        }

        return null;
    }

    public List<GameObject> QueryObstaclesInRange(Vector3 _Pos, float _Radius)
    {
        GameObject[] foundObjects = m_Tree.FindRange(CreateRect(_Pos, _Radius));

        if (foundObjects != null)
        {
            List<GameObject> obstacles = new List<GameObject>();

            for (int i = 0; i < foundObjects.Length; i++)
            {
                if (foundObjects[i].layer == LayerMask.NameToLayer("Obstacle"))
                {
                    obstacles.Add(foundObjects[i]);
                }
            }

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
            TreeDebugger.Instance?.DrawDebug(m_Tree.Root);
        }
    }
}