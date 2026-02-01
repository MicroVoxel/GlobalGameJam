using UnityEngine;
using Core.Managers;

namespace Core.World
{
    /// <summary>
    /// จุดเกิดที่เป็น Checkpoint ได้ด้วย
    /// เมื่อผู้เล่นเดินผ่าน จะบันทึกจุดนี้เป็นจุดเกิดล่าสุด
    /// </summary>
    [RequireComponent(typeof(BoxCollider))] // บังคับให้มี Collider ไว้เช็คการเดินชน
    public class SpawnPoint : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("ถ้าเป็น True จะทำหน้าที่เป็น Checkpoint (เดินชนแล้วบันทึก)")]
        [SerializeField] private bool _isCheckpoint = true;

        [Tooltip("ลำดับความสำคัญ (เผื่อใช้กันการเดินย้อนกลับมา Save จุดเก่า)")]
        [SerializeField] private int _priority = 0;

        private void Awake()
        {
            // บังคับให้ Collider เป็น Trigger เสมอ
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isCheckpoint) return;

            // เมื่อผู้เล่นเดินผ่าน
            if (other.CompareTag("Player"))
            {
                LevelManager.Instance?.SetCheckpoint(this);
            }
        }

        private void OnDrawGizmos()
        {
            // วาด Gizmo แยกสี: เขียว = Checkpoint, ฟ้า = จุดเกิดธรรมดา
            Gizmos.color = _isCheckpoint ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.5f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            // วาดกล่อง Trigger ให้เห็นระยะ
            if (_isCheckpoint)
            {
                var col = GetComponent<BoxCollider>();
                if (col != null)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.2f);
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(col.center, col.size);
                }
            }
        }

        public int Priority => _priority;
    }
}