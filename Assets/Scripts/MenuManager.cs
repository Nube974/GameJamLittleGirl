using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    // -------- Singleton --------
    public static MenuManager Instance { get; private set; }

    public GameObject victoryPanel, defeatPanel, pausePanel, hudPanel, creditsPanel, settingsPanel;

    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu";
    public string firstGameSceneName = "Level_01";

    [Header("Player")]
    public Health playerHealth;                   // peut rester vide (autobind)

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool allowGamepadStart = true;         // Start/Options (JoystickButton7)

    bool paused, ended;

    // ===== Singleton & lifecycle =====
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        Time.timeScale = 1f;
        ShowPanel(null);

        // Autobind du Health du joueur si non assigné
        if (!playerHealth)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponentInChildren<Health>();
            if (!playerHealth) playerHealth = FindObjectOfType<Health>();
        }
        if (playerHealth) playerHealth.OnDeath.AddListener(OnPlayerDeath);
    }

    void OnDestroy()
    {
        if (playerHealth) playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
    }

    void Update()
    {
        if (ended) return;

        bool press = Input.GetKeyDown(pauseKey);
        if (allowGamepadStart) press |= Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetButtonDown("Pause");
        if (press) TogglePause();
    }

    // ---------- Panels ----------
    void ShowPanel(GameObject panel)
    {
        if (victoryPanel) victoryPanel.SetActive(panel == victoryPanel);
        if (defeatPanel) defeatPanel.SetActive(panel == defeatPanel);
        if (pausePanel) pausePanel.SetActive(panel == pausePanel);
        if (creditsPanel) creditsPanel.SetActive(panel == creditsPanel);
        if (settingsPanel) settingsPanel.SetActive(panel == settingsPanel);
        if (hudPanel) hudPanel.SetActive(panel == null || panel == pausePanel);
    }

    public void TogglePause()
    {
        if (paused) { paused = false; Time.timeScale = 1f; ShowPanel(null); }
        else { paused = true; Time.timeScale = 0f; ShowPanel(pausePanel); }
    }

    public void ShowVictory() { ended = true; Time.timeScale = 0f; ShowPanel(victoryPanel); }
    public void ShowDefeat() { ended = true; Time.timeScale = 0f; ShowPanel(defeatPanel); }
    public void ShowCredits() => ShowPanel(creditsPanel);
    public void ShowSettings() => ShowPanel(settingsPanel);

    // ---------- Scènes ----------
    public void StartGame() => SceneManager.LoadScene(firstGameSceneName);
    public void LoadRestartLevel() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void LoadNextLevel()
    {
        int i = SceneManager.GetActiveScene().buildIndex + 1;
        if (i < SceneManager.sceneCountInBuildSettings) SceneManager.LoadScene(i);
        else SceneManager.LoadScene(mainMenuSceneName);
    }
    public void ToMenu() => SceneManager.LoadScene(mainMenuSceneName);
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- Hooks ----------
    void OnPlayerDeath() => ShowDefeat();
    public void TriggerVictory() => ShowVictory(); // à appeler depuis un trigger de fin

    // ---------- Scène chargée ----------
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Vous pouvez ajouter ici le code à exécuter lors du chargement d'une scène,
        // par exemple réinitialiser l'état des panneaux ou du joueur.
        ShowPanel(null);
        ended = false;
        paused = false;
        Time.timeScale = 1f;

        // Autobind du Health du joueur si nécessaire
        if (!playerHealth)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) playerHealth = p.GetComponentInChildren<Health>();
            if (!playerHealth) playerHealth = FindObjectOfType<Health>();
        }
        if (playerHealth) playerHealth.OnDeath.AddListener(OnPlayerDeath);
    }
}
