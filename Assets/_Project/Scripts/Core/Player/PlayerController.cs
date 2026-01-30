using UnityEngine;
using Core.Input;
using Core.StateMachine;
using System;

namespace Core.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameInputReader _inputReader;
        [SerializeField] private PlayerConfig _config;

        [Header("Camera Setup")]
        [SerializeField] private GameObject _cameraRoot;
        public Transform CameraRoot => _cameraRoot.transform;

        [Header("Visuals")]
        [Tooltip("LAK MODEL TO HERE! ลาก Model ที่มี Animator มาใส่ตรงนี้")]
        [SerializeField] private Animator _animator;

        // Components
        private CharacterController _characterController;
        private PlayerStateMachine _stateMachine;

        // Runtime Variables
        private Vector2 _currentInputVector;
        private Vector2 _currentLookVector;
        private Vector3 _velocity;
        private float _cinemachineTargetPitch;

        public Vector3 Velocity => _characterController.velocity;

        // Animation Hash IDs
        private int _animIDVelocityX;
        private int _animIDVelocityZ;
        private int _animIDCrouch;
        private int _animIDMask;
        private int _animIDGrounded;
        private int _animIDJump;

        // Mask State
        public bool IsMaskEquipped { get; private set; }
        public event Action<bool> OnMaskStateChanged;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _stateMachine = new PlayerStateMachine(this);

            AssignAnimationIDs();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Start()
        {
            if (_config == null) Debug.LogError("❌ PlayerConfig is MISSING!");

            if (_animator == null)
            {
                Debug.LogError("❌ ANIMATOR IS MISSING! Please assign the Player Model's Animator to the PlayerController script.");
                _animator = GetComponentInChildren<Animator>();
                if (_animator != null) Debug.Log("💡 Auto-found Animator in children.");
            }

            _stateMachine.Initialize(new PlayerStandingState(this, _stateMachine, _config));
        }

        private void OnEnable()
        {
            if (_inputReader == null) return;
            _inputReader.EnableInput();
            _inputReader.MoveEvent += OnMove;
            _inputReader.LookEvent += OnLook;
            _inputReader.JumpEvent += OnJump; // [Fix] Subscribe Jump
            _inputReader.CrouchEvent += OnCrouch;
            _inputReader.ToggleMaskEvent += OnToggleMask;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.MoveEvent -= OnMove;
                _inputReader.LookEvent -= OnLook;
                _inputReader.JumpEvent -= OnJump; // [Fix] Unsubscribe Jump
                _inputReader.CrouchEvent -= OnCrouch;
                _inputReader.ToggleMaskEvent -= OnToggleMask;
            }
        }

        private void Update()
        {
            _stateMachine.CurrentState?.Tick();
            ApplyGravity();
            UpdateAnimator();
        }

        private void LateUpdate()
        {
            HandleCameraRotation();
        }

        // --- Animation Logic ---

        private void AssignAnimationIDs()
        {
            _animIDVelocityX = Animator.StringToHash("VelocityX");
            _animIDVelocityZ = Animator.StringToHash("VelocityZ");
            _animIDCrouch = Animator.StringToHash("IsCrouching");
            _animIDMask = Animator.StringToHash("IsMaskEquipped");
            _animIDGrounded = Animator.StringToHash("IsGrounded");
            _animIDJump = Animator.StringToHash("Jump");
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            float targetX = _currentInputVector.x * (_config.WalkSpeed);
            float targetZ = _currentInputVector.y * (_config.WalkSpeed);

            _animator.SetFloat(_animIDVelocityX, targetX, 0.1f, Time.deltaTime);
            _animator.SetFloat(_animIDVelocityZ, targetZ, 0.1f, Time.deltaTime);
            _animator.SetBool(_animIDGrounded, _characterController.isGrounded);
        }

        public void SetCrouchAnimation(bool isCrouching)
        {
            if (_animator) _animator.SetBool(_animIDCrouch, isCrouching);
        }

        public void TriggerJumpAnimation()
        {
            if (_animator) _animator.SetTrigger(_animIDJump);
        }

        // --- Logic Methods ---

        private void HandleCameraRotation()
        {
            if (_currentLookVector.sqrMagnitude >= 0.01f)
            {
                _cinemachineTargetPitch += _currentLookVector.y * _config.LookSensitivityY * -1.0f;
                float rotationVelocity = _currentLookVector.x * _config.LookSensitivityX;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, _config.BottomClamp, _config.TopClamp);

                _cameraRoot.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
                transform.Rotate(Vector3.up * rotationVelocity);
            }
        }

        public void HandleMovement(float speedMultiplier)
        {
            Vector3 inputDirection = transform.right * _currentInputVector.x + transform.forward * _currentInputVector.y;
            inputDirection = inputDirection.normalized;

            _characterController.Move(inputDirection * (_config.WalkSpeed * speedMultiplier) * Time.deltaTime);
        }

        public void SetHeight(float height, Vector3 center)
        {
            _characterController.height = Mathf.Lerp(_characterController.height, height, Time.deltaTime * _config.CrouchTransitionSpeed);
            _characterController.center = Vector3.Lerp(_characterController.center, center, Time.deltaTime * _config.CrouchTransitionSpeed);
        }

        private void ApplyGravity()
        {
            // Reset velocity Y เมื่ออยู่บนพื้นและไม่ได้กำลังกระโดดขึ้น
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _velocity.y += _config.Gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        // --- Event Handlers ---

        private void OnMove(Vector2 input) => _currentInputVector = input;
        private void OnLook(Vector2 input) => _currentLookVector = input;

        private void OnJump()
        {
            // [Fix] Logic การกระโดด
            if (_characterController.isGrounded)
            {
                // สูตรฟิสิกส์: v = sqrt(h * -2 * g)
                _velocity.y = Mathf.Sqrt(_config.JumpHeight * -2f * _config.Gravity);
                TriggerJumpAnimation();
            }
        }

        private void OnCrouch()
        {
            if (_stateMachine.CurrentState is PlayerStandingState)
                _stateMachine.ChangeState(new PlayerCrouchingState(this, _stateMachine, _config));
            else if (_stateMachine.CurrentState is PlayerCrouchingState)
                _stateMachine.ChangeState(new PlayerStandingState(this, _stateMachine, _config));
        }

        private void OnToggleMask()
        {
            IsMaskEquipped = !IsMaskEquipped;
            OnMaskStateChanged?.Invoke(IsMaskEquipped);

            if (_animator) _animator.SetBool(_animIDMask, IsMaskEquipped);
        }
    }
}