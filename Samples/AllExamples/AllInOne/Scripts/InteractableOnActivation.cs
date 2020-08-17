using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities.Editor;
using UnityEngine.Events;

namespace HoloLight.STK.Examples.AllExamples
{
    /// <summary>
    /// A basic activate/deactivate event receiver from Interactable
    /// When the gameobject gets enabled or disable these events are triggered
    /// </summary>
    public class InteractableOnActivation : ReceiverBaseMonoBehavior
    {
        /// <summary>
        /// Invoked on Enable
        /// </summary>
        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Enable", Tooltip = "Gameobject was activated")]
        public UnityEvent OnEnableEvent = new UnityEvent();

        /// <summary>
        /// Invoked on Disable
        /// </summary>
        [InspectorField(Type = InspectorField.FieldTypes.Event, Label = "On Disable", Tooltip = "Gameobject was deactivated")]
        public UnityEvent OnDisableEvent = new UnityEvent();

        private void OnEnable()
        {
            base.OnEnable();

            OnEnableEvent.Invoke();
        }
        private void OnDisable()
        {
            base.OnEnable();

            OnDisableEvent.Invoke();
        }

    }
}
