using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class JetpackAndHealPickup : MonoBehaviour
{
    [Header("Effet")]
    public float jetpackDurationBonus = 0.3f;  // +0.3s de capacité par item
    public bool alsoRefillBonus = true;  // on ajoute aussi au réservoir courant
    public int healAmount = 15;    // soins au player

    [Header("FX (optionnels)")]
    public GameObject pickupVFX;
    public AudioClip pickupSFX;
    public bool destroyOnPickup = true;

    AudioSource src;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        src = GetComponent<AudioSource>(); // optionnel
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 1) Jetpack
        var jet = other.GetComponentInChildren<JumpJetPackSystem>();
        if (jet != null)
        {
            jet.AddJetpackDuration(jetpackDurationBonus, alsoRefillBonus);
        }

        // 2) Heal (utilise ton script Health)
        var hp = other.GetComponentInChildren<Health>();
        if (hp != null && healAmount != 0)
        {
            hp.HealthUpdate(Mathf.Abs(healAmount)); // soins = valeur positive
        }

        // 3) FX
        if (pickupVFX) Instantiate(pickupVFX, transform.position, Quaternion.identity);
        if (pickupSFX)
        {
            if (!src) src = gameObject.AddComponent<AudioSource>();
            src.PlayOneShot(pickupSFX);
        }

        // 4) Retirer l’item
        AudioManager.Instance?.PlayPickup();
        if (destroyOnPickup) Destroy(gameObject);
        else gameObject.SetActive(false);

    }
}
