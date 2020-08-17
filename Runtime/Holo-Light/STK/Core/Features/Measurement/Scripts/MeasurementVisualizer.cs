using UnityEngine;

namespace HoloLight.STK.Features.Measurement
{
    [CreateAssetMenu(fileName = "MeasurementVisualizer", menuName = "Measurement Visualizer")]
    public class MeasurementVisualizer : ScriptableObject
    {
        public GameObject EndPoint;
        public GameObject HelperPoint;
        public TextMesh TextWindow;

        public Material LineMaterial;
        public bool VisualizeHelper;
        public float LineThickness;
    }
}