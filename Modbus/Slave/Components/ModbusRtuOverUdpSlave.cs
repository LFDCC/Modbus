namespace Modbus
{
    public class ModbusRtuOverUdpSlave :
      UdpSessionBase,
      IModbusRtuOverUdpSlave,
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
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SetAdapter((UdpDataHandlingAdapter)new ModbusUdpRtuAdapterForSlave());
            base.LoadConfig(config);
        }

        internal Task InternalSendAsync(
          EndPoint endPoint,
          ModbusRtuResponseForSlave modbusRtuResponseForSlave)
        {
            return this.ProtectedSendAsync(endPoint, (IRequestInfo)modbusRtuResponseForSlave, CancellationToken.None);
        }
    }
}
