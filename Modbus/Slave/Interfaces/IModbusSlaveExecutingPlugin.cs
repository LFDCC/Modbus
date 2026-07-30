namespace Modbus
{
    [DynamicMethod]
    public interface IModbusSlaveExecutingPlugin : IPlugin, IDisposableObject, IDisposable
    {
        Task OnModbusSlaveExecuting(IModbusSlavePoint sender, ModbusSlaveExecutingEventArgs e);
    }
}
