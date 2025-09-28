using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    // --- Variables (diagramme) ---
    [SerializeField] private int currentHealth;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int minHealth = 0;

    [Header("UI (optionnel)")]
    public Slider healthSlider;                 // peut être laissé vide
    [SerializeField] private bool hideSliderWhenFull = false;

    [Header("Death")]
    [SerializeField] private bool destroyOnDeath = false; // sinon SetActive(false)

    // Events utiles (optionnels)
    public System.Action<int> OnDamaged;        // amount
    public System.Action<int> OnHealed;         // amount
    public System.Action OnDeath;

    // --- Lifecycle ---
    void Awake()
    {
        // init à la pleine vie si valeur non réglée
        if (currentHealth <= 0) currentHealth = maxHealth;
        UpdateSlider();
    }

    void OnValidate()
    {
        if (maxHealth < 1) maxHealth = 1;
        if (minHealth < 0) minHealth = 0;
        if (minHealth > maxHealth) minHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        UpdateSlider();
    }

    // --- Fonctions (diagramme) ---

    /// <summary>Ajoute une valeur (+/-) puis met à jour la vie & l'UI.</summary>
    public void HealthUpdate(int value)
    {
        if (value == 0 || IsDead()) return;

        int before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + value, minHealth, maxHealth);

        int delta = currentHealth - before;
        if (delta < 0) OnDamaged?.Invoke(-delta);
        else if (delta > 0) OnHealed?.Invoke(delta);

        UpdateSlider();

        if (IsDead())
        {
            OnDeath?.Invoke();
            if (destroyOnDeath) Destroy(gameObject);
            else gameObject.SetActive(false);
        }
    }

    /// <summary>Remet la vie au maximum.</summary>
    public void FullHeal()
    {
        if (IsDead()) return;
        int heal = maxHealth - currentHealth;
        currentHealth = maxHealth;
        if (heal > 0) OnHealed?.Invoke(heal);
        UpdateSlider();
    }

    /// <summary>Met à jour le Slider s’il est assigné.</summary>
    public void UpdateSlider()
    {
        if (!healthSlider) return;

        healthSlider.minValue = minHealth;          // 0 recommandé
        healthSlider.maxValue = maxHealth;
        healthSlider.wholeNumbers = true;           // évite les décimales
        healthSlider.value = Mathf.Clamp(currentHealth, minHealth, maxHealth);

        // Cache le handle et/ou le fill quand on est à 0
        if (healthSlider.handleRect)
            healthSlider.handleRect.gameObject.SetActive(currentHealth > minHealth);

        if (healthSlider.fillRect)
        {
            var fillImg = healthSlider.fillRect.GetComponent<UnityEngine.UI.Image>();
            if (fillImg) fillImg.enabled = currentHealth > minHealth;
        }
    }

    /// <summary>Inflige des dégâts (valeur positive).</summary>
    public void TakeDamage(int amount)
    {
        Debug.Log($"TakeDamage {amount} on {name}");
        if (amount <= 0) return;
        HealthUpdate(-amount);
    }

    /// <summary>Vrai si la vie est au minimum (? minHealth).</summary>
    public bool IsDead() => currentHealth <= minHealth;

    // --- Helpers pratiques ---
    public int Current => currentHealth;
    public void SetMaxHealth(int newMax, bool refill = false)
    {
        maxHealth = Mathf.Max(newMax, 1);
        if (refill) currentHealth = maxHealth;
        currentHealth = Mathf.Clamp(currentHealth, minHealth, maxHealth);
        UpdateSlider();
    }
}
