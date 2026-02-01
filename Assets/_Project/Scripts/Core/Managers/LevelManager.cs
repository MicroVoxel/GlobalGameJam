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
        [SerializeField] private SpawnPoint _defaultSpawnPoint;
        [SerializeField] private float _respawnDelay = 1.0f;

        private PlayerController _player;
        private SpawnPoint _currentCheckpoint;

        protected override void Awake()
        {
            base.Awake();
            _player = FindFirstObjectByType<PlayerController>();
        }

        private void Start()
        {
            if (_defaultSpawnPoint == null)
            {
                _defaultSpawnPoint = FindFirstObjectByType<SpawnPoint>();
            }
            SetCheckpoint(_defaultSpawnPoint);
        }

        public void SetCheckpoint(SpawnPoint newPoint)
        {
            if (newPoint == null) return;

            if (_currentCheckpoint != null && newPoint.Priority < _currentCheckpoint.Priority)
            {
                return;
            }

            if (_currentCheckpoint != newPoint)
            {
                _currentCheckpoint = newPoint;
                Debug.Log($"🚩 Checkpoint Updated: {newPoint.name}");
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

            SpawnPoint targetSpawn = _currentCheckpoint != null ? _currentCheckpoint : _defaultSpawnPoint;

            Vector3 targetPos = targetSpawn != null ? targetSpawn.transform.position : Vector3.zero;
            Quaternion targetRot = targetSpawn != null ? targetSpawn.transform.rotation : Quaternion.identity;

            CharacterController charCtrl = _player.GetComponent<CharacterController>();
            if (charCtrl != null) charCtrl.enabled = false;

            _player.transform.position = targetPos;
            _player.transform.rotation = targetRot;

            if (charCtrl != null) charCtrl.enabled = true;

            Debug.Log($"🔄 Player Respawned at {(targetSpawn ? targetSpawn.name : "World Origin")}");
        }

        // [Updated] ปรับปรุง Logic การ Respawn ของ
        public void RespawnObject(Rigidbody rb)
        {
            // 1. เช็คก่อนว่าวัตถุมีระบบจำตำแหน่งเริ่มต้นไหม (Respawnable)
            var respawnable = rb.GetComponent<Respawnable>();
            if (respawnable != null)
            {
                respawnable.Respawn();
                return; // จบงาน กลับจุดเดิม
            }

            // 2. ถ้าไม่มี ให้ใช้ Logic เดิม (ส่งไปหาผู้เล่นที่ Checkpoint)
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            SpawnPoint targetSpawn = _currentCheckpoint != null ? _currentCheckpoint : _defaultSpawnPoint;

            if (targetSpawn != null)
            {
                // ส่งไปที่จุดเกิดผู้เล่น + สูงขึ้นหน่อยจะได้ไม่ทับคน
                rb.position = targetSpawn.transform.position + Vector3.up * 2;
                rb.rotation = targetSpawn.transform.rotation;
                Debug.Log($"📦 Object '{rb.name}' moved to Checkpoint (No Respawnable script found).");
            }
        }
    }
}