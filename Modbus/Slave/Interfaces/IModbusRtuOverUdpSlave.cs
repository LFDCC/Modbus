namespace Modbus
{
    public interface IModbusRtuOverUdpSlave :
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
