using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    void Start()
    {
        // Load saved values, default to 1f if not found
        float music = PlayerPrefs.GetFloat(MUSIC_VOLUME, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_VOLUME, 1f);

        // Set slider values (this may trigger OnValueChanged callbacks)
        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);

        // Apply the audio mixer values
        SetMusicVolume(music);
        SetSFXVolume(sfx);

        // Add listeners for slider changes
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetMusicVolume(float value)
    {
        float dbValue = value > 0 ? Mathf.Log10(Mathf.Clamp01(value)) * 20f : -80f;
        audioMixer.SetFloat(MUSIC_VOLUME, dbValue);
        PlayerPrefs.SetFloat(MUSIC_VOLUME, value);
    }

    public void SetSFXVolume(float value)
    {
        float dbValue = value > 0 ? Mathf.Log10(Mathf.Clamp01(value)) * 20f : -80f;
        audioMixer.SetFloat(SFX_VOLUME, dbValue);
        PlayerPrefs.SetFloat(SFX_VOLUME, value);
    }
}
