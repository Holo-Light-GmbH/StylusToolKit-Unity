using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine;
using UnityEngine.Events;

namespace HoloLight.STK.Core
{
    /// <summary>
    /// Extend ReceiverBaseMonoBehavior to build external event components
    /// </summary>
    [AddComponentMenu("Scripts/MRTK/Examples/InteractableOnDoubleClick")]
    public class InteractableOnDoubleClick : ReceiverBase
    {
        /// <summary>
        /// The amount of time to press before triggering event
        /// </summary
        [InspectorField(Type = InspectorField.FieldTypes.Float, Label = "Max Delay Time [ms]", Tooltip = "The amount of delay time that can pass to be interpreted as a double-click")]
        public float DelayTime = 300f;

        /// <summary>
        /// Invoked when interactable has been clicked 2 times in a row
        /// </summary>
        public UnityEvent OnDoubleClick => uEvent;

        public InteractableOnDoubleClick(UnityEvent ev) : base(ev, "OnDoubleClick")
        {
        }

        /// <summary>
        /// Called on update, check to see if the state has changed sense the last call
        /// </summary>
        public override void OnUpdate(InteractableStates state, Interactable source)
        {
           
        }

        private float _timeSinceLastClick = 0;

        /// <summary>
        /// click happened
        /// </summary>
        public override void OnClick(InteractableStates state, Interactable source, IMixedRealityPointer pointer = null)
        {
            float timePassed = Time.time - _timeSinceLastClick;
            base.OnClick(state, source);

            if (timePassed < DelayTime / 1000)
            {
                OnDoubleClick?.Invoke();
                _timeSinceLastClick = 0;
            } else { 
                _timeSinceLastClick = Time.time;
            }
        }
    }
}
