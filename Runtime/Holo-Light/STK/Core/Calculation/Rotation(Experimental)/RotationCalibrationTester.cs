
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core.Rotation
{
    /// <summary>
    /// Script for Testing the Rotation. (Experimental Dev Stuff)
    /// </summary>
    public class RotationCalibrationTester : MonoBehaviour
    {
        private HoloStylusManager _holoStylusManager;

        [SerializeField]
        private TextMeshPro _text;

        private void Awake()
        {
            _holoStylusManager = GameObject.FindObjectOfType<HoloStylusManager>();
        }

        public void CalibrateIMU()
        {
            _holoStylusManager.Connector.SendData(new byte[] { 0xCA, 0x51 });
        }

        public void CalibrateOffset()
        {
            Vector3 offset = Camera.main.transform.TransformPoint(_holoStylusManager.StylusTransform.RawRotation - transform.rotation.eulerAngles);
            _holoStylusManager.CalibrationPreferences.SaveOffset(offset);
        }

        public void ResetOffset()
        {
            _holoStylusManager.CalibrationPreferences.SaveOffset(_holoStylusManager.CalibrationPreferences.RotationOffset);
        }
    }
}