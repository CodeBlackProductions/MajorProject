using UnityEngine;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private void Start()
    {
        GameObject temp1 = GameObject.Instantiate(prefab, transform.position, Quaternion.identity);
        GameObject temp2 = GameObject.Instantiate(prefab, transform.position + transform.forward * 100 + transform.right * 100, Quaternion.identity);
        GameObject temp3 = GameObject.Instantiate(prefab, transform.position + transform.forward * 150 + transform.right * 50, Quaternion.identity);

        temp1.GetComponent<BoidFlockingManager>().m_TestTarget = temp2;
        temp1.GetComponent<BoidFlockingManager>().m_TestTarget2 = temp3;
        temp2.GetComponent<BoidFlockingManager>().m_TestTarget = temp1;
        temp2.GetComponent<BoidFlockingManager>().m_TestTarget2 = temp3;
        temp3.GetComponent<BoidFlockingManager>().m_TestTarget = temp1;
    }
}