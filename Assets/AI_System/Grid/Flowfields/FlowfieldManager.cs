using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowfieldManager : MonoBehaviour
{
    public static FlowfieldManager Instance;

    [SerializeField] private SO_FlowfieldDatabase m_Database;

    private Vector2[,] currentDebugFlowfield;

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

    private void Start()
    {
        if (m_Database != null && m_Database.LevelName == SceneManager.GetActiveScene().name)
        {
            if (m_Database.Calculate)
            {
                m_Database.Precompute();
            }
            return;
        }
        else
        {
            throw new System.Exception("FlowfieldManager does not have access to correct Database!");
        }
    }

    public Vector2[,] QueryFlowfield(Vector2Int _TargetPos)
    {
        if (m_Database != null)
        {
            currentDebugFlowfield = m_Database.GetPrecomputedFlowField(_TargetPos);
            return m_Database.GetPrecomputedFlowField(_TargetPos);
        }

        return null;
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying && m_Database != null && currentDebugFlowfield != null)
        {
            for (int x = 0; x < 80; x++)
            {
                for (int y = 0; y < 80; y++)
                {
                    Vector3 vec = new Vector3(this.transform.position.x + x * 4, 0, this.transform.position.z + y * 4);
                    Vector3 vec2 = new Vector3(currentDebugFlowfield[x, y].x * 2, 0, currentDebugFlowfield[x, y].y * 2);

                    Gizmos.DrawLine(vec, vec + vec2);
                }
            }
        }
    }
}