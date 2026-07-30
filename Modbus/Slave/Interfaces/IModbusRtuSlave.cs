namespace Modbus
{
    public interface IModbusRtuSlave :
      IDependencyClient,
      IDependencyObject,
      IDisposableObject,
      IDisposable,
      IClient,
      ILoggerObject,
      ISetupConfigObject,
      IResolverConfigObject,
      IConfigObject,
      IPluginObject,
      IResolverObject,
      IOnlineClient,
      IConnectableClient,
      IClosableClient,
      IModbusSlave
    {
    }
}
