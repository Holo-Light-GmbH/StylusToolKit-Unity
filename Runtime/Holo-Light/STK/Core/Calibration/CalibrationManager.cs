
using HoloLight.STK.MRTK;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;
using static HoloLight.STK.Core.CalibrationPreferences;

namespace HoloLight.STK.Core
{
    public enum WhenToTrigger
    {
        OnActionBtn = 0,
        OnBackBtn = 1
    }

    /// <summary>
    /// Adjusting the Stylus Virtual Tip with the Phyisical one 
    /// </summary>
    public class CalibrationManager : MonoBehaviour
    {
        private StylusPokePointer _stylusPokePointer;
        private StylusSpherePointer _stylusSpherePointer;
        private StylusRayPointer _stylusRayPointer;

        [SerializeField] private HoloStylusManager _manager;

        [SerializeField]
        private WhenToTrigger _triggerOn;

        private Camera _camera;

        private Vector3 _startPosition;
        private Vector3 _startRotation;

        private StylusController _stylusController;

        private Transform _cursorOldTransform;

        private StylusSpherePointer _stylusCursor;

        void OnEnable()
        {
            _stylusPokePointer = _manager.PointerSwitcher.GetPokePointer();
            _stylusRayPointer = _manager.PointerSwitcher.GetRayPointer();
            _stylusSpherePointer = _manager.PointerSwitcher.GetSpherePointer();

            _stylusController = _stylusPokePointer.Controller as StylusController;
            if (_stylusController == null)
            {
                _stylusController = _stylusRayPointer.Controller as StylusController;

                if (_stylusController == null)
                {
                    _stylusController = _stylusSpherePointer.Controller as StylusController;

                    if (_stylusController == null)
                    {
                        Debug.LogWarning("The Stylus Pointers are not available. Please check if they are configured in your InputSystem Profile");
                    }
                }
            }

            _camera = Camera.main;
            _stylusCursor = _manager.PointerSwitcher.GetSpherePointer();
        }

        public void StartCalibration()
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }

        public void StopCalibration()
        {
            if (gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
        }

        public void ToggleCalibration()
        {
            if (gameObject.activeSelf)
            {
                StopCalibration();
            }
            else
            {
                StartCalibration();
            }
        }

        /// <summary>
        /// Reset Offset
        /// </summary>
        public void ResetCalibration()
        {
            Vector3 resetCoordinates = _manager.CalibrationPreferences.PositionOffset;
            Vector3 resetRotation = _manager.CalibrationPreferences.RotationOffset;

            UpdateOffset(resetCoordinates, resetRotation);
        }

        public void OnButtonDown(BaseInputEventData inputEventData)
        {
            if (inputEventData.InputSource.SourceName.Contains("Stylus"))
            {
                string compareToString = _triggerOn == 0 ? "Select" : "Stylus Back";
                if (inputEventData.MixedRealityInputAction.Description.Contains(compareToString))
                {
                    _cursorOldTransform = _stylusCursor.transform.parent;
                    _stylusCursor.transform.parent = _camera.transform;

                    _manager.PointerSwitcher.DisablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
                    _startPosition = _camera.transform.InverseTransformPoint(_stylusCursor.transform.position);
                    _startRotation = _manager.StylusTransform.RawRotation;

                    _stylusController.DisablePositionChanges();
                }
            }
        }

        public void OnButtonUp(BaseInputEventData inputEventData)
        {
            if (inputEventData.InputSource.SourceName.Contains("Stylus"))
            {
                string compareToString = _triggerOn == 0 ? "Select" : "Stylus Back";
                if (inputEventData.MixedRealityInputAction.Description.Contains(compareToString))
                {
                    _manager.PointerSwitcher.EnablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
                    Vector3 newPositionOffset = _camera.transform.InverseTransformPoint(_manager.StylusTransform.Position) - _startPosition;
                    Vector3 newRotationOffset = _manager.StylusTransform.RawRotation - _startRotation;

                    UpdateOffset(newPositionOffset, newRotationOffset);

                    _stylusController.EnablePositionChanges();
                    _stylusCursor.transform.parent = _cursorOldTransform;
                }
            }
        }

        void UpdateOffset(Vector3 newPositionOffset, Vector3 newRotationOffset)
        {
            _manager.CalibrationPreferences.SaveCalibration(newPositionOffset, newRotationOffset);
        }

        public void SetStylusHand(int holdingHand)
        {
            _manager.CalibrationPreferences.SetStylusHand((StylusHoldingHand)holdingHand);
        }

    }
}