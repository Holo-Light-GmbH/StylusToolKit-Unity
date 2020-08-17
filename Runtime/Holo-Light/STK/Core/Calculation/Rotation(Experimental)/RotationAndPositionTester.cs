using HoloLight.STK.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLight.STK.Core.Rotation
{
    /// <summary>
    /// Script for Testing the Rotation. (Experimental Dev Stuff)
    /// </summary>
    public class RotationAndPositionTester : MonoBehaviour
    {
        private HoloStylusManager _holoStylusManager;
        private void Awake()
        {
            _holoStylusManager = GameObject.FindObjectOfType<HoloStylusManager>();
        }


        // Update is called once per frame
        void Update()
        {
            Vector3 stylusRot = _holoStylusManager.StylusTransform.RawRotation;
            Vector3 stylusPos = _holoStylusManager.StylusTransform.Position - new Vector3(0.0f, 0, 0);

            // transform.eulerAngles = angle;
            Quaternion quat = Quaternion.Euler(stylusRot);
            transform.position = stylusPos;
            transform.rotation = quat;
        }
    }
}
