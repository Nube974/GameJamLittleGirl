using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnvironmentDamage : MonoBehaviour
{
    public enum Hazard { Poison, Lava }

    [Header("Réglages")]
    public Hazard type = Hazard.Poison;
    public int damage = 10;                 // dégâts par tick
    public float tickInterval = 3f;         // toutes les 3 secondes
    public bool hitOnEnter = false;         // inflige une 1re fois dès l'entrée

    // Suivi des cibles déjà en train de recevoir des dégâts (évite les doublons)
    private readonly Dictionary<Health, Coroutine> _running = new();

    void Reset()
    {
        // ce volume doit être un trigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var hp = FindHealth(other);
        if (!hp) return;

        if (!_running.ContainsKey(hp))
        {
            if (hitOnEnter) hp.TakeDamage(damage);
            var co = StartCoroutine(DamageLoop(hp));
            _running.Add(hp, co);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var hp = FindHealth(other);
        if (!hp) return;

        if (_running.TryGetValue(hp, out var co))
        {
            StopCoroutine(co);
            _running.Remove(hp);
        }
    }

    void OnDisable()
    {
        // Nettoyage si l'objet est désactivé/détruit
        foreach (var kv in _running) if (kv.Value != null) StopCoroutine(kv.Value);
        _running.Clear();
    }

    private IEnumerator DamageLoop(Health hp)
    {
        while (true)
        {
            hp.TakeDamage(damage);
            yield return new WaitForSeconds(tickInterval);
        }
    }

    // Récupère un Health même si le collider est sur un enfant
    private Health FindHealth(Collider2D col)
    {
        return col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
    }
}
