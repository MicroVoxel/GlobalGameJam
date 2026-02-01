using UnityEngine;
using Core.Patterns;
using Core.World;
using Core.Player;
using System.Collections;

namespace Core.Managers
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Header("Settings")]
        [SerializeField] private SpawnPoint _defaultSpawnPoint; // จุดเกิดเริ่มต้นของฉาก
        [SerializeField] private float _respawnDelay = 1.0f;

        private PlayerController _player;
        private SpawnPoint _currentCheckpoint; // [New] จำจุดเกิดปัจจุบัน

        protected override void Awake()
        {
            base.Awake();
            _player = FindFirstObjectByType<PlayerController>();
        }

        private void Start()
        {
            // หาจุดเกิดเริ่มต้นถ้าไม่ได้ลากใส่
            if (_defaultSpawnPoint == null)
            {
                _defaultSpawnPoint = FindFirstObjectByType<SpawnPoint>();
            }

            // เริ่มต้นเซ็ตให้จุด Default เป็นจุดปัจจุบัน
            SetCheckpoint(_defaultSpawnPoint);
        }

        // [New] ฟังก์ชันบันทึกจุดเกิดใหม่
        public void SetCheckpoint(SpawnPoint newPoint)
        {
            if (newPoint == null) return;

            // (Optional) เช็ค Priority: ป้องกันการ Save ทับจุดที่ไกลกว่าด้วยจุดที่ใกล้กว่า (ถ้าต้องการ)
            // ถ้า Checkpoint ใหม่ Priority ต่ำกว่าอันปัจจุบัน ไม่ต้อง Save
            if (_currentCheckpoint != null && newPoint.Priority < _currentCheckpoint.Priority)
            {
                return;
            }

            if (_currentCheckpoint != newPoint)
            {
                _currentCheckpoint = newPoint;
                Debug.Log($"🚩 Checkpoint Updated: {newPoint.name}");
                // ตรงนี้ใส่เสียงหรือ Particle ตอนเก็บ Checkpoint ได้
            }
        }

        public void RespawnPlayer()
        {
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(_respawnDelay);
            TeleportPlayerToSpawn();
        }

        private void TeleportPlayerToSpawn()
        {
            if (_player == null) return;

            // [Modified] เลือกใช้ _currentCheckpoint ถ้ามี, ถ้าไม่มีใช้ Default
            SpawnPoint targetSpawn = _currentCheckpoint != null ? _currentCheckpoint : _defaultSpawnPoint;

            // ถ้าหาไม่เจอเลยจริงๆ ให้ใช้ 0,0,0
            Vector3 targetPos = targetSpawn != null ? targetSpawn.transform.position : Vector3.zero;
            Quaternion targetRot = targetSpawn != null ? targetSpawn.transform.rotation : Quaternion.identity;

            CharacterController charCtrl = _player.GetComponent<CharacterController>();
            if (charCtrl != null) charCtrl.enabled = false;

            _player.transform.position = targetPos;
            _player.transform.rotation = targetRot;

            if (charCtrl != null) charCtrl.enabled = true;

            Debug.Log($"🔄 Player Respawned at {(targetSpawn ? targetSpawn.name : "World Origin")}");
        }

        public void RespawnObject(Rigidbody rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // ส่งของไปที่จุดเกิดล่าสุดเหมือนกัน
            SpawnPoint targetSpawn = _currentCheckpoint != null ? _currentCheckpoint : _defaultSpawnPoint;

            if (targetSpawn != null)
            {
                rb.position = targetSpawn.transform.position + Vector3.up * 2;
                rb.rotation = targetSpawn.transform.rotation;
            }
        }
    }
}