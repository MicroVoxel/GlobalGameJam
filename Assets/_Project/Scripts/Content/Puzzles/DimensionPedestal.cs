using UnityEngine;
using Core.World;
using Core.Interaction;
using DG.Tweening;

namespace Content.Puzzles
{
    [RequireComponent(typeof(AudioSource))] // เพิ่ม AudioSource
    public class DimensionPedestal : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("เมื่อวางของแล้ว จะเปลี่ยนของชิ้นนั้นให้ไปอยู่โลกไหน?")]
        [SerializeField] private ObjectRealityType _targetReality;

        [Header("Visuals")]
        [SerializeField] private Transform _visualFeedback;
        [SerializeField] private float _cooldown = 2.0f;

        [Header("Audio")]
        [SerializeField] private AudioClip _convertSound; // เสียงตอนวาร์ปของ

        private float _lastProcessTime;
        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < _lastProcessTime + _cooldown) return;

            if (other.attachedRigidbody == null) return;

            GrabbableObject grabbable = other.GetComponent<GrabbableObject>();
            if (grabbable != null && grabbable.IsHeld)
            {
                return;
            }

            DualObject dualObj = other.GetComponent<DualObject>();
            if (dualObj != null && dualObj.CurrentType != _targetReality)
            {
                ProcessObject(dualObj);
            }
        }

        private void ProcessObject(DualObject obj)
        {
            _lastProcessTime = Time.time;

            if (_visualFeedback)
            {
                _visualFeedback.DOPunchScale(Vector3.one * 0.1f, 0.5f);
            }

            // [New] Play Sound
            if (_audioSource && _convertSound)
            {
                _audioSource.pitch = Random.Range(0.9f, 1.1f); // เสียงแนว Sci-fi หรือ Magic มักจะมีการเปลี่ยน Pitch
                _audioSource.PlayOneShot(_convertSound);
            }

            Debug.Log($"🌀 Transforming object '{obj.name}' to World: {_targetReality}");

            obj.SetRealityType(_targetReality);
        }
    }
}