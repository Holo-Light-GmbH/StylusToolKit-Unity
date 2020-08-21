using TMPro;
using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    [CreateAssetMenu(fileName = "MeasurementVisualizer", menuName = "Holo-Light/Measurement Visualizer", order = 2)]
    public class MeasurementVisualizer : ScriptableObject
    {
        public GameObject Container;
        public GameObject EndPoint;
        public GameObject HelperPoint;
        public TextMeshPro TextWindow;

        public Material LineMaterial;
        public bool VisualizeHelper;
        public float LineThickness;
    }
}