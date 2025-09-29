using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels (assigne tes Canvas enfants)")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;
    public GameObject pausePanel;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject hudPanel;                 // HUD in-game (optionnel)

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";
    public string firstGameSceneName = "Level_01";   // pour StartGame()

    [Header("Player (pour la défaite automatique)")]
    public Health playerHealth;                 // glisse le Health du joueur (ou laisse vide et tag "Player")

    [Header("Options")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool allowGamepadStart = true;       // Start de la manette

    bool isPaused;
    bool gameEnded;                              // victoire/défaite (bloque l’input)

    void Awake()
    {
        // verrouillage de base
        Time.timeScale = 1f;
        HideAllPanels();
    }

    void Update()
    {
        if (gameEnded) return;

        bool pausePressed = Input.GetKeyDown(pauseKey);
        if (allowGamepadStart)
        {
            // Start (Xbox) = JoystickButton7 ; Options (PS) selon mapping -> souvent 7 aussi
            pausePressed |= Input.GetKeyDown(KeyCode.JoystickButton7);
            // ou si tu as défini un bouton "Pause" dans Input Manager :
            pausePressed |= Input.GetButtonDown("Pause");
        }

        if (pausePressed) TogglePause();
    }

    // ---------- API PANELS ----------
    public void Show(GameObject panel)
    {
        HideAllPanels();
        if (panel) panel.SetActive(true);

        // HUD caché si on montre un écran (hors pause ? au choix)
        if (hudPanel) hudPanel.SetActive(panel == null || panel == pausePanel ? true : false);
    }

    public void ShowPause()
    {
        if (gameEnded) return;
        isPaused = true;
        Time.timeScale = 0f;
        Show(pausePanel);
        // (désactive tes contrôles joueur si besoin)
    }

    public void HidePause()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Show(null); // affiche juste le HUD
    }

    public void TogglePause()
    {
        if (isPaused) HidePause();
        else ShowPause();
    }

    public void ShowVictory()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        Show(victoryPanel);
    }

    public void ShowDefeat()
    {
        gameEnded = true;
        Time.timeScale = 0f;
        Show(defeatPanel);
    }

    public void ShowCredits() => Show(creditsPanel);
    public void ShowSettings() => Show(settingsPanel);

    void HideAllPanels()
    {
        if (victoryPanel) victoryPanel.SetActive(false);
        if (defeatPanel) defeatPanel.SetActive(false);
        if (pausePanel) pausePanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (settingsPanel) settingsPanel.SetActive(false);
        if (hudPanel) hudPanel.SetActive(true);
    }

    // ---------- API SCÈNES / FLUX ----------
    public void StartGame()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(firstGameSceneName))
            SceneManager.LoadScene(firstGameSceneName);
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;
        int next = SceneManager.GetActiveScene().buildIndex + 1;
        if (next < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(next);
        else if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadRestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadPause() => ShowPause();

    public void ToMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
            SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Hooks logiques ----------
    void OnPlayerDeath() => ShowDefeat();

    // Appelle ceci depuis un trigger de fin de niveau
    public void TriggerVictory() => ShowVictory();
}
