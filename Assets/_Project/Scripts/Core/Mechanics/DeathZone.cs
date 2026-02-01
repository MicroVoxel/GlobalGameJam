using UnityEngine;
using Core.Managers;

namespace Core.Mechanics
{
    [RequireComponent(typeof(BoxCollider))]
    public class DeathZone : MonoBehaviour
    {
        private void Awake()
        {
            // บังคับให้เป็น Trigger เสมอ กันลืม
            GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // 1. ถ้าเป็นผู้เล่น
            if (other.CompareTag("Player"))
            {
                Debug.Log("💀 Player fell into Death Zone!");
                LevelManager.Instance?.RespawnPlayer();
            }
            // 2. ถ้าเป็นวัตถุที่มีฟิสิกส์ (Rigidbody) และไม่ใช่ Player
            else if (other.attachedRigidbody != null)
            {
                Debug.Log($"📦 Object '{other.name}' fell into Death Zone!");
                LevelManager.Instance?.RespawnObject(other.attachedRigidbody);
            }
        }

        // วาด Gizmo ให้เห็นโซนแดงๆ ใน Editor
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            var collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(collider.center, collider.size);
            }
        }
    }
}