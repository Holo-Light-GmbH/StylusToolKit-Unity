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
        private TextMesh _distanceText;

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
            return _line;
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
                GameObject.Destroy(_distanceText);
                _distanceText = null;
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
                        _endPointVisual = GameObject.Instantiate<GameObject>(_visualizer.EndPoint);
                        _endPointVisual.transform.position = _endPoint.Value;
                        _line = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        GameObject.Destroy(_line.GetComponent<Collider>());
                        var length = (_endPoint.Value - _startPoint.Value);
                        _line.GetComponent<Renderer>().material = _visualizer.LineMaterial;
                        _line.transform.position = _startPoint.Value + length / 2;
                        _line.transform.forward = _endPoint.Value - _startPoint.Value;
                        _line.transform.localScale = new Vector3(_visualizer.LineThickness, _visualizer.LineThickness,
                            length.magnitude);
                        _distanceText = GameObject.Instantiate<TextMesh>(_visualizer.TextWindow);
                        _distanceText.transform.position = new Vector3(0, 0.01f, 0) + _startPoint.Value + length / 2;
                        _distanceText.transform.right = _line.transform.forward * (Vector3.Dot(_line.transform.forward, Camera.main.transform.right));


                        float lengthInM = length.magnitude; // in m
                        string finalValueString = "";
                        if (lengthInM >= 1)
                        {
                            finalValueString = (lengthInM).ToString("0.00") + " m";
                        }
                        else if (lengthInM < 1 && lengthInM > 0.02f)
                        {
                            finalValueString = (lengthInM * 100).ToString("0.0") + " cm";
                        }
                        else if (lengthInM <= 0.02f)
                        {
                            finalValueString = (lengthInM * 1000).ToString("0") + " mm";
                        }

                        _distanceText.text = finalValueString;
                    }
                }
            }
        }

        public void Destroy()
        {
            UnityEngine.Object.Destroy(_startPointVisual);
            UnityEngine.Object.Destroy(_endPointVisual);
            UnityEngine.Object.Destroy(_line);
            if (_distanceText != null)
            {
                UnityEngine.Object.Destroy(_distanceText.gameObject);
            }
        }
    }
}