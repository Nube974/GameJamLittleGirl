using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsToMain : MonoBehaviour
{
    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
