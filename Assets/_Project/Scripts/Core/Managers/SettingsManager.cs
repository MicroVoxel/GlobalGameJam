using UnityEngine;
using UnityEngine.Audio;
using Core.Patterns;

namespace Core.Managers
{
    [System.Serializable]
    public class GameSettingsData
    {
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool IsFullscreen = true;
        public int ResolutionIndex = 0;
    }

    /// <summary>
    /// Handles Game Settings Logic (Audio, Video) and Persistence.
    /// Follows Single Responsibility Principle: Data Management only.
    /// </summary>
    public class SettingsManager : Singleton<SettingsManager>
    {
        [Header("Audio Configuration")]
        [SerializeField] private AudioMixer _mainMixer;

        // Exposed Parameters in AudioMixer
        private const string MIXER_MASTER = "MasterVolume";
        private const string MIXER_MUSIC = "MusicVolume";
        private const string MIXER_SFX = "SFXVolume";

        private const string SAVE_KEY = "GameSettings";

        public GameSettingsData CurrentSettings { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            LoadSettings();
        }

        private void Start()
        {
            // Apply settings after initialization
            ApplyAllSettings();
        }

        // --- Public Setters ---

        public void SetMasterVolume(float value)
        {
            CurrentSettings.MasterVolume = value;
            SetMixerVolume(MIXER_MASTER, value);
        }

        public void SetMusicVolume(float value)
        {
            CurrentSettings.MusicVolume = value;
            SetMixerVolume(MIXER_MUSIC, value);
        }

        public void SetSFXVolume(float value)
        {
            CurrentSettings.SfxVolume = value;
            SetMixerVolume(MIXER_SFX, value);
        }

        public void SetFullscreen(bool isFullscreen)
        {
            CurrentSettings.IsFullscreen = isFullscreen;
            Screen.fullScreen = isFullscreen;
        }

        public void SetResolution(int width, int height, int index)
        {
            CurrentSettings.ResolutionIndex = index;
            Screen.SetResolution(width, height, CurrentSettings.IsFullscreen);
        }

        // --- Persistence ---

        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(CurrentSettings);
            PlayerPrefs.SetString(SAVE_KEY, json);
            PlayerPrefs.Save();
            Debug.Log($"[SettingsManager] Settings Saved: {json}");
        }

        private void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(SAVE_KEY);
                try
                {
                    CurrentSettings = JsonUtility.FromJson<GameSettingsData>(json);
                }
                catch
                {
                    Debug.LogError("Failed to load settings json, reverting to default.");
                    CurrentSettings = new GameSettingsData();
                }
            }
            else
            {
                CurrentSettings = new GameSettingsData(); // Default values
            }
        }

        // --- Internals ---

        private void ApplyAllSettings()
        {
            SetMasterVolume(CurrentSettings.MasterVolume);
            SetMusicVolume(CurrentSettings.MusicVolume);
            SetSFXVolume(CurrentSettings.SfxVolume);
            Screen.fullScreen = CurrentSettings.IsFullscreen;

            // Note: Resolution is usually applied by OS/Unity at start, 
            // but we store the index preference for the UI dropdown.
        }

        private void SetMixerVolume(string paramName, float sliderValue)
        {
            if (_mainMixer == null) return;

            // Convert linear 0.0001-1 to Logarithmic -80db to 0db
            // Clamp to prevent log(0)
            float dbValue = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
            _mainMixer.SetFloat(paramName, dbValue);
        }
    }
}