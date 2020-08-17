using HoloLight.STK.Core;
using HoloLight.STK.Features.Drawing;
using System.Collections.Generic;
using UnityEngine;

namespace HoloLight.STK.Examples.Drawing
{
    public class ColorManager : MonoBehaviour
    {
        [SerializeField]
        private List<Color> _colors;

        [SerializeField]
        DrawingManager _drawingManager;

        private BoxCollider[] _colorButtons;

        private GameObject _lastSelectedGameObject;

        void Start()
        {
            _colorButtons = transform.Find("Colors").transform.GetComponentsInChildren<BoxCollider>();

            for (int i = 0; i < _colorButtons.Length; i++)
            {
                _colorButtons[i].GetComponent<MeshRenderer>().material.color = _colors[i];
            }

            SetColor(0);
        }

        public void SetColor(int newColorIndex)
        {
            _drawingManager.LineColor = _colors[newColorIndex];

            if (_lastSelectedGameObject != null)
            {
                _lastSelectedGameObject.SetActive(false);
            }

            _lastSelectedGameObject = _colorButtons[newColorIndex].transform.GetChild(1).gameObject;
            _lastSelectedGameObject.SetActive(true);
        }

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (gameObject.activeSelf)
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }
    }
}