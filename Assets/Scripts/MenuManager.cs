using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Scene names")]
    public string mainMenuSceneName = "MainMenu";
    public string firstGameSceneName = "Level_01";
    public string secondGameSceneName = "Level_02";
    public string creditsSceneName = "Credits";

    [Header("Panel names in scene (find by name, include inactive)")]
    public string victoryPanelName = "Panel_Victory";
    public string defeatPanelName = "Panel_Defeat";
    public string pausePanelName = "Panel_Pause";
    public string hudPanelName = "Panel_HUD";
    public string creditsPanelName = "Panel_Credits";
    public string settingsPanelName = "Panel_Settings";

    [Header("Input")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool allowGamepadStart = true; // Start/Options (JoystickButton7) ou bouton "Pause" si tu l'as

    // Références runtime (trouvées par nom)
    GameObject victoryPanel, defeatPanel, pausePanel, hudPanel, creditsPanel, settingsPanel;
    Health playerHealth;

    bool paused, ended;

    // ---------------- Singleton ----------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindPlayerHealth();
        if (Instance == this) Instance = null;
    }

    // ---------------- Scene binding (no tags, no binder) ----------------
    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        ended = false;
        paused = false;
        Time.timeScale = 1f;

        // (Re)trouve tous les panels par NOM (même s’ils sont désactivés)
        victoryPanel = FindInSceneByName(victoryPanelName);
        defeatPanel = FindInSceneByName(defeatPanelName);
        pausePanel = FindInSceneByName(pausePanelName);
        hudPanel = FindInSceneByName(hudPanelName);
        creditsPanel = FindInSceneByName(creditsPanelName);
        settingsPanel = FindInSceneByName(settingsPanelName);

        // (Re)bind le joueur
        BindPlayerHealthByNameOrType();

        // État par défaut
        ShowPanel(null);
    }

    GameObject FindInSceneByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName)) return null;

        // Parcourt tous les root GOs de la scène courante, puis tous leurs enfants (includeInactive = true)
        var roots = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in roots)
        {
            var tr = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in tr)
                if (t.name == targetName) return t.gameObject;
        }
        return null;
    }

    void BindPlayerHealthByNameOrType()
    {
        UnbindPlayerHealth();

        // 1) si un GO nommé "Player" existe
        var playerGO = FindInSceneByName("Player");
        if (playerGO) playerHealth = playerGO.GetComponentInChildren<Health>(true);

        // 2) sinon, prends le premier Health de la scène (même inactif)
        if (!playerHealth)
        {
            // FindObjectsOfType<Health>(true) dispo Unity 2020.1+ ; sinon fallback LINQ via roots
#if UNITY_2020_1_OR_NEWER
            var all = Object.FindObjectsOfType<Health>(true);
            playerHealth = all.FirstOrDefault();
#else
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var r in roots)
            {
                var h = r.GetComponentsInChildren<Health>(true).FirstOrDefault();
                if (h) { playerHealth = h; break; }
            }
#endif
        }

        if (playerHealth) playerHealth.OnDeath.AddListener(OnPlayerDeath);
        // else Debug.LogWarning("[MenuManager] Aucun Health de joueur trouvé (défaite auto inactive).");
    }

    void UnbindPlayerHealth()
    {
        if (playerHealth) playerHealth.OnDeath.RemoveListener(OnPlayerDeath);
        playerHealth = null;
    }

    // ---------------- Update (pause) ----------------
    void Update()
    {
        if (ended) return;

        bool press = Input.GetKeyDown(pauseKey);
        if (allowGamepadStart)
        {
            press |= Input.GetKeyDown(KeyCode.JoystickButton7);
            press |= Input.GetButtonDown("Pause"); // si tu as créé cet input
        }

        if (press) TogglePause();
    }

    // ---------------- Panels ----------------
    void ShowPanel(GameObject panel)
    {
        SafeSet(victoryPanel, panel == victoryPanel);
        SafeSet(defeatPanel, panel == defeatPanel);
        SafeSet(pausePanel, panel == pausePanel);
        SafeSet(creditsPanel, panel == creditsPanel);
        SafeSet(settingsPanel, panel == settingsPanel);
        // HUD visible par défaut, aussi pendant la pause si tu veux (à toi d’ajuster)
        SafeSet(hudPanel, panel == null || panel == pausePanel);
    }

    void SafeSet(GameObject go, bool state)
    {
        if (!go) return;
        // S'assure que toute la chaîne de parents est active si on veut l'activer
        if (state)
        {
            var t = go.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
                t = t.parent;
            }
        }
        go.SetActive(state);
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

    // ---------------- Scenes ----------------
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

    // ---------------- Hooks ----------------
    void OnPlayerDeath() => ShowDefeat();
    public void TriggerVictory() => ShowVictory(); // à appeler depuis ton trigger de fin
}
