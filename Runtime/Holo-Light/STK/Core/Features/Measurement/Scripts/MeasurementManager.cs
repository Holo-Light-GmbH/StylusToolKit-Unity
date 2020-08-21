using System.Collections.Generic;
using HoloLight.STK.Core;
using HoloLight.STK.MRTK;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    /// <summary>
    /// With the MeasurementManager you can create Measurements, Get the created Object or/and Remove them.
    /// </summary>
    public class MeasurementManager : MonoBehaviour
    {
        private MeasurementPoint _startPoint;
        private MeasurementPoint _endPoint;

        [Tooltip("If set to true, it is possible to resize and realign the measurements after you have created them.")]
        public bool DynamicMeasurements = true;
        private int _clickCounter = 0;

        [Tooltip("If x > 1,  you need to click x times for every point. It averages the clicked values and uses the average as the StartPoint/EndPoint")]
        public int ClicksPerPoint = 1;
        [Tooltip("If ClicksPerPoint > 1, you can define the Distance Threshhold for the Position Average. If you click to far away from the average, it won't register that click")]
        public float DistanceThreshold = 1;
        [Tooltip("Create your custom Visualizer and assign your custom prefabs.")]
        public MeasurementVisualizer Visualizer;
        private Stack<Measurement> _measurementStack = new Stack<Measurement>();
        private Measurement _currentMeasurement;

        [SerializeField]
        private GameObject _measurementsContainer;

        [SerializeField]
        private InputActionHandler _inputACTIONHandler;

        [SerializeField]
        private HoloStylusManager _holoStylusManager;

        public void Activate()
        {
            if (_measurementsContainer == null)
            {
                _measurementsContainer = new GameObject("MeasurementsContainer");
            }
            _inputACTIONHandler.enabled = true;
            _holoStylusManager.PointerSwitcher.DisablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
            if (DynamicMeasurements)
            {
                SetMeasurementManipulation(false);
            }
        }

        public void Deactivate()
        {
            CleanUp();
            _inputACTIONHandler.enabled = false;
            _holoStylusManager.PointerSwitcher.EnablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
            SetMeasurementManipulation(DynamicMeasurements);
        }

        public void SetMeasurementManipulation(bool enable)
        {
            foreach (Measurement measurement in _measurementStack)
            {
                GameObject containerMeasurement = measurement.GetGameObject();
                var boundingBoxes = containerMeasurement.GetComponentsInChildren<BoundingBox>();
                foreach (BoundingBox box in boundingBoxes)
                {
                    box.enabled = enable;
                }

                var manipulationHandlers = containerMeasurement.GetComponentsInChildren<ManipulationHandler>();
                foreach (ManipulationHandler handler in manipulationHandlers)
                {
                    handler.enabled = enable;
                }

                var boxCollliders = containerMeasurement.GetComponentsInChildren<BoxCollider>();
                foreach (BoxCollider box in boxCollliders)
                {
                    box.enabled = enable; 
                }
            }
        }

        private void CleanUp()
        {
            if (_currentMeasurement != null)
            {
                _clickCounter = 0;
                _startPoint.Destroy();
                if (_endPoint != null)
                {
                    _endPoint.Destroy();
                }
                _currentMeasurement.Destroy();
                _startPoint = null;
                _endPoint = null;
                _currentMeasurement = null;
            }
        }

        public void OnStylusActionClick()
        {
            Vector3 currentPosition = _holoStylusManager.StylusTransform.Position;
            if (CheckMeasurement(currentPosition))
            {
                _clickCounter++;
                GetActivePoint().AddPoint(currentPosition);
                if (_clickCounter == ClicksPerPoint)
                {
                    _currentMeasurement = new Measurement();
                    _currentMeasurement.Visualizer = Visualizer;
                    _currentMeasurement.AddPoint(GetActivePoint().AveragedPoint);
                }

                else if (_clickCounter == 2 * ClicksPerPoint)
                {
                    _currentMeasurement.AddPoint(GetActivePoint().AveragedPoint);
                    _clickCounter = 0;
                    _startPoint.Destroy();
                    _endPoint.Destroy();
                    _startPoint = null;
                    _endPoint = null;

                    GameObject lineContainer = _currentMeasurement.GetGameObject();
                    lineContainer.transform.parent = _measurementsContainer.transform;

                    _measurementStack.Push(_currentMeasurement);
                    _currentMeasurement = null;
                }
            }
        }

        public void Undo()
        {
            if (_measurementStack.Count == 0)
            {
                return;
            }

            Measurement tmp = _measurementStack.Pop();
            tmp.Destroy();
            tmp = null;
        }

        public void DeleteAll()
        {
            while (_measurementStack.Count > 0)
            {
                Undo();
            }
        }
        public void Reset()
        {
            _startPoint?.Destroy();
            _startPoint = null;
            _endPoint?.Destroy();
            _endPoint = null;
            _currentMeasurement?.Destroy();
            _currentMeasurement = null;
            _clickCounter = 0;
        }

        public Stack<Measurement> GetMeasurements()
        {
            return _measurementStack;
        }

        private bool CheckMeasurement(Vector3 pointToCheck)
        {
            if (_clickCounter % ClicksPerPoint == 0)
            {
                return true;
            }
            MeasurementPoint activePoint = GetActivePoint();

            var distance = (activePoint.AveragedPoint - pointToCheck).magnitude;
            return distance < DistanceThreshold;
        }

        private MeasurementPoint GetActivePoint()
        {
            if (_clickCounter <= ClicksPerPoint)
            {
                if (_startPoint == null)
                {
                    _startPoint = new MeasurementPoint();
                    _startPoint.Visualizer = Visualizer;
                }

                return _startPoint;
            }
            else
            {
                if (_endPoint == null)
                {
                    _endPoint = new MeasurementPoint();
                    _endPoint.Visualizer = Visualizer;
                }
                return _endPoint;
            }
        }
    }
}