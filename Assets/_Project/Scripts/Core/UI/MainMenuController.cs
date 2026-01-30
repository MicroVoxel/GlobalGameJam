using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Core.Managers;
using Core.Data; // Import Constants

namespace Core.UI
{
    /// <summary>
    /// Orchestrates the Main Menu flow including Play, Settings, and Credits.
    /// Follows SOLID: Acts as a mediator between different UI views.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Panel Views")]
        [SerializeField] private CanvasGroup _mainMenuCanvasGroup;
        [SerializeField] private CanvasGroup _settingsCanvasGroup;
        [SerializeField] private CanvasGroup _creditsCanvasGroup;

        [Header("Dependencies")]
        [SerializeField] private SettingsUI _settingsUI;
        [SerializeField] private CreditsUI _creditsUI;

        private void Start()
        {
            InitializeState();
        }

        private void InitializeState()
        {
            // Setup Initial State: Only Main Menu is visible
            SetCanvasGroupState(_mainMenuCanvasGroup, true);
            SetCanvasGroupState(_settingsCanvasGroup, false);
            SetCanvasGroupState(_creditsCanvasGroup, false);

            if (_settingsUI != null) _settingsUI.Initialize();
        }

        // --- Event Methods (Linked to Buttons) ---

        public void OnClickStartGame()
        {
            StartGameRoutine().Forget();
        }

        public void OnClickSettings()
        {
            SwitchPanelAsync(_mainMenuCanvasGroup, _settingsCanvasGroup).Forget();
        }

        public void OnClickCredits()
        {
            SwitchPanelAsync(_mainMenuCanvasGroup, _creditsCanvasGroup, () =>
            {
                // Callback when transition finishes: Start scrolling
                _creditsUI?.PlayCredits();
            }).Forget();
        }

        public void OnClickBackFromSettings()
        {
            SettingsManager.Instance.SaveSettings();
            SwitchPanelAsync(_settingsCanvasGroup, _mainMenuCanvasGroup).Forget();
        }

        public void OnClickBackFromCredits()
        {
            _creditsUI?.StopCredits();
            SwitchPanelAsync(_creditsCanvasGroup, _mainMenuCanvasGroup).Forget();
        }

        public void OnClickQuit()
        {
            QuitGameRoutine().Forget();
        }

        // --- Logic & Transition ---

        private async UniTaskVoid StartGameRoutine()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _mainMenuCanvasGroup.interactable = false;

            await _mainMenuCanvasGroup.DOFade(0f, GameConstants.UI_FADE_DURATION)
                .SetEase(Ease.OutQuad)
                .ToUniTask(cancellationToken: token);

            await SceneManager.LoadSceneAsync(GameConstants.SCENE_GAMEPLAY).ToUniTask(cancellationToken: token);
        }

        private async UniTask SwitchPanelAsync(CanvasGroup from, CanvasGroup to, System.Action onComplete = null)
        {
            var token = this.GetCancellationTokenOnDestroy();
            from.interactable = false;

            // Fade Out
            await from.DOFade(0f, GameConstants.UI_FADE_DURATION)
                .SetEase(Ease.OutQuad)
                .ToUniTask(cancellationToken: token);

            from.gameObject.SetActive(false);

            // Fade In
            to.gameObject.SetActive(true);
            to.alpha = 0f;

            await to.DOFade(1f, GameConstants.UI_FADE_DURATION)
                .SetEase(Ease.OutQuad)
                .ToUniTask(cancellationToken: token);

            to.interactable = true;
            to.blocksRaycasts = true;

            onComplete?.Invoke();
        }

        private async UniTaskVoid QuitGameRoutine()
        {
            var token = this.GetCancellationTokenOnDestroy();
            _mainMenuCanvasGroup.interactable = false;

            await _mainMenuCanvasGroup.DOFade(0f, GameConstants.UI_FADE_DURATION)
                .ToUniTask(cancellationToken: token);

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        private void SetCanvasGroupState(CanvasGroup group, bool isActive)
        {
            if (group == null) return;
            group.alpha = isActive ? 1f : 0f;
            group.interactable = isActive;
            group.blocksRaycasts = isActive;
            group.gameObject.SetActive(isActive);
        }
    }
}