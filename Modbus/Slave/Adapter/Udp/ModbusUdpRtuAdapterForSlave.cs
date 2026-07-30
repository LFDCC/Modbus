namespace Modbus
{
    internal class ModbusUdpRtuAdapterForSlave :
      ModbusUdpCustomDataHandlingAdapter<ModbusRtuRequestForSlave>
    {
        protected override FilterResult Filter<TReader>(
          ref TReader reader,
          ref ModbusRtuRequestForSlave request)
        {
            return ModbusRtuRequestForSlaveParser.Filter<TReader>(ref reader, ref request);
        }
    }
}
