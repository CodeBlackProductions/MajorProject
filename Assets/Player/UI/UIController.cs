using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Unit Spawning")]
    [SerializeField] private bool m_ShowWaveSpawner = false;

    [SerializeField] private GameObject m_BTNSpawnAllyWave;
    [SerializeField] private GameObject m_BTNSpawnEnemyWave;
    [SerializeField] private GameObject m_UISpawnPrompt;
    [SerializeField] private GameObject m_UISpawnTypePrompts;
    [SerializeField] private GameObject m_UIMeleePrompt;
    [SerializeField] private GameObject m_UIRangePrompt;
    [SerializeField] private GameObject m_UISpawnTeamPrompts;

    [Header("Unit Formations")]
    [SerializeField] private GameObject m_UIFormationControls;

    [SerializeField] private GameObject m_UIFormationPrompt;
    [SerializeField] private GameObject m_UIFormationTypePrompts;

    private EventManager m_EventManager;

    private void Awake()
    {
        if (!m_ShowWaveSpawner)
        {
            m_BTNSpawnAllyWave.SetActive(false);
            m_BTNSpawnEnemyWave.SetActive(false);
        }
    }

    private void Start()
    {
        m_EventManager = EventManager.Instance;

        m_EventManager.PlayerSpaceDown += OnSpawnMode;
        m_EventManager.PlayerSpaceUp += OnSpawnModeEnd;
        m_EventManager.Player1Up += OnMeleeSelect;
        m_EventManager.Player2Up += OnRangeSelect;
        m_EventManager.PlayerFormationDown += OnFormationMode;
        m_EventManager.PlayerFormationUp += OnFormationModeEnd;
        m_EventManager.PlayerUnitsSelected += OnUnitsSelectionChange;

        m_UIMeleePrompt.GetComponent<Image>().color = Color.cyan;
        m_UIRangePrompt.GetComponent<Image>().color = Color.white;
    }

    public void OnSpawnAllyWavePressed()
    {
        EventManager.Instance?.SpawnNewWave?.Invoke(Team.Ally);
    }

    public void OnSpawnEnemyWavePressed()
    {
        EventManager.Instance?.SpawnNewWave?.Invoke(Team.Enemy);
    }

    private void OnSpawnMode()
    {
        m_UISpawnPrompt.SetActive(false);
        m_UISpawnTypePrompts.SetActive(true);
        m_UISpawnTeamPrompts.SetActive(true);
    }

    private void OnSpawnModeEnd()
    {
        m_UISpawnPrompt.SetActive(true);
        m_UISpawnTypePrompts.SetActive(false);
        m_UISpawnTeamPrompts.SetActive(false);
    }

    private void OnMeleeSelect()
    {
        if (m_UISpawnTypePrompts.activeSelf)
        {
            m_UIMeleePrompt.GetComponent<Image>().color = Color.cyan;
            m_UIRangePrompt.GetComponent<Image>().color = Color.white;
        }
    }

    private void OnRangeSelect()
    {
        if (m_UISpawnTypePrompts.activeSelf)
        {
            m_UIMeleePrompt.GetComponent<Image>().color = Color.white;
            m_UIRangePrompt.GetComponent<Image>().color = Color.cyan;
        }
    }

    private void OnUnitsSelectionChange(bool _UnitsSelected)
    {
        m_UIFormationControls.SetActive(_UnitsSelected);
    }

    private void OnFormationMode()
    {
        m_UIFormationPrompt.SetActive(false);
        m_UIFormationTypePrompts.SetActive(true);
    }

    private void OnFormationModeEnd()
    {
        m_UIFormationPrompt.SetActive(true);
        m_UIFormationTypePrompts.SetActive(false);
    }
}