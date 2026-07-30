namespace Modbus
{
    public interface IModbusRtuOverTcpSlave :
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
      IModbusSlave
    {
    }
}
