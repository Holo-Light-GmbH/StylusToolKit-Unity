using HoloLight.STK.Features.Drawing;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Examples.Drawing
{
    /// <summary>
    /// Handles the Drawing Brush Size slider Setting
    /// </summary>
    public class SliderValuesManager : MonoBehaviour
    {
        [SerializeField]
        private TextMeshPro _brushSizeText;

        [SerializeField]
        private DrawingManager _drawingManager;
        public float BrushSizeValue { get; private set; }

        public void UpdateBrushSizeSliderValue(SliderEventData sliderEventData)
        {
            BrushSizeValue = sliderEventData.NewValue + 0.3f;
            _brushSizeText.text = (sliderEventData.NewValue*100+50).ToString("N0") + " %";

            _drawingManager.SetLineWidthMultiplier(BrushSizeValue);
        }

    }
}