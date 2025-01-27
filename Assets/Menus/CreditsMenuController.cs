using UnityEngine;

public class CreditsMenuController : MonoBehaviour
{
    public void OnTRBRY()
    {
        Application.OpenURL("https://opengameart.org/users/trbry");
    }

    public void OnQubodup()
    {
        Application.OpenURL("https://opengameart.org/users/qubodup");
    }

    public void OnHyperlink(string _Hyperlink) 
    {
        Application.OpenURL(_Hyperlink);
    }

    public void Back()
    {
        gameObject.SetActive(false);
    }
}