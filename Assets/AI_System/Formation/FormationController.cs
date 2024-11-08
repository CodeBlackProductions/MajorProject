using UnityEngine;
using static UnityEditor.PlayerSettings;

[RequireComponent(typeof(FormationBoidManager))]
public class FormationController : MonoBehaviour
{
    private FormationBoidManager m_BoidManager;
    private float updateTime = 0.05f;
    private float updateTimer = 0.05f;

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
        if (updateTimer <= 0)
        {
            Vector3 pos = Vector3.zero;
            for (int i = 0; i < m_BoidManager.Boids.Count; i++)
            {
                pos += m_BoidManager.Boids[i].Value.transform.position;
            }
            pos = pos / m_BoidManager.Boids.Count;
            transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime * 25f);

            Vector3 fwd = Vector3.zero;
            fwd = GetComponent<FormationDataManager>().QueryFlowfieldDir();
            transform.forward = Vector3.Slerp(transform.forward, fwd, Time.deltaTime * 10f);

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