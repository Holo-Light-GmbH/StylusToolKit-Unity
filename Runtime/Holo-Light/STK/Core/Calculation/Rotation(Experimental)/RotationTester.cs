
using System;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Core.Rotation
{
    /// <summary>
    /// Script for Testing the Rotation. (Experimental Dev Stuff)
    /// </summary>
    public class RotationTester : MonoBehaviour
    {
        private HoloStylusManager _holoStylusManager;

        [SerializeField]
        private TextMeshPro _text;

        public enum RotationCalcType
        {
            Calculation1 = 0,
            Calculation2 = 1,
            Calculation3 = 2,
            Calculation4 = 3,
            Calculation5 = 4,
            Calculation6 = 5
        }

        public RotationCalcType rotationCalcType = RotationCalcType.Calculation1;


        // original rotation of our object
        Quaternion initialObjRotation;

        // original rotation of the IMU
        Quaternion initialIMURotation;
        bool imuInitialized = false;

        // Temporary quaternion (kept around just to for efficiency)
        Quaternion diff;

        private void Awake()
        {
            _holoStylusManager = GameObject.FindObjectOfType<HoloStylusManager>();

            initialObjRotation = transform.rotation;
            diff = new Quaternion();
        }

        /*
            However sometimes it’s desirable to use Euler angles in your scripts. 
            In this case it’s important to note that you must keep your angles in variables, 
            and only use them to apply them as Euler angles to your rotation. 
            While it’s possible to retrieve Euler angles from a quaternion, if you retrieve, modify and re-apply, problems will arise.         
         */

        private void Update()
        {
            Vector3 stylusRot = _holoStylusManager.StylusTransform.RawRotation;
            Quaternion quatRot = Quaternion.Euler(stylusRot);

            switch(rotationCalcType)
            {
                case RotationCalcType.Calculation1:
                    Calc1(quatRot);
                    break;
                case RotationCalcType.Calculation2:
                    Calc2(quatRot);
                    break;
                case RotationCalcType.Calculation3:
                    Calc3(quatRot);
                    break;
                case RotationCalcType.Calculation4:
                    Calc4(quatRot);
                    break;
                case RotationCalcType.Calculation5: break;
                case RotationCalcType.Calculation6: break;
            }

            // transform.eulerAngles = angle;
           // Quaternion imuRot = Quaternion.Euler(angle);
            //transform.rotation = quat;


          //  transform.rotation = imuRot;// Quaternion.Inverse(); 
            /*
            if (!imuInitialized)
            {
                // This is the first IMU reading; just store it as
                // the initial IMU rotation.
                initialIMURotation = imuRot;
                imuInitialized = true;
            }
            else
            { 
                // This is a subsequent reading; find out how the
                // IMU has changed since the start, and apply that
                // same change to our object.
                //                diff.SetFromToRotation(initialIMURotation.eulerAngles, imuRot.eulerAngles);
                diff = imuRot * Quaternion.Inverse(initialIMURotation);
                transform.rotation = diff * initialIMURotation;
            }
            */

            if (_text != null)
            {
                _text.text = transform.rotation.eulerAngles.ToString("F2");
            }
        }

        /// <summary>
        /// Direct Angle to quaternion converting and assigning it to the object
        /// </summary>
        /// <param name="angle"></param>
        private void Calc1(Quaternion quatRot)
        {
         //   Quaternion calculatedRot = Quaternion.Euler(angle);

            transform.rotation = quatRot;
        }

        Quaternion calc2InitialQuat = new Quaternion(1, 0, 0, 0);
        Quaternion calc2Q1;

        private void Calc2(Quaternion quatRot)
        {
           // Quaternion calculatedRot = Quaternion.Euler(angle);

            transform.rotation = initialObjRotation * quatRot;

            //transform.rotation = calculatedRot;
        }

        private void Calc3(Quaternion quatRot)
        {
            // Quaternion calculatedRot = Quaternion.Euler(angle);
            Quaternion diff = Quaternion.RotateTowards(transform.rotation, quatRot, 1);
            transform.rotation = diff;
            
        }

        private void Calc4(Quaternion quatRot)
        {
            Quaternion diff = Quaternion.Inverse(quatRot);
            transform.rotation = diff;
        }
    }
}