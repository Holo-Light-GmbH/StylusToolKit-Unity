using HoloLight.STK.MRTK;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HoloLight.STK.Core.Emulator
{
    /// <summary>
    /// Mouse and Keyboard emulation of the stylus.
    /// </summary>
    public class MouseStylusControl : MonoBehaviour
    {

#if UNITY_EDITOR
        /// <summary>
        /// Used to get the configuration informations for the Unity Mouse Simulator
        /// </summary>
        private StylusMixedRealityInputProfile profile;

        // Current depth of the tip
        private float _currentDepth;

        // Used camera for local transform data.
        private Camera _camera;

        private HoloStylusManager _manager;

        public void Activate(HoloStylusManager manager)
        {
            _manager = manager;

            MixedRealityInputDataProviderConfiguration[] inputProfiles = CoreServices.InputSystem.InputSystemProfile.DataProviderConfigurations;

            for (int i = 0; i < inputProfiles.Length; i++)
            {
                if (inputProfiles[i].DeviceManagerProfile != null)
                {
                    if (inputProfiles[i].DeviceManagerProfile.GetType() == typeof(StylusMixedRealityInputProfile))
                    {
                        profile = inputProfiles[i].DeviceManagerProfile as StylusMixedRealityInputProfile;
                        break;
                    }
                }
            }

            if (profile == null)
            {
                Debug.LogError("StylusMixedRealityInputProfile could not be found");
            }
            else
            {
                gameObject.SetActive(true);
            }
        }

        public void DeActivate()
        {
            gameObject.SetActive(false);
        }

        // Set up the emulation
        private void Start()
        {
            // Find the camera
            _camera = Camera.main;
            if (_camera == null)
            {
                _camera = FindObjectOfType<Camera>();
                if (_camera == null)
                {
                    enabled = false;
                }
            }

            _currentDepth = Vector3.Distance(Vector3.forward * profile.StartingDistance, _camera.transform.position);
        }

        void FixedUpdate()
        {
            if (KeyInputSystem.GetKey(profile.StylusForwardKey))
            {
                _currentDepth += Time.fixedDeltaTime * profile.DepthSpeed;
            }
            else if (KeyInputSystem.GetKey(profile.StylusBackwardKey))
            {
                _currentDepth -= Time.fixedDeltaTime * profile.DepthSpeed;
            }

            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, _currentDepth);

            Vector3 fromPosition = _camera.transform.position;
            Vector3 toPosition = _camera.ScreenToWorldPoint(mousePosition);

            Vector3 direction = toPosition - fromPosition;
            var ray = new Ray(fromPosition, direction);

            var currentPosition = ray.GetPoint(_currentDepth);

            bool actionButton = Input.GetMouseButton(0);
            bool backButton = Input.GetMouseButton(1);

            StylusData stylusData = new StylusData();
            stylusData.Buttons[0] = actionButton;
            stylusData.Buttons[1] = backButton;
            stylusData.Position = currentPosition;

            stylusData = _manager.StylusTransform.GetDataForEmulatorV2(stylusData);

            _manager.EventManager.PushData(stylusData);
        }
#else
        void Awake() {
            Destroy(gameObject, 0.05f);
            return;
        }
#endif
    }
}