using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private GameObject CreditsMenu;
    [SerializeField] private GameObject ControlsMenu;

    public void OnSceneSwitch(int _SceneIndex)
    {
        SceneManager.LoadScene(_SceneIndex);
    }

    public void OnControlsMenu()
    {
        ControlsMenu.SetActive(true);
    }

    public void OnCreditsMenu()
    {
        CreditsMenu.SetActive(true);
    }

    public void Quit()
    {
#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}