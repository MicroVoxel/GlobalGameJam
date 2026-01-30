using System.Threading;
using Cysharp.Threading.Tasks;
using Core.Patterns;
using UnityEngine;

namespace Core.Systems
{
    /// <summary>
    /// Manages global asynchronous tasks and tokens.
    /// Prevents memory leaks from uncancelled tasks when changing scenes.
    /// </summary>
    public class AsyncManager : Singleton<AsyncManager>
    {
        private CancellationTokenSource _globalCts;

        protected override void Awake()
        {
            base.Awake();
            InitializeToken();
        }

        private void InitializeToken()
        {
            if (_globalCts != null)
            {
                _globalCts.Cancel();
                _globalCts.Dispose();
            }
            _globalCts = new CancellationTokenSource();
        }

        /// <summary>
        /// Gets a token that cancels when the AsyncManager (Game) is destroyed.
        /// </summary>
        public CancellationToken GetGlobalToken() => _globalCts.Token;

        /// <summary>
        /// Utility: Wait for seconds safely (cancelled on destroy).
        /// </summary>
        public async UniTask Delay(float seconds)
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(seconds), cancellationToken: _globalCts.Token);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy(); // Call Singleton's OnDestroy logic if any

            if (_globalCts != null)
            {
                _globalCts.Cancel();
                _globalCts.Dispose();
                _globalCts = null;
            }
        }
    }
}