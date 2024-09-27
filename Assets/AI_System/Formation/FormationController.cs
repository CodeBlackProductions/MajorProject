using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(FormationBoidManager))]
public class FormationController : MonoBehaviour
{
    private FormationBoidManager m_BoidManager;
    private float updateTime = 0.5f;
    private float updateTimer = 0.5f;

    private void Awake()
    {
        m_BoidManager = GetComponent<FormationBoidManager>();
    }

    private void Update()
    {
        if (updateTimer <= 0)
        {
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < m_BoidManager.Boids.Count; i++)
            {
                pos += m_BoidManager.Boids[i].Value.transform.position;
            }
            transform.position = pos / m_BoidManager.Boids.Count;

            Vector3 fwd = Vector3.zero;
            for (int i = 0; i < m_BoidManager.Boids.Count; i++)
            {
                fwd += m_BoidManager.Boids[i].Value.transform.forward;
            }
            transform.forward = fwd / m_BoidManager.Boids.Count;

            m_BoidManager.UpdateFormationPos();

            updateTimer = updateTime;
        }
        else
        {
            updateTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}