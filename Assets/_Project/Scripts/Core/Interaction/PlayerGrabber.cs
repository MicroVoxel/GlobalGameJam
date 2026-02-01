using UnityEngine;

namespace Core.Interaction
{
    /// <summary>
    /// จัดการ Logic การถือของ (Physics Grabbing)
    /// Updated: Added Audio Feedback
    /// </summary>
    [RequireComponent(typeof(AudioSource))] // เพิ่ม AudioSource
    public class PlayerGrabber : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Transform _holdPoint;
        [SerializeField] private float _moveSpeed = 100f;
        [SerializeField] private float _rotateSpeed = 20f;
        [SerializeField] private float _throwForce = 10f;

        [Header("Audio")]
        [SerializeField] private AudioClip _grabSound;
        [SerializeField] private AudioClip _throwSound;
        [SerializeField] private float _sfxVolume = 0.8f;

        private AudioSource _audioSource;

        public GrabbableObject CurrentHeldObject { get; private set; }
        public bool IsGrabbing => CurrentHeldObject != null;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void FixedUpdate()
        {
            if (CurrentHeldObject != null)
            {
                MoveObjectToHoldPoint();
            }
        }

        public void Grab(GrabbableObject obj)
        {
            if (IsGrabbing) Drop();

            CurrentHeldObject = obj;
            CurrentHeldObject.OnGrabbed(this);

            // Play Sound
            PlaySound(_grabSound);
        }

        public void Drop()
        {
            if (CurrentHeldObject != null)
            {
                CurrentHeldObject.OnDropped();
                CurrentHeldObject = null;
                // อาจจะใส่เสียงวาง (DropSound) ตรงนี้ได้ถ้าต้องการ
            }
        }

        public void Throw()
        {
            if (CurrentHeldObject != null)
            {
                GrabbableObject obj = CurrentHeldObject;
                Drop();

                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddForce(_holdPoint.forward * _throwForce, ForceMode.Impulse);

                    // Play Sound
                    PlaySound(_throwSound);
                }
            }
        }

        private void MoveObjectToHoldPoint()
        {
            if (CurrentHeldObject == null) return;

            if (CurrentHeldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.MoveRotation(_holdPoint.rotation);
                Vector3 direction = _holdPoint.position - rb.position;
                float distance = direction.magnitude;
                rb.linearVelocity = direction.normalized * (distance * _moveSpeed);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(clip, _sfxVolume);
            }
        }
    }
}