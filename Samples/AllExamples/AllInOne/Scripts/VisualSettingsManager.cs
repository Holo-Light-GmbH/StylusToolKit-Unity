using HoloLight.STK.Core;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLight.STK.Examples.AllExamples
{
    public class VisualSettingsManager : MonoBehaviour
    {
        [SerializeField]
        private HoloStylusManager _manager;

        [SerializeField]
        private List<Color> _colors;

        private void Awake()
        {
            BoxCollider[] colorButtons = transform.Find("Settings/Colors").transform.GetComponentsInChildren<BoxCollider>();

            for (int i = 0; i < colorButtons.Length; i++)
            {
                colorButtons[i].GetComponent<MeshRenderer>().material.color = _colors[i];
            }
        }

        public void ToggleVisibility()
        {
            _manager.VisualSettings.ToggleVisibility();
        }

        public void OnNewTipSizeSliderValue(SliderEventData newSliderData)
        {
            _manager.VisualSettings.SetScale(newSliderData.NewValue+0.5f);
        }

        public void SetColor(int newColorIndex)
        {
            _manager.VisualSettings.SetColor(_colors[newColorIndex]);
        }

        public void SetMesh(Mesh newMesh)
        {
            _manager.VisualSettings.SetMesh(newMesh);
        }

    }
}