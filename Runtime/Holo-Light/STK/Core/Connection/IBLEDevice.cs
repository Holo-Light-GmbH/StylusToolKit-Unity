
namespace HoloLight.STK.Core
{
    public interface IBLEDevice
    {
        string Name { get; }
        string ID { get; }
        bool IsPaired { get; }
        bool IsConnectable();
    }
}