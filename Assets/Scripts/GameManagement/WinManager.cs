using System;
using UnityEngine;

namespace GameCore
{
    public sealed class WinManager : MonoBehaviour
    {
        public event Action OnWinned;

        [SerializeField]
        private GameObject _winWindow;

        public void MakeOnWin()
        {
            _winWindow.SetActive(true);

            OnWinned?.Invoke();
        }
    }
}