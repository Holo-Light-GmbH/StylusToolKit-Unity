
using Microsoft.MixedReality.Toolkit.UI;
using System;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    public class MeasurementObject : MonoBehaviour
    {
        private GameObject _startPointVisual;
        private GameObject _endPointVisual;
        private GameObject _line;
        private TextMeshPro _distanceText;
        private MeasurementVisualizer _visualizer;

        private float _fontSizeFactor = 0.002f;
        private int _isManipulating = 0;

        public void OnManipulationStarted(ManipulationEventData data)
        {
            _isManipulating++;
        }

        public void OnManipulationEnded(ManipulationEventData data)
        {
            _isManipulating--;
        }

        private void Update()
        {
            if (_isManipulating > 0)
            {
                ReRender();
            }
        }
         
        private void ReRender()
        {
            Vector3 startPoint = _startPointVisual.transform.position;
            Vector3 endPoint = _endPointVisual.transform.position;

            var length = (endPoint - startPoint);
            _line.transform.position = startPoint + length / 2;
            _line.transform.forward = endPoint - startPoint;
            _line.transform.localScale = new Vector3(_visualizer.LineThickness, _visualizer.LineThickness,
                length.magnitude);

            _distanceText.transform.position = new Vector3(0, 0.02f, 0) + (startPoint + length/2);

            float lengthInM = length.magnitude; // in m

            _distanceText.text = GetDistanceText(lengthInM);
        }

        internal void Init(Vector3 startPoint, Vector3 endPoint, MeasurementVisualizer visualizer)
        {
            _startPointVisual = transform.Find("StartPoint").gameObject;
            _endPointVisual = transform.Find("EndPoint").gameObject;
            _line = transform.Find("Line").gameObject;
            _distanceText = transform.Find("DistanceText").GetComponent<TextMeshPro>(); 
            _visualizer = visualizer;

            _line.GetComponent<MeshRenderer>().material = _visualizer.LineMaterial;

            _startPointVisual.transform.position = startPoint;
            _endPointVisual.transform.position = endPoint;

            ReRender();
        }

        private string GetDistanceText(float lengthInM)
        {
            string finalValueString = "";
            if (lengthInM >= 1)
            {
                _distanceText.fontSize = 60 * _fontSizeFactor;
                finalValueString = (lengthInM).ToString("0.00") + " m";
            }
            else if (lengthInM < 1 && lengthInM > 0.03f)
            {
                _distanceText.fontSize = 45 * _fontSizeFactor + 15 * lengthInM * _fontSizeFactor;
                finalValueString = (lengthInM * 100).ToString("0.0") + " cm";
            }
            else if (lengthInM <= 0.03f)
            {
                _distanceText.fontSize = 38 * _fontSizeFactor + _fontSizeFactor * 7 * lengthInM / 0.03f;
                finalValueString = (lengthInM * 1000).ToString("0") + " mm";
            }
            return finalValueString;
        }
    }
}