using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI")]
    public Slider musicSlider;   // 0..1
    public Slider sfxSlider;     // 0..1
    public Toggle muteMusicToggle;
    public Toggle muteSfxToggle;

    // PlayerPrefs keys
    const string KEY_MUSIC = "audio_music";
    const string KEY_SFX = "audio_sfx";

    float lastMusic = 0.8f;  // mémoires pour les toggles mute
    float lastSfx = 1.0f;

    void Awake()
    {
        // Valeurs par défaut si aucune pref
        float music = PlayerPrefs.HasKey(KEY_MUSIC) ? PlayerPrefs.GetFloat(KEY_MUSIC) : 0.8f;
        float sfx = PlayerPrefs.HasKey(KEY_SFX) ? PlayerPrefs.GetFloat(KEY_SFX) : 1.0f;

        // Clamp
        music = Mathf.Clamp01(music);
        sfx = Mathf.Clamp01(sfx);

        // UI
        if (musicSlider) musicSlider.value = music;
        if (sfxSlider) sfxSlider.value = sfx;

        // Applique aux volumes actuels
        AudioManager.Instance?.SetMusicVolume(music);
        AudioManager.Instance?.SetSfxVolume(sfx);

        // Mute toggles initial (OFF si volume > 0)
        if (muteMusicToggle) muteMusicToggle.isOn = (music <= 0.0001f);
        if (muteSfxToggle) muteSfxToggle.isOn = (sfx <= 0.0001f);

        lastMusic = (music > 0f) ? music : lastMusic;
        lastSfx = (sfx > 0f) ? sfx : lastSfx;

        // Abonnements
        if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (sfxSlider) sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        if (muteMusicToggle) muteMusicToggle.onValueChanged.AddListener(OnMuteMusic);
        if (muteSfxToggle) muteSfxToggle.onValueChanged.AddListener(OnMuteSfx);
    }

    void OnDestroy()
    {
        if (musicSlider) musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (sfxSlider) sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
        if (muteMusicToggle) muteMusicToggle.onValueChanged.RemoveListener(OnMuteMusic);
        if (muteSfxToggle) muteSfxToggle.onValueChanged.RemoveListener(OnMuteSfx);
    }

    // --- Callbacks ---

    void OnMusicChanged(float v)
    {
        v = Mathf.Clamp01(v);
        if (v > 0f) lastMusic = v;
        AudioManager.Instance?.SetMusicVolume(v);
        PlayerPrefs.SetFloat(KEY_MUSIC, v);
        PlayerPrefs.Save();

        if (muteMusicToggle && muteMusicToggle.isOn && v > 0f)
            muteMusicToggle.isOn = false; // désactive le mute si on bouge le slider
    }

    void OnSfxChanged(float v)
    {
        v = Mathf.Clamp01(v);
        if (v > 0f) lastSfx = v;
        AudioManager.Instance?.SetSfxVolume(v);
        PlayerPrefs.SetFloat(KEY_SFX, v);
        PlayerPrefs.Save();

        if (muteSfxToggle && muteSfxToggle.isOn && v > 0f)
            muteSfxToggle.isOn = false;
    }

    void OnMuteMusic(bool mute)
    {
        if (mute)
        {
            AudioManager.Instance?.SetMusicVolume(0f);
            if (musicSlider) musicSlider.SetValueWithoutNotify(0f);
            PlayerPrefs.SetFloat(KEY_MUSIC, 0f);
        }
        else
        {
            float v = Mathf.Clamp01(lastMusic);
            AudioManager.Instance?.SetMusicVolume(v);
            if (musicSlider) musicSlider.SetValueWithoutNotify(v);
            PlayerPrefs.SetFloat(KEY_MUSIC, v);
        }
        PlayerPrefs.Save();
    }

    void OnMuteSfx(bool mute)
    {
        if (mute)
        {
            AudioManager.Instance?.SetSfxVolume(0f);
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(0f);
            PlayerPrefs.SetFloat(KEY_SFX, 0f);
        }
        else
        {
            float v = Mathf.Clamp01(lastSfx);
            AudioManager.Instance?.SetSfxVolume(v);
            if (sfxSlider) sfxSlider.SetValueWithoutNotify(v);
            PlayerPrefs.SetFloat(KEY_SFX, v);
        }
        PlayerPrefs.Save();
    }

    // Bouton "Reset défaut" (optionnel)
    public void ResetDefaults()
    {
        float m = 0.8f, s = 1.0f;
        lastMusic = m; lastSfx = s;

        if (musicSlider) musicSlider.value = m;
        if (sfxSlider) sfxSlider.value = s;
        if (muteMusicToggle) muteMusicToggle.isOn = false;
        if (muteSfxToggle) muteSfxToggle.isOn = false;

        AudioManager.Instance?.SetMusicVolume(m);
        AudioManager.Instance?.SetSfxVolume(s);
        PlayerPrefs.SetFloat(KEY_MUSIC, m);
        PlayerPrefs.SetFloat(KEY_SFX, s);
        PlayerPrefs.Save();
    }
}
