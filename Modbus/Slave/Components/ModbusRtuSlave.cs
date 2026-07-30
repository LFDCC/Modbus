namespace Modbus
{
    public class ModbusRtuSlave :
      SerialPortClientBase,
      IModbusRtuSlave,
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
        public

        Task ConnectAsync(CancellationToken cancellationToken)
        {
            return this.SerialPortConnectAsync(cancellationToken);
        }

        protected override async Task OnSerialConnecting(ConnectingEventArgs e)
        {
            ModbusRtuSlave modbusRtuSlave = this;
            modbusRtuSlave.SetAdapter((SingleStreamDataHandlingAdapter)new ModbusRtuAdapterForSlave());
            // ISSUE: reference to a compiler-generated method
            await base.OnSerialConnecting(e).ConfigureDefaultAwait();
        }

        internal Task InternalSendAsync(ModbusRtuResponseForSlave modbusRtuResponse)
        {
            return this.ProtectedSendAsync<ModbusRtuResponseForSlave>(modbusRtuResponse, CancellationToken.None);
        }
    }
}
