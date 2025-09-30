using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class JetpackFuel : MonoBehaviour
{
    [Header("Valeurs")]
    [SerializeField] private float maxFuel = 3f;       // ex: 3 secondes d’utilisation
    [SerializeField] private float currentFuel = 3f;   // start plein (change si tu veux)
    [SerializeField] private float minFuel = 0f;

    [Header("UI (facultatif)")]
    public Slider fuelSlider;                          // glisse le Slider (comme la vie)
    [SerializeField] private bool hideWhenFull = false;

    [Header("Events (optionnels)")]
    public UnityEvent<float> OnFuelChanged;            // envoie la valeur [0..1]
    public UnityEvent OnFuelDepleted;                  // plus de fuel

    void Start()
    {
        // init
        currentFuel = Mathf.Clamp(currentFuel, minFuel, maxFuel);
        UpdateUI();
    }

    // --- API publique ---
    public void AddFuel(float amount)
    {
        if (amount <= 0f) return;
        currentFuel = Mathf.Clamp(currentFuel + amount, minFuel, maxFuel);
        UpdateUI();
    }

    /// <summary>Consomme instantanément (ex: rate * deltaTime). Retourne true si on a pu consommer.</summary>
    public bool Consume(float amount)
    {
        if (amount <= 0f) return false;
        if (currentFuel <= minFuel) { currentFuel = minFuel; UpdateUI(); return false; }

        currentFuel = Mathf.Max(minFuel, currentFuel - amount);
        UpdateUI();

        if (currentFuel <= minFuel) OnFuelDepleted?.Invoke();
        return true;
    }

    public void SetMaxFuel(float newMax, bool refill = false)
    {
        maxFuel = Mathf.Max(0.0001f, newMax);
        if (refill) currentFuel = maxFuel;
        currentFuel = Mathf.Clamp(currentFuel, minFuel, maxFuel);
        UpdateUI();
    }

    public float Fuel01 => maxFuel <= 0f ? 0f : Mathf.Clamp01(currentFuel / maxFuel);
    public bool IsEmpty => currentFuel <= minFuel;
    public float Current => currentFuel;
    public float Max => maxFuel;

    // --- UI ---
    void UpdateUI()
    {
        if (fuelSlider)
        {
            // setup de base
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = 1f;
            fuelSlider.wholeNumbers = false;

            // auto-assign Fill si pas fait (cherche "Fill Area/Fill")
            if (fuelSlider.fillRect == null)
            {
                var fill = fuelSlider.transform.Find("Fill Area/Fill") as RectTransform;
                if (fill != null) fuelSlider.fillRect = fill;
            }

            // (re)active le Fill
            if (fuelSlider.fillRect != null)
            {
                var img = fuelSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
                if (img) img.enabled = true;
                fuelSlider.fillRect.gameObject.SetActive(true);

                // s'assure que Fill Area a une largeur > 0
                var area = fuelSlider.transform.Find("Fill Area") as RectTransform;
                if (area && area.rect.width < 2f) // cas "barre blanche"
                {
                    area.sizeDelta = Vector2.zero; // laisse les anchors gérer la largeur
                    area.anchorMin = new Vector2(0, 0.25f);
                    area.anchorMax = new Vector2(1, 0.75f);
                    area.anchoredPosition = Vector2.zero;
                }
            }
            else
            {
                Debug.LogWarning("[JetpackFuel] Fill Rect non assigné : Fuel ne pourra pas s'afficher.");
            }

            // applique la valeur
            fuelSlider.value = Fuel01;

            // hide when full
            if (hideWhenFull)
                fuelSlider.gameObject.SetActive(Fuel01 < 0.999f);
            else
                fuelSlider.gameObject.SetActive(true);
        }

        OnFuelChanged?.Invoke(Fuel01);
    }
}
