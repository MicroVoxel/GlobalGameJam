using UnityEngine;

namespace Core.Player
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAudio : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController _controller;
        [SerializeField] private CharacterController _characterController;

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;
        [Range(0f, 1f)][SerializeField] private float _volume = 0.5f;
        [SerializeField] private float _stepDistance = 1.8f;

        [Tooltip("เวลาขั้นต่ำระหว่างก้าว (กันเสียงรัวเป็นปืนกล)")]
        [SerializeField] private float _minStepInterval = 0.3f;

        [Tooltip("ถ้าติ๊กถูก จะตัดเสียงก้าวเก่าทิ้งทันทีเมื่อก้าวใหม่ (เสียงจะไม่ซ้อนกัน แต่หางเสียงจะหาย)")]
        [SerializeField] private bool _cutPreviousSound = false;

        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = true;

        private AudioSource _audioSource;
        private float _distanceTraveled;
        private bool _isMoving;
        private float _debugTimer;
        private Vector3 _lastPosition;

        // [New] ตัวแปรสำหรับแก้ปัญหาเสียงรัว/ซ้อน
        private float _lastStepTime;
        private float _stopTimer; // ตัวนับเวลาหยุดเดิน (Debounce)

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_controller == null) _controller = GetComponentInParent<PlayerController>();
            if (_characterController == null) _characterController = GetComponentInParent<CharacterController>();
        }

        private void Start()
        {
            if (_showDebugInfo) Debug.Log($"🎧 PlayerAudio Started on {gameObject.name}");
            _lastPosition = transform.position;
        }

        private void Update()
        {
            HandleFootsteps();
        }

        private void HandleFootsteps()
        {
            if (_controller == null || _characterController == null) return;

            // คำนวณความเร็ว (Manual Calculation)
            Vector3 currentPosition = transform.position;
            Vector3 horizontalDelta = new Vector3(currentPosition.x - _lastPosition.x, 0, currentPosition.z - _lastPosition.z);
            float actualSpeed = horizontalDelta.magnitude / Time.deltaTime;
            _lastPosition = currentPosition;

            bool isGrounded = _characterController.isGrounded;

            // Debug
            if (_showDebugInfo)
            {
                _debugTimer += Time.deltaTime;
                if (_debugTimer > 0.5f)
                {
                    _debugTimer = 0;
                    // Debug.Log($"🔍 Audio Status -> Grounded: {isGrounded} | Real Speed: {actualSpeed:F3}");
                }
            }

            // Logic: ต้องอยู่บนพื้น และ มีความเร็ว
            if (isGrounded && actualSpeed > 0.1f)
            {
                _stopTimer = 0f; // รีเซ็ตตัวจับเวลาหยุดเดิน

                if (!_isMoving)
                {
                    _isMoving = true;
                    // เริ่มต้นที่ระยะเกือบถึง เพื่อให้ก้าวแรกดังเร็วขึ้น
                    _distanceTraveled = _stepDistance * 0.9f;
                }

                _distanceTraveled += actualSpeed * Time.deltaTime;

                // [Fix] เพิ่มเงื่อนไข Time.time - _lastStepTime เพื่อกันเสียงรัวเกินไป
                if (_distanceTraveled >= _stepDistance && (Time.time - _lastStepTime > _minStepInterval))
                {
                    PlayFootstep();
                    _distanceTraveled = 0f;
                    _lastStepTime = Time.time;
                }
            }
            else
            {
                // [Fix] Stop Debounce: ต้องหยุดนิ่งเกิน 0.2 วิ ถึงจะถือว่าหยุดจริง
                // (กันกรณีความเร็วแกว่ง 0 -> 0.1 -> 0 ซึ่งทำให้ระบบรีเซ็ต _isMoving รัวๆ)
                _stopTimer += Time.deltaTime;
                if (_stopTimer > 0.2f)
                {
                    _isMoving = false;
                    _distanceTraveled = 0f;
                }
            }
        }

        private void PlayFootstep()
        {
            if (_footstepClips == null || _footstepClips.Length == 0) return;

            int index = Random.Range(0, _footstepClips.Length);
            AudioClip clip = _footstepClips[index];

            if (clip != null)
            {
                _audioSource.pitch = Random.Range(0.9f, 1.1f);

                if (_cutPreviousSound)
                {
                    // หยุดเสียงเก่าก่อนเล่นเสียงใหม่ (แก้เสียงซ้อนแบบเด็ดขาด)
                    _audioSource.Stop();
                    _audioSource.clip = clip;
                    _audioSource.volume = _volume;
                    _audioSource.Play();
                }
                else
                {
                    // ปล่อยให้ซ้อนกันได้เล็กน้อย (ธรรมชาติกว่าสำหรับเสียงเดิน)
                    _audioSource.PlayOneShot(clip, _volume);
                }
            }
        }
    }
}