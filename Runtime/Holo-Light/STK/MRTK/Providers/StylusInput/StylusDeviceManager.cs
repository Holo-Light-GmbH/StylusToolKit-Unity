using HoloLight.STK.Core;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

namespace HoloLight.STK.MRTK
{
    /// <summary>
    /// Manages Stylus Devices on the Windows Mixed Reality platform.
    /// </summary>
    [MixedRealityDataProvider(
        typeof(IMixedRealityInputSystem),
        (SupportedPlatforms)(-1),
        "Stylus Device Manager",
        "STK/MRTK/Profiles/StylusMixedRealityInputProfile.asset",
        "Holo-Light",
        true)]
    public class StylusDeviceManager : BaseInputDeviceManager
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="inputSystem">The <see cref="Microsoft.MixedReality.Toolkit.Input.IMixedRealityInputSystem"/> instance that receives data from this provider.</param>
        /// <param name="name">Friendly name of the service.</param>
        /// <param name="priority">Service priority. Used to determine order of instantiation.</param>
        /// <param name="profile">The service's configuration profile.</param>
        public StylusDeviceManager(
            IMixedRealityInputSystem inputSystem,
            string name = null,
            uint priority = DefaultPriority,
            BaseMixedRealityProfile profile = null) : base(inputSystem, name, priority, profile)
        {
        }


        /// <summary>
        /// Return the service profile and ensure that the type is correct.
        /// </summary>
        public StylusMixedRealityInputProfile StylusInputProfile
        {
            get
            {
                var profile = ConfigurationProfile as StylusMixedRealityInputProfile;
                if (!profile)
                {
                    Debug.LogError("Profile for Stylus Device Manager must be a StylusMixedRealityInputProfile");
                }
                return profile;
            }
        }

        /// <summary>
        /// Current Stylus Controller.
        /// </summary>
        public StylusController Controller { get; private set; }

        /// <summary>
        /// The HoloStylusManager Object.
        /// </summary>
        public HoloStylusManager HoloStylusManager { get; private set; }


        /// <inheritdoc />
        public override void Update()
        {

            base.Update();

            // get stylus data and raise events to the controller..

            if (Controller == null) { Enable(); }

            Controller?.Update();
        }


        /// <inheritdoc />
        public override void Enable()
        {
            base.Enable();

            if (Controller != null)
            {
                // Stylus Device Manager has already been set up
                return;
            }

            IMixedRealityInputSource stylusInputSource = null;

            const Handedness handedness = Handedness.Any;
            System.Type controllerType = typeof(StylusController);

            // Make sure that the handedness declared in the controller attribute matches what we expect
            var controllerAttribute = MixedRealityControllerAttribute.Find(controllerType);
            if (controllerAttribute != null)
            {
                Handedness[] handednesses = controllerAttribute.SupportedHandedness;
                Debug.Assert(handednesses.Length == 1 && handednesses[0] == Handedness.Any, "Unexpected stylus handedness declared in MixedRealityControllerAttribute");
            }

            if (Service != null)
            {
                var pointers = RequestPointers(SupportedControllerType.Stylus, handedness);
                // this is the string i get from input source... string is somehow 
                stylusInputSource = Service.RequestNewGenericInputSource("Stylus Input Source", pointers);
            }

            Controller = new StylusController(TrackingState.Tracked, handedness, stylusInputSource);
            StylusData defaultStylusData = new StylusData();

            defaultStylusData.Position = Vector3.zero;
            defaultStylusData.Buttons[0] = false;
            defaultStylusData.Buttons[1] = false;

            Controller.StylusData = defaultStylusData;
            if (stylusInputSource != null)
            {
                for (int i = 0; i < stylusInputSource.Pointers.Length; i++)
                {
                    stylusInputSource.Pointers[i].Controller = Controller;
                }
            }

            Controller.SetupConfiguration(typeof(StylusController));
            Controller.SetupDefaultInteractions(handedness);

            if (HoloStylusManager == null)
            {
                GameObject stylusGO = GameObject.Find("Stylus");
                if (stylusGO != null)
                {
                    HoloStylusManager = stylusGO.GetComponent<HoloStylusManager>();
                }

                if (HoloStylusManager == null)
                {
                    HoloStylusManager = GameObject.FindObjectOfType<HoloStylusManager>();
                }

                if (HoloStylusManager == null)
                {
                    Debug.LogError("HoloStylusManager Comonent was not found in the Scene. Please add the Stylus Prefab to your Scene");
                }

                EnableEvents();
            }
        }

        /// <inheritdoc />
        public override void Disable()
        {
            base.Disable();

            if (HoloStylusManager != null)
            {
                DisableEvents();

                HoloStylusManager = null;
            }

            if (Controller != null)
            {
                if (HoloStylusManager != null)
                {
                    Service?.RaiseSourceLost(Controller.InputSource, Controller);
                }

                RecyclePointers(Controller.InputSource);

                Controller = null;
            }
        }

        public void EnableEvents()
        {
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnActionButtonDown, UpdateStylusData);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnActionButtonUp, UpdateStylusData);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnBackButtonDown, UpdateStylusData);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnBackButtonUp, UpdateStylusData);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnPositionChanged, UpdateStylusData);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnStylusConnected, OnStylusConnected);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnStylusDisconnected, OnStylusDisConnected);
            HoloStylusManager.EventManager.RegisterCallback(StylusEventType.OnStylusPreferedHandChanged, OnStylusHandChanged);
        }

        public void DisableEvents()
        {
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnActionButtonDown, UpdateStylusData);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnActionButtonUp, UpdateStylusData);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnBackButtonDown, UpdateStylusData);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnBackButtonUp, UpdateStylusData);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnPositionChanged, UpdateStylusData);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnStylusConnected, OnStylusConnected);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnStylusDisconnected, OnStylusDisConnected);
            HoloStylusManager.EventManager.UnRegisterCallback(StylusEventType.OnStylusPreferedHandChanged, OnStylusHandChanged);
        }

        #region STYLUS EVENTS  

        /// <summary>
        /// Triggers, when the Application recieves the first data from the HMU
        /// </summary>
        /// <param name="data"></param>
        private void OnStylusConnected(StylusData data)
        {
            Service?.RaiseSourceDetected(Controller.InputSource, Controller);
        }

        /// <summary>
        /// Triggers, when the Application doesn't recieve stylus stream for 3s
        /// </summary>
        /// <param name="data"></param>
        private void OnStylusDisConnected(StylusData data)
        {
            if (Controller != null)
            {
                Service?.RaiseSourceLost(Controller.InputSource, Controller);
            }
        }

        /// <summary>
        /// Controller gets new stylus data
        /// </summary>
        /// <param name="newStylusData"></param>
        private void UpdateStylusData(StylusData newStylusData)
        {
            Controller.StylusData = newStylusData;
        }

        private void OnStylusHandChanged(StylusData data)
        {
            Controller.HoldingHand = HoloStylusManager.CalibrationPreferences.StylusPreferredHand;
        }

        #endregion
    }
}