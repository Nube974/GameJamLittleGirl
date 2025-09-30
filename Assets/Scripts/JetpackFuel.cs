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
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = 1f;
            fuelSlider.wholeNumbers = false;
            fuelSlider.value = Fuel01;

            if (hideWhenFull)
            {
                bool show = Fuel01 < 0.999f;
                fuelSlider.gameObject.SetActive(show);
            }
        }
        OnFuelChanged?.Invoke(Fuel01);
    }
}
