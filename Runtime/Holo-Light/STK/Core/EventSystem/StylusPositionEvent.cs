using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// Attach this component to a gameobject and react on position changes from stylus
    /// </summary>
    public class StylusPositionEvent : MonoBehaviour, IMixedRealityInputHandler<MixedRealityPose>
    {
        public PositionEvent OnStylusPositionChanged;

        void OnEnable()
        {
            CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputHandler<MixedRealityPose>>(this);
        }

        void OnDisable()
        {
            CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputHandler<MixedRealityPose>>(this);
        }

        public void OnInputChanged(InputEventData<MixedRealityPose> eventData)
        {
            if (eventData.MixedRealityInputAction.Description.Contains("Stylus"))
            {
                OnStylusPositionChanged?.Invoke(eventData.InputData.Position);
            }
        }

        [System.Serializable]
        public class PositionEvent : UnityEvent<Vector3> { }
    }
}