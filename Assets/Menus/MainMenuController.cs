using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{

    [SerializeField] GameObject CreditsMenu;

    public void OnSceneSwitch(int _SceneIndex) 
    {
        SceneManager.LoadScene(_SceneIndex);
    }

    public void OnCreditsMenu() 
    {
        CreditsMenu.SetActive(true);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
