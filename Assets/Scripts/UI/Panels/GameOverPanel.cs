using System;
using UnityEngine;

namespace UI.Panels
{
    public class GameOverPanel : Panel
    {
        public static event Action OnRetryRequested;

        public void Retry()
        {
            OnRetryRequested?.Invoke();
        }
    }
}