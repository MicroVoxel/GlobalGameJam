using UnityEngine;
using Core.Managers;
using Core.Player;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Content.Sequences
{
    /// <summary>
    /// สคริปต์สำหรับฉากจบ: ล็อคตัวละคร -> ถอดหน้ากากอัตโนมัติ -> ขึ้นข้อความ -> กลับหน้าเมนู
    /// </summary>
    public class EndingTrigger : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("Canvas Group ของข้อความตอนจบ (ต้องมี Canvas Group เพื่อทำ Fade)")]
        [SerializeField] private CanvasGroup _endingTextCanvasGroup;

        [Header("Timing Settings")]
        [Tooltip("รอเท่าไหร่หลังเดินชน ก่อนจะเริ่มถอดหน้ากาก")]
        [SerializeField] private float _delayBeforeUnequip = 1.0f;

        [Tooltip("รอเท่าไหร่หลังถอดหน้ากากเสร็จ ก่อนจะขึ้นข้อความ")]
        [SerializeField] private float _delayBeforeMessage = 1.5f;

        [Tooltip("เวลาในการ Fade ข้อความขึ้นมา")]
        [SerializeField] private float _messageFadeDuration = 3.0f;

        [Tooltip("รอให้อ่านข้อความนานเท่าไหร่ก่อนตัดจบ")]
        [SerializeField] private float _waitBeforeQuit = 6.0f;

        [Header("Scene Navigation")]
        [Tooltip("ชื่อ Scene ของหน้าเมนูหลักที่จะโหลดกลับไป")]
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private bool _hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (_hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                _hasTriggered = true;
                PlayEndingSequence(other.GetComponent<PlayerController>()).Forget();
            }
        }

        private async UniTaskVoid PlayEndingSequence(PlayerController player)
        {
            Debug.Log("🎬 Ending Sequence Started");

            // 1. ล็อคการควบคุมทันที (หยุดเดิน, หยุดหัน)
            if (player != null)
            {
                player.SetControlLock(true);
            }

            // 2. รอจังหวะนิดนึงให้ผู้เล่นหยุดนิ่งและซึมซับบรรยากาศ
            await UniTask.Delay(System.TimeSpan.FromSeconds(_delayBeforeUnequip));

            // 3. บังคับถอดหน้ากาก (ถ้าใส่อยู่)
            if (player != null && player.IsMaskEquipped)
            {
                // *Hack: ต้องปลดล็อคชั่วคราวเพื่อให้คำสั่งถอดหน้ากากทำงานได้ (เพราะ Controller เช็ค Lock อยู่)
                player.SetControlLock(false);

                // สั่งถอด
                player.ForceUnequip().Forget();

                // ล็อคกลับทันที เพื่อไม่ให้ผู้เล่นขยับได้ระหว่างอนิเมชั่น
                player.SetControlLock(true);
            }

            // 4. รอจังหวะหลังถอดหน้ากากเสร็จ (ให้เห็นโลกความจริงชัดๆ)
            await UniTask.Delay(System.TimeSpan.FromSeconds(_delayBeforeMessage));

            // 5. ค่อยๆ Fade In ข้อความขึ้นมา
            if (_endingTextCanvasGroup != null)
            {
                _endingTextCanvasGroup.alpha = 0f;
                _endingTextCanvasGroup.gameObject.SetActive(true);

                // ใช้ DOTween Fade Alpha
                await _endingTextCanvasGroup.DOFade(1f, _messageFadeDuration).ToUniTask();
            }

            // 6. รอให้อ่านจบ
            await UniTask.Delay(System.TimeSpan.FromSeconds(_waitBeforeQuit));

            Debug.Log($"👋 THE END - Loading {_mainMenuSceneName}");

            // 6.5 [Fix] ปลดล็อคเมาส์ก่อนโหลด Scene ใหม่ เพื่อให้กดปุ่มในเมนูได้
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 7. โหลดกลับหน้าเมนูหลัก
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}