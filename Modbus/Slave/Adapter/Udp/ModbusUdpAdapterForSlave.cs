namespace Modbus
{
    internal class ModbusUdpAdapterForSlave :
      ModbusUdpCustomDataHandlingAdapter<ModbusTcpRequestForSlave>
    {
        protected override FilterResult Filter<TReader>(
          ref TReader reader,
          ref ModbusTcpRequestForSlave request)
        {
            return ModbusTcpRequestForSlaveParser.Filter<TReader>(ref reader, ref request);
        }
    }
}
