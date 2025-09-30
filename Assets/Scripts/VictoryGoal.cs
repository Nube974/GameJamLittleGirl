using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class VictoryGoal : MonoBehaviour
{
    public enum DetectMode { LayerMask, Tag }

    [Header("Détection du joueur")]
    public DetectMode detectMode = DetectMode.LayerMask;
    public LayerMask playerLayers = 0;      // ex: couche "Player"
    public string playerTag = "Player";     // si tu préfères un Tag

    [Header("Options")]
    public bool onlyOnce = true;
    public float delay = 0f;                // délai avant affichage (ex: 0.3s)
    public bool pauseOnFallback = true;     // si pas de MenuManager, on pause le jeu
    public GameObject fallbackVictoryPanel; // optionnel: panel si pas de MenuManager

    bool triggered;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // important
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && onlyOnce) return;

        bool isPlayer = false;
        if (detectMode == DetectMode.LayerMask)
            isPlayer = (playerLayers.value & (1 << other.gameObject.layer)) != 0;
        else
            isPlayer = other.CompareTag(playerTag);

        if (!isPlayer) return;

        triggered = true;
        if (delay > 0f) Invoke(nameof(DoVictory), delay);
        else DoVictory();
    }

    void DoVictory()
    {
        // 1) Avec ton MenuManager (singleton)
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.TriggerVictory(); // appelle ShowVictory() interne
            return;
        }

        // 2) Fallback simple: activer un panel si fourni
        if (fallbackVictoryPanel) fallbackVictoryPanel.SetActive(true);
        if (pauseOnFallback) Time.timeScale = 0f;
    }
}
