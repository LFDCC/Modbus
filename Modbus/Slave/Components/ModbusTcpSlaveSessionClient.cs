namespace Modbus
{
    public abstract class ModbusTcpSlaveSessionClient : TcpSessionClientBase
    {
        protected override async

        Task OnTcpConnecting(ConnectingEventArgs e)
        {
            ModbusTcpSlaveSessionClient slaveSessionClient = this;
            slaveSessionClient.SetAdapter((SingleStreamDataHandlingAdapter)new ModbusTcpAdapterForSlave());
            // ISSUE: reference to a compiler-generated method
            await base.OnTcpConnecting(e).ConfigureDefaultAwait();
        }

        internal Task InternalSendAsync(
          ModbusTcpResponseForSlave modbusTcpResponseForSlave)
        {
            return this.ProtectedSendAsync<ModbusTcpResponseForSlave>(modbusTcpResponseForSlave, CancellationToken.None);
        }
    }
}
