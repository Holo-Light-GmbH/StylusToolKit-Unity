using System.Collections.Generic;
using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    public class MeasurementPoint
    {
        private List<Vector3> _rawPoints = new List<Vector3>();
        public int NumberOfPoints => _rawPoints.Count;
        public Vector3 AveragedPoint => AverageOverPoints();


        private List<GameObject> _pointsList = new List<GameObject>();
        private MeasurementVisualizer _visualizer;
        public MeasurementVisualizer Visualizer
        {
            private get
            {
                return _visualizer;
            }
            set
            {
                _visualizer = value;
                if (_visualizer.VisualizeHelper)
                {
                    foreach (var point in _rawPoints)
                    {
                        var visualizedPoint = GameObject.Instantiate<GameObject>(_visualizer.HelperPoint);
                        visualizedPoint.transform.position = point;
                        _pointsList.Add(visualizedPoint);
                    }
                }
            }
        }

        public Vector3 AverageOverPoints()
        {
            var tmp = new Vector3(0, 0, 0);
            foreach (var element in _rawPoints)
            {
                tmp += element;
            }

            return tmp / _rawPoints.Count;
        }

        public void AddPoint(Vector3 newRawPoint)
        {
            _rawPoints.Add(newRawPoint);
            if (_visualizer != null)
            {
                var visualizedPoint = GameObject.Instantiate<GameObject>(_visualizer.HelperPoint);
                visualizedPoint.transform.position = newRawPoint;
                _pointsList.Add(visualizedPoint);

            }
        }

        public void Destroy()
        {
            foreach (var visualizedPoint in _pointsList)
            {
                Object.Destroy(visualizedPoint);
            }
        }
    }
}