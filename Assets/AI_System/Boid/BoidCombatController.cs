using System;
using System.Collections.Generic;
using UnityEngine;

public class BoidCombatController : MonoBehaviour
{
    [SerializeField] private bool m_IsRanged = false;
    [SerializeField] private GameObject m_ProjectilePrefab;
    [SerializeField] private GameObject m_BowVisuals;

    private float m_AtkDamage = 0;
    private float m_AtkSpeed = 0;
    private float m_AtkRange = 0;

    private float m_AtkTime = 0;
    private float m_AtkTimer = 0;
    private Team m_Team = Team.Neutral;

    private BoidDataManager m_DataManager;

    public bool IsRanged { get => m_IsRanged; set => m_IsRanged = value; }
    public Team Team { get => m_Team;}

    private void Start()
    {
        m_DataManager = GetComponent<BoidDataManager>();

        m_AtkDamage = m_DataManager.QueryStat(BoidStat.AtkDamage);
        m_AtkSpeed = m_DataManager.QueryStat(BoidStat.AtkSpeed);
        m_AtkRange = m_DataManager.QueryStat(BoidStat.AtkRange);
        m_Team = m_DataManager.Team;

        m_AtkTime = 1 / m_AtkSpeed;
        m_AtkTimer = m_AtkTime;

        m_BowVisuals.SetActive(m_IsRanged);

        if (EventManager.Instance)
        {
            EventManager.Instance.BoidAttack += OnAttacked;
        }
    }

    private void Update()
    {
        if (m_DataManager != null)
        {
            if (m_AtkTimer <= 0)
            {
                KeyValuePair<Guid, Rigidbody> targetEnemy = m_DataManager.QueryClosestNeighbour(Team.Enemy);

                if (targetEnemy.Value != null && Vector3.Distance(targetEnemy.Value.position, transform.position) <= (m_AtkRange + (m_AtkRange * 0.1f)))
                {
                    if (m_IsRanged)
                    {
                        GameObject projectile = GameObject.Instantiate(m_ProjectilePrefab);
                        projectile.transform.position = transform.position;
                        Projectile projectileController = projectile.GetComponent<Projectile>();
                        projectileController.Damage = m_AtkDamage;
                        projectileController.Team = m_Team;
                        projectileController.TargetPos = targetEnemy.Value.position;
                        m_AtkTimer = m_AtkTime;
                    }
                    else
                    {
                        EventManager.Instance.BoidAttack?.Invoke(m_AtkDamage, targetEnemy.Key);
                        m_AtkTimer = m_AtkTime;
                    }
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
            float newHealth = m_DataManager.QueryStat(BoidStat.Health) - _Damage;

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

    public void OnArrowHit(float _Damage) 
    {
        float newHealth = m_DataManager.QueryStat(BoidStat.Health) - _Damage;

        if (newHealth <= 0)
        {
            Death();
        }
        else
        {
            m_DataManager.SetStat(BoidStat.Health, newHealth);
        }
    }

    private void Death()
    {
        if (EventManager.Instance)
        {
            EventManager.Instance.BoidDeath.Invoke(new KeyValuePair<Guid, BoidDataManager>(m_DataManager.Guid, m_DataManager));
        }
        BoidPool.Instance.ReturnActiveBoid(m_DataManager.Guid);
    }
}