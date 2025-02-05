using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float m_Speed = 75;
    [SerializeField] private float m_ArcHeight = 15;

    private Team m_Team = Team.Neutral;
    private float m_Damage = 0;
    private Vector3 m_TargetPos = Vector3.zero;
    private Vector3 m_StartPos;
    private Guid m_ParentBoid;

    public Vector3 TargetPos { get => m_TargetPos; set => m_TargetPos = value; }
    public float Speed { get => m_Speed; set => m_Speed = value; }
    public float ArcHeight { get => m_ArcHeight; set => m_ArcHeight = value; }
    public float Damage { get => m_Damage; set => m_Damage = value; }
    public Team Team { get => m_Team; set => m_Team = value; }
    public Guid ParentBoid { get => m_ParentBoid; set => m_ParentBoid = value; }

    private void OnEnable()
    {
        m_StartPos = transform.position;
    }

    private void Update()
    {
        if (m_TargetPos != Vector3.zero)
        {
            Vector2 startPos2D = new Vector2(m_StartPos.x, m_StartPos.z);
            Vector2 targetPos2D = new Vector2(m_TargetPos.x, m_TargetPos.z);
            Vector2 currentPos2D = new Vector2(transform.position.x, transform.position.z);
            Vector2 nextPos2D = Vector2.MoveTowards(currentPos2D, targetPos2D, m_Speed * Time.deltaTime);

            float distTotal = Vector2.Distance(targetPos2D, startPos2D);
            float distTraveled = Vector2.Distance(nextPos2D, startPos2D);
            float distRemaining = Vector2.Distance(nextPos2D, targetPos2D);
            float baseY = Mathf.Lerp(m_StartPos.y, m_TargetPos.y, distTraveled / distTotal);
            float dynamicArcHeight = m_ArcHeight * (distTotal / 100f);
            float arc = dynamicArcHeight * distTraveled * distRemaining / (0.25f * distTotal * distTotal);
            Vector3 nextPos = new Vector3(nextPos2D.x, baseY + arc, nextPos2D.y);

            transform.LookAt(nextPos);
            transform.position = nextPos;

            if (nextPos == m_TargetPos) Arrived();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Boid"))
        {
            BoidCombatController controller;
            if (other.gameObject.TryGetComponent<BoidCombatController>(out controller))
            {
                if (m_Team != controller.Team)
                {
                    controller.OnArrowHit(m_Damage, m_ParentBoid);
                    ProjectilePool.Instance.ReturnProjectile(this.gameObject);
                }
            }
        }
    }

    private void Arrived()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 1, LayerMask.NameToLayer("Boid"));
        BoidCombatController controller;
        if (hits[0].TryGetComponent<BoidCombatController>(out controller) && controller.Team != m_Team)
        {
            controller.OnArrowHit(m_Damage,m_ParentBoid);
        }
        ProjectilePool.Instance.ReturnProjectile(this.gameObject);
    }
}