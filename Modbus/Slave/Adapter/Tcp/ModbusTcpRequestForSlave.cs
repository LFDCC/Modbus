namespace Modbus
{
    internal sealed class ModbusTcpRequestForSlave : ModbusTcpBase, IRequestInfo
    {
        internal ModbusTcpRequestForSlave(
          ushort transactionId,
          ushort protocolId,
          byte slaveId,
          FunctionCode functionCode)
        {
            this.TransactionId = transactionId;
            this.ProtocolId = protocolId;
            this.SlaveId = slaveId;
            this.FunctionCode = functionCode;
        }
    }
}
