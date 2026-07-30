
namespace Modbus
{
    public interface IModbusSlavePoint
    {
        IModbusDataLocater DataLocater { get; }

        byte SlaveId { get; }

        bool IgnoreSlaveId { get; }
    }
}
