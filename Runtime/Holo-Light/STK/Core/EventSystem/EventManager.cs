using System;

namespace HoloLight.STK.Core
{
    public enum StylusEventType
    {
        OnActionButtonDown = 0,
        OnActionButtonUp,
        OnBackButtonDown,
        OnBackButtonUp,
        OnPositionChanged,
        OnStylusConnected,
        OnStylusDisconnected,
        OnStylusBatteryValue,
        OnStylusPreferedHandChanged,
    };

    public delegate void EventCallback(StylusData data);

    public class EventManager
    {
        public EventCallback[] Callbacks;
        private StylusData _lastFrame;

        private bool _firstFrameRecieved = false;

        public EventManager()
        {
            Callbacks = new EventCallback[Enum.GetNames(typeof(StylusEventType)).Length];
        }

        private void FireEvent(StylusEventType eventType, StylusData data)
        {
            Callbacks[(int)eventType]?.Invoke(data);
        }

        public void PushData(StylusData data)
        {
            if (_lastFrame == null)
            {
                _lastFrame = data;
                return;
            }

            if (_lastFrame.Position != data.Position)
            {
                FireEvent(StylusEventType.OnPositionChanged, data);
            }

            if (_lastFrame.Buttons[0] != data.Buttons[0])
            {
                if (data.Buttons[0])
                {
                    FireEvent(StylusEventType.OnActionButtonDown, data);
                }
                else
                {
                    FireEvent(StylusEventType.OnActionButtonUp, data);
                }
            }

            if (_lastFrame.Buttons[1] != data.Buttons[1])
            {
                if (data.Buttons[1])
                {
                    FireEvent(StylusEventType.OnBackButtonDown, data);
                }
                else
                {
                    FireEvent(StylusEventType.OnBackButtonUp, data);
                }
            }

            if (!_firstFrameRecieved)
            {
                FireEvent(StylusEventType.OnStylusConnected, data);
                _firstFrameRecieved = true;
            }

            _lastFrame = data;
        }

        public void PushNewBatteryData(int percentageValue)
        {
            if (_lastFrame == null)
            {
                _lastFrame = new StylusData();
            }
            _lastFrame.BatteryPercentage = percentageValue;
            FireEvent(StylusEventType.OnStylusBatteryValue, _lastFrame);
        }

        public void RegisterCallback(StylusEventType eventType, EventCallback callback)
        {
            Callbacks[(int)eventType] += callback;
        }

        public void UnRegisterCallback(StylusEventType eventType, EventCallback callback)
        {
            Callbacks[(int)eventType] -= callback;
        }

        internal void TriggerDisconnected()
        {
            FireEvent(StylusEventType.OnStylusDisconnected, _lastFrame);
            _firstFrameRecieved = false;
        }

        internal void TriggerNewPreferedHand()
        {
            FireEvent(StylusEventType.OnStylusPreferedHandChanged, _lastFrame);
        }
    }
}