using UnityEngine;
using UnityEngine.Events;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// React on Connected and Disconnected Events from HMU
    /// </summary>
    public class StylusConnectionEvents : MonoBehaviour
    {
        [SerializeField]
        private HoloStylusManager _manager;

        public UnityEvent OnStylusConnected;
        public UnityEvent OnStylusDisconnected;

        void OnEnable()
        {
            _manager.EventManager.RegisterCallback(StylusEventType.OnStylusConnected, OnConnected);
            _manager.EventManager.RegisterCallback(StylusEventType.OnStylusDisconnected, OnDisconnected);
        }

        void OnDisable()
        {
            _manager.EventManager.UnRegisterCallback(StylusEventType.OnStylusConnected, OnConnected);
            _manager.EventManager.UnRegisterCallback(StylusEventType.OnStylusDisconnected, OnDisconnected);
        }

        private void OnConnected(StylusData data)
        {
            OnStylusConnected?.Invoke();
        }

        private void OnDisconnected(StylusData data)
        {
            OnStylusDisconnected?.Invoke();
        }
    }
}