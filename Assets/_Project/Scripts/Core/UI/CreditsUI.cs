using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Core.UI
{
    /// <summary>
    /// Handles the behavior of the Credits panel, specifically the scrolling text.
    /// </summary>
    public class CreditsUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _content;

        [Header("Settings")]
        [SerializeField] private float _scrollSpeed = 50f; // Pixels per second
        [SerializeField] private float _resetDelay = 1f;

        private Tween _scrollTween;
        private float _initialY;

        private void Awake()
        {
            if (_content != null)
                _initialY = _content.anchoredPosition.y;
        }

        public void PlayCredits()
        {
            // Reset position
            ResetPosition();

            // Calculate duration based on height and speed (Distance / Speed = Time)
            float distance = _content.rect.height - _scrollRect.viewport.rect.height;
            if (distance <= 0) return; // Content is smaller than view, no need to scroll

            float duration = distance / _scrollSpeed;

            // Simple Linear Tween
            _scrollTween = _content.DOAnchorPosY(distance + _initialY, duration)
                .SetEase(Ease.Linear)
                .SetDelay(_resetDelay)
                .SetUpdate(true); // Ignore TimeScale if game is paused
        }

        public void StopCredits()
        {
            _scrollTween?.Kill();
            ResetPosition();
        }

        private void ResetPosition()
        {
            if (_content != null)
                _content.anchoredPosition = new Vector2(_content.anchoredPosition.x, _initialY);
        }

        private void OnDestroy()
        {
            _scrollTween?.Kill();
        }
    }
}