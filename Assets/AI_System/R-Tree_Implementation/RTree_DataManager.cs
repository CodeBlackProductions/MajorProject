using RBush;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class RTree_DataManager : MonoBehaviour
{
    public static RTree_DataManager Instance;

    private RBush<RTree_Object> m_BoidTree = new RBush<RTree_Object>();
    private RBush<RTree_Object> m_ObstacleTree = new RBush<RTree_Object>();

    private Dictionary<GameObject, RTree_Object> PresentValues = new Dictionary<GameObject, RTree_Object>();

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

    public void AddObjectToTree(GameObject _Obj, RTree_Object _RObj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.Insert(_RObj);
            PresentValues.Add(_Obj, _RObj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.Insert(_RObj);
            PresentValues.Add(_Obj, _RObj);
        }
    }

    public void RemoveObjectFromTree(GameObject _Obj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.Delete(PresentValues[_Obj]);
            PresentValues.Remove(_Obj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.Delete(PresentValues[_Obj]);
            PresentValues.Remove(_Obj);
        }
    }

    public void UpdateObjectInTree(GameObject _Obj, RTree_Object _RObj)
    {
        if (_Obj.layer == LayerMask.NameToLayer("Boid"))
        {
            m_BoidTree.Delete(PresentValues[_Obj]);
            PresentValues.Remove(_Obj);

            m_BoidTree.Insert(_RObj);
            PresentValues.Add(_Obj, _RObj);
        }
        else if (_Obj.layer == LayerMask.NameToLayer("Obstacle"))
        {
            m_ObstacleTree.Delete(PresentValues[_Obj]);
            PresentValues.Remove(_Obj);

            m_ObstacleTree.Insert(_RObj);
            PresentValues.Add(_Obj, _RObj);
        }
    }

    public Dictionary<GameObject, Team> QueryNeighboursInRange(Vector3 _Pos, float _Radius)
    {
        IEnumerable<RTree_Object> foundObjects = m_BoidTree.Search(new Envelope(_Pos.x - _Radius, _Pos.z - _Radius, _Pos.x + _Radius, _Pos.z + _Radius));

        if (foundObjects != null)
        {
            Dictionary<GameObject, Team> neighbours = new Dictionary<GameObject, Team>();
            GridDataManager gridDataManager = GridDataManager.Instance;
            int cellsize = gridDataManager.CellSize;

            foreach (RTree_Object obj in foundObjects)
            {
                Vector2Int fromVec = new Vector2Int((int) (_Pos.x / cellsize),(int)(_Pos.z / cellsize));
                Vector2Int toVec = new Vector2Int((int) (obj.Object.transform.position.x / cellsize),(int)(obj.Object.transform.position.z / cellsize));
                if (gridDataManager.HasLoS(fromVec, toVec))
                {
                    neighbours.Add(obj.Object, obj.Object.GetComponent<BoidDataManager>().Team);
                }
            }

            return neighbours;
        }

        return null;
    }

    public List<GameObject> QueryObstaclesInRange(Vector3 _Pos, float _Radius)
    {
        IEnumerable<RTree_Object> foundObjects = m_ObstacleTree.Search(new Envelope(_Pos.x - _Radius, _Pos.z - _Radius, _Pos.x + _Radius, _Pos.z + _Radius));

        if (foundObjects != null)
        {
            List<GameObject> obstacles = new List<GameObject>();

            foreach (RTree_Object obj in foundObjects)
            {
                obstacles.Add(obj.Object);
            }

            return obstacles;
        }
        return null;
    }

    public void BulkAddToTree(List<GameObject> _Objects, List<RTree_Object> _RObjects)
    {
        for (int i = 0; i < _Objects.Count; i++)
        {
            PresentValues.Add(_Objects[i], _RObjects[i]);
        }
        m_BoidTree.BulkLoad(_RObjects);
    }
}