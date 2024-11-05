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
            int cellsize = GridDataManager.Instance.CellSize;
            for (int x = 0; x < GridDataManager.Instance.BoidGrid.GetLength(0); x++)
            {
                for (int y = 0; y < GridDataManager.Instance.BoidGrid.GetLength(1); y++)
                {
                    Vector3 vec = new Vector3(this.transform.position.x + x * cellsize, 0, this.transform.position.z + y * cellsize);
                    Vector3 vec2 = new Vector3(currentDebugFlowfield[x, y].x, 0, currentDebugFlowfield[x, y].y);

                    Gizmos.DrawLine(vec, vec + vec2);
                }
            }
        }
    }
}