using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicCombatScript : MonoBehaviour
{
    private float m_AtkDamage = 0;
    private float m_AtkSpeed = 0;

    private float m_AtkTime = 0;
    private float m_AtkTimer = 0;

    private BoidDataManager m_DataManager;

    private void Start()
    {
        m_DataManager = GetComponent<BoidDataManager>();

        m_AtkDamage = m_DataManager.QueryStat(BoidStat.AtkDamage);
        m_AtkSpeed = m_DataManager.QueryStat(BoidStat.AtkSpeed);

        m_AtkTime = 1 / m_AtkSpeed;
        m_AtkTimer = m_AtkTime;

        if (BasicEventManager.Instance)
        {
            BasicEventManager.Instance.Attack += OnAttacked;
        }
    }

    private void Update()
    {
        if (m_DataManager != null)
        {
            if (m_AtkTimer <= 0)
            {
                KeyValuePair<Guid, Rigidbody> targetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);
                if (targetEnemy.Value != null && Vector3.Distance(targetEnemy.Value.position, transform.position) <= m_DataManager.QueryStat(BoidStat.AtkRange))
                {
                    Debug.DrawLine(transform.position, targetEnemy.Value.position);
                    Debug.Log("Dealt " + m_AtkDamage + " to " + targetEnemy.Key);

                    BasicEventManager.Instance.Attack?.Invoke(m_AtkDamage, targetEnemy.Key);
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
        if (_Target == m_DataManager.Guid)
        {
            Debug.Log(m_DataManager.Guid + " received " + _Damage + " damage.");

            float newHealth = m_DataManager.QueryStat(BoidStat.Health) - m_AtkDamage;

            if (newHealth <= 0)
            {
                Death();
            }
            else
            {
                m_DataManager.SetStat(BoidStat.Health, newHealth);
            }
        }
    }

    private void Death()
    {
        if (BasicEventManager.Instance)
        {
            BasicEventManager.Instance.BoidDeath.Invoke(new KeyValuePair<Guid, GameObject>(m_DataManager.Guid, this.gameObject));
        }
        BoidPool.Instance.ReturnActiveBoid(m_DataManager.Guid);
    }
}