using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    public enum DetectMode { LayerMask, Tag }

    [Header("Détection du joueur")]
    public DetectMode detectMode = DetectMode.LayerMask;
    public LayerMask playerLayers;     // ex: coche la couche "Player"
    public string playerTag = "Player"; // si tu préfères par Tag

    [Header("Options")]
    public bool onlyOnce = true;
    public float delay = 0f;           // ex: 0.2s pour laisser jouer un SFX

    [Header("Fallback (si pas de MenuManager)")]
    public GameObject defeatPanel;     // optionnel
    public bool pauseOnFallback = true;

    bool triggered;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;          // IMPORTANT
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered && onlyOnce) return;

        bool isPlayer = detectMode == DetectMode.LayerMask
            ? (playerLayers.value & (1 << other.gameObject.layer)) != 0
            : other.CompareTag(playerTag);

        if (!isPlayer) return;

        triggered = true;
        if (delay > 0f) Invoke(nameof(DoDefeat), delay);
        else DoDefeat();
    }

    void DoDefeat()
    {
        if (MenuManager.Instance != null)
        {
            MenuManager.Instance.ShowDefeat();
            return;
        }

        // Fallback simple si pas de MenuManager
        if (defeatPanel) defeatPanel.SetActive(true);
        if (pauseOnFallback) Time.timeScale = 0f;
    }
}
