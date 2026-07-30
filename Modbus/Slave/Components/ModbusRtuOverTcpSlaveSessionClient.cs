namespace Modbus
{
    public abstract class ModbusRtuOverTcpSlaveSessionClient : TcpSessionClientBase
    {
        protected override Task OnTcpConnecting(ConnectingEventArgs e)
        {
            this.SetAdapter((SingleStreamDataHandlingAdapter)new ModbusRtuAdapterForSlave());
            return base.OnTcpConnecting(e);
        }

        internal Task InternalSendAsync(
          ModbusRtuResponseForSlave modbusRtuResponseForSlave,
          CancellationToken cancellationToken)
        {
            return this.ProtectedSendAsync<ModbusRtuResponseForSlave>(modbusRtuResponseForSlave, cancellationToken);
        }
    }
}
