using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class ParallaxBackground : MonoBehaviour
{
    [Header("Références")]
    public Transform cameraTransform;               // Laisse vide → Main Camera
    [Tooltip("Sous-objet qui contient les Sprite/Renderers. Laisse vide pour ce GameObject.")]
    public Transform visualsRoot;

    [Header("Parallax")]
    [Range(-2f, 2f)] public float parallaxMultiplierX = 0.5f; // 0=immobile, 1=suit caméra, <0 inverse
    [Range(-2f, 2f)] public float parallaxMultiplierY = 0f;
    public bool lockYToStart = true;

    [Header("Duplication automatique")]
    [Min(3)] public int minTileCount = 3;
    [Range(0f, 2f)] public float viewPadding = 0.5f;

    [Header("Espacement entre duplicatas")]
    [Tooltip("Espace ajouté entre les tuiles (en unités monde). Positif = écart, négatif = chevauchement.")]
    public float tileGapX = 0f;

    [Header("Divers")]
    public float zOffset = 0f;
    public bool drawBoundsGizmo = false;

    // internes
    private readonly List<Transform> tiles = new List<Transform>();
    private readonly List<float> cachedOffsetsX = new List<float>();
    private bool offsetsCached = false;

    private float segWidth, segHeight;   // largeur/hauteur réelles du segment (bounds)
    private float strideX;               // pas horizontal = segWidth + tileGapX
    private Vector3 startPos;
    private Camera cam;
    private bool initialized;

    void Awake()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        cam = cameraTransform ? cameraTransform.GetComponentInChildren<Camera>() : Camera.main;
    }

    void Start()
    {
        if (!cameraTransform || !cam)
        {
            Debug.LogWarning($"[ParallaxAutoTiler2D] Caméra introuvable sur {name}");
            enabled = false;
            return;
        }

        // 1) Bounds combinés du segment
        Transform root = visualsRoot ? visualsRoot : transform;
        GetCombinedBounds(root, out segWidth, out segHeight);
        if (segWidth <= 0.0001f)
        {
            Debug.LogWarning($"[ParallaxAutoTiler2D] Largeur nulle sur {name}. Vérifie qu'il y a des Renderers.");
            segWidth = 10f; // fallback
        }

        // Stride = largeur + écart souhaité
        strideX = segWidth + tileGapX;

        // 2) Nombre de tuiles requis selon la vue (utilise stride)
        float halfView = GetCameraHalfWidthWorld(cam) + viewPadding * strideX;
        int needed = Mathf.CeilToInt((halfView * 2f) / strideX) + 1;
        int tileCount = Mathf.Max(minTileCount, needed);

        // 3) Créer/collecter les tuiles (un seul script pilote)
        tiles.Clear();
        tiles.Add(transform);
        for (int i = tiles.Count; i < tileCount; i++)
        {
            var clone = Instantiate(gameObject, transform.position, transform.rotation, transform.parent).transform;
            var tiler = clone.GetComponent<ParallaxBackground>();
            if (tiler) tiler.enabled = false;
            tiles.Add(clone);
        }

        // 4) Aligner en bande contiguë avec l'écart choisi
        startPos = transform.position;
        ArrangeTilesHorizontally();

        // 5) Offsets X figés (clé pour que l’original bouge aussi)
        cachedOffsetsX.Clear();
        for (int i = 0; i < tiles.Count; i++)
            cachedOffsetsX.Add(tiles[i].position.x - transform.position.x);
        offsetsCached = true;

        // 6) Désactive les scripts sur les clones
        for (int i = 1; i < tiles.Count; i++)
        {
            var s = tiles[i].GetComponent<ParallaxBackground>();
            if (s) s.enabled = false;
        }

        initialized = true;
    }

    void LateUpdate()
    {
        if (!initialized || !offsetsCached) return;

        Vector3 camPos = cameraTransform.position;
        float baseX = startPos.x + camPos.x * parallaxMultiplierX;
        float baseY = lockYToStart ? startPos.y : (startPos.y + camPos.y * parallaxMultiplierY);
        Vector3 basePos = new Vector3(baseX, baseY, startPos.z + zOffset);

        float leftMostX = float.PositiveInfinity, rightMostX = float.NegativeInfinity;
        int leftIdx = 0, rightIdx = 0;

        // Place chaque tuile selon son offset figé
        for (int i = 0; i < tiles.Count; i++)
        {
            Vector3 p = basePos + new Vector3(cachedOffsetsX[i], 0f, 0f);
            tiles[i].position = p;

            float x = p.x;
            if (x < leftMostX) { leftMostX = x; leftIdx = i; }
            if (x > rightMostX) { rightMostX = x; rightIdx = i; }
        }

        // Recyclage basé sur le STRIDE (pas l’ancienne segWidth)
        float halfView = GetCameraHalfWidthWorld(cam) + viewPadding * strideX;
        float camX = camPos.x;

        while (camX - (leftMostX + strideX * 0.5f) > halfView)
        {
            cachedOffsetsX[leftIdx] = cachedOffsetsX[rightIdx] + strideX;

            rightIdx = leftIdx;
            rightMostX = baseX + cachedOffsetsX[rightIdx];

            leftIdx = GetLeftmostIndex(baseX);
            leftMostX = baseX + cachedOffsetsX[leftIdx];
        }

        while (((rightMostX - strideX * 0.5f) - camX) > halfView)
        {
            cachedOffsetsX[rightIdx] = cachedOffsetsX[leftIdx] - strideX;

            leftIdx = rightIdx;
            leftMostX = baseX + cachedOffsetsX[leftIdx];

            rightIdx = GetRightmostIndex(baseX);
            rightMostX = baseX + cachedOffsetsX[rightIdx];
        }
    }

    // === Utils ===
    private void GetCombinedBounds(Transform root, out float width, out float height)
    {
        var renderers = root.GetComponentsInChildren<Renderer>(includeInactive: true);
        if (renderers.Length == 0) { width = 0f; height = 0f; return; }
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        width = b.size.x;
        height = b.size.y;
    }

    private float GetCameraHalfWidthWorld(Camera c)
    {
        if (c.orthographic)
            return c.orthographicSize * c.aspect;

        float z = Mathf.Abs(transform.position.z - c.transform.position.z);
        return Mathf.Tan(c.fieldOfView * 0.5f * Mathf.Deg2Rad) * z * c.aspect;
    }

    private void ArrangeTilesHorizontally()
    {
        int count = tiles.Count;
        int half = count / 2;

        for (int i = 0; i < count; i++)
        {
            int idx = i - half; // … -2,-1,0,1,2 …
            Vector3 lp = transform.localPosition + new Vector3(idx * strideX, 0f, 0f);
            tiles[i].localPosition = new Vector3(lp.x, transform.localPosition.y, transform.localPosition.z);
        }
    }

    private int GetLeftmostIndex(float baseX)
    {
        int idx = 0; float min = float.PositiveInfinity;
        for (int i = 0; i < cachedOffsetsX.Count; i++)
        {
            float x = baseX + cachedOffsetsX[i];
            if (x < min) { min = x; idx = i; }
        }
        return idx;
    }

    private int GetRightmostIndex(float baseX)
    {
        int idx = 0; float max = float.NegativeInfinity;
        for (int i = 0; i < cachedOffsetsX.Count; i++)
        {
            float x = baseX + cachedOffsetsX[i];
            if (x > max) { max = x; idx = i; }
        }
        return idx;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawBoundsGizmo) return;
        Transform root = visualsRoot ? visualsRoot : transform;
        GetCombinedBounds(root, out float w, out float h);
        Gizmos.color = new Color(0, 0.6f, 1f, 0.25f);
        Gizmos.DrawCube(root.position, new Vector3(w, h, 0.1f));
    }
}
