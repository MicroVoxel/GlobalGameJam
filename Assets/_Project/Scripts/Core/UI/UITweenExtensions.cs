using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Core.UI
{
    /// <summary>
    /// Extension methods for clean UI animations using DOTween and UniTask.
    /// Removes boilerplate code from Controllers.
    /// </summary>
    public static class UITweenExtensions
    {
        public static async UniTask FadeInAsync(this CanvasGroup canvasGroup, float duration = 0.5f, Ease ease = Ease.OutCubic)
        {
            if (canvasGroup == null) return;

            canvasGroup.DOKill(); // Clean up old tweens
            canvasGroup.gameObject.SetActive(true);

            // Reset state if coming from completely hidden
            if (canvasGroup.alpha == 0) canvasGroup.alpha = 0;

            await canvasGroup.DOFade(1f, duration)
                .SetEase(ease)
                .ToUniTask(); // Wait for completion

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public static async UniTask FadeOutAsync(this CanvasGroup canvasGroup, float duration = 0.5f, Ease ease = Ease.InCubic)
        {
            if (canvasGroup == null) return;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            canvasGroup.DOKill();

            await canvasGroup.DOFade(0f, duration)
                .SetEase(ease)
                .ToUniTask();

            canvasGroup.gameObject.SetActive(false);
        }

        public static void SetAlpha(this CanvasGroup canvasGroup, float alpha)
        {
            if (canvasGroup == null) return;
            canvasGroup.alpha = alpha;
        }
    }
}