using UnityEngine;

[RequireComponent(typeof(FormationBoidManager))]
public class FormationController : MonoBehaviour
{
    private FormationBoidManager m_BoidManager;
    private float m_UpdateTime = 0.05f;
    private float m_UpdateTimer = 0.05f;

    private void Awake()
    {
        m_BoidManager = GetComponent<FormationBoidManager>();
    }

    private void Start()
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < m_BoidManager.Boids.Count; i++)
        {
            pos += m_BoidManager.Boids[i].Value.transform.position;
        }
        transform.position = pos / m_BoidManager.Boids.Count;
    }

    private void Update()
    {
        if (m_UpdateTimer <= 0)
        {
            Vector3 pos = Vector3.zero;
            Vector3 fwd = Vector3.zero;
            for (int i = 0; i < m_BoidManager.Boids.Count; i++)
            {
                Transform t = m_BoidManager.Boids[i].Value.transform;
                pos += t.position;

                fwd += t.forward;
            }

            pos = pos / m_BoidManager.Boids.Count;
           
            if (fwd != Vector3.zero && Vector3.Distance(pos,transform.position) > 0.25f)
            {
                transform.forward = Vector3.Slerp(transform.forward, fwd, Time.deltaTime * 10f);
            }

            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 25f);

            m_BoidManager.UpdateFormationPos();

            m_UpdateTimer = m_UpdateTime;
        }
        else
        {
            m_UpdateTimer -= Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, Vector3.one);
    }
}