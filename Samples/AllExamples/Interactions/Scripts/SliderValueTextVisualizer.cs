using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;

namespace HoloLight.STK.Examples.Interactions
{
    public class SliderValueTextVisualizer : MonoBehaviour
    {
        [SerializeField]
        private TextMesh textMesh = null;
        [SerializeField]
        private TextMeshPro textMeshPro = null;

        [SerializeField]
        private string _numberUnit = " %";

        [SerializeField]
        private float _multiplicator = 100;

        [SerializeField]
        private float _offsetStart = 0;

        [SerializeField]
        private int _deciamls = 0;
        private void Start()
        {
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMesh>();
            }

            if (textMeshPro == null)
            {
                textMeshPro = GetComponent<TextMeshPro>();
            }
        }
        public void OnSliderUpdated(SliderEventData eventData)
        {
            string visualziedString = ((eventData.NewValue + _offsetStart) * _multiplicator).ToString("F" + _deciamls) + "" + _numberUnit;
            if (textMesh != null)
            {
                textMesh.text = visualziedString;
            }

            if (textMeshPro != null)
            {
                textMeshPro.text = visualziedString;
            }
        }
    }
}