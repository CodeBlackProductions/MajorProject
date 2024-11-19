using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicCombatScript : MonoBehaviour
{
    private float m_AtkDamage = 0;
    private float m_AtkSpeed = 0;

    private float m_AtkTime = 0;
    private float m_AtkTimer = 0;

    private BoidDataManager m_Datamanager;
    private Action<float, Guid> Attack;

    private void Awake()
    {
        m_Datamanager = GetComponent<BoidDataManager>();
        m_AtkDamage = m_Datamanager.QueryStat(BoidStat.AtkDamage);
        m_AtkSpeed = m_Datamanager.QueryStat(BoidStat.AtkSpeed);

        m_AtkTime = 1 / m_AtkSpeed;
        m_AtkTimer = m_AtkTime;

        if (BasicEventManager.Instance)
        {
            Attack += BasicEventManager.Instance.Attack;
            BasicEventManager.Instance.Attack += OnAttacked;
        }
    }

    private void Update()
    {
        if (m_Datamanager != null)
        {
            if (m_AtkTimer <= 0)
            {
                KeyValuePair<Guid, Rigidbody> targetEnemy = m_Datamanager.QueryClosestNeighbour(Team.Enemy);
                if (targetEnemy.Value != null && Vector3.Distance(targetEnemy.Value.position, transform.position) <= m_Datamanager.QueryStat(BoidStat.AtkRange))
                {
                    Attack?.Invoke(m_AtkDamage, targetEnemy.Key);
                    m_AtkTimer = m_AtkTime;
                }
            }
            else
            {
                m_AtkTimer -= Time.deltaTime;
            }
        }
    }

    private void OnAttacked(float _Damage, Guid _Target)
    {
        if (_Target == m_Datamanager.Guid)
        {
            float newHealth = m_Datamanager.QueryStat(BoidStat.Health) - m_AtkDamage;

            if (newHealth <= 0)
            {
                this.gameObject.SetActive(false);
            }
            else
            {
                m_Datamanager.SetStat(BoidStat.Health, newHealth);
            }
        }
    }
}