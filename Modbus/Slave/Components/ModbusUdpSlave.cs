namespace Modbus
{
    public class ModbusUdpSlave :
      UdpSessionBase,
      IModbusUdpSlave,
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
        protected override void LoadConfig(TouchSocketConfig config)
        {
            this.SetAdapter((UdpDataHandlingAdapter)new ModbusUdpAdapterForSlave());
            base.LoadConfig(config);
        }

        internal Task InternalSendAsync(
          EndPoint endPoint,
          ModbusTcpResponseForSlave modbusTcpResponseForSlave)
        {
            return this.ProtectedSendAsync(endPoint, (IRequestInfo)modbusTcpResponseForSlave, CancellationToken.None);
        }
    }
}
