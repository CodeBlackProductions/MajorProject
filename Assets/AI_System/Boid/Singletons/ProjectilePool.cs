using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    public static ProjectilePool Instance;

    [SerializeField] private GameObject m_ProjectilePrefab;

    private List<GameObject> m_ProjectilePool;

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

        m_ProjectilePool = new List<GameObject>();
    }

    private bool IsPoolEmpty()
    {
        return m_ProjectilePool.Count == 0;
    }

    public GameObject GetNewProjectile()
    {
        if (IsPoolEmpty())
        {
            GameObject temp = GameObject.Instantiate(m_ProjectilePrefab);

            temp.SetActive(false);

            return temp;
        }
        else
        {
            GameObject temp = m_ProjectilePool.First();

            m_ProjectilePool.RemoveAt(0);

            temp.SetActive(false);

            return temp;
        }
    }

    public void ReturnProjectile(GameObject _Projectile)
    {
        _Projectile.SetActive(false);
        _Projectile.GetComponent<Projectile>().TargetPos = Vector3.zero;
        _Projectile.GetComponent<Projectile>().ParentBoid = Guid.Empty;
        _Projectile.GetComponent<Projectile>().Team = Team.Neutral;

        m_ProjectilePool.Add(_Projectile);
    }
}