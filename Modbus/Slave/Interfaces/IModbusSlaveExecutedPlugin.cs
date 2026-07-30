namespace Modbus
{
    [DynamicMethod]
    public interface IModbusSlaveExecutedPlugin : IPlugin, IDisposableObject, IDisposable
    {
        Task OnModbusSlaveExecuted(IModbusSlavePoint sender, ModbusSlaveExecutedEventArgs e);
    }
}
