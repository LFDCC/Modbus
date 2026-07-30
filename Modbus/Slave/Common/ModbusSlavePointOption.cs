
namespace Modbus
{
    public class ModbusSlavePointOption
    {
        public bool IgnoreSlaveId { get; set; }

        public IModbusDataLocater DataLocater { get; set; }

        public byte SlaveId { get; set; } = 1;
    }
}
