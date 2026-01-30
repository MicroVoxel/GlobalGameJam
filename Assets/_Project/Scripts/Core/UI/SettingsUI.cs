using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Core.Managers;
using System.Linq; // ใช้สำหรับกรอง Resolution ซ้ำ

namespace Core.UI
{
    /// <summary>
    /// Class สำหรับจัดการ Elements ในหน้า Settings (View Layer)
    /// หน้าที่: รับ Input จาก User ส่งไป Manager และแสดงผลค่าปัจจุบัน
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        [Header("Audio Settings")]
        [SerializeField] private Slider _masterSlider;
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;

        [Header("Video Settings")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private TMP_Dropdown _resolutionDropdown;

        private Resolution[] _filteredResolutions;

        /// <summary>
        /// เรียกใช้งานโดย MainMenuController เพื่อเตรียมค่าเริ่มต้น
        /// </summary>
        public void Initialize()
        {
            SetupResolutionDropdown();
            UpdateVisualsToMatchData();
            BindEvents();
        }

        private void SetupResolutionDropdown()
        {
            // ดึง Resolution ทั้งหมดที่มี
            Resolution[] allResolutions = Screen.resolutions;

            // กรองเอาเฉพาะ Resolution ที่ไม่ซ้ำกัน (ป้องกัน List ยาวเกินไป) และเรียงลำดับ
            // ใช้ RefreshRateRatio สำหรับ Unity 2022.2+ หรือ refreshRate สำหรับเวอร์ชั่นเก่า
            _filteredResolutions = allResolutions
                .Select(r => new { r.width, r.height }) // เลือกเฉพาะ width/height
                .Distinct() // ตัดตัวซ้ำ
                .Select(x => allResolutions.First(r => r.width == x.width && r.height == x.height)) // แปลงกลับเป็น Resolution
                .ToArray();

            _resolutionDropdown.ClearOptions();

            List<string> options = new List<string>();
            int currentResolutionIndex = 0;

            for (int i = 0; i < _filteredResolutions.Length; i++)
            {
                string option = $"{_filteredResolutions[i].width} x {_filteredResolutions[i].height}";
                options.Add(option);

                // เช็คว่า Resolution นี้ตรงกับหน้าจอปัจจุบันไหม
                if (_filteredResolutions[i].width == Screen.width &&
                    _filteredResolutions[i].height == Screen.height)
                {
                    currentResolutionIndex = i;
                }
            }

            _resolutionDropdown.AddOptions(options);

            // ถ้ามีค่า Saved ไว้ ให้ใช้ค่า Saved แทนค่าปัจจุบันของจอ
            var savedIndex = SettingsManager.Instance.CurrentSettings.ResolutionIndex;
            // เช็ค Bound เพื่อป้องกัน Error กรณีเปลี่ยนจอ
            if (savedIndex >= 0 && savedIndex < _filteredResolutions.Length)
            {
                _resolutionDropdown.value = savedIndex;
            }
            else
            {
                _resolutionDropdown.value = currentResolutionIndex;
            }

            _resolutionDropdown.RefreshShownValue();
        }

        private void UpdateVisualsToMatchData()
        {
            var settings = SettingsManager.Instance.CurrentSettings;

            if (_masterSlider) _masterSlider.value = settings.MasterVolume;
            if (_musicSlider) _musicSlider.value = settings.MusicVolume;
            if (_sfxSlider) _sfxSlider.value = settings.SfxVolume;
            if (_fullscreenToggle) _fullscreenToggle.isOn = settings.IsFullscreen;
        }

        private void BindEvents()
        {
            // ล้าง Listener เดิมก่อนเพื่อป้องกันการเรียกซ้ำ (Safe practice)
            _masterSlider?.onValueChanged.RemoveAllListeners();
            _musicSlider?.onValueChanged.RemoveAllListeners();
            _sfxSlider?.onValueChanged.RemoveAllListeners();
            _fullscreenToggle?.onValueChanged.RemoveAllListeners();
            _resolutionDropdown?.onValueChanged.RemoveAllListeners();

            // ผูก Event ใหม่
            _masterSlider?.onValueChanged.AddListener(val => SettingsManager.Instance.SetMasterVolume(val));
            _musicSlider?.onValueChanged.AddListener(val => SettingsManager.Instance.SetMusicVolume(val));
            _sfxSlider?.onValueChanged.AddListener(val => SettingsManager.Instance.SetSFXVolume(val));

            _fullscreenToggle?.onValueChanged.AddListener(val => SettingsManager.Instance.SetFullscreen(val));

            _resolutionDropdown?.onValueChanged.AddListener(index =>
            {
                if (index >= 0 && index < _filteredResolutions.Length)
                {
                    Resolution res = _filteredResolutions[index];
                    SettingsManager.Instance.SetResolution(res.width, res.height, index);
                }
            });
        }
    }
}