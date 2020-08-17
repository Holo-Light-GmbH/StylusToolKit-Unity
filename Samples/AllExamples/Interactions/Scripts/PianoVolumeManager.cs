using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace HoloLight.STK.Examples.Interactions
{
    /// <summary>
    /// Changes the Volume of the Piano Keys, regarding the Slider Changes
    /// </summary>
    public class PianoVolumeManager : MonoBehaviour
    {
        [SerializeField]
        private Transform _pianoKeyParent;

        AudioSource[] pianoAudioSources;
        private void Awake()
        {
            pianoAudioSources = _pianoKeyParent.transform.GetComponentsInChildren<AudioSource>();
        }
        public void OnVolumeChange(SliderEventData newSliderValue)
        {
            float newValue = newSliderValue.NewValue;
            for (int i = 0; i < pianoAudioSources.Length; i++)
            {
                pianoAudioSources[i].volume = newValue;
            }
        }
    }
}