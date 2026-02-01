using UnityEngine;
using Core.Input;
using Core.StateMachine;
using Core.Managers;
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
        [SerializeField] private Animator _animator;
        [SerializeField] private float _modelYOffset = 0f;

        [Header("Animation Settings")]
        [Tooltip("Cooldown กันกดรัว")]
        [SerializeField] private float _maskCooldown = 1.5f;

        [Header("Collider Profiles")]
        [SerializeField] private CapsuleCollider _standingProfile;
        [SerializeField] private CapsuleCollider _crouchingProfile;

        // Components
        private CharacterController _characterController;
        private PlayerStateMachine _stateMachine;

        // Runtime Variables
        private Vector2 _currentInputVector;
        private Vector2 _currentLookVector;
        private Vector3 _velocity;
        private float _cinemachineTargetPitch;

        private bool _isTogglingMask = false;
        private float _lastToggleTime;
        private bool _isEquippingSequence;
        private bool _inputLocked = false;

        private Vector3 _initialModelLocalPos;
        private Transform _modelTransform;

        public Vector3 Velocity => _characterController.velocity;

        public bool IsCrouching { get; private set; }
        public bool IsMaskEquipped { get; private set; }

        // Events
        public event Action<int> OnMoveMaskTo;
        public event Action<bool> OnCrouchChanged;

        // Animation Hash IDs
        private int _animIDVelocityX;
        private int _animIDVelocityZ;
        private int _animIDCrouch;
        private int _animIDMask;
        private int _animIDToggleMask;
        private int _animIDGrounded;
        private int _animIDJump;

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
            if (_animator == null) _animator = GetComponentInChildren<Animator>();

            if (_animator != null)
            {
                _modelTransform = _animator.transform;
                _initialModelLocalPos = _modelTransform.localPosition;
                ApplyModelOffset();
            }

            if (_standingProfile != null)
            {
                _characterController.center = _standingProfile.center;
                _characterController.height = _standingProfile.height;
                _characterController.radius = _standingProfile.radius;
            }

            _stateMachine.Initialize(new PlayerStandingState(this, _stateMachine, _config));

            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged += OnRealityStateChanged;
        }

        public void SetControlLock(bool isLocked)
        {
            _inputLocked = isLocked;
            if (isLocked)
            {
                _currentInputVector = Vector2.zero;
                UpdateAnimator();
            }
        }

        public async Cysharp.Threading.Tasks.UniTask ForceUnequip()
        {
            if (!IsMaskEquipped || _isTogglingMask) return;
            OnToggleMask();
            await Cysharp.Threading.Tasks.UniTask.Delay(TimeSpan.FromSeconds(_maskCooldown));
        }

        private void ApplyModelOffset()
        {
            if (_modelTransform != null)
            {
                _modelTransform.localPosition = new Vector3(
                    _initialModelLocalPos.x,
                    _initialModelLocalPos.y + _modelYOffset,
                    _initialModelLocalPos.z
                );
            }
        }

        private void OnEnable()
        {
            if (_inputReader == null) return;
            _inputReader.EnableInput();
            _inputReader.MoveEvent += OnMove;
            _inputReader.LookEvent += OnLook;
            _inputReader.JumpEvent += OnJump;
            _inputReader.CrouchEvent += OnCrouch;
            _inputReader.ToggleMaskEvent += OnToggleMask;
        }

        private void OnDisable()
        {
            if (_inputReader != null)
            {
                _inputReader.MoveEvent -= OnMove;
                _inputReader.LookEvent -= OnLook;
                _inputReader.JumpEvent -= OnJump;
                _inputReader.CrouchEvent -= OnCrouch;
                _inputReader.ToggleMaskEvent -= OnToggleMask;
            }

            if (RealityManager.Instance != null)
                RealityManager.Instance.OnRealityChanged -= OnRealityStateChanged;
        }

        private void Update()
        {
            _stateMachine.CurrentState?.Tick();
            ApplyGravity();
            UpdateAnimator();
            UpdateColliderShape();

#if UNITY_EDITOR
            if (_modelTransform != null) ApplyModelOffset();
#endif
        }

        private void LateUpdate()
        {
            if (!_inputLocked) HandleCameraRotation();
        }

        public void AnimEvent_GrabMask()
        {
            if (!_isTogglingMask) return;

            if (_isEquippingSequence) OnMoveMaskTo?.Invoke(1);
            else OnMoveMaskTo?.Invoke(0);
        }

        public void AnimEvent_EquipMask()
        {
            if (!_isTogglingMask) return;

            if (_isEquippingSequence)
            {
                OnMoveMaskTo?.Invoke(2);
                if (!IsMaskEquipped) RealityManager.Instance?.ToggleReality();
            }
            else
            {
                OnMoveMaskTo?.Invoke(1);
                if (IsMaskEquipped) RealityManager.Instance?.ToggleReality();
            }
        }

        public void AnimEvent_Finish()
        {
            _isTogglingMask = false;

            if (_isEquippingSequence)
            {
                OnMoveMaskTo?.Invoke(2);
                if (!IsMaskEquipped) RealityManager.Instance?.ToggleReality();
            }
            else
            {
                OnMoveMaskTo?.Invoke(0);
                if (IsMaskEquipped) RealityManager.Instance?.ToggleReality();
            }
        }

        private void OnToggleMask()
        {
            if (_inputLocked || _isTogglingMask || Time.time < _lastToggleTime + _maskCooldown) return;

            _isTogglingMask = true;
            _lastToggleTime = Time.time;

            _isEquippingSequence = !IsMaskEquipped;

            TriggerToggleMaskAnimation();
        }

        private void UpdateColliderShape()
        {
            if (_standingProfile == null || _crouchingProfile == null) return;
            CapsuleCollider targetProfile = IsCrouching ? _crouchingProfile : _standingProfile;
            float transitionSpeed = _config.CrouchTransitionSpeed * Time.deltaTime;
            _characterController.height = Mathf.Lerp(_characterController.height, targetProfile.height, transitionSpeed);
            _characterController.center = Vector3.Lerp(_characterController.center, targetProfile.center, transitionSpeed);
            _characterController.radius = Mathf.Lerp(_characterController.radius, targetProfile.radius, transitionSpeed);
        }

        private void HandleCameraRotation()
        {
            if (_currentLookVector.sqrMagnitude < 0.0001f) return;
            float topClamp = (_config.TopClamp == 0) ? 90f : _config.TopClamp;
            float bottomClamp = (_config.BottomClamp == 0) ? -90f : _config.BottomClamp;
            _cinemachineTargetPitch += _currentLookVector.y * _config.LookSensitivityY * -1.0f;
            float rotationVelocity = _currentLookVector.x * _config.LookSensitivityX;
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, bottomClamp, topClamp);
            _cameraRoot.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * rotationVelocity);
        }

        public void HandleMovement(float speedMultiplier)
        {
            if (_inputLocked)
            {
                _characterController.Move(Vector3.zero);
                return;
            }

            Vector3 inputDirection = transform.right * _currentInputVector.x + transform.forward * _currentInputVector.y;
            inputDirection = inputDirection.normalized;
            _characterController.Move(inputDirection * (_config.WalkSpeed * speedMultiplier) * Time.deltaTime);
        }

        private void ApplyGravity()
        {
            if (_characterController.isGrounded && _velocity.y < 0) _velocity.y = -2f;
            _velocity.y += _config.Gravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void AssignAnimationIDs()
        {
            _animIDVelocityX = Animator.StringToHash("VelocityX");
            _animIDVelocityZ = Animator.StringToHash("VelocityZ");
            _animIDCrouch = Animator.StringToHash("IsCrouching");
            _animIDMask = Animator.StringToHash("IsMaskEquipped");
            _animIDToggleMask = Animator.StringToHash("ToggleMask");
            _animIDGrounded = Animator.StringToHash("IsGrounded");
            _animIDJump = Animator.StringToHash("Jump");
        }

        private void UpdateAnimator()
        {
            if (_animator == null) return;

            float targetX = _inputLocked ? 0 : _currentInputVector.x * (_config.WalkSpeed);
            float targetZ = _inputLocked ? 0 : _currentInputVector.y * (_config.WalkSpeed);

            _animator.SetFloat(_animIDVelocityX, targetX, 0.1f, Time.deltaTime);
            _animator.SetFloat(_animIDVelocityZ, targetZ, 0.1f, Time.deltaTime);
            _animator.SetBool(_animIDGrounded, _characterController.isGrounded);
        }

        public void SetCrouchState(bool isCrouching)
        {
            if (_inputLocked) return;
            if (IsCrouching == isCrouching) return;
            IsCrouching = isCrouching;
            _animator?.SetBool(_animIDCrouch, isCrouching);
            OnCrouchChanged?.Invoke(isCrouching);
        }

        public void TriggerJumpAnimation() => _animator?.SetTrigger(_animIDJump);
        public void TriggerToggleMaskAnimation() => _animator?.SetTrigger(_animIDToggleMask);

        private void OnMove(Vector2 input) => _currentInputVector = input;
        private void OnLook(Vector2 input) => _currentLookVector = input;

        private void OnJump()
        {
            if (_inputLocked) return;

            // [Check] ตรวจสอบ Config ว่าอนุญาตให้กระโดดไหม
            if (!_config.CanJump) return;

            if (_characterController.isGrounded && !IsCrouching)
            {
                _velocity.y = Mathf.Sqrt(_config.JumpHeight * -2f * _config.Gravity);
                TriggerJumpAnimation();
            }
        }

        private void OnCrouch()
        {
            if (_inputLocked) return;

            // [Check] ตรวจสอบ Config ว่าอนุญาตให้ย่อไหม
            if (!_config.CanCrouch) return;

            if (_stateMachine.CurrentState is PlayerStandingState)
                _stateMachine.ChangeState(new PlayerCrouchingState(this, _stateMachine, _config));
            else if (_stateMachine.CurrentState is PlayerCrouchingState)
                _stateMachine.ChangeState(new PlayerStandingState(this, _stateMachine, _config));
        }

        private void OnRealityStateChanged(bool isEquipped)
        {
            IsMaskEquipped = isEquipped;
            if (_animator) _animator.SetBool(_animIDMask, IsMaskEquipped);
        }
    }
}