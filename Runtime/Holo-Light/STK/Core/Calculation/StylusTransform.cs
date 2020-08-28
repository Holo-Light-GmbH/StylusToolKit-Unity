using System;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLight.STK.Core
{
    public class StylusTransform
    {
        #region CAMERA MOVEMENT FILTER 

        private const int TRANSFORM_QUEUE_LENGTH = 2;

        internal class TransformQueueElement
        {
            public Vector3 Position = new Vector3(0, 0, 0);
            public Quaternion Rotation = new Quaternion(0, 0, 0, 0);
        }

        private Transform _oldTransformInternal;
        private Queue<TransformQueueElement> _oldTransformsQueue = new Queue<TransformQueueElement>();
        private Transform _oldTransform
        {
            get
            {
                if (_oldTransformInternal == null)
                {
                    var oldtransform = new GameObject("StylusTransformPositionHelper");
                    _oldTransformInternal = oldtransform.transform;
                }
                return _oldTransformInternal;
            }
        }

        private int _queueCounter = 0;

        #endregion

        /// <summary>
        /// This value is getting updated from the HoloStylusManager. Affect the Filter Responsiveness
        /// </summary>
        private float _multiplier;
        private float _defaultMultiplier;
        private float _additor = 0.1f;

        private CalibrationPreferences _calibrationPreferences;
        private Transform _cameraTransform;
        public Vector3 RawPosition { get; private set; } = Vector3.zero;
        public Vector3 Position { get; private set; } = Vector3.zero;
        public Vector3 LastPosition { get; private set; } = Vector3.zero;

        public Vector3 RawRotation { get; private set; } = Vector3.zero;
        public Vector3 Rotation { get; private set; } = Vector3.zero;
        public Vector3 LastRotation { get; private set; } = Vector3.zero;

        public void Init(HoloStylusManager manager)
        {
            _calibrationPreferences = manager.CalibrationPreferences;
            _cameraTransform = Camera.main.transform;
            _multiplier = manager.StylusConfiguration.Smoothness;
            _defaultMultiplier = _multiplier;
        }

        /// <summary>
        /// Sets the multiplier for the smoothfilter
        /// </summary>
        /// <param name="newMultiplier"></param>
        public void SetMultiplier(float newMultiplier)
        {
            _multiplier = newMultiplier;
        }

        public void SetDefaultMultiplier()
        {
            _multiplier = _defaultMultiplier;
        }

        public float GetDefaultMultiplier()
        {
            return _defaultMultiplier;
        }

        public float GetMultiplicator()
        {
            return _multiplier;
        }

        /// <summary>
        /// This just corrects the relative position to camera and adds the offset
        /// </summary>
        /// <param name="stylusFrame"></param>
        /// <returns></returns>
        public StylusData GetUnFilteredData(StylusData stylusFrame)
        {
            StylusData newStylusData = new StylusData();

            newStylusData.Position = stylusFrame.Position + _calibrationPreferences.PositionOffset;
            newStylusData.Rotation = stylusFrame.Rotation + _calibrationPreferences.RotationOffset;

            Vector3 newStylusPosition = _cameraTransform.TransformPoint(newStylusData.Position);

            newStylusData.Position = newStylusPosition;
            newStylusData.Buttons[0] = stylusFrame.Buttons[0];
            newStylusData.Buttons[1] = stylusFrame.Buttons[1];

            RawPosition = newStylusData.Position;
            RawRotation = newStylusData.Rotation;

            return newStylusData;
        }

        /// <summary>
        /// This lerps on every axis, sepereatly, depending on their distance
        /// this should be smoother and faster then the one we had, and we should be able to make it faster by increasing the 2 factors
        /// </summary>
        /// <param name="stylusFrame"></param>
        /// <returns></returns>
        internal StylusData GetFilteredDataV2(StylusData stylusFrame)
        {
            LastPosition = new Vector3(Position.x, Position.y, Position.z); // set the last position here;
            LastRotation = new Vector3(Rotation.x, Rotation.y, Rotation.z); // set the last position here;
            StylusData newStylusData = new StylusData();
            newStylusData.Position = stylusFrame.Position + _calibrationPreferences.PositionOffset;
            newStylusData.Rotation = stylusFrame.Rotation + _calibrationPreferences.RotationOffset;

            ////// CAMERA FILTER /////////////////////////
            _oldTransform.position = _cameraTransform.position;
            _oldTransform.rotation = _cameraTransform.rotation;
           
            Vector3 newStylusPosition;

            _oldTransformsQueue.Enqueue(new TransformQueueElement() { Position = _oldTransform.position, Rotation = _oldTransform.rotation });

            if (_queueCounter < TRANSFORM_QUEUE_LENGTH)
            {
                _queueCounter++;
                newStylusPosition = _cameraTransform.TransformPoint(newStylusData.Position);
            }
            else
            {
                var old = _oldTransformsQueue.Dequeue();
                _oldTransform.position = old.Position;
                _oldTransform.rotation = old.Rotation;
                newStylusPosition = _oldTransform.TransformPoint(newStylusData.Position);
            }

            RawPosition = newStylusPosition;
            RawRotation = newStylusData.Rotation;

            /////////////////////////////////////////  CALCULATION OF SMOOTH POSITION ///////////////////////////////////////////////////////////

            float xDisatanceFactor = Math.Abs(LastPosition.x - RawPosition.x) * _multiplier + _additor;
            float yDisatanceFactor = Math.Abs(LastPosition.y - RawPosition.y) * _multiplier + _additor;
            float zDisatanceFactor = Math.Abs(LastPosition.z - RawPosition.z) * _multiplier + _additor;

            float newX = Mathf.Lerp(LastPosition.x, RawPosition.x, xDisatanceFactor);
            float newY = Mathf.Lerp(LastPosition.y, RawPosition.y, yDisatanceFactor);
            float newZ = Mathf.Lerp(LastPosition.z, RawPosition.z, zDisatanceFactor);

            Vector3 smoothedPosition = new Vector3(newX, newY, newZ);

            newStylusData.Position = smoothedPosition;
            newStylusData.Buttons[0] = stylusFrame.Buttons[0];
            newStylusData.Buttons[1] = stylusFrame.Buttons[1];

            Position = newStylusData.Position;

            /////////////////////////////////////////  CALCULATION OF SMOOTH ROTATION ///////////////////////////////////////////////////////////

            float xRotDistanceFactor = Math.Abs(LastRotation.x - RawRotation.x) / 300 + _additor;
            float yRotDistanceFactor = Math.Abs(LastRotation.y - RawRotation.y) / 300 + _additor;
            float zRotDistanceFactor = Math.Abs(LastRotation.z - RawRotation.z) / 300 + _additor; // maybe one of these values have to be multiplied with /2?pitch?

            float newRotX = Mathf.Lerp(LastRotation.x, RawRotation.x, xRotDistanceFactor);
            float newRotY = Mathf.Lerp(LastRotation.y, RawRotation.y, yRotDistanceFactor);
            float newRotZ = Mathf.Lerp(LastRotation.z, RawRotation.z, zRotDistanceFactor);

            Vector3 smoothedRotation = new Vector3(newRotX, newRotY, newRotZ);

            newStylusData.Rotation = smoothedRotation;

            Rotation = newStylusData.Rotation;

            return newStylusData;
        }

        #region EMULATOR
        /// <summary>
        /// This lerps on every axis, sepereatly, depending on their distance, need to test
        /// </summary>
        /// <param name="stylusFrame"></param>
        /// <returns></returns>
        public StylusData GetDataForEmulatorV2(StylusData stylusFrame)
        {
            LastPosition = new Vector3(Position.x, Position.y, Position.z); // set the last position here;
            LastRotation = new Vector3(Rotation.x, Rotation.y, Rotation.z); // set the last position here;
            StylusData newStylusData = new StylusData();
            newStylusData.Position = stylusFrame.Position + _calibrationPreferences.PositionOffset;
            newStylusData.Rotation = stylusFrame.Rotation + _calibrationPreferences.RotationOffset;

            RawPosition = newStylusData.Position;
            RawRotation = newStylusData.Rotation;

            Vector3 newStylusPosition = newStylusData.Position;

            /////////////////////////////////////////  CALCULATION OF SMOOTH POSITION ///////////////////////////////////////////////////////////

            float xDisatanceFactor = Math.Abs(LastPosition.x - newStylusPosition.x) * _multiplier + _additor;
            float yDisatanceFactor = Math.Abs(LastPosition.y - newStylusPosition.y) * _multiplier + _additor;
            float zDisatanceFactor = Math.Abs(LastPosition.z - newStylusPosition.z) * _multiplier + _additor;

            float newX = Mathf.Lerp(LastPosition.x, newStylusPosition.x, xDisatanceFactor);
            float newY = Mathf.Lerp(LastPosition.y, newStylusPosition.y, yDisatanceFactor);
            float newZ = Mathf.Lerp(LastPosition.z, newStylusPosition.z, zDisatanceFactor);

            Vector3 smoothedPosition = new Vector3(newX, newY, newZ);

            newStylusData.Position = smoothedPosition;
            newStylusData.Buttons[0] = stylusFrame.Buttons[0];
            newStylusData.Buttons[1] = stylusFrame.Buttons[1];

            Position = newStylusData.Position;


            /////////////////////////////////////////  CALCULATION OF SMOOTH ROTATION ///////////////////////////////////////////////////////////

            float xRotDistanceFactor = Math.Abs(LastRotation.x - RawRotation.x) * _multiplier + _additor;
            float yRotDistanceFactor = Math.Abs(LastRotation.y - RawRotation.y) * _multiplier + _additor;
            float zRotDistanceFactor = Math.Abs(LastRotation.z - RawRotation.z) * _multiplier + _additor;

            float newRotX = Mathf.Lerp(LastRotation.x, RawRotation.x, xRotDistanceFactor);
            float newRotY = Mathf.Lerp(LastRotation.y, RawRotation.y, yRotDistanceFactor);
            float newRotZ = Mathf.Lerp(LastRotation.z, RawRotation.z, zRotDistanceFactor);

            Vector3 smoothedRotation = new Vector3(newRotX, newRotY, newRotZ);

            newStylusData.Rotation = smoothedRotation;

            Rotation = newStylusData.Rotation;

            return newStylusData;
        }

        #endregion
    }
}
