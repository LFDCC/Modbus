namespace Modbus
{
    public interface IModbusUdpSlave :
      IUdpSessionBase,
      IServiceBase,
      ISetupConfigObject,
      IResolverConfigObject,
      IConfigObject,
      IDependencyObject,
      IDisposableObject,
      IDisposable,
      ILoggerObject,
      IPluginObject,
      IResolverObject,
      IDependencyClient,
      IClient,
      IModbusSlave
    {
    }
}
