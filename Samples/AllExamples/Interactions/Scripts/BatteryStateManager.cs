using HoloLight.STK.Core;
using TMPro;
using UnityEngine;

namespace HoloLight.STK.Examples.Interactions
{
    /// <summary>
    /// Recieves Battery Update Changes and changes the text and battery bar 
    /// </summary>
    public class BatteryStateManager : MonoBehaviour
    {

        [SerializeField]
        private HoloStylusManager _manager;

        private float _currentBatteryLevel = 100;

        [SerializeField]
        private TextMeshPro _batteryLevelText;

        [SerializeField]
        private GameObject _batteryLevelGO;

        private void OnEnable()
        {
            _manager.EventManager.RegisterCallback(StylusEventType.OnStylusBatteryValue, OnNewBatteryValue);
        }
        private void OnDisable()
        {
            _manager.EventManager.UnRegisterCallback(StylusEventType.OnStylusBatteryValue, OnNewBatteryValue);
        }

        public void UpdateBar()
        {
            _batteryLevelText.text = (int)_currentBatteryLevel + " %";
            _batteryLevelGO.transform.localPosition = new Vector3(-0.47f + (_currentBatteryLevel / 108.4f) / 2, _batteryLevelGO.transform.localPosition.y, _batteryLevelGO.transform.localPosition.z);
            _batteryLevelGO.transform.localScale = new Vector3(_currentBatteryLevel / 108.4f, _batteryLevelGO.transform.localScale.y, _batteryLevelGO.transform.localScale.z);
        }

        public void OnNewBatteryValue(StylusData stylusData)
        {
            _currentBatteryLevel = stylusData.BatteryPercentage;
            UpdateBar();
        }
    }
}