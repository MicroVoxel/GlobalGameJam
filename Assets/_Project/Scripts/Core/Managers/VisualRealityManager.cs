using UnityEngine;
using UnityEngine.Rendering; // สำหรับ Volume
using DG.Tweening; // สำหรับ Animation
using Core.Managers;

namespace Core.Managers
{
    /// <summary>
    /// จัดการ Visual Feedback (Post-Processing) เมื่อสลับโลก
    /// </summary>
    public class VisualRealityManager : MonoBehaviour
    {
        [Header("Volumes")]
        [Tooltip("Volume สำหรับโลกปกติ (สีสดใส)")]
        [SerializeField] private Volume _realityVolume;

        [Tooltip("Volume สำหรับโลกหน้ากาก (สีเขียวหม่น)")]
        [SerializeField] private Volume _maskVolume;

        [Header("Settings")]
        [SerializeField] private float _transitionDuration = 0.5f;

        private void Start()
        {
            // เริ่มต้น: ให้โลกปกติทำงาน 100%, โลกหน้ากาก 0%
            if (_realityVolume) _realityVolume.weight = 1f;
            if (_maskVolume) _maskVolume.weight = 0f;

            // Subscribe Event
            if (RealityManager.Instance != null)
            {
                RealityManager.Instance.OnRealityChanged += UpdateVisuals;
                // Sync สถานะเริ่มต้น
                UpdateVisuals(RealityManager.Instance.IsMaskEquipped);
            }
        }

        private void OnDestroy()
        {
            if (RealityManager.Instance != null)
            {
                RealityManager.Instance.OnRealityChanged -= UpdateVisuals;
            }
        }

        private void UpdateVisuals(bool isMaskEquipped)
        {
            // ฆ่า Tween เก่าก่อน เพื่อป้องกันการตีกันถ้ารัวปุ่ม
            _realityVolume.DOKill();
            _maskVolume.DOKill();

            if (isMaskEquipped)
            {
                // เข้าสู่ Mask World: Reality -> 0, Mask -> 1
                DOTween.To(() => _realityVolume.weight, x => _realityVolume.weight = x, 0f, _transitionDuration);
                DOTween.To(() => _maskVolume.weight, x => _maskVolume.weight = x, 1f, _transitionDuration);
            }
            else
            {
                // กลับสู่ Reality: Reality -> 1, Mask -> 0
                DOTween.To(() => _realityVolume.weight, x => _realityVolume.weight = x, 1f, _transitionDuration);
                DOTween.To(() => _maskVolume.weight, x => _maskVolume.weight = x, 0f, _transitionDuration);
            }
        }
    }
}