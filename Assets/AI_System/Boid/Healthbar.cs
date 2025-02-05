using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    [SerializeField] private Image m_HealthBar;

    private Camera m_Camera;
    private float m_MaxHealth = 0;
    private float m_FillPercentage = 1f;

    public float MaxHealth { get => m_MaxHealth; set => m_MaxHealth = value; }

    private void Start()
    {
        m_Camera = Camera.main;
    }

    private void Update()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward, m_Camera.transform.rotation * Vector3.up);
    }

    public void UpdateHealth(float _NewHealth)
    {
        m_FillPercentage = _NewHealth / m_MaxHealth;
        m_HealthBar.fillAmount = m_FillPercentage;
    }
}