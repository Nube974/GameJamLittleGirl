using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // -------- Singleton --------
    public static AudioManager Instance { get; private set; }

    [Header("Music Clips")]
    public AudioClip menuClip;
    public AudioClip level1Clip;
    public AudioClip level2Clip;
    public AudioClip creditsClip;
    public AudioClip victoryClip;
    public AudioClip defeatClip;

    [Header("SFX Clips - Player")]
    public AudioClip playerShootClip;
    public AudioClip playerJumpClip;
    public AudioClip jetpackStartClip;
    public AudioClip jetpackLoopClip;     // LOOP
    public AudioClip jetpackStopClip;

    [Header("SFX Clips - Enemies & Items")]
    public AudioClip enemyShootClip;
    public AudioClip enemyDeathClip;
    public AudioClip pickupItemClip;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;
    [Range(0f, 5f)] public float fadeTime = 0.75f;

    // internals
    AudioSource musicA, musicB;   // crossfade (2 pistes)
    bool useA = true;
    AudioSource sfx;              // one-shots
    AudioSource jetpackLoop;      // boucle jetpack

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Music A/B
        musicA = gameObject.AddComponent<AudioSource>();
        musicB = gameObject.AddComponent<AudioSource>();
        foreach (var m in new[] { musicA, musicB })
        {
            m.playOnAwake = false;
            m.loop = true;
            m.volume = 0f;
        }

        // SFX one-shot
        sfx = gameObject.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        sfx.loop = false;
        sfx.volume = sfxVolume;

        // Jetpack loop (indépendant)
        jetpackLoop = gameObject.AddComponent<AudioSource>();
        jetpackLoop.playOnAwake = false;
        jetpackLoop.loop = true;
        jetpackLoop.volume = sfxVolume * 0.8f; // un poil plus bas
        jetpackLoop.clip = jetpackLoopClip;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ---------------- MUSIC ----------------

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Choix simple par nom de scène (adapte les chaînes si besoin)
        var name = scene.name;
        if (name.Contains("MainMenu")) PlayMusic(menuClip);
        else if (name.Contains("Level 1")) PlayMusic(level1Clip);
        else if (name.Contains("Level 2")) PlayMusic(level2Clip);
        else if (name.Contains("Credits")) PlayMusic(creditsClip);
        // sinon on garde la musique en cours (ex: autres scènes/UI)
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        var from = useA ? musicA : musicB;
        var to = useA ? musicB : musicA;
        useA = !useA;

        StopCoroutine(nameof(Crossfade));
        StartCoroutine(Crossfade(from, to, clip, fadeTime, musicVolume));
    }

    public void PlayVictoryMusic() { if (victoryClip) PlayMusic(victoryClip); }
    public void PlayDefeatMusic() { if (defeatClip) PlayMusic(defeatClip); }

    IEnumerator Crossfade(AudioSource from, AudioSource to, AudioClip next, float time, float targetVol)
    {
        to.clip = next;
        to.volume = 0f;
        to.Play();

        float t = 0f;
        float fromStart = from.volume;

        while (t < time)
        {
            t += Time.unscaledDeltaTime; // continue même en pause
            float k = Mathf.Clamp01(t / time);
            to.volume = Mathf.Lerp(0f, targetVol, k);
            from.volume = Mathf.Lerp(fromStart, 0f, k);
            yield return null;
        }

        to.volume = targetVol;
        from.volume = 0f;
        from.Stop();
    }

    // ---------------- SFX One-shots ----------------

    public void PlayOneShot(AudioClip clip, float vol = 1f)
    {
        if (!clip) return;
        sfx.PlayOneShot(clip, sfxVolume * Mathf.Clamp01(vol));
    }

    // Hooks pratiques à appeler depuis tes scripts
    public void PlayPlayerShoot() => PlayOneShot(playerShootClip);
    public void PlayPlayerJump() => PlayOneShot(playerJumpClip);
    public void PlayEnemyShoot() => PlayOneShot(enemyShootClip);
    public void PlayEnemyDeath() => PlayOneShot(enemyDeathClip);
    public void PlayPickup() => PlayOneShot(pickupItemClip);

    // Jetpack (start/loop/stop)
    public void JetpackStart()
    {
        PlayOneShot(jetpackStartClip, 0.9f);
        if (jetpackLoopClip)
        {
            jetpackLoop.clip = jetpackLoopClip;
            if (!jetpackLoop.isPlaying) jetpackLoop.Play();
        }
    }

    public void JetpackStop()
    {
        PlayOneShot(jetpackStopClip, 0.9f);
        if (jetpackLoop.isPlaying) jetpackLoop.Stop();
    }

    // pour ajuster le volume en live
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        (useA ? musicB : musicA).volume = musicVolume;
    }
    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        sfx.volume = sfxVolume;
        jetpackLoop.volume = sfxVolume * 0.8f;
    }
}
