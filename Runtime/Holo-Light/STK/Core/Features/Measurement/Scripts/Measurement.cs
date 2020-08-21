using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    public class Measurement
    {
        private Vector3? _startPoint;
        private Vector3? _endPoint;

        private GameObject _startPointVisual; //TODO Encapsulate the visualization together with their Vectors
        private GameObject _endPointVisual; //TODO Container the Gameobjects together
        private GameObject _line;
        private GameObject _containerObject;

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
                Visualize();

            }
        }

        public GameObject GetGameObject()
        {
            return _containerObject;
        }

        public void AddPoint(Vector3 point)
        {
            if (_startPoint == null)
            {
                _startPoint = point;
                Visualize();

            }
            else
            {
                _endPoint = point;
                Visualize();
            }
        }

        public void RemovePoint(Vector3 point)
        {
            if (_endPoint != null)
            {
                _endPoint = null;
                GameObject.Destroy(_line);
                _line = null;
                GameObject.Destroy(_endPointVisual);
                _endPointVisual = null;
            }
            else
            {
                _startPoint = null;
                GameObject.Destroy(_startPointVisual);
                _startPointVisual = null;
            }
        }

        private void Visualize()
        {
            if (_visualizer == null)
            {
                Debug.LogWarning("No Visualizer Attached to the Measurement so you won't see anything");
                return;
            }

            if (_startPoint != null)
            {
                if (_startPointVisual == null)
                {
                    _startPointVisual = GameObject.Instantiate<GameObject>(_visualizer.EndPoint);
                    _startPointVisual.transform.position = _startPoint.Value;
                }
                if (_endPoint != null)
                {
                    if (_endPointVisual == null)
                    {
                        GameObject.Destroy(_startPointVisual);
                        _containerObject = GameObject.Instantiate(_visualizer.Container);
                        _containerObject.GetComponent<MeasurementObject>().Init(_startPoint.Value, _endPoint.Value, _visualizer);
                    }
                }
            }
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(_startPointVisual);
            UnityEngine.Object.Destroy(_endPointVisual);
            UnityEngine.Object.Destroy(_line);
            UnityEngine.Object.Destroy(_containerObject);
        }
    }
}