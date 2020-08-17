using System.Collections.Generic;
using HoloLight.STK.Core;
using HoloLight.STK.MRTK;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    /// <summary>
    /// With the MeasurementManager you can create Measurements, the created ones and Remove them.
    /// </summary>
    public class MeasurementManager : MonoBehaviour
    {
        private MeasurementPoint _startPoint;
        private MeasurementPoint _endPoint;

        private int _clickCounter = 0;
        public int ClicksPerPoint = 1;
        public float DistanceThreshold = 1;
        public MeasurementVisualizer Visualizer;
        private Stack<Measurement> _measurementStack = new Stack<Measurement>();
        private Measurement _currentMeasurement;

        [SerializeField]
        private InputActionHandler _inputACTIONHandler;

        [SerializeField]
        private HoloStylusManager _holoStylusManager;

        public void Activate()
        {
            _inputACTIONHandler.enabled = true;
            _holoStylusManager.PointerSwitcher.DisablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
        }

        public void Deactivate()
        {
            CleanUp();
            _inputACTIONHandler.enabled = false;
            _holoStylusManager.PointerSwitcher.EnablePointer(StylusPointerSwitcher.PointerType.StylusRayPointer);
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

        public bool CheckMeasurement(Vector3 pointToCheck)
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