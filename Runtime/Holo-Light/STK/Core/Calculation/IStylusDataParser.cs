using HoloLight.STK.Core.Tracker;

namespace HoloLight.STK.Core
{
    public interface IStylusDataParser
    {
        bool Initialize(NeuralNetworkData data);
        bool IsInitialized { get; }
        bool IsVisible { get; }
        StylusData ParseFrame(byte[] data);
    }
}